using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class CompanyBenefit : SiteEntity
    {
        public virtual int Companyid { get; set; }
        public virtual int CompanyPlanid { get; set; }
        public virtual int BenefitId { get; set; }
        public virtual string BenefitLimit { get; set; }
    }
}