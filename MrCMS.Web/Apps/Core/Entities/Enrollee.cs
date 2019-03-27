using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Enrollee : SiteEntity
    {
        public virtual int Parentid { get; set; }
        public virtual int Parentrelationship { get; set; }
        public virtual string Policynumber { get; set; }
        public virtual string RefPolicynumber { get; set; }
        public virtual bool HasRefPolicyNumber { get; set; }
        public virtual string Title { get; set; }
        public virtual string Surname { get; set; }
        public virtual string Othernames { get; set; }
        public virtual DateTime Dob { get; set; }
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
        public virtual int LastyearBirthdaymsgsent { get; set; }
        public virtual int Bulkjobid { get; set; }


    }



    public class TempEnrollee : SiteEntity
    {
        public virtual byte[] Imgraw { get; set; }
        public virtual string Policynumber { get; set; }
        public virtual string RefPolicynumber { get; set; }
        public virtual bool HasRefPolicyNumber { get; set; }
        public virtual string Title { get; set; }
        public virtual string Surname { get; set; }
        public virtual string Othernames { get; set; }
        public virtual DateTime Dob { get; set; }
        public virtual int Age { get; set; }
        public virtual int Maritalstatus { get; set; }
        public virtual string Occupation { get; set; }
        public virtual int Sex { get; set; }
        public virtual string Residentialaddress { get; set; }
        public virtual int Stateoforiginid { get; set; }
        public virtual string Lga { get; set; }
        public virtual int Stateofresidence { get; set; }
        public virtual string Mobilenumber { get; set; }
        public virtual string Mobilenumber2 { get; set; }
        public virtual string Emailaddress { get; set; }
        public virtual int Sponsorshiptype { get; set; }
        public virtual string Sponsorshiptypeothername { get; set; }
        public virtual string Preexistingmedicalhistory { get; set; }
        public virtual string Sponsorshiptypenote { get; set; }
        public virtual int Companyid { get; set; }
        public virtual int officestate { get; set; }
        public virtual string BranchName { get; set; }
        public virtual string staffid { get; set; }
        public virtual int Subscriptionplanid { get; set; }
        public virtual bool Hasdependents { get; set; }
        public virtual string Specialidcardfield1 { get; set; }
        public virtual string Specialidcardfield2 { get; set; }
        public virtual string Specialidcardfield3 { get; set; }
        public virtual int Staffprofileid { get; set; }
        public virtual bool IdCardPrinted { get; set; }
        public virtual string Primaryprovider { get; set; }
        public virtual int Status { get; set; }
        public virtual bool Hasactivesubscription { get; set; }
        public virtual bool Isexpundged { get; set; }
        public virtual EnrolleePassport EnrolleePassport { get; set; }
        public virtual string ExpungeNote { get; set; }
        public virtual int Expungedby { get; set; }
        public virtual DateTime? Dateexpunged { get; set; }
        public virtual int Createdby { get; set; }
        public virtual DateTime? Datereceived { get; set; }
        public virtual string BranchID { get; set; }

        //spouse information
        public virtual byte[] S_Imgraw { get; set; }
        public virtual string s_Surname { get; set; }
        public virtual string s_Othernames { get; set; }
        public virtual DateTime s_Dob { get; set; }
        public virtual int S_Sex { get; set; }
        public virtual string S_mobile { get; set; }
        public virtual string S_email { get; set; }
        public virtual string S_hospital { get; set; }
        public virtual string S_medicalhistory { get; set; }

        //child1 information
        public virtual byte[] child1_Imgraw { get; set; }
        public virtual string child1_Surname { get; set; }
        public virtual string child1_Othernames { get; set; }
        public virtual DateTime child1_Dob { get; set; }
        public virtual int child1_Sex { get; set; }
        public virtual string child1_mobile { get; set; }
        public virtual string child1_email { get; set; }
        public virtual string child1_hospital { get; set; }
        public virtual string child1_medicalhistory { get; set; }

        //child2 information
        public virtual byte[] child2_Imgraw { get; set; }
        public virtual string child2_Surname { get; set; }
        public virtual string child2_Othernames { get; set; }
        public virtual DateTime child2_Dob { get; set; }
        public virtual int child2_Sex { get; set; }
        public virtual string child2_mobile { get; set; }
        public virtual string child2_email { get; set; }
        public virtual string child2_hospital { get; set; }
        public virtual string child2_medicalhistory { get; set; }

        //child3 information
        public virtual byte[] child3_Imgraw { get; set; }
        public virtual string child3_Surname { get; set; }
        public virtual string child3_Othernames { get; set; }
        public virtual DateTime child3_Dob { get; set; }
        public virtual int child3_Sex { get; set; }
        public virtual string child3_mobile { get; set; }
        public virtual string child3_email { get; set; }
        public virtual string child3_hospital { get; set; }
        public virtual string child3_medicalhistory { get; set; }

        //child3 information
        public virtual byte[] child4_Imgraw { get; set; }
        public virtual string child4_Surname { get; set; }
        public virtual string child4_Othernames { get; set; }
        public virtual DateTime child4_Dob { get; set; }
        public virtual int child4_Sex { get; set; }
        public virtual string child4_mobile { get; set; }
        public virtual string child4_email { get; set; }
        public virtual string child4_hospital { get; set; }
        public virtual string child4_medicalhistory { get; set; }


        public virtual bool addspouse { get; set; }
        public virtual bool addchild1 { get; set; }
        public virtual bool addchild2 { get; set; }
        public virtual bool addchild3 { get; set; }
        public virtual bool addchild4 { get; set; }
    }
}