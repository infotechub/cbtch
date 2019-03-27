using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{


    public class ClaimService : SiteEntity
    {
        public virtual Claim Claim { get; set; }
        public virtual string ServiceName { get; set; }
        public virtual string ServiceDescription { get; set; }
        public virtual string Duration { get; set; }
        public virtual string rate { get; set; }
        public virtual decimal? InitialAmount { get; set; }
        public virtual decimal? costofdrug { get; set; }
        public virtual bool flagred { get; set; }
        public virtual int ServiceId { get; set; }
        public virtual decimal? VettedAmount { get; set; }
        public virtual string VettingComment { get; set; }

        public virtual ClaimsBillStatus status { get; set; }


    }
}