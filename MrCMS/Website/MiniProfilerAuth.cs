using System.Linq;
using System.Web;
using MrCMS.Entities.People;
using MrCMS.Services;
using MrCMS.Website.Profiling;

namespace MrCMS.Website
{
    public static class MiniProfilerAuth
    {
        public static bool IsUserAllowedToSeeMiniProfilerUI(HttpRequest arg)
        {
            IUserService userService = arg.RequestContext.HttpContext.Get<IUserService>();
            if (userService == null)
                return false;
            User currentUser = userService.GetCurrentUser(arg.RequestContext.HttpContext);
            return currentUser != null && currentUser.IsAdmin;
        }

        public static bool ShouldStartFor(HttpRequest request)
        {
            bool miniProfilerEnabled = CurrentRequestData.SiteSettings != null && CurrentRequestData.SiteSettings.MiniProfilerEnabled;
            if (!miniProfilerEnabled)
                return false;

            if (CurrentRequestData.CurrentContext.GetAll<IReasonToEnableMiniProfiler>().Any(reason => reason.ShouldEnableFor(request)))
                return true;

            return !CurrentRequestData.CurrentContext.GetAll<IReasonToDisableMiniProfiler>().Any(reason => reason.ShouldDisableFor(request));
        }
    }
}