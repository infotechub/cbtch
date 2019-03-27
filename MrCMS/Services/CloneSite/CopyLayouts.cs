using System.Collections.Generic;
using System.Linq;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using NHibernate;

namespace MrCMS.Services.CloneSite
{
    [CloneSitePart(-90)]
    public class CopyLayouts : ICloneSiteParts
    {
        private readonly ISession _session;

        public CopyLayouts(ISession session)
        {
            _session = session;
        }

        public void Clone(Site @from, Site to, SiteCloneContext siteCloneContext)
        {
            IEnumerable<Layout> copies = GetLayoutCopies(@from, to, siteCloneContext);

            _session.Transact(session => copies.ForEach(layout =>
            {
                session.Save(layout);
                List<LayoutArea> layoutAreas = layout.LayoutAreas.ToList();
                foreach (LayoutArea layoutArea in layoutAreas)
                {
                    session.Save(layoutArea);
                    layoutArea.Widgets.ForEach(widget => session.Save(widget));
                }
            }));
        }
        private IEnumerable<Layout> GetLayoutCopies(Site @from, Site to, SiteCloneContext siteCloneContext, Layout fromParent = null, Layout toParent = null)
        {
            IQueryOver<Layout, Layout> queryOver = _session.QueryOver<Layout>().Where(layout => layout.Site.Id == @from.Id);
            queryOver = fromParent == null
                ? queryOver.Where(layout => layout.Parent == null)
                : queryOver.Where(layout => layout.Parent.Id == fromParent.Id);
            IList<Layout> layouts = queryOver.List();

            foreach (Layout layout in layouts)
            {
                Layout copy = layout.GetCopyForSite(to);
                siteCloneContext.AddEntry(layout, copy);
                copy.LayoutAreas = layout.LayoutAreas.Select(area =>
                {
                    LayoutArea areaCopy = area.GetCopyForSite(to);
                    siteCloneContext.AddEntry(area, areaCopy);
                    areaCopy.Layout = copy;
                    areaCopy.Widgets = area.Widgets
                        .Where(widget => widget.Webpage == null)
                        .Select(widget =>
                        {
                            Entities.Widget.Widget widgetCopy = widget.GetCopyForSite(to);
                            siteCloneContext.AddEntry(widget, widgetCopy);
                            widgetCopy.LayoutArea = areaCopy;
                            return widgetCopy;
                        })
                        .ToList();
                    return areaCopy;
                }).ToList();
                copy.Parent = toParent;
                yield return copy;
                foreach (Layout child in GetLayoutCopies(@from, to, siteCloneContext, layout, copy))
                {
                    yield return child;
                }
            }
        }
    }
}