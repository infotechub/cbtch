using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class ClaimHistory : SiteEntity
    {
        public virtual string PROVIDER { get; set; }
        public virtual int PROVIDERID { get; set; }
        public virtual string LOCATION { get; set; }
        public virtual string CLIENTNAME { get; set; }
        public virtual string COMPANY { get; set; }
        public virtual string POLICYNUMBER { get; set; }
        public virtual string HEALTHPLAN { get; set; }
        public virtual DateTime ENCOUNTERDATE { get; set; }
        public virtual DateTime DATERECEIVED { get; set; }
        public virtual string DIAGNOSIS { get; set; }
        public virtual string CLASS { get; set; }
        public virtual decimal AMOUNTSUBMITTED { get; set; }
        public virtual decimal AMOUNTPROCESSED { get; set; }
        public virtual string TREATMENT { get; set; }

        public virtual int SerialNo { get; set; }
    }
}