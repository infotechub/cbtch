using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Elmah;
using MrCMS.Entities.Notifications;
using MrCMS.Entities.People;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Services.Notifications;
using MrCMS.Services.Resources;
using MrCMS.Web.Apps.Core.ModelBinders;
using MrCMS.Web.Apps.Core.Models;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Pages;
using MrCMS.Web.Apps.Core.Services;
using MrCMS.Website;
using MrCMS.Website.Controllers;


namespace MrCMS.Web.Apps.Core.Controllers
{
    public class UserNotificationController : MrCMSAppUIController<CoreApp>
    {

        private readonly INotificationHubService _service;

        public UserNotificationController(INotificationHubService service)
        {
            _service = service;
        }


        public JsonResult Get()
        {

            System.Collections.Generic.IList<Entities.UserNotification> reply = _service.GetNotifications();
            JsonResult response = Json(reply, JsonRequestBehavior.AllowGet);


            return response;
        }

        public JsonResult GetCount()
        {
            return Json(_service.GetNotificationCount(), JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult MarkAllAsRead()
        {
            _service.MarkAllAsRead(CurrentRequestData.CurrentUser);
            return Json(true);
        }
        [HttpGet]
        public JsonResult Create()
        {
            User user = CurrentRequestData.CurrentUser;

            //_service.PushUserNotification("d4e17231-0929-4ca7-a25c-fea654d5d819", "Welcome to tony's world", user.Roles.Single(), Core.Utility.NotificationType.Persistent);
            return Json(true);
        }
    }
}