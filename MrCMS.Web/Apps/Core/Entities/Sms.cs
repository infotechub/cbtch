using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Sms : SiteEntity
    {

        public virtual string Msisdn { get; set; }
        public virtual int EnrolleeId { get; set; }
        public virtual string FromId { get; set; }
        public virtual string Message { get; set; }
        public virtual SmsStatus Status { get; set; }
        public virtual SmsType Type { get; set; }
        public virtual DateTime? DeliveryDate { get; set; }
        public virtual DateTime? DateDelivered { get; set; }
        public virtual string ServerResponse { get; set; }
        public virtual string ServerCode { get; set; }
        public virtual string ServerStatus { get; set; }
        public virtual int CreatedBy { get; set; }
    }
}