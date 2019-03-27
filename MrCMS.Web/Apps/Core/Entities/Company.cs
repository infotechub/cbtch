using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Company : SiteEntity
    {

        public Company()
        {
            CompanyBranch = new List<CompanyBranch>();

        }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual string Address { get; set; }
        public virtual string City { get; set; }
        public virtual long Stateid { get; set; }
        public virtual long Parentid { get; set; }
        public virtual string Email { get; set; }
        public virtual string Website { get; set; }
        public virtual CompanySubsidiary Subsidiary { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual string Description { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual int Logoid { get; set; }
        public virtual int SubscriptionStatus { get; set; }
        public virtual string Plans { get; set; }
        public virtual int AuthorizationStatus { get; set; }
        public virtual string AuthorizationNote { get; set; }
        public virtual string DisapprovalNote { get; set; }
        public virtual int AuthorizedBy { get; set; }
        public virtual bool Status { get; set; }
        public virtual int DisapprovedBy { get; set; }
        public virtual DateTime? AuthorizedDate { get; set; }
        public virtual DateTime? DisapprovalDate { get; set; }
        public virtual string LogoLink { get; set; }
        public virtual string regCode { get; set; }
        public virtual string DeletionNote { get; set; }
        public virtual bool WeboperationMode { get; set; }
        public virtual IList<CompanyBranch> CompanyBranch { get; set; }
        public virtual int RegAgeLimit { get; set; }
        public virtual bool isRenewal { get; set; }

    }
}