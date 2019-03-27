using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Elmah;
using MrCMS.Entities.Messaging;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Logging;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Areas.Admin.Services;
using NHibernate;
using NHibernate.Criterion;
using StackExchange.Profiling.Helpers;

namespace MrCMS.Web.Apps.Core.Services
{
    public class HelperService : IHelperService
    {
        private readonly ISession _session;
        private readonly ILogAdminService _logger;
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;
        private readonly MailSettings _mailSettings;
        private readonly IRoleService _roleSvc;
        public HelperService(ISession session, ILogAdminService log, IUserService userserv, IEmailSender emailSender, MailSettings mailSettings, IRoleService rolesvc)
        {
            _session = session;
            _logger = log;
            _userService = userserv;
            _emailSender = emailSender;
            _mailSettings = mailSettings;
            _roleSvc = rolesvc;
        }

        public IEnumerable<Zone> GetallZones()
        {
            return _session.QueryOver<Zone>().Where(x => x.IsDeleted == false).List<Zone>();
        }

        public IEnumerable<State> GetallStates()
        {
            return _session.QueryOver<State>().Where(x => x.IsDeleted == false).List<State>().OrderBy(x => x.Name);
        }

        public IEnumerable<Bank> Getallbanks()
        {
            return _session.QueryOver<Bank>().Where(x => x.IsDeleted == false).List<Bank>();
        }

        public Bank Getbank(int id)
        {
            return _session.QueryOver<Bank>().Where(x => x.IsDeleted == false && x.Id == id).SingleOrDefault();
        }

        public IEnumerable<State> GetStatesinZone(int zoneid)
        {
            return _session.QueryOver<State>().Where(x => x.IsDeleted == false && x.Zone == zoneid).List<State>();
        }

        public State GetState(int id)
        {
            return _session.QueryOver<State>().Where(x => x.IsDeleted == false && x.Id == id).SingleOrDefault();
        }

        public IEnumerable<Lga> GetLgainstate(int stateId)
        {
            return _session.QueryOver<Lga>().Where(x => x.IsDeleted == false && x.State == stateId).List<Lga>();
        }

        public Lga GetLgabyId(int id)
        {
            return _session.QueryOver<Lga>().Where(x => x.IsDeleted == false && x.Id == id).SingleOrDefault();
        }

        public Zone GetzonebyId(int id)
        {
            return _session.QueryOver<Zone>().Where(x => x.IsDeleted == false && x.Id == id).SingleOrDefault();
        }


        public string GenerateProvidersubCode(Lga lga)
        {
            Provider top =
                _session.QueryOver<Provider>().Where(x => x.IsDeleted == false && x.Lga.Id == lga.Id).OrderBy(x => x.Id)
                    .Desc

                    .Take(1).SingleOrDefault();
            string response = string.Empty;
            if (top != null)
            {
                int newid = top.Id + 1;

                return string.Format("LAN-{0}-{1}", lga.Code, newid);



            }
            else
            {
                int newid = 1;

                return string.Format("LAN-{0}-{1}", lga.Code, newid);
            }
        }

        public string GenerateCompanysubCode()
        {
            Company top =
                _session.QueryOver<Company>().Where(x => x.IsDeleted == false).OrderBy(x => x.Id)
                    .Desc.Take(1).SingleOrDefault();




            if (top != null)
            {
                int code = (top.Id + 1);
                string companycode = string.Format("NHA-COM-{0}", code);
                return companycode;
            }
            else
            {
                return string.Format("NHA-COM-{0}", 1);
            }

        }

        public string GenerateFourDigitCode()
        {

            const int min = 1000;
            const int max = 9999;
            Random rdm = new Random();
            return rdm.Next(min, max).ToString();

        }

        public string GeneratesubscriptionCode()
        {

            Subscription top =
                _session.QueryOver<Subscription>().Where(x => x.IsDeleted == false).OrderBy(x => x.Id)
                    .Desc.Take(1).SingleOrDefault();




            if (top != null)
            {
                int code = (top.Id + 1);
                string companycode = string.Format("NHA-SUB-{0}", code);
                return companycode;
            }
            else
            {
                return string.Format("NHA-SUB-{0}", 1);
            }
        }
        public string GetDescription(Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {

                    DisplayAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DisplayAttribute)) as DisplayAttribute;
                    if (attr != null)
                    {
                        return attr.Name;
                    }
                }
            }
            return null;
        }
        public bool Log(LogEntryType type, Error error, string message, string detail)
        {
            //create new log

            Log log = new Log()
            {
                Type = type,
                Error = error,
                Message = message,
                Detail = detail
            };

            _logger.Insert(log);
            return true;
        }
        public bool PushUserNotification(string destinationuser, string message, NotificationTarget target, string roles,
                                                NotificationType notificationType, string clickaction)
        {

            //Check if its role or user 

            //do the role maths here 
            if (target == NotificationTarget.Role)
            {
                foreach (string role in roles.Split(','))
                {
                    IList<User> usersinrole = _userService.GetAllUsersByRole(_roleSvc.GetRole(Convert.ToInt32(role)));

                    foreach (User user in usersinrole)
                    {
                        //push to each of theme

                        UserNotification notify = new UserNotification
                        {
                            UserId = user.Guid.ToString(),
                            Message = message,
                            Role = _roleSvc.GetRole(Convert.ToInt32(role)),
                            Target = Convert.ToInt32(target),
                            ClickAction = clickaction,
                            Type = Convert.ToInt32(notificationType)
                        };

                        _session.Transact(session => session.Save(notify));

                        //send email notification
                        //Add to email queue.
                        QueuedMessage emailmsg = new QueuedMessage();
                        emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                        emailmsg.ToAddress = user.Email;
                        emailmsg.Subject = "NovoHub Notification - " + notify.Message.Truncate(20);
                        emailmsg.FromName = "NOVOHUB";
                        emailmsg.Body = notify.Message;


                        //Commented this out for now for the data input guys
                        // _emailSender.SendMailMessage(emailmsg);
                        _emailSender.AddToQueue(emailmsg);

                        //Notify the theres a new notification
                        NewNotificationArgs args = new NewNotificationArgs
                        {
                            Notification = notify
                        };
                        //Notify the Hub of the new Input
                        EventContext.Instance.Publish(typeof(INewNotificationEvent), args);

                    }
                }


            }
            else
            {
                //push to each of theme
                User user = _userService.GetUserByGUID(destinationuser);
                UserNotification notify = new UserNotification
                {
                    UserId = destinationuser,
                    Message = message,
                    Role = user.Roles.FirstOrDefault(),
                    Target = Convert.ToInt32(target),
                    ClickAction = clickaction,
                    Type = Convert.ToInt32(notificationType)
                };

                _session.Transact(session => session.Save(notify));

                //send email notification
                //Add to email queue.
                QueuedMessage emailmsg = new QueuedMessage();
                emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                emailmsg.ToAddress = user.Email;
                emailmsg.Subject = "NovoHub Notification - " + notify.Message.Truncate(20);
                emailmsg.FromName = "NOVOHUB";
                emailmsg.Body = notify.Message;


                //Commented this out for now for the data input guys
                // _emailSender.SendMailMessage(emailmsg);
                _emailSender.AddToQueue(emailmsg);

                //Notify the theres a new notification
                NewNotificationArgs args = new NewNotificationArgs
                {
                    Notification = notify
                };
                //Notify the Hub of the new Input
                EventContext.Instance.Publish(typeof(INewNotificationEvent), args);
            }



            return true;
        }

        public bool AddNotificationTable(int notificationcode, string description, string roles, bool active)
        {
            NotificationTable notificationTable = new NotificationTable()
            {
                Notificationcode = notificationcode,
                Description = description,
                Active = active,
                Roles = roles
            };
            if (!_session.QueryOver<NotificationTable>().Where(x => x.IsDeleted == false && x.Notificationcode == notificationcode).Any())
            {
                _session.Transact(session => session.Save(notificationTable));
                return true;

            }
            return false;
        }

        public NotificationTable GetNotificationTable(int notificationCode)
        {
            return _session.QueryOver<NotificationTable>().Where(x => x.IsDeleted == false && x.Notificationcode == notificationCode).SingleOrDefault();
        }

        public IEnumerable<NotificationTable> GetallNotificationTable()
        {
            return _session.QueryOver<NotificationTable>().Where(x => x.IsDeleted == false).List<NotificationTable>();
        }

        public IList<string> GeneratePolicyNumber(int count, bool validate)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < count; i++)
            {



                if (!validate)
                {
                    string temp = Guid.NewGuid().ToString().Replace("-", string.Empty);
                    string num = Regex.Replace(temp, "[a-zA-Z]", string.Empty);
                    if (num.Length > 11)
                    {
                        num = num.Substring(0, 12);
                    }
                    else
                    {
                        int paddcount = 12 - num.Length;
                        for (int z = 0; z < paddcount; z++)
                        {
                            Random rnd = new Random();
                            int value = rnd.Next(1, 10); // creates a number between 1 and 12

                            num = num.ToString() + value.ToString();

                            num = num.Substring(0, 12);
                        }
                    }


                    string policycode = "NHA-" + num;



                    result.Add(policycode);
                }

                do
                {
                    string temp = Guid.NewGuid().ToString().Replace("-", string.Empty);
                    string num = Regex.Replace(temp, "[a-zA-Z]", string.Empty);
                    if (num.Length > 11)
                    {
                        num = num.Substring(0, 12);

                    }
                    else
                    {
                        int paddcount = 12 - num.Length;
                        for (int z = 0; z < paddcount; z++)
                        {
                            Random rnd = new Random();
                            int value = rnd.Next(1, 10); // creates a number between 1 and 12

                            num = num.ToString() + value.ToString();

                            num = num.Substring(0, 12);
                        }
                    }

                    string policycode = "NHA-" + num;


                    if (ValidatePolicyNumber(policycode))
                    {
                        result.Add(policycode);
                        break;
                    }


                } while (validate);



            }

            return result;
        }

        public string GenerateVerificationCode()
        {

            while (true)
            {
                Random rnd = new Random();
                int myRandomNo = rnd.Next(10000000, 99999999);

                if (!(_session.QueryOver<EnrolleeVerificationCode>().Where(x => x.VerificationCode == Convert.ToString(myRandomNo)).Any()))
                {

                    return Convert.ToString(myRandomNo);

                }
            }

        }

        public IList<EnrolleeVerificationCode> Getallverifications()
        {
            return _session.QueryOver<EnrolleeVerificationCode>().List<EnrolleeVerificationCode>().OrderByDescending(x => x.EncounterDate).Take(100).ToList();
        }

        public EnrolleeVerificationCode Getverification(int id)
        {
            return _session.QueryOver<EnrolleeVerificationCode>().Where(x => x.IsDeleted == false && x.Id == id).SingleOrDefault();
        }

        public EnrolleeVerificationCode GetverificationByVerificationCode(string verification)
        {
            return _session.QueryOver<EnrolleeVerificationCode>().Where(x => x.IsDeleted == false && x.VerificationCode == verification).SingleOrDefault();
        }

        public bool Addverification(EnrolleeVerificationCode verificationcode)
        {

            bool exist = _session.QueryOver<EnrolleeVerificationCode>().Where(x => x.VerificationCode == verificationcode.VerificationCode).Any();
            if (verificationcode != null && exist == false)
            {
                _session.Transact(session => session.Save(verificationcode));

                //Notify the theres a new verificationCode
                VerificationCodeChangeArgs args = new VerificationCodeChangeArgs
                {
                    VerificationCode = verificationcode
                };
                //Notify the Hub of the new Input
                EventContext.Instance.Publish(typeof(INewNotificationEvent), args);
                return true;
            }
            return false;
        }

        public bool Updateverification(EnrolleeVerificationCode verificationcode)
        {
            if (verificationcode != null)
            {
                _session.Transact(session => session.Update(verificationcode));
                //Notify the theres a new verificationCode
                VerificationCodeChangeArgs args = new VerificationCodeChangeArgs
                {
                    VerificationCode = verificationcode
                };
                //Notify the Hub of the new Input
                EventContext.Instance.Publish(typeof(INewNotificationEvent), args);
                return true;
            }

            return false;
        }

        public int GetTotalNoofverification()
        {
            return _session.QueryOver<EnrolleeVerificationCode>().RowCount();
        }

        public int GetAllAuthenticatedVerification()
        {
            return _session.QueryOver<EnrolleeVerificationCode>().Where(x => x.Status == EnrolleeVerificationCodeStatus.Authenticated).RowCount();
        }

        public int GetwithoutAuthentication()
        {
            return _session.QueryOver<EnrolleeVerificationCode>().Where(x => x.Status == EnrolleeVerificationCodeStatus.Pending).RowCount();
        }

        public int GetMobileCount()
        {
            return _session.QueryOver<EnrolleeVerificationCode>().Where(x => x.Channel == (int)ChannelType.MobileApp).RowCount();
        }

        public int GetSmsCount()
        {
            return _session.QueryOver<EnrolleeVerificationCode>().Where(x => x.Channel == (int)ChannelType.ShortCode).RowCount();
        }

        public int GetUniqueProvidersAuntenticated()
        {
            IList<EnrolleeVerificationCode> listt = _session.QueryOver<EnrolleeVerificationCode>()
                .Where(x => x.Status == EnrolleeVerificationCodeStatus.Authenticated).List<EnrolleeVerificationCode>();


            List<string> outputlst = new List<string>();

            foreach (EnrolleeVerificationCode item in listt)
            {
                if (!outputlst.Contains(item.ProviderId.ToString()))
                {
                    outputlst.Add(item.ProviderId.ToString());
                }
            }

            return outputlst.Count;
        }

        public int GetUniquesEnrolleeGenerated()
        {
            IList<EnrolleeVerificationCode> listt = _session.QueryOver<EnrolleeVerificationCode>().List<EnrolleeVerificationCode>();


            List<string> outputlst = new List<string>();

            foreach (EnrolleeVerificationCode item in listt)
            {
                if (!outputlst.Contains(item.EnrolleeId.ToString()))
                {
                    outputlst.Add(item.EnrolleeId.ToString());
                }
            }

            return outputlst.Count;
        }

        public int GetTotalNoofverificationByDate(DateTime startdate, DateTime endDate)
        {
            return _session.QueryOver<EnrolleeVerificationCode>().Where(Restrictions.On<EnrolleeVerificationCode>(a => a.CreatedOn).IsBetween(startdate).And(endDate)).Where(x => x.Status == EnrolleeVerificationCodeStatus.Pending).RowCount();
        }

        public int GetAllAuthenticatedVerificationByDate(DateTime startdate, DateTime endDate)
        {
            return _session.QueryOver<EnrolleeVerificationCode>().Where(Restrictions.On<EnrolleeVerificationCode>(a => a.CreatedOn).IsBetween(startdate).And(endDate)).Where(x => x.Status == EnrolleeVerificationCodeStatus.Authenticated).RowCount();
        }

        public IList<ShortCodeMsg> QueryShortCodeMsgs(out int totalRecord, out int totalcountinresult, string search, int start, int lenght,
            string scrMobilenumber, string scrMessage, bool useDate, DateTime scrFromDate, DateTime scrToDate)
        {
            IQueryOver<ShortCodeMsg, ShortCodeMsg> query = _session.QueryOver<ShortCodeMsg>().Where(x => x.IsDeleted == false);

            if (!string.IsNullOrEmpty(scrMobilenumber))
            {
                //search policy number



                scrMobilenumber = "%" + scrMobilenumber + "%";


                query = query.Where(Restrictions.On<ShortCodeMsg>(x => x.Mobile).IsInsensitiveLike(scrMobilenumber));

            }


            if (!string.IsNullOrEmpty(scrMessage))
            {
                scrMessage = "%" + scrMessage + "%";
                query = query.Where(Restrictions.On<ShortCodeMsg>(x => x.Msg).IsInsensitiveLike(scrMessage));
            }



            if (useDate)
            {
                DateTime newenddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", scrToDate.Month, scrToDate.Day, scrToDate.Year, "23:59"));
                query.Where(Restrictions.On<ShortCodeMsg>(a => a.CreatedOn).IsBetween(scrFromDate).And(newenddate));
            }
            //return normal list.
            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            return query.OrderBy(x => x.CreatedOn).Desc.Skip(start).Take(lenght).List();
        }

        public bool ValidatePolicyNumber(string policy)
        {
            return !(_session.QueryOver<Enrollee>().Where(x => x.Policynumber == policy || x.Policynumber == policy.ToLower()).Any());
        }

        public bool AddDownloadFile(DownloadFile file)
        {
            if (file != null)
            {
                _session.Transact(session => session.Save(file));
                return true;
            }

            return false;
        }

        public bool updateDownloadFile(DownloadFile file)
        {
            if (file != null)
            {
                _session.Transact(session => session.Update(file));
                return true;
            }

            return false;
        }

        public bool DeleteDownloadedFile(DownloadFile fileid)
        {
            if (fileid != null)
            {
                _session.Transact(session => session.Delete(fileid));
                return true;
            }

            return false;
        }

        public IList<DownloadFile> QueryDownloadFiles(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string searchText)
        {
            IQueryOver<DownloadFile, DownloadFile> query = _session.QueryOver<DownloadFile>().Where(x => x.IsDeleted == false);

            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = "%" + searchText + "%";
                query = query.Where(Restrictions.On<DownloadFile>(x => x.fileName).IsInsensitiveLike(searchText));
            }

            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            return query.OrderBy(x => x.CreatedOn).Desc.Skip(start).Take(lenght).List();
        }

        public DownloadFile getDownloadedFile(int fileid)
        {
            DownloadFile result = _session.QueryOver<DownloadFile>().Where(x => x.Id == fileid).SingleOrDefault();
            return result;


        }

        public string GenerateAuthorizactionCode()
        {
            string guidvalue = Guid.NewGuid().ToString();
            string bit = guidvalue.Split('-')[2];
            string bit2 = guidvalue.Split('-')[1].ToString();
            string bit3 = guidvalue.Split('-')[0].ToString();

            string resp = string.Format("NHA-{0}-{1}", bit3, bit);

            return resp;


        }

        public bool addTask(TaskShit Task)
        {
            if (Task != null)
            {
                _session.Transact(session => session.Save(Task));
                return true;
            }

            return false;
        }

        public bool deleteTask(TaskShit Task)
        {
            if (Task != null)
            {
                _session.Transact(session => session.Delete(Task));
                return true;
            }

            return false;
        }

        public TaskShit getTask(int Id)
        {
            TaskShit result = _session.QueryOver<TaskShit>().Where(x => x.Id == Id).SingleOrDefault();
            return result;
        }

        public bool updateTask(TaskShit Task)
        {
            if (Task != null)
            {
                _session.Transact(session => session.Update(Task));
                return true;
            }

            return false;
        }

        public IList<TaskShit> GetAllTask()
        {
            IQueryOver<TaskShit, TaskShit> query = _session.QueryOver<TaskShit>().Where(x => x.IsDeleted == false);

            return query.List();
        }

        public long usersCount()
        {
            return _session.QueryOver<User>().Where(x => x.IsDeleted == false).RowCount();
        }

        public bool addSponsor(ConnectCareSponsor sponsor)
        {
            if (sponsor != null)
            {
                _session.Transact(session => session.SaveOrUpdate(sponsor));


                //after saving then save the beneficiaries

                return true;
            }

            return false;
        }

        public bool addBeneficiary(ConnectCareBeneficiary beneficiary)
        {
            if (beneficiary != null)
            {
                _session.Transact(session => session.SaveOrUpdate(beneficiary));
                return true;
            }

            return false;
        }

        public string GenerateCCPolicyNumber()
        {
            Random rnd = new Random();
            int myRandomNo = rnd.Next(10000000, 99999999); // creates a 8 digit random no.

            string policynumber = "NHA-CC-" + myRandomNo.ToString();


            while (!ValidateConenctCarePolicynumber(policynumber))
            {
                myRandomNo = rnd.Next(10000000, 99999999);
                policynumber = "NHA-CC-" + myRandomNo.ToString();
            }


            return policynumber;



        }

        public bool ValidateConenctCarePolicynumber(string policynumber)
        {
            return !(_session.QueryOver<ConnectCareSponsor>().Where(x => x.policynumber == policynumber || x.policynumber == policynumber.ToLower()).Any());
        }

        public bool UpdateSponsor(ConnectCareSponsor sponsor)
        {
            if (sponsor != null)
            {
                _session.Transact(session => session.Update(sponsor));
                return true;
            }

            return false;
        }

        public IList<ConnectCareSponsor> QueryConnectCareSponsor(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string fullname, string Policynumber, int senttomatontine, bool useDate, DateTime scrFromDate, DateTime scrToDate)
        {
            IQueryOver<ConnectCareSponsor, ConnectCareSponsor> query = _session.QueryOver<ConnectCareSponsor>().Where(x => x.IsDeleted == false);
            if (senttomatontine > -1)
            {
                bool sent = Convert.ToBoolean(senttomatontine);
                query.Where(x => x.pushedtoMatontine == sent);

            }

            if (!string.IsNullOrEmpty(fullname))
            {
                fullname = "%" + fullname + "%";
                query.Where(Restrictions.On<ConnectCareSponsor>(x => x.fullname).IsInsensitiveLike(fullname));
            }

            if (!string.IsNullOrEmpty(Policynumber))
            {
                query.Where(x => x.policynumber == Policynumber || x.policynumber == Policynumber.ToLower());

            }

            if (useDate)
            {
                DateTime newenddate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", scrToDate.Month, scrToDate.Day, scrToDate.Year, "23:59"));
                query.Where(Restrictions.On<ConnectCareSponsor>(a => a.CreatedOn).IsBetween(scrFromDate).And(newenddate));
            }
            //return normal list.
            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            return query.OrderBy(x => x.CreatedOn).Desc.Skip(start).Take(lenght).List();

        }

        public ConnectCareSponsor GetSponsor(int Id)
        {
            return _session.QueryOver<ConnectCareSponsor>().Where(x => x.Id == Id).SingleOrDefault();

        }

        public IList<ConnectCareBeneficiary> GetBeneficiariesBySponsorID(int sponsorId)
        {
            return _session.QueryOver<ConnectCareBeneficiary>().Where(x => x.sponsorid == sponsorId).List<ConnectCareBeneficiary>();

        }

        public ConnectCareBeneficiary getBeneficiary(int Id)
        {
            return _session.QueryOver<ConnectCareBeneficiary>().Where(x => x.Id == Id).SingleOrDefault();
        }

        public bool addpaymentdetails(ConnectcarePaymentDetails payment)
        {
            if (payment != null)
            {
                _session.Transact(session => session.Save(payment));
                return true;
            }
            return false;
        }

        public ConnectcarePaymentDetails getpaymentbyId(int Id)
        {
            return _session.QueryOver<ConnectcarePaymentDetails>().Where(x => x.sponsorID == Id).SingleOrDefault();
        }

        public IList<ConnectcarePaymentDetails> getAllPaymentDetails()
        {
            return _session.QueryOver<ConnectcarePaymentDetails>().List();
        }

        public IList<ConnectcarePaymentDetails> getAllPaymentDetailsBySponsorID(int Id)
        {
            return _session.QueryOver<ConnectcarePaymentDetails>().Where(x => x.sponsorID == Id).List<ConnectcarePaymentDetails>();

        }

        public bool checkpaymentuniqueID(string Id)
        {
            return _session.QueryOver<ConnectcarePaymentDetails>().Where(x => x.paymentid == Id).Any();
        }

        public IList<ConnectCareBeneficiary> getAllUnverifiedBeneficiary()
        {
            return _session.QueryOver<ConnectCareBeneficiary>().Where(x => x.VerificationStatus == false).List();
        }

        public bool addEmailDest(EmailDEST email)
        {
            if (email != null)
            {
                _session.Transact(x => x.Save(email));
                return true;
            }
            return false;
        }

        public EmailDEST getEmailDestByCode(string code)
        {
            return _session.QueryOver<EmailDEST>().Where(x => x.code == code).SingleOrDefault();
        }
    }
}