using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Models;
using MrCMS.Web.Apps.Core.Models.Plan;

namespace MrCMS.Web.Apps.Core.MapperConfig
{
    public class AutoMapperConfig
    {

        public class ConfigurationFile
        {

            public static MapperConfiguration Config { get; set; }
            public static MapperConfiguration Config2 { get; set; }

            public static object ConvertPlan(object input, Type output)
            {
                if (input.GetType() == typeof(Plan) && output == typeof(PlanVm))
                {
                    //conver to View model from entity model
                    Plan inpuT = (Plan)input;
                    PlanVm vm = new PlanVm()
                    {
                        Id = inpuT.Id,
                        Name = inpuT.Name,
                        Code = inpuT.Code,
                        Description = inpuT.Description,
                        Status = inpuT.Status,
                        CreatedOn = inpuT.CreatedOn,

                    };

                    return vm;

                }
                else if (input.GetType() == typeof(PlanVm) && output == typeof(Plan))
                {
                    PlanVm inpuT = (PlanVm)input;
                    Plan md = new Plan()
                    {
                        Id = inpuT.Id,
                        Name = inpuT.Name,
                        Code = inpuT.Code,
                        Description = inpuT.Description,
                        Status = inpuT.Status,
                        CreatedOn = inpuT.CreatedOn

                    };

                    return md;
                }
                else
                {
                    return null;
                }




            }
        }
    }
}