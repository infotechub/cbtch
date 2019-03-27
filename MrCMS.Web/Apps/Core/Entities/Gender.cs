using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Gender : SiteEntity
    {
        public virtual string Name { get; set; }
    }
}