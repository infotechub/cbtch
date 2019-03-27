using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Antlr.Runtime.Misc;
using MrCMS.Entities.People;
using MrCMS.Logging;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Models.Provider;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Areas.Admin.Services;
using MrCMS.Website;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Helpers;
using System.Globalization;
using static MrCMS.Web.Apps.Core.Utility.Tools;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace MrCMS.Web.Apps.Core.Services
{
    public class EnrolleeService : IEnrolleeService
    {
        private readonly ISession _session;
        private readonly ILogAdminService _logger;
        private readonly IUserService _userService;
        private readonly IHelperService _helpersvc;
        public EnrolleeService(ISession session, ILogAdminService log, IUserService userserv, IHelperService helpersvc)
        {
            _session = session;
            _logger = log;
            _userService = userserv;
            _helpersvc = helpersvc;

        }
        public bool ValidatePolicyNumber(string policy)
        {
            return !(_session.QueryOver<Enrollee>().Where(x => x.Policynumber == policy).Any());
        }

        public IList<Enrollee> Getallenrollee()
        {
            return _session.QueryOver<Enrollee>().Where(x => x.IsDeleted == false).List<Enrollee>();
        }

        public IList<Enrollee> GetallenrolleeRange(out int totalRecord, int start, int lenght)
        {
            IQueryOver<Enrollee, Enrollee> query = _session.QueryOver<Enrollee>().Where(x => x.IsDeleted == false && x.Isexpundged == false);
            totalRecord = query.RowCount();
            return query.Skip(start).Take(lenght).List();
        }

        public IList<Enrollee> QueryAllenrollee(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcPolicynumber, string scrOthername, string scrLastname, string scrMobilenumber, string scrProvider, string scrCompany, string scrCompanySubsidiary, bool useDate, DateTime scrFromDate, DateTime scrToDate, int showExpunge, string scrUsers, int enrolleetype, int otherFilters, int zones, int state, int plantype, int planmode)
        {

            IQueryOver<Enrollee, Enrollee> query = _session.QueryOver<Enrollee>().Where(x => x.IsDeleted == false);

            if (!string.IsNullOrEmpty(srcPolicynumber))
            {
                //search policy number



                srcPolicynumber = "%" + srcPolicynumber + "%";


                query = query.Where(Restrictions.On<Enrollee>(x => x.Policynumber).IsInsensitiveLike(srcPolicynumber) || Restrictions.On<Enrollee>(x => x.RefPolicynumber).IsInsensitiveLike(srcPolicynumber));


                //.JoinAlias(p => p.Primaryprovider, () => providerAlias)
                //.JoinAlias(p => p.Stateid, () => stateAlias)
                //.JoinAlias(p=>p.Staffprofileid,()=> staffAlias)
                //.JoinAlias(()=>staffAlias.CompanySubsidiary,()=> subsidiaryAlias)

                //.Where(()=>companyAlias.Name =="")


                //Restrictions.On<Enrollee>(x => x.Policynumber).IsInsensitiveLike(search) ||
                //                       Restrictions.On<Enrollee>(x => x.Surname).IsInsensitiveLike(search) ||
                //                       Restrictions.On<Enrollee>(x => x.Othernames).IsInsensitiveLike(search) ||
                //                       Restrictions.On<Enrollee>(x => x.Occupation).IsInsensitiveLike(search) ||
                //                       Restrictions.On<Enrollee>(x => x.Mobilenumber).IsInsensitiveLike(search) ||
                //                       Restrictions.On<Enrollee>(x => x.Residentialaddress).IsInsensitiveLike(search));
                ;



            }




            if (!string.IsNullOrEmpty(scrOthername))
            {
                string content = Regex.Replace(scrOthername.Trim(), @"\s+", ":");
                string[] namelist = content.Split(':');

                foreach (string namesearch in namelist)
                {
                    string seartxt = "%" + namesearch + "%";
                    query.Where(Restrictions.InsensitiveLike(
                                       Projections.SqlFunction("concat",
                                           NHibernateUtil.String,
                                           Projections.Property("Othernames"),
                                           Projections.Constant(" "),
                                           Projections.Property("Surname")),
                                      seartxt,
                                       MatchMode.Anywhere));
                }


            }



            if (!string.IsNullOrEmpty(scrMobilenumber))
            {
                scrMobilenumber = "%" + scrMobilenumber + "%";
                query = query.Where(Restrictions.On<Enrollee>(x => x.Mobilenumber).IsInsensitiveLike(scrMobilenumber));
            }
            if (!string.IsNullOrEmpty(scrProvider) && Convert.ToInt32(scrProvider) > -1)
            {
                int provider = Convert.ToInt32(scrProvider);
                query = query.Where(x => x.Primaryprovider == provider);
            }
            if (!string.IsNullOrEmpty(scrCompany) && Convert.ToInt32(scrCompany) > -1)
            {
                int company = Convert.ToInt32(scrCompany);
                query = query.Where(x => x.Companyid == company);
            }

            if (!string.IsNullOrEmpty(scrCompanySubsidiary) && Convert.ToInt32(scrCompanySubsidiary) > -1)
            {
                int companysub = Convert.ToInt32(scrCompanySubsidiary);
                List<int> stafflist = (List<int>)_session.QueryOver<Staff>().Where(x => x.IsDeleted == false && x.CompanySubsidiary == companysub).SelectList(a => a.Select(p => p.Id)).List<int>();
                query = query.WhereRestrictionOn(w => w.Staffprofileid).IsIn(stafflist);

            }

            if (plantype > -1 && !string.IsNullOrEmpty(scrCompany) && Convert.ToInt32(scrCompany) > 0)
            {
                List<int> companyplan = (List<int>)_session.QueryOver<CompanyPlan>().Where(x => x.Planid == plantype && x.Companyid == Convert.ToInt32(scrCompany)).Select(a => a.Id).List<int>();

                if (planmode == 0)
                {
                    //single
                    companyplan = (List<int>)_session.QueryOver<CompanyPlan>().Where(x => x.Planid == plantype && x.AllowChildEnrollee == false && x.Companyid == Convert.ToInt32(scrCompany)).Select(a => a.Id).List<int>();
                }
                if (planmode == 1)
                {
                    //family
                    companyplan = (List<int>)_session.QueryOver<CompanyPlan>().Where(x => x.Planid == plantype && x.AllowChildEnrollee == true && x.Companyid == Convert.ToInt32(scrCompany)).Select(a => a.Id).List<int>();
                }


                List<int> stafflist = (List<int>)_session.QueryOver<Staff>().Where(x => x.IsDeleted == false).WhereRestrictionOn(w => w.StaffPlanid).IsIn(companyplan).SelectList(a => a.Select(p => p.Id)).List<int>();

                query = query.WhereRestrictionOn(w => w.Staffprofileid).IsIn(stafflist);



            }



            if (useDate)
            {
                DateTime datete = Convert.ToDateTime(scrToDate);

                int dd = datete.Day;
                int month = datete.Month;
                int year = datete.Year;

                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", month, dd, year, time));
                query.Where(Restrictions.On<Enrollee>(a => a.CreatedOn).IsBetween(scrFromDate).And(enddate));
            }
            if (!string.IsNullOrEmpty(scrUsers))
            {
                int toint = Convert.ToInt32(scrUsers);

                if (toint > 0)
                {
                    query.Where(x => x.Createdby == toint);
                }

            }
            if (showExpunge > -1)
            {
                switch (showExpunge)
                {
                    case 0:
                        //all
                        break;
                    case 1:
                        //not expunged
                        query = query.Where(x => x.Isexpundged == false);
                        break;
                    case 2:
                        query = query.Where(x => x.Isexpundged == true);
                        break;

                }
            }

            if (enrolleetype > -1)
            {
                switch (enrolleetype)
                {
                    case 0:
                        //all
                        break;
                    case 1:
                        //not expunged
                        query = query.Where(x => x.Parentid <= 0);
                        break;
                    case 2:
                        query = query.Where(x => x.Parentid > 0);
                        break;

                }
            }




            if (zones > -1)
            {
                List<int> states = (List<int>)_session.QueryOver<State>().Where(x => x.Zone == zones).SelectList(a => a.Select(p => p.Id)).List<int>();


                //var ids = new List<int> { 1, 2, 5, 7 };
                //var query2 = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2 );

                query
                    .WhereRestrictionOn(w => w.Stateid).IsIn(states);
            }


            if (state > -1)
            {
                query = query.Where(x => x.Stateid == state);
            }

            DateTime today = CurrentRequestData.Now;
            DateTime childagelimit = today.Date.AddYears(-21);
            DateTime parentagelimit = today.Date.AddYears(-70);
            if (otherFilters > -1)
            {
                switch (otherFilters)
                {
                    case 0:
                        //all
                        break;
                    case 1:
                        //female only
                        query = query.Where(x => x.Sex == (int)Sex.Female);
                        break;
                    case 2:
                        //Male only
                        query = query.Where(x => x.Sex == (int)Sex.Male);
                        break;
                    case 3:
                        //With Mobile Numbers only
                        query = query.Where(Restrictions.Gt(
            Projections.SqlFunction("length", NHibernateUtil.String,
                Projections.Property<Enrollee>(b => b.Mobilenumber)),
            9
        ));
                        break;

                    case 4:
                        //Without Mobile Numbers only
                        query.Where(Restrictions.Lt(
           Projections.SqlFunction("length", NHibernateUtil.String,
               Projections.Property<Enrollee>(b => b.Mobilenumber)),
           9
       ));
                        break;


                    case 5:
                        //With email only
                        query.Where(Restrictions.Gt(
             Projections.SqlFunction("length", NHibernateUtil.String,
                 Projections.Property<Enrollee>(b => b.Emailaddress)),
             5
         ));
                        break;

                    case 6:
                        //Without email only
                        query.Where(Restrictions.Lt(
       Projections.SqlFunction("length", NHibernateUtil.String,
           Projections.Property<Enrollee>(b => b.Emailaddress)),
       5
   ));
                        break;

                    case 7:
                        //with age 
                        query = query.Where(x => x.Dob <= childagelimit && x.Parentid > 0 && x.Parentrelationship == (int)Relationship.Child || x.Dob <= parentagelimit && x.Parentid == 0);
                        break;

                    case 8:
                        //with birthdays for the day
                        string etoday = today.ToString("MM-dd");
                        query = query.Where(x => x.Dob.Month == today.Month && x.Dob.Day == today.Day);


                        break;

                    case 9:
                        //idcard not printed
                        query = query.Where(x => x.IdCardPrinted == false);
                        break;

                    case 10:
                        query = query.Where(x => x.IdCardPrinted == true);
                        break;
                }
            }
            //sort order

            //return normal list.
            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            return query.Skip(start).Take(lenght).List();
        }

        public bool AddEnrollee(Enrollee enrollee, byte[] imgData)
        {

            bool exist =
                _session.QueryOver<Enrollee>()
                    .Where(
                        x =>
                            x.Policynumber == enrollee.Policynumber.ToLower() ||
                            x.Policynumber == enrollee.Policynumber.ToUpper()).Any();



            if (enrollee != null && !exist)
            {


                EnrolleePassport imagerec = new EnrolleePassport();
                imagerec.Enrolleeid = enrollee.Id;
                imagerec.Enrolleepolicyno = enrollee.Policynumber;
                imagerec.Imgraw = imgData;

                enrollee.EnrolleePassport = imagerec;

                _session.Transact(session => session.Save(enrollee));



                if (enrollee.Parentid > 0)
                {

                }
                else
                {
                    //update the staff to indicate update
                    Staff staff = _session.QueryOver<Staff>().Where(x => x.Id == enrollee.Staffprofileid).SingleOrDefault();

                    staff.HasProfile = true;
                    staff.Profileid = enrollee.Id;
                    _session.Transact(session => session.Save(staff));

                }


                int curr = CurrentRequestData.CurrentUser != null ? CurrentRequestData.CurrentUser.Id : 0;


                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "New enrollee has been added to the system enrollee name {0} , enrollee id {1}, by {2}",
                                  enrollee.Surname + " " + enrollee.Othernames, enrollee.Id, curr), "Enrollee Added.");

                return true;
            }
            return false;
        }

        public bool DeletEnrollee(Enrollee enrollee)
        {
            if (enrollee != null)
            {

                _session.Transact(session => session.Delete(enrollee));


                _helpersvc.Log(LogEntryType.Audit, null,
                               string.Format(
                                   "Enrollee has been deleted on the  system enrollee name {0} , enrollee id {1}, by {2}",
                                   enrollee.Surname + " " + enrollee.Othernames, enrollee.Id, CurrentRequestData.CurrentUser.Id.ToString()), "Enrollee Deleted.");
                return true;
            }
            return false;
        }

        public bool UpdateEnrollee(Enrollee enrolee)
        {
            if (enrolee != null)
            {

                _session.Transact(session => session.Update(enrolee));

                _helpersvc.Log(LogEntryType.Audit, null,
                               string.Format(
                                   "Enrollee has been updated on the  system enrollee name {0} , enrollee id {1}, by {2}",
                                   enrolee.Surname + " " + enrolee.Othernames, enrolee.Id, CurrentRequestData.CurrentUser != null ? CurrentRequestData.CurrentUser.Id.ToString() : "1"), "Enrollee Updated.");
                return true;
            }
            return false;
        }

        public Enrollee GetEnrollee(int id)
        {

            Enrollee enrollee = _session.QueryOver<Enrollee>().Where(x => x.Id == id).SingleOrDefault();
            return enrollee;
        }

        public IList<Enrollee> GetEnrolleesByStaffId(int id)
        {
            IList<Enrollee> enrollees = _session.QueryOver<Enrollee>().Where(x => x.Staffprofileid == id && x.IsDeleted == false).List<Enrollee>();
            return enrollees;
        }

        public Enrollee GetEnrolleeByPolicyNumber(string policyNumber)
        {
            if (!string.IsNullOrEmpty(policyNumber))
            {
                Enrollee enrollee = _session.QueryOver<Enrollee>().Where(x => x.Policynumber == policyNumber.ToUpper() || x.Policynumber == policyNumber.ToLower()).SingleOrDefault();
                return enrollee;
            }

            return null;
        }

        public Enrollee GetEnrolleeByReferencePolicyNumber(string policyNumber)
        {
            if (!string.IsNullOrEmpty(policyNumber))
            {
                Enrollee enrollee = _session.QueryOver<Enrollee>().Where(x => x.RefPolicynumber == policyNumber.ToUpper() || x.RefPolicynumber == policyNumber.ToLower()).SingleOrDefault();
                return enrollee;
            }

            return null;
        }

        public IList<Enrollee> GetFamilyTreeByPolicyNumber(string policyNumber)
        {
            List<Enrollee> response = new List<Enrollee>();
            if (!string.IsNullOrEmpty(policyNumber))
            {
                policyNumber = policyNumber.ToLower();
                IQueryOver<Enrollee, Enrollee> enrollee = _session.QueryOver<Enrollee>().Where(x => x.Policynumber == policyNumber || x.Policynumber == policyNumber.ToUpper());



                if (enrollee.RowCount() > 0)
                {
                    Enrollee enrolleeObj = enrollee.SingleOrDefault();

                    //check if principal

                    if (enrolleeObj.Parentid > 0)
                    {
                        //child
                        //get the parent
                        Enrollee parent = GetEnrollee(enrolleeObj.Parentid);
                        IList<Enrollee> dependants = GetDependentsEnrollee(parent.Id);

                        response.Add(parent);
                        response.AddRange(dependants);

                    }
                    else
                    {
                        //parent

                        IList<Enrollee> dependants = GetDependentsEnrollee(enrolleeObj.Id);

                        response.Add(enrolleeObj);
                        response.AddRange(dependants);



                    }

                }
            }
            return response;
        }

        public bool CheckEnrolleePhoneNumber(string phonenumber, string enrolleePolicyNumber)
        {

            if (phonenumber.StartsWith("234"))
            {
                phonenumber = phonenumber.Replace("234", "");

            }
            phonenumber = "%" + phonenumber + "%";
            bool resp = _session.QueryOver<Enrollee>().Where(Restrictions.On<Enrollee>(b => b.Mobilenumber).IsInsensitiveLike(phonenumber) && Restrictions.On<Enrollee>(b => b.Policynumber).IsInsensitiveLike(enrolleePolicyNumber)).Any() || _session.QueryOver<Enrollee>().Where(Restrictions.On<Enrollee>(b => b.Mobilenumber).IsInsensitiveLike(phonenumber) && Restrictions.On<Enrollee>(b => b.RefPolicynumber).IsInsensitiveLike(enrolleePolicyNumber)).Any(); ;
            return resp;

        }


        public IList<Enrollee> GetDependentsEnrollee(int principalid)
        {
            return _session.QueryOver<Enrollee>().Where(x => x.Parentid == principalid).List<Enrollee>();
        }

        public IList<Enrollee> GetEnrolleeunderPlan(int planid)
        {
            return _session.QueryOver<Enrollee>().Where(x => x.Subscriptionplanid == planid && x.Hasactivesubscription == false).List<Enrollee>();
        }

        public EnrolleePassport GetEnrolleepassport(int enrolleeid)
        {
            EnrolleePassport enrolleePass = _session.QueryOver<EnrolleePassport>().Where(x => x.Enrolleeid == Convert.ToInt32(enrolleeid)).SingleOrDefault();
            return enrolleePass;
        }

        public bool UpdateEnrollee(EnrolleePassport enroleepassport)
        {
            if (enroleepassport != null)
            {

                _session.Transact(session => session.Update(enroleepassport));

                _helpersvc.Log(LogEntryType.Audit, null,
                               string.Format(
                                   "EnrolleePassport has been updated on the  system enrollee id {0}, by {1}",
                                    enroleepassport.Id, CurrentRequestData.CurrentUser.Id.ToString()), "Enrollee Updated.");
                return true;
            }
            return false;
        }

        public IList<MobileEnrolleeTied> GetEnrolleesTiedToPhone(string phone)
        {
            IList<Enrollee> items = _session.QueryOver<Enrollee>().Where(x => x.Mobilenumber == phone && x.IsDeleted == false).List<Enrollee>();
            List<MobileEnrolleeTied> response = items.Select(item => new MobileEnrolleeTied()
            {
                PolicyNumber = item.Policynumber,
                FullName = item.Surname + " " + item.Othernames
            }).ToList();


            return response;
        }

        public bool AddMobileSignup(MobileSignup signup)
        {
            if (signup != null)
            {
                _session.Transact(session => session.Save(signup));
                return true;
            }
            return false;
        }

        public bool AddShortMessage(ShortCodeMsg shortCodeMsg)
        {
            if (shortCodeMsg != null)
            {
                _session.Transact(session => session.Save(shortCodeMsg));
                return true;
            }
            return false;
        }

        public bool UpdateShortMessage(ShortCodeMsg shortCodeMsg)
        {
            if (shortCodeMsg != null)
            {

                _session.Transact(session => session.Update(shortCodeMsg));


                return true;
            }
            return false;
        }

        public ShortCodeMsg GetShortMessage(int id)
        {
            ShortCodeMsg shortCode = _session.QueryOver<ShortCodeMsg>().Where(x => x.Id == id).SingleOrDefault();
            return shortCode;
        }

        public IList<ShortCodeMsg> GetShortMessages()
        {
            return _session.QueryOver<ShortCodeMsg>().Where(x => x.IsDeleted == false).List<ShortCodeMsg>();
        }

        public IList<ShortCodeMsg> GetUnProccessedShortMessages()
        {
            return _session.QueryOver<ShortCodeMsg>().Where(x => x.IsDeleted == false && x.Status == false).List<ShortCodeMsg>();
        }

        public IList<Enrollee> GetEnrolleeCelebratingBirthday()
        {
            DateTime today = CurrentRequestData.Now;
            IList<Enrollee> result = _session.QueryOver<Enrollee>().Where(x => x.IsDeleted == false && x.Isexpundged == false && (x.Dob.Day >= today.AddDays(-2).Day && x.Dob.Day < (today.Day + 2)) && x.Dob.Month == today.Month && x.Parentid < 1).List<Enrollee>();

            List<Enrollee> bithdaylist = new List<Enrollee>();
            foreach (Enrollee item in result.ToList())
            {
                DateTime datee = Convert.ToDateTime(item.Dob);
                if (datee.Month == today.Month && datee.Day == today.Day)
                {
                    bithdaylist.Add(item);

                }
            }

            //some shit i added for the sake of it
            bool exist = result.ToList().Where(x => x.Id == 3151).Any();

            return bithdaylist; /*result.Where(x => x.Dob.Day == today.Day && x.Dob.Month == today.Month).ToList<Enrollee>();*/

        }

        public string GetConcatenatedPhoneNumberString(SMSLeads Mode)
        {
            IQueryOver<Enrollee, Enrollee> query = _session.QueryOver<Enrollee>().Where(x => x.Isexpundged == false && x.IsDeleted == false);
            query.Where(Restrictions.Gt(
          Projections.SqlFunction("length", NHibernateUtil.String,
              Projections.Property<Enrollee>(b => b.Mobilenumber)),
          9
      ));
            switch (Mode)
            {
                case SMSLeads.OnlyPrincipal:
                    query.Where(x => x.Parentid < 1);
                    break;
            }



            string response = string.Empty;

            foreach (Enrollee item in query.List<Enrollee>())
            {
                if (!response.Contains(item.Mobilenumber))
                {
                    response = response + "," + item.Mobilenumber;
                }

            }

            return response;
        }

        public IList<EnrolleeVerificationCode> GetPreviousDayAuthenticatedCodes()
        {

            IQueryOver<EnrolleeVerificationCode, EnrolleeVerificationCode> query = _session.QueryOver<EnrolleeVerificationCode>().Where(x => x.Status == EnrolleeVerificationCodeStatus.Authenticated && x.IsDeleted == false && x.PostSMSSent == false);

            DateTime datete = CurrentRequestData.Now.AddHours(-24);

            int dd = datete.Day;
            int month = datete.Month;
            int year = datete.Year;
            string time2 = "00:00";
            string time = "23:59";
            DateTime startdate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", month, dd, year, time2));
            DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", month, dd, year, time));
            query.Where(Restrictions.On<EnrolleeVerificationCode>(a => a.DateAuthenticated).IsBetween(startdate).And(enddate));


            return query.List<EnrolleeVerificationCode>();
        }

        public bool AddTempEnrollee(TempEnrollee enrollee)
        {
            if (enrollee != null)
            {
                _session.Transact(session => session.SaveOrUpdate(enrollee));

                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "New Temp Enrollee has been added to the system enrollee name {0} , enrollee id {1}, by {2}",
                                  enrollee.Surname + " " + enrollee.Othernames, enrollee.Id, 1), "Enrollee Added.");

                return true;
            }
            return false;
        }

        public TempEnrollee GetTempEnrollee(int id)
        {
            TempEnrollee query = _session.QueryOver<TempEnrollee>().Where(x => x.Id == id).SingleOrDefault();

            return query;
        }

        public TempEnrollee GetTempEnrolleeByMobileorEmail(string MobileorEmail)
        {
            return new TempEnrollee();
        }

        public IList<EnrolleePolicyName> GetEnrolleePolicyNumberName(string phrase)
        {
            IQueryOver<Enrollee, Enrollee> query = _session.QueryOver<Enrollee>().Where(x => x.IsDeleted == false);
            List<EnrolleePolicyName> response = new List<EnrolleePolicyName>();
            if (!string.IsNullOrEmpty(phrase))
            {
                //search policy number

                phrase = "%" + phrase + "%";

                query = query.Where(Restrictions.On<Enrollee>(x => x.Policynumber).IsInsensitiveLike(phrase));

                foreach (Enrollee item in query.List().OrderBy(x => x.Policynumber))
                {
                    EnrolleePolicyName itemo = new EnrolleePolicyName
                    {
                        Id = item.Id,
                        Name = item.Surname + " " + item.Othernames,
                        Policynumber = item.Policynumber,

                    };

                    response.Add(itemo);

                }
            }

            return response;
        }



        public TempEnrollee GetTempEnrolleebystaffid(string staffid)
        {
            //var query = _session.QueryOver<TempEnrollee>().Where(x => x.staffid == staffid.ToUpper() || x.staffid == staffid.ToLower());
            IQueryOver<TempEnrollee, TempEnrollee> query = _session.QueryOver<TempEnrollee>().Where(x => x.Staffprofileid == Convert.ToInt32(staffid));
            return query.Take(1).SingleOrDefault();
        }

        public TempEnrollee GetTempEnrolleebystaffProfileid(int staffid)
        {
            IQueryOver<TempEnrollee, TempEnrollee> query = _session.QueryOver<TempEnrollee>().Where(x => x.Staffprofileid == staffid);
            return query.Take(1).SingleOrDefault();
        }

        public long EnrolleeCount()
        {
            return _session.QueryOver<Enrollee>().Where(x => x.IsDeleted == false && x.Isexpundged == false).RowCount();
        }

        public IList<Enrollee> GetEnrolleesbyPhone(string mobile)
        {
            IList<Enrollee> items = _session.QueryOver<Enrollee>().Where(x => x.Mobilenumber == mobile && x.IsDeleted == false).List<Enrollee>();
            return items.ToList();


        }

        public bool AddTempDependant(PendingDependant dependant)
        {
            if (dependant != null)
            {
                _session.Transact(session => session.SaveOrUpdate(dependant));

                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "New Temp Dependant Added to the system {0} ,{1}, by {2}",
                                  dependant.lastname + " " + dependant.firstName, dependant.Principalpolicynum, 1), "Temp Dependant Added.");

                return true;
            }
            return false;
        }

        public IList<PendingDependant> getalltempDependant()
        {

            DateTime startdate = CurrentRequestData.Now.AddMonths(-3);

            //sort for last three months 
            return _session.QueryOver<PendingDependant>().Where(x => x.IsDeleted == false && x.Approved == false && x.ImgRaw != null).Where(Restrictions.On<PendingDependant>(a => a.CreatedOn).IsBetween(startdate).And(CurrentRequestData.Now.AddDays(1))).List();


        }

        public PendingDependant getTempDependant(int Id)
        {
            return _session.QueryOver<PendingDependant>().Where(x => x.IsDeleted == false && x.Id == Id).SingleOrDefault();
        }

        public Enrollee GetEnrolleeGuid(string Guid)
        {
            Guid guid = new Guid(Guid);

            Enrollee enrollee = _session.QueryOver<Enrollee>().Where(x => x.Guid == guid).SingleOrDefault();
            return enrollee;
        }

        public bool updateTempDependant(PendingDependant Dep)
        {
            if (Dep != null)
            {
                _session.Transact(session => session.Update(Dep));

                _helpersvc.Log(LogEntryType.Audit, null,
                              string.Format(
                                  "Temp Dependant updated to the system {0} ,{1}, by {2}",
                                  Dep.lastname + " " + Dep.firstName, Dep.Principalpolicynum, 1), "Temp Dependant updated.");

                return true;
            }
            return false;
        }

        public IList<Enrollee> GetEnrolleesbyEmail(string email)
        {
            IList<Enrollee> items = _session.QueryOver<Enrollee>().Where(x => x.Emailaddress == email && x.IsDeleted == false).List<Enrollee>();
            return items.ToList();
        }

        public IList<PendingDependant> getTempDependant(string parentguid)
        {
            return _session.QueryOver<PendingDependant>().Where(x => x.IsDeleted == false && x.principalGuid == parentguid && x.Approved == false).List();
        }

        public IList<EnrolleeVerificationCode> QueryVerificationCode(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcPolicynumber, int providerid, string verificationcode, string scrMobilenumber, bool useDate, DateTime scrFromDate, DateTime scrToDate, evsvisittype visittype)
        {
            IQueryOver<EnrolleeVerificationCode, EnrolleeVerificationCode> query = _session.QueryOver<EnrolleeVerificationCode>().Where(x => x.IsDeleted == false);

            if (!string.IsNullOrEmpty(srcPolicynumber))
            {
                Enrollee enrolll = GetEnrolleeByPolicyNumber(srcPolicynumber);
                if (enrolll != null)
                {
                    query.Where(x => x.EnrolleeId == enrolll.Id);
                }

            }


            if (visittype != evsvisittype.All)
            {
                query.Where(x => x.visittype == visittype);

            }
            if (providerid > -1)
            {
                query.Where(x => x.ProviderId == providerid);
            }

            if (!string.IsNullOrEmpty(verificationcode))
            {
                query.Where(x => x.VerificationCode == verificationcode);

            }

            if (!string.IsNullOrEmpty(scrMobilenumber))
            {
                query.Where(x => x.RequestPhoneNumber == scrMobilenumber);

            }

            if (useDate)
            {
                DateTime datete = Convert.ToDateTime(scrToDate);

                int dd = datete.Day;
                int month = datete.Month;
                int year = datete.Year;

                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", month, dd, year, time));
                query.Where(Restrictions.On<EnrolleeVerificationCode>(a => a.CreatedOn).IsBetween(scrFromDate).And(enddate));

            }

            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;

            return query.OrderBy(x => x.CreatedOn).Desc().Skip(start).Take(lenght).List<EnrolleeVerificationCode>();
        }

        public object QueryAllenrollee(out int toltareccount, out int totalinresult, string search, int v1, int v2, string sortColumnnumber, string sortOrder, string scrPolicynumber, string scrOthername, string scrLastname, string scrMobilenumber, string scrProvider, string scrCompany, bool usedate, DateTime fromdate, DateTime todate, int showexpunge, string scr_user)
        {
            throw new NotImplementedException();
        }
    }
}