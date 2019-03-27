using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities.People;
using MrCMS.Logging;
using MrCMS.Web.Apps.Core.Entities;
using Elmah;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Services
{
    public interface ISmsService
    {
        bool SendSms(Sms sms);
        IList<Sms> Getallmessages();
        bool Savemessage(Sms sms);
        bool Delete(Sms sms);
        Sms GetMessage(int id);
        bool UpdateMessage(Sms sms);


        bool SaveSmsConfig(SmsConfig config);
        SmsConfig GetConfig();
        bool UpdateSmsConfig(SmsConfig config);



    }
}