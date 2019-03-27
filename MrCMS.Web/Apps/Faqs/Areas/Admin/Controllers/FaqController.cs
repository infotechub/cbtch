using System.Web.Mvc;
using MrCMS.Web.Apps.Faqs.Entities;
using MrCMS.Web.Apps.Faqs.Pages;
using MrCMS.Web.Apps.Faqs.Services;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Apps.Faqs.Areas.Admin.Controllers
{
    public class FaqController : MrCMSAppAdminController<FaqsApp>
    {
        private readonly IFaqService _faqService;
        public FaqController(IFaqService faqService)
        {
            _faqService = faqService;
        }

        [HttpGet]
        public PartialViewResult Add(ShowFaqs faqPage)
        {
            return PartialView(new Faq { ShowFaqs = faqPage });
        }

        [HttpPost]
        [ActionName("Add")]
        public RedirectToRouteResult Add_POST(Faq item)
        {
            _faqService.Add(item);
            return RedirectToAction("Edit", "Webpage", new { id = item.ShowFaqs.Id });

        }

        [HttpGet]
        public PartialViewResult Edit(Faq item)
        {
            return PartialView(item);
        }

        [HttpPost]
        [ActionName("Edit")]
        public RedirectToRouteResult Edit_POST(Faq item)
        {
            _faqService.Update(item);
            return RedirectToAction("Edit", "Webpage", new { id = item.ShowFaqs.Id });
        }

        [HttpGet]
        public PartialViewResult Delete(Faq item)
        {
            return PartialView(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        public RedirectToRouteResult Delete_POST(Faq item)
        {
            _faqService.Delete(item);
            return RedirectToAction("Edit", "Webpage", new { id = item.ShowFaqs.Id });
        }
    }


}