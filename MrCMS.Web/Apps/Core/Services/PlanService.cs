using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.MapperConfig;
using MrCMS.Web.Apps.Core.Models.Plan;
using MrCMS.Web.Apps.Core.Models.Services;
using MrCMS.Website;
using NHibernate;

namespace MrCMS.Web.Apps.Core.Services
{


    public class PlanService : IPlanService
    {
        private readonly ISession _session;
        public PlanService(ISession session)
        {
            _session = session;
        }
        public IList<Plan> GetallPlans()
        {
            IList<Plan> response = _session.QueryOver<Plan>().Where(x => x.IsDeleted == false).List<Plan>();

            if (response != null)
            {
                return response;
            }
            return new List<Plan>();
        }

        public bool AddnewPlan(PlanVm service)
        {
            //Add date Added

            Plan planMd = new Plan()
            {
                Name = service.Name,
                Code = service.Code,
                Description = service.Description,
                Status = service.Status,
                CreatedBy = CurrentRequestData.CurrentUser.Guid.ToString(),
                CreatedOn = DateTime.Now,


            };

            _session.Transact(session => session.Save(planMd));
            return true;
        }

        public bool DeletePlan(PlanVm plan)
        {
            if (plan != null)
            {
                //get the service using the id
                Plan planMd = _session.QueryOver<Plan>().Where(x => x.Id == plan.Id).SingleOrDefault();
                //update the plan
                planMd.IsDeleted = true;
                _session.Transact(session => session.Update(planMd));
            }
            return true;
        }

        public bool UpdatePlan(PlanVm plan)
        {
            Plan planMd = _session.QueryOver<Plan>().Where(x => x.Id == plan.Id).SingleOrDefault();

            if (planMd != null)
            {
                planMd.Name = plan.Name;
                planMd.Code = plan.Code;
                planMd.Description = plan.Description;
                planMd.Status = plan.Status;

                _session.Transact(session => session.Update(planMd));
                return true;
            }
            else
            {
                return false;
            }
        }

        public PlanVm GetPlan(int id)
        {
            Plan planMd = _session.QueryOver<Plan>().Where(x => x.Id == id).SingleOrDefault();
            if (planMd != null)
            {
                PlanVm planVmm = (PlanVm)AutoMapperConfig.ConfigurationFile.ConvertPlan(planMd, typeof(PlanVm));
                return planVmm;
            }
            return null;

            //var mapper = AutoMapperConfig.ConfigurationFile.Config2.CreateMapper();
            //var res= mapper.Map<PlanVm>(planMd);





        }
    }
}