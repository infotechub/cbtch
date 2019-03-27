using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using MrCMS.Batching.Entities;

namespace MrCMS.Web.Areas.Admin.Services.Batching
{
    public class ExecuteRequestForNextTask : IExecuteRequestForNextTask
    {
        private readonly UrlHelper _urlHelper;
        private readonly HttpContextBase _context;

        public ExecuteRequestForNextTask(UrlHelper urlHelper, HttpContextBase context)
        {
            _urlHelper = urlHelper;
            _context = context;
        }

        public void Execute(BatchRun run)
        {
            CookieContainer cookieContainer = new CookieContainer();
            HttpCookieCollection cookies = _context.Request.Cookies;
            HttpCookie cookie = cookies[".AspNet.ApplicationCookie"];
            cookieContainer.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, _context.Request.Url.Host));
            HttpClientHandler httpClientHandler = new HttpClientHandler { UseCookies = true, CookieContainer = cookieContainer };
            HttpClient httpClient = new HttpClient(httpClientHandler);
            string url = _urlHelper.Action("ExecuteNext", "BatchExecution", new { id = run.Guid }, "http");
            httpClient.GetAsync(url);
        }
    }
}