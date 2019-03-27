using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MrCMS.Web.Apps.Core.Pages;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Apps.Core.Controllers
{
    public class InvoicePageController : MrCMSAppUIController<CoreApp>
    {
        // GET: Invoice
        public ActionResult Index(InvoicePage page)
        {
            return View(page);
        }
    }
}