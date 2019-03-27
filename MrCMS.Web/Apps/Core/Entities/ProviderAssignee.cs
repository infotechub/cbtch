using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class ProviderAssignee : SiteEntity
    {
        public virtual int Providerid { get; set; }
        public virtual int Userid { get; set; }
        public virtual int AssignedBy { get; set; }
        public virtual bool Status { get; set; }
    }
}