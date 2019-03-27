using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Web.Mvc;
using System;
using System.Collections;
using System.Data.SqlTypes;
using MrCMS.Entities.Messaging;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.ModelBinders;
using MrCMS.Web.Apps.Core.Models.Plan;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Models.Search;
using MrCMS.Web.Apps.Core.Models.Provider;
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
using MrCMS.Logging;
using MrCMS.Web.Areas.Admin.Services;
using System.Linq;
using System.ServiceModel.Configuration;
using System.ServiceModel.Security;
using System.Threading;
using System.Web;
using Elmah;
using GetShortCode;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using OfficeOpenXml;
using System.Text;
using MrCMS.DbConfiguration.Mapping;
using MrCMS.Entities.Documents.Media;
using MrCMS.Web.Areas.Admin.Models;
using NHibernate.Hql.Ast;
using NHibernate.Loader.Collection;
using System.Text.RegularExpressions;
using MrCMS.Settings;

namespace MrCMS.Web.Apps.Core.Controllers
{
    public class CompanyPageController : MrCMSAppUIController<CoreApp>
    {
        private readonly IPlanService _planService;
        private readonly IServicesService _serviceSvc;
        private readonly IUniquePageService _uniquePageService;
        private readonly IPageMessageSvc _pageMessageSvc;
        private readonly IHelperService _helperSvc;
        private readonly IProviderService _providerSvc;
        private readonly ICompanyService _companySvc;
        private readonly ILogAdminService _logger;
        private readonly UserService _userservice;
        private readonly IRoleService _rolesvc;
        private readonly IEmailSender _emailSender;
        private readonly IEnrolleeService _enrolleeService;
        private readonly IFileAdminService _fileService;
        private readonly IFileAdminService _fileAdminService;
        private readonly MailSettings _mailSettings;

        public CompanyPageController(IPlanService planService, IUniquePageService uniquepageService, IPageMessageSvc pageMessageSvc, IHelperService helperService, IServicesService serviceSvc, IProviderService Providersvc, ILogAdminService logger, ICompanyService companyService, UserService userService, IRoleService roleService, IEmailSender emailSender, IEnrolleeService enrolleeService, IFileAdminService fileService, IFileAdminService fileAdminService, MailSettings mailSettings)
        {

            _mailSettings = mailSettings;
            _emailSender = emailSender;
            _planService = planService;
            _uniquePageService = uniquepageService;
            _pageMessageSvc = pageMessageSvc;
            _helperSvc = helperService;
            _serviceSvc = serviceSvc;
            _providerSvc = Providersvc;
            _logger = logger;
            _companySvc = companyService;
            _userservice = userService;
            _rolesvc = roleService;

            _enrolleeService = enrolleeService;
            _fileService = fileService;
            _fileAdminService = fileAdminService;
            ////remove once live
            //var roles = from tony in _rolesvc.GetAllRoles()
            //            select new
            //            {
            //                id = tony.Id.ToString()
            //            };
            //var str = roles.Aggregate("", (current, item) => current + item.id + ",");
            //var test = _helperSvc.AddNotificationTable(4,
            //                                           "This is the subscription notification ,for the client service head  to notify them of a new subscription.",
            //                                           str, true);

            //var message = new QueuedMessage();
            //message.FromAddress = "anthonya@novohealthafrica.org";
            //message.ToAddress = "asije.anthony@gmail.com";
            //message.Subject = "Hello";
            //message.FromName = "Tony";

            //emailSender.SendMailMessage(message);

        }

        [ActionName("Show")]
        public ActionResult Show(CompanyListPage page)
        {
            //Get the companies
            List<State> states = new List<State>();
            IList<Company> companies = _companySvc.GetallCompany();

            List<CompanyObj> companynewlist = new List<CompanyObj>();
            IEnumerable<State> statess = _helperSvc.GetallStates();
            page.Companies = companies;
            states.Add(new State() { Id = -1, Name = "--SELECT--" });
            foreach (State item in statess)
            {

                states.Add(item);
            }


            ViewBag.Statelist = states;


            return View(page);
        }



        public string ExecuteBulkStaffTask(MediaCategory mediaCategory)
        {


            //get the list of guys that needs to be executed
            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
                    "App_Data");
            string foldername = Guid.NewGuid().ToString();
            string fullpath = Path.Combine(appdatafolder, foldername);
            Directory.CreateDirectory(fullpath);


            IList<StaffUploadJob> listjob = _companySvc.QueryPendingJobs();
            foreach (StaffUploadJob jobb in listjob)
            {

                try
                {
                    //set started
                    StaffUploadJob jobo = _companySvc.GetStaffUploadJob(jobb.Id);
                    jobo.JobStatus = JobStatus.Started;
                    jobo.StartTime = CurrentRequestData.Now;

                    _companySvc.UpdateStaffUploadJobs(jobo);

                    //Do what you can from here 
                    string file = jobb.filelink;
                    int company = jobb.CompanyID;
                    int subsidiary = jobb.Subsidiary;
                    List<staffshit> Newlist = new List<staffshit>();
                    //var NewList = new List<StaffnameandPlan>();

                    Dictionary<int, string> expungelist = new Dictionary<int, string>();
                    Dictionary<int, string> expungelistDependants = new Dictionary<int, string>();
                    Dictionary<string, string> ChangePlanList = new Dictionary<string, string>();
                    Dictionary<string, string> similarList = new Dictionary<string, string>();
                    Dictionary<string, string> SusidiaryMovedList = new Dictionary<string, string>();
                    List<string> duplicateslist = new List<string>();
                    //var duplicateslistSelf = new List<string>();
                    List<string> NewAddition = new List<string>();


                    IList<CompanyPlan> allplans = _companySvc.GetallplanForCompany(Convert.ToInt32(company));


                    if (file != null)
                    {

                        using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                        {
                            //excel open
                            if (package.Workbook.Worksheets.Count == 0)
                            {

                                //  _pageMessageSvc.SetErrormessage("Your Excel file does not contain any work sheets.");

                                jobo.JobStatus = JobStatus.Failed;
                                jobo.FinishTime = CurrentRequestData.Now;

                                _companySvc.UpdateStaffUploadJobs(jobo);
                                return "The workbook does not contain any workbook.";


                            }
                            int count = 1;
                            if (package.Workbook.Worksheets.Count > 0)
                            {
                                ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
                                ExcelCellAddress start = worksheet.Dimension.Start;
                                ExcelCellAddress end = worksheet.Dimension.End;
                                for (int row = start.Row; row <= end.Row; row++)
                                {
                                    // Row by row...

                                    string staffname = worksheet.Cells[row, 1].Text;

                                    string staffPlan = Regex.Replace(worksheet.Cells[row, 2].Text, @"[^\d]", "");

                                    if (!string.IsNullOrEmpty(staffname) && !string.IsNullOrEmpty(staffPlan) && allplans.Where(x => x.Id == Convert.ToInt32(staffPlan.Trim())).Count() == 0)
                                    {
                                        //the plan does not exist
                                        Log logg = new Log()
                                        {
                                            Detail = string.Format("The company plan does not exist in row {0}", row),
                                            Message = string.Format("The company plan does not exist in row {0}", row),
                                            Type = LogEntryType.Error

                                        };
                                        _logger.Insert(logg);

                                        jobo.JobStatus = JobStatus.Failed;
                                        jobo.FinishTime = CurrentRequestData.Now;

                                        _companySvc.UpdateStaffUploadJobs(jobo);
                                        return "The plan does not exist.";
                                    }
                                    int staffid = 0;
                                    string staffidcardno = "";
                                    try
                                    {
                                        staffidcardno = worksheet.Cells[row, 3].Text;
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    StaffnameandPlan item = new StaffnameandPlan();
                                    staffshit itemm = new staffshit
                                    {
                                        Id = -1,
                                        Name = staffname,
                                        staffPlan = staffPlan,
                                        staffidcardno = staffidcardno,

                                    };


                                    if (!string.IsNullOrEmpty(itemm.Name) && !string.IsNullOrEmpty(itemm.staffPlan))
                                    {
                                        Newlist.Add(itemm);
                                    }


                                }
                            }
                            if (jobb.ExpungeMode == JobExpungeMode.Renewal)
                            {
                                _companySvc.ExpundgeAllStaffInCompany(jobb.Subsidiary, jobb.Id);

                            }
                            //add the new guys 
                            if (jobb.ExpungeMode == JobExpungeMode.Additions || jobb.ExpungeMode == JobExpungeMode.Renewal)
                            {
                                foreach (staffshit item2 in Newlist)
                                {




                                    Staff staffnew = new Staff();
                                    staffnew.StaffFullname = item2.Name;
                                    staffnew.StaffPlanid = Convert.ToInt32(item2.staffPlan);
                                    staffnew.CompanyId = Convert.ToString(company);
                                    staffnew.CompanySubsidiary = subsidiary;
                                    staffnew.StaffId = item2.staffidcardno;

                                    if (!string.IsNullOrEmpty(staffnew.StaffId))
                                    {
                                        //the guy has a staff code

                                        Staff oldguy = _companySvc.GetstaffByCompanyStaffId(staffnew.StaffId);

                                        if (oldguy != null && Convert.ToInt32(oldguy.CompanyId) == jobb.CompanyID && oldguy.CompanySubsidiary == jobb.Subsidiary)
                                        {

                                            //restore the guy

                                            IList<Enrollee> enrollees = _enrolleeService.GetEnrolleesByStaffId(oldguy.Id);

                                            if (enrollees != null)
                                            {
                                                foreach (Enrollee enrollee in enrollees)
                                                {
                                                    //restore only the guys that were expunged here
                                                    if (enrollee != null && enrollee.Bulkjobid == jobb.Id)
                                                    {
                                                        enrollee.ExpungeNote = string.Empty;
                                                        enrollee.Status = (int)EnrolleesStatus.Active;
                                                        enrollee.Isexpundged = false;
                                                        enrollee.Expungedby = 0;
                                                        enrollee.Dateexpunged = CurrentRequestData.Now;

                                                        _enrolleeService.UpdateEnrollee(enrollee);

                                                    }
                                                }
                                            }
                                            //Expunge the staff by
                                            oldguy.StaffFullname = staffnew.StaffFullname;
                                            oldguy.StaffPlanid = staffnew.StaffPlanid;

                                            oldguy.IsExpunged = false;

                                            bool response = _companySvc.UpdateStaff(oldguy);



                                        }
                                        else
                                        {
                                            _companySvc.AddStaff(staffnew);
                                        }



                                    }
                                    else
                                    {
                                        _companySvc.AddStaff(staffnew);
                                    }
                                }



                                //update the staff in the subsidiary



                                jobo.JobStatus = JobStatus.Completed;
                                jobo.TotalRecord = Newlist.Count;
                                jobo.FinishTime = CurrentRequestData.Now;
                                jobo.TotalRecordDone = Newlist.Count;

                                jobo.TotalStaffForExpunged = 0;
                                _companySvc.UpdateStaffUploadJobs(jobo);
                                StringBuilder narrative = new StringBuilder();

                                User theuser = _userservice.GetUser(jobo.UploadedBy);
                                Company thecompany = _companySvc.GetCompany(jobo.CompanyID);

                                //set company renewal status
                                if (jobo.ExpungeMode == JobExpungeMode.Renewal && thecompany != null)
                                {
                                    thecompany.isRenewal = true;
                                    _companySvc.UpdateCompany(thecompany);
                                }

                                string username = theuser != null ? theuser.Name.ToUpper() : "Unknown";
                                string companyname = thecompany != null ? thecompany.Name.ToUpper() : "Unknown";

                                narrative.AppendFormat("Dear Sir,There was a bulk upload <b> {0} </b> for <b>{1} </b> by <b> {2} </b>. The upload Completed Successfully.Kindly follow up with Data Unit to complete the process.Thank you", jobo.ExpungeMode, companyname, username);
                                if (true)
                                {//_helperSvc.PushUserNotification 
                                    QueuedMessage emailmsg = new QueuedMessage();
                                    emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                                    emailmsg.ToAddress = "temidayok@novohealthafrica.org";
                                    emailmsg.Subject = "Bulk Upload Notification";
                                    emailmsg.FromName = "NovoHub Notification";
                                    emailmsg.Body = narrative.ToString();
                                    emailmsg.IsHtml = true;
                                    _emailSender.AddToQueue(emailmsg);


                                }

                                if (theuser != null)
                                {//_helperSvc.PushUserNotification 
                                    QueuedMessage emailmsg = new QueuedMessage();
                                    emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                                    emailmsg.ToAddress = theuser.Email;
                                    emailmsg.Subject = "Bulk Upload Notification";
                                    emailmsg.FromName = "NovoHub Notification";
                                    emailmsg.Body = narrative.ToString();
                                    emailmsg.IsHtml = true;
                                    _emailSender.AddToQueue(emailmsg);


                                }
                            }
                        }
                    }
                }
                catch (Exception ex)

                {

                    Log logg = new Log()
                    {
                        Detail = string.Format("There was an error {0}", ex.Message),
                        Message = ex.Message,
                        Type = LogEntryType.Audit

                    };
                    _logger.Insert(logg);

                }

            }

            return "Done";
        }


        //public string ExecuteBulkStaffTask(MediaCategory mediaCategory)
        //{
        //    //get the list of guys that needs to be executed
        //    string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
        //            "App_Data");
        //    var foldername = Guid.NewGuid().ToString();
        //    var fullpath = Path.Combine(appdatafolder, foldername);
        //    System.IO.Directory.CreateDirectory(fullpath);


        //    var listjob = _companySvc.QueryPendingJobs();
        //    foreach (var jobb in listjob)
        //    {

        //        try
        //        {



        //            //set started
        //            var jobo = _companySvc.GetStaffUploadJob(jobb.Id);
        //            jobo.JobStatus = JobStatus.Started;
        //            jobo.StartTime = CurrentRequestData.Now;

        //            _companySvc.UpdateStaffUploadJobs(jobo);

        //            //Do what you can from here 
        //            var file = jobb.filelink;
        //            var company = jobb.CompanyID;
        //            var subsidiary = jobb.Subsidiary;
        //            var Newlist = new List<staffshit>();
        //            //var NewList = new List<StaffnameandPlan>();

        //            var expungelist = new Dictionary<int, string>();
        //            var expungelistDependants = new Dictionary<int, string>();
        //            var ChangePlanList = new Dictionary<string, string>();
        //            var similarList = new Dictionary<string, string>();
        //            var SusidiaryMovedList = new Dictionary<string, string>();
        //            var duplicateslist = new List<string>();
        //            //var duplicateslistSelf = new List<string>();
        //            var NewAddition = new List<string>();

        //            var allstaffincompany = new List<StaffnameandPlan>();

        //            var allstaffnotinsubsidiary = new List<StaffnameandPlan>();
        //            var allplans = _companySvc.GetallplanForCompany(Convert.ToInt32(company));

        //            if (true)
        //            {

        //                //including those that have been expunged will be gotten


        //                //(List<StaffnameandPlan>)
        //                //_companySvc.GetAllStaffinCompanySubsidiaryLite(Convert.ToInt32(company), 2).Where(x => x.Expunged == false).ToList();
        //                var allstaffinentirecompany = (List<StaffnameandPlan>)_companySvc.GetAllStaffinCompanyLite(Convert.ToInt32(company)).Where(x => x.Expunged == false).ToList();




        //                allstaffincompany = allstaffinentirecompany.Where(x => x.Subsidiary == Convert.ToInt32(subsidiary)).ToList();
        //                allstaffnotinsubsidiary =
        //                      allstaffinentirecompany.Where(x => x.Subsidiary != Convert.ToInt32(subsidiary)).ToList();

        //                //(List<StaffnameandPlan>)
        //                //_companySvc.GetAllStaffinCompanySubsidiaryLite(Convert.ToInt32(company), 2).Where(x => x.Expunged == false).ToList();


        //            }

        //            if (file != null)
        //            {

        //                using (var package = new ExcelPackage(new FileInfo(file)))
        //                {
        //                    //excel open
        //                    if (package.Workbook.Worksheets.Count == 0)
        //                    {

        //                        //  _pageMessageSvc.SetErrormessage("Your Excel file does not contain any work sheets.");

        //                        jobo.JobStatus = JobStatus.Failed;
        //                        jobo.FinishTime = CurrentRequestData.Now;

        //                        _companySvc.UpdateStaffUploadJobs(jobo);
        //                        return "The workbook does not contain any workbook.";


        //                    }
        //                    var count = 1;
        //                    if (package.Workbook.Worksheets.Count > 0)
        //                    {
        //                        ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
        //                        var start = worksheet.Dimension.Start;
        //                        var end = worksheet.Dimension.End;
        //                        for (int row = start.Row; row <= end.Row; row++)
        //                        {
        //                            // Row by row...

        //                            var staffname = worksheet.Cells[row, 1].Text;
        //                            var staffPlan = worksheet.Cells[row, 2].Text;
        //                            var staffid = 0;
        //                            var staffidcardno = "";
        //                            try
        //                            {
        //                                staffidcardno = worksheet.Cells[row, 3].Text;
        //                            }
        //                            catch (Exception ex)
        //                            {

        //                            }

        //                            var item = new StaffnameandPlan();
        //                            var itemm = new staffshit
        //                            {
        //                                Id = -1,
        //                                Name = staffname,
        //                                staffPlan = staffPlan,
        //                                staffidcardno = staffidcardno,

        //                            };

        //                            Newlist.Add(itemm);

        //                        }
        //                    }

        //                    //where the real stuff happens 
        //                    var countt = 1;
        //                    //check in other subsidiary for the name so we can change subsidiary.
        //                    foreach (var item2 in Newlist)
        //                    {
        //                        foreach (var item in allstaffnotinsubsidiary)
        //                        {

        //                            if (Utility.Tools.compareNames(item2.Name.Trim(), item.Name.Trim()) > 50)
        //                            {

        //                                if (true)
        //                                {
        //                                    var staff = _companySvc.Getstaff(item.Id);
        //                                    if (staff != null)
        //                                    {

        //                                        //took out subsidiary change
        //                                        staff.CompanySubsidiary = Convert.ToInt32(subsidiary);
        //                                        _companySvc.UpdateStaff(staff);


        //                                        SusidiaryMovedList.Add($"{item.Name} {countt} -- {item.Subsidiary}",
        //                                            $"{item.Name} -- {subsidiary}");

        //                                        countt++;
        //                                        //ChangePlanList.Add(staff.StaffFullname,
        //                                        //    $"Plan changed from {item.PlanId} to {item2.staffPlan}");

        //                                    }
        //                                }
        //                                break;
        //                            }

        //                        }


        //                    }

        //                    //get the names again
        //                    var allstaffinentirecompany = (List<StaffnameandPlan>)_companySvc.GetAllStaffinCompanyLite(Convert.ToInt32(company)).Where(x => x.Expunged == false).ToList();




        //                    allstaffincompany = allstaffinentirecompany.Where(x => x.Subsidiary == Convert.ToInt32(subsidiary)).ToList();
        //                    allstaffnotinsubsidiary =
        //                          allstaffinentirecompany.Where(x => x.Subsidiary != Convert.ToInt32(subsidiary)).ToList();


        //                    foreach (var item2 in Newlist)
        //                    {
        //                        if (jobb.ExpungeMode == JobExpungeMode.Additions || jobb.ExpungeMode == JobExpungeMode.Renewal)
        //                        {
        //                            var staffnew = new Staff();
        //                            staffnew.StaffFullname = item2.Name;
        //                            staffnew.StaffPlanid = Convert.ToInt32(item2.staffPlan);
        //                            staffnew.CompanyId = Convert.ToString(company);
        //                            staffnew.CompanySubsidiary = subsidiary;
        //                            staffnew.StaffId = item2.staffidcardno;
        //                            _companySvc.AddStaff(staffnew);



        //                        }
        //                    }


        //                    foreach (var item in allstaffincompany)
        //                    {
        //                        var staffexist = false;



        //                        if (jobb.ExpungeMode == JobExpungeMode.Renewal)
        //                        {
        //                            //update staff and set isdeleted=1
        //                            var staff333 = _companySvc.Getstaff(item.Id);
        //                            staff333.StaffJobId = jobb.Id;
        //                            staff333.IsExpunged = true;
        //                            //staff333.IsDeleted = true;
        //                            _companySvc.UpdateStaff(staff333);
        //                            var enrollees = _enrolleeService.GetEnrolleesByStaffId(staff333.Id);

        //                            if (enrollees != null)
        //                            {
        //                                foreach (var enrollee in enrollees)
        //                                {
        //                                    if (enrollee != null)
        //                                    {
        //                                        enrollee.ExpungeNote = "automatic Staff Expunged.";
        //                                        enrollee.Status = (int)EnrolleesStatus.Inactive;
        //                                        enrollee.Isexpundged = true;
        //                                        enrollee.Expungedby = CurrentRequestData.CurrentUser.Id;
        //                                        enrollee.Dateexpunged = CurrentRequestData.Now;

        //                                        _enrolleeService.UpdateEnrollee(enrollee);

        //                                    }
        //                                }
        //                            }
        //                            //Expunge the staff by



        //                        }



        //                        foreach (var item2 in Newlist)
        //                        {

        //                            if (Utility.Tools.compareNames(item.Name.Trim(), item2.Name.Trim()) > 50)
        //                            {
        //                                staffexist = true;
        //                                if (item.PlanId != item2.staffPlan)
        //                                {
        //                                    var staff = _companySvc.Getstaff(item.Id);
        //                                    if (staff != null)
        //                                    {
        //                                        staff.StaffPlanid = Convert.ToInt32(item2.staffPlan);
        //                                        _companySvc.UpdateStaff(staff);

        //                                        ChangePlanList.Add(staff.StaffFullname,
        //                                            $"Plan changed from {item.PlanId} to {item2.staffPlan}");

        //                                    }
        //                                }


        //                                //change bring out the children for expunge

        //                                var allowdepend = _companySvc.GetCompanyPlan(Convert.ToInt32(item2.staffPlan));
        //                                if (allowdepend != null && allowdepend.AllowChildEnrollee == false)
        //                                {
        //                                    //delete all dependents

        //                                    var enrollees = _enrolleeService.GetEnrolleesByStaffId(item.Id);

        //                                    if (enrollees != null)
        //                                    {
        //                                        var childs = enrollees.Where(x => x.Parentid > 0).ToList();


        //                                        foreach (var child in childs)
        //                                        {

        //                                            if (!expungelistDependants.ContainsKey(child.Id))
        //                                            {
        //                                                expungelistDependants.Add(child.Id, child.Surname + " " + child.Othernames);
        //                                            }

        //                                        }

        //                                    }

        //                                }




        //                                break;
        //                            }

        //                        }


        //                        if (!staffexist)
        //                        {
        //                            //does not exit
        //                            expungelist.Add(item.Id, item.Name);

        //                        }

        //                    }



        //                    //Now loop through expunge list and check the new list to find similarity.
        //                    var counter = 1;
        //                    foreach (var item in expungelist)
        //                    {

        //                        foreach (var item2 in Newlist)
        //                        {
        //                            var percen = Utility.Tools.compareNames(item.Value.Trim(), item2.Name.Trim());

        //                            if (percen > 0 && percen <= 50)
        //                            {
        //                                //similar shit


        //                                similarList.Add(counter.ToString(), string.Format("{0} -- {1}", item.Value, item2.Name));
        //                                counter++;
        //                            }
        //                        }


        //                    }


        //                    //check the duplicate in the new  list


        //                    foreach (var item in Newlist)
        //                    {
        //                        var count2 = 0;
        //                        foreach (var item2 in Newlist)
        //                        {
        //                            var percen = Utility.Tools.compareNames(item.Name.Trim(), item2.Name.Trim());
        //                            if (percen > 70)
        //                            {
        //                                count2++;

        //                                if (!duplicateslist.Contains(item.Name.Trim() + " : " + item2.Name.Trim()) && count2 > 1)
        //                                {
        //                                    duplicateslist.Add(item.Name.Trim() + " : " + item2.Name.Trim());
        //                                }
        //                            }
        //                        }
        //                    }


        //                    //check the new addition

        //                    foreach (var item in Newlist)
        //                    {
        //                        var existhere = true;
        //                        foreach (var item2 in allstaffincompany)
        //                        {
        //                            var percen = Utility.Tools.compareNames(item.Name.Trim(), item2.Name.Trim());

        //                            if (percen < 50)
        //                            {

        //                                existhere = false;

        //                            }
        //                            else
        //                            {
        //                                existhere = true;
        //                                break;

        //                            }
        //                        }

        //                        if (!existhere)
        //                        {
        //                            if (!NewAddition.Contains(item.Name))
        //                            {
        //                                NewAddition.Add(string.Format("{0}:{1};", item.Name, item.staffPlan));
        //                            }
        //                        }
        //                    }


        //                    var response = new StringBuilder();

        //                    response.Append("Analysis for the Staff Upload");
        //                    response.Append(Environment.NewLine);
        //                    response.AppendFormat(
        //                        "Total No in old list is {4} and total in new List is {0} the total no to be expunged is {1} and total to be added is {2} kindly note you are adviced to look at those to be expuged carefully before you expunge.. Total Dulicates in new list {3}",
        //                        Newlist.Count, expungelist.Count, NewAddition.Count, duplicateslist.Count, allstaffincompany.Count);

        //                    response.AppendLine(Environment.NewLine);
        //                    response.Append("--TO BE EXPUNGED--");
        //                    foreach (var item in expungelist)
        //                    {
        //                        var obj = allstaffincompany.SingleOrDefault(x => x.Id == item.Key);

        //                        response.AppendLine(string.Format("Id:{0},name:{1},isExpunged:{2}", item.Key, item.Value,
        //                            obj.Expunged));

        //                    }

        //                    response.AppendLine("--TO BE ADDED--");
        //                    foreach (var item in NewAddition)
        //                    {
        //                        response.AppendLine(item.ToUpper());

        //                    }
        //                    response.Append("--SUBSIDIARY CHANGED--");
        //                    foreach (var item in SusidiaryMovedList)
        //                    {
        //                        //var obj = allstaffincompany.SingleOrDefault(x => x.Id == item.Key);
        //                        response.AppendLine($"Subsidiary changed from {item.Key} to {item.Value}");


        //                    }
        //                    response.AppendLine(Environment.NewLine);
        //                    response.AppendLine("--EXPUNGE LIST CHECK AGAINST NEW LIST--");

        //                    foreach (var item in similarList)
        //                    {
        //                        response.AppendLine(string.Format("{0} and {1} --are similar.", item.Key, item.Value));
        //                    }


        //                    response.AppendLine(Environment.NewLine);
        //                    response.AppendLine("--EXPUNGE LIST DEPENDANTS--");

        //                    foreach (var item in expungelistDependants)
        //                    {
        //                        response.AppendLine($"Id:{item.Key},name:{item.Value};");
        //                    }

        //                    response.AppendLine("--DUPLICATE IN NEWLIST--");

        //                    foreach (var item in duplicateslist)
        //                    {
        //                        response.AppendLine(item);
        //                    }


        //                    response.AppendLine("--ALL IN OLD LIST--");

        //                    foreach (var item in allstaffincompany)
        //                    {
        //                        response.AppendLine(item.Name.ToUpper());
        //                    }

        //                    response.AppendLine("--ALL IN NEW LIST--");

        //                    foreach (var item in Newlist)
        //                    {
        //                        response.AppendLine(string.Format("{0}:{1};", item.Name.ToUpper(), item.staffPlan));
        //                    }
        //                    response.AppendLine("--DONE--");
        //                    var linko = Path.Combine(fullpath, foldername + ".txt");
        //                    System.IO.File.WriteAllText(linko, response.ToString());
        //                    ////update result 
        //                    Stream fs = System.IO.File.OpenRead(linko);


        //                    if (true)
        //                    {
        //                        ViewDataUploadFilesResult dbFile = _fileService.AddFile(fs, foldername + ".txt",
        //                            "application/text, application/octet-stream", fs.Length,

        //                            mediaCategory);


        //                        jobo.Analysislink = CurrentRequestData.CurrentSite.BaseUrl + dbFile.url;



        //                    }
        //                    fs.Close();
        //                    System.IO.File.Delete(linko);



        //                    //update the staff in the subsidiary



        //                    jobo.JobStatus = JobStatus.Completed;
        //                    jobo.TotalRecord = Newlist.Count;
        //                    jobo.FinishTime = CurrentRequestData.Now;
        //                    jobo.TotalRecordDone = Newlist.Count;

        //                    jobo.TotalStaffForExpunged = 0;
        //                    _companySvc.UpdateStaffUploadJobs(jobo);

        //                }
        //            }
        //        }
        //        catch (Exception ex)

        //        {

        //            var logg = new Log()
        //            {
        //                Detail = string.Format("There was an error {0}", ex.Message),
        //                Message = ex.Message,
        //                Type = LogEntryType.Audit

        //            };
        //            _logger.Insert(logg);

        //        }

        //    }

        //    return "Done";
        //}

        [ActionName("ShowSub")]
        public ActionResult ShowSub(CompanySubPage page, int? id)
        {
            //Get the companies
            page.ParentcompanyName = _companySvc.GetCompany(Convert.ToInt32(id)).Name;
            page.Parentcompanyid = Convert.ToInt32(id);



            return View(page);
        }
        //This is the provider list json
        public JsonResult GetJson()
        {

            IEnumerable<Company> reply = _companySvc.GetallCompanyforJson().Where(x => x.AuthorizationStatus == 0 || x.AuthorizationStatus == 2);  //get only those approved. or does not require approval
            var replyedited = from areply in reply
                              select new
                              {
                                  Id = areply.Id,
                                  Name = areply.Name,
                                  Code = areply.Code,
                                  Address = !string.IsNullOrEmpty(areply.Address) ? areply.Address : "Nil",
                                  State = _helperSvc.GetState(Convert.ToInt32(areply.Stateid)).Name,
                                  Email = !string.IsNullOrEmpty(areply.Email) ? areply.Email : "Nil",
                                  Website = !string.IsNullOrEmpty(areply.Website) ? areply.Website : "Nil",
                                  PhoneNumber = areply.PhoneNumber,
                                  SubscriptionStatus = _companySvc.checkifCompanyHasSubscription(areply.Id),
                                  CreatedOn = Convert.ToDateTime(areply.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern),
                                  subsidiary = _companySvc.GetAllSubsidiary().Count(x => x.ParentcompanyId == areply.Id)
                              };
            JsonResult response = Json(replyedited, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                aaData = response.Data
            });


        }
        public JsonResult GetJsonPending()
        {
            Log log = new Log()
            {

            };



            IEnumerable<ProviderVm> reply = _providerSvc.GetallProviderforJson().Where(x => x.AuthorizationStatus == 1);  //get only those approved. or does not require approval
            JsonResult response = Json(reply, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                aaData = response.Data
            });


        }
        public JsonResult GetJsonSubsidiary(int? id)
        {
            IEnumerable<CompanySubsidiary> subsidiary = _companySvc.GetAllSubsidiary().Where(x => x.ParentcompanyId == Convert.ToInt32(id));





            JsonResult response = Json(subsidiary, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                aaData = response.Data
            });


        }

        [ActionName("BulkUploadStaff")]
        public ActionResult BulkUploadStaff(BulkUploadStaffPage page)
        {

            //bulkuploadPage
            return View(page);
        }

        [HttpPost]
        public ActionResult BulkUploadStaff(FormCollection form)
        {

            return null;
        }



        [ActionName("BulkStaffJob")]
        public ActionResult BulkStaffJob(BulkStaffJobPage page)
        {

            //bulkuploadPage
            return View(page);
        }
        [ActionName("ProviderList")]
        public ActionResult ProviderList(ProviderListPage page)
        {

            //Return to the providers list page
            return View(page);
        }
        [ActionName("PendingApprovalList")]
        public ActionResult PendingApprovalList(ProviderApprovalPage page)
        {

            //Return to the providers list page
            return View(page);
        }
        [HttpGet]
        public ActionResult Details(int id)
        {
            ProviderVm providervm = _providerSvc.GetProviderVm(id);


            return PartialView("Details", providervm);
        }
        [HttpGet]
        public ActionResult AddStaffBulk()
        {
            IList<Company> list = _companySvc.GetallCompany();
            string[] expungelist = Enum.GetNames(typeof(JobExpungeMode));
            var ExpungeListOutput = from item in expungelist
                                    select new
                                    {
                                        Id = (int)Enum.Parse(typeof(JobExpungeMode), item),
                                        Name = item
                                    };



            list.Add(new Company() { Id = -1, Name = "--Select--" });
            ViewBag.CompanyList = list.OrderBy(x => x.Id);

            ViewBag.PlanList = new[]
                {
                    new {Id = "-1", Name = "Select"}

                };
            ViewBag.SubsidiaryList = new[]

                {
                  new {Id = "-1", Name = "None"}

                };


            ViewBag.ExpungeModeList = ExpungeListOutput;

            return PartialView("AddBulkStaff");
        }
        [HttpPost]
        public ActionResult AddStaffBulk(FormCollection form)
        {
            //Do what you can from here 
            HttpPostedFileBase file = CurrentRequestData.CurrentContext.Request.Files.Count > 0 ? CurrentRequestData.CurrentContext.Request.Files[0] : null;
            string company = form["CompanyId"];
            string subsidiary = form["Subsidiary"];

            string expungeMode = form["expungeMode"];

            if (string.IsNullOrEmpty(company) || string.IsNullOrEmpty(subsidiary) || (!string.IsNullOrEmpty(subsidiary) && Convert.ToInt32(subsidiary) < 1))
            {
                _pageMessageSvc.SetErrormessage("Kindly fill the form properly ,you must select a company and subsidiary");

                return _uniquePageService.RedirectTo<BulkUploadStaffPage>();
            }
            //var errorlist = new Dictionary<string, string>();
            //var successlist = new Dictionary<string, string>();

            //var allstaffincompany = _companySvc.GetAllStaffinCompany(Convert.ToInt32(company));

            //foreach(var staff in allstaffincompany)
            //{
            //    //add to expungeList.
            //    var authomatic = new AutomaticExpungeStaff();
            //    authomatic.StaffId = staff.Id;
            //    authomatic.Companyid = Convert.ToInt32(company);
            //    authomatic.Subsidiary = Convert.ToInt32(subsidiary);
            //    authomatic.CreatedBy = CurrentRequestData.CurrentUser.Id;
            //    _companySvc.AddAutomaticDeletion(authomatic);

            //}
            if (file != null)
            {
                string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath, path2: "App_Data");
                string foldername = "Bulk_Upload"; //Guid.NewGuid().ToString();

                string filename = Guid.NewGuid().ToString() + ".xlsx";
                string fullpath = Path.Combine(appdatafolder, foldername);
                Directory.CreateDirectory(fullpath);

                string filefullname = Path.Combine(fullpath, filename);
                byte[] FileContent = Tools.ReadToEnd(file.InputStream);
                //Write to File in the Database

                System.IO.File.WriteAllBytes(filefullname, FileContent);

                //when done writing file to system
                StaffUploadJob job = new StaffUploadJob();
                job.filelink = filefullname;
                job.UploadedBy = CurrentRequestData.CurrentUser.Id;
                job.JobStatus = JobStatus.Uploaded;
                job.CompanyID = Convert.ToInt32(company);
                job.Subsidiary = Convert.ToInt32(subsidiary);
                job.ExpungeMode = (JobExpungeMode)Enum.Parse(typeof(JobExpungeMode), expungeMode);

                _companySvc.AddStaffJob(job);


                _pageMessageSvc.SetSuccessMessage(
                    "Staff List Have been Uploaded Successfully kindly check file progress in job queue.");


                //push email to the upload and Temi
                if (true)
                {//_helperSvc.PushUserNotification 
                    QueuedMessage emailmsg = new QueuedMessage();
                    emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                    emailmsg.ToAddress = CurrentRequestData.CurrentUser.Email;
                    emailmsg.Subject = "Renewal Upload Successful";
                    emailmsg.FromName = "NovoHub Notification";
                    emailmsg.Body = "New Renewal List uploaded successfully,Awaiting approval from Head Data Unit.";
                    emailmsg.IsHtml = true;
                    _emailSender.AddToQueue(emailmsg);


                }
                if (true)
                {//_helperSvc.PushUserNotification 
                    QueuedMessage emailmsg = new QueuedMessage();
                    emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                    emailmsg.ToAddress = "temidayok@novohealthafrica.org";
                    emailmsg.Subject = "Renewal Approval";
                    emailmsg.FromName = "NovoHub Notification";
                    emailmsg.Body = "New Renewal List uploaded successfully,Awaiting your approval.";
                    emailmsg.IsHtml = true;
                    _emailSender.AddToQueue(emailmsg);


                }
                // var context = CurrentRequestData.CurrentContext;
                ////execute the task for jobs in a different thread.
                //new Thread(() =>
                //{
                //   HttpContext.c= context;
                //    ExecuteBulkStaffTask();
                //}).Start();
                return _uniquePageService.RedirectTo<BulkStaffJobPage>();
                //redirect to the job page

            }



            return _uniquePageService.RedirectTo<BulkUploadStaffPage>();
        }

        [HttpGet]
        public ActionResult Add()
        {
            //Get the states
            List<State> states = new List<State>();
            IEnumerable<State> statess = _helperSvc.GetallStates();
            states.Add(new State() { Id = -1, Name = "--SELECT--" });
            foreach (State item in statess)
            {

                states.Add(item);
            }


            ViewBag.Statelist = states;


            return PartialView("Add");
        }
        //Action for Add View
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Company company, FormCollection form)
        {

            //Get the form items.

            string stateid = form["companystate"];
            company.Stateid = Convert.ToInt32(stateid);

            //so far company does not go for authorization
            company.AuthorizationStatus = (int)AuthorizationStatus.Default;
            company.SubscriptionStatus = (int)SubscriptionStatus.Default;
            company.CreatedBy = CurrentRequestData.CurrentUser.Guid.ToString();

            //create a subsidiary for the company
            CompanySubsidiary companysub = new CompanySubsidiary()
            {
                ParentcompanyId = company.Id,
                Subsidaryname = company.Name.ToUpper(),
                Subsidaryprofile = company.Description,
                CreatedBy = CurrentRequestData.CurrentUser.Id
            };

            company.Subsidiary = companysub;
            if (string.IsNullOrEmpty(company.Code))
            {
                company.Code = _helperSvc.GenerateCompanysubCode();
            }
            bool response = _companySvc.AddnewCompany(company);

            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Company [{0}] was added successfully.", company.Name.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  adding company [{0}] ",
                                                             company.Name.ToUpper()));
            }

            return _uniquePageService.RedirectTo<CompanyListPage>();



        }

        //Action for Edit View
        [HttpGet]
        public ActionResult Edit(int id)
        {


            Company company = _companySvc.GetCompany(id);
            ViewBag.Portal = CurrentRequestData.CurrentSite.BaseUrl + "/Portal/" + company.Id.ToString();
            //Get the states
            List<State> states = new List<State>();
            IEnumerable<State> statess = _helperSvc.GetallStates();
            states.Add(new State() { Id = -1, Name = "--SELECT--" });
            foreach (State item in statess)
            {

                states.Add(item);
            }


            ViewBag.Statelist = states;

            return PartialView("Edit", company);
        }

        [HttpPost]
        public ActionResult Edit(Company company, FormCollection form)
        {

            //Get the form items.
            Company compmodel = _companySvc.GetCompany(company.Id);
            compmodel.Name = company.Name;
            compmodel.Code = company.Code;
            compmodel.Stateid = Convert.ToInt32(form["companystate"]);
            compmodel.Address = company.Address;
            compmodel.Website = company.Website;
            compmodel.Email = company.Email;
            compmodel.PhoneNumber = company.PhoneNumber;
            compmodel.Description = company.Description;


            //update the user





            bool response = _companySvc.UpdateCompany(compmodel);

            if (response)
            {
                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Company [{0}] was updated successfully.", company.Name.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  updating company [{0}] ",
                                                              company.Name.ToUpper()));
            }

            return _uniquePageService.RedirectTo<CompanyListPage>();
        }

        //Action for Delete View

        [HttpGet]
        public ActionResult Delete(int id)
        {
            Company item = _companySvc.GetCompany(id);
            return PartialView("Delete", item);
        }
        public JsonResult GetLga(int? id)
        {
            IEnumerable<Lga> items = _helperSvc.GetLgainstate(Convert.ToInt32(id));
            return Json(items, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetCompanyRegBranchList(int id, int stateID)
        {
            Company company = _companySvc.GetCompany(id);
            List<GenericReponse> outlist = new List<GenericReponse>();
            if (company != null)
            {

                string branchh = company.CompanyBranch.Where(x => x.Statecode == stateID).SingleOrDefault() != null ? company.CompanyBranch.Where(x => x.Statecode == stateID).SingleOrDefault().Branch : ",";

                string[] items = branchh.Split(',');
                int count = 1;
                foreach (string item in items)
                {
                    outlist.Add(new GenericReponse { Id = count.ToString(), Name = item.Trim() });
                    count++;

                }

            }

            return Json(outlist, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCompanyRegBranch(int id, int stateID)
        {
            Company company = _companySvc.GetCompany(id);

            if (company != null)
            {

                string branchh = company.CompanyBranch.Where(x => x.Statecode == stateID).SingleOrDefault() != null ? company.CompanyBranch.Where(x => x.Statecode == stateID).SingleOrDefault().Branch : "";
                return Json(branchh, JsonRequestBehavior.AllowGet);
            }

            return Json("-1", JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveCompanyRegBranch(int id, int stateID, string branch, int ageLimit)
        {
            Company company = _companySvc.GetCompany(id);

            if (company != null)
            {
                CompanyBranch branchobj = new CompanyBranch();
                branchobj.Statecode = stateID;
                branchobj.company = company;
                branchobj.Branch = branch;


                if (stateID > 0)
                {
                    if (company.CompanyBranch.Any(x => x.Statecode == stateID))
                    {
                        CompanyBranch obj = company.CompanyBranch.SingleOrDefault(x => x.Statecode == stateID);

                        company.CompanyBranch[company.CompanyBranch.IndexOf(obj)] = branchobj;


                    }
                    else
                    {
                        company.CompanyBranch.Add(branchobj);
                    }
                }


                if (ageLimit > 0)
                {
                    company.RegAgeLimit = ageLimit;
                }
                _companySvc.UpdateCompany(company);
                return Json("0", JsonRequestBehavior.AllowGet);
            }

            return Json("-1", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Company company)
        {
            if (company.Id > 0)
            {
                Company comp = _companySvc.GetCompany(company.Id);

                bool response = _companySvc.DeleteCompany(comp);
                if (response)
                {
                    _pageMessageSvc.SetSuccessMessage(string.Format("Company [{0}] was deleted successfully.", comp.Name.ToUpper()));
                }
                else
                {
                    _pageMessageSvc.SetErrormessage(string.Format("There was an error deleting company [{0}] ",
                                                                 comp.Name.ToUpper()));
                }
            }
            return _uniquePageService.RedirectTo<CompanyListPage>();
        }




        //Action for Edit View
        [HttpGet]
        public ActionResult Approve(int id)
        {
            ProviderVm item = _providerSvc.GetProviderVm(id);
            return PartialView("Approve", item);

        }

        [HttpPost]
        public ActionResult Approve(ProviderVm provider, FormCollection form)
        {

            Provider providerM = _providerSvc.GetProvider(provider.Id);
            providerM.AuthorizationNote = provider.AuthorizationNote;
            providerM.Status = true;
            providerM.AuthorizationStatus = (int)AuthorizationStatus.Authorized;
            providerM.AuthorizedDate = CurrentRequestData.Now;
            providerM.AuthorizedBy = CurrentRequestData.CurrentUser.Id;

            //update the stuff 
            bool resp = _providerSvc.UpdateProvider(providerM);
            if (resp)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Provider [{0}] was approved successfully.", providerM.Name.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  approving provider [{0}] ",
                                                              providerM.Name.ToUpper()));
            }

            return _uniquePageService.RedirectTo<ProviderApprovalPage>();
        }
        //Action for Edit View
        [HttpGet]
        public ActionResult Disapprove(int id)
        {
            ProviderVm item = _providerSvc.GetProviderVm(id);
            return PartialView("Disapprove", item);

        }

        [HttpPost]
        public ActionResult Disapprove(ProviderVm provider, FormCollection form)
        {

            Provider providerM = _providerSvc.GetProvider(provider.Id);
            providerM.DisapprovalNote = provider.DisapprovalNote;
            providerM.Status = false;
            providerM.AuthorizationStatus = (int)AuthorizationStatus.Disapproved;
            providerM.DisapprovalDate = CurrentRequestData.Now;
            providerM.DisapprovedBy = CurrentRequestData.CurrentUser.Id;

            //update the stuff 
            bool resp = _providerSvc.UpdateProvider(providerM);
            if (resp)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Provider [{0}] was disapproved successfully.", providerM.Name.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  disapproving provider [{0}] ",
                                                              providerM.Name.ToUpper()));
            }

            return _uniquePageService.RedirectTo<ProviderApprovalPage>();
        }


        //Benefit Part
        [HttpGet]
        public ActionResult AddCategory(int? id)
        {

            int intid = Convert.ToInt32(id);

            return PartialView("AddCategory");
        }


        public JsonResult AddCategory(BenefitsCategory category, FormCollection form)
        {
            if (category != null)
            {
                _companySvc.AddnewCategory(category);
            }


            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }
        [ActionName("Benefit")]
        public ActionResult EditContent(BenefitPage page, int? id)
        {

            return View(page);
        }


        public JsonResult GetCategoryJson(int? id)
        {

            IList<BenefitsCategory> reply = _companySvc.GetallBenefitCategory();
            JsonResult response = Json(reply, JsonRequestBehavior.AllowGet);

            return response;


        }

        [HttpGet]
        public ActionResult AddBenefit()
        {
            List<BenefitsCategory> lst = _companySvc.GetallBenefitCategory().ToList();
            ViewBag.BenefitGroup = lst;
            return PartialView("AddBenefit");
        }


        public JsonResult AddBenefit(Benefit benefit, FormCollection form)
        {
            //Redirect to the creation page
            //get id from session

            if (benefit != null)
            {
                string name = _companySvc.GetCategory(benefit.Benefitcategory).Name;
                benefit.CategoryName = name.ToUpper();

                _companySvc.AddnewBenefit(benefit);
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBenefitJson()
        {

            IList<BenefitsCategory> categoryinlist = _companySvc.GetallBenefitCategory();
            List<Benefit> output = new List<Benefit>();
            foreach (BenefitsCategory item in categoryinlist)
            {
                BenefitsCategory item1 = item;
                IEnumerable<Benefit> cont = _companySvc.Getallbenefit().Where(x => x.Benefitcategory == item1.Id);
                if (cont.Any())
                {
                    output.AddRange(cont.ToList());
                }
            }


            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }
        public ActionResult EditBenefit(int id)
        {

            Benefit benefit = _companySvc.GetBenefit(id);
            IList<BenefitsCategory> lst = _companySvc.GetallBenefitCategory();
            ViewBag.benefitGroup = lst;

            return PartialView("EditBenefit", benefit);
        }
        [HttpPost]
        public JsonResult EditBenefit(Benefit benefit, FormCollection form)
        {
            if (benefit != null)
            {
                string name = _companySvc.GetCategory(benefit.Benefitcategory).Name;
                benefit.CategoryName = name.ToUpper();
                _companySvc.UpdateBenefit(benefit);
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteBenefit(int id)
        {
            Benefit item = _companySvc.GetBenefit(id);
            return PartialView("DeleteBenefit", item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBenefit(Benefit benefit)
        {
            if (benefit.Id > 0)
            {


                bool response = _companySvc.DeleteBenefit(benefit);


            }
            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            BenefitsCategory item = _companySvc.GetCategory(id);
            return PartialView("DeleteCategory", item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategory(BenefitsCategory category)
        {
            if (category.Id > 0)
            {


                bool response = _companySvc.DeleteCategory(category);


            }
            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        [ActionName("CompanyPlan")]
        public ActionResult Show(CompanyPlanPage page)
        {
            List<Provider> providerlist = _providerSvc.GetallProvider().OrderBy(x => x.Name).ToList();
            providerlist.Insert(0, new Provider() { Id = -1, Name = "All Providers" });

            ViewBag.providerlist = providerlist;

            List<Company> companylist = _companySvc.GetallCompany().OrderBy(x => x.Name).ToList();
            companylist.Insert(0, new Company() { Id = -1, Name = "All Companies" });
            ViewBag.Companylist = companylist;

            List<User> userlist = _userservice.GetAllUsers().Where(x => x.IsAdmin == false).OrderBy(x => x.Name).ToList();
            userlist.Insert(0, new User() { Id = -1, FirstName = "Select All", LastName = "" });
            ViewBag.userlist = userlist;

            return View(page);
        }

        [HttpGet]
        public ActionResult AddCompanyPlan()
        {
            //Add all companies
            //Add all plans

            IList<Company> companylist = _companySvc.GetallCompany();
            ViewBag.Companylistbag = companylist;

            List<Plan> planlist = _planService.GetallPlans().Where(x => x.Status == true).ToList();
            List<Plan> planlistt = new List<Plan>();

            planlistt.Add(new Plan { Id = -1, Name = "--Select Plan--" });
            planlistt.AddRange(planlist);


            ViewBag.PlanTypeList = planlistt;

            return PartialView("AddCompanyPlan");
        }


        public JsonResult AddCompanyPlan(CompanyPlan companyPlan, FormCollection form)
        {
            //Redirect to the creation page
            //get id from session

            if (companyPlan != null)
            {
                string companyList = form["companyList"];
                string planType = form["PlanType"];
                string discountEnrollee = form["discountEnrollee"];
                string annualPremium = form["AnnualPremium"];
                string discountLump = form["discountLump"];

                companyPlan.Companyid = Convert.ToInt32(companyList);
                companyPlan.Planid = Convert.ToInt32(planType);
                companyPlan.AnnualPremium = Convert.ToDecimal(annualPremium);
                companyPlan.Discountperenrollee = Convert.ToDecimal(discountEnrollee);
                companyPlan.Discountlump = Convert.ToDecimal(discountLump);

                companyPlan.Createdby = CurrentRequestData.CurrentUser.Id;


                if (companyPlan.Planid > -1)
                {
                    _companySvc.AddCompanyPlan(companyPlan);
                }
                else
                {
                    return Json("{result:0}", JsonRequestBehavior.AllowGet);
                }



                //Add all the default benefits
                IList<PlanDefaultBenefit> allbenefits = _companySvc.GetPlanBenefits(companyPlan.Planid);

                foreach (PlanDefaultBenefit benefit in allbenefits)
                {

                    CompanyBenefit companyplanbenefit = new CompanyBenefit()
                    {
                        BenefitId = benefit.BenefitId,
                        BenefitLimit = benefit.BenefitLimit,
                        Companyid = companyPlan.Companyid,
                        CompanyPlanid = companyPlan.Id,

                    };

                    _companySvc.AddCompanyPlanBenefit(companyplanbenefit);

                }


            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }
        //create on of each company plan for various companies
        //Remove @ real time
        public string RunCompanyplanGeneration()
        {
            IList<Plan> allplantypes = _planService.GetallPlans();
            IList<Company> companies = _companySvc.GetallCompanyWithOutAPlan();

            foreach (Company company in companies)
            {
                foreach (Plan plantype in allplantypes)
                {
                    //for iindividual

                    CompanyPlan companyplan = new CompanyPlan();
                    companyplan.Companyid = company.Id;
                    companyplan.Planid = plantype.Id;
                    companyplan.Planfriendlyname = string.Format("{0} - {1} ", company.Name, plantype.Name);
                    companyplan.Description = companyplan.Planfriendlyname;
                    companyplan.Status = true;
                    companyplan.AllowChildEnrollee = false;
                    companyplan.AnnualPremium = 0;

                    FormCollection form = new FormCollection();

                    form.Add("companyList", Convert.ToString(companyplan.Companyid));
                    form.Add("PlanType", Convert.ToString(companyplan.Planid));
                    form.Add("discountEnrollee", Convert.ToString(0));
                    form.Add("AnnualPremium", Convert.ToString(0));
                    form.Add("discountLump", Convert.ToString(0));

                    JsonResult resp = AddCompanyPlan(companyplan, form);

                    CompanyPlan companyplan2 = new CompanyPlan();
                    companyplan2.Companyid = company.Id;
                    companyplan2.Planid = plantype.Id;
                    companyplan2.Planfriendlyname = string.Format("{0} - {1}  Family", company.Name, plantype.Name);
                    companyplan2.Description = companyplan.Planfriendlyname;
                    companyplan2.Status = true;
                    companyplan2.AllowChildEnrollee = true;
                    companyplan2.AnnualPremium = 0;



                    JsonResult resp2 = AddCompanyPlan(companyplan2, form);



                    //for family
                }
            }

            return "done!";
        }

        [HttpPost]
        public ActionResult DoMassExpunge(FormCollection form)
        {
            string ids = form["hidden_selectedIDs"];

            string[] staffs = ids.Split(',');
            if (staffs.Any())
            {
                foreach (string item in staffs)
                {
                    Staff itemstaff = _companySvc.Getstaff(Convert.ToInt32(item));
                    if (itemstaff != null)
                    {
                        try
                        {
                            FormCollection formx = new FormCollection();
                            formx.Add("ExpungeNote", "This Enrollee was auto-deleted ");

                            ExpungeStaff(itemstaff, formx);
                            _companySvc.DeleteAutomaticExpungeStaff(Convert.ToInt32(item));
                        }
                        catch (Exception ex)
                        {

                            _pageMessageSvc.SetErrormessage(string.Format("There was an error deleting staff {0}", item));
                        }
                    }
                }


            }


            return _uniquePageService.RedirectTo<BulkUploadStaffPage>();

        }

        [HttpPost]
        public ActionResult DoRemoveExpunge(FormCollection form)
        {
            string ids = form["hidden_selectedIDs2"];

            string[] staffs = ids.Split(',');
            if (staffs.Any())
            {
                foreach (string item in staffs)
                {
                    AutomaticExpungeStaff itemstaff = _companySvc.AutomaticExpungeStaff(Convert.ToInt32(item));
                    if (itemstaff != null)
                    {
                        try
                        {
                            _companySvc.DeleteAutomaticExpungeStaff(Convert.ToInt32(item));
                        }
                        catch (Exception ex)
                        {

                            _pageMessageSvc.SetErrormessage(string.Format("There was an error deleting staff {0}", item));
                        }
                    }
                }


            }


            return _uniquePageService.RedirectTo<BulkUploadStaffPage>();

        }
        public JsonResult GetCompanyPlanJson()
        {

            IList<CompanyPlan> companylist = _companySvc.Getallplan();

            var responses = from areply in companylist
                            select new
                            {
                                Id = areply.Id,
                                groupname = _companySvc.GetCompany(areply.Companyid).Name.ToUpper(),
                                plantype = _planService.GetPlan(areply.Planid).Name.ToUpper(),
                                name = areply.Planfriendlyname.ToUpper(),
                                description = areply.Description,
                                datecreated = areply.CreatedOn.ToString("ddd MMM yyyy"),
                                subsidiary = _companySvc.GetAllSubsidiary().Count(x => x.ParentcompanyId == areply.Companyid)

                            };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }
        public JsonResult QueryCompanyPlanJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);

            string scrPlanName = CurrentRequestData.CurrentContext.Request["src_PlanName"];
            string scrPlanDesc = CurrentRequestData.CurrentContext.Request["src_PlanDesc"];
            string scrCompany = CurrentRequestData.CurrentContext.Request["scr_company"];
            string scrPlanType = CurrentRequestData.CurrentContext.Request["scr_plantype"];
            string scrUser = CurrentRequestData.CurrentContext.Request["scr_users"];

            string scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            string scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            string scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];
            string search = "";
            int toltareccount = 0;
            int totalinresult = 0;

            int showexpunge = 0;
            DateTime fromdate = CurrentRequestData.Now;
            DateTime todate = CurrentRequestData.Now;
            bool usedate = false;
            if (!string.IsNullOrEmpty(scrFromDate) && !string.IsNullOrEmpty(scrToDate))
            {
                fromdate = Convert.ToDateTime(scrFromDate);
                todate = Convert.ToDateTime(scrToDate);
                usedate = Convert.ToBoolean(scruseDate);
            }

            IList<CompanyPlan> companylist = _companySvc.Queryallplan(out toltareccount, out totalinresult, search,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, scrPlanName, scrPlanDesc, scrCompany, usedate, fromdate, todate, scrUser, Convert.ToInt32(scrPlanType));


            List<CompanyPlanRespomse> responses = new List<CompanyPlanRespomse>();
            foreach (CompanyPlan areply in companylist)
            {
                CompanyPlanRespomse itemo = new CompanyPlanRespomse();
                Company compny = _companySvc.GetCompany(areply.Companyid);
                PlanVm plan = _planService.GetPlan(areply.Planid);
                itemo.Id = areply.Id.ToString();
                itemo.groupname = compny != null ? compny.Name.ToUpper() : "--";
                itemo.plantype = plan != null ? plan.Name.ToUpper() : "--";
                itemo.name = areply.Planfriendlyname ?? "--";
                itemo.description = areply.Description;
                itemo.datecreated =
                    Convert.ToDateTime(areply.CreatedOn)
                        .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                itemo.subsidiary = _companySvc.GetAllSubsidiary().Count(x => x.ParentcompanyId == areply.Companyid).ToString();

                responses.Add(itemo);

            }

            //var responses = from areply in companylist
            //                select new
            //                {
            //                    Id = areply.Id,
            //                    groupname = _companySvc.GetCompany(areply.Companyid).Name.ToUpper(),
            //                    plantype = _planService.GetPlan(areply.Planid).Name.ToUpper(),
            //                    name = areply.Planfriendlyname.ToUpper(),
            //                    description = areply.Description,
            //                    datecreated = Convert.ToDateTime(areply.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern),
            //                    subsidiary = _companySvc.GetAllSubsidiary().Count(x => x.ParentcompanyId == areply.Companyid)

            //                };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                aaData = response.Data
            });



        }

        [ActionName("CustomizePlan")]
        public ActionResult CustomizePlan(CustomizePlanPage page, int? id)
        {



            if (id > 0)
            {
                CompanyPlan companyplan = _companySvc.GetCompanyPlan(Convert.ToInt32(id));
                page.CustomizePlanName = companyplan.Planfriendlyname.ToUpper();
                page.Companyplanid = companyplan.Id.ToString();
                page.CompanyName = _companySvc.GetCompany(companyplan.Companyid).Name.ToUpper();
                page.PlanType = _planService.GetPlan(companyplan.Planid).Name.ToUpper();
                page.Name = companyplan.Planfriendlyname;
                page.Description = companyplan.Description;
                page.AnnualPremium = "₦ " + companyplan.AnnualPremium.ToString("N");
                page.Discountperenrollee = "₦ " + companyplan.Discountperenrollee.ToString("N");
                page.Discountlump = "₦ " + companyplan.Discountlump.ToString("N");
                page.AllowChildEnrollee = companyplan.AllowChildEnrollee;
                page.Createdby = _userservice.GetUser(Convert.ToInt32(companyplan.Createdby)).Name;


            }
            return View(page);
        }


        [ActionName("CustomizeDefaultPlan")]
        public ActionResult CustomizeDefaultPlan(CustomizeDefaultPlanPage page, int? id)
        {


            PlanVm companyplan = _planService.GetPlan(Convert.ToInt32(id));
            if (companyplan != null)
            {

                page.PlanId = companyplan.Id.ToString();
                page.PlanName = companyplan.Name.ToUpper();
                page.Description = companyplan.Description;
            }
            return View(page);
        }
        [ActionName("StaffList")]
        public ActionResult StaffList(StaffListPage page, int? id)
        {
            List<Company> companylist = _companySvc.GetallCompany().OrderBy(x => x.Name).ToList();
            companylist.Insert(0, new Company() { Id = -1, Name = "All Companies" });
            ViewBag.Companylist = companylist;

            List<User> userlist = _userservice.GetAllUsers().Where(x => x.IsAdmin == false).OrderBy(x => x.Name).ToList();
            userlist.Insert(0, new User() { Id = -1, FirstName = "Select All", LastName = "" });
            ViewBag.userlist = userlist;

            List<Company> sublist = new List<Company>();
            sublist.Insert(0, new Company() { Id = -1, Name = "All Subsidiary" });
            ViewBag.sublist = sublist;

            return View(page);
        }
        [ActionName("EnrolleeStaffList")]
        public ActionResult EnrolleeStaffList(EnrolleeStaffListPage page, int? id)
        {
            //var providerlist = _providerSvc.GetallProvider().OrderBy(x => x.Name).ToList();
            //providerlist.Insert(0, new Provider() { Id = -1, Name = "All Providers" });

            //ViewBag.providerlist = providerlist;

            List<Company> companylist = _companySvc.GetallCompany().OrderBy(x => x.Name).ToList();
            companylist.Insert(0, new Company() { Id = -1, Name = "All Companies" });
            ViewBag.Companylist = companylist;

            List<User> userlist = _userservice.GetAllUsers().Where(x => x.IsAdmin == false).OrderBy(x => x.Name).ToList();
            userlist.Insert(0, new User() { Id = -1, FirstName = "Select All", LastName = "" });
            ViewBag.userlist = userlist;

            return View(page);
        }
        public JsonResult GetStaffListJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);


            string scrStaffName = CurrentRequestData.CurrentContext.Request["src_StaffName"];
            string scrCompany = CurrentRequestData.CurrentContext.Request["scr_company"];
            string scrCompanySub = CurrentRequestData.CurrentContext.Request["scr_companysub"];
            string scrPlanType = CurrentRequestData.CurrentContext.Request["scr_plantype"];
            string scrUser = CurrentRequestData.CurrentContext.Request["scr_users"];
            string scr_ProfileStatus = CurrentRequestData.CurrentContext.Request["scr_ProfileStatus"];

            string expunged = CurrentRequestData.CurrentContext.Request["scr_expungetype"];
            string scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            string scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            string scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];
            string search = "";
            int toltareccount = 0;
            int totalinresult = 0;

            int showexpunge = 0;
            DateTime fromdate = CurrentRequestData.Now;
            DateTime todate = CurrentRequestData.Now;
            bool usedate = false;
            if (!string.IsNullOrEmpty(scrFromDate) && !string.IsNullOrEmpty(scrToDate))
            {
                fromdate = Convert.ToDateTime(scrFromDate);
                todate = Convert.ToDateTime(scrToDate);
                usedate = Convert.ToBoolean(scruseDate);
            }



            IList<Staff> staffList = _companySvc.QueryAllStaff(out toltareccount, out totalinresult, search,
                                                      Convert.ToInt32(displayStart),
                                                      Convert.ToInt32(displayLength), sortColumnnumber, sortOrder,
                                                    scrStaffName, Convert.ToInt32(scrCompany), Convert.ToInt32(scrCompanySub), -1, Convert.ToInt32(scrUser), usedate, fromdate, todate, Convert.ToInt32(scr_ProfileStatus), Convert.ToInt32(expunged));

            var responses = from areply in staffList
                            let theuser = _userservice.GetUser(areply.Createdby)
                            let companysub = _companySvc.Getsubsidiary(areply.CompanySubsidiary)
                            let companyplan = _companySvc.GetCompanyPlan(areply.StaffPlanid)
                            select new
                            {
                                Id = areply.Id,
                                groupname = _companySvc.GetCompany(Convert.ToInt32(areply.CompanyId)).Name.ToUpper(),
                                name = areply.StaffFullname,
                                staffid = !string.IsNullOrEmpty(areply.StaffId) ? areply.StaffId : "--",
                                created = Convert.ToDateTime(areply.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern),
                                creator = areply.Createdby > 0 && theuser != null ? theuser.Name.ToUpper() : "unknown",
                                subsidiary = companysub != null ? companysub.Subsidaryname.ToUpper() : "Unknown",
                                companyid = areply.CompanyId,
                                Plan = companyplan != null ? companyplan.Planfriendlyname : "Plan Deleted",
                                hasprofile = areply.HasProfile ? "yes" : "no",
                                hasbeenExpunged = areply.IsExpunged,
                                allowdependant = companyplan != null ? companyplan.AllowChildEnrollee : false

                            };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                aaData = response.Data
            });



        }

        public JsonResult GetNewStaffListJson()
        {
            //get only staff without profile
            IEnumerable<Staff> staffList = _companySvc.GetAllStaff().Where(x => x.HasProfile == false && x.IsExpunged == false);

            var responses = from areply in staffList
                            select new
                            {
                                Id = areply.Id,
                                groupname = _companySvc.GetCompany(Convert.ToInt32(areply.CompanyId)).Name.ToUpper(),
                                name = areply.StaffFullname,
                                Plan = _companySvc.GetCompanyPlan(areply.StaffPlanid).Planfriendlyname,
                                hasprofile = areply.HasProfile ? "yes" : "no",

                            };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }
        [HttpGet]
        public ActionResult AddStaff()
        {
            IList<Company> list = _companySvc.GetallCompany();

            list.Add(new Company() { Id = -1, Name = "--Select--" });
            ViewBag.CompanyList = list.OrderBy(x => x.Id);

            ViewBag.PlanList = new[]
                {
                    new {Id = "-1", Name = "Select"}

                };
            ViewBag.SubsidiaryList = new[]

                {
                  new {Id = "-1", Name = "None"}

                };
            return PartialView("AddStaff");
        }
        [HttpPost]
        public JsonResult AddStaff(FormCollection form, Staff staff)
        {
            GenericReponse response = new GenericReponse();
            //check if staff exist
            int staffid = 0;
            List<string> outid = new List<string>();
            string over = form["overridenamecheck"];
            bool overridename = over == null || over.ToLower() != "on" ? false : true;

            bool exist = _companySvc.CheckifStaffExistwithName(staff.StaffFullname, Convert.ToInt32(staff.CompanyId), out staffid, Convert.ToInt32(staff.CompanySubsidiary), out outid);

            if (exist && overridename == false)
            {
                response.Id = "-1";
                response.Name = "The staff already exist on the system.";
                return Json(response, JsonRequestBehavior.AllowGet);

            }

            if (staff.StaffPlanid > 0 && (exist == false || overridename == true))
            {
                response.Id = "0";
                response.Name = "The staff was added succesfully.";
                staff.Createdby = CurrentRequestData.CurrentUser.Id;
                _companySvc.AddStaff(staff);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPlans(int? id)
        {
            IList<CompanyPlan> items = _companySvc.GetallplanForCompany(Convert.ToInt32(id));
            return Json(items, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetPlanswithoutsubscription(int? id)
        {


            IList<CompanyPlan> items = _companySvc.GetallplanForCompany(Convert.ToInt32(id));

            List<CompanyPlan> ritems = new List<CompanyPlan>();

            foreach (CompanyPlan item in items)
            {

                Subscription subscription = _companySvc.GetSubscriptionByPlan(item.Id);


                if (subscription == null || subscription.Status == (int)SubscriptionStatus.Expired)
                {
                    //no subscription yet

                    ritems.Add(item);

                }
            }

            return Json(ritems, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSubsidiary(int? id)
        {
            IEnumerable<CompanySubsidiary> items = _companySvc.GetAllSubsidiary().Where(x => x.ParentcompanyId == Convert.ToInt32(id));
            return Json(items, JsonRequestBehavior.AllowGet);
        }


        public ActionResult updateRenewal(int id)
        {
            Company company = _companySvc.GetCompany(id);

            if (company != null)
            {
                company.isRenewal = false;
                _companySvc.UpdateCompany(company);
            }

            _pageMessageSvc.SetSuccessMessage("Company renawal Status was updated successfully.");

            return _uniquePageService.RedirectTo<BulkStaffJobPage>();

        }
        public JsonResult GetCompanyBenefitforPlanJson(int id)
        {

            IList<CompanyBenefit> benefitlist = _companySvc.GetCompanyPlanBenefits(id);

            var responses = from areply in benefitlist
                            select new
                            {
                                Id = areply.Id,
                                groupname = _companySvc.GetBenefit(areply.BenefitId).CategoryName.ToUpper(),
                                name = _companySvc.GetBenefit(areply.BenefitId).Name,
                                description = _companySvc.GetBenefit(areply.BenefitId).Description,
                                benefitlimit = areply.BenefitLimit == null ? string.Empty : areply.BenefitLimit.ToUpper()

                            };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }

        //Select a list of 
        public JsonResult GetCompanyFreeBenefitforPlanJson(int id)
        {
            IList<BenefitsCategory> categoryinlist = _companySvc.GetallBenefitCategory();
            List<Benefit> output = new List<Benefit>();
            foreach (BenefitsCategory item in categoryinlist)
            {
                BenefitsCategory item1 = item;
                IEnumerable<Benefit> cont = _companySvc.Getallbenefit().Where(x => x.Benefitcategory == item1.Id);
                if (cont.Any())
                {
                    output.AddRange(cont.ToList());
                }
            }
            List<Benefit> allbenefits = output;

            IList<CompanyBenefit> benefitlist = _companySvc.GetCompanyPlanBenefits(id);

            List<Benefit> resplist = allbenefits.Where(benefit => benefitlist.All(x => x.BenefitId != benefit.Id)).ToList();


            var responses = from areply in resplist
                            select new
                            {
                                Id = areply.Id,
                                groupname = areply.CategoryName,
                                name = areply.Name,
                                description = areply.Description,
                                benefitlimit = areply.Benefitlimit

                            };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }



        public JsonResult GetBenefitforPlanJson(int id)
        {

            IList<PlanDefaultBenefit> benefitlist = _companySvc.GetPlanBenefits(id);

            var responses = from areply in benefitlist
                            select new
                            {
                                Id = areply.Id,
                                groupname = _companySvc.GetBenefit(areply.BenefitId).CategoryName.ToUpper(),
                                name = _companySvc.GetBenefit(areply.BenefitId).Name,
                                description = _companySvc.GetBenefit(areply.BenefitId).Description,
                                benefitlimit = areply.BenefitLimit == null ? string.Empty : areply.BenefitLimit.ToUpper()

                            };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }

        //forplan page
        public JsonResult GetFreeBenefitforPlanJson(int id)
        {
            IList<BenefitsCategory> categoryinlist = _companySvc.GetallBenefitCategory();
            List<Benefit> output = new List<Benefit>();
            foreach (BenefitsCategory item in categoryinlist)
            {
                BenefitsCategory item1 = item;
                IEnumerable<Benefit> cont = _companySvc.Getallbenefit().Where(x => x.Benefitcategory == item1.Id);
                if (cont.Any())
                {
                    output.AddRange(cont.ToList());
                }
            }
            List<Benefit> allbenefits = output;

            IList<PlanDefaultBenefit> benefitlist = _companySvc.GetPlanBenefits(id);

            List<Benefit> resplist = allbenefits.Where(benefit => benefitlist.All(x => x.BenefitId != benefit.Id)).ToList();


            var responses = from areply in resplist
                            select new
                            {
                                Id = areply.Id,
                                groupname = areply.CategoryName,
                                name = areply.Name,
                                description = areply.Description,
                                benefitlimit = areply.Benefitlimit

                            };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }
        [HttpGet]
        public ActionResult AddBenefitToPlan(int? id)
        {
            CompanyPlan companyplan = _companySvc.GetCompanyPlan(Convert.ToInt32(id));

            return PartialView("AddBenefitToPlan", companyplan);
        }

        [HttpPost]
        public JsonResult AddBenefitToPlan(FormCollection form)
        {
            //Redirect to the creation page
            //get id from session
            string planid = form["planid"];
            string benefitid = form["benefitid"];

            CompanyPlan plan = _companySvc.GetCompanyPlan(Convert.ToInt32(planid));
            Benefit benefit = _companySvc.GetBenefit(Convert.ToInt32(benefitid));

            CompanyBenefit newbenefit = new CompanyBenefit()
            {
                BenefitId = benefit.Id,
                BenefitLimit = benefit.Benefitlimit,
                CompanyPlanid = plan.Id,
                Companyid = plan.Companyid,


            };

            _companySvc.AddCompanyPlanBenefit(newbenefit);

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult RemoveBenefitToPlan(FormCollection form)
        {

            string benefitid = form["benefitid"];


            CompanyBenefit benefit = _companySvc.GetCompanyPlanBenefit(Convert.ToInt32(benefitid));
            _companySvc.DeleteCompanyPlanBenefit(benefit);

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditBenefitLimit(int id)
        {

            CompanyBenefit benefit = _companySvc.GetCompanyPlanBenefit(id);


            return PartialView("EditBenefitLimit", benefit);
        }
        [HttpPost]
        public JsonResult EditBenefitLimit(CompanyBenefit benefit, FormCollection form)
        {
            if (benefit != null)
            {

                _companySvc.UpdateCompanyPlanBenefit(benefit);
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult EditCompanyPlan(int id)
        {
            //Add all companies
            //Add all plans
            CompanyPlan company = _companySvc.GetCompanyPlan(id);
            IList<Company> companylist = _companySvc.GetallCompany();
            ViewBag.Companylistbag = companylist;

            IEnumerable<Plan> planlist = _planService.GetallPlans().Where(x => x.Status == true);
            ViewBag.PlanTypeList = planlist;

            return PartialView("EditCompanyPlan", company);
        }


        [HttpPost]
        public ActionResult EditCompanyPlan(CompanyPlan companyP, FormCollection form)
        {
            if (companyP != null)
            {
                CompanyPlan companyPlan = _companySvc.GetCompanyPlan(companyP.Id);

                string companyList = form["companyList"];
                string planType = form["PlanType"];
                string discountEnrollee = form["discountEnrollee"];
                string annualPremium = form["AnnualPremium"];
                string discountLump = form["discountLump"];

                companyPlan.Companyid = Convert.ToInt32(companyList);
                companyPlan.Planid = Convert.ToInt32(planType);
                companyPlan.AnnualPremium = Convert.ToDecimal(annualPremium);
                companyPlan.Discountperenrollee = Convert.ToDecimal(discountEnrollee);
                companyPlan.Discountlump = Convert.ToDecimal(discountLump);

                //companyPlan.Createdby = CurrentRequestData.CurrentUser.Id;


                _companySvc.UpdateCompanyPlan(companyPlan);


            }

            return _uniquePageService.RedirectTo<CompanyPlanPage>();
        }


        [HttpGet]
        public ActionResult DeleteCompanyPlan(int id)
        {
            CompanyPlan item = _companySvc.GetCompanyPlan(id);
            return PartialView("DeleteCompanyPlan", item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCompanyPlan(CompanyPlan companyplan)
        {
            if (companyplan.Id > 0)
            {


                bool response = _companySvc.DeleteCompanyPlan(companyplan);
                if (response)
                {
                    _pageMessageSvc.SetSuccessMessage(string.Format("Company Plan [{0}] was deleted successfully.", companyplan.Planfriendlyname.ToUpper()));
                }
                else
                {
                    _pageMessageSvc.SetErrormessage(string.Format("There was an error deleting company plan [{0}] ",
                                                                 companyplan.Planfriendlyname.ToUpper()));
                }

            }

            return _uniquePageService.RedirectTo<CompanyPlanPage>();
        }


        //Add Default benefit to plan

        [HttpPost]
        public JsonResult AddDefaultBenefitToPlan(FormCollection form)
        {
            //Redirect to the creation page
            //get id from session
            string planid = form["planid"];
            string benefitid = form["benefitid"];

            PlanVm plan = _planService.GetPlan(Convert.ToInt32(planid));
            Benefit benefit = _companySvc.GetBenefit(Convert.ToInt32(benefitid));

            PlanDefaultBenefit newbenefit = new PlanDefaultBenefit()
            {
                BenefitId = benefit.Id,
                BenefitLimit = benefit.Benefitlimit,
                Planid = plan.Id,
            };

            _companySvc.AddPlanBenefit(newbenefit);

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RemoveBenefitTfromPlan(FormCollection form)
        {

            string benefitid = form["benefitid"];


            PlanDefaultBenefit benefit = _companySvc.GetPlanBenefit(Convert.ToInt32(benefitid));
            _companySvc.DeletePlanBenefit(benefit);

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }


        public ActionResult EditStaff(int id)
        {
            IList<Company> list = _companySvc.GetallCompany();
            Staff staff = _companySvc.Getstaff(id);
            list.Add(new Company() { Id = -1, Name = "--Select--" });
            ViewBag.CompanyList = list.OrderBy(x => x.Id);

            ViewBag.PlanList = _companySvc.Getallplan().Where(x => x.Companyid == Convert.ToInt32(staff.CompanyId));
            ViewBag.SubsidiaryList = _companySvc.GetAllSubsidiary().Where(x => x.ParentcompanyId == Convert.ToInt32(staff.CompanyId));




            return PartialView("EditStaff", staff);
        }
        [HttpPost]
        public JsonResult EditStaff(Staff staff, FormCollection form)
        {
            if (staff != null)
            {
                //check if staff have enrollee profile
                IList<Enrollee> enrollees = _enrolleeService.GetEnrolleesByStaffId(staff.Id);


                _companySvc.UpdateStaff(staff);

                if (enrollees != null)
                {

                    foreach (Enrollee enrollee in enrollees)
                    {
                        enrollee.Companyid = Convert.ToInt32(staff.CompanyId);
                        enrollee.Subscriptionplanid = staff.StaffPlanid;
                        _enrolleeService.UpdateEnrollee(enrollee);

                    }

                }
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        public ActionResult RestoreStaff(int id)
        {

            Staff staff = _companySvc.Getstaff(id);

            if (staff != null)
            {
                CompanyPlan staffplan = _companySvc.GetCompanyPlan(staff.StaffPlanid);
                Enrollee enrollee = _enrolleeService.GetEnrollee(staff.Profileid);
                ViewBag.StaffPlan = staffplan.Planfriendlyname ?? staffplan.Planfriendlyname;
                ViewBag.Dependents = _enrolleeService.GetDependentsEnrollee(enrollee.Id).Count;
                ViewBag.DateExpunges = enrollee.Dateexpunged != null ? Tools.ConvertToLongDate(Convert.ToDateTime(enrollee.Dateexpunged)) : "--";
                ViewBag.ExpungedBy = enrollee.Expungedby > 0 ? _userservice.GetUser(enrollee.Expungedby).Name : "--";
                ViewBag.ExpungeNote = enrollee.ExpungeNote != null ? enrollee.ExpungeNote : "--";

            }
            else
            {
                ViewBag.StaffPlan = "--";
                ViewBag.Dependents = "--";
                ViewBag.DateExpunges = "--";
                ViewBag.ExpungedBy = "--";
                ViewBag.ExpungeNote = "--";
            }



            ViewBag.PlanList = _companySvc.Getallplan().Where(x => x.Companyid == Convert.ToInt32(staff.CompanyId));
            ViewBag.SubsidiaryList = _companySvc.GetAllSubsidiary().Where(x => x.ParentcompanyId == Convert.ToInt32(staff.CompanyId));




            return PartialView("RestoreStaff", staff);
        }

        [HttpPost]
        public ActionResult RestoreStaff(Staff staff, FormCollection form)
        {

            staff = _companySvc.Getstaff(staff.Id);
            if (staff.Id > 0)
            {
                IList<Enrollee> enrollees = _enrolleeService.GetEnrolleesByStaffId(staff.Id);

                if (enrollees != null)
                {
                    foreach (Enrollee enrollee in enrollees)
                    {
                        if (enrollee != null)
                        {
                            enrollee.ExpungeNote = string.Empty;
                            enrollee.Status = (int)EnrolleesStatus.Active;
                            enrollee.Isexpundged = false;
                            enrollee.Expungedby = 0;
                            enrollee.Dateexpunged = CurrentRequestData.Now;

                            _enrolleeService.UpdateEnrollee(enrollee);

                        }
                    }
                }
                //Expunge the staff by

                staff.IsExpunged = false;

                bool response = _companySvc.UpdateStaff(staff);

                if (response)
                {


                    //successfule
                    //Set the success message for user to see 
                    _pageMessageSvc.SetSuccessMessage(string.Format("Staff [{0}] was restored successfully.",
                        staff.StaffFullname));
                }
                else
                {
                    _pageMessageSvc.SetErrormessage(string.Format("There was a problem  restoring staff [{0}] ",
                        staff.StaffFullname));
                }





            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  restoring staff ."
                        ));
            }


            return _uniquePageService.RedirectTo<StaffListPage>();
        }

        [HttpGet]
        public ActionResult DeleteStaff(int id)
        {
            Staff item = _companySvc.Getstaff(id);
            return PartialView("DeleteStaff", item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteStaff(Staff staff)
        {
            if (staff.Id > 0)
            {


                bool response = _companySvc.DeleteStaff(staff);
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult ExpungeStaff(int id)
        {
            Staff item = _companySvc.Getstaff(id);
            return PartialView("ExpungeStaff", item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExpungeStaff(Staff staff, FormCollection form)
        {

            string expungeNote = form["ExpungeNote"];

            if (staff.Id > 0)
            {
                IList<Enrollee> enrollees = _enrolleeService.GetEnrolleesByStaffId(staff.Id);

                if (enrollees != null)
                {
                    foreach (Enrollee enrollee in enrollees)
                    {
                        if (enrollee != null)
                        {
                            enrollee.ExpungeNote = expungeNote;
                            enrollee.Status = (int)EnrolleesStatus.Inactive;
                            enrollee.Isexpundged = true;
                            enrollee.Expungedby = CurrentRequestData.CurrentUser.Id;
                            enrollee.Dateexpunged = CurrentRequestData.Now;

                            _enrolleeService.UpdateEnrollee(enrollee);

                        }
                    }
                }
                //Expunge the staff by

                Staff staffSys = _companySvc.Getstaff(staff.Id);
                staffSys.IsExpunged = true;

                bool response = _companySvc.UpdateStaff(staffSys);

                if (response)
                {


                    //successfule
                    //Set the success message for user to see 
                    _pageMessageSvc.SetSuccessMessage(string.Format("Staff [{0}] was expunged successfully.",
                                                                    staff.StaffFullname));
                }
                else
                {
                    _pageMessageSvc.SetErrormessage(string.Format("There was a problem  expunging staff [{0}] ",
                                                                  staff.StaffFullname));
                }
                EnrolleeDetailsPage page = _uniquePageService.GetUniquePage<EnrolleeDetailsPage>();

                return _uniquePageService.RedirectTo<EnrolleeListPage>();
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }



        public ActionResult EditPlanBenefitLimit(int id)
        {

            PlanDefaultBenefit benefit = _companySvc.GetPlanBenefit(id);


            return PartialView("EditPlanBenefitLimit", benefit);
        }
        [HttpPost]
        public JsonResult EditPlanBenefitLimit(PlanDefaultBenefit benefit, FormCollection form)
        {
            if (benefit != null)
            {
                _companySvc.UpdatePlanBenefit(benefit);
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        //forplan page
        public JsonResult GetSubscriptionsJson()
        {
            IEnumerable<Subscription> sublist = _companySvc.GetallSubscription().Where(x => x.AuthorizationStatus == (int)AuthorizationStatus.Authorized && x.Status != (int)SubscriptionStatus.Expired && x.Status != (int)SubscriptionStatus.Terminated);

            var responses = from areply in sublist
                            select new
                            {
                                Id = areply.Id,
                                groupname = _companySvc.GetCompany(areply.CompanyId) != null ? _companySvc.GetCompany(areply.CompanyId).Name.ToUpper() : "--",
                                code = areply.SubscriptionCode,
                                Company = _companySvc.GetCompany(areply.CompanyId) != null ? _companySvc.GetCompany(areply.CompanyId).Name.ToUpper() : "--",
                                subsidiary = areply.Subsidiary == null ? "--" : areply.Subsidiary.Subsidaryname.ToUpper(),
                                startdate = Convert.ToDateTime(areply.Startdate).ToString("dd MMM yyyy"),
                                expirationdate = Convert.ToDateTime(areply.Expirationdate).ToString("dd MMM yyyy"),
                                duration = _helperSvc.GetDescription((SubscriptionDuration)areply.Duration),
                                Status = Enum.GetName(typeof(SubscriptionStatus), areply.Status),
                                created = areply.CreatedOn.ToString("dd MMM yyyy")
                            };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }
        public JsonResult GetpendingSubscriptionsJson()
        {
            IEnumerable<Subscription> sublist = _companySvc.GetallSubscription().Where(x => x.AuthorizationStatus == (int)AuthorizationStatus.Pending);
            var responses = from areply in sublist
                            select new
                            {

                                Id = areply.Id,
                                groupname = _companySvc.GetCompany(areply.CompanyId).Name.ToUpper(),
                                code = areply.SubscriptionCode,
                                Company = _companySvc.GetCompany(areply.CompanyId).Name.ToUpper(),
                                subsidiary = areply.Subsidiary != null ? areply.Subsidiary.Subsidaryname.ToUpper() : "--",
                                startdate = Convert.ToDateTime(areply.Startdate, CurrentRequestData.CultureInfo.DateTimeFormat).ToString("dd MMM yyyy"),
                                expirationdate = Convert.ToDateTime(areply.Expirationdate, CurrentRequestData.CultureInfo.DateTimeFormat).ToString("dd MMM yyyy"),
                                duration = _helperSvc.GetDescription((SubscriptionDuration)areply.Duration),
                                Status = Enum.GetName(typeof(SubscriptionStatus), areply.Status),
                                created = areply.CreatedOn.ToString("dd MMM yyyy")
                            };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }
        public JsonResult GetSubscriptionssmmaryJson()
        {
            IEnumerable<Subscription> sublist = _companySvc.GetallSubscription().Where(x => x.Status == (int)SubscriptionStatus.Active && x.AuthorizationStatus == (int)AuthorizationStatus.Authorized);
            List<string> companylist = new List<string>();
            foreach (Subscription item in sublist)
            {
                if (!companylist.Contains(item.CompanyId.ToString()))
                {
                    companylist.Add(item.CompanyId.ToString());
                }
            }


            var responses =
                           new
                           {
                               companycount = _companySvc.GetallCompany().Count,
                               companywithsub = companylist.Count.ToString(),
                               companywithnosub = (_companySvc.GetallCompany().Count - companylist.Count).ToString()

                           };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }
        public JsonResult GetSubscriptionsTop5ExpiringJson()
        {
            IEnumerable<Subscription> sublistexpiringsoon = _companySvc.GetallSubscription().Where(x => x.Status == (int)SubscriptionStatus.Active && x.AuthorizationStatus == (int)AuthorizationStatus.Authorized && x.Expirationdate <= CurrentRequestData.Now.AddMonths(3)).OrderByDescending(x => x.Expirationdate).Take(5);
            var responses = from areply in sublistexpiringsoon
                            let company = _companySvc.GetCompany(areply.CompanyId)

                            select new
                            {
                                Id = areply.Id,
                                company = company != null ? company.Name.ToUpper() : "--",
                                Subsidiary = areply.Subsidiary != null ? areply.Subsidiary.Subsidaryname.ToUpper() : "--",
                                subcode = areply.SubscriptionCode,
                                startdate = Convert.ToDateTime(areply.Startdate).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern),
                                expirationdate = Convert.ToDateTime(areply.Expirationdate).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern),
                                duration = _helperSvc.GetDescription((SubscriptionDuration)areply.Duration)
                            };

            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }
        [ActionName("Subscription")]
        public ActionResult Subscription(SubscriptionPage page)
        {
            //
            NotificationTable table = _helperSvc.GetNotificationTable(4);

            if (table != null)
            {
                foreach (string item in table.Roles.Split(','))
                {
                    int intitem = 0;
                    int.TryParse(item, out intitem);
                    if (CurrentRequestData.CurrentUser.Roles.Contains(_rolesvc.GetRole(intitem)))
                    {
                        ViewBag.ShowPendingSubscription = true;
                        break;
                    }
                    else
                    {
                        ViewBag.ShowPendingSubscription = false;
                    }
                }
            }

            //Return to the providers list page
            return View(page);
        }

        [HttpGet]
        public ActionResult AddSubscription()
        {
            //ViewBag.CompanyList
            Subscription subscription = new Subscription();
            subscription.SubscriptionCode = _helperSvc.GeneratesubscriptionCode();
            IList<Company> list = _companySvc.GetallCompany();

            list.Add(new Company() { Id = -1, Name = "--Select--" });
            ViewBag.CompanyList = list.OrderBy(x => x.Id);



            var plans = new[]
                 {
                    new {Id = "-1", Name = "Select"}

                };
            ViewBag.subplansbag = new MultiSelectList(plans, "Id", "Name");
            ViewBag.Duration = new[]
                {
                    new {Id = "-1", Name = "Select"},
                      new {Id = "0", Name = "3 Months"},
                       new {Id = "1", Name = "6 Months"},
                             new {Id = "2", Name = "1 Year"}
                };

            return PartialView("AddSubscription", subscription);
        }
        //Action for Add View
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult AddSubscription(Subscription subscription, FormCollection form)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            //Get the form items.
            string startdate = form["startdate"];
            string enddate = form["enddate"];

            string plans = form["Planid"];
            string code = form["subCode"];
            string subsidiary = form["Subsidiary"];
            subscription.SubscriptionCode = code;

            DateTime Sdate = CurrentRequestData.Now;
            DateTime Edate = CurrentRequestData.Now;
            if (DateTime.TryParse(startdate, out Sdate))
            {
                subscription.Startdate = Sdate;

            }

            if (DateTime.TryParse(enddate, out Edate))
            {
                subscription.Expirationdate = Edate;

            }

            subscription.Companyplans = plans.Trim();
            subscription.AuthorizationStatus = (int)AuthorizationStatus.Pending;
            subscription.Createdby = CurrentRequestData.CurrentUser.Id;
            subscription.Status = (int)SubscriptionStatus.Default;


            //Set expiration date.
            //switch (subscription.Duration)
            //{

            //    case 0:
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Startdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddMonths(3);
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Expirationdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddDays(-1);

            //        break;
            //    case 1:
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Startdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddMonths(6);
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Expirationdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddDays(-1);
            //        break;
            //    case 2:
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Startdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddMonths(12);
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Expirationdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddDays(-1);
            //        break;
            //}


            //subscription.SubscriptionCode += "-" + _companySvc.GetCompany(subscription.CompanyId).Name.Substring(0, 5);


            //check subsidiary

            if (Convert.ToInt32(subsidiary) > -1)
            {
                subscription.Subsidiary = _companySvc.Getsubsidiary(Convert.ToInt32(subsidiary));
            }
            bool response = _companySvc.AddSubscription(subscription);

            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Subscription [{0}] was added successfully.", subscription.SubscriptionCode.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  adding Subscription [{0}] ",
               subscription.SubscriptionCode.ToUpper()));
            }

            return _uniquePageService.RedirectTo<SubscriptionPage>();



        }


        public ActionResult EditSubscription(int id)
        {

            Subscription subscription = _companySvc.GetSubscription(id);

            IList<Company> list = _companySvc.GetallCompany();

            list.Add(new Company() { Id = -1, Name = "--Select--" });
            ViewBag.CompanyList = list.OrderBy(x => x.Id);



            IEnumerable<CompanyPlan> plans = _companySvc.Getallplan().Where(x => x.Companyid == Convert.ToInt32(subscription.CompanyId));
            ViewBag.subplansbag = new MultiSelectList(plans, "Id", "Planfriendlyname", subscription.Companyplans.Split(',').ToArray());
            ViewBag.Durationbag = new[]
                {
                    new {Id = "-1", Name = "Select"},
                      new {Id = "0", Name = "3 Months"},
                       new {Id = "1", Name = "6 Months"},
                             new {Id = "2", Name = "1 Year"}
                };
            IList<CompanySubsidiary> subsid = _companySvc.GetAllSubsidiaryofACompany(subscription.CompanyId);

            List<SelectListItem> sub_list = new List<SelectListItem>();

            foreach (CompanySubsidiary item in subsid)
            {

                sub_list.Add(new SelectListItem()
                {
                    Value = Convert.ToString(item.Id),
                    Text = item.Subsidaryname.ToUpper(),
                    Selected = item.Id == subscription.CompanyId ? true : false

                });
            }
            //sub_list.Insert(0, new GenericReponse2()
            //{
            //    Id = -1,
            //    Name = "All Subsidiary"
            //});
            ViewBag.SubsidiaryList = sub_list;


            return PartialView("EditSubscription", subscription);
        }
        [HttpPost]
        public JsonResult EditSubscription(Subscription subscription, FormCollection form)
        {
            //Get the form items.
            //Get the form items.
            string startdate = form["startdate"];
            string enddate = form["enddate"];

            DateTime Sdate = CurrentRequestData.Now;
            DateTime Edate = CurrentRequestData.Now;
            if (DateTime.TryParse(startdate, out Sdate))
            {
                subscription.Startdate = Sdate;

            }

            if (DateTime.TryParse(enddate, out Edate))
            {
                subscription.Expirationdate = Edate;

            }


            string plans = form["Planid"];
            string code = form["subCode"];
            subscription.SubscriptionCode = code;
            string subsidiary = form["SubsidiaryID"];


            subscription.Companyplans = plans.Trim();

            //switch (subscription.Duration)
            //{

            //    case 0:
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Startdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddMonths(3);
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Expirationdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddDays(-1);

            //        break;
            //    case 1:
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Startdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddMonths(6);
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Expirationdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddDays(-1);
            //        break;
            //    case 2:
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Startdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddMonths(12);
            //        subscription.Expirationdate = Convert.ToDateTime(subscription.Expirationdate, CurrentRequestData.CultureInfo.DateTimeFormat).AddDays(-1);
            //        break;
            //}

            //subscription.SubscriptionCode += "-" + _companySvc.GetCompany(subscription.CompanyId).Name.Substring(0, 5);
            if (Convert.ToInt32(subsidiary) > -1)
            {
                //theres subsidiary
                CompanySubsidiary sub = _companySvc.Getsubsidiary(Convert.ToInt32(subsidiary));

                subscription.Subsidiary = sub;


            }
            bool response = _companySvc.UpdateSubscription(subscription);

            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Subscription [{0}] was updated successfully.", subscription.SubscriptionCode.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  updating Subscription [{0}] ",
               subscription.SubscriptionCode.ToUpper()));
            }

            //return _uniquePageService.RedirectTo<SubscriptionPage>();
            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteSubscription(int id)
        {
            Subscription item = _companySvc.GetSubscription(id);


            return PartialView("DeleteSubscription", item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSubscription(Subscription subscription)
        {
            if (subscription.Id > 0)
            {

                if (_companySvc.GetSubscription(subscription.Id).Status == (int)SubscriptionStatus.Default)
                {
                    bool response = _companySvc.DeleteSubscription(subscription);
                }
                else
                {
                    //there was an error trying to delete a subscription that has been approved.
                }

            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult TerminateSubscription(int id)
        {
            Subscription item = _companySvc.GetSubscription(id);


            return PartialView("TerminateSubscription", item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TerminateSubscription(Subscription subscription)
        {
            if (subscription.Id > 0)
            {
                Subscription mainsub = _companySvc.GetSubscription(subscription.Id);
                if (mainsub.Status == (int)SubscriptionStatus.Active)
                {

                    mainsub.TerminationNote = subscription.TerminationNote;
                    mainsub.Terminatedby = CurrentRequestData.CurrentUser.Id;
                    mainsub.Status = (int)SubscriptionStatus.Terminated;

                    bool response = _companySvc.UpdateSubscription(subscription);
                }
                else
                {
                    //there was an error trying to delete a subscription that has been approved.


                }

            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubscriptionDetails(int id)
        {

            Subscription subscription = _companySvc.GetSubscription(id);
            ViewBag.CompanyName = _companySvc.GetCompany(subscription.CompanyId).Name.ToUpper();
            IList<CompanyPlan> list = _companySvc.Getallplan();

            string[] lst = subscription.Companyplans.Split(',');
            ViewBag.Plans = lst.Aggregate("", (current, item) => current + ("," + _companySvc.GetCompanyPlan(Convert.ToInt32(item)).Planfriendlyname.ToUpper()));


            switch ((SubscriptionDuration)subscription.Duration)
            {
                case SubscriptionDuration.Month3:
                    ViewBag.duration = "3 Months";
                    break;
                case SubscriptionDuration.Month6:
                    ViewBag.duration = "6 Months";
                    break;
                case SubscriptionDuration.Month12:
                    ViewBag.duration = "1 Year";
                    break;
            }
            ViewBag.createdByStr = _userservice.GetUser(subscription.Createdby).Name;

            ViewBag.AuthorizationStatusStr = Enum.GetName(typeof(AuthorizationStatus), subscription.AuthorizationStatus);

            ViewBag.AuthorizedByStr = "--";

            if ((AuthorizationStatus)subscription.AuthorizationStatus == AuthorizationStatus.Authorized && subscription.AuthorizedBy > 0)
            {
                ViewBag.AuthorizedByStr = _userservice.GetUser(subscription.AuthorizedBy).Name.ToString();

            }

            return PartialView("SubscriptionDetails", subscription);
        }

        //Action for Edit View
        [HttpGet]
        public ActionResult ApproveSubscription(int id)
        {
            Subscription subscription = _companySvc.GetSubscription(id);
            ViewBag.CompanyName = _companySvc.GetCompany(subscription.CompanyId).Name.ToUpper();
            IList<CompanyPlan> list = _companySvc.Getallplan();

            string[] lst = subscription.Companyplans.Split(',');
            ViewBag.Plans = lst.Aggregate("", (current, item) => current + ("," + _companySvc.GetCompanyPlan(Convert.ToInt32(item)).Planfriendlyname.ToUpper()));

            ViewBag.Subsidiary = subscription.Subsidiary != null ? subscription.Subsidiary.Subsidaryname.ToUpper() : "--";
            switch ((SubscriptionDuration)subscription.Duration)
            {
                case SubscriptionDuration.Month3:
                    ViewBag.duration = "3 Months";
                    break;
                case SubscriptionDuration.Month6:
                    ViewBag.duration = "6 Months";
                    break;
                case SubscriptionDuration.Month12:
                    ViewBag.duration = "1 Year";
                    break;
            }
            ViewBag.createdByStr = _userservice.GetUser(subscription.Createdby).Name;

            ViewBag.AuthorizationStatusStr = Enum.GetName(typeof(AuthorizationStatus), subscription.AuthorizationStatus);

            ViewBag.AuthorizedByStr = "--";

            if ((AuthorizationStatus)subscription.AuthorizationStatus == AuthorizationStatus.Authorized && subscription.AuthorizedBy > 0)
            {
                ViewBag.AuthorizedByStr = _userservice.GetUser(subscription.AuthorizedBy).ToString();

            }
            return PartialView("ApproveSubscription", subscription);

        }
        [HttpPost]
        public ActionResult ApproveSubscription(Subscription subscription, FormCollection form)
        {

            Subscription subscriptionM = _companySvc.GetSubscription(subscription.Id);
            subscriptionM.AuthorizationNote = subscription.AuthorizationNote;
            subscriptionM.AuthorizationStatus = (int)AuthorizationStatus.Authorized;
            subscriptionM.AuthorizedDate = CurrentRequestData.Now;
            subscriptionM.AuthorizedBy = CurrentRequestData.CurrentUser.Id;

            //update the stuff 
            bool resp = _companySvc.UpdateSubscription(subscriptionM);
            //if (resp)
            //{


            //    //successfule
            //    //Set the success message for user to see 
            //    _pageMessageSvc.SetSuccessMessage(string.Format("Provider [{0}] was approved successfully.", providerM.Name.ToUpper()));
            //}
            //else
            //{
            //    _pageMessageSvc.SetErrormessage(string.Format("There was a problem  approving provider [{0}] ",
            //                                                  providerM.Name.ToUpper()));
            //}

            return _uniquePageService.RedirectTo<SubscriptionPage>();
        }
        //Action for Edit View
        [HttpGet]
        public ActionResult DisapproveSubscription(int id)
        {
            Subscription subscription = _companySvc.GetSubscription(id);
            ViewBag.CompanyName = _companySvc.GetCompany(subscription.CompanyId).Name.ToUpper();
            IList<CompanyPlan> list = _companySvc.Getallplan();

            string[] lst = subscription.Companyplans.Split(',');
            ViewBag.Plans = lst.Aggregate("", (current, item) => current + ("," + _companySvc.GetCompanyPlan(Convert.ToInt32(item)).Planfriendlyname.ToUpper()));


            switch ((SubscriptionDuration)subscription.Duration)
            {
                case SubscriptionDuration.Month3:
                    ViewBag.duration = "3 Months";
                    break;
                case SubscriptionDuration.Month6:
                    ViewBag.duration = "6 Months";
                    break;
                case SubscriptionDuration.Month12:
                    ViewBag.duration = "1 Year";
                    break;
            }
            ViewBag.createdByStr = _userservice.GetUser(subscription.Createdby).Name;

            ViewBag.AuthorizationStatusStr = Enum.GetName(typeof(AuthorizationStatus), subscription.AuthorizationStatus);

            ViewBag.AuthorizedByStr = "--";

            if ((AuthorizationStatus)subscription.AuthorizationStatus == AuthorizationStatus.Authorized && subscription.AuthorizedBy > 0)
            {
                ViewBag.AuthorizedByStr = _userservice.GetUser(subscription.AuthorizedBy).ToString();

            }
            return PartialView("DisapproveSubscription", subscription);

        }

        [HttpPost]
        public ActionResult DisapproveSubscription(Subscription subscription, FormCollection form)
        {

            Subscription subscriptnionM = _companySvc.GetSubscription(subscription.Id);
            subscriptnionM.DisapprovalNote = subscription.DisapprovalNote;
            subscriptnionM.AuthorizationStatus = (int)AuthorizationStatus.Disapproved;
            subscriptnionM.DisapprovalDate = CurrentRequestData.Now;
            subscriptnionM.DisapprovedBy = CurrentRequestData.CurrentUser.Id;

            //update the subscription
            bool resp = _companySvc.UpdateSubscription(subscriptnionM);
            //if (resp)
            //{


            //    //successfule
            //    //Set the success message for user to see 
            //    _pageMessageSvc.SetSuccessMessage(string.Format("Provider [{0}] was disapproved successfully.", providerM.Name.ToUpper()));
            //}
            //else
            //{
            //    _pageMessageSvc.SetErrormessage(string.Format("There was a problem  disapproving provider [{0}] ",
            //                                                  providerM.Name.ToUpper()));
            //}

            return _uniquePageService.RedirectTo<SubscriptionPage>();
        }

        [HttpGet]
        public ActionResult AddSubsidiary(int id)
        {
            CompanySubsidiary sub = new CompanySubsidiary();
            sub.ParentcompanyId = id;



            return PartialView("AddSubsidiary", sub);
        }

        [HttpPost]
        public ActionResult AddSubsidiary(CompanySubsidiary companySub)
        {


            bool response = false;
            if (companySub != null)
            {
                companySub.CreatedBy = CurrentRequestData.CurrentUser.Id;
                response = _companySvc.AddSubsidiary(companySub);
            }

            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Subsidiary [{0}] was added successfully.", companySub.Subsidaryname.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  adding Subsidiary [{0}] ",
               companySub.Subsidaryname.ToUpper()));
            }
            CompanySubPage page = _uniquePageService.GetUniquePage<CompanySubPage>();
            return Redirect(string.Format(page.AbsoluteUrl + "?id={0}&tabid=2", companySub.ParentcompanyId));
        }


        [HttpGet]
        public ActionResult EditSubsidiary(int id)
        {
            CompanySubsidiary sub = _companySvc.Getsubsidiary(id);




            return PartialView("EditSubsidiary", sub);
        }
        [HttpPost]
        public ActionResult EditSubsidiary(CompanySubsidiary companySub)
        {


            bool response = false;
            CompanySubsidiary sub = _companySvc.Getsubsidiary(companySub.Id);
            if (sub != null)
            {
                //companySub.CreatedBy = CurrentRequestData.CurrentUser.Id;
                sub.Subsidaryname = companySub.Subsidaryname;
                sub.Subsidaryprofile = companySub.Subsidaryprofile;
                response = _companySvc.Updatesubsidiary(sub);
            }

            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Subsidiary [{0}] was updated successfully.", companySub.Subsidaryname.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  updating Subsidiary [{0}] ",
               companySub.Subsidaryname.ToUpper()));
            }
            CompanySubPage page = _uniquePageService.GetUniquePage<CompanySubPage>();
            return Redirect(string.Format(page.AbsoluteUrl + "?id={0}", companySub.ParentcompanyId));
        }

        [HttpGet]
        public ActionResult DeleteSubsidiary(int id)
        {
            CompanySubsidiary sub = _companySvc.Getsubsidiary(id);



            return PartialView("DeleteSubsidiary", sub);
        }
        [HttpPost]
        public ActionResult DeleteSubsidiary(CompanySubsidiary companySub)
        {

            CompanySubsidiary sub = _companySvc.Getsubsidiary(companySub.Id);

            bool response = false;
            if (sub != null)
            {
                //companySub.CreatedBy = CurrentRequestData.CurrentUser.Id;
                response = _companySvc.DeleteSubsidiary(sub);
            }

            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Subsidiary [{0}] was deleted successfully.", companySub.Subsidaryname.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  deleting Subsidiary [{0}] ",
               companySub.Subsidaryname.ToUpper()));
            }
            CompanySubPage page = _uniquePageService.GetUniquePage<CompanySubPage>();
            return Redirect(string.Format(page.AbsoluteUrl + "?id={0}", companySub.ParentcompanyId));
        }

        public JsonResult GetAutomaticExpungeStaffListJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);

            string search = CurrentRequestData.CurrentContext.Request["sSearch"];
            int toltareccount = 0;
            int totalinresult = 0;
            IList<AutomaticExpungeStaff> staffList = _companySvc.QueryAllAutomaticExpungeStaff(out toltareccount, out totalinresult, search,
                                                      Convert.ToInt32(displayStart),
                                                      Convert.ToInt32(displayLength), sortColumnnumber, sortOrder,
                                                      search);

            var responses = from areply in staffList
                            let staff = _companySvc.Getstaff(areply.StaffId)
                            let company = _companySvc.GetCompany(areply.Companyid)
                            let compsub = _companySvc.Getsubsidiary(areply.Subsidiary)
                            select new
                            {
                                Id = areply.Id,
                                staffid = areply.StaffId,
                                name = staff != null ? staff.StaffFullname : "Nil",
                                company = company != null ? company.Name.ToUpper() : "NIL",
                                subsidairy = compsub != null ? compsub.Subsidaryname.ToUpper() : "NIL",
                                dateadded = areply.CreatedOn

                            };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                aaData = response.Data
            });



        }

        public JsonResult GetBulkJobsJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);

            string search = CurrentRequestData.CurrentContext.Request["sSearch"];
            int toltareccount = 0;
            int totalinresult = 0;
            IList<StaffUploadJob> staffList = _companySvc.QueryStaffUploadJobs(out toltareccount, out totalinresult, search,
                                                      Convert.ToInt32(displayStart),
                                                      Convert.ToInt32(displayLength), sortColumnnumber, sortOrder,
                                                      null, null, -1);

            var responses = from areply in staffList

                            let company = _companySvc.GetCompany(areply.CompanyID)
                            let compsub = _companySvc.Getsubsidiary(areply.Subsidiary)
                            let user = _userservice.GetUser(areply.UploadedBy)
                            select new
                            {
                                Id = areply.Id,

                                UploadedBy = user.Name,
                                Company = company != null ? company.Name : "--",
                                Companyid = company != null ? company.Id : -1,
                                Subsidiary = compsub != null ? compsub.Subsidaryname : "--",
                                Mode = Enum.GetName(typeof(JobExpungeMode), areply.ExpungeMode),
                                Status = Enum.GetName(typeof(JobStatus), areply.JobStatus),
                                TotalRecordinFile = areply.TotalRecord,
                                TotalRecordProcessed = areply.TotalRecordDone,
                                TotalRecordSuccessful = areply.TotalRecordSuccess,
                                TotalRecordFailed = areply.TotalRecordFailed,
                                TotalRecordaddedToStaffList = areply.TotalStaffAdded,
                                TotalRecordForExpunge = areply.TotalStaffForExpunged,
                                StartTime = areply.StartTime,
                                approved = areply.approved,
                                EndTime = areply.FinishTime,
                                dateadded = areply.CreatedOn,
                                analysislink = areply.Analysislink
                            };
            JsonResult response = Json(responses, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                aaData = response.Data
            });



        }


        public JsonResult GetallStaffinCompanyJson(int companyid)
        {
            IDictionary<int, string> item = _companySvc.GetAllStaffInCompany(companyid);

            return Json(item, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetallStaffinCompanyLiteJson(int companyid)
        {
            List<StaffnameandPlan> item = _companySvc.GetAllStaffinCompanyLite(companyid).Where(x => x.hasprofile == true && x.Expunged == true).ToList();

            return Json(item, JsonRequestBehavior.AllowGet);
        }


        public ActionResult BulkLoadStaff(int? id)
        {
            int idd = Convert.ToInt32(id);
            ViewBag.Idd = idd;

            return PartialView("AddLoadStaff");
        }
        public ActionResult DoBulkUpload(FormCollection form)
        {
            string thelist = form["bulklist"];
            string thejob = form["jobid"];

            if (!string.IsNullOrEmpty(thelist) && !string.IsNullOrEmpty(thejob))
            {


                StaffUploadJob job = _companySvc.GetStaffUploadJob(Convert.ToInt32(thejob));


                if (job != null)
                {
                    string[] splitted = thelist.Split(';');

                    foreach (string item in splitted)
                    {
                        if (item.Contains(':'))
                        {
                            //not in the main list

                            string staffname = item.Trim().Split(':')[0];
                            string planid = item.Trim().Split(':')[1];
                            string staffidcard = "";
                            try
                            {
                                staffidcard = item.Trim().Split(':')[2];
                            }
                            catch (Exception ex)
                            {

                            }



                            CompanyPlan plan = _companySvc.GetCompanyPlan(Convert.ToInt32(planid));


                            if (plan != null && plan.Companyid == job.CompanyID)
                            {
                                //add the staff
                                Staff staff = new Staff();
                                staff.StaffFullname = staffname.ToUpper();
                                staff.CompanyId = Convert.ToString(job.CompanyID);
                                staff.StaffId = staffidcard;
                                staff.CompanySubsidiary = job.Subsidiary;
                                staff.StaffPlanid = plan.Id;
                                _companySvc.AddStaff(staff);


                            }
                        }
                        else
                        {
                            //in the uploaded list check the factor

                            //don't have the strength to implement this now.
                        }
                    }
                }

            }
            _pageMessageSvc.SetSuccessMessage("Bulk Staff processed successfully.");


            return _uniquePageService.RedirectTo<BulkStaffJobPage>();



        }



        public ActionResult BulkExpungeStaff(int? id)
        {
            int idd = Convert.ToInt32(id);
            ViewBag.Idd = idd;

            return PartialView("BulkExpungeLoadStaff");
        }

        public ActionResult DoExpungeUpload(FormCollection form)
        {
            string thelist = form["bulklist"];
            string thejob = form["jobid"];

            if (!string.IsNullOrEmpty(thelist) && !string.IsNullOrEmpty(thejob))
            {


                StaffUploadJob job = _companySvc.GetStaffUploadJob(Convert.ToInt32(thejob));


                if (job != null)
                {
                    string[] splitted = thelist.Split(';');

                    foreach (string item in splitted)
                    {
                        if (item.Contains(':'))
                        {
                            //not in the main list



                            Match staffid = Regex.Matches(item.Split(':')[1], "\\d+")[0];

                            Staff staff = _companySvc.Getstaff(Convert.ToInt32(staffid.ToString()));

                            if (staff != null)
                            {
                                staff.IsExpunged = true;
                                if (staff.Id > 0)
                                {
                                    IList<Enrollee> enrollees = _enrolleeService.GetEnrolleesByStaffId(staff.Id);

                                    if (enrollees != null)
                                    {
                                        foreach (Enrollee enrollee in enrollees)
                                        {
                                            if (enrollee != null)
                                            {
                                                enrollee.ExpungeNote = "automatic Staff Expunged.";
                                                enrollee.Status = (int)EnrolleesStatus.Inactive;
                                                enrollee.Isexpundged = true;
                                                enrollee.Expungedby = CurrentRequestData.CurrentUser.Id;
                                                enrollee.Dateexpunged = CurrentRequestData.Now;

                                                _enrolleeService.UpdateEnrollee(enrollee);

                                            }
                                        }
                                    }
                                    //Expunge the staff by


                                    staff.IsExpunged = true;

                                    bool response = _companySvc.UpdateStaff(staff);



                                }

                                //var response = _companySvc.UpdateStaff(staff);


                            }


                        }
                        else
                        {
                            //in the uploaded list check the factor

                            //don't have the strength to implement this now.
                        }
                    }
                }

            }
            _pageMessageSvc.SetSuccessMessage("Bulk expunge processed successfully.");


            return _uniquePageService.RedirectTo<BulkStaffJobPage>();



        }


        public ActionResult ApproveUpload(int id)
        {


            StaffUploadJob job = _companySvc.GetStaffUploadJob(id);

            if (job != null && CurrentRequestData.CurrentUser.Roles.Where(x => x.Id == 7).Any())
            {
                job.approved = true;
                job.dateapproved = CurrentRequestData.Now;
                job.approvedby = CurrentRequestData.CurrentUser.Id;



                _companySvc.UpdateStaffUploadJobs(job);
                _pageMessageSvc.SetSuccessMessage("Upload was approved successfully.");



            }
            else
            {
                _pageMessageSvc.SetErrormessage("You don't have permission to approve this job.");
            }

            return _uniquePageService.RedirectTo<BulkStaffJobPage>();

        }

        public int DoStaffLinking(string Ids)
        {
            if (!string.IsNullOrEmpty(Ids))
            {

                Ids = Ids.Replace(",", ";");
                string[] splitted = Ids.Split(';');
                if (splitted.Any())
                {
                    int staffid = Convert.ToInt32(splitted[0]);
                    int staffid2 = Convert.ToInt32(splitted[1]);

                    Staff staffold = _companySvc.Getstaff(staffid);
                    Staff staffnew = _companySvc.Getstaff(staffid2);

                    //remove first two from splitted

                    splitted.ElementAt(0).Remove(0);
                    splitted.ElementAt(1).Remove(1);
                    if (staffold != null && staffnew != null)
                    {
                        //now do the magic
                        IList<Enrollee> enrollees = _enrolleeService.GetEnrolleesByStaffId(staffid);
                        Enrollee principal = enrollees.Where(x => x.Parentid == 0).FirstOrDefault();

                        principal.Isexpundged = false;
                        principal.IsDeleted = false;

                        principal.Companyid = Convert.ToInt32(staffnew.CompanyId);
                        principal.Subscriptionplanid = staffnew.StaffPlanid;
                        principal.Staffprofileid = staffnew.Id;


                        //update enrollee
                        _enrolleeService.UpdateEnrollee(principal);


                        foreach (Enrollee enrollee in enrollees)
                        {
                            //change all the details to the new staff

                            enrollee.Companyid = Convert.ToInt32(staffnew.CompanyId);
                            enrollee.Subscriptionplanid = staffnew.StaffPlanid;
                            enrollee.Staffprofileid = staffnew.Id;

                            if (splitted.Contains(Convert.ToString(enrollee.Id)))
                            {

                                enrollee.Isexpundged = false;
                                enrollee.IsDeleted = false;
                                enrollee.Status = 1;

                            }
                            else
                            {
                                //exclude the principal
                                if (enrollee.Id != principal.Id)
                                {
                                    enrollee.Isexpundged = true;
                                    //enrollee.IsDeleted = true;
                                    enrollee.Expungedby = CurrentRequestData.CurrentUser.Id;
                                    enrollee.ExpungeNote = "Enrollee was expunged during Staff Linking.";
                                }


                            }


                            _enrolleeService.UpdateEnrollee(enrollee);

                        }

                        //change the staff
                        staffnew.HasProfile = true;
                        staffnew.Profileid = principal.Id;
                        _companySvc.UpdateStaff(staffnew);

                        staffold.NewStaffId = staffnew.Id;
                        staffold.stafflinkDate = CurrentRequestData.Now;
                        staffold.stafflinkUSer = CurrentRequestData.CurrentUser.Id;
                        staffold.IsDeleted = true;
                        _companySvc.UpdateStaff(staffold);

                        return 1;


                    }
                    else
                    {
                        return 99;

                    }
                }
            }
            else
            {
                return 99;
            }

            return 1;
        }

        [HttpGet]
        public JsonResult importPlanBenefits(int planid)
        {
            CompanyPlan companyplan = _companySvc.GetCompanyPlan(planid);
            List<GenericReponse2> plansout = new List<GenericReponse2>();
            if (companyplan != null)
            {
                IEnumerable<CompanyPlan> companyplans = _companySvc.GetallplanForCompany(companyplan.Companyid).Where(x => x.Planid == companyplan.Planid);


                foreach (CompanyPlan item in companyplans)
                {
                    GenericReponse2 planobj = new GenericReponse2();

                    if (item.Id != planid)
                    {
                        planobj.Id = item.Id;
                        planobj.Name = item.Planfriendlyname.ToUpper();
                        plansout.Add(planobj);
                    }


                }

                return Json(plansout, JsonRequestBehavior.AllowGet);



            }

            return Json(plansout, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult importPlanBenefits(FormCollection form)
        {
            string mainplan = form["planid"];
            string cloneplan = form["cloneplanid"];

            if (!string.IsNullOrEmpty(mainplan) && !string.IsNullOrEmpty(cloneplan))
            {
                IList<CompanyBenefit> benefitlist = _companySvc.GetCompanyPlanBenefits(Convert.ToInt32(cloneplan));
                //clear the benefits of the previous 
                IList<CompanyBenefit> benefitlistold = _companySvc.GetCompanyPlanBenefits(Convert.ToInt32(mainplan));

                foreach (CompanyBenefit item in benefitlistold)
                {
                    _companySvc.DeleteCompanyPlanBenefit(item);

                }


                foreach (CompanyBenefit item in benefitlist)
                {

                    CompanyBenefit itemm = new CompanyBenefit();
                    itemm.Companyid = item.Companyid;
                    itemm.CompanyPlanid = Convert.ToInt32(mainplan);
                    itemm.BenefitLimit = item.BenefitLimit;
                    itemm.BenefitId = item.BenefitId;


                    _companySvc.AddCompanyPlanBenefit(itemm);

                }
            }
            _pageMessageSvc.SetSuccessMessage("Benefits imported successfully.");

            return _uniquePageService.RedirectTo<CompanyPlanPage>();

        }
    }




}
