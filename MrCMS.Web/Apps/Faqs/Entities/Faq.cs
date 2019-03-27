using MrCMS.Entities;
using MrCMS.Web.Apps.Faqs.Pages;

namespace MrCMS.Web.Apps.Faqs.Entities
{
    public class Faq : SiteEntity
    {
        public virtual string Question { get; set; }
        public virtual string Answer { get; set; }

        public virtual ShowFaqs ShowFaqs { get; set; }
    }


}