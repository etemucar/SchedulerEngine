using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.Security;
using SchedulerEngine.Core.Enums;
using SchedulerEngine.Core.Exceptions;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Helpers;
using SchedulerEngine.Core.Seeding;

namespace SchedulerEngine.Service.Features.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IRepository<Party, int>           _partyRepository;
    private readonly IRepository<ApplicationUser, int> _userRepository;
    private readonly IRepository<RefreshToken, int>    _refreshTokenRepository;
    private readonly ITokenService                     _tokenService;
    private readonly IPasswordHasher                   _passwordHasher;
    private readonly ILogger<RegisterCommandHandler>   _logger;

    public RegisterCommandHandler(
        IRepository<Party, int>           partyRepository,
        IRepository<ApplicationUser, int> userRepository,
        IRepository<RefreshToken, int>    refreshTokenRepository,
        ITokenService                     tokenService,
        IPasswordHasher                   passwordHasher,
        ILogger<RegisterCommandHandler>   logger)
    {
        _partyRepository        = partyRepository;
        _userRepository         = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService           = tokenService;
        _passwordHasher         = passwordHasher;
        _logger                 = logger;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
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

        // 2. Nesne ağacını kur
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
                    PartyRoleTypeId = ReferenceDataIds.PartyRoleType.User,
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

        // 3. Navigation chain'i manuel kur
        // AuthHelper.ResolveUserInfo: appUser.DigitalIdentity.PartyRole.Party.Individual
        // EF bu back-reference'ları SaveChanges sonrası doldurur;
        // token üretimi AddAsync öncesi yapıldığı için burada elle set ediyoruz.
        var partyRole = party.PartyRoles!.First();
        var digitalId = partyRole.DigitalIdentity
            ?? throw new InvalidOperationException("DigitalIdentity oluşturulamadı.");
        var appUser   = digitalId.ApplicationUser
            ?? throw new InvalidOperationException("ApplicationUser oluşturulamadı.");

        appUser.DigitalIdentity   = digitalId;
        digitalId.PartyRole       = partyRole;
        partyRole.Party           = party;

        digitalId.Credentials.First().ContactMedia.First().Party = party;

        // 4. Token üret
        var (userId, userName, userIdentifier) = Helper.ResolveUserInfo(appUser);
        var accessToken = _tokenService.CreateAccessToken(userName, appUser.Id, userIdentifier, ReferenceDataIds.PartyRoleType.UserCd);
        var refreshToken = _tokenService.CreateRefreshToken(appUser.Id);

        appUser.RefreshTokens.Add(refreshToken);

        // 5. Kaydet
        await _partyRepository.AddAsync(party, cancellationToken);

        _logger.LogInformation(
            "Yeni kullanıcı kaydoldu. UserId: {UserId}, Identifier: {Identifier}",
            appUser.Id, request.Identifier);

        return new AuthResult
        {
            UserId                 = userId,
            UserName               = userName,
            UserIdentifier         = userIdentifier,
            AccessToken            = accessToken.Token,
            AccessTokenExpiration  = accessToken.Expiration,
            RefreshToken           = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.ExpiresAt
        };
    }
}