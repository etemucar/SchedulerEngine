using System.Security.Claims;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Core.Security;

public interface ITokenService
{
    AccessToken CreateAccessToken(ApplicationUser user);
    AccessToken CreateAccessToken(string userName, int userId, string userCredential, string roleCd);
    RefreshToken CreateRefreshToken(int userId);
    ClaimsPrincipal? ValidateToken(string token);
}
