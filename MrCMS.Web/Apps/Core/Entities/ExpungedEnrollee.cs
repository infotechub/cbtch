using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class ExpungedEnrollee : SiteEntity
    {
        public virtual int Enrolleeid { get; set; }
        public virtual string Enrolleepolicyno { get; set; }
        public virtual string ExpungeNote { get; set; }
        public virtual int Expungedby { get; set; }
        public virtual DateTime Dateexpunged { get; set; }

    }
}