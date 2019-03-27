using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class EmailDEST : SiteEntity
    {
        public virtual string code { get; set; }
        public virtual string desc { get; set; }
        public virtual string emailaddress { get; set; }

    }
}