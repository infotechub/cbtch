using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Issues : SiteEntity
    {

        public virtual string subject { get; set; }
        public virtual string Details { get; set; }
        public virtual IssuesStatus status { get; set; }
        public virtual string providers { get; set; }
        public virtual string companies { get; set; }
        public virtual priority prority { get; set; }
        public virtual string users { get; set; }
        public virtual string report { get; set; }
        public virtual bool escalated { get; set; }

    }
}