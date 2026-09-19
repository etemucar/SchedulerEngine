using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Data;
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
    private readonly IUnitOfWork                       _unitOfWork;
    private readonly ILogger<RegisterCommandHandler>   _logger;

    public RegisterCommandHandler(
        IRepository<Party, int>           partyRepository,
        IRepository<ApplicationUser, int> userRepository,
        IRepository<RefreshToken, int>    refreshTokenRepository,
        ITokenService                     tokenService,
        IPasswordHasher                   passwordHasher,
        IUnitOfWork                       unitOfWork,
        ILogger<RegisterCommandHandler>   logger)
    {
        _partyRepository        = partyRepository;
        _userRepository         = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService           = tokenService;
        _passwordHasher         = passwordHasher;
        _unitOfWork             = unitOfWork;
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
        var partyRole = party.PartyRoles!.First();
        var digitalId = partyRole.DigitalIdentity
            ?? throw new InvalidOperationException("DigitalIdentity oluşturulamadı.");
        var appUser   = digitalId.ApplicationUser
            ?? throw new InvalidOperationException("ApplicationUser oluşturulamadı.");

        appUser.DigitalIdentity   = digitalId;
        digitalId.PartyRole       = partyRole;
        partyRole.Party           = party;

        digitalId.Credentials.First().ContactMedia.First().Party = party;
        digitalId.Status = GeneralStatus.Active;

        // 4. Kaydet — DÜZELTME (kritik): SaveChanges, token üretiminden ÖNCE
        // buraya taşındı. Önceki halde appUser.Id, AddAsync'TEN BİLE ÖNCE
        // token'a gömülüyordu — yani appUser.Id garanti 0'dı (int/DB-generated
        // key). Sonuç: TokenService.CreateAccessToken, JWT'nin ClaimTypes.Sid
        // claim'ine 0 yazıyordu. CurrentUserService.UserId bu claim'i okuyor
        // (bkz. CurrentUserService.cs), yani ilk kayıt token'ıyla yapılan HER
        // istek, PartyRoleId çözümlemesinde "ApplicationUser Id=0 bulunamadı"
        // (NotFoundException) ile patlıyordu.
        await _partyRepository.AddAsync(party, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Token üret — artık appUser.Id gerçek, DB'den dönen değeri taşıyor.
        var (userId, userName, userIdentifier) = Helper.ResolveUserInfo(appUser);
        var accessToken = _tokenService.CreateAccessToken(userName, userId, userIdentifier, ReferenceDataIds.PartyRoleType.UserCd);
        var refreshToken = _tokenService.CreateRefreshToken(userId);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Yeni kullanıcı kaydoldu. UserId: {UserId}, Identifier: {Identifier}",
            userId, request.Identifier);

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