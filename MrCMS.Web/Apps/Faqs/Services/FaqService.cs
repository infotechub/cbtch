using System.Collections.Generic;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Faqs.Entities;
using NHibernate;

namespace MrCMS.Web.Apps.Faqs.Services
{
    public class FaqService : IFaqService
    {
        private readonly ISession _session;

        public FaqService(ISession session)
        {
            _session = session;
        }

        public void Add(Faq faq)
        {
            faq.ShowFaqs.Faqs.Add(faq); //required to bust page cache
            _session.Transact(session => session.Save(faq));
        }

        public void Update(Faq faq)
        {
            _session.Transact(session => session.Update(faq));

        }

        public void Delete(Faq faq)
        {
            faq.ShowFaqs.Faqs.Remove(faq); //required to bust page cache
            _session.Transact(session => session.Delete(faq));
        }
    }
}