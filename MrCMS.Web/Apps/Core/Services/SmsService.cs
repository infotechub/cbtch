using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Logging;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Apps.Core.Models;
using MrCMS.Web.Apps.Core;
using MrCMS.Web.Areas.Admin.Services;
using MrCMS.Website;
using NHibernate;
using NHibernate.Transform;
using SmsEngine;

namespace MrCMS.Web.Apps.Core.Services
{
    public class SmsService : ISmsService
    {
        private readonly ISession _session;
        private readonly ILogAdminService _logger;
        private readonly IUserService _userService;
        private readonly IHelperService _helpersvc;
        public SmsService(ISession session, ILogAdminService log, IUserService userserv, IHelperService helpersvc)
        {
            _session = session;
            _logger = log;
            _userService = userserv;
            _helpersvc = helpersvc;

        }
        public bool SendSms(Sms sms)
        {

            SmsConfig config = GetConfig();
            if (config != null)
            {
                //"itsupport@novohealthafrica.org:project", "novohealth123"
                SmsProxy smsengine = new SmsProxy(config.UserName, config.Password);
                List<string> tonumber = new List<string>();
                tonumber.Add(sms.Msisdn);
                sms.Status = SmsStatus.Pending;
                SmsEngine.ServiceReference1.ResponseInfo resp = SmsProxy.SendSms(sms.FromId, tonumber, sms.Message, Convert.ToDateTime(sms.DeliveryDate));

                sms.ServerCode = resp.ErrorCode.ToString();

                if (resp.ErrorCode.Equals(0))
                {
                    sms.ServerResponse = resp.ExtraMessage;
                    sms.Status = SmsStatus.Delivered;

                }
                else
                {
                    sms.ServerResponse = resp.ErrorMessage;
                }

                Savemessage(sms);
                return true;
            }
            return false;

        }

        public IList<Sms> Getallmessages()
        {
            return _session.QueryOver<Sms>().Where(x => x.IsDeleted == false).List<Sms>();
        }

        public bool Savemessage(Sms sms)
        {
            if (sms != null)
            {
                _session.Transact(session => session.Save(sms));
                //_helpersvc.Log(LogEntryType.Audit, null,
                //               string.Format(
                //                   "New Sms saved {0} , SMS ID {1}, by {2}",
                //                   sms.Id, CurrentRequestData.CurrentUser.Id.ToString()), "Sms Added.");

                return true;
            }
            return false;

        }

        public bool Delete(Sms sms)
        {
            if (sms != null)
            {

                _session.Transact(session => session.Delete(sms));

                _helpersvc.Log(LogEntryType.Audit, null,
                               string.Format(
                                   "Sms Was deleted {0}, by {1}",
                                   sms.Id, CurrentRequestData.CurrentUser.Id.ToString()), "Sms Deleted.");
                return true;
            }
            return false;
        }

        public Sms GetMessage(int id)
        {

            Sms message = _session.QueryOver<Sms>().Where(x => x.Id == id).SingleOrDefault();
            return message;
        }

        public bool UpdateMessage(Sms sms)
        {
            if (sms != null)
            {

                _session.Transact(session => session.Update(sms));

                _helpersvc.Log(LogEntryType.Audit, null,
                               string.Format(
                                   "Sms was updated successfully {0}  id {1}, by {1}",
                                   sms.Id, CurrentRequestData.CurrentUser.Id.ToString()), "Sms Updated.");
                return true;
            }
            return false;
        }

        public bool SaveSmsConfig(SmsConfig config)
        {
            if (config != null)
            {
                _session.Transact(session => session.Save(config));
                //_helpersvc.Log(LogEntryType.Audit, null,
                //               string.Format(
                //                   "New Sms saved {0} , SMS ID {1}, by {2}",
                //                   sms.Id, CurrentRequestData.CurrentUser.Id.ToString()), "Sms Added.");

                return true;
            }
            return false;
        }

        public SmsConfig GetConfig()
        {
            SmsConfig config = _session.QueryOver<SmsConfig>().Take(1).SingleOrDefault();
            return config;
        }

        public bool UpdateSmsConfig(SmsConfig config)
        {
            if (config != null)
            {

                _session.Transact(session => session.Update(config));

                _helpersvc.Log(LogEntryType.Audit, null,
                               string.Format(
                                   "Sms  configuration was updated successfully {0}  id {1}, by {1}",
                                   config.Id, CurrentRequestData.CurrentUser.Id.ToString()), "Sms config Updated.");
                return true;
            }
            return false;
        }
    }
}