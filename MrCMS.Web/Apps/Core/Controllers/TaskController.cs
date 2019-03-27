using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MrCMS.Website.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MrCMS.Tasks;
using GetShortCode;
using MrCMS.Entities.Messaging;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Services;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Website;
using MrCMS.Logging;
using MrCMS.Settings;
using MrCMS.Web.Areas.Admin.Services;
using NHibernate;

namespace MrCMS.Web.Apps.Core.Controllers
{
    public class TaskController : MrCMSAppUIController<CoreApp>
    {
        private readonly IHelperService _helperSvc;
        private readonly IEnrolleeService _enrolleeService;
        private readonly ISmsService _smsservice;
        private readonly IProviderService _providerservice;
        private readonly ILogAdminService _logger;
        public const int MAX_TRIES = 5;
        private readonly ISession _session;
        private readonly IEmailSender _emailSender;
        private readonly SiteSettings _siteSettings;
        private readonly Site _site;

        private readonly IPlanService _planService;
        private readonly IServicesService _serviceSvc;
        private readonly IUniquePageService _uniquePageService;
        private readonly IPageMessageSvc _pageMessageSvc;

        private readonly IProviderService _providerSvc;
        private readonly ICompanyService _companySvc;

        private readonly UserService _userservice;
        private readonly IRoleService _rolesvc;

        private readonly MailSettings _mailSettings;
        private readonly Services.ClaimService _claimservice;

        // GET: Task
        public TaskController(IHelperService helperService, ISmsService smsSvc, IEnrolleeService enrolleeService, ILogAdminService loggersvc, IProviderService providersvc, ISession session, IEmailSender emailSender, SiteSettings siteSettings, Site site, IPlanService planService, IUniquePageService uniquepageService, IPageMessageSvc pageMessageSvc, IServicesService serviceSvc, IProviderService Providersvc, ILogAdminService logger, ICompanyService companyService, UserService userService, IRoleService roleService, MailSettings mailSettings, Services.ClaimService claimserv)
        {
            _helperSvc = helperService;
            _enrolleeService = enrolleeService;
            _smsservice = smsSvc;
            _logger = loggersvc;
            _providerservice = providersvc;

            _session = session;
            _emailSender = emailSender;
            _siteSettings = siteSettings;
            _site = site;
            _planService = planService;
            _uniquePageService = uniquepageService;
            _pageMessageSvc = pageMessageSvc;

            _serviceSvc = serviceSvc;
            _providerSvc = Providersvc;
            _logger = logger;
            _companySvc = companyService;
            _userservice = userService;
            _rolesvc = roleService;

            _mailSettings = mailSettings;

            _claimservice = claimserv;

        }

        public string RunTask()
        {
            //run emailTask



            IList<TaskShit> tasks = _helperSvc.GetAllTask();

            if (tasks.Count() == 3)
            {
                //add Executebulkstafftask
                TaskShit BulkUpload = new TaskShit
                {
                    Name = "BulkUpload",
                    Enabled = true,
                    status = false,
                    RunTimerSeconds = 360,
                    LastRun = CurrentRequestData.Now

                };
                _helperSvc.addTask(BulkUpload);


                //add Executebulkstafftask
                TaskShit PortalImport = new TaskShit
                {
                    Name = "PortalImport",
                    Enabled = true,
                    status = false,
                    RunTimerSeconds = 360,
                    LastRun = CurrentRequestData.Now

                };
                _helperSvc.addTask(PortalImport);


            }

            if (!tasks.Any())
            {
                //create tasks 

                TaskShit birthday = new TaskShit
                {
                    Name = "Birthday",
                    Enabled = true,
                    status = false,
                    RunTimerSeconds = 36000,
                    LastRun = CurrentRequestData.Now

                };

                TaskShit Email = new TaskShit
                {
                    Name = "SendEmail",
                    Enabled = true,
                    status = false,
                    RunTimerSeconds = 30,
                    LastRun = CurrentRequestData.Now
                };


                TaskShit Subscriptions = new TaskShit
                {
                    Name = "Subscriptions",
                    Enabled = true,
                    status = false,
                    RunTimerSeconds = 36000,
                    LastRun = CurrentRequestData.Now
                };

                _helperSvc.addTask(birthday);
                _helperSvc.addTask(Email);
                _helperSvc.addTask(Subscriptions);

            }

            foreach (TaskShit task in tasks)
            {

                double timediff = CurrentRequestData.Now.Subtract(task.LastRun).TotalSeconds;
                //check if task is running.
                if (!task.status && timediff >= task.RunTimerSeconds)
                {
                    switch (task.Name)
                    {
                        case "SendEmail":
                            try
                            {


                                SendEmailMessages(task);

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                            break;


                        case "Birthday":
                            try
                            {

                                DateTime datetime = CurrentRequestData.Now;

                                int hour = datetime.Hour;

                                if (hour > 8)
                                {
                                    SendBithdayMessageTask(task);
                                }


                            }
                            catch (Exception e)
                            {

                                task.status = false;
                                _helperSvc.updateTask(task);
                                Console.WriteLine(e);
                                throw;
                            }
                            break;


                        case "Subscriptions":
                            try
                            {
                                SubscriptionTask(task);

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                            break;
                    }

                }

            }

            return "Complete";

        }
        private bool SendBithdayMessageTask(TaskShit Task)
        {
            TaskShit task = _helperSvc.getTask(Task.Id);
            task.status = true;
            _helperSvc.updateTask(task);
            IList<Enrollee> enrolleewithbirthday = _enrolleeService.GetEnrolleeCelebratingBirthday();

            List<string> phonenumbersent = new List<string>();
            DateTime today = CurrentRequestData.Now;
            foreach (Enrollee enrollee in enrolleewithbirthday.Where(x => x.Mobilenumber.Length > 10))
            {
                if (enrollee.LastyearBirthdaymsgsent < today.Year)
                {

                    string fullname = enrollee.Surname + " " + enrollee.Othernames;

                    if (!string.IsNullOrEmpty(fullname) && !phonenumbersent.Contains(enrollee.Mobilenumber))
                    {
                        string messageFormated = _smsservice.GetConfig().BdaySmsTemplate.Replace("%name%", fullname);


                        Sms sms = new Sms();
                        sms.FromId = "NovoHealth";
                        sms.DeliveryDate = CurrentRequestData.Now;
                        sms.Message =
                            string.Format(messageFormated);
                        sms.DateDelivered = CurrentRequestData.Now;
                        sms.CreatedBy = CurrentRequestData.CurrentUser != null ? CurrentRequestData.CurrentUser.Id : 1;
                        sms.Msisdn = enrollee.Mobilenumber;
                        sms.Status = SmsStatus.Pending;
                        sms.Type = SmsType.Birthday;

                        bool resp = _smsservice.SendSms(sms);

                        phonenumbersent.Add(enrollee.Mobilenumber);

                        //update enrollee last birthdaysent

                        enrollee.LastyearBirthdaymsgsent = today.Year;
                        _enrolleeService.UpdateEnrollee(enrollee);

                    }
                    //not sent for years

                    if (phonenumbersent.Contains(enrollee.Mobilenumber))
                    {
                        //duplicate enrollee
                        enrollee.LastyearBirthdaymsgsent = today.Year;
                        _enrolleeService.UpdateEnrollee(enrollee);
                    }

                }
            }
            Log log = new Log();
            log.Message = string.Format("Birthday Message was sent to  {0} Enrollee's {1}", phonenumbersent.Count(), CurrentRequestData.Now.ToLongTimeString());
            log.Type = LogEntryType.Audit;
            log.Detail = "Birthday Message Sent";

            _logger.Insert(log);



            //Do PostVerification EVS

            IList<EnrolleeVerificationCode> verificationlis = _enrolleeService.GetPreviousDayAuthenticatedCodes();

            int smscount = 0;
            foreach (EnrolleeVerificationCode item in verificationlis)
            {
                Enrollee enrollee = _enrolleeService.GetEnrollee(item.EnrolleeId);
                Provider provider = _providerservice.GetProvider(item.ProviderId);

                if (enrollee != null && provider != null & enrollee.Mobilenumber.Length > 9)
                {
                    string msg = string.Format("Dear {0} ,Novo Health Africa Cares. How were you served at {1} yesterday ,Call or text.08180287867.", (enrollee.Surname + " " + enrollee.Othernames).ToUpper(), provider.Name.ToUpper());


                    Sms sms = new Sms();
                    sms.FromId = "NovoHealth";
                    sms.DeliveryDate = CurrentRequestData.Now;
                    sms.Message = msg;

                    sms.DateDelivered = CurrentRequestData.Now;
                    sms.CreatedBy = CurrentRequestData.CurrentUser != null ? CurrentRequestData.CurrentUser.Id : 1;
                    sms.Msisdn = enrollee.Mobilenumber;
                    sms.Status = SmsStatus.Pending;
                    sms.Type = SmsType.Others;

                    bool resp = _smsservice.SendSms(sms);


                    smscount++;

                }
                item.PostSMSSent = true;
                _helperSvc.Updateverification(item);
            }
            Log logit = new Log();
            logit.Message = string.Format("PostEVS Message was sent to  {0} Enrollee's {1}", smscount, CurrentRequestData.Now.ToLongTimeString());
            logit.Type = LogEntryType.Audit;
            logit.Detail = "Post EVS Message Sent";



            //send delivery
            IList<AuthorizationCode> thedeliveries = _claimservice.getBirthAuthorization();

            foreach (AuthorizationCode item in thedeliveries)
            {
                Enrollee enrollee = _enrolleeService.GetEnrollee(item.enrolleeID);

                if (enrollee != null && enrollee.Mobilenumber.Length > 9)
                {
                    Sms sms = new Sms();
                    sms.FromId = "NovoHealth";
                    sms.DeliveryDate = CurrentRequestData.Now;
                    sms.Message = "Dear esteemed enrollee, Novo Health Africa wishes to use this medium to congratulate you on the arrival of your bundle of Joy. It is with great joy we share this period and experience of Joy with you and your family. Please accept our heartfelt wishes. With love from NHA.";

                    sms.DateDelivered = CurrentRequestData.Now;
                    sms.CreatedBy = CurrentRequestData.CurrentUser != null ? CurrentRequestData.CurrentUser.Id : 1;
                    sms.Msisdn = enrollee.Mobilenumber;
                    sms.Status = SmsStatus.Pending;
                    sms.Type = SmsType.Others;

                    bool resp = _smsservice.SendSms(sms);
                    item.deliverysmssent = true;

                    _claimservice.updateAuthorization(item);
                }
            }


            //update task

            task.status = false;
            task.LastRun = CurrentRequestData.Now;






            return _helperSvc.updateTask(task);
        }

        private bool SendEmailMessages(TaskShit Task)
        {
            TaskShit task = _helperSvc.getTask(Task.Id);
            task.status = true;
            _helperSvc.updateTask(task);
            _session.Transact(session =>
            {
                foreach (
                    QueuedMessage queuedMessage in
                    session.QueryOver<QueuedMessage>().Where(
                            message => message.SentOn == null && message.Tries < MAX_TRIES)
                        .Where(message => message.Site.Id == _site.Id)
                        .List())
                {
                    if (_emailSender.CanSend(queuedMessage))
                        _emailSender.SendMailMessage(queuedMessage);
                    else
                        queuedMessage.SentOn = CurrentRequestData.Now;
                    session.SaveOrUpdate(queuedMessage);
                }
            });

            //update task

            task.status = false;
            task.LastRun = CurrentRequestData.Now;

            return _helperSvc.updateTask(task);
        }

        private bool SubscriptionTask(TaskShit Task)
        {
            TaskShit task = _helperSvc.getTask(Task.Id);
            task.status = true;
            _helperSvc.updateTask(task);
            //companySvc.ExecuteSubscriptionCheck();

            //activate them old shit

            IList<Subscription> activate = _companySvc.GetNewlyApprovedActiveSubscription();

            foreach (Subscription item in activate)
            {

                if (item.Expirationdate > CurrentRequestData.Now)
                {
                    item.Status = (int)SubscriptionStatus.Active;
                }
                else
                {
                    item.Status = (int)SubscriptionStatus.Expired;
                }
                _companySvc.UpdateSubscription(item);

            }


            IList<Subscription> expiringsoon = _companySvc.GetSubscriptionExpiringSoon();
            IList<Subscription> expiredd = _companySvc.GetexpiredSubscriptions();
            IEnumerable<Subscription> expired = expiringsoon.Where(x => x.Expirationdate < CurrentRequestData.Now);

            foreach (Subscription item in expiredd)
            {
                item.Status = (int)SubscriptionStatus.Expired;
                _companySvc.UpdateSubscription(item);
            }

            StringBuilder bodyText = new StringBuilder();

            bodyText.AppendLine("The following companies are about to expire.");
            bodyText.AppendLine(Environment.NewLine);


            foreach (Subscription sub in expiringsoon)
            {
                Company company = _companySvc.GetCompany(sub.CompanyId);


                bodyText.AppendFormat("{0} ------- {1} {2}", company.Name.ToUpper(), sub.SubscriptionCode, Convert.ToDateTime(sub.Expirationdate).ToShortDateString());
                bodyText.AppendLine(Environment.NewLine);

            }
            bodyText.AppendLine("Thank You");

            MrCMS.Entities.People.UserRole role = _rolesvc.GetRoleByName("CLIENT SERVICE");



            if (role != null && expiringsoon.Any())
            {
                foreach (MrCMS.Entities.People.User user in role.Users)
                {
                    //each user
                    QueuedMessage emailmsg = new QueuedMessage();
                    //Send Company Subscription Notice to these emails
                    string Email2 = "info@novohealthafrica.org, jessicao@novohealthafrica.org, silverlinea@novohealthafrica.org, regionalmanagers@novohealthafrica.org, akinbamidelea@novohealthafrica.org ";
                    emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                    emailmsg.ToAddress = user.Email;
                    emailmsg.ToAddress = Email2;
                    emailmsg.Subject = "Company Subscription Expiration Notice ";
                    emailmsg.FromName = "NOVOHUB";
                    emailmsg.Body = bodyText.ToString();

                    _emailSender.AddToQueue(emailmsg);
                }
            }


            //update task

            task.status = false;
            task.LastRun = CurrentRequestData.Now;

            return _helperSvc.updateTask(task);
        }

        private bool BulkUpload(TaskShit Task)
        {
            TaskShit task = _helperSvc.getTask(Task.Id);
            task.status = true;
            _helperSvc.updateTask(task);
            //execute Task


            //update task

            task.status = false;
            task.LastRun = CurrentRequestData.Now;

            return _helperSvc.updateTask(task);
        }

        private bool PortalImport(TaskShit Task)
        {
            TaskShit task = _helperSvc.getTask(Task.Id);
            task.status = true;
            _helperSvc.updateTask(task);
            //execute Task

            //update task

            task.status = false;
            task.LastRun = CurrentRequestData.Now;

            return _helperSvc.updateTask(task);
        }

    }
}