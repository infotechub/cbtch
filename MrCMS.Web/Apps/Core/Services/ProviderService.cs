using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Antlr.Runtime.Misc;
using MrCMS.Logging;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Models.Provider;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Website;
using NHibernate;
using NHibernate.Transform;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Helpers;
using NHibernate.Criterion;

namespace MrCMS.Web.Apps.Core.Services
{
    public class ProviderService : IProviderService
    {
        private readonly ISession _session;
        private readonly IUserService _userService;
        private readonly IHelperService _helpersvc;
        private readonly IServicesService _servicevc;
        private readonly IPlanService _plansvc;
        private readonly IUniquePageService _uniquePageService;
        private readonly ITariffService _tariffService;
        private readonly ICompanyService _companyService;
        //private readonly INotificationHubService _notificationHubService;
        public ProviderService(ISession session, IUserService userservice, IHelperService helpersvc, IServicesService servicesvc, IPlanService plansvc, IUniquePageService uniquepage, ITariffService tariffservice, ICompanyService companyservice)
        {
            _session = session;
            _userService = userservice;
            _helpersvc = helpersvc;
            _servicevc = servicesvc;
            _plansvc = plansvc;
            _uniquePageService = uniquepage;
            _tariffService = tariffservice;
            _companyService = companyservice;
            //_notificationHubService = notificationHub;
        }
        public IList<Provider> GetallProvider()
        {
            return _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.isDelisted == false).List<Provider>();
        }

        public IList<ProviderVm> GetallProviderforJson()
        {
            List<ProviderVm> response = new List<ProviderVm>();
            IList<Provider> list = GetallProvider();
            if (list != null)
            {
                response.AddRange(from item in list
                                  let split = !string.IsNullOrEmpty(item.Providerservices) ? item.Providerservices.Split(',') : new string[] { }
                                  let split2 = !string.IsNullOrEmpty(item.Providerplans) ? item.Providerplans.Split(',') : new string[] { }
                                  let servicelist = split.Select(serv => _servicevc.GetService(Convert.ToInt32(serv)).Name).ToList()
                                  let planlist = split2.Select(plan => _plansvc.GetPlan(Convert.ToInt32(plan)).Name).ToList()


                                  select new ProviderVm()
                                  {
                                      Id = item.Id,
                                      Name = item.Name.ToUpper(),
                                      Code = item.Code,
                                      SubCode = item.SubCode,
                                      //Phone = item.Phone,
                                      //Phone2 = item.Phone2,
                                      //Email = item.Email,
                                      //Website = item.Website,
                                      Address = item.Address,
                                      //Provideraccount = item.Provideraccount ?? new ProviderAccount(),
                                      //Provideraccount2 = item.Provideraccount2 ?? new ProviderAccount(),
                                      //Assignee = item.Assignee,
                                      //AssigneeName = _userService.GetUser(item.Assignee).Name,
                                      State = item.State,
                                      Area = item.Area,
                                      Lganame = item.Lga.Name,
                                      Lgaid = item.Lga.Id
                                      //AuthorizationStatus = item.AuthorizationStatus,
                                      //AuthorizationNote = item.AuthorizationNote,
                                      //AuthorizedBy = item.AuthorizedBy,
                                      //Status = item.Status,
                                      //Providerplans = planlist,
                                      //Providerservices = servicelist,
                                      //CreatedBy = item.CreatedBy != null ? _userService.GetAllUsers().SingleOrDefault(x => x.Guid.ToString().ToLower() == Convert.ToString(item.CreatedBy).ToLower()).Name : "",
                                      //Zone = _helpersvc.GetzonebyId(Convert.ToInt32(item.State.Zone)).Name,
                                      //BankName = _helpersvc.Getbank(item.Provideraccount.BankId).Name,
                                      //BankName2 = item.Provideraccount2 != null? _helpersvc.Getbank(item.Provideraccount.BankId).Name : "--",
                                      //CreatedDate = item.CreatedOn.ToString("dd MMM yyyy"),
                                      //    ProviderplansStr = item.Providerplans,
                                      //    ProviderservicesStr = item.Providerservices,
                                      //    AuthorizedDate = item.AuthorizedDate,
                                      //    AuthorizedByString = item.AuthorizedBy > 0 && item.AuthorizedBy > 0 ? _userService.GetUser(item.AuthorizedBy).Name : "--",
                                      //    AuthorizationStatusStr = Enum.GetName(typeof(AuthorizationStatus), item.AuthorizationStatus)
                                  });
                return response;
            }
            return new List<ProviderVm>();
        }
        public IList<ProviderVm> QueryallProviderforJson(out int totalRecord, out int totalcountinresult, string search, int start, int lenght,
      string sortColumn, string sortOrder, string srcProviderName, string scrAddress, int state, int zone, bool useDate,
      DateTime scrFromDate, DateTime scrToDate, string scrUsers, int otherFilters, int plantype, int Zone, int ServiceType, int BoundByType, int category, bool delistshow)
        {


            IQueryOver<Provider, Provider> query = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && (x.AuthorizationStatus == 0 || x.AuthorizationStatus == 2));

            if (!delistshow)
            {
                query.Where(x => x.isDelisted == false);

            }
            List<ProviderVm> response = new List<ProviderVm>();
            if (!string.IsNullOrEmpty(srcProviderName))
            {

                srcProviderName = "%" + srcProviderName + "%";
                query.AndRestrictionOn(x => x.Name).IsInsensitiveLike(srcProviderName);

            }

            if (!string.IsNullOrEmpty(scrAddress))
            {
                scrAddress = "%" + scrAddress + "%";
                query.AndRestrictionOn(x => x.Address).IsInsensitiveLike(scrAddress);
            }

            if (state > 0)
            {
                query.Where(x => x.State.Id == state);
            }

            if (category > -1)
            {
                ProviderCategory conv = (ProviderCategory)category;
                query.Where(x => x.Category == conv);

            }

            if (zone > 0)
            {
                query.Where(x => x.State.Zone == zone);

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

                query.Where(x => x.CreatedBy == scrUsers);


            }
            if (Zone > -1)
            {
                List<int> states = (List<int>)_session.QueryOver<State>().Where(x => x.Zone == Zone).SelectList(a => a.Select(p => p.Id)).List<int>();


                //var ids = new List<int> { 1, 2, 5, 7 };
                //var query2 = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2 );

                query
 .WhereRestrictionOn(w => w.State).IsIn(states);



            }

            if (ServiceType > -1)
            {
                string serviceid = "%" + Convert.ToString(ServiceType) + "%";

                IQueryOver<Provider, Provider> query2 = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2);
                query2.AndRestrictionOn(x => x.Providerservices).IsInsensitiveLike(ServiceType);
                List<int> output = new List<int>();
                int count = query2.RowCount();
                for (int i = 0; i < count; i++)
                {

                    Provider item = query2.Skip(i).Take(1).SingleOrDefault();
                    string[] allservices = item.Providerservices.Split(',');
                    output.AddRange(from service in allservices where service.Trim() == Convert.ToString(ServiceType) select item.Id);


                }

                query.WhereRestrictionOn(bp => bp.Id)
         .IsIn(output);

            }
            if (plantype > -1)
            {
                string planid = "%" + Convert.ToString(plantype) + "%";

                IQueryOver<Provider, Provider> query2 = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2);
                query2.AndRestrictionOn(x => x.Providerplans).IsInsensitiveLike(planid);
                List<int> output = new List<int>();
                int count = query2.RowCount();
                for (int i = 0; i < count; i++)
                {

                    Provider item = query2.Skip(i).Take(1).SingleOrDefault();
                    string[] allplans = item.Providerplans.Split(',');
                    //if bound ny type is greater than -1 then check the first plan
                    if (BoundByType > -1)
                    {
                        if (allplans.Count() > 0 && allplans[0] == Convert.ToString(plantype))
                        {
                            output.Add(item.Id);
                        }

                    }
                    else
                    {
                        output.AddRange(from plan in allplans where plan.Trim() == Convert.ToString(plantype) select item.Id);
                    }



                }



                query.WhereRestrictionOn(bp => bp.Id)
         .IsIn(output);


            }

            //sort order

            //return normal list.
            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            IList<Provider> list = query.Skip(start).Take(lenght).List();


            if (list != null)
            {
                response.AddRange(from item in list
                                  let split = !string.IsNullOrEmpty(item.Providerservices) ? item.Providerservices.Split(',') : new string[] { }
                                  let split2 = !string.IsNullOrEmpty(item.Providerplans) ? item.Providerplans.Split(',') : new string[] { }
                                  let servicelist = split.Select(serv => _servicevc.GetService(Convert.ToInt32(serv)).Name).ToList()
                                  let planlist = split2.Select(plan => _plansvc.GetPlan(Convert.ToInt32(plan)).Name).ToList()
                                  let singleOrDefault = _userService.GetAllUsers().SingleOrDefault(x => x.Guid.ToString().ToLower() == Convert.ToString(item.CreatedBy).ToLower())

                                  let providerAccount = item.Provideraccount
                                  where providerAccount != null
                                  select new ProviderVm()
                                  {
                                      Id = item.Id,
                                      Name = item.Name.ToUpper(),
                                      Code = item.Code,
                                      SubCode = item.SubCode,
                                      Phone = item.Phone,
                                      Phone2 = item.Phone2,
                                      Email = item.Email,
                                      Website = item.Website,
                                      Address = item.Address,
                                      Provideraccount = providerAccount ?? new ProviderAccount(),
                                      Assignee = item.Assignee,
                                      AssigneeName = _userService.GetUser(item.Assignee).Name,
                                      State = item.State,
                                      Lganame = item.Lga.Name,
                                      Lgaid = item.Id,
                                      AuthorizationStatus = item.AuthorizationStatus,
                                      AuthorizationNote = item.AuthorizationNote,
                                      AuthorizedBy = item.AuthorizedBy,
                                      Status = item.Status,
                                      Providerplans = planlist,
                                      Providerservices = servicelist,
                                      CreatedBy = singleOrDefault != null && item.CreatedBy != null ? singleOrDefault.Name : "--",
                                      Zone = _helpersvc.GetzonebyId(Convert.ToInt32(item.State.Zone)).Name,
                                      BankName = _helpersvc.Getbank(providerAccount.BankId).Name,
                                      CreatedDate = item.CreatedOn.ToString("dd MMM yyyy"),
                                      ProviderplansStr = item.Providerplans,
                                      ProviderservicesStr = item.Providerservices,
                                      AuthorizedDate = item.AuthorizedDate,
                                      AuthorizedByString = item.AuthorizedBy > 0 ? _userService.GetUser(item.AuthorizedBy).Name : "--",
                                      AuthorizationStatusStr = Enum.GetName(typeof(AuthorizationStatus), item.AuthorizationStatus),
                                      CategoryString = Enum.GetName(typeof(ProviderCategory), item.Category),
                                      category = item.Category,
                                      services = item.Servicesanddays
                                  });
                return response;
            }
            return new List<ProviderVm>();
        }
        public IList<ProviderVm> QueryallPendingProviderforJson(out int totalRecord, out int totalcountinresult, string search, int start, int lenght,
            string sortColumn, string sortOrder, string srcProviderName, string scrAddress, int state, int zone, bool useDate,
            DateTime scrFromDate, DateTime scrToDate, string scrUsers, int otherFilters, int plantype)



        {
            IQueryOver<Provider, Provider> query = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 1);
            List<ProviderVm> response = new List<ProviderVm>();
            if (!string.IsNullOrEmpty(srcProviderName))
            {

                srcProviderName = "%" + srcProviderName + "%";
                query.AndRestrictionOn(x => x.Name).IsInsensitiveLike(srcProviderName);

            }

            if (!string.IsNullOrEmpty(scrAddress))
            {
                scrAddress = "%" + scrAddress + "%";
                query.AndRestrictionOn(x => x.Address).IsInsensitiveLike(scrAddress);
            }

            if (state > 0)
            {
                query.Where(x => x.State.Id == state);
            }

            if (zone > 0)
            {
                query.Where(x => x.State.Zone == zone);

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

                query.Where(x => x.CreatedBy == scrUsers);


            }
            if (plantype > -1)
            {
                string planid = "%" + Convert.ToString(plantype) + "%";

                IQueryOver<Provider, Provider> query2 = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 1);
                query2.AndRestrictionOn(x => x.Providerplans).IsInsensitiveLike(planid);
                List<int> output = new List<int>();
                int count = query2.RowCount();
                for (int i = 0; i < count; i++)
                {

                    Provider item = query2.Skip(i).Take(1).SingleOrDefault();
                    string[] allplans = item.Providerplans.Split(',');
                    output.AddRange(from plan in allplans where plan.Trim() == Convert.ToString(plantype) select item.Id);


                }




                query.WhereRestrictionOn(bp => bp.Id)
         .IsIn(output);


            }

            //sort order

            //return normal list.
            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            IList<Provider> list = query.Skip(start).Take(lenght).List();


            if (list != null)
            {
                response.AddRange(from item in list
                                  let split = !string.IsNullOrEmpty(item.Providerservices) ? item.Providerservices.Split(',') : new string[] { }
                                  let split2 = !string.IsNullOrEmpty(item.Providerplans) ? item.Providerplans.Split(',') : new string[] { }
                                  let servicelist = split.Select(serv => _servicevc.GetService(Convert.ToInt32(serv)).Name).ToList()
                                  let planlist = split2.Select(plan => _plansvc.GetPlan(Convert.ToInt32(plan)).Name).ToList()
                                  let singleOrDefault = _userService.GetAllUsers().SingleOrDefault(x => x.Guid.ToString().ToLower() == Convert.ToString(item.CreatedBy).ToLower())
                                  where singleOrDefault != null
                                  let providerAccount = item.Provideraccount
                                  where providerAccount != null
                                  select new ProviderVm()
                                  {
                                      Id = item.Id,
                                      Name = item.Name.ToUpper(),
                                      Code = item.Code,
                                      SubCode = item.SubCode,
                                      Phone = item.Phone,
                                      Phone2 = item.Phone2,
                                      Email = item.Email,
                                      Website = item.Website,
                                      Address = item.Address,
                                      Provideraccount = providerAccount ?? new ProviderAccount(),
                                      Assignee = item.Assignee,
                                      AssigneeName = _userService.GetUser(item.Assignee).Name,
                                      State = item.State,
                                      Lganame = item.Lga.Name,
                                      Lgaid = item.Id,
                                      AuthorizationStatus = item.AuthorizationStatus,
                                      AuthorizationNote = item.AuthorizationNote,
                                      AuthorizedBy = item.AuthorizedBy,
                                      Status = item.Status,
                                      Providerplans = planlist,
                                      Providerservices = servicelist,
                                      CreatedBy = item.CreatedBy != null ? singleOrDefault.Name : "",
                                      Zone = _helpersvc.GetzonebyId(Convert.ToInt32(item.State.Zone)).Name,
                                      BankName = _helpersvc.Getbank(providerAccount.BankId).Name,
                                      CreatedDate = item.CreatedOn.ToString("dd MMM yyyy"),
                                      ProviderplansStr = item.Providerplans,
                                      ProviderservicesStr = item.Providerservices,
                                      AuthorizedDate = item.AuthorizedDate,
                                      AuthorizedByString = item.AuthorizedBy > 0 ? _userService.GetUser(item.AuthorizedBy).Name : "--",
                                      AuthorizationStatusStr = Enum.GetName(typeof(AuthorizationStatus), item.AuthorizationStatus)
                                  });
                return response;
            }
            return new List<ProviderVm>();
        }

        public bool AddnewProvider(Provider provider)
        {
            if (provider != null)
            {
                _session.Transact(session => session.Save(provider));
                _helpersvc.Log(LogEntryType.Audit, null,
                               string.Format(
                                   "New provider has been added to the system provider name {0} , provider id {1}, by {2}",
                                   provider.Name, provider.Id, provider.Assignee.ToString()), "Provider Added.");


                // send the notification to roles

                //var rolesinpage = _uniquePageService.GetUniquePage<Pages.ProviderApprovalPage>().FrontEndAllowedRoles;
                //foreach (var role in rolesinpage)
                //{
                //    //send notification for each guy in the ro;e
                //    _helpersvc.PushUserNotification(string.Empty, string.Format("New Provider Authorization Request for [{0}]  provider, created by {1}.", provider.Name, _userService.GetUser(provider.Assignee).Name),
                //                                                  NotificationTarget.Role, role,
                //                                                  NotificationType.Persistent, UniquePageHelper.GetUrl<Pages.ProviderApprovalPage>());

                //}
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool DeleteProvider(Provider provider)
        {
            if (provider != null)
            {
                //update the provider
                provider.IsDeleted = true;
                _session.Transact(session => session.Update(provider));
                _helpersvc.Log(LogEntryType.Audit, null, string.Format(
                  "Provider has been deleted provider name {0} , provider id {1}, by {2}",

                  provider.Name, provider.Id, CurrentRequestData.CurrentUser.Id), "Provider deleted.");
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateProvider(Provider provider)
        {
            if (provider != null)
            {

                _session.Transact(session => session.Update(provider));
                _helpersvc.Log(LogEntryType.Audit, null, string.Format(
                    "Provider has been updated provider name {0} , provider id {1}, by {2}",

                    provider.Name, provider.Id, CurrentRequestData.CurrentUser != null ? CurrentRequestData.CurrentUser.Id : 1), "Provider update.");

                return true;
            }
            else
            {
                return false;
            }
        }

        public Provider GetProvider(int id)
        {
            Provider provider = _session.QueryOver<Provider>().Where(x => x.Id == id).SingleOrDefault();
            return provider;
        }

        public Provider GetProviderByName(string name)
        {
            Provider provider = _session.QueryOver<Provider>().Where(x => x.Name == name.ToString()).Take(1).SingleOrDefault();
            return provider;
        }

        public ProviderVm GetProviderVm(int id)
        {
            Provider item = GetProvider(id);
            if (item != null)
            {
                string[] split = item.Providerservices != null ? item.Providerservices.Split(',') : new string[] { string.Empty };
                string[] split2 = item.Providerplans != null ? item.Providerplans.Split(',') : new string[] { string.Empty };
                string[] split3 = item.ProviderTariffs != null ? item.ProviderTariffs.Split(',') : new string[] { string.Empty };
                string[] split4 = item.CompanyConsession != null ? item.CompanyConsession.Split(',') : new string[] { string.Empty };
                List<string> planlist = new List<string>();
                List<string> servicelist = new List<string>();
                List<string> tarifflist = new List<string>();
                List<string> consessionslist = new List<string>();
                if (split.Any())
                {


                    foreach (string serv in split)
                    {
                        if (!string.IsNullOrEmpty(serv))
                        {
                            servicelist.Add(_servicevc.GetService(Convert.ToInt32(serv)).Name + ",");
                        }
                    }


                }



                if (split2.Any())
                {

                    foreach (string plan in split2)
                    {
                        if (!string.IsNullOrEmpty(plan))
                        {
                            planlist.Add(_plansvc.GetPlan(Convert.ToInt32(plan)).Name + ",");
                        }
                    }
                    // planlist = split2.Select(plan => _plansvc.GetPlan(Convert.ToInt32(plan)).Name + ",").ToList();
                }
                if (split3.Any())
                {


                    Tariff tempitem = null;

                    try
                    {
                        tarifflist = split3.Select(tariff => (tempitem = _tariffService.GetTariff(Convert.ToInt32(tariff))) != null ? tempitem.Name + "," : string.Empty).ToList();
                    }
                    catch (Exception)
                    {

                        //throw;
                    }


                }

                if (split4.Any())
                {


                    Company tempitem = null;

                    try
                    {
                        consessionslist = split4.Select(concess => (tempitem = _companyService.GetCompany(Convert.ToInt32(concess))) != null ? tempitem.Name + "," : string.Empty).ToList();
                    }
                    catch (Exception)
                    {

                        //throw;
                    }


                }
                if (servicelist.Any())
                {
                    string last = servicelist.Last();
                    servicelist.Remove(last);
                    try
                    {
                        servicelist.Add(last.Substring(0, last.Length - 1));
                    }
                    catch (Exception)
                    {

                    }
                }

                if (planlist.Any())
                {
                    string plast = planlist.Last();




                    planlist.Remove(plast);
                    try
                    {
                        planlist.Add(plast.Substring(0, plast.Length - 1));
                    }
                    catch (Exception)
                    {


                    }
                }








                try
                {
                    string tlast = tarifflist.Last();
                    tarifflist.Remove(tlast);
                    tarifflist.Add(tlast.Substring(0, tlast.Length - 1));
                }
                catch (Exception)
                {


                }

                try
                {
                    string tlast = consessionslist.Last();
                    consessionslist.Remove(tlast);
                    consessionslist.Add(tlast.Substring(0, tlast.Length - 1));
                }
                catch (Exception)
                {


                }

                MrCMS.Entities.People.User test = _userService.GetAllUsers().SingleOrDefault(x => x.Guid.ToString().ToLower() == item.CreatedBy.ToLower());
                MrCMS.Entities.People.User authby = item.AuthorizedBy > 0 ? _userService.GetUser(item.AuthorizedBy) : null;
                if (true)
                {
                    ProviderVm model = new ProviderVm()
                    {

                        Id = item.Id,
                        Name = item.Name.ToUpper(),
                        Code = item.Code,
                        SubCode = item.SubCode,
                        Phone = item.Phone,
                        Phone2 = item.Phone2,
                        Email = item.Email,
                        Website = item.Website,
                        Address = item.Address,
                        Provideraccount = item.Provideraccount,
                        Provideraccount2 = item.Provideraccount2,
                        Area = item.Area,
                        Assignee = item.Assignee,
                        AssigneeName = _userService.GetUser(item.Assignee).Name,
                        State = item.State,
                        Lganame = item.Lga.Name,
                        Lgaid = item.Lga.Id,
                        AuthorizationStatus = item.AuthorizationStatus,
                        AuthorizationNote = item.AuthorizationNote,
                        AuthorizedBy = item.AuthorizedBy,
                        Status = item.Status,
                        Providerplans = planlist,
                        Providerservices = servicelist,
                        ProviderTariffs = tarifflist,
                        consessionslist = consessionslist,
                        consessions = item.CompanyConsession,
                        CreatedBy =
                                item.CreatedBy != null && test != null
                                    ? test.
                                          Name
                                    : "--",
                        Zone = _helpersvc.GetzonebyId(Convert.ToInt32(item.State.Zone)).Name,
                        BankName = _helpersvc.Getbank(item.Provideraccount.BankId).Name,
                        BankName2 = item.Provideraccount2 != null ? _helpersvc.Getbank(item.Provideraccount2.BankId).Name : "--",
                        CreatedDate = item.CreatedOn.ToString("dd MMM yyyy"),
                        ProviderplansStr = item.Providerplans,
                        ProviderservicesStr = item.Providerservices,
                        ProvidertariffsStr = item.ProviderTariffs,
                        AuthorizedDate = item.AuthorizedDate,
                        category = item.Category,
                        isdelisted = item.isDelisted,
                        DelistNote = item.DelistNote,
                        delistedBy = item.delistedBy,
                        AuthorizedByString = item.AuthorizedBy > 0 && authby != null ? authby.Name : "--",
                        AuthorizationStatusStr = Enum.GetName(typeof(AuthorizationStatus), item.AuthorizationStatus)

                    };

                    return model;
                }
            }


            return null;
        }

        public IList<GenericReponse2> GetProviderNameList()
        {
            IQueryOver<Provider, Provider> query = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2 && x.isDelisted == false);
            List<GenericReponse2> response = new List<GenericReponse2>();
            foreach (Provider item in query.List())
            {
                GenericReponse2 itemo = new GenericReponse2
                {
                    Id = item.Id,
                    Name = item.Name + "-" + item.Address
                    //Name = item.Name + "-" + item.Address;
                };

                response.Add(itemo);

            }



            return response;

        }

        public IList<Provider> GetallProviderByPlan(int planType)
        {
            IQueryOver<Provider, Provider> query = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2 && x.isDelisted == false);
            if (planType > -1)
            {
                string planid = "%" + Convert.ToString(planType) + "%";

                IQueryOver<Provider, Provider> query2 = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2);
                query2.AndRestrictionOn(x => x.Providerplans).IsInsensitiveLike(planid);
                List<int> output = new List<int>();
                int count = query2.RowCount();
                for (int i = 0; i < count; i++)
                {

                    Provider item = query2.Skip(i).Take(1).SingleOrDefault();
                    string[] allplans = item.Providerplans.Split(',');
                    output.AddRange(from plan in allplans where plan.Trim() == Convert.ToString(planType) select item.Id);


                }
                query.WhereRestrictionOn(bp => bp.Id)
   .IsIn(output);


            }

            return query.List<Provider>();
        }

        public IList<ProviderReponse> GetProviderNameWithAddressList()
        {
            IQueryOver<Provider, Provider> query = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2 && x.isDelisted == false);
            List<ProviderReponse> response = new List<ProviderReponse>();
            //if (query != null){ 
            foreach (Provider item in query.List().OrderBy(x => x.Name))
            {
                ProviderReponse itemo = new ProviderReponse
                {
                    Id = item.Id,
                    Name = item.Name,
                    Address = item.Address,
                    State = item.State.Name,
                };

                response.Add(itemo);

            }
            // }



            return response;
        }

        public IList<Provider> GetallProviderByService(int serviceType)
        {
            IQueryOver<Provider, Provider> query = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2 && x.isDelisted == false);
            string serviceid = "%" + Convert.ToString(serviceType) + "%";

            IQueryOver<Provider, Provider> query2 = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2);
            query2.AndRestrictionOn(x => x.Providerservices).IsInsensitiveLike(serviceid);
            List<int> output = new List<int>();
            int count = query2.RowCount();
            for (int i = 0; i < count; i++)
            {

                Provider item = query2.Skip(i).Take(1).SingleOrDefault();
                string[] allplans = item.Providerservices.Split(',');
                output.AddRange(from plan in allplans where plan.Trim() == Convert.ToString(serviceType) select item.Id);


            }
            query.WhereRestrictionOn(bp => bp.Id)
.IsIn(output);


            return query.List<Provider>();
        }

        public long ProviderCount()
        {
            return _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2 && x.isDelisted == false).RowCount();
        }

        public bool addproviderFeedBack(ProviderRating rating)
        {
            if (rating != null)
            {
                _session.Transact(session => session.Save(rating));
                return true;

            }
            return false;
        }

        public IList<ProviderRating> GetProviderfeedbackList(int providerid)
        {
            IQueryOver<ProviderRating, ProviderRating> query = _session.QueryOver<ProviderRating>().Where(x => x.IsDeleted == false && x.providerID == providerid);
            return query.List();
        }

        public ProviderRating GetProviderfeedback(int feedbackid)
        {
            IQueryOver<ProviderRating, ProviderRating> query = _session.QueryOver<ProviderRating>().Where(x => x.IsDeleted == false && x.Id == feedbackid);
            return query.SingleOrDefault();
        }

        public int GetenrolleeusingproviderCount(int providerId)
        {
            return _session.QueryOver<Enrollee>().Where(x => x.Primaryprovider == providerId).RowCount();
        }

        public bool SetAltProvider(int provider, int altprovider)
        {
            ITransaction tx = _session.BeginTransaction();
            string hqlVersionedUpdate = "update Enrollee set Primaryprovider= :newValue where Primaryprovider = :oldValue";
            int updatedEntities = _session.CreateQuery(hqlVersionedUpdate)

                    .SetInt32("newValue", altprovider)
                    .SetInt32("oldValue", provider)
                    .ExecuteUpdate();
            tx.Commit();


            return true;
        }


        public IList<ProviderVm> QueryallDelistedProviderforJson(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcProviderName, string scrAddress, int state, int zone, bool useDate, DateTime scrFromDate, DateTime scrToDate, string scrUsers, int otherFilters, int plantype)
        {
            IQueryOver<Provider, Provider> query = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.isDelisted);
            List<ProviderVm> response = new List<ProviderVm>();
            if (!string.IsNullOrEmpty(srcProviderName))
            {

                srcProviderName = "%" + srcProviderName + "%";
                query.AndRestrictionOn(x => x.Name).IsInsensitiveLike(srcProviderName);

            }

            if (!string.IsNullOrEmpty(scrAddress))
            {
                scrAddress = "%" + scrAddress + "%";
                query.AndRestrictionOn(x => x.Address).IsInsensitiveLike(scrAddress);
            }

            if (state > 0)
            {
                query.Where(x => x.State.Id == state);
            }

            if (zone > 0)
            {
                query.Where(x => x.State.Zone == zone);

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

                query.Where(x => x.CreatedBy == scrUsers);


            }
            if (plantype > -1)
            {
                string planid = "%" + Convert.ToString(plantype) + "%";

                IQueryOver<Provider, Provider> query2 = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 1);
                query2.AndRestrictionOn(x => x.Providerplans).IsInsensitiveLike(planid);
                List<int> output = new List<int>();
                int count = query2.RowCount();
                for (int i = 0; i < count; i++)
                {

                    Provider item = query2.Skip(i).Take(1).SingleOrDefault();
                    string[] allplans = item.Providerplans.Split(',');
                    output.AddRange(from plan in allplans where plan.Trim() == Convert.ToString(plantype) select item.Id);


                }




                query.WhereRestrictionOn(bp => bp.Id)
         .IsIn(output);


            }

            //sort order

            //return normal list.
            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            IList<Provider> list = query.Skip(start).Take(lenght).List();


            if (list != null)
            {
                response.AddRange(from item in list
                                  let split = !string.IsNullOrEmpty(item.Providerservices) ? item.Providerservices.Split(',') : new string[] { }
                                  let split2 = !string.IsNullOrEmpty(item.Providerplans) ? item.Providerplans.Split(',') : new string[] { }
                                  let servicelist = split.Select(serv => _servicevc.GetService(Convert.ToInt32(serv)).Name).ToList()
                                  let planlist = split2.Select(plan => _plansvc.GetPlan(Convert.ToInt32(plan)).Name).ToList()
                                  let singleOrDefault = _userService.GetAllUsers().SingleOrDefault(x => x.Guid.ToString().ToLower() == Convert.ToString(item.CreatedBy).ToLower())
                                  where singleOrDefault != null
                                  let providerAccount = item.Provideraccount
                                  where providerAccount != null
                                  select new ProviderVm()
                                  {
                                      Id = item.Id,
                                      Name = item.Name.ToUpper(),
                                      Code = item.Code,
                                      SubCode = item.SubCode,
                                      Phone = item.Phone,
                                      Phone2 = item.Phone2,
                                      Email = item.Email,
                                      Website = item.Website,
                                      Address = item.Address,
                                      Provideraccount = providerAccount ?? new ProviderAccount(),
                                      Assignee = item.Assignee,
                                      AssigneeName = _userService.GetUser(item.Assignee).Name,
                                      State = item.State,
                                      Lganame = item.Lga.Name,
                                      Lgaid = item.Id,
                                      AuthorizationStatus = item.AuthorizationStatus,
                                      AuthorizationNote = item.AuthorizationNote,
                                      AuthorizedBy = item.AuthorizedBy,
                                      Status = item.Status,
                                      Providerplans = planlist,
                                      Providerservices = servicelist,
                                      CreatedBy = item.CreatedBy != null ? singleOrDefault.Name : "",
                                      Zone = _helpersvc.GetzonebyId(Convert.ToInt32(item.State.Zone)).Name,
                                      BankName = _helpersvc.Getbank(providerAccount.BankId).Name,
                                      CreatedDate = item.CreatedOn.ToString("dd MMM yyyy"),
                                      ProviderplansStr = item.Providerplans,
                                      ProviderservicesStr = item.Providerservices,
                                      AuthorizedDate = item.AuthorizedDate,
                                      AuthorizedByString = item.AuthorizedBy > 0 ? _userService.GetUser(item.AuthorizedBy).Name : "--",
                                      AuthorizationStatusStr = Enum.GetName(typeof(AuthorizationStatus), item.AuthorizationStatus)
                                  });
                return response;
            }
            return new List<ProviderVm>();
        }
    }
}