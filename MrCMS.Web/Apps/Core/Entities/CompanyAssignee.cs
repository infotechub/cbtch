using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class CompanyAssignee : SiteEntity
    {
        public virtual int Companyid { get; set; }
        public virtual int Userid { get; set; }
        public virtual bool Status { get; set; }

    }
}