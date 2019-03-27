using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class SmsConfig : SiteEntity
    {

        public virtual string BdaySmsTemplate { get; set; }
        public virtual bool PreScheduleText { get; set; }
        public virtual int Mode { get; set; }
        public virtual bool Active { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
    }


}
