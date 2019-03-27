using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class ProviderServices : SiteEntity
    {

        public virtual Provider provider { get; set; }
        public virtual string Name { get; set; }
        public virtual string description { get; set; }
        public virtual string OpeningDays { get; set; }


    }
}