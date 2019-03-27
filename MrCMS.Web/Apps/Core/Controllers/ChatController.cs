using System;
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
using MrCMS.Website.Binders;
using MrCMS.Website.Controllers;
using MrCMS.Web.Apps.Core.Pages;
using MrCMS.Web.Apps.Core.Services.UserChat;
namespace MrCMS.Web.Apps.Core.Controllers
{
    public class ChatController : MrCMSAppUIController<CoreApp>
    {
        private readonly IUserChat _userchat;
        public ChatController(IUserChat userchat)
        {
            _userchat = userchat;
        }
        [HttpGet]
        public ActionResult Show()
        {
            return View();
        }


        //Returns the list of everyone on the network. including the Admins
        [HttpGet]
        public ActionResult GetContacts()
        {
            Widgets.RightSideBarWidget model = new Widgets.RightSideBarWidget();
            //var result=_userchat.Getallusers();
            //model.GetUsers = result;
            return PartialView("ContactPartial", model);
        }
    }
}