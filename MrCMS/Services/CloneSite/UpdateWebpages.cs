using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Multisite;
using NHibernate;

namespace MrCMS.Services.CloneSite
{
    [CloneSitePart(-70)]
    public class UpdateWebpages : ICloneSiteParts
    {
        private readonly ICloneWebpageSiteParts _cloneWebpageSiteParts;
        private readonly ISession _session;

        public UpdateWebpages(ICloneWebpageSiteParts cloneWebpageSiteParts, ISession session)
        {
            _cloneWebpageSiteParts = cloneWebpageSiteParts;
            _session = session;
        }

        public void Clone(Site @from, Site to, SiteCloneContext siteCloneContext)
        {
            System.Collections.Generic.IList<Webpage> webpages = _session.QueryOver<Webpage>().Where(webpage => webpage.Site.Id == to.Id).List();
            foreach (Webpage webpage in webpages)
            {
                Webpage original = siteCloneContext.GetOriginal(webpage);
                if (original != null)
                    _cloneWebpageSiteParts.Clone(original, webpage, siteCloneContext);
            }
        }
    }
}