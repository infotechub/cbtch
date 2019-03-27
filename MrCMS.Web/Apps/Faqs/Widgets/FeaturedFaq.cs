using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities.Widget;
using MrCMS.Web.Apps.Faqs.Entities;

namespace MrCMS.Web.Apps.Faqs.Widgets
{
    public class FeaturedFaq : Widget
    {
        public virtual Faq Faq { get; set; }
    }
}