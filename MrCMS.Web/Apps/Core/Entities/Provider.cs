using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Provider : SiteEntity
    {
        private IList<ProviderServices> _providerservices = new List<ProviderServices>();
        private IList<ProviderClaimBK> _ProviderClaimBK = new List<ProviderClaimBK>();
        public Provider()
        {
            Servicesanddays = new List<ProviderServices>();
            ClaimBK = new List<ProviderClaimBK>();

        }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual string SubCode { get; set; }
        public virtual string Email { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Phone2 { get; set; }
        public virtual string Website { get; set; }
        public virtual string Address { get; set; }
        public virtual string Area { get; set; }
        public virtual State State { get; set; }
        public virtual Lga Lga { get; set; }
        public virtual ProviderAccount Provideraccount { get; set; }
        public virtual ProviderAccount Provideraccount2 { get; set; }
        public virtual ProviderLogin providerlogin { get; set; }

        public virtual string PaymentEmail1 { get; set; }
        public virtual string PaymentEmail2 { get; set; }
        public virtual int Assignee { get; set; }
        public virtual string Providergpscordinate { get; set; }
        public virtual string Providerservices { get; set; }
        public virtual string Providerplans { get; set; }
        public virtual string ProviderTariffs { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual int AuthorizationStatus { get; set; }
        public virtual string AuthorizationNote { get; set; }
        public virtual string DisapprovalNote { get; set; }
        public virtual int AuthorizedBy { get; set; }
        public virtual int DisapprovedBy { get; set; }
        public virtual DateTime? AuthorizedDate { get; set; }
        public virtual DateTime? DisapprovalDate { get; set; }
        public virtual string DeletionNote { get; set; }
        public virtual long Parentid { get; set; }
        public virtual bool Status { get; set; }
        public virtual ProviderCategory Category { get; set; }
        public virtual IList<ProviderServices> Servicesanddays
        {
            get


            {
                return _providerservices;
            }
            set
            {
                _providerservices = value;
            }
        }
        public virtual bool isDelisted { get; set; }
        public virtual string DelistNote { get; set; }
        public virtual DateTime? delisteddate { get; set; }
        public virtual int delistedBy { get; set; }
        public virtual IList<ProviderClaimBK> ClaimBK
        {
            get


            {
                return _ProviderClaimBK;
            }
            set
            {
                _ProviderClaimBK = value;
            }
        }

        public virtual string CompanyConsession { get; set; }

    }
}