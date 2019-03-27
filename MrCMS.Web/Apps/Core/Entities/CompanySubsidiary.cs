using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class CompanySubsidiary : SiteEntity
    {
        public virtual int ParentcompanyId { get; set; }
        public virtual string Subsidaryname { get; set; }
        public virtual string Subsidaryprofile { get; set; }
        public virtual int CreatedBy { get; set; }
    }
}