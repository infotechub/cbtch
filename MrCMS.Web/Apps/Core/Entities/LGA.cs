using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Lga : SiteEntity
    {
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual long State { get; set; }
        public virtual bool Status { get; set; }
    }
}