using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Core.MapperConfig;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Models.Services;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Apps.Core;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Website;
using NHibernate;
using NHibernate.Transform;

namespace MrCMS.Web.Apps.Core.Services
{
    public class ServiceService : IServicesService
    {
        private readonly ISession _session;
        public ServiceService(ISession session)
        {
            _session = session;

        }
        public IList<Service> GetallServices()
        {

            return _session.QueryOver<Service>().Where(x => x.IsDeleted == false).List<Service>();

        }

        public bool AddnewService(ServiceVm service)
        {
            //Add date Added

            Service serviceMd = new Service
            {
                Name = service.Name,
                Code = service.Code,
                Description = service.Description,
                Status = service.Status,
                //Dateadded = DateTime.Now,
                CreatedBy = CurrentRequestData.CurrentUser.Id,



            };

            _session.Transact(session => session.Save(serviceMd));
            return true;
        }

        public bool DeleteService(ServiceVm service)
        {

            if (service != null)
            {
                //get the service using the id
                Service serviceMd = _session.QueryOver<Service>().Where(x => x.Id == service.Id).SingleOrDefault();
                //update the service
                serviceMd.IsDeleted = true;
                _session.Transact(session => session.Update(serviceMd));
            }
            return true;
        }

        public bool UpdateService(ServiceVm service)
        {
            Service serviceMd = _session.QueryOver<Service>().Where(x => x.Id == service.Id).SingleOrDefault();

            if (serviceMd != null)
            {
                serviceMd.Name = service.Name;
                serviceMd.Code = service.Code;
                serviceMd.Description = service.Description;
                serviceMd.Status = service.Status;

                _session.Transact(session => session.Update(serviceMd));
                return true;
            }
            else
            {
                return false;
            }

        }

        public ServiceVm GetService(int id)
        {
            Service service = _session.QueryOver<Service>().Where(x => x.Id == id).SingleOrDefault();
            AutoMapper.IMapper mapper = AutoMapperConfig.ConfigurationFile.Config.CreateMapper();
            return mapper.Map<ServiceVm>(service);

        }
    }
}