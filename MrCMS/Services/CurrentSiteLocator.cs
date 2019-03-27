using System;
using System.Configuration;
using System.Linq;
using System.Web;
using MrCMS.Entities.Multisite;
using NHibernate;

namespace MrCMS.Services
{
    public class CurrentSiteLocator : ICurrentSiteLocator
    {
        private readonly ISession _session;
        private readonly HttpRequestBase _requestBase;
        private Site _currentSite;
        public CurrentSiteLocator(ISession session, HttpRequestBase requestBase)
        {
            _session = session;
            _requestBase = requestBase;
        }

        public Site GetCurrentSite()
        {
            return _currentSite ?? (_currentSite = GetSiteFromSettingForDebugging() ?? GetSiteFromRequest());
        }

        private Site GetSiteFromSettingForDebugging()
        {
            string appSetting = ConfigurationManager.AppSettings["debugSiteId"];

            int id;
            return int.TryParse(appSetting, out id) ? _session.Get<Site>(id) : null;
        }

        private Site GetSiteFromRequest()
        {
            string authority = _requestBase.Url.Authority;

            System.Collections.Generic.IList<Site> allSites = _session.QueryOver<Site>().Fetch(s => s.RedirectedDomains).Eager.Cacheable().List();
            System.Collections.Generic.List<RedirectedDomain> redirectedDomains = allSites.SelectMany(s => s.RedirectedDomains).ToList();
            Site site = allSites.FirstOrDefault(s => s.BaseUrl != null && s.BaseUrl.Equals(authority, StringComparison.OrdinalIgnoreCase));
            if (site == null)
            {
                RedirectedDomain redirectedDomain =
                    redirectedDomains.FirstOrDefault(
                        s => s.Url != null && s.Url.Equals(authority, StringComparison.OrdinalIgnoreCase));
                if (redirectedDomain != null)
                    site = redirectedDomain.Site;
            }

            return site ?? allSites.First();
        }
    }
}