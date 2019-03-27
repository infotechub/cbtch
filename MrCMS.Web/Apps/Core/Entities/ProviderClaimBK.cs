using MrCMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class ProviderClaimBK : SiteEntity
    {

        public virtual Provider provider { get; set; }
        public virtual string clientkey { get; set; }
        public virtual string data { get; set; }
    }
}