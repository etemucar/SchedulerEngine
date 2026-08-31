using System.Security.Claims;
using Hangfire.Dashboard;
using SchedulerEngine.Core.Seeding; // DocDes.Core.Seeding'in SchedulerEngine karşılığı - namespace farklıysa düzelt

namespace SchedulerEngine.Api.Security;

/// <summary>
/// DocDes'teki AdminOnlyDashboardAuthFilter ile aynı mantık: ayrı bir
/// authentication scheme'e ihtiyaç yok. UseAuthentication() middleware'i
/// zaten HttpContext.User'ı dolduruyor (JWT cookie'den ya da header'dan -
/// hangisi kuruluysa), biz sadece o an dolu olan User'daki SiteAdmin
/// claim'ine bakıyoruz.
/// </summary>
public class AdminOnlyDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.HasClaim(ClaimTypes.Role, ReferenceDataIds.PartyRoleType.SiteAdminCd);
    }
}