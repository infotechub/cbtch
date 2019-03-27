using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class DeletedEnrollee : SiteEntity
    {
        public virtual int Parentid { get; set; }
        public virtual int Parentrelationship { get; set; }
        public virtual string Policynumber { get; set; }
        public virtual string RefPolicynumber { get; set; }
        public virtual bool HasRefPolicyNumber { get; set; }
        public virtual string Title { get; set; }
        public virtual string Surname { get; set; }
        public virtual string Othernames { get; set; }
        public virtual DateTime? Dob { get; set; }
        public virtual int Age { get; set; }
        public virtual int Maritalstatus { get; set; }
        public virtual string Occupation { get; set; }
        public virtual int Sex { get; set; }
        public virtual string Residentialaddress { get; set; }
        public virtual int Stateid { get; set; }
        public virtual int Lgaid { get; set; }
        public virtual string Mobilenumber { get; set; }
        public virtual string Mobilenumber2 { get; set; }
        public virtual string Emailaddress { get; set; }
        public virtual int Sponsorshiptype { get; set; }
        public virtual string Sponsorshiptypeothername { get; set; }
        public virtual string Preexistingmedicalhistory { get; set; }
        public virtual string Sponsorshiptypenote { get; set; }
        public virtual int Companyid { get; set; }
        public virtual int Subscriptionplanid { get; set; }
        public virtual bool Hasdependents { get; set; }
        public virtual string Specialidcardfield1 { get; set; }
        public virtual string Specialidcardfield2 { get; set; }
        public virtual string Specialidcardfield3 { get; set; }
        public virtual int Staffprofileid { get; set; }
        public virtual bool IdCardPrinted { get; set; }
        public virtual int Primaryprovider { get; set; }
        public virtual int Status { get; set; }
        public virtual bool Hasactivesubscription { get; set; }
        public virtual bool Isexpundged { get; set; }
        public virtual EnrolleePassport EnrolleePassport { get; set; }
        public virtual string ExpungeNote { get; set; }
        public virtual int Expungedby { get; set; }
        public virtual DateTime? Dateexpunged { get; set; }
        public virtual int Createdby { get; set; }
        public virtual DateTime? Datereceived { get; set; }

    }
}