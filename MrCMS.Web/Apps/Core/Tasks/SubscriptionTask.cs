using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Tasks;
using MrCMS.Web.Apps.Core.MessageTemplates;
using MrCMS.Web.Apps.Core.Services;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Areas.Admin.Services;
using MrCMS.Website;
using System.Text;
using MrCMS.Entities.Messaging;
using MrCMS.Settings;

namespace MrCMS.Web.Apps.Core.Tasks
{
    public class SubscriptionTask : SchedulableTask
    {
        private readonly IPlanService _planService;
        private readonly IServicesService _serviceSvc;
        private readonly IUniquePageService _uniquePageService;
        private readonly IPageMessageSvc _pageMessageSvc;
        private readonly IHelperService _helperSvc;
        private readonly IProviderService _providerSvc;
        private readonly ICompanyService _companySvc;
        private readonly ILogAdminService _logger;
        private readonly UserService _userservice;
        private readonly IRoleService _rolesvc;
        private readonly IEmailSender _emailSender;
        private readonly MailSettings _mailSettings;
        private readonly IEnrolleeService _enrolleeService;


        public SubscriptionTask(IPlanService planService, IUniquePageService uniquepageService, IPageMessageSvc pageMessageSvc, IHelperService helperService, IServicesService serviceSvc, IProviderService Providersvc, ILogAdminService logger, ICompanyService companyService, UserService userService, IRoleService roleService, IEmailSender emailSender, MailSettings mailSettings, IEnrolleeService enrolleeService)
        {
            _planService = planService;
            _uniquePageService = uniquepageService;
            _pageMessageSvc = pageMessageSvc;
            _helperSvc = helperService;
            _serviceSvc = serviceSvc;
            _providerSvc = Providersvc;
            _logger = logger;
            _companySvc = companyService;
            _userservice = userService;
            _rolesvc = roleService;
            _emailSender = emailSender;
            _mailSettings = mailSettings;

            _enrolleeService = enrolleeService;
        }
        public override int Priority
        {
            get { return 0; }
        }

        protected override void OnExecute()
        {



            //companySvc.ExecuteSubscriptionCheck();

            //activate them old shit

            IList<Entities.Subscription> activate = _companySvc.GetNewlyApprovedActiveSubscription();

            foreach (Entities.Subscription item in activate)
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


            IList<Entities.Subscription> expiringsoon = _companySvc.GetSubscriptionExpiringSoon();
            IList<Entities.Subscription> expiredd = _companySvc.GetexpiredSubscriptions();
            IEnumerable<Entities.Subscription> expired = expiringsoon.Where(x => x.Expirationdate < CurrentRequestData.Now);

            foreach (Entities.Subscription item in expiredd)
            {
                item.Status = (int)SubscriptionStatus.Expired;
                _companySvc.UpdateSubscription(item);
            }

            StringBuilder bodyText = new StringBuilder();

            bodyText.AppendLine("The following companies are about to expire.");
            bodyText.AppendLine(Environment.NewLine);


            foreach (Entities.Subscription sub in expiringsoon)
            {
                Entities.Company company = _companySvc.GetCompany(sub.CompanyId);


                bodyText.AppendFormat("{0} ------- {1} {2}", company.Name.ToUpper(), sub.SubscriptionCode, Convert.ToDateTime(sub.Expirationdate).ToShortDateString());
                bodyText.AppendLine(Environment.NewLine);

            }
            bodyText.AppendLine(">Thank You");

            UserRole role = _rolesvc.GetRoleByName("CLIENT SERVICE");


            if (role != null && expiringsoon.Any())
            {
                foreach (User user in role.Users)
                {
                    //each user
                    QueuedMessage emailmsg = new QueuedMessage();
                    emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                    emailmsg.ToAddress = user.Email;
                    emailmsg.Subject = "NovoHub Subscription Notice ";
                    emailmsg.FromName = "NOVOHUB";
                    emailmsg.Body = bodyText.ToString();

                    _emailSender.AddToQueue(emailmsg);
                }
            }


            //Check for Previously Expired
            //Check if the subscription has ended

            //var endingsubscription = _companySvc.GetSubscriptionExpiringSoon();

            //foreach (var ending in endingsubscription)
            //{

            //    //Send Expiration Email
            //    var notification = _helperSvc.GetNotificationTable(4);
            //    //send notification for each guy in the role to notify that a new staff was added.
            //    var message = "";

            //    message = String.Format(CurrentRequestData.Now > ending.Expirationdate ? "Subscription [{1}] for {0}  has expired." : "Subscription [{1}] for {0}  is expiring soon.", _companySvc.GetCompany(Convert.ToInt32(ending.CompanyId)).Name, ending.SubscriptionCode);

            //    foreach (var role in notification.Roles.Split(','))
            //    {
            //        int roleint = 0;
            //        if (int.TryParse(role, out roleint))
            //        {

            //            var rolee = _rolesvc.GetRole(roleint);
            //            _helperSvc.PushUserNotification(string.Empty, message, NotificationTarget.Role, rolee,
            //                                                        NotificationType.Persistent, _uniquePageService.GetUniquePage<Pages.SubscriptionPage>().AbsoluteUrl);

            //        }

            //    }

            //    if (CurrentRequestData.Now > ending.Expirationdate)
            //    {
            //        var plans = ending.Companyplans.Split(',');

            //        if (plans.Any())
            //        {
            //            foreach (var plan in plans)
            //            {
            //                //Disable the enrollees under the plan. 

            //                if (!string.IsNullOrEmpty(plan.Trim()))
            //                {
            //                    _companySvc.DisableEnrolleeUnderCompanyPlan(Convert.ToInt32(plan));
            //                }

            //            }
            //        }


            //        //update ending and set status to expired.
            //        ending.Status = (int)SubscriptionStatus.Expired;
            //        _companySvc.UpdateSubscription(ending);


            //        var company = _companySvc.GetCompany(ending.CompanyId);
            //        if (company != null)
            //        {
            //            company.SubscriptionStatus = (int)SubscriptionStatus.Expired;
            //            _companySvc.UpdateCompany(company);
            //        }
            //    }





            //}




            //Update all new Approved Subscription to Active.

            //var allnewlyapproved = _companySvc.GetNewlyApprovedActiveSubscription();
            //foreach (var item in allnewlyapproved)
            //{//Set the status to active then update.
            //    item.Status = (int)SubscriptionStatus.Active;
            //    _companySvc.UpdateSubscription(item);

            //}


            //Get all active subscription and update the enrollees to update.

            //var allactive = _companySvc.GetAllActiveSubscription();


            //foreach (var active in allactive)
            //{
            //    var plans = active.Companyplans.Split(',');

            //    foreach (var plan in plans)
            //    {
            //        if (!string.IsNullOrEmpty(plan.Trim()))
            //        {
            //            _companySvc.EnableEnrolleeUnderCompanyPlan(Convert.ToInt32(plan));
            //        }
            //    }

            //    var company = _companySvc.GetCompany(active.CompanyId);
            //    if (company != null)
            //    {
            //        company.SubscriptionStatus = (int)SubscriptionStatus.Active;
            //        _companySvc.UpdateCompany(company);
            //    }
            //}



            //update the company.



        }
    }
}