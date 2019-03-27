using System.Web.Routing;
using MrCMS.Settings;

namespace MrCMS.Website.Routing
{
    public class MrCMSAdminSSLRedirectHandler : IMrCMSRouteHandler
    {
        private readonly SiteSettings _siteSettings;

        public MrCMSAdminSSLRedirectHandler(SiteSettings siteSettings)
        {
            _siteSettings = siteSettings;
        }

        public int Priority { get { return 1040; } }
        public bool Handle(RequestContext context)
        {
            System.Uri url = context.HttpContext.Request.Url;
            string scheme = url.Scheme;
            if (CurrentRequestData.CurrentUserIsAdmin && _siteSettings.SSLAdmin && _siteSettings.SiteIsLive && !context.HttpContext.Request.IsLocal)
            {
                if (scheme == "http")
                {
                    string redirectUrl = url.ToString().Replace(scheme + "://", "https://");
                    context.HttpContext.Response.Redirect(redirectUrl);
                    return true;
                }
                return false;
            }
            if (CurrentRequestData.CurrentUserIsAdmin && scheme == "http" && _siteSettings.SSLAdmin && _siteSettings.SiteIsLive && !context.HttpContext.Request.IsLocal)
            {
                string redirectUrl = url.ToString().Replace(scheme + "://", "https://");
                context.HttpContext.Response.Redirect(redirectUrl);
                return true;
            }
            return false;
        }
    }
}