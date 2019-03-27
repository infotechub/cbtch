using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class ClaimBatch : SiteEntity
    {

        private IList<Claim> _claimslist = new List<Claim>();
        public ClaimBatch()
        {
            IncomingClaims = new List<IncomingClaims>();
            Claims = new List<Claim>();
        }
        public virtual int ProviderId { get; set; }
        public virtual string ProviderName { get; set; }
        public virtual int month { get; set; }
        public virtual int year { get; set; }
        public virtual PaymentBatch paymentbatch { get; set; }
        public virtual IList<IncomingClaims> IncomingClaims { get; set; }
        public virtual IList<Claim> Claims
        {
            get
            {
                return _claimslist;//.Where(x => x.IsDeleted == false).ToList<Claim>();
            }
            set
            {
                _claimslist = value;
            }
        }
        public virtual Utility.ClaimBatch Batch { get; set; }
        public virtual ClaimBatchStatus status { get; set; }
        public virtual int submitedVetbyUser { get; set; }
        public virtual int submitedReviewbyUser { get; set; }
        public virtual DateTime? SubmitedForReviewDate { get; set; }
        public virtual DateTime? reviewDate { get; set; }
        public virtual int reviewedBy { get; set; }
        public virtual DateTime? VetDate { get; set; }
        public virtual DateTime? SubmitedForPaymentDate { get; set; }
        public virtual int submitedPaymentbyUser { get; set; }
        public virtual AuthorizationStatus AuthorizationStatus { get; set; }
        public virtual string AuthorizationNote { get; set; }
        public virtual string DisapprovalNote { get; set; }
        public virtual int AuthorizedBy { get; set; }
        public virtual int DisapprovedBy { get; set; }
        public virtual DateTime? AuthorizedDate { get; set; }
        public virtual DateTime? DisapprovalDate { get; set; }
        public virtual string DeletionNote { get; set; }

        //payment stuff the amount paid and all
        public virtual decimal AmountPaid { get; set; }
        //public virtual PaymentMethod paymentmethod { get; set; }
        public virtual string paymentmethodstring { get; set; }
        public virtual string paymentref { get; set; }
        public virtual string chequeno { get; set; }
        public virtual string sourceBankName { get; set; }
        public virtual string sourceBankAccountNo { get; set; }
        public virtual string DestBankName { get; set; }
        public virtual string DestBankAccountNo { get; set; }
        public virtual string remark { get; set; }
        public virtual DateTime? paymentdate { get; set; }
        public virtual string paidby { get; set; }
        public virtual int markpaidby { get; set; }
        public virtual int claimscountfromclient { get; set; }
        public virtual bool paymentadvicesent { get; set; }
        public virtual DateTime? datepaymentadvicesent { get; set; }
        public virtual bool isremote { get; set; }

    }
}