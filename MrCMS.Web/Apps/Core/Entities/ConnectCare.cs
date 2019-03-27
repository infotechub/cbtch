using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Entities
{

    public class ConnectCareSponsor : SiteEntity
    {

        public virtual string sponsorSTRID { get; set; }
        public virtual string firstname { get; set; }
        public virtual string lastname { get; set; }
        public virtual string secondarysponsor { get; set; }
        public virtual string fullname { get; set; }
        public virtual string gender { get; set; }
        public virtual string occupation { get; set; }
        public virtual string country { get; set; }
        public virtual string state { get; set; }
        public virtual string zipcode { get; set; }
        public virtual string address { get; set; }
        public virtual string email { get; set; }
        public virtual string phonenumber { get; set; }
        public virtual string SubscriptionType { get; set; }
        public virtual bool Addon { get; set; }
        public virtual DateTime? SponsorStartDate { get; set; }
        public virtual DateTime? PolicyStartDate { get; set; }
        public virtual DateTime? PolicyEndDate { get; set; }
        public virtual int PolicynotificationConfig { get; set; }
        public virtual int instalment { get; set; }
        public virtual bool emailsent { get; set; }
        public virtual string policynumber { get; set; }
        public virtual Enrollee Enrolleeprofile { get; set; }
        public virtual bool active { get; set; }
        public virtual int Administeredby { get; set; }
        public virtual DateTime? AdministrationDate { get; set; }
        public virtual bool pushedtoMatontine { get; set; }
        public virtual DateTime? pushedDate { get; set; }

    }



    public class ConnectCareBeneficiary : SiteEntity
    {
        public virtual byte[] Passport { get; set; }
        public virtual int sponsorid { get; set; }
        public virtual string sponsoridstring { get; set; }
        public virtual string BeneficiaryID { get; set; }
        public virtual string fullname { get; set; }
        public virtual string firstname { get; set; }
        public virtual DateTime? dob { get; set; }
        public virtual string lastname { get; set; }
        public virtual string gender { get; set; }
        public virtual string country { get; set; }
        public virtual string state { get; set; }
        public virtual string address { get; set; }
        public virtual string email { get; set; }
        public virtual string city { get; set; }
        public virtual string LGA { get; set; }
        public virtual string GuardianPhonenumber { get; set; }
        public virtual string GuardianEmail { get; set; }
        public virtual string SuggestedProvider { get; set; }
        public virtual string suggestedPlan { get; set; }
        public virtual string PolicyNumber { get; set; }
        public virtual string phonenumber { get; set; }
        public virtual string relationship { get; set; }
        public virtual int age { get; set; }
        public virtual bool VerificationStatus { get; set; }
        public virtual bool active { get; set; }
        public virtual ConnectCareStatus Status { get; set; }
        public virtual string Category { get; set; }
        public virtual bool addon { get; set; }
        public virtual int Administeredby { get; set; }
        public virtual DateTime? AdministrationDate { get; set; }
        public virtual string BeneficiaryCat { get; set; }


    }


    public class ConnectcarePaymentDetails : SiteEntity
    {
        public virtual string paymentid { get; set; }
        public virtual int sponsorID { get; set; }
        public virtual string sponsorIDString { get; set; }
        public virtual string beneficiaryID { get; set; }
        public virtual string policyid { get; set; }
        public virtual decimal amountpaid { get; set; }
        public virtual string planpurchased { get; set; }
        public virtual bool addon { get; set; }
        public virtual DateTime paymentDate { get; set; }
    }
}