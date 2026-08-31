using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.Security;
using SchedulerEngine.Core.Enums;
using SchedulerEngine.Core.Exceptions;
using SchedulerEngine.Core.Seeding;
using SchedulerEngine.Core.Services;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Helpers;

namespace SchedulerEngine.Service.Features.Handlers;

public class CreateAdminUserCommandHandler : IRequestHandler<CreateAdminUserCommand, CreateAdminUserResult>
{
    private readonly IRepository<Party, int>              _partyRepository;
    private readonly IRepository<ApplicationUser, int>    _userRepository;
    private readonly ICurrentUserService                  _currentUserService; // sadece audit log için — yetki kontrolü [Authorize(Policy="SiteAdmin")]'de
    private readonly IPasswordHasher                      _passwordHasher;
    private readonly ILogger<CreateAdminUserCommandHandler> _logger;

    public CreateAdminUserCommandHandler(
        IRepository<Party, int>              partyRepository,
        IRepository<ApplicationUser, int>    userRepository,
        ICurrentUserService                  currentUserService,
        IPasswordHasher                      passwordHasher,
        ILogger<CreateAdminUserCommandHandler> logger)
    {
        _partyRepository     = partyRepository;
        _userRepository      = userRepository;
        _currentUserService  = currentUserService;
        _passwordHasher      = passwordHasher;
        _logger              = logger;
    }

    public async Task<CreateAdminUserResult> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Identifier daha önce alınmış mı?
        var exists = await _userRepository.AnyAsync(
            u => u.DigitalIdentity.Credentials
                .Any(c => c.ContactMedia
                    .Any(cm => cm.Email == request.Identifier
                            || cm.PhoneNumber == request.Identifier)),
            cancellationToken);

        if (exists)
            throw new ConflictException("Bu email veya telefon zaten kayıtlı.");

        // 2. Nesne ağacını kur (RegisterCommandHandler ile aynı desen, sadece rol SiteAdmin)
        var isEmail = request.Identifier.Contains('@');

        var party = new Party
        {
            PartyType  = PartyType.Individual,
            Individual = new Individual
            {
                GivenName     = request.GivenName,
                FamilyName    = request.FamilyName,
                ValidForStart = DateTime.UtcNow
            },
            PartyRoles = new List<PartyRole>
            {
                new PartyRole
                {
                    PartyRoleTypeId = ReferenceDataIds.PartyRoleType.SiteAdmin,
                    ValidForStart   = DateTime.UtcNow,
                    DigitalIdentity = new DigitalIdentity
                    {
                        DigitalIdentityDate = DateTime.UtcNow,
                        Credentials = new List<Credential>
                        {
                            new Credential
                            {
                                CredentialType = CredentialType.Password,
                                ContactMedia   = new List<ContactMedium>
                                {
                                    new ContactMedium
                                    {
                                        Email       = isEmail ? request.Identifier : null,
                                        PhoneNumber = isEmail ? null : request.Identifier,
                                        MediumType  = isEmail
                                            ? ContactMediumType.EmailAddress
                                            : ContactMediumType.PhoneNumber,
                                        IsPreferred = true
                                    }
                                },
                                Characteristics = new List<CredentialCharacteristic>
                                {
                                    new CredentialCharacteristic
                                    {
                                        Name  = "passwordHash",
                                        Value = _passwordHasher.Hash(request.Password)
                                    }
                                }
                            }
                        },
                        ApplicationUser = new ApplicationUser
                        {
                            LanguageId = request.LanguageId,
                        }
                    }
                }
            }
        };

        // 3. Navigation chain'i manuel kur (RegisterCommandHandler'daki gerekçeyle aynı)
        var partyRole = party.PartyRoles!.First();
        var digitalId = partyRole.DigitalIdentity
            ?? throw new InvalidOperationException("DigitalIdentity oluşturulamadı.");
        var appUser   = digitalId.ApplicationUser
            ?? throw new InvalidOperationException("ApplicationUser oluşturulamadı.");

        appUser.DigitalIdentity = digitalId;
        digitalId.PartyRole     = partyRole;
        partyRole.Party         = party;

        digitalId.Credentials.First().ContactMedia.First().Party = party;

        var (userId, userName, userIdentifier) = Helper.ResolveUserInfo(appUser);

        // 4. Kaydet — bu bilinçli olarak token üretmiyor: yeni oluşturulan admin'e
        // otomatik login verilmiyor, kendi credential'larıyla ayrıca login olması gerekiyor.
        await _partyRepository.AddAsync(party, cancellationToken);

        var creatorUserId = _currentUserService.UserId;

        _logger.LogInformation(
            "Yeni Site Admin oluşturuldu. UserId: {UserId}, Identifier: {Identifier}, OluşturanUserId: {CreatorId}",
            appUser.Id, request.Identifier, creatorUserId);

        return new CreateAdminUserResult
        {
            UserId         = userId,
            UserName       = userName,
            UserIdentifier = userIdentifier
        };
    }
}