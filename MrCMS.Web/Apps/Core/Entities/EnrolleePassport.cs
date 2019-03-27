using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class EnrolleePassport : SiteEntity
    {
        public virtual int Enrolleeid { get; set; }
        public virtual string Enrolleepolicyno { get; set; }
        public virtual byte[] Imgraw { get; set; }
    }
}