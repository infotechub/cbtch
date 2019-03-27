using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.MapperConfig;
using MrCMS.Web.Apps.Core.Models.Plan;
using MrCMS.Web.Apps.Core.Models.Services;
using MrCMS.Website;
using NHibernate;
using MrCMS.Services;
using System.Globalization;
using System.Text;
using MrCMS.Settings;
using MrCMS.Web.Apps.Core.Utility;
using NHibernate.Criterion;
using MrCMS.Entities.Messaging;
using MrCMS.DbConfiguration;

namespace MrCMS.Web.Apps.Core.Services
{


    public class ClaimService : IClaimService
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
        private readonly IProviderService _providersvc;
        private readonly ISystemConfigurationProvider _systemConfigurationProvider;
        private readonly IEmailSender _emailSender;
        private readonly MailSettings _mailSettings;
        //private readonly INotificationHubService _notificationHubService;
        public ClaimService(ISession session, IUserService userservice, IHelperService helpersvc, IServicesService servicesvc, IPlanService plansvc, IUniquePageService uniquepage, ITariffService tariffservice, IRoleService roleService, ISystemConfigurationProvider systemConfigurationProvider, IProviderService providerSvc, IEmailSender emailSender, MailSettings mailSettings)
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
            _providersvc = providerSvc;
            _emailSender = emailSender;
            _mailSettings = mailSettings;
            //_notificationHubService = notificationHub;
        }
        public bool DeleteIncomingClaim(IncomingClaims Claim)
        {
            if (Claim != null)
            {
                _session.Transact(session => session.Delete(Claim));
                return true;
            }
            else
            {
                return false;
            }


        }



        public bool ReceiveNewIncomingClaim(IncomingClaims claim)
        {

            if (claim != null)
            {
                _session.Transact(session => session.Save(claim));
                //claims Department
                //var notification = _helpersvc.GetNotificationTable(4);
                //send notification for each guy in the role to notify that a new staff was added.


                //_helpersvc.PushUserNotification(_userService.GetUser(claim.receivedBy).Guid.ToString().ToLower(), String.Format("New Claim [{1}] was received from {0} .", _providersvc.GetProvider(claim.providerid).Name, claim.Id), Utility.NotificationTarget.User, string.Empty, Utility.NotificationType.Persistent, UniquePageHelper.GetUrl<Pages.IncomingClaimsPage>());


                return true;
            }
            return false;
        }

        public bool UpdateIncomingClaim(IncomingClaims Claim)
        {
            if (Claim != null)
            {
                _session.Transact(session => session.Update(Claim));
                return true;
            }
            else
            {
                return false;
            }
        }

        public IList<IncomingClaims> QueryAllIncomingClaims(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, int scrProvider, int addedBy, string scrDeliveredBy, int month, int year, int transferedTo, bool useDate, DateTime scrFromDate, DateTime scrToDate, int otherFilters, int trackingid)
        {
            IQueryOver<IncomingClaims, IncomingClaims> query = _session.QueryOver<IncomingClaims>().Where(x => x.IsDeleted == false);

            //if(addedBy > -1)
            //{
            //    query.Where(x => x.receivedBy == addedBy);

            //}

            if (scrProvider > -1)
            {
                query.Where(x => x.providerid == scrProvider);
            }
            //added else
            else
            {


            }





            if (!string.IsNullOrEmpty(scrDeliveredBy))
            {
                query.Where(x => x.deliveredby == scrDeliveredBy.ToUpper() && x.deliveredby == scrDeliveredBy.ToLower());
            }
            else
            {

            }

            if (useDate)
            {
                DateTime datete = Convert.ToDateTime(scrToDate);

                int dd = datete.Day;
                int dmonth = datete.Month;
                int dyear = datete.Year;

                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", dmonth, dd, dyear, time));
                query.Where(Restrictions.On<Enrollee>(a => a.CreatedOn).IsBetween(scrFromDate).And(enddate));
            }

            else
            {

            }

            if (year > -1 && trackingid < 0)
            {
                query.Where(x => x.year == year);


            }
            else
            {

            }
            if (month > -1)
            {
                //query.Where(x => x.month == month);

                string monthstr = month.ToString();
                query.WhereRestrictionOn(x => x.month_string).IsInsensitiveLike(monthstr, MatchMode.Anywhere);
                //if(query.WhereRestrictionOn(x => x.month_string).IsInsensitiveLike(",", MatchMode.Anywhere).RowCount() > 0)
                //{
                //    //contains the coma guys


                //   foreach(var item in )
                //    var states = (List<int>)query.WhereRestrictionOn(x => x.month_string).IsInsensitiveLike(",", MatchMode.Anywhere).SelectList(a => a.Select(p => p.Id)).List<int>();

                //}

            }
            //added else
            else
            {

            }


            if (trackingid > -1)
            {

                IList<Entities.ClaimBatch> claimlist = _session.QueryOver<Entities.ClaimBatch>().Where(v => v.Id == trackingid).List<Entities.ClaimBatch>();

                List<int> intlist = new List<int>();
                if (claimlist.Any())
                {
                    IncomingClaims first = claimlist.FirstOrDefault().IncomingClaims.FirstOrDefault();

                    if (first != null)
                    {
                        intlist.Add(first.Id);

                    }
                    //added else
                    else
                    {

                    }
                }
                query.WhereRestrictionOn(x => x.Id).IsIn(intlist);

            }

            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            IList<IncomingClaims> list = query.OrderBy(x => x.CreatedOn).Desc.Skip(start).Take(lenght).List();


            return list;
        }

        public IncomingClaims GetIncomingClaim(int Id)
        {
            return _session.QueryOver<IncomingClaims>().Where(x => x.IsDeleted == false && x.Id == Id).SingleOrDefault();
        }

        public IList<TariffGenericReponse> GetServiceTariff(int Id, string phrase)
        {

            //var groups = _session.QueryOver<TariffCategory>().Where(x => x.TariffId == Id && x.IsDeleted == false).List<TariffCategory>();
            IList<int> idlist = _session.QueryOver<TariffCategory>().Where(x => x.TariffId == Id && x.IsDeleted == false && x.Type == 1).SelectList(list => list
           .Select(p => p.Id)
     ).List<int>();

            List<int> output = new List<int>();
            //added ?? new List<int>
            foreach (int item in idlist ?? new List<int>())
            {
                output.Add(item);
            }
            IQueryOver<ServiceTariff, ServiceTariff> query = _session.QueryOver<ServiceTariff>().WhereRestrictionOn(bp => bp.GroupId)
                   .IsIn(output);
            if (!string.IsNullOrEmpty(phrase))
            {
                phrase = phrase + "%";
                query.WhereRestrictionOn(x => x.Name).IsInsensitiveLike(phrase);

            }

            //added else return null
            else
            {
                return null;
            }

            List<TariffGenericReponse> outputlistobj = new List<TariffGenericReponse>();
            foreach (ServiceTariff item in query.List<ServiceTariff>())
            {
                TariffGenericReponse itemo = new TariffGenericReponse()
                {
                    Id = item.Id.ToString(),
                    Name = item.Name,
                    Price = item.Price.ToString("N"),
                };
                outputlistobj.Add(itemo);
            }

            return outputlistobj;
        }
        public IList<TariffGenericReponse> GetDrugTariff(int Id, string phrase)
        {

            //var groups = _session.QueryOver<TariffCategory>().Where(x => x.TariffId == Id && x.IsDeleted == false).List<TariffCategory>();
            IList<int> idlist = _session.QueryOver<TariffCategory>().Where(x => x.TariffId == Id && x.IsDeleted == false && x.Type == 0).SelectList(list => list
           .Select(p => p.Id)
     ).List<int>();

            List<int> output = new List<int>();

            //added ?? new List<int>()
            foreach (int item in idlist ?? new List<int>())
            {
                output.Add(item);
            }
            IQueryOver<DrugTariff, DrugTariff> query = _session.QueryOver<DrugTariff>().WhereRestrictionOn(bp => bp.GroupId)
                      .IsIn(output);

            if (!string.IsNullOrEmpty(phrase))
            {
                phrase = phrase + "%";
                query.WhereRestrictionOn(x => x.Name).IsInsensitiveLike(phrase);

            }
            else
            {
                return null;
            }





            List<TariffGenericReponse> outputlistobj = new List<TariffGenericReponse>();

            foreach (DrugTariff item in query.List<DrugTariff>())
            {
                TariffGenericReponse itemo = new TariffGenericReponse()
                {
                    Id = item.Id.ToString(),
                    Name = item.Name,
                    Price = item.Price.ToString("N")
                };
                outputlistobj.Add(itemo);
            }

            return outputlistobj;
        }

        public bool AddClaim(Claim claim)
        {
            if (claim != null)
            {
                _session.Transact(session => session.Save(claim));
                return true;
            }
            return false;
        }

        public bool UpdateClaim(Claim claim)
        {
            if (claim != null)
            {
                _session.Transact(session => session.Update(claim));
                return true;
            }
            return false;
        }

        public bool DeleteClaim(Claim claim)
        {

            foreach (ClaimDrug item in claim.DrugList)
            {
                _session.Transact(session => session.Delete(item));
            }
            foreach (Entities.ClaimService item in claim.ServiceList)
            {
                _session.Transact(session => session.Delete(item));
            }
            if (claim != null)
            {
                _session.Transact(session => session.Delete(claim));
                return true;
            }
            return false;
        }

        public Claim GetClaim(int Id)
        {
            return _session.QueryOver<Claim>().Where(x => x.IsDeleted == false && x.Id == Id).SingleOrDefault();

        }

        public IList<IncomingClaims> QueryAllClaims(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, int Provider, int enrolleeID, string enrolleePolicynumber, int Batch, int Year, int Month, string Diagnosis, int capturedBy, int vettedBy, int ClaimsTag, string otherfilter, int claimsstatus)
        {

            totalcountinresult = 3;
            totalRecord = 0;


            return new List<IncomingClaims>();
        }

        public bool AddClaimBatch(Entities.ClaimBatch batch)
        {
            if (batch != null)
            {
                _session.Transact(session => session.SaveOrUpdate(batch));
                return true;
            }
            return false;

        }

        public bool UpdateClaimBatch(Entities.ClaimBatch batch)
        {
            if (batch != null)
            {
                _session.Transact(session => session.Update(batch));
                _session.Flush();

                return true;
            }
            return false;

        }

        public bool DeleteClaimBatch(Entities.ClaimBatch batch)
        {
            if (batch != null)
            {
                _session.Transact(session => session.Delete(batch));
                return true;
            }
            return false;
        }

        public Entities.ClaimBatch GetClaimBatch(int Id)
        {
            return _session.QueryOver<Entities.ClaimBatch>().Where(x => x.IsDeleted == false && x.Id == Id).SingleOrDefault();
        }

        public Entities.ClaimBatch GetBatchForProvider(int ProviderID, DateTime DateReceived)
        {

            Utility.ClaimBatch batch = Tools.CheckClaimBatch(DateReceived);
            return _session.QueryOver<Entities.ClaimBatch>().Where(x => x.IsDeleted == false && x.ProviderId == ProviderID && x.Batch == batch && x.month == DateReceived.Month && x.year == DateReceived.Year).SingleOrDefault();
        }

        public IList<Entities.ClaimBatch> QueryAllClaimBatch(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, int scrProvider, int month, int year, Utility.ClaimBatch Batch, string zone, int otherFilters, ClaimBatchStatus status, out int noofprovider, out decimal totalamountcaptured, out decimal totalproccessed, int remoteonly, int batchid)
        {


            IQueryOver<Entities.ClaimBatch, Entities.ClaimBatch> query = _session.QueryOver<Entities.ClaimBatch>().Where(x => x.IsDeleted == false);



            if (batchid > 0)
            {
                query.Where(x => x.Id == batchid);

            }
            else
            {
                if (Batch != Utility.ClaimBatch.All)
                {
                    query.Where(x => x.Batch == Batch);
                }
                if (status == ClaimBatchStatus.Default)
                {
                    query.Where(x => x.status == status || x.status == ClaimBatchStatus.Capturing);
                }
                else
                {

                    if (status == ClaimBatchStatus.Vetting)
                    {
                        //get the capturing
                        query.Where(x => x.status == status);
                    }
                    else if (status == ClaimBatchStatus.ReviewingANDAwaitingApproval)
                    {
                        query.Where(x => x.status == ClaimBatchStatus.AwaitingApproval || status == ClaimBatchStatus.Reviewing);
                    }
                    else if (status == ClaimBatchStatus.All)
                    {
                        //dont add this shit if its all

                        //do nothing bro.
                    }
                    else
                    {
                        query.Where(x => x.status == status);
                    }




                }
                if (month > -1)
                {
                    query.Where(x => x.month == month);
                }


                if (year > -1)
                {
                    query.Where(x => x.year == year);
                }

                if (scrProvider > -1)
                {
                    query.Where(x => x.ProviderId == scrProvider);

                }
                //query.Where(x=>x.)


                if (remoteonly == 2)
                {
                    if (status == ClaimBatchStatus.Vetting)
                    {
                        query.Where(x => x.isremote);
                    }
                }
                else if (remoteonly == 3)
                {
                    if (status == ClaimBatchStatus.Vetting)
                    {
                        query.Where(x => x.isremote == false);
                    }
                }




                if (!string.IsNullOrEmpty(zone))
                {
                    List<int> splitted = new List<int>();

                    foreach (string item in zone.Split(','))
                    {
                        int intt = -1;
                        int.TryParse(item, out intt);

                        if (intt > -1)
                        {
                            splitted.Add(intt);
                        }
                        //added else
                        else
                        {

                        }
                    }


                    if (splitted.Any())
                    {
                        List<int> states = (List<int>)_session.QueryOver<State>().WhereRestrictionOn(x => x.Zone).IsIn(splitted).SelectList(a => a.Select(p => p.Id)).List<int>();


                        //var ids = new List<int> { 1, 2, 5, 7 };
                        //var query2 = _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.AuthorizationStatus == 2 );
                        List<int> providers = (List<int>)_session.QueryOver<Provider>().WhereRestrictionOn(w => w.State).IsIn(states).SelectList(a => a.Select(p => p.Id)).List<int>();
                        query
         .WhereRestrictionOn(w => w.ProviderId).IsIn(providers);
                    }
                    //added else
                    else
                    {

                    }





                }
            }



            totalRecord = query.RowCount();
            noofprovider = totalRecord;


            totalamountcaptured = 0m;
            totalproccessed = 0m;



            IList<Entities.ClaimBatch> buffer = query.List();

            foreach (Entities.ClaimBatch item in buffer)
            {
                foreach (Claim claim in item.Claims)
                {
                    totalamountcaptured = totalamountcaptured + Convert.ToDecimal(claim.DrugList.Sum(x => x.InitialAmount) + claim.ServiceList.Sum(x => x.InitialAmount));
                    totalproccessed = totalproccessed + Convert.ToDecimal(claim.DrugList.Sum(x => x.VettedAmount) + claim.ServiceList.Sum(x => x.VettedAmount));

                }

            }



            totalcountinresult = totalRecord;
            //var list = new List<Entities.ClaimBatch>();


            if (status == ClaimBatchStatus.Vetting)
            {
                return query.OrderBy(x => x.VetDate).Asc.Skip(start).Take(lenght).List();
            }
            else
            {
                return query.OrderBy(x => x.CreatedOn).Desc.Skip(start).Take(lenght).List();
            }


            //return list;
        }

        public bool CleanClaim(Claim Claim)
        {
            if (Claim != null)
            {
                foreach (ClaimDrug item in Claim.DrugList)
                {
                    _session.Transact(session => session.Delete(item));
                }
                foreach (Entities.ClaimService item in Claim.ServiceList)
                {
                    _session.Transact(session => session.Delete(item));
                }

                UpdateClaim(Claim);
                return true;
            }

            return false;
        }

        public bool addAuthorization(AuthorizationCode authorizaction)
        {

            if (authorizaction != null)
            {
                _session.Transact(session => session.SaveOrUpdate(authorizaction));

                MrCMS.Entities.People.User usergenerated = _userService.GetUser(authorizaction.generatedby);
                MrCMS.Entities.People.User userauth = _userService.GetUser(authorizaction.Authorizedby);
                Provider provider = _providersvc.GetProvider(authorizaction.provider);


                QueuedMessage emailmsg = new QueuedMessage();
                emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                emailmsg.ToAddress = userauth.Email;
                emailmsg.Subject = "NovoHub Authorization Code Generated ";
                emailmsg.FromName = "NOVOHUB";



                emailmsg.Body = string.Format("An authorization code {5} was generated for {0} by {1} and was authorized by you on {2} for {4} with policy number {3}", provider.Name.ToUpper(), usergenerated.Name.ToUpper(), Convert.ToDateTime(authorizaction.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern), authorizaction.policyNumber, authorizaction.enrolleeName, authorizaction.authorizationCode);

                //_emailSender.AddToQueue(emailmsg);

                //send admission shit
                if (authorizaction.Isadmission)
                {
                    StringBuilder bodyText = new StringBuilder();
                    bodyText.Append("<p>Dear Admin,</p>");
                    bodyText.AppendLine(string.Format("<p>A new admission was approved with code {5} for {0} and generated by {1} and was authorized by {6} on {2} for {4} with policy number {3} </p>", provider.Name.ToUpper(), usergenerated.Name.ToUpper(), Convert.ToDateTime(authorizaction.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern), authorizaction.policyNumber, authorizaction.enrolleeName, authorizaction.authorizationCode, userauth.Name.ToUpper()));
                    bodyText.AppendLine(Environment.NewLine);
                    bodyText.AppendLine("<p>You are required to follow up on the admission.</p>");
                    bodyText.AppendLine("<p>Thank You.</p>");

                    //send to client service.
                    MrCMS.Entities.People.UserRole role = _roleSvc.GetRoleByName("CLIENT SERVICE");
                    if (role != null)
                    {
                        foreach (MrCMS.Entities.People.User user in role.Users)
                        {
                            //each user

                            QueuedMessage emailmsgAdd = new QueuedMessage();
                            emailmsgAdd.FromAddress = _mailSettings.SystemEmailAddress;
                            emailmsgAdd.ToAddress = user.Email;
                            emailmsgAdd.Subject = "NovoHub -New Admission ";
                            emailmsgAdd.FromName = "NOVOHUB";
                            emailmsgAdd.Body = bodyText.ToString();
                            emailmsgAdd.IsHtml = true;

                            //_emailSender.AddToQueue(emailmsgAdd);
                        }
                    }
                    //added else
                    else
                    {

                    }
                    //send to provider service.
                    role = _roleSvc.GetRoleByName("PROVIDER");
                    if (role != null)
                    {
                        foreach (MrCMS.Entities.People.User user in role.Users)
                        {
                            //each user

                            QueuedMessage emailmsgAdd = new QueuedMessage();
                            emailmsgAdd.FromAddress = _mailSettings.SystemEmailAddress;
                            emailmsgAdd.ToAddress = user.Email;
                            emailmsgAdd.Subject = "NovoHub -New Admission ";
                            emailmsgAdd.FromName = "NOVOHUB";
                            emailmsgAdd.Body = bodyText.ToString();
                            emailmsgAdd.IsHtml = true;
                            // _emailSender.AddToQueue(emailmsgAdd);
                        }
                    }
                    //added else
                    else
                    {

                    }


                    //send to medical service.
                    role = _roleSvc.GetRoleByName("MEDICAL UNIT");
                    if (role != null)
                    {
                        foreach (MrCMS.Entities.People.User user in role.Users)
                        {
                            //each user

                            QueuedMessage emailmsgAdd = new QueuedMessage();
                            emailmsgAdd.FromAddress = _mailSettings.SystemEmailAddress;
                            emailmsgAdd.ToAddress = user.Email;
                            emailmsgAdd.Subject = "NovoHub -New Admission ";
                            emailmsgAdd.FromName = "NOVOHUB";
                            emailmsgAdd.Body = bodyText.ToString();
                            emailmsgAdd.IsHtml = true;
                            // _emailSender.AddToQueue(emailmsgAdd);
                        }
                    }
                    //added else
                    else
                    {

                    }
                }


                //Notify the theres a new verificationCode
                AuthenticationCodeCreatedArgs args = new AuthenticationCodeCreatedArgs
                {
                    AuthorizationCode = authorizaction
                };
                //Notify the Hub of the new Input
                EventContext.Instance.Publish(typeof(INewNotificationEvent), args);

                return true;
            }
            return false;
        }

        public bool updateAuthorization(AuthorizationCode authorization)
        {
            if (authorization != null)
            {
                _session.Transact(session => session.Update(authorization));
                return true;
            }
            return false;
        }

        public AuthorizationCode getAuthorization(int id)
        {
            return _session.QueryOver<AuthorizationCode>().Where(x => x.IsDeleted == false && x.Id == id).SingleOrDefault();
        }

        public bool deleteAuthorization(AuthorizationCode authorization)
        {
            if (authorization != null)
            {
                _session.Transact(session => session.Delete(authorization));
                return true;
            }
            return false;
        }

        public AuthorizationCode getAuthorizationByCode(string Authorization)
        {
            IList<AuthorizationCode> itemm = _session.QueryOver<AuthorizationCode>().Where(x => x.IsDeleted == false && x.authorizationCode == Authorization).Take(1).List();

            return itemm.FirstOrDefault();

        }

        public IList<AuthorizationCode> QueryAuthorization(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, int scrProvider, int addedBy, int authorizedby, string Policynumber, int companyid, bool useDate, DateTime scrFromDate, DateTime scrToDate, int otherFilters, int opmode)
        {
            IQueryOver<AuthorizationCode, AuthorizationCode> query = _session.QueryOver<AuthorizationCode>().Where(x => x.IsDeleted == false);


            if (useDate)
            {
                DateTime datete = Convert.ToDateTime(scrToDate);
                int dd = datete.Day;
                int month = datete.Month;
                int year = datete.Year;
                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", month, dd, year, time));
                if (opmode > 0)
                {
                    query.Where(Restrictions.On<AuthorizationCode>(a => a.AdmissionDate).IsBetween(scrFromDate).And(enddate));
                }
                else
                {
                    query.Where(Restrictions.On<AuthorizationCode>(a => a.CreatedOn).IsBetween(scrFromDate).And(enddate));
                }

            }


            if (scrProvider > 0)
            {
                query.Where(x => x.provider == scrProvider);
            }

            if (addedBy > 0)
            {
                query.Where(x => x.generatedby == addedBy);
            }
            if (authorizedby > 0)
            {
                query.Where(x => x.Authorizedby == authorizedby);
            }

            if (!string.IsNullOrEmpty(Policynumber))
            {
                query.Where(x => x.policyNumber == Policynumber);
            }

            if (companyid > 0)
            {
                query.Where(x => x.EnrolleeCompany == Convert.ToString(companyid));
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = "%" + search + "%";
                query.WhereRestrictionOn(x => x.authorizationCode).IsInsensitiveLike(search);


            }
            if (opmode > 0)
            {
                query.Where(x => x.Isadmission);
            }



            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            IList<AuthorizationCode> list = query.OrderBy(x => x.Id).Desc.Skip(start).Take(lenght).List();


            return list;
        }



        public bool CheckClaimByClientID(string Id)
        {
            return _session.QueryOver<Claim>().Where(x => x.IsDeleted == false && x.ClientsideID == Id).RowCount() > 0;
        }

        public IList<VettingProtocol> GetallVettingProtocol()
        {
            IQueryOver<VettingProtocol, VettingProtocol> query = _session.QueryOver<VettingProtocol>().Where(x => x.IsDeleted == false);

            return query.List();
        }

        public VettingProtocol getVettingPRotocol(int id)
        {
            IQueryOver<VettingProtocol, VettingProtocol> query = _session.QueryOver<VettingProtocol>().Where(x => x.IsDeleted == false && x.Id == id);
            return query.SingleOrDefault();
        }

        public bool addVettingPRotocol(VettingProtocol obj)
        {
            if (obj != null)
            {
                _session.Transact(session => session.SaveOrUpdate(obj));
                return true;
            }
            return false;
        }

        public IList<AuthorizationCode> getAuthorizationByPolicyNumber(string policynumber, DateTime startdate, DateTime enddate)
        {
            IList<AuthorizationCode> all = _session.QueryOver<AuthorizationCode>().WhereRestrictionOn(x => x.policyNumber).IsInsensitiveLike(policynumber).Where(y => y.CreatedOn >= startdate).Where(z => z.CreatedOn <= enddate).List<AuthorizationCode>();

            return all.ToList();


        }

        public bool addClaimHistory(ClaimHistory history)
        {
            if (history != null)
            {
                _session.Transact(session => session.SaveOrUpdate(history));
                return true;
            }
            return false; ;
        }

        public bool deleteClaimHistory(ClaimHistory history)
        {
            if (history != null)
            {
                _session.Transact(session => session.Delete(history));
                return true;
            }
            return false; ;
        }

        public ClaimHistory GetClaimHistory(int Id)
        {
            ClaimHistory all = _session.QueryOver<ClaimHistory>().Where(x => x.Id == Id).SingleOrDefault();

            return all;
        }

        public IList<ClaimHistory> GetClaimHistoryByPolicyNumber(string policynumber, DateTime Start, DateTime end)
        {
            IList<ClaimHistory> all = NewMethod(policynumber, Start, end);
            IList<Claim> allfromclaims = _session.QueryOver<Claim>().WhereRestrictionOn(x => x.enrolleePolicyNumber).IsInsensitiveLike(policynumber, MatchMode.End).Where(y => y.ServiceDate >= Start).Where(z => z.ServiceDate <= end).List<Claim>();

            //convert the claimhistory to shiit
            List<ClaimHistory> collection = new List<ClaimHistory>();
            foreach (Claim item in allfromclaims)
            {
                ClaimHistory nobj = new ClaimHistory();
                Provider provider = _providersvc.GetProvider(item.ProviderId);
                nobj.PROVIDER = provider != null ? provider.Name.ToUpper() : "Unknown";
                nobj.PROVIDERID = provider != null ? provider.Id : -1;
                nobj.POLICYNUMBER = item.enrolleePolicyNumber;
                nobj.ENCOUNTERDATE = Convert.ToDateTime(item.ServiceDate);
                nobj.DATERECEIVED = item.CreatedOn;
                nobj.DIAGNOSIS = item.Diagnosis.ToUpper();
                nobj.AMOUNTSUBMITTED = Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount) + item.ServiceList.Sum(x => x.InitialAmount));
                nobj.AMOUNTPROCESSED = Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount) + item.ServiceList.Sum(x => x.VettedAmount));
                nobj.CLASS = Enum.GetName(typeof(ClaimsTAGS), item.Tag);
                nobj.CLIENTNAME = item.enrolleeFullname;

                //added ? after collection, collection and all

                collection?.Add(nobj);

            }
            collection?.AddRange(all?.ToList());

            return collection;
        }

        private IList<ClaimHistory> NewMethod(string policynumber, DateTime Start, DateTime end)
        {
            return _session.QueryOver<ClaimHistory>().WhereRestrictionOn(x => x.POLICYNUMBER).IsInsensitiveLike(policynumber, MatchMode.End).Where(y => y.ENCOUNTERDATE >= Start).Where(z => z.ENCOUNTERDATE <= end).List<ClaimHistory>();
        }

        public bool deleteClaimHistory(int id)
        {
            throw new NotImplementedException();
        }

        public int MaxClaimHistory()
        {
            return _session.QueryOver<ClaimHistory>().RowCount();
        }

        public long GetNumberofAdmission(DateTime start, DateTime End, ref Dictionary<int, int> theplan)
        {
            IQueryOver<AuthorizationCode, AuthorizationCode> query = _session.QueryOver<AuthorizationCode>().Where(x => x.IsDeleted == false && x.Isadmission == true);

            IList<Plan> genericlist = _session.QueryOver<Plan>().Where(x => x.IsDeleted == false).List();
            //var placount = new Dictionary<int, int>();


            foreach (Plan item in genericlist)
            {

                theplan.Add(item.Id, 0);
            }

            if (true)
            {
                DateTime datete = Convert.ToDateTime(End);

                int dd = datete.Day;
                int month = datete.Month;
                int year = datete.Year;

                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", month, dd, year, time));
                query.Where(Restrictions.On<AuthorizationCode>(a => a.AdmissionDate).IsBetween(start).And(enddate));
            }
            //added else
            else
            {

            }

            IList<AuthorizationCode> totaladd = query.List();

            foreach (AuthorizationCode item in totaladd)
            {
                Enrollee enrollee = _session.QueryOver<Enrollee>().Where(x => x.IsDeleted == false && x.Id == item.enrolleeID).SingleOrDefault();
                if (enrollee != null)
                {
                    Staff staff = _session.QueryOver<Staff>().Where(x => x.Id == enrollee.Staffprofileid).SingleOrDefault();
                    if (staff != null)
                    {
                        CompanyPlan plan = _session.QueryOver<CompanyPlan>().Where(x => x.Id == staff.StaffPlanid).SingleOrDefault();

                        if (plan != null)
                        {
                            int count = theplan[plan.Planid];
                            count++;
                            theplan[plan.Planid] = count;

                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                }
                //added else
                else
                {

                }
            }


            return query.RowCount();
        }

        public IDictionary<string, int> GetutilizationReport(DateTime start, DateTime End)
        {
            IQueryOver<AuthorizationCode, AuthorizationCode> query = _session.QueryOver<AuthorizationCode>().Where(x => x.IsDeleted == false);


            if (true)
            {
                DateTime datete = Convert.ToDateTime(End);

                int dd = datete.Day;
                int month = datete.Month;
                int year = datete.Year;

                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", month, dd, year, time));
                query.Where(Restrictions.On<AuthorizationCode>(a => a.CreatedOn).IsBetween(start).And(enddate));
            }
            //added else
            else
            {

            }

            IOrderedEnumerable<AuthorizationCode> list = query.List<AuthorizationCode>().OrderBy(x => x.EnrolleeCompany);
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (AuthorizationCode itemm in list)
            {

                if (!string.IsNullOrEmpty(itemm.EnrolleeCompany))
                {
                    if (result.ContainsKey(itemm.EnrolleeCompany))
                    {
                        int count = result[itemm.EnrolleeCompany];
                        count++;
                        result[itemm.EnrolleeCompany] = count;
                    }
                    else
                    {
                        result.Add(itemm.EnrolleeCompany, 1);

                    }

                }
                //added else return null
                else
                {
                    return null;
                }

            }

            return result;



        }

        public bool deleteVettingProtocol(VettingProtocol vetprotocol)
        {
            if (vetprotocol != null)
            {
                _session.Transact(session => session.Delete(vetprotocol));
                return true;
            }

            return false;
        }

        public bool UpdateVettingProtocol(VettingProtocol vetprotocol)
        {
            if (vetprotocol != null)
            {
                _session.Transact(session => session.Update(vetprotocol));
                return true;
            }
            return false;
        }

        public IList<AuthorizationCode> getBirthAuthorization()
        {
            IQueryOver<AuthorizationCode, AuthorizationCode> query = _session.QueryOver<AuthorizationCode>().Where(x => x.IsDeleted == false && x.isdelivery == true && x.deliverysmssent == false);

            return query.List<AuthorizationCode>();
        }

        public IDictionary<string, int> GetProviderutilizationReport(DateTime start, DateTime End)
        {
            IQueryOver<AuthorizationCode, AuthorizationCode> query = _session.QueryOver<AuthorizationCode>().Where(x => x.IsDeleted == false);


            if (true)
            {
                DateTime datete = Convert.ToDateTime(End);

                int dd = datete.Day;
                int month = datete.Month;
                int year = datete.Year;

                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", month, dd, year, time));
                query.Where(Restrictions.On<AuthorizationCode>(a => a.CreatedOn).IsBetween(start).And(enddate));
            }

            IOrderedEnumerable<AuthorizationCode> list = query.List<AuthorizationCode>().OrderBy(x => x.provider);
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (AuthorizationCode itemm in list)
            {

                if (itemm.provider > 0)
                {
                    if (result.ContainsKey(itemm.provider.ToString()))
                    {
                        int count = result[itemm.provider.ToString()];
                        count++;
                        result[itemm.provider.ToString()] = count;
                    }
                    else
                    {
                        result.Add(itemm.provider.ToString(), 1);

                    }

                }

            }

            return result;

        }

        public bool addPaymentBatch(PaymentBatch paymentbatch)
        {
            if (paymentbatch != null)
            {
                _session.Transact(session => session.SaveOrUpdate(paymentbatch));
                return true;
            }
            return false; ;
        }

        public bool deletepaymentbatch(PaymentBatch paymentbatch)
        {
            if (paymentbatch != null)
            {
                _session.Transact(session => session.Delete(paymentbatch));
                return true;
            }
            return false; ;
        }

        public PaymentBatch getpaymentbatch(int id)
        {
            return _session.QueryOver<PaymentBatch>().Where(x => x.Id == id).SingleOrDefault();
        }

        public IList<PaymentBatch> Queryallpaymentbatch(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string Title, bool usestatus, PaymentStatus paymentstatus, bool useDate, DateTime scrFromDate, DateTime scrToDate)
        {
            IQueryOver<PaymentBatch, PaymentBatch> query = _session.QueryOver<PaymentBatch>().Where(x => x.IsDeleted == false);


            if (!string.IsNullOrEmpty(Title))
            {
                Title = "%" + Title + "%";
                query.AndRestrictionOn(x => x.Title).IsInsensitiveLike(Title, MatchMode.Anywhere);

            }

            if (usestatus)
            {
                query.Where(x => x.status == paymentstatus);

            }
            if (useDate)
            {
                DateTime datete = Convert.ToDateTime(scrToDate);

                int dd = datete.Day;
                int dmonth = datete.Month;
                int dyear = datete.Year;

                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", dmonth, dd, dyear, time));
                query.Where(Restrictions.On<Enrollee>(a => a.CreatedOn).IsBetween(scrFromDate).And(enddate));
            }


            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            IList<PaymentBatch> list = query.OrderBy(x => x.CreatedOn).Desc.Skip(start).Take(lenght).List();


            return list;
        }

        public IList<PaymentBatch> getrecentpaymentbatch()
        {
            IQueryOver<PaymentBatch, PaymentBatch> query = _session.QueryOver<PaymentBatch>().Where(x => x.IsDeleted == false && x.status == PaymentStatus.Default || x.status == PaymentStatus.Pending).OrderBy(x => x.CreatedOn).Desc();

            return query.Take(1000).List();


        }

        public bool addAuthRequest(AuthorizationRequest request)
        {

            bool isnew = request.Id < 1 ? true : false;
            if (request != null)
            {
                _session.Transact(session => session.SaveOrUpdate(request));


                if (isnew)
                {
                    //Notify the theres a new verificationCode
                    AuthRequestCodeCreatedArgs args = new AuthRequestCodeCreatedArgs
                    {
                        AuthRequest = request
                    };
                    //Notify the Hub of the new Input
                    EventContext.Instance.Publish(typeof(INewNotificationEvent), args);
                }

                return true;
            }
            return false;
        }

        public bool deleteAuthRequest(AuthorizationRequest request)
        {
            if (request != null)
            {
                _session.Transact(session => session.Delete(request));
                return true;
            }
            return false;
        }

        public AuthorizationRequest GetAuthRequest(int Id)
        {
            return _session.QueryOver<AuthorizationRequest>().Where(x => x.Id == Id).SingleOrDefault();
        }

        public IList<AuthorizationRequest> QueryallAuthRequest(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string Search, bool useDate, DateTime scrFromDate, DateTime scrToDate)
        {
            IQueryOver<AuthorizationRequest, AuthorizationRequest> query = _session.QueryOver<AuthorizationRequest>().Where(x => x.IsDeleted == false);


            if (!string.IsNullOrEmpty(Search))
            {
                Search = "%" + Search + "%";
                query.AndRestrictionOn(x => x.fullname).IsInsensitiveLike(search, MatchMode.Anywhere);

            }

            if (useDate)
            {
                DateTime datete = Convert.ToDateTime(scrToDate);

                int dd = datete.Day;
                int dmonth = datete.Month;
                int dyear = datete.Year;

                string time = "23:59";
                DateTime enddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", dmonth, dd, dyear, time));
                query.Where(Restrictions.On<AuthorizationRequest>(a => a.CreatedOn).IsBetween(scrFromDate).And(enddate));
            }


            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            IList<AuthorizationRequest> list = query.OrderBy(x => x.CreatedOn).Desc.Skip(start).Take(lenght).List();


            return list;

        }

        public List<string> getAllDeletedClaimsForProvider(int providerid)
        {
            //disaable notdeletefilter for this query
            NotDeletedFilterDisabler shitt = new NotDeletedFilterDisabler(_session);

            IList<string> deleted = _session.CreateQuery("select clientkey from ProviderClaimBK where isdeleted=1 and providerId=" + providerid.ToString()).List<string>();
            List<string> outputstring = new List<string>();

            shitt.Dispose();


            foreach (string item in deleted)
            {
                outputstring.Add(item.ToString());

            }
            return outputstring;
        }

        public IncomingClaims getincomingClaimByMonthandYear(int providerid, int month, int year)
        {
            string monthstring = month.ToString();
            IQueryOver<IncomingClaims, IncomingClaims> query = _session.QueryOver<IncomingClaims>().Where(x => x.IsDeleted == false && x.month_string == monthstring && x.year == year && x.providerid == providerid && x.IsRemoteSubmission == true);

            IncomingClaims response = new IncomingClaims();
            if (query.RowCount() > 0)
            {
                foreach (IncomingClaims item in query.List().OrderByDescending(x => x.CreatedOn))
                {
                    if (item.ClaimBatch.status == ClaimBatchStatus.Capturing)
                    {
                        //found our guy

                        response = item;
                        break;
                    }
                }
            }
            return response;
        }

        public IList<Entities.ClaimBatch> QueryAllClaimBatch(out int toltareccount, out int totalinresult, string empty, bool v1, bool v2, string sortColumnnumber, string sortOrder, int v3, int v4, int v5, Utility.ClaimBatch claimBatch, string zone, int v6, ClaimBatchStatus vetting, out int providercount, out decimal totalamount, out decimal totalprocessed, int channelint, int clambatchiddd)
        {
            throw new NotImplementedException();
        }
    }
}