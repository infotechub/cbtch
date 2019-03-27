using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Services.Widgets;
using MrCMS.Web.Apps.Core.Models.Navigation;
using MrCMS.Web.Apps.Core.Widgets;
using MrCMS.Website;
using NHibernate;
using NHibernate.Criterion;

namespace MrCMS.Web.Apps.Core.Services.Widgets
{
    public class GetNavigationRecords : GetWidgetModelBase<Navigation>
    {
        private readonly ISession _session;

        public GetNavigationRecords(ISession session)
        {
            _session = session;
        }

        public override object GetModel(Navigation widget)
        {
            //FA icon added by Tony
            IList<Webpage> rootPages = GetPages(null);
            IList<Webpage> childPages = widget.IncludeChildren ? GetPages(rootPages) : new List<Webpage>();
            List<NavigationRecord> navigationRecords =
                rootPages.Where(webpage => webpage.Published).OrderBy(webpage => webpage.DisplayOrder)
                       .Select(webpage => new NavigationRecord
                       {
                           FaIcon = webpage.FaIcon,
                           Text = MvcHtmlString.Create(webpage.Name),
                           Url = MvcHtmlString.Create("/" + webpage.LiveUrlSegment),
                           Children = childPages.Where(webpage1 => webpage1.ParentId == webpage.Id)
                                            .Select(webpage1 =>
                                                    new NavigationRecord
                                                    {
                                                        Text = MvcHtmlString.Create(webpage1.Name),
                                                        Url = MvcHtmlString.Create("/" + webpage1.LiveUrlSegment)
                                                    }).ToList()
                       }).ToList();

            return new NavigationList(navigationRecords.ToList());
        }

        private IList<Webpage> GetPages(IList<Webpage> parents)
        {
            IQueryOver<Webpage, Webpage> queryOver = _session.QueryOver<Webpage>();
            if (parents == null)
            {
                queryOver = queryOver.Where(webpage => webpage.Parent == null);
            }
            else
            {
                List<int> parentIds = parents.Select(p => p.Id).ToList();
                queryOver = queryOver.Where(webpage => webpage.Parent.Id.IsIn(parentIds));
            }

            List<Webpage> resp = queryOver.Where(webpage => webpage.RevealInNavigation)
                    .OrderBy(webpage => webpage.DisplayOrder).Asc
                    .Cacheable()
                    .List().ToList(x => x.Published);

            //Check if the user is permitted for the particular page
            //Added by Tony navigation returns for only users that are permitted
            MrCMS.Entities.People.User user = CurrentRequestData.CurrentUser;

            if (user != null)
            {
                ISet<MrCMS.Entities.People.UserRole> userRoles = user.Roles;
                List<Webpage> notPermitted = resp.Where(page => !userRoles.Any(x => x.FrontEndWebpages.Contains(page))).ToList();
                //Remove the pages that are not permitted


                foreach (Webpage item in notPermitted)
                {
                    if (item.ShowOnlyIfPermitted)
                    {
                        resp.Remove(item);
                    }


                }
            }
            else
            {

                //Clear all Tabs
                resp.Clear();
            }

            return resp;

        }
    }
}