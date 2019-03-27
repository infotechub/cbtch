using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Models.Provider;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Services
{
    public interface ICompanyService
    {
        IList<Company> GetallCompany();

        IList<Company> GetallCompanyWithOutAPlan();
        IList<Company> GetallCompanyforJson();
        bool AddnewCompany(Company company);
        bool DeleteCompany(Company company);
        bool UpdateCompany(Company company);
        Company GetCompany(int id);
        Company GetCompanyByName(string name);
        bool AddnewCategory(BenefitsCategory category);
        BenefitsCategory GetCategory(int id);
        bool DeleteCategory(BenefitsCategory category);
        IList<BenefitsCategory> GetallBenefitCategory();

        //Benefit 
        bool AddnewBenefit(Benefit drug);
        Benefit GetBenefit(int id);
        bool DeleteBenefit(Benefit benefit);
        IList<Benefit> Getallbenefit();
        bool UpdateBenefit(Benefit benefit);


        //Company Plan
        bool AddCompanyPlan(CompanyPlan plan);
        CompanyPlan GetCompanyPlan(int id);
        bool DeleteCompanyPlan(CompanyPlan plan);
        IList<CompanyPlan> Getallplan();
        IList<CompanyPlan> Queryallplan(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcPlanName, string scrPlanDesc, string scrCompany, bool useDate, DateTime scrFromDate, DateTime scrToDate, string scrUsers, int plantype);
        IList<CompanyPlan> GetallplanForCompany(int id);

        bool UpdateCompanyPlan(CompanyPlan plan);

        //Get the bebefits of the companypla
        bool AddCompanyPlanBenefit(CompanyBenefit plan);
        IList<CompanyBenefit> GetallCompanyPlanBenefits();
        IList<CompanyBenefit> GetCompanyPlanBenefits(int planid);
        bool DeleteCompanyPlanBenefit(CompanyBenefit benefit);
        CompanyBenefit GetCompanyPlanBenefit(int benefitid);
        bool UpdateCompanyPlanBenefit(CompanyBenefit benefit);
        long CompanyCount();

        //Get the bebefits of the companypla
        bool AddPlanBenefit(PlanDefaultBenefit plan);
        IList<PlanDefaultBenefit> GetallPlanBenefits();
        IList<PlanDefaultBenefit> GetPlanBenefits(int planid);
        bool DeletePlanBenefit(PlanDefaultBenefit benefit);
        PlanDefaultBenefit GetPlanBenefit(int benefitid);
        bool UpdatePlanBenefit(PlanDefaultBenefit benefit);

        //Staff List
        bool AddStaff(Staff staff);
        IList<Staff> GetAllStaff();
        IList<Staff> GetAllStaffinCompanySubsidiary(int companyId);
        IList<StaffnameandPlan> GetAllStaffinCompanyLite(int subsidiaryId, int mode);
        IList<StaffnameandPlan> GetAllStaffinCompanySubsidiaryLite(int subsidiaryId, int mode);
        IList<StaffnameandPlan> GetAllStaffinCompanyLite(int CompanyId);


        IList<Staff> GetAllStaffinCompany(int companyId);
        IList<Staff> QueryAllStaff(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcStaffName, int company, int subsidiary, int plantype, int user, bool userdate, DateTime fromdate, DateTime todate, int profilestatus, int expunged);
        IList<Staff> QueryFidelityComplete(int company, int subsidiary);
        bool CheckStaffProfileStatus(int staff);

        bool DeleteStaff(Staff staff);
        Staff Getstaff(int id);

        Staff GetstaffByCompanyStaffId(string staffid);
        bool UpdateStaff(Staff staff);
        bool CheckifStaffExistwithName(string stafffullname, int companyId, out int id, int subsidiary, out List<string> ids);

        //Company Subscription
        bool AddSubscription(Subscription subscription);
        Subscription GetSubscription(int id);
        bool DeleteSubscription(Subscription subscription);
        IList<Subscription> GetallSubscription();
        IList<Subscription> GetSubscriptionExpiringSoon();
        IList<Subscription> GetexpiredSubscriptions();
        bool UpdateSubscription(Subscription subscription);
        IList<Subscription> GetNewlyApprovedActiveSubscription();
        IList<Subscription> GetAllActiveSubscription();
        Subscription GetSubscriptionByPlan(int id);
        Subscription GetSubscriptionByPlan(int id, int subsidiary);
        bool DisableEnrolleeUnderCompanyPlan(int id);
        bool EnableEnrolleeUnderCompanyPlan(int id);
        bool EnableEnrolleeSubscription(int staffid, int subid);
        bool ExecuteSubscriptionCheck();
        bool DisableEnrolleeSubscription(int staffid);

        bool checkifSubsidiaryhasSubscrirption(int subsidiaryid);
        bool checkifCompanyHasSubscription(int company);

        //Subsidiary List
        bool AddSubsidiary(CompanySubsidiary subsidiary);
        IList<CompanySubsidiary> GetAllSubsidiary();
        bool DeleteSubsidiary(CompanySubsidiary subsidiary);
        CompanySubsidiary Getsubsidiary(int id);
        bool Updatesubsidiary(CompanySubsidiary subsidiary);

        IList<CompanySubsidiary> GetAllSubsidiaryofACompany(int companyId);
        //AutomaticDeletetionList
        //Subsidiary List
        bool AddAutomaticDeletion(AutomaticExpungeStaff staff);
        IList<AutomaticExpungeStaff> GetAllAutomaticExpungeStaff(int subsidiary);
        bool DeleteAutomaticExpungeStaff(int staffId);
        AutomaticExpungeStaff AutomaticExpungeStaff(int staffid);
        bool UpdateAutomaticExpungeStaff(AutomaticExpungeStaff staff);
        IList<AutomaticExpungeStaff> QueryAllAutomaticExpungeStaff(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcStaffName);


        //bulk staff upload page
        //Company Plan
        bool AddStaffJob(StaffUploadJob Job);
        StaffUploadJob GetStaffUploadJob(int id);
        bool DeleteStaffUpload(StaffUploadJob Job);
        IList<StaffUploadJob> QueryStaffUploadJobs(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string startDate, string EndDate, int uploadedby);
        bool UpdateStaffUploadJobs(StaffUploadJob job);

        bool ExpundgeAllStaffInCompany(int CompanyId, int jobid);

        IDictionary<int, string> GetAllStaffInCompany(int companyid);

        IList<StaffUploadJob> QueryPendingJobs();
    }
}