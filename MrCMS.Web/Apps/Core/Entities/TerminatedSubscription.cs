using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class TerminatedSubscription : SiteEntity
    {
        public virtual int SubscriptionId { get; set; }
        public virtual DateTime Terminationdate { get; set; }
        public virtual int TerminatedByUserId { get; set; }
        public virtual string Note { get; set; }

        public virtual int AuthorizationStatus { get; set; }
        public virtual string AuthorizationNote { get; set; }
        public virtual string AuthorizedBy { get; set; }
        public virtual DateTime AuthorizedDate { get; set; }
    }
}