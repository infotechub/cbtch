using MrCMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class VettingProtocol : SiteEntity
    {
        public virtual string Diagnosis { get; set; }
        public virtual string investigations { get; set; }
        public virtual string treatment { get; set; }
        public virtual string specialist { get; set; }


    }
}