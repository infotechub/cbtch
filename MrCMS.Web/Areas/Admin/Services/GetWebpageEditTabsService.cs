using System.Collections.Generic;
using System.Linq;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Web.Areas.Admin.Models.WebpageEdit;
using Ninject;

namespace MrCMS.Web.Areas.Admin.Services
{
    public class GetWebpageEditTabsService : IGetWebpageEditTabsService
    {
        private readonly IKernel _kernel;

        public GetWebpageEditTabsService(IKernel kernel)
        {
            _kernel = kernel;
        }

        public List<WebpageTabBase> GetEditTabs(Webpage page)
        {
            List<WebpageTabBase> tabsToShow =
                TypeHelper.GetAllConcreteTypesAssignableFrom<WebpageTabBase>()
                    .Select(type => _kernel.Get(type))
                    .OfType<WebpageTabBase>()
                    .Where(@base => @base.ShouldShow(page))
                    .ToList();

            List<WebpageTabBase> rootTabs = tabsToShow.Where(@base => @base.ParentType == null).OrderBy(@base => @base.Order).ToList();
            foreach (WebpageTabBase tab in rootTabs)
            {
                AssignChildren(tab, tabsToShow);
            }

            return rootTabs;
        }

        private void AssignChildren(WebpageTabBase tab, List<WebpageTabBase> allTabs)
        {
            WebpageTabGroup tabGroup = tab as WebpageTabGroup;
            if (tabGroup == null)
            {
                return;
            }
            List<WebpageTabBase> children =
                allTabs.Where(x => x.ParentType == tabGroup.GetType()).OrderBy(@base => @base.Order).ToList();
            tabGroup.SetChildren(children);
            foreach (WebpageTabBase tabBase in children)
            {
                AssignChildren(tabBase, allTabs);
            }
        }
    }
}