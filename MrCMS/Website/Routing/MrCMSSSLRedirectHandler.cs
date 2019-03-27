using System.Web.Routing;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Settings;

namespace MrCMS.Website.Routing
{
    public class MrCMSSSLRedirectHandler : IMrCMSRouteHandler
    {
        private readonly IGetWebpageForRequest _getWebpageForRequest;
        private readonly SiteSettings _siteSettings;

        public MrCMSSSLRedirectHandler(IGetWebpageForRequest getWebpageForRequest, SiteSettings siteSettings)
        {
            _getWebpageForRequest = getWebpageForRequest;
            _siteSettings = siteSettings;
        }

        public int Priority { get { return 9750; } }
        public bool Handle(RequestContext context)
        {
            System.Uri url = context.HttpContext.Request.Url;
            string scheme = url.Scheme;

            Webpage webpage = _getWebpageForRequest.Get(context);
            if (webpage == null)
                return false;
            if (webpage.RequiresSSL(context.HttpContext.Request, _siteSettings) && scheme != "https")
            {
                string redirectUrl = url.ToString().Replace(scheme + "://", "https://");
                context.HttpContext.Response.RedirectPermanent(redirectUrl);
                return true;
            }
            if (!webpage.RequiresSSL(context.HttpContext.Request, _siteSettings) && scheme != "http")
            {
                string redirectUrl = url.ToString().Replace(scheme + "://", "http://");
                context.HttpContext.Response.RedirectPermanent(redirectUrl);
                return true;
            }
            return false;
        }
    }
}