using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;
namespace MrCMS.Web.Apps.Core.Entities
{
    public class OfflineMessage : SiteEntity
    {

        public virtual string FromId { get; set; }
        public virtual string ToId { get; set; }
        public virtual string Message { get; set; }
        public virtual string MsgDate { get; set; }
        public virtual bool Read { get; set; }

    }
}