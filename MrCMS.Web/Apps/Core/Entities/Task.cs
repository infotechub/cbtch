using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class TaskShit : SiteEntity
    {
        public virtual string Name { get; set; }
        public virtual DateTime LastRun { get; set; }
        public virtual long RunTimerSeconds { get; set; }
        public virtual bool status { get; set; }

        public virtual bool Enabled { get; set; }

    }
}