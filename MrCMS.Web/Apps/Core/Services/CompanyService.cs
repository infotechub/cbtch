using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Antlr.Runtime.Misc;
using MrCMS.Entities.People;
using MrCMS.Logging;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Models.Provider;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Website;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Helpers;
using System.Globalization;
using System.Text.RegularExpressions;
using FluentNHibernate.Utils;
using Microsoft.Ajax.Utilities;
using NHibernate.Proxy;
using MrCMS.Settings;
using StackExchange.Profiling.Helpers.Dapper;
using System.Text;
namespace MrCMS.Web.Apps.Core.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ISession _session;
        private readonly IUserService _userService;
        private readonly IHelperService _helpersvc;
        private readonly IServicesService _servicevc;
        private readonly IPlanService _plansvc;
        private readonly IUniquePageService _uniquePageService;
        private readonly ITariffService _tariffService;
        private readonly IRoleService _roleSvc;
        private readonly TextInfo _myTi = new CultureInfo("en-US", false).TextInfo;

        private readonly ISystemConfigurationProvider _systemConfigurationProvider;
        //private readonly INotificationHubService _notificationHubService;
        public CompanyService(ISession session, IUserService userservice, IHelperService helpersvc, IServicesService servicesvc, IPlanService plansvc, IUniquePageService uniquepage, ITariffService tariffservice, IRoleService roleService, ISystemConfigurationProvider systemConfigurationProvider)
        {
            _session = session;
            _userService = userservice;
            _helpersvc = helpersvc;
            _servicevc = servicesvc;
            _plansvc = plansvc;
            _uniquePageService = uniquepage;
            _tariffService = tariffservice;
            _roleSvc = roleService;
            _systemConfigurationProvider = systemConfigurationProvider;
            //_notificationHubService = notificationHub;
        }

        public IList<Company> GetallCompany()
        {
            return _session.QueryOver<Company>().Where(x => x.IsDeleted == false).List<Company>();
        }

        public IList<Company> GetallCompanyWithOutAPlan()
        {



            IList<Company> company = _session.QueryOver<Company>().Where(x => x.IsDeleted == false).List<Company>();

            List<Company> listt = new List<Company>();
            foreach (Company companie in company)
            {
                bool exist = _session.QueryOver<CompanyPlan>().Where(x => x.IsDeleted == false && x.Companyid == companie.Id).List<CompanyPlan>().Any();
                if (!exist)
                {
                    listt.Add(companie);
                }
            }
            return listt;
        }

        public IList<Company> GetallCompanyforJson()
        {
            return _session.QueryOver<Company>().Where(x => x.IsDeleted == false).List<Company>();
        }

        public bool AddnewCompany(Company company)
        {
            if (company != null)
            {
                company.Status = true;

                _session.Transact(session => session.Save(company));


                CompanySubsidiary sub = Getsubsidiary(company.Subsidiary.Id);
                sub.ParentcompanyId = company.Id;

                Updatesubsidiary(sub);
                //_helpersvc.Log(LogEntryType.Audit, null,
                //              string.Format(
                //                  "New company has been added to the system company name {0} , provider id {1}, by {2}",
                //                  company.Name, company.Id,CurrentRequestData.CurrentUser.Id.ToString( )), "Company Added.");

                return true;
            }
            return false;
        }

        public bool DeleteCompany(Company company)
        {
            if (company != null)
            {
                company.IsDeleted = true;
                _session.Transact(session => session.Update(company));

                _helpersvc.Log(LogEntryType.Audit, null, string.Format(
                     "Company has been deleted company name {0} , company id {1}, by {2}",

                    company.Name, company.Id, CurrentRequestData.CurrentUser.Id), "Company deleted.");
                return true;
            }
            return false;
        }

        public bool UpdateCompany(Company company)
        {
            if (company != null)
            {

                _session.Transact(session => session.Update(company));



                _helpersvc.Log(LogEntryType.Audit, null, string.Format(
                     "Company has been updated company name {0} , company id {1}, by {2}",

                    company.Name, company.Id, CurrentRequestData.CurrentUser.Id), "Company updated.");
                return true;
            }
            return false;
        }

        public Company GetCompany(int id)
        {

            Company company = _session.QueryOver<Company>().Where(x => x.Id == id).SingleOrDefault();
            return company;
        }

        public Company GetCompanyByName(string name)
        {
            Company company = _session.QueryOver<Company>().Where(x => x.Name == name).SingleOrDefault();
            return company;
        }

        public bool AddnewCategory(BenefitsCategory category)
        {
            if (category != null)
            {
                category.Name = category.Name.ToUpper();
                category.Status = true;
                category.Code = category.Name.ToLower();
                category.Servicetype = "";
                _session.Transact(session => session.Save(category));
                _helpersvc.Log(LogEntryType.Audit, null,
                               string.Format(
                                   "New Benefit category has been added to the system category name {0} , Category id {1}, by {2}",
                                   category.Name, category.Id, CurrentRequestData.CurrentUser.Id), "category Added.");
                return true;
            }
            return false;
        }

        public BenefitsCategory GetCategory(int id)
        {
            BenefitsCategory benefitcategory = _session.QueryOver<BenefitsCategory>().Where(x => x.Id == id).SingleOrDefault();
            return benefitcategory;
        }

        public bool DeleteCategory(BenefitsCategory category)
        {
            if (category != null)
            {
                category.IsDeleted = true;
                _session.Transact(session => session.Update(category));
                return true;
            }
            return false;

        }

        public IList<BenefitsCategory> GetallBenefitCategory()
        {
            return _session.QueryOver<BenefitsCategory>().Where(x => x.IsDeleted == false).List<BenefitsCategory>();
        }

        public bool AddnewBenefit(Benefit benefit)
        {
            if (benefit != null)
            {
                _session.Transact(session => session.Save(benefit));
                return true;
            }
            else
            {
                return false;
            }
        }

        public Benefit GetBenefit(int id)
        {
            Benefit benefit = _session.QueryOver<Benefit>().Where(x => x.Id == id).SingleOrDefault();
            return benefit;
        }

        public bool DeleteBenefit(Benefit benefit)
        {
            if (benefit != null)
            {
                benefit.IsDeleted = true;
                _session.Transact(session => session.Update(benefit));
                return true;
            }
            return false;
        }

        public IList<Benefit> Getallbenefit()
        {
            return _session.QueryOver<Benefit>().Where(x => x.IsDeleted == false).List<Benefit>();
        }

        public bool UpdateBenefit(Benefit benefit)
        {
            if (benefit != null)
            {

                _session.Transact(session => session.Update(benefit));
                return true;
            }
            return false;
        }

        public bool AddCompanyPlan(CompanyPlan plan)
        {
            if (plan != null)
            {

                _session.Transact(session => session.Save(plan));


                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "New company plan has been added to the system, company plan name {0} , Plan id {1}, by {2}",
                                  plan.Planfriendlyname, plan.Id, CurrentRequestData.CurrentUser.Id.ToString()), "Company plan Added.");

                return true;
            }
            return false;
        }

        public CompanyPlan GetCompanyPlan(int id)
        {
            CompanyPlan companyPlan = _session.QueryOver<CompanyPlan>().Where(x => x.Id == id).SingleOrDefault();
            return companyPlan;
        }

        public bool DeleteCompanyPlan(CompanyPlan plan)
        {
            if (plan != null)
            {
                plan.IsDeleted = true;
                _session.Transact(session => session.Update(plan));
                return true;
            }
            return false;
        }

        public IList<CompanyPlan> Getallplan()
        {
            return _session.QueryOver<CompanyPlan>().Where(x => x.IsDeleted == false).List<CompanyPlan>();
        }

        public IList<CompanyPlan> Queryallplan(out int totalRecord, out int totalcountinresult, string search, int start, int lenght,
            string sortColumn, string sortOrder, string srcPlanName, string scrPlanDesc, string scrCompany, bool useDate,
            DateTime scrFromDate, DateTime scrToDate, string scrUsers, int plantype)
        {
            IQueryOver<CompanyPlan, CompanyPlan> query = _session.QueryOver<CompanyPlan>().Where(x => x.IsDeleted == false);




            if (!string.IsNullOrEmpty(srcPlanName))
            {
                //search policy number



                srcPlanName = "%" + srcPlanName + "%";


                query = query.Where(Restrictions.On<CompanyPlan>(x => x.Planfriendlyname).IsInsensitiveLike(srcPlanName));



            }


            if (!string.IsNullOrEmpty(scrPlanDesc))
            {

                scrPlanDesc = "%" + scrPlanDesc + "%";
                query = query.Where(Restrictions.On<CompanyPlan>(x => x.Description).IsInsensitiveLike(scrPlanDesc));

            }






            if (!string.IsNullOrEmpty(scrCompany) && Convert.ToInt32(scrCompany) > -1)
            {
                int company = Convert.ToInt32(scrCompany);
                query = query.Where(x => x.Companyid == company);
            }


            if (useDate)
            {

                DateTime datete = Convert.ToDateTime(scrToDate);

                int dd = datete.Day;
                int month = datete.Month;
                int year = datete.Year;

                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", dd, month, year, time));
                query.Where(Restrictions.On<CompanyPlan>(a => a.CreatedOn).IsBetween(scrFromDate).And(enddate));
            }
            if (!string.IsNullOrEmpty(scrUsers))
            {
                int toint = Convert.ToInt32(scrUsers);

                if (toint > 0)
                {
                    query.Where(x => x.Createdby == toint);
                }

            }


            if (plantype > 0)
            {
                query.Where(x => x.Planid == plantype);
            }


            if (!string.IsNullOrEmpty(scrUsers) && Convert.ToInt32(scrUsers) > 0)
            {
                int intstr = Convert.ToInt32(scrUsers);

                query.Where(x => x.Createdby == intstr);
            }

            //sort order

            //return normal list.
            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            return query.OrderBy(x => x.CreatedOn).Desc.Skip(start).Take(lenght).List();
        }

        public IList<CompanyPlan> GetallplanForCompany(int id)
        {
            return _session.QueryOver<CompanyPlan>().Where(x => x.IsDeleted == false && x.Companyid == id).List<CompanyPlan>();
        }

        public bool UpdateCompanyPlan(CompanyPlan plan)
        {
            if (plan != null)
            {
                _session.Transact(session => session.Update(plan));
                return true;
            }
            return false;
        }



        public bool AddCompanyPlanBenefit(CompanyBenefit benefit)
        {
            if (benefit != null)
            {

                _session.Transact(session => session.Save(benefit));
                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "New company plan benefit has been added. {0} , Plan id {1}, by {2}",
                                benefit.CompanyPlanid, benefit.Id, CurrentRequestData.CurrentUser.Id.ToString()), "Company plan benefit  Added to plan.");

                return true;
            }
            return false;
        }

        public IList<CompanyBenefit> GetallCompanyPlanBenefits()
        {
            return _session.QueryOver<CompanyBenefit>().Where(x => x.IsDeleted == false).List<CompanyBenefit>();
        }

        public IList<CompanyBenefit> GetCompanyPlanBenefits(int planid)
        {
            return _session.QueryOver<CompanyBenefit>().Where(x => x.IsDeleted == false && x.CompanyPlanid == planid).List<CompanyBenefit>();
        }

        public bool DeleteCompanyPlanBenefit(CompanyBenefit benefit)
        {
            if (benefit != null)
            {

                _session.Transact(session => session.Delete(benefit));
                return true;
            }
            return false;
        }

        public CompanyBenefit GetCompanyPlanBenefit(int benefitid)
        {
            CompanyBenefit companyBenefit = _session.QueryOver<CompanyBenefit>().Where(x => x.Id == benefitid).SingleOrDefault();
            return companyBenefit;
        }

        public bool UpdateCompanyPlanBenefit(CompanyBenefit benefit)
        {
            if (benefit != null)
            {

                _session.Transact(session => session.Update(benefit));
                return true;
            }
            return false;
        }

        public bool AddPlanBenefit(PlanDefaultBenefit plan)
        {
            if (plan != null)
            {

                _session.Transact(session => session.Save(plan));
                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "New  plan default  benefit has been added. {0} , Plan id {1}, benefit id {2}",
                               plan.Planid, plan.BenefitId, CurrentRequestData.CurrentUser.Id.ToString()), "plan benefit  Added to plan.");

                return true;
            }
            return false; ;
        }

        public IList<PlanDefaultBenefit> GetallPlanBenefits()
        {
            return _session.QueryOver<PlanDefaultBenefit>().Where(x => x.IsDeleted == false).List<PlanDefaultBenefit>();
        }

        public IList<PlanDefaultBenefit> GetPlanBenefits(int planid)
        {
            return _session.QueryOver<PlanDefaultBenefit>().Where(x => x.IsDeleted == false && x.Planid == planid).List<PlanDefaultBenefit>();
        }

        public bool DeletePlanBenefit(PlanDefaultBenefit benefit)
        {
            if (benefit != null)
            {

                _session.Transact(session => session.Delete(benefit));
                return true;
            }
            return false;
        }

        public PlanDefaultBenefit GetPlanBenefit(int benefitid)
        {
            PlanDefaultBenefit benefit = _session.QueryOver<PlanDefaultBenefit>().Where(x => x.Id == benefitid).SingleOrDefault();
            return benefit;
        }


        public bool UpdatePlanBenefit(PlanDefaultBenefit benefit)
        {
            if (benefit != null)
            {

                _session.Transact(session => session.Update(benefit));
                return true;
            }
            return false;
        }

        public bool AddStaff(Staff staff)
        {

            if (staff != null)
            {
                staff.StaffFullname = _myTi.ToTitleCase(staff.StaffFullname);
                _session.Transact(session => session.Save(staff));
                User currrentuser = CurrentRequestData.CurrentUser;
                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "New staff was added to company staff list.  ,Staff id {0}   , company id {1} by {2}",
                               staff.Id, staff.CompanyId, currrentuser != null ? currrentuser.Id.ToString() : "1"), "Staff added to company staff list.");


                // send notification to those responsible
                //only send notifications if they where added later than 10 mins 
                //Get notification for new Staff
                NotificationTable notification = _helpersvc.GetNotificationTable(3);
                //send notification for each guy in the role to notify that a new staff was added.
                if (notification != null)
                {

                    //_helpersvc.PushUserNotification(string.Empty, String.Format("New staff[{1}] was added for {0} ", GetCompany(Convert.ToInt32(staff.CompanyId)).Name, staff.StaffFullname),
                    //                                           NotificationTarget.Role, notification.Roles,
                    //                                           NotificationType.Persistent, UniquePageHelper.GetUrl<Pages.StaffListPage>());

                }




                return true;
            }
            return false; ;
        }

        public IList<Staff> GetAllStaff()
        {
            return _session.QueryOver<Staff>().Where(x => x.IsDeleted == false).List<Staff>();
        }

        public IList<Staff> GetAllStaffinCompanySubsidiary(int companyId)
        {
            string companyidstr = Convert.ToString(companyId);
            return _session.QueryOver<Staff>().Where(x => x.IsDeleted == false && x.IsExpunged == false && x.CompanySubsidiary == companyId).List<Staff>();
        }

        public IList<Staff> GetAllStaffinCompany(int companyId)
        {
            string companyidstr = Convert.ToString(companyId);
            return _session.QueryOver<Staff>().Where(x => x.IsDeleted == false && x.CompanyId == companyidstr && x.IsExpunged == false).List<Staff>();
        }

        public IList<Staff> QueryAllStaff(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcStaffName, int company, int subsidiary, int plantype, int user, bool userdate, DateTime fromdate, DateTime todate, int profilestatus, int expunged)
        {
            //i reinvented the wheel by using HQL i had so much time in my hands.

            //var query = _session.QueryOver<Staff>().Where(x => x.IsDeleted == false);
            string QueryAll = @"select a from Staff a where  a.IsDeleted=0 ";
            string queryCountString = @"select count(*) from Staff a where  a.IsDeleted=0 ";
            StringBuilder queryString = new StringBuilder();

            //var query = _session.CreateQuery(queryString.ToString());
            if (subsidiary > 0)
            {
                //var companysub = Convert.ToInt32(subsidiary);
                //var stafflist = (List<int>)_session.QueryOver<Staff>().Where(x => x.IsDeleted == false && x.CompanySubsidiary == companysub).SelectList(a => a.Select(p => p.Id)).List<int>();
                //CompanySubsidiary companysubalias = null;
                //query.JoinAlias(p => p.CompanySubsidiary, () => companysubalias);

                queryString.Append(string.Format("and a.CompanySubsidiary = {0} ", subsidiary));

                //query = query.WhereRestrictionOn(w => w.Id).IsIn(stafflist);

                //var staff = _session.CreateQuery(queryString.ToString());
                //var all = staff.List();
            }




            if (!string.IsNullOrEmpty(srcStaffName))
            {
                //search policy number

                srcStaffName = "%" + srcStaffName + "%";
                queryString.Append(string.Format("and a.StaffFullname like '{0}' ", srcStaffName));


            }
            if (userdate)
            {

                DateTime datete = Convert.ToDateTime(todate);

                int dd = datete.Day;
                int month = datete.Month;
                int year = datete.Year;

                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", month, dd, year, time));
                queryString.Append(string.Format("and a.CreatedOn is between '{0}' and '{1}' ", fromdate.ToString("yyyy/MM/dd hh:mm"), enddate.ToString("yyyy/MM/dd hh:mm")));

                //query.Where(Restrictions.On<Staff>(a => a.CreatedOn).IsBetween(fromdate).And(enddate));
            }
            if (expunged > 0)
            {
                int status = 0;

                if (expunged == 2)
                {
                    status = 1;


                }
                queryString.Append(string.Format("and a.IsExpunged = '{0}' ", status));
                //query.Where(x => x.IsExpunged == status);

            }
            if (company > 0)
            {
                //query.Where(x => x.CompanyId == company.ToString());

                queryString.Append(string.Format("and a.CompanyId = '{0}' ", company));
            }

            if (user > 0)
            {
                int toint = user;

                if (toint > 0)
                {

                    queryString.Append(string.Format("and a.Createdby = {0} ", toint));
                    //query.Where(x => x.Createdby == toint);
                }

            }


            //if (plantype > 0)
            //{
            //    query.Where(x => x.StaffPlanid == plantype);
            //}


            if (profilestatus > -1)
            {
                switch (profilestatus)
                {
                    case 0:
                        break;
                    case 1:
                        queryString.Append(string.Format("and a.HasProfile = {0} ", 1));
                        break;
                    case 2:
                        queryString.Append(string.Format("and a.HasProfile = {0} ", 0));
                        break;


                }
            }

            IQuery result = _session.CreateQuery(QueryAll + queryString.ToString());
            long count = (long)_session.CreateQuery(queryCountString + queryString.ToString()).UniqueResult();
            //return normal list.
            totalRecord = Convert.ToInt32(count); //query.RowCount();

            //query.Skip(start).Take(lenght).List()
            totalcountinresult = totalRecord;
            IList<Staff> response = result.SetFirstResult(start).SetMaxResults(lenght).List<Staff>();
            return response;
        }

        public bool DeleteStaff(Staff staff)
        {
            if (staff != null)
            {
                _session.Transact(session => session.Delete(staff));
                return true;
            }
            return false;
        }

        public Staff Getstaff(int id)
        {
            Staff staff = _session.QueryOver<Staff>().Where(x => x.Id == id).SingleOrDefault();
            return staff;
        }

        public bool UpdateStaff(Staff staff)
        {
            if (staff != null)
            {
                _session.Transact(session => session.Update(staff));

                IList<Enrollee> enrollees = _session.QueryOver<Enrollee>().Where(x => x.Staffprofileid == staff.Id).List();

                foreach (Enrollee enr in enrollees)
                {
                    enr.Companyid = Convert.ToInt32(staff.CompanyId);
                    enr.Subscriptionplanid = staff.StaffPlanid;

                    _session.Transact(session => session.Update(enr));

                }


                return true;
            }
            return false;
        }

        public bool CheckifStaffExistwithName(string stafffullname, int companyId, out int id, int subsidiary,
            out List<string> ids)
        {
            float percent = 0;
            string fullname = string.Empty;
            Staff thestaff = new Staff();
            List<string> thestafflist = new List<string>();
            string thename = stafffullname;
            stafffullname = Regex.Replace(stafffullname.Trim(), @"\s+", ":");
            string[] namelist = stafffullname.Split(':');
            IQueryOver<Staff, Staff> query =
                _session.QueryOver<Staff>()
                    .Where(
                        x => x.IsDeleted == false && x.IsExpunged == false && x.HasProfile && x.CompanyId == Convert.ToString(companyId));

            if (subsidiary > 0)
            {
                query =
                    _session.QueryOver<Staff>()
                        .Where(
                            x =>
                                x.IsDeleted == false && x.HasProfile && x.IsExpunged == false &&
                                x.CompanyId == Convert.ToString(companyId) && x.CompanySubsidiary == subsidiary);


            }

            if (namelist.Any())
            {
                IQueryOver<Staff, Staff> oldquery;

                foreach (string name in namelist)
                {

                    if (string.IsNullOrEmpty(name.Trim()))
                    {
                        continue;
                    }

                    oldquery = query.Clone();
                    //oldquery = query;
                    query =
                        query.Where(
                            Restrictions.On<Staff>(x => x.StaffFullname)
                                .IsInsensitiveLike("%" + name.Trim() + "%", MatchMode.Anywhere));

                    int count = query.RowCount();

                    if (count > 0)
                    {
                        //do the search

                        foreach (Staff item in query.List())
                        {
                            if (Tools.compareNames(thename, item.StaffFullname) > 50)
                            {
                                id = item.Id;
                                ids = null;
                                return true;

                            }
                        }

                    }
                    else
                    {
                        query = oldquery.Clone();
                    }



                }

            }

            id = 0;
            ids = null;
            return false;

        }


        public bool AddSubscription(Subscription subscription)
        {
            if (subscription != null)
            {

                _session.Transact(session => session.Save(subscription));
                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "New subscription has been added. {0} , Subsidiary {3} ,Company id {1} , by {2}",
                               subscription.Id, GetCompany(subscription.CompanyId).Name.ToUpper(), CurrentRequestData.CurrentUser.Id.ToString(), subscription.Subsidiary != null ? subscription.Subsidiary.Subsidaryname.ToUpper() : "--"), "Subscription has been added.");
                NotificationTable notification = _helpersvc.GetNotificationTable(4);
                //send notification for each guy in the role to notify that a new staff was added.

                //_helpersvc.PushUserNotification(string.Empty, String.Format("New Subscription[{1}] was added for {0} ", GetCompany(Convert.ToInt32(subscription.CompanyId)).Name, subscription.SubscriptionCode),
                //                                           NotificationTarget.Role, notification.Roles,
                //                                           NotificationType.Persistent, UniquePageHelper.GetUrl<Pages.SubscriptionPage>());




                return true;
            }
            return false;
        }

        public Subscription GetSubscription(int id)
        {
            Subscription staff = _session.QueryOver<Subscription>().Where(x => x.Id == id).SingleOrDefault();
            return staff;
        }

        public bool DeleteSubscription(Subscription subscription)
        {
            if (subscription != null)
            {
                _session.Transact(session => session.Delete(subscription));
                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "Subscription has been deleted. {0} , Company id {1}, by {2}",
                               subscription.Id, subscription.CompanyId, CurrentRequestData.CurrentUser.Id.ToString()), "Subscription has been deleted.");
                return true;
            }
            return false;
        }

        public IList<Subscription> GetallSubscription()
        {
            return _session.QueryOver<Subscription>().Where(x => x.IsDeleted == false).List<Subscription>();
        }

        public IList<Subscription> GetSubscriptionExpiringSoon()
        {

            DateTime startdate = CurrentRequestData.Now.AddDays(-3);
            DateTime enddate = CurrentRequestData.Now.AddDays(1);
            return _session.QueryOver<Subscription>().Where(x => x.IsDeleted == false && x.Status != (int)SubscriptionStatus.Expired).Where(Restrictions.On<Subscription>(a => a.Expirationdate).IsBetween(startdate).And(enddate)).List<Subscription>();
            //Restrictions.Lt(Projections.Property<Subscription>(x => x.Expirationdate),
            // CurrentRequestData.Now)

        }

        public bool UpdateSubscription(Subscription subscription)
        {
            if (subscription != null)
            {

                int currenid = 0;
                if (CurrentRequestData.CurrentUser != null)
                {
                    currenid = CurrentRequestData.CurrentUser.Id;
                }
                _session.Transact(session => session.Update(subscription));
                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "Subscription has been updated. {0} , Company id {1}, by {2}",
                               subscription.Id, subscription.CompanyId, currenid), "Subscription has been updated.");


                return true;
            }
            return false;
        }

        public IList<Subscription> GetNewlyApprovedActiveSubscription()
        {
            IQueryOver<Subscription, Subscription> itemo =
                _session.QueryOver<Subscription>()
                    .Where(
                        x =>
                            x.Status == (int)SubscriptionStatus.Default &&
                            x.AuthorizationStatus == (int)AuthorizationStatus.Authorized)
                    .Where(Restrictions.Gt(Projections.Property<Subscription>(x => x.Expirationdate),
                        CurrentRequestData.Now));
            return itemo.List();
        }

        public IList<Subscription> GetAllActiveSubscription()
        {
            return _session.QueryOver<Subscription>().Where(x => x.Status == (int)SubscriptionStatus.Active && x.IsDeleted == false).List();
        }

        public Subscription GetSubscriptionByPlan(int id)
        {


            IQueryOver<Subscription, Subscription> latestsub = _session.QueryOver<Subscription>().Where(
                    x =>
                    x.IsDeleted == false).Where(Restrictions.On<Subscription>(X => X.Companyplans).IsInsensitiveLike("%" + Convert.ToString(id) + "%")).OrderBy(x => x.Expirationdate).Desc;

            // &&   x.Status != (int)SubscriptionStatus.Expired

            int count = latestsub.RowCount();

            //use sorting algorith

            List<Subscription> output = new List<Subscription>();
            for (int i = 0; i < count; i++)
            {

                Subscription item = latestsub.Skip(i).Take(1).SingleOrDefault();
                string[] allplans = item.Companyplans.Split(',');
                output.AddRange(from plan in allplans where plan.Trim() == Convert.ToString(id) select item);
            }
            //check the one with subscrip tion
            return output.OrderByDescending(x => x.Expirationdate).Take(1).SingleOrDefault();
        }

        public bool DisableEnrolleeUnderCompanyPlan(int id)
        {
            //Disable all the the users under the plan

            //get all the staff under the plan
            IQueryOver<Staff, Staff> allstaffunderplan = _session.QueryOver<Staff>().Where(x => x.IsDeleted == false && x.StaffPlanid == id);


            if (allstaffunderplan.RowCount() > 0)
            {
                foreach (Staff staff in allstaffunderplan.List())
                {
                    DisableEnrolleeSubscription(staff.Id);
                }
            }

            return true;
        }

        public bool EnableEnrolleeUnderCompanyPlan(int id)
        {
            //get all the staff under the plan
            IQueryOver<Staff, Staff> allstaffunderplan = _session.QueryOver<Staff>().Where(x => x.IsDeleted == false && x.StaffPlanid == id);


            if (allstaffunderplan.RowCount() > 0)
            {
                foreach (Staff staff in allstaffunderplan.List())
                {
                    EnableEnrolleeSubscription(staff.Id, id);
                }
            }

            return true;
        }


        public bool ExecuteSubscriptionCheck()
        {
            DatabaseSettings databaseSettings = _systemConfigurationProvider.GetSystemSettings<DatabaseSettings>();

            using (SqlConnection con = new SqlConnection(databaseSettings.ConnectionString))
            {

                //Check the new subscription
                //using (SqlCommand cmd = new SqlCommand("CheckNewSubscription", con))
                //{
                //    cmd.CommandType = CommandType.StoredProcedure;
                //    con.Open();
                //    cmd.ExecuteNonQuery();
                //}

                //using (SqlCommand cmd = new SqlCommand("DoExpirationCheck", con))
                //{
                //    cmd.CommandType = CommandType.StoredProcedure;
                //    cmd.ExecuteNonQuery();
                //}
                using (SqlCommand cmd = new SqlCommand("CheckActiveSubscription", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }

            }
            return true;
        }

        public bool DisableEnrolleeSubscription(int staffid)
        {
            ITransaction tx = _session.BeginTransaction();
            string hqlVersionedUpdate = "update Enrollee set Subscriptionplanid=0,Hasactivesubscription = :newValue where Staffprofileid = :oldValue";
            int updatedEntities = _session.CreateQuery(hqlVersionedUpdate)

                    .SetBoolean("newValue", false)
                    .SetInt32("oldValue", staffid)
                    .ExecuteUpdate();
            tx.Commit();


            return true;
        }

        public bool EnableEnrolleeSubscription(int staffid, int subid)
        {



            //ITransaction tx = _session.BeginTransaction();
            //string hqlVersionedUpdate = "update Enrollee set Subscriptionplanid= :subValue ,Hasactivesubscription = :newValue where Staffprofileid = :oldValue";
            //int updatedEntities = _session.CreateQuery(hqlVersionedUpdate)
            //    .SetInt32("subValue", subid)
            //        .SetBoolean("newValue", true)
            //        .SetInt32("oldValue", staffid)
            //        .ExecuteUpdate();
            //tx.Commit();


            return true;
        }

        public bool AddSubsidiary(CompanySubsidiary subsidiary)
        {
            if (subsidiary != null)
            {

                _session.Transact(session => session.Save(subsidiary));
                _helpersvc.Log(LogEntryType.Audit, null,
                               string.Format(
                                   "New subsidiary has been added. {0} , Company id {1}, by {2}",
                                   subsidiary.Id, subsidiary.ParentcompanyId,
                                   CurrentRequestData.CurrentUser.Id.ToString()), "subsidiary has been added.");
                //var notification = _helpersvc.GetNotificationTable(4);
                ////send notification for each guy in the role to notify that a new staff was added.
                //foreach (var role in notification.Roles.Split(','))
                //{
                //    int roleint = 0;
                //    if (int.TryParse(role, out roleint))
                //    {
                //        _helpersvc.PushUserNotification(string.Empty, String.Format("New Subscription[{1}] was added for {0} ", GetCompany(Convert.ToInt32(subscription.CompanyId)).Name, subscription.SubscriptionCode),
                //                                                   NotificationTarget.Role, _roleSvc.GetRole(roleint),
                //                                                   NotificationType.Persistent, UniquePageHelper.GetUrl<Pages.SubscriptionPage>());
                //    }

                //}

                return true;
            }
            return false;
        }


        public IList<CompanySubsidiary> GetAllSubsidiary()
        {
            return _session.QueryOver<CompanySubsidiary>().Where(x => x.IsDeleted == false).List<CompanySubsidiary>();
        }

        public bool DeleteSubsidiary(CompanySubsidiary subsidiary)
        {
            if (subsidiary != null)
            {
                _session.Transact(session => session.Delete(subsidiary));
                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "subsidiary has been deleted. {0} , subsidiary id {1}, by {2}",
                               subsidiary.Id, subsidiary.ParentcompanyId, CurrentRequestData.CurrentUser.Id.ToString()), "subsidiary has been deleted.");
                return true;
            }
            return false;
        }

        public CompanySubsidiary Getsubsidiary(int id)
        {
            CompanySubsidiary subsidiary = _session.QueryOver<CompanySubsidiary>().Where(x => x.Id == id).SingleOrDefault();
            return subsidiary;
        }

        public bool Updatesubsidiary(CompanySubsidiary subsidiary)
        {
            if (subsidiary != null)
            {
                _session.Transact(session => session.Update(subsidiary));
                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "subsidiary has been updated. {0} , subsidiary id {1}, by {2}",
                               subsidiary.Id, subsidiary.ParentcompanyId, CurrentRequestData.CurrentUser.Id.ToString()), "subsidiary has been updated.");


                return true;
            }
            return false;
        }

        public IList<CompanySubsidiary> GetAllSubsidiaryofACompany(int companyId)
        {
            return _session.QueryOver<CompanySubsidiary>().Where(x => x.IsDeleted == false && x.ParentcompanyId == companyId).List<CompanySubsidiary>();
        }

        public bool AddAutomaticDeletion(AutomaticExpungeStaff staff)
        {
            if (staff != null)
            {

                AutomaticExpungeStaff staffex = AutomaticExpungeStaff(staff.StaffId);
                if (staffex == null)
                {
                    _session.Transact(session => session.Save(staff));
                    return true;
                }
                else
                {
                    return false;
                }


            }
            else
            {
                return false;
            }
        }

        public IList<AutomaticExpungeStaff> GetAllAutomaticExpungeStaff(int subsidiary)
        {
            return _session.QueryOver<AutomaticExpungeStaff>().Where(x => x.Subsidiary == subsidiary).List();
        }

        public bool DeleteAutomaticExpungeStaff(int staffId)
        {
            if (staffId > 0)
            {
                AutomaticExpungeStaff staff = _session.QueryOver<AutomaticExpungeStaff>().Where(x => x.StaffId == staffId).Take(1).SingleOrDefault();

                if (staff != null)
                {
                    _session.Transact(session => session.Delete(staff));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public AutomaticExpungeStaff AutomaticExpungeStaff(int staffid)
        {
            if (staffid > 0)
            {
                AutomaticExpungeStaff staff = _session.QueryOver<AutomaticExpungeStaff>().Where(x => x.StaffId == staffid).Take(1).SingleOrDefault();
                return staff;
            }
            else
            {
                return null;
            }
        }

        public bool UpdateAutomaticExpungeStaff(AutomaticExpungeStaff staff)
        {
            if (staff != null)
            {
                _session.Transact(session => session.Update(staff));
                return true;
            }
            else
            {
                return false;
            }
        }

        public IList<AutomaticExpungeStaff> QueryAllAutomaticExpungeStaff(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcStaffName)
        {

            IQueryOver<AutomaticExpungeStaff, AutomaticExpungeStaff> query = _session.QueryOver<AutomaticExpungeStaff>().Where(x => x.IsDeleted == false && x.Showtouser == true);




            if (!string.IsNullOrEmpty(srcStaffName))
            {
                //search policy number



                srcStaffName = "%" + srcStaffName + "%";


                //query = query.Where(Restrictions.On<AutomaticExpungeStaff>(x => x.StaffId).IsInsensitiveLike(srcStaffName));

            }



            //return normal list.
            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            return query.Skip(start).Take(lenght).List();
        }

        public bool AddStaffJob(StaffUploadJob Job)
        {

            if (Job != null)
            {
                _session.Transact(session => session.Save(Job));
                return true;

            }
            return false;

        }

        public StaffUploadJob GetStaffUploadJob(int id)
        {
            return _session.QueryOver<StaffUploadJob>().Where(x => x.Id == id).SingleOrDefault();
        }

        public bool DeleteStaffUpload(StaffUploadJob Job)
        {
            if (Job != null)
            {
                _session.Transact(session => session.Delete(Job));
                return true;
            }
            return false;
        }

        public IList<StaffUploadJob> QueryStaffUploadJobs(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string startDate, string EndDate, int uploadedby)
        {
            IQueryOver<StaffUploadJob, StaffUploadJob> query = _session.QueryOver<StaffUploadJob>().Where(x => x.IsDeleted == false);




            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(EndDate))
            {
                //search policy number



                DateTime datete = Convert.ToDateTime(EndDate);

                int dd = datete.Day;
                int month = datete.Month;
                int year = datete.Year;

                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", dd, month, year, time));

                query.Where(Restrictions.On<Enrollee>(a => a.CreatedOn).IsBetween(startDate).And(enddate));





            }

            if (uploadedby > -1)
            {
                query.Where(x => x.UploadedBy == uploadedby);
            }

            //return normal list.
            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            return query.OrderBy(x => x.CreatedOn).Desc.Skip(start).Take(lenght).List();
        }

        public bool UpdateStaffUploadJobs(StaffUploadJob job)
        {
            if (job != null)
            {
                _session.Transact(session => session.Update(job));
                return true;
            }

            return false;
        }

        public IList<StaffUploadJob> QueryPendingJobs()
        {
            IQueryOver<StaffUploadJob, StaffUploadJob> query = _session.QueryOver<StaffUploadJob>().Where(x => x.IsDeleted == false && x.JobStatus == JobStatus.Uploaded && x.approved);
            return query.List<StaffUploadJob>();
        }

        public Subscription GetSubscriptionByPlan(int id, int subsidiary)
        {
            IQueryOver<Subscription, Subscription> latestsub = _session.QueryOver<Subscription>().Where(
                    x =>
                    x.IsDeleted == false && x.Subsidiary.Id == subsidiary).Where(Restrictions.On<Subscription>(X => X.Companyplans).IsInsensitiveLike("%" + Convert.ToString(id) + "%")).OrderBy(x => x.Expirationdate).Desc;

            // &&   x.Status != (int)SubscriptionStatus.Expired

            int count = latestsub.RowCount();

            //use sorting algorith

            List<Subscription> output = new List<Subscription>();
            for (int i = 0; i < count; i++)
            {

                Subscription item = latestsub.Skip(i).Take(1).SingleOrDefault();
                string[] allplans = item.Companyplans.Split(',');
                output.AddRange(from plan in allplans where plan.Trim() == Convert.ToString(id) select item);
            }
            //check the one with subscrip tion
            return output.OrderByDescending(x => x.Expirationdate).Take(1).SingleOrDefault();
        }

        public bool checkifSubsidiaryhasSubscrirption(int subsidiaryid)
        {
            return _session.QueryOver<Subscription>().Where(x => x.IsDeleted == false && x.Subsidiary.Id == subsidiaryid).RowCount() > 0;


        }

        public bool checkifCompanyHasSubscription(int company)
        {
            return _session.QueryOver<Subscription>().Where(x => x.IsDeleted == false && x.CompanyId == company).RowCount() > 0;


        }

        public Staff GetstaffByCompanyStaffId(string staffid)
        {
            IQueryOver<Staff, Staff> query = _session.QueryOver<Staff>().Where(x => x.StaffId == staffid.ToUpper() || x.StaffId == staffid.ToLower());
            return query.Take(1).SingleOrDefault();
        }

        public IList<Staff> QueryFidelityComplete(int company, int subsidiary)
        {
            IQueryOver<Staff, Staff> query = _session.QueryOver<Staff>().Where(x => x.IsDeleted == false && x.CompanyId == Convert.ToString(company) && x.HasProfile == false);


            if (subsidiary > 0)
            {
                query.Where(x => x.CompanySubsidiary == subsidiary);

            }

            return query.List<Staff>();




        }

        public bool CheckStaffProfileStatus(int staff)
        {
            Staff staffobj = Getstaff(staff);

            if (staffobj != null)
            {
                if (staffobj.HasProfile)
                {
                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        public IDictionary<int, string> GetAllStaffInCompany(int companyid)
        {
            IQueryOver<Staff, Staff> staffs = _session.QueryOver<Staff>().Where(x => x.IsDeleted == false && x.CompanyId == Convert.ToString(companyid) && x.IsExpunged == false);
            Dictionary<int, string> output = new Dictionary<int, string>();
            foreach (Staff item in staffs.List().OrderBy(x => x.StaffFullname))
            {
                output.Add(item.Id, $"{item.StaffId} {item.StaffFullname.ToUpper()}");


            }

            return output;
        }

        public IList<Subscription> GetexpiredSubscriptions()
        {
            return _session.QueryOver<Subscription>()
                .Where(
                    x =>
                        x.IsDeleted == false && x.Status != (int)SubscriptionStatus.Expired &&
                        x.Expirationdate < CurrentRequestData.Now).List();
        }

        public IList<StaffnameandPlan> GetAllStaffinCompanySubsidiaryLite(int subsidiaryId, int mode)
        {
            string companyidstr = Convert.ToString(subsidiaryId);
            IList<Staff> bucket = _session.QueryOver<Staff>().Where(x => x.IsDeleted == false && x.CompanySubsidiary == subsidiaryId).List<Staff>();
            List<StaffnameandPlan> output = new List<StaffnameandPlan>();

            foreach (Staff item in bucket)
            {
                StaffnameandPlan obj = new StaffnameandPlan();

                obj.Id = item.Id;
                obj.Name = item.StaffFullname;
                obj.PlanId = item.StaffPlanid.ToString();
                obj.Expunged = item.IsExpunged;
                output.Add(obj);
            }

            return output;

        }

        public IList<StaffnameandPlan> GetAllStaffinCompanyLite(int subsidiaryId, int mode)
        {

            string companyidstr = Convert.ToString(subsidiaryId);
            IList<Staff> bucket = _session.QueryOver<Staff>().Where(x => x.IsDeleted == false && x.CompanySubsidiary == subsidiaryId).List<Staff>();
            List<StaffnameandPlan> output = new List<StaffnameandPlan>();
            foreach (Staff item in bucket)
            {
                CompanyPlan plan = GetCompanyPlan(item.StaffPlanid);
                StaffnameandPlan obj = new StaffnameandPlan();
                obj.Id = item.Id;
                obj.Name = item.StaffFullname;
                obj.PlanId = item.StaffPlanid.ToString();
                obj.Expunged = item.IsExpunged;
                obj.Subsidiary = item.CompanySubsidiary;
                obj.PlanName = plan != null ? plan.Planfriendlyname.ToUpper() : "";
                obj.CanAddDependants = plan != null ? plan.AllowChildEnrollee : false;
                obj.hasprofile = item.HasProfile;
                output.Add(obj);
            }

            return output;
        }

        public IList<StaffnameandPlan> GetAllStaffinCompanyLite(int CompanyId)
        {
            string companyidstr = Convert.ToString(CompanyId);
            IList<Staff> bucket = _session.QueryOver<Staff>().Where(x => x.IsDeleted == false && x.CompanyId == Convert.ToString(CompanyId)).List<Staff>();
            List<StaffnameandPlan> output = new List<StaffnameandPlan>();

            foreach (Staff item in bucket)
            {
                StaffnameandPlan obj = new StaffnameandPlan();
                List<EnrolleeModel> dependand = new List<EnrolleeModel>();

                IList<Enrollee> enrolleeeeede = _session.QueryOver<Enrollee>().Where(x => x.Staffprofileid == item.Id && x.IsDeleted == false && x.Parentid > 0)
          .List();
                foreach (Enrollee item2 in enrolleeeeede)
                {
                    EnrolleeModel itemo = new EnrolleeModel
                    {
                        Id = item2.Id,
                        Name = item2.Surname + " " + item2.Othernames,
                        IsExpunged = item2.Isexpundged,
                        PolicyNum = item2.Policynumber

                    };

                    dependand.Add(itemo);
                }

                CompanyPlan companyplan = GetCompanyPlan(item.StaffPlanid);

                obj.Id = item.Id;
                obj.Name = item.StaffFullname;
                obj.PlanId = item.StaffPlanid.ToString();
                obj.Expunged = item.IsExpunged;
                obj.Subsidiary = item.CompanySubsidiary;
                obj.PlanName = companyplan != null ? companyplan.Planfriendlyname.ToUpper() : "Unknown";
                obj.CanAddDependants = companyplan != null ? companyplan.AllowChildEnrollee : false;
                obj.DependantEnrollee = dependand;
                obj.hasprofile = item.HasProfile;
                output.Add(obj);
            }

            return output;
        }

        public long CompanyCount()
        {
            return _session.QueryOver<Company>().Where(x => x.IsDeleted == false).RowCount();
        }



        public bool ExpundgeAllStaffInCompany(int CompanyId, int jobid)
        {
            string query = "update Staff set isexpunged =1,StaffJobId= :jobid where companysubsidiary= :subsidiary";

            IQuery update = _session.CreateQuery(query)

                                .SetParameter("subsidiary", CompanyId.ToString())
            .SetParameter("jobid", jobid);

            update.ExecuteUpdate();


            //expunge enrollees
            IList<StaffnameandPlan> staff = GetAllStaffinCompanyLite(CompanyId, 0);

            foreach (StaffnameandPlan item in staff)
            {
                IList<Enrollee> enrolleeeeede = _session.QueryOver<Enrollee>().Where(x => x.Staffprofileid == item.Id && x.Isexpundged == false)
           .List();

                foreach (Enrollee enrolll in enrolleeeeede)
                {
                    enrolll.Isexpundged = true;
                    enrolll.ExpungeNote = "This was exunged for renewal for jobid : " + jobid.ToString();
                    enrolll.Dateexpunged = CurrentRequestData.Now;
                    enrolll.Bulkjobid = jobid;
                    _session.Transact(session => session.Update(enrolll));


                }

            }

            return true;

        }


    }
}