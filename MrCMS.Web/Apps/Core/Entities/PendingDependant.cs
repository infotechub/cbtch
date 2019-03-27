using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class PendingDependant : SiteEntity
    {


        public virtual byte[] ImgRaw { get; set; }
        public virtual string firstName { get; set; }
        public virtual string lastname { get; set; }
        public virtual DateTime dob { get; set; }
        public virtual int sex { get; set; }
        public virtual int hospital { get; set; }
        public virtual string mobile { get; set; }
        public virtual string preexisting { get; set; }
        public virtual int relationship { get; set; }
        public virtual bool Approved { get; set; }
        public virtual string Note { get; set; }
        public virtual string Principalpolicynum { get; set; }
        public virtual string principalGuid { get; set; }
    }
}