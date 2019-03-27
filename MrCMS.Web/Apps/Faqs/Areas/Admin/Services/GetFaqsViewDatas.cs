using System.Web.Mvc;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Faqs.Entities;
using MrCMS.Web.Apps.Faqs.Widgets;
using MrCMS.Web.Areas.Admin.Services;
using NHibernate;

namespace MrCMS.Web.Apps.Faqs.Areas.Admin.Services
{
    public class GetFaqsViewData : BaseAssignWidgetAdminViewData<FeaturedFaq>
    {
        private readonly ISession _session;

        public GetFaqsViewData(ISession session)
        {
            _session = session;
        }

        public override void AssignViewData(FeaturedFaq featuredFaq, ViewDataDictionary viewData)
        {
            viewData["faqs"] = _session.QueryOver<Faq>()
                .List()
                .BuildSelectItemList(faq => faq.Question, user => user.Id.ToString(),
                        emptyItem: new SelectListItem { Text = "Please select...", Value = "0" });
        }
    }
}