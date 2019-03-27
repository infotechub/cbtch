using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;
namespace MrCMS.Web.Apps.Core.Entities
{
    public class UserNotification : SiteEntity
    {

        public virtual string UserId { get; set; }
        public virtual string Message { get; set; }
        public virtual int Type { get; set; }
        public virtual int Target { get; set; }
        public virtual MrCMS.Entities.People.UserRole Role { get; set; }
        public virtual bool Read { get; set; }
        public virtual string ClickAction { get; set; }

    }
}