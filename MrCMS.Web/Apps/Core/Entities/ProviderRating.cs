using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class ProviderRating : SiteEntity
    {
        public virtual int providerID { get; set; }
        public virtual int rating { get; set; }
        public virtual string FeedBack { get; set; }
        public virtual int enrolleeid { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual DateTime dateaccessed { get; set; }
        public virtual string Email { get; set; }
    }
}