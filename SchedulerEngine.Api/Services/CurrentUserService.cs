using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SchedulerEngine.Core.Services;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.Exceptions;

namespace SchedulerEngine.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepository<ApplicationUser, int> _applicationUserRepository;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IRepository<ApplicationUser, int> applicationUserRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _applicationUserRepository = applicationUserRepository;
    }

    public int? UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Sid)?.Value;

            return !string.IsNullOrEmpty(claim) ? Convert.ToInt32(claim) : null;
        }
    }

    // JWT'ye PartyRoleId hiç konmuyor (TokenService.cs'e göre sadece
    // ApplicationUser.Id (ClaimTypes.Sid) taşınıyor) — bu yüzden her
    // ihtiyaç duyulduğunda ApplicationUser → DigitalIdentity → PartyRole
    // zincirinden bir DB lookup gerekiyor. Bunu her controller'da tekrar
    // yazmak yerine burada, tek yerde, merkezi olarak çözüyoruz.
    //
    // Performans notu: bu her çağrıda bir DB round-trip demek. İleride
    // PartyRoleId'yi login sırasında JWT claim'ine de eklerseniz (TokenService.
    // CreateAccessToken'a bir Claim daha eklemek yeterli), bu metodu
    // claim'den okuyacak şekilde sadeleştirebiliriz — DB'ye hiç gitmeden.
    public async Task<int> GetPartyRoleIdAsync(CancellationToken ct = default)
    {
        var userId = UserId ?? throw new UnauthorizedException("Kullanıcı doğrulanamadı.");

        var user = await _applicationUserRepository.FindOneAsync(
            u => u.Id == userId,
            include: q => q.Include(u => u.DigitalIdentity).ThenInclude(di => di.PartyRole),
            ct: ct);

        var partyRoleId = user?.DigitalIdentity?.PartyRole?.Id;

        return partyRoleId
            ?? throw new NotFoundException("Kullanıcının PartyRole kaydı bulunamadı.");
    }
}
