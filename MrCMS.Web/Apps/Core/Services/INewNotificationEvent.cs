using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Events;

namespace MrCMS.Web.Apps.Core.Services
{
    public interface INewNotificationEvent : IEvent
    {
    }
    public class NewNotificationArgs { public Entities.UserNotification Notification { get; set; } }

    public class VerificationCodeChangeArgs { public Entities.EnrolleeVerificationCode VerificationCode { get; set; } }

    public class AuthenticationCodeCreatedArgs { public Entities.AuthorizationCode AuthorizationCode { get; set; } }

    public class AuthRequestCodeCreatedArgs { public Entities.AuthorizationRequest AuthRequest { get; set; } }

    public class ProviderClaimBKArgs { public Entities.ProviderClaimBK ClaimBK { get; set; } }


    public class DeleteBillProviderPortalArgs
    {
        public string providerid { get; set; }
        public string key { get; set; }

    }
}