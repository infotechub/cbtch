using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MrCMS.Entities.People;
using MrCMS.Logging;
using MrCMS.Web.Apps.Core.Entities;
using Elmah;
using MrCMS.Web.Apps.Core.Utility;
using OfficeOpenXml;
using static MrCMS.Web.Apps.Core.Utility.Tools;

namespace MrCMS.Web.Apps.Core.Services
{
    public interface IEnrolleeService
    {
        bool ValidatePolicyNumber(string policy);

        IList<Enrollee> Getallenrollee();
        IList<Enrollee> GetallenrolleeRange(out int totalRecord, int start, int lenght);

        IList<Enrollee> QueryAllenrollee(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcPolicynumber, string scrOthername, string scrLastname, string scrMobilenumber, string scrProvider, string scrCompany, string scrCompanySubsidiary, bool useDate, DateTime scrFromDate, DateTime scrToDate, int showExpunge, string scrUsers, int enrolleetype, int otherFilters, int zones, int state, int plantype, int planmode);
        bool AddEnrollee(Enrollee enrollee, byte[] imgData);
        bool AddTempEnrollee(TempEnrollee enrollee);

        long EnrolleeCount();

        IList<EnrolleeVerificationCode> QueryVerificationCode(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcPolicynumber, int providerid, string verificationcode, string scrMobilenumber, bool useDate, DateTime scrFromDate, DateTime scrToDate, evsvisittype visittype);

        bool DeletEnrollee(Enrollee enrollee);
        bool UpdateEnrollee(Enrollee enrolee);
        Enrollee GetEnrollee(int id);
        Enrollee GetEnrolleeGuid(string Guid);
        TempEnrollee GetTempEnrollee(int id);
        TempEnrollee GetTempEnrolleeByMobileorEmail(string MobileorEmail);
        TempEnrollee GetTempEnrolleebystaffid(string staffid);
        TempEnrollee GetTempEnrolleebystaffProfileid(int staffid);
        IList<Enrollee> GetEnrolleesByStaffId(int id);
        Enrollee GetEnrolleeByPolicyNumber(string policyNumber);
        Enrollee GetEnrolleeByReferencePolicyNumber(string policyNumber);
        IList<Enrollee> GetFamilyTreeByPolicyNumber(string policyNumber);
        bool CheckEnrolleePhoneNumber(string phonenumber, string enrolleePolicyNumber);
        IList<Enrollee> GetDependentsEnrollee(int principalid);
        IList<Enrollee> GetEnrolleeunderPlan(int planid);
        EnrolleePassport GetEnrolleepassport(int enrolleeid);
        bool UpdateEnrollee(EnrolleePassport enroleepassport);
        IList<Enrollee> GetEnrolleeCelebratingBirthday();


        IList<MobileEnrolleeTied> GetEnrolleesTiedToPhone(string phone);

        bool AddMobileSignup(MobileSignup signup);

        bool AddShortMessage(ShortCodeMsg shortCodeMsg);
        bool UpdateShortMessage(ShortCodeMsg shortCodeMsg);
        ShortCodeMsg GetShortMessage(int id);
        IList<ShortCodeMsg> GetShortMessages();
        IList<ShortCodeMsg> GetUnProccessedShortMessages();

        IList<EnrolleeVerificationCode> GetPreviousDayAuthenticatedCodes();
        string GetConcatenatedPhoneNumberString(SMSLeads Mode);
        IList<Enrollee> GetEnrolleesbyPhone(string mobile);
        IList<Enrollee> GetEnrolleesbyEmail(string email);
        IList<EnrolleePolicyName> GetEnrolleePolicyNumberName(string phrase);


        bool AddTempDependant(PendingDependant dependant);
        IList<PendingDependant> getalltempDependant();
        PendingDependant getTempDependant(int Id);
        IList<PendingDependant> getTempDependant(string parentguid);
        bool updateTempDependant(PendingDependant Dep);
        object QueryAllenrollee(out int toltareccount, out int totalinresult, string search, int v1, int v2, string sortColumnnumber, string sortOrder, string scrPolicynumber, string scrOthername, string scrLastname, string scrMobilenumber, string scrProvider, string scrCompany, bool usedate, DateTime fromdate, DateTime todate, int showexpunge, string scr_user);
    }
}