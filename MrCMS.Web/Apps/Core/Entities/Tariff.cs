using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Tariff : SiteEntity
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual bool Status { get; set; }
        public virtual int CreatedBy { get; set; }
        public virtual bool authstatus { get; set; }
        public virtual int authBy { get; set; }
        public virtual int defaultProvider { get; set; }
        public virtual DateTime? AuthorizedDate { get; set; }

    }
}