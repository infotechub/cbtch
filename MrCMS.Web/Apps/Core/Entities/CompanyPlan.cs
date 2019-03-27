using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class CompanyPlan : SiteEntity
    {
        public virtual int Companyid { get; set; }
        public virtual int Planid { get; set; }
        public virtual string Planfriendlyname { get; set; }
        public virtual decimal AnnualPremium { get; set; }
        public virtual decimal Discountlump { get; set; }
        public virtual decimal Discountperenrollee { get; set; }
        public virtual int? Createdby { get; set; }
        public virtual string Description { get; set; }
        public virtual int AuthorizationStatus { get; set; }
        public virtual string AuthorizationNote { get; set; }
        public virtual bool AllowChildEnrollee { get; set; }

        public virtual int MaxNoOfDependant { get; set; }
        public virtual string DisapprovalNote { get; set; }
        public virtual int AuthorizedBy { get; set; }
        public virtual int DisapprovedBy { get; set; }
        public virtual DateTime? AuthorizedDate { get; set; }
        public virtual DateTime? DisapprovalDate { get; set; }
        public virtual bool Status { get; set; }
        public virtual string ProviderConsession { get; set; }
    }
}