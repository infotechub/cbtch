using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class ProviderAccount : SiteEntity
    {
        public virtual int Providerid { get; set; }
        public virtual int BankId { get; set; }
        public virtual string Bankaccountname { get; set; }
        public virtual string Bankaccountnum { get; set; }
        public virtual string Bankbranch { get; set; }
        public virtual string Note { get; set; }
        public virtual bool Status { get; set; }
        public virtual int AuthorizationStatus { get; set; }
        public virtual string AuthorizationNote { get; set; }
        public virtual string DisapprovalNote { get; set; }
        public virtual int AuthorizedBy { get; set; }
        public virtual int DisapprovedBy { get; set; }
        public virtual DateTime? AuthorizedDate { get; set; }
        public virtual DateTime? DisapprovalDate { get; set; }
    }
}