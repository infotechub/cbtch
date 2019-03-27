using System.Collections.Generic;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Apps.Faqs.Entities;
using MrCMS.Web.Apps.Faqs.Models;
namespace MrCMS.Web.Apps.Faqs.Pages
{
    public class ShowFaqs : Webpage
    {
        public virtual IList<Faq> Faqs { get; set; }

    }


}