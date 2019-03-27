using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class ServiceTariff : SiteEntity
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string Unit { get; set; }
        public virtual decimal Price { get; set; }
        public virtual string AlternatePrice { get; set; }
        public virtual bool PreauthorizationRequired { get; set; }
        public virtual int GroupId { get; set; }
        public virtual string Groupname { get; set; }
        public virtual string Remark { get; set; }
    }
}