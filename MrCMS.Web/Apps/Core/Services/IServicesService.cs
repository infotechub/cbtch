using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using MrCMS.Entities.People;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Models.Services;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Apps.Core;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Faqs.Models;

namespace MrCMS.Web.Apps.Core.Services
{
    public interface IServicesService
    {
        IList<Service> GetallServices();
        bool AddnewService(ServiceVm service);
        bool DeleteService(ServiceVm service);
        bool UpdateService(ServiceVm service);
        ServiceVm GetService(int id);

    }
}