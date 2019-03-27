using System.Threading.Tasks;
using System.Web.Mvc;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Services.Resources;
using MrCMS.Web.Apps.Core.Models;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Pages;
using MrCMS.Website;
using MrCMS.Website.Controllers;
using MrCMS.Services;

namespace MrCMS.Web.Apps.NovoHub.Controllers
{
    public class NotificationController : MrCMSAppUIController<NovoHubApp>
    {
        private readonly IUserService _userService;
        private readonly IAuthorisationService _authorisationService;
        private readonly IStringResourceProvider _stringResourceProvider;
        private IPasswordManagementService _passwordManagementService;

        public ActionResult Index()
        {
            return View();
        }
        
    }
}