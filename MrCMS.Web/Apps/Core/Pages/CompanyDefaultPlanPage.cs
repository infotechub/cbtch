using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Models.Provider;

namespace MrCMS.Web.Apps.Core.Pages
{
    public class CustomizeDefaultPlanPage : Webpage, IUniquePage
    {
        public virtual string PlanId { get; set; }
        public virtual string PlanName { get; set; }
        public virtual string Createdby { get; set; }
        public virtual string Description { get; set; }


    }


}