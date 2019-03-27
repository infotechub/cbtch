using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class ShortCodeMsg : SiteEntity
    {

        public virtual string Mobile { get; set; }
        public virtual string Msg { get; set; }
        public virtual DateTime MsgTime { get; set; }
        public virtual bool Status { get; set; }

    }
}