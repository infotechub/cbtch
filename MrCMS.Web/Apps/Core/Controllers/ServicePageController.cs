using System.Collections.Generic;
using System.Web.Mvc;
using System;
using System.Collections;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.ModelBinders;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Models.Search;
using MrCMS.Web.Apps.Core.Models.Services;
using MrCMS.Web.Apps.Core.Pages;
using MrCMS.Web.Apps.Core.Services;
using MrCMS.Web.Apps.Core.Services.Search;
using MrCMS.Web.Apps.Core.Services.Widgets;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Apps.Faqs.Models;
using MrCMS.Website;
using MrCMS.Website.Binders;
using MrCMS.Website.Controllers;
using MrCMS.Web.Apps.Core.Services;
using MrCMS.Web.Apps.Core.MapperConfig;
using AutoMapper;
namespace MrCMS.Web.Apps.Core.Controllers
{
    public class ServicesPageController : MrCMSAppUIController<CoreApp>
    {
        private readonly IServicesService _serviceService;
        private readonly IUniquePageService _uniquePageService;
        private readonly INotificationHubService _notificationHubService;
        private readonly IPageMessageSvc _pageMessageSvc;

        public ServicesPageController(IServicesService serviceService, IUniquePageService uniquepageService, INotificationHubService notificationHubService, IPageMessageSvc pageMessageSvc)
        {
            _serviceService = serviceService;
            _uniquePageService = uniquepageService;
            _notificationHubService = notificationHubService;
            _pageMessageSvc = pageMessageSvc;


        }

        [ActionName("Show")]
        public ActionResult Show(ServicesPage page)
        {

            page.Services = _serviceService.GetallServices();

            return View(page);
        }


        public JsonResult GetJson()
        {

            IList<Service> reply = _serviceService.GetallServices();
            JsonResult response = Json(reply, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                aaData = response.Data
            });


        }
        //Action for Add View
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(ServiceVm services)
        {
            //set the created user
            bool response = _serviceService.AddnewService(services);
            if (response)
            {
                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Service [{0}] was added successfully.", services.Name.ToUpper()));
                //Session["PageSuccessMessage"] = string.Format("Service [{0}] was added successfully.", services.Name.ToUpper());

            }
            else
            {
                //there was an error
                //Set the Error message for user to see 
                //Session["PageErrorMessage"] = string.Format("There was an error adding service [{0}] ", services.Name.ToUpper());
                _pageMessageSvc.SetErrormessage(string.Format("There was an error adding service [{0}] ",
                                                              services.Name.ToUpper()));
            }

            return _uniquePageService.RedirectTo<ServicesPage>();



        }
        [HttpGet]
        public ActionResult Add()
        {
            return PartialView("AddService");
        }
        //Action for Edit View
        [HttpGet]
        public ActionResult Edit(int id)
        {
            ServiceVm service = _serviceService.GetService(id);
            return PartialView("EditService", service);
        }

        [HttpPost]
        public ActionResult Edit(ServiceVm service)
        {
            _serviceService.UpdateService(service);
            return _uniquePageService.RedirectTo<ServicesPage>();
        }

        //Action for Delete View

        [HttpGet]
        public ActionResult Delete(int id)
        {
            ServiceVm item = _serviceService.GetService(id);
            return PartialView("DeleteService", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ServiceVm service)
        {
            if (service.Id > 0)
            {

                bool response = _serviceService.DeleteService(service);
                if (response)
                {
                    _pageMessageSvc.SetSuccessMessage(string.Format("Service [{0}] was deleted successfully.", service.Name.ToUpper()));
                }
                else
                {
                    _pageMessageSvc.SetErrormessage(string.Format("There was an error deleting service [{0}] ",
                                                                  service.Name.ToUpper()));
                }
            }
            return _uniquePageService.RedirectTo<ServicesPage>();
        }

    }
}
