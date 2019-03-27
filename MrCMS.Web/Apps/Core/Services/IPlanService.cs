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
using MrCMS.Web.Apps.Core.Models.Plan;
namespace MrCMS.Web.Apps.Core.Services
{
    public interface IPlanService
    {
        IList<Plan> GetallPlans();
        bool AddnewPlan(PlanVm service);
        bool DeletePlan(PlanVm plan);
        bool UpdatePlan(PlanVm service);
        PlanVm GetPlan(int id);

    }
}