using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Benefit : SiteEntity
    {
        public virtual string Name { get; set; }
        public virtual int Benefitcategory { get; set; }
        public virtual string Description { get; set; }
        public virtual string Benefitlimit { get; set; }
        public virtual string CategoryName { get; set; }
        public virtual bool Status { get; set; }
    }
}