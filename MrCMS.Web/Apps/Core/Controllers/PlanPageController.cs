using System.Collections.Generic;
using System.Web.Mvc;
using System;
using System.Collections;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.ModelBinders;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Models.Search;
using MrCMS.Web.Apps.Core.Models.Plan;
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
    public class PlanPageController : MrCMSAppUIController<CoreApp>
    {
        private readonly IPlanService _planService;
        private readonly IUniquePageService _uniquePageService;
        private readonly IPageMessageSvc _pageMessageSvc;

        public PlanPageController(IPlanService planService, IUniquePageService uniquepageService, IPageMessageSvc pageMessageSvc)
        {
            _planService = planService;
            _uniquePageService = uniquepageService;
            _pageMessageSvc = pageMessageSvc;
        }

        [ActionName("Show")]
        public ActionResult Show(PlanPage page)
        {
            //get all plans
            page.Plans = _planService.GetallPlans();

            return View(page);
        }
        //Action for Add View
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(PlanVm plan)
        {
            //set the created user
            bool response = _planService.AddnewPlan(plan);
            if (response)
            {
                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Plan [{0}] was added successfully.", plan.Name.ToUpper()));
                //Session["PageSuccessMessage"] = string.Format("Service [{0}] was added successfully.", services.Name.ToUpper());

            }
            else
            {
                //there was an error
                //Set the Error message for user to see 
                //Session["PageErrorMessage"] = string.Format("There was an error adding service [{0}] ", services.Name.ToUpper());
                _pageMessageSvc.SetErrormessage(string.Format("There was an error adding plan [{0}] ",
                                                              plan.Name.ToUpper()));
            }

            return _uniquePageService.RedirectTo<PlanPage>();



        }
        [HttpGet]
        public ActionResult Add()
        {
            return PartialView("AddPlan");
        }
        //Action for Edit View
        [HttpGet]
        public ActionResult Edit(int id)
        {
            PlanVm plan = _planService.GetPlan(id);
            return PartialView("EditPlan", plan);
        }

        [HttpPost]
        public ActionResult Edit(PlanVm plan)
        {
            _planService.UpdatePlan(plan);
            return _uniquePageService.RedirectTo<PlanPage>();
        }

        //Action for Delete View

        [HttpGet]
        public ActionResult Delete(int id)
        {
            PlanVm item = _planService.GetPlan(id);
            return PartialView("DeletePlan", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(PlanVm plan)
        {
            if (plan.Id > 0)
            {

                bool response = _planService.DeletePlan(plan);
                if (response)
                {
                    _pageMessageSvc.SetSuccessMessage(string.Format("Plan [{0}] was deleted successfully.", plan.Name.ToUpper()));
                }
                else
                {
                    _pageMessageSvc.SetErrormessage(string.Format("There was an error deleting plan [{0}] ",
                                                                  plan.Name.ToUpper()));
                }
            }
            return _uniquePageService.RedirectTo<PlanPage>();
        }

    }
}
