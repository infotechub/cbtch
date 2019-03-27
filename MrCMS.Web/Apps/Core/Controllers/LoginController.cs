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
using ResetPasswordViewModel = MrCMS.Web.Apps.Core.Models.RegisterAndLogin.ResetPasswordViewModel;
using MrCMS.Website;

namespace MrCMS.Web.Apps.Core.Controllers
{
    public class LoginController : MrCMSAppUIController<CoreApp>
    {
        private readonly IAuthorisationService _authorisationService;
        private readonly IUniquePageService _uniquePageService;
        private readonly ILoginService _loginService;
        private readonly IStringResourceProvider _stringResourceProvider;
        private readonly IUserService _userService;
        private readonly IResetPasswordService _resetPasswordService;

        public LoginController(IUserService userService, IResetPasswordService resetPasswordService, IAuthorisationService authorisationService, IUniquePageService uniquePageService,
            ILoginService loginService, IStringResourceProvider stringResourceProvider)
        {
            _userService = userService;
            _resetPasswordService = resetPasswordService;
            _authorisationService = authorisationService;
            _uniquePageService = uniquePageService;
            _loginService = loginService;
            _stringResourceProvider = stringResourceProvider;
        }

        [HttpGet]
        public ViewResult Show(LoginPage page, LoginModel model)
        {
            ModelState.Clear();
            ViewData["login-model"] = TempData["login-model"] as LoginModel ?? model;
            return View(page);
        }



        [HttpPost]
        public async Task<RedirectResult> Post([IoCModelBinder(typeof(LoginModelModelBinder))]LoginModel loginModel)
        {
            if (loginModel != null && ModelState.IsValid)
            {
                LoginResult result = await _loginService.AuthenticateUser(loginModel);

                loginModel.Message = result.Message;
                if (result.Success)
                {
                    //update the userlogin 
                    //use this to update lastlogin for user
                    User user = CurrentRequestData.CurrentUser;
                    user.LastLoginDate = CurrentRequestData.Now;
                    _userService.SaveUser(user);

                    return Redirect(result.RedirectUrl);

                }
                else
                {
                    loginModel.Message = result.Message;

                    if (!string.IsNullOrEmpty(result.RedirectUrl))
                    {
                        return Redirect(result.RedirectUrl);
                    }


                }

                //else if (result.Success)
                //{
                //    loginModel.Message = "You are being redirected to our new site.";
                //    return Redirect("http://197.255.55.54");
                //}



            }
            TempData["login-model"] = loginModel;

            return _uniquePageService.RedirectTo<LoginPage>();
        }

        [HttpGet]
        public ViewResult ForgottenPassword(ForgottenPasswordPage page)
        {
            ViewData["message"] = TempData["message"];
            return View(page);
        }

        [HttpPost]
        public ActionResult ForgottenPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["message"] = _stringResourceProvider.GetValue("Login Email Not Recognized", "Email not recognized.");
                return _uniquePageService.RedirectTo<ForgottenPasswordPage>();
            }

            User user = _userService.GetUserByEmail(email);

            if (user != null)
            {
                _resetPasswordService.SetResetPassword(user);
                TempData["message"] =
                _stringResourceProvider.GetValue("Login Password Reset", "We have sent password reset details to you. Please check your spam folder if this is not received shortly.");
            }
            else
            {
                TempData["message"] = _stringResourceProvider.GetValue("Login Email Not Recognized", "Email not recognized.");
            }

            return _uniquePageService.RedirectTo<ForgottenPasswordPage>();
        }




        [HttpGet]
        public ActionResult PasswordReset(ResetPasswordPage page, Guid id)
        {
            User user = _userService.GetUserByResetGuid(id);

            if (user == null)
                return Redirect("~");

            ViewData["ResetPasswordViewModel"] = new ResetPasswordViewModel(id, user);

            return View(page);
        }

        [HttpPost]
        public RedirectResult PasswordReset(ResetPasswordViewModel model)
        {
            try
            {
                _resetPasswordService.ResetPassword(model);
                return _uniquePageService.RedirectTo<LoginPage>();
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return _uniquePageService.RedirectTo<LoginPage>();
            }
        }
    }
}