using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities.Widget;
using MrCMS.Services;
 using MrCMS.Entities.People;
using MrCMS.Web.Apps.Faqs.Entities;

namespace MrCMS.Web.Apps.NovoHub.Widgets
{

    public class RightSideBarWidget : Widget
    {
        public virtual IList<User> GetUsers { get;set; }


    }

}