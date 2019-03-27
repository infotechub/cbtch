using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class EnrolleeVerificationCode : SiteEntity
    {
        public virtual int EnrolleeId { get; set; }
        public virtual EnrolleeVerificationCodeStatus Status { get; set; }
        public virtual string VerificationCode { get; set; }
        public virtual DateTime EncounterDate { get; set; }
        public virtual DateTime? DateAuthenticated { get; set; }
        public virtual DateTime? DateExpired { get; set; }

        public virtual int Channel { get; set; }
        public virtual string RequestPhoneNumber { get; set; }
        public virtual int CreatedBy { get; set; }
        public virtual int VisitPurpose { get; set; }
        public virtual int ProviderId { get; set; }
        public virtual int AuthChannel { get; set; }

        public virtual bool Pickedup { get; set; }
        public virtual int PickedUpById { get; set; }
        public virtual bool AttendedTo { get; set; }
        public virtual DateTime? DateAttendedTo { get; set; }
        public virtual string ServiceAccessed { get; set; }
        public virtual bool AuthorizationCodeGiven { get; set; }
        public virtual string AuthorizationCode { get; set; }
        public virtual int AuthorizedByUserId { get; set; }
        public virtual DateTime? DateAuthorized { get; set; }
        public virtual string AgentNote { get; set; }
        public virtual string Note { get; set; }
        public virtual evsvisittype visittype { get; set; }
        public virtual bool PostSMSSent { get; set; }

    }
}