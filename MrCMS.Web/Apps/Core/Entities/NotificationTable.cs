using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class NotificationTable : SiteEntity
    {

        public virtual int Notificationcode { get; set; }
        public virtual string Roles { get; set; }
        public virtual string Description { get; set; }
        public virtual bool Active { get; set; }

    }
}