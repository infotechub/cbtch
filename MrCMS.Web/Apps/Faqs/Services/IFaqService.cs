using System.Collections.Generic;
using MrCMS.Web.Apps.Faqs.Entities;

namespace MrCMS.Web.Apps.Faqs.Services
{
    public interface IFaqService
    {
        void Add(Faq speaker);
        void Update(Faq speaker);
        void Delete(Faq speaker);
    }
}