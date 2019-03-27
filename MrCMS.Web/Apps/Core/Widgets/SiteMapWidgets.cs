using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MrCMS.Entities.Widget;
using MrCMS.Web;
using MrCMS.Website;

namespace MrCMS.Web.Apps.Core.Widgets
{
    public class SiteMapWidgets : Widget
    {
        [AllowHtml]
        public virtual Dictionary<string, string> Result
        {
            get
            {

                Dictionary<string, string> response = new Dictionary<string, string>();
                MrCMS.Entities.Documents.Web.Webpage currentPage = CurrentRequestData.CurrentPage;
                IEnumerable<MrCMS.Entities.Documents.Web.Webpage> activePages = currentPage.ActivePages;

                foreach (MrCMS.Entities.Documents.Web.Webpage page in activePages)
                {
                    string title = page.Name;
                    string link = page.AbsoluteUrl;
                    response.Add(title, link);
                }
                return response;

            }
        }
    }
}