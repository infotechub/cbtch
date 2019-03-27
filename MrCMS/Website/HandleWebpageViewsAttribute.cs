using System.Web.Mvc;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Website
{
    public class HandleWebpageViewsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ViewResult result = filterContext.Result as ViewResult;
            if (result == null) return;
            Webpage webpage = result.Model as Webpage;
            if (webpage == null) return;
            MrCMSApplication.Get<IProcessWebpageViews>().Process(result, webpage);
        }
    }
}