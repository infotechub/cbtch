using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using MrCMS.Entities.Multisite;
using MrCMS.Settings;
using MrCMS.Web.Apps.Core.MapperConfig;
using MrCMS.Web.Apps.Core.Models.Services;
using MrCMS.Web.Apps.Core.Models.Plan;
using MrCMS.Web.Apps.Faqs.Models;
using MrCMS.Website;
using MrCMS.Website.Controllers;
using NHibernate;
using Ninject;
using MrCMS.Web.Apps.Core.Entities;
namespace MrCMS.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : MrCMSApplication
    {
        protected override void OnApplicationStart()
        {
            //mapping the sh*t
            MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateMap<Service, ServiceVm>().ReverseMap());
            MapperConfiguration config2 = new MapperConfiguration(cfg => cfg.CreateMap<Plan, PlanVm>().ReverseMap());

            //set the global
            AutoMapperConfig.ConfigurationFile.Config = config;
            //set the global
            AutoMapperConfig.ConfigurationFile.Config2 = config2;
        }

    }
}