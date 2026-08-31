using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.Security;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Core.Exceptions;
using SchedulerEngine.Core.Seeding;

namespace SchedulerEngine.Service.Features.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IRepository<RefreshToken, int>    _refreshTokenRepository;
    private readonly IRepository<ApplicationUser, int> _userRepository;
    private readonly ITokenService                     _tokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IRepository<RefreshToken, int>    refreshTokenRepository,
        IRepository<ApplicationUser, int> userRepository,
        ITokenService                     tokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository         = userRepository;
        _tokenService           = tokenService;
        _logger                 = logger;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Token'ı bul
        var existingToken = await _refreshTokenRepository.FindOneAsync(
            t => t.Token == request.RefreshToken,
            i => i.Include(t => t.ApplicationUser)
                    .ThenInclude(u => u.DigitalIdentity)
                        .ThenInclude(d => d.PartyRole)
                            .ThenInclude(pr => pr.Party)
                                .ThenInclude(p => p.Individual)
                  .Include(t => t.ApplicationUser)
                    .ThenInclude(u => u.DigitalIdentity)
                        .ThenInclude(d => d.Credentials)
                            .ThenInclude(c => c.ContactMedia),
            asNoTracking: false,
            ct: cancellationToken);

        if (existingToken is null)
            throw new UnauthorizedException("Refresh token bulunamadı.");

        // 2. Aktif mi?
        if (!existingToken.IsActive)
            throw new UnauthorizedException("Refresh token geçersiz veya süresi dolmuş.");

        // 3. Eski token'ı iptal et
        existingToken.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        // 4. Kullanıcı bilgilerini çöz
        var appUser   = existingToken.ApplicationUser;
        var digitalId = appUser.DigitalIdentity;
        var partyRole   = digitalId.PartyRole;
        var individual = partyRole.Party?.Individual;
        var userName = individual != null
            ? $"{individual.GivenName} {individual.FamilyName}".Trim()
            : digitalId.Nickname ?? string.Empty;

        var contactMedium  = digitalId.Credentials
            .SelectMany(c => c.ContactMedia)
            .FirstOrDefault();
        var userIdentifier = contactMedium?.Email
            ?? contactMedium?.PhoneNumber
            ?? string.Empty;

        var roleCd = ReferenceDataIds.PartyRoleType.ToCode(partyRole.PartyRoleTypeId);

        // 5. Yeni token'ları üret
        var accessToken  = _tokenService.CreateAccessToken(userName, appUser.Id, userIdentifier, roleCd);
        var refreshToken = _tokenService.CreateRefreshToken(appUser.Id);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        _logger.LogInformation(
            "Refresh token yenilendi. UserId: {UserId}", appUser.Id);

        return new AuthResult
        {
            UserId                 = appUser.Id,
            UserName               = userName,
            UserIdentifier         = userIdentifier,
            AccessToken            = accessToken.Token,
            AccessTokenExpiration  = accessToken.Expiration,
            RefreshToken           = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.ExpiresAt
        };
    }
}