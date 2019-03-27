using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class IncomingClaims : SiteEntity
    {

        public virtual ClaimBatch ClaimBatch { get; set; }
        public virtual int providerid { get; set; }
        public virtual int month { get; set; }
        public virtual string month_string { get; set; }
        public virtual int year { get; set; }
        public virtual DateTime? fullDateofbill { get; set; }
        public virtual string deliveredby { get; set; }
        public virtual DateTime? DateReceived { get; set; }
        public virtual int receivedBy { get; set; }
        public virtual int transferedTo { get; set; }
        public virtual int transferstatus { get; set; }
        public virtual int noofencounter { get; set; }
        public virtual decimal totalamount { get; set; }
        public virtual ReceivedClaimStatus status { get; set; }
        public virtual DateTime? dateTransferAcknowledged { get; set; }
        public virtual string Note { get; set; }
        //this part is for the captures
        public virtual string CapturerList { get; set; }
        public virtual DateTime? captureStarted { get; set; }
        public virtual string caption { get; set; }
        public virtual bool IsRemoteSubmission { get; set; }
        //public virtual IList<Provider> Claims { get; set; }

    }
}