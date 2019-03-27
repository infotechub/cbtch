using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class ProviderLogin : SiteEntity
    {
        public virtual Provider Provider { get; set; }
        public virtual string password { get; set; }
        public virtual bool passwordchange { get; set; }
        public virtual string browserid { get; set; }
        public virtual string lastloginId { get; set; }
        public virtual DateTime? lastlogin { get; set; }
        public virtual bool active { get; set; }
        public virtual DateTime? LastClaimSubmited { get; set; }
        public virtual string email { get; set; }
        public virtual string Altemail { get; set; }
        public virtual string Altemail2 { get; set; }
        public virtual string Altemail3 { get; set; }
    }
}