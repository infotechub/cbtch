using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Subscription : SiteEntity
    {

        public virtual string SubscriptionCode { get; set; }
        public virtual int CompanyId { get; set; }
        public virtual SubscriptionMode SubscriptionMode { get; set; }
        public virtual CompanySubsidiary Subsidiary { get; set; }
        public virtual string Companyplans { get; set; }
        public virtual DateTime? Startdate { get; set; }
        public virtual int Duration { get; set; }
        public virtual DateTime? Expirationdate { get; set; }
        public virtual string Note { get; set; }
        public virtual int Status { get; set; }
        public virtual int Createdby { get; set; }
        public virtual int AuthorizationStatus { get; set; }
        public virtual string AuthorizationNote { get; set; }
        public virtual string DisapprovalNote { get; set; }
        public virtual string TerminationNote { get; set; }
        public virtual int Terminatedby { get; set; }
        public virtual int AuthorizedBy { get; set; }
        public virtual int DisapprovedBy { get; set; }
        public virtual DateTime? AuthorizedDate { get; set; }
        public virtual DateTime? DisapprovalDate { get; set; }
    }
}