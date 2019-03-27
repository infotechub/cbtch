using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class AuthorizationRequest : SiteEntity
    {
        public virtual int providerid { get; set; }
        public virtual string providerName { get; set; }
        public virtual string policynumber { get; set; }
        public virtual string fullname { get; set; }
        public virtual string company { get; set; }
        public virtual string diagnosis { get; set; }
        public virtual string reasonforcode { get; set; }
        public virtual bool isnew { get; set; }

    }
}