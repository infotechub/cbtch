using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MrCMS.Tasks;
using GetShortCode;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Services;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Website;
using MrCMS.Logging;
using MrCMS.Web.Areas.Admin.Services;

namespace MrCMS.Web.Apps.Core.Tasks
{
    public class SendBirthDayMessageTask : SchedulableTask
    {
        private readonly IHelperService _helperSvc;
        private readonly IEnrolleeService _enrolleeService;
        private readonly ISmsService _smsservice;
        private readonly IProviderService _providerservice;
        private readonly ILogAdminService _logger;
        public SendBirthDayMessageTask(IHelperService helperService, ISmsService smsSvc, IEnrolleeService enrolleeService, ILogAdminService loggersvc, IProviderService providersvc)
        {
            _helperSvc = helperService;
            _enrolleeService = enrolleeService;
            _smsservice = smsSvc;
            _logger = loggersvc;
            _providerservice = providersvc;
        }
        public override int Priority
        {
            get { return 0; }
        }

        protected override void OnExecute()
        {


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
                    string msg = string.Format("Dear {0} ,We noticed from our E-VS platform that you recently assessed care at {1}.We hope you enjoyed their services.If you have any complaints,kindly call or text any of these numbers.08179709298,07053173582,07053173582,08179711606.", (enrollee.Surname + " " + enrollee.Othernames).ToUpper(), provider.Name.ToUpper());


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
        }


    }
}