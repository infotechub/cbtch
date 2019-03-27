using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Services.Resources;
using MrCMS.Web.Apps.Core.Models;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Pages;
using MrCMS.Website;
using MrCMS.Website.Controllers;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Services.UserChat;

namespace MrCMS.Web.Apps.Core.Controllers
{
    public class UserAccountController : MrCMSAppUIController<CoreApp>
    {
        private readonly IUserService _userService;
        private readonly IAuthorisationService _authorisationService;
        private readonly IStringResourceProvider _stringResourceProvider;
        private IPasswordManagementService _passwordManagementService;
        private readonly IUserChat _chatservice;
        public UserAccountController(IUserService userService, IPasswordManagementService passwordManagementService, IAuthorisationService authorisationService, IStringResourceProvider stringResourceProvider, IUserChat UserChat)
        {
            _userService = userService;
            _passwordManagementService = passwordManagementService;
            _authorisationService = authorisationService;
            _stringResourceProvider = stringResourceProvider;
            _chatservice = UserChat;
        }

        public ActionResult Show(UserAccountPage page)
        {
            ViewData["message"] = TempData["message"];

            if (CurrentRequestData.CurrentUser != null)
            {
                return View(page);
            }

            return Redirect(UniquePageHelper.GetUrl<LoginPage>());
        }

        [HttpGet]
        public ActionResult UserAccountDetails(UserAccountModel model)
        {
            User user = CurrentRequestData.CurrentUser;
            if (user != null)
            {
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.Email = user.Email;
                model.MobilePhone = user.Mobilephone;
                model.CugMobilePhone = user.CugMobilephone;
                model.Dob = user.Dob;
                ModelState.Clear();
                return View(model);
            }
            return Redirect(UniquePageHelper.GetUrl<LoginPage>());
        }

        [HttpPost]
        [ActionName("UserAccountDetails")]
        public async Task<RedirectResult> UserAccountDetails_POST(UserAccountModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                User user = CurrentRequestData.CurrentUser;
                if (user != null && user.IsActive)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.Mobilephone = model.MobilePhone;
                    user.CugMobilephone = model.CugMobilePhone;
                    user.Dob = model.Dob;
                    _userService.SaveUser(user);
                    await _authorisationService.SetAuthCookie(user, false);

                    return Redirect(UniquePageHelper.GetUrl<UserAccountPage>());
                }
            }
            return Redirect(UniquePageHelper.GetUrl<UserAccountPage>());
        }

        public JsonResult IsUniqueEmail(string email)
        {
            if (_userService.IsUniqueEmail(email, CurrentRequestData.CurrentUser.Id))
                return Json(true, JsonRequestBehavior.AllowGet);

            return Json(_stringResourceProvider.GetValue("Register Email Already Registered", "Email already registered."), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            ModelState.Clear();
            return View(model);
        }

        [HttpPost]
        [ActionName("ChangePassword")]
        public RedirectResult ChangePassword_POST(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                User user = CurrentRequestData.CurrentUser;
                _passwordManagementService.SetPassword(user, model.Password, model.ConfirmPassword);
                model.Message = _stringResourceProvider.GetValue("Login Password Updated", "Password updated.");

            }
            else
            {
                model.Message = _stringResourceProvider.GetValue("Login Invalid", "Please ensure both fields are filled out and valid");
            }

            TempData["message"] = model.Message;
            return Redirect(UniquePageHelper.GetUrl<UserAccountPage>());
        }



        public JsonResult GetAlLusers()
        {
            System.Collections.Generic.IList<User> users = _chatservice.Getallusers();

            var userlist = from aereply in users
                           select new
                           {
                               Id = aereply.Id,
                               Name = aereply.Name.ToUpper(),
                           };



            return Json(userlist, JsonRequestBehavior.AllowGet);
        }
    }
}