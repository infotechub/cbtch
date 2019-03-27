using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class MobileSignup : SiteEntity
    {

        public virtual string PolicyNumber { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual string Email { get; set; }
        public virtual string CodeGenerated { get; set; }
        public virtual int Status { get; set; }
        public virtual string Smsid { get; set; }
    }
}