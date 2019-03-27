using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class PaymentBatch : SiteEntity
    {
        private List<ClaimBatch> _claimbatch;
        public PaymentBatch()
        {
            _claimbatch = new List<ClaimBatch>();

            ClaimBatchList = _claimbatch;

        }
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        public virtual string Note { get; set; }
        public virtual IList<ClaimBatch> ClaimBatchList { get; set; }
        public virtual PaymentStatus status { get; set; }
        public virtual DateTime? datepaymentstarted { get; set; }
        public virtual DateTime? datepaymentcompleted { get; set; }
        public virtual DateTime? terminationdate { get; set; }
        public virtual int paidby { get; set; }
        public virtual int terminatedby { get; set; }
        public virtual int createdBy { get; set; }


    }
}