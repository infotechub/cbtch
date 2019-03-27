using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class AuthorizationCode : SiteEntity
    {
        public virtual string authorizationCode { get; set; }
        public virtual string policyNumber { get; set; }
        public virtual string enrolleeName { get; set; }
        public virtual string EnrolleeCompany { get; set; }
        public virtual string Diagnosis { get; set; }
        public virtual string TypeofAuthorization { get; set; }
        public virtual int enrolleeAge { get; set; }
        public virtual int enrolleeID { get; set; }
        public virtual string Plan { get; set; }
        public virtual string Note { get; set; }
        public virtual string requestername { get; set; }
        public virtual string requesterphone { get; set; }
        public virtual int provider { get; set; }
        public virtual int generatedby { get; set; }
        public virtual int Authorizedby { get; set; }

        public virtual bool Isadmission { get; set; }
        public virtual bool isdelivery { get; set; }
        public virtual bool deliverysmssent { get; set; }
        public virtual bool IsNHIS { get; set; }
        public virtual DateTime? AdmissionDate { get; set; }
        public virtual int DaysApprovded { get; set; }
        public virtual DateTime? DischargeDate { get; set; }
        public virtual admissionStatus admissionStatus { get; set; }
        public virtual string treatmentAuthorized { get; set; }

        public virtual bool AcknowledgedByAuthorizer { get; set; }

    }
}