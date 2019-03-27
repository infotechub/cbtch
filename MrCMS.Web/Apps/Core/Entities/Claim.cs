using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Claim : SiteEntity
    {
        public Claim()
        {
            ServiceList = new List<ClaimService>();
            DrugList = new List<ClaimDrug>();
        }

        public virtual ClaimBatch ClaimBatch { get; set; }

        public virtual int ProviderId { get; set; }
        public virtual string ClientsideID { get; set; }
        public virtual int Enrolleeid { get; set; }

        public virtual string enrolleeFullname { get; set; }
        public virtual string enrolleeGender { get; set; }

        public virtual string enrolleeCompanyName { get; set; }
        public virtual int enrolleeCompanyId { get; set; }

        public virtual string enrolleePolicyNumber { get; set; }
        public virtual string enrolleeage { get; set; }
        public virtual string EnrolleePlan { get; set; }
        public virtual string ClaimsSerialNo { get; set; }
        public virtual string EVSCode { get; set; }
        public virtual string DoctorsName { get; set; }
        public virtual string DoctorsId { get; set; }
        public virtual string AreaOfSpecialty { get; set; }
        public virtual string Signature { get; set; }
        public virtual bool DoctorSigned { get; set; }
        public virtual DateTime? DoctorSignecDate { get; set; }
        //specialist
        public virtual string specialistName { get; set; }
        public virtual string AreaOfSpecialtyforspecialist { get; set; }
        public virtual string specialistphonenumber { get; set; }
        public virtual bool specialistSigned { get; set; }
        [DataType(DataType.Date)]
        public virtual DateTime? specialistSignecDate { get; set; }
        [DataType(DataType.Date)]
        public virtual DateTime? ServiceDate { get; set; }
        [DataType(DataType.Date)]
        public virtual DateTime? AdmissionDate { get; set; }
        [DataType(DataType.Date)]
        public virtual DateTime? DischargeDate { get; set; }
        public virtual string Durationoftreatment { get; set; }
        public virtual string Diagnosis { get; set; }
        public virtual string TreatmentGiven { get; set; }
        public virtual string TreatmentCode { get; set; }
        public virtual string referalCode { get; set; }
        public virtual IList<ClaimDrug> DrugList { get; set; }
        public virtual IList<ClaimService> ServiceList { get; set; }
        public virtual bool enrolleeSigned { get; set; }
        public virtual DateTime? EnrolleeSignDate { get; set; }
        public virtual bool AllprescibedDrugs { get; set; }
        public virtual bool LaboratoryInvestigation { get; set; }
        public virtual bool Admission { get; set; }
        public virtual bool Feeding { get; set; }
        public virtual string Note { get; set; }
        public virtual ClaimsTAGS Tag { get; set; }
        //vetting row
        public virtual int capturedBy { get; set; }
        public virtual string capturedName { get; set; }
        public virtual int vettedBy { get; set; }
        public virtual int RevettedBy { get; set; }
        public virtual DateTime? VettedDate { get; set; }
        public virtual DateTime? ReVettedDate { get; set; }
        public virtual string diagnosticsIDString { get; set; }
        public virtual ClaimsBillStatus status { get; set; }
        //newly added by provider

        public virtual bool SubmitByProvider { get; set; }
        public virtual string ipaddressofprovider { get; set; }
       
    }
}