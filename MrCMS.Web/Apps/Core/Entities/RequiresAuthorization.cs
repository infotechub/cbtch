using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class RequiresAuthorization
    {
        public virtual int AuthorizationStatus { get; set; }
        public virtual string AuthorizationNote { get; set; }
        public virtual string DisapprovalNote { get; set; }
        public virtual int AuthorizedBy { get; set; }
        public virtual int DisapprovedBy { get; set; }
        public virtual DateTime AuthorizedDate { get; set; }
        public virtual DateTime DisapprovalDate { get; set; }
    }
}