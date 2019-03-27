using MrCMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class CompanyBranch : SiteEntity
    {

        public virtual Company company { get; set; }
        public virtual int Statecode { get; set; }
        public virtual string Branch { get; set; }

    }
}