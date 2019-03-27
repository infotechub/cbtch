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
    public class CustomizePlanPage : Webpage, IUniquePage
    {
        public virtual string CustomizePlanName { get; set; }
        public virtual string Companyplanid { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual string PlanType { get; set; }
        public virtual string Planfriendlyname { get; set; }
        public virtual string AnnualPremium { get; set; }
        public virtual string Discountlump { get; set; }
        public virtual string Discountperenrollee { get; set; }
        public virtual string Createdby { get; set; }
        public virtual string Description { get; set; }

        public virtual bool AllowChildEnrollee { get; set; }
    }


}