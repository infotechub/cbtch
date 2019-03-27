using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Web.Apps.Faqs.Entities;
using MrCMS.Web.Apps.Faqs.Widgets;

namespace MrCMS.Web.Apps.Faqs.Models
{
    public class RandomFaqModel
    {
        public RandomFaq RandomFaq { get; set; }
        public Faq Faq { get; set; }
    }
}