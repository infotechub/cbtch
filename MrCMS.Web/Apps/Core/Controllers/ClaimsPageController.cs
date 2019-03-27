
using MrCMS.Entities.Messaging;
using MrCMS.Logging;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Models.Plan;
using MrCMS.Web.Apps.Core.Pages;
using MrCMS.Web.Apps.Core.Services;
using MrCMS.Web.Apps.Core.Services.UserChat;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Areas.Admin.Services;
using MrCMS.Website;
using MrCMS.Website.Controllers;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CsvHelper;

namespace MrCMS.Web.Apps.Core.Controllers
{
    public class ClaimsPageController : MrCMSAppUIController<CoreApp>
    {
        private readonly IPlanService _planService;
        private readonly IServicesService _serviceSvc;
        private readonly IUniquePageService _uniquePageService;
        private readonly IPageMessageSvc _pageMessageSvc;
        private readonly IHelperService _helperSvc;
        private readonly IProviderService _providerSvc;
        private readonly ITariffService _tariffSvc;
        private readonly ILogAdminService _logger;
        private readonly ICompanyService _companyService;
        private readonly IEnrolleeService _enrolleeService;
        private readonly ISmsService _smsservice;
        private readonly IUserChat _chatservice;
        private readonly IClaimService _claimsvc;
        private readonly IUserService _userservice;
        private readonly IRoleService _rolesvc;
        private readonly MailSettings _mailSettings;
        private readonly IEmailSender _emailSender;
        public ClaimsPageController(IPlanService planService, IUniquePageService uniquepageService,
                                      IPageMessageSvc pageMessageSvc, IHelperService helperService,
                                      IServicesService serviceSvc, IProviderService Providersvc, ILogAdminService logger,
                                      ITariffService tariffService, ICompanyService companyService,
                                      IEnrolleeService enrolleeService, ISmsService smsSvc, IUserChat UserChat, IClaimService claimservice, IUserService userservice, MailSettings mailSettings, IEmailSender emailSender, IRoleService roleService)
        {
            _planService = planService;
            _uniquePageService = uniquepageService;
            _pageMessageSvc = pageMessageSvc;
            _helperSvc = helperService;
            _serviceSvc = serviceSvc;
            _providerSvc = Providersvc;
            _logger = logger;
            _tariffSvc = tariffService;
            _companyService = companyService;
            _enrolleeService = enrolleeService;
            _smsservice = smsSvc;
            _chatservice = UserChat;
            _claimsvc = claimservice;
            _userservice = userservice;
            _mailSettings = mailSettings;
            _emailSender = emailSender;
            _rolesvc = roleService;

        }


        [ActionName("IncomingClaims")]
        public ActionResult IncomingClaims(IncomingClaimsPage Page)
        {


            //year list

            int year = CurrentRequestData.Now.Year;
            List<GenericReponse2> yealist = new List<GenericReponse2>();

            List<GenericReponse2> provslist = new List<GenericReponse2>();
            for (int i = 0; i < 5; i++)
            {
                yealist.Add(new GenericReponse2 { Id = year - i, Name = (year - i).ToString() });
            }
            ViewBag.YearList = yealist;




            ViewBag.PrvidersList = _providerSvc.GetProviderNameList().OrderBy(x => x.Name);
            ViewBag.MyProvidersList = _providerSvc.GetProviderNameList().OrderBy(x => x.Name);

            IList<MrCMS.Entities.People.User> users = _chatservice.Getallusers();

            var userlist = from aereply in users
                           select new
                           {
                               Id = aereply.Id,
                               Name = aereply.Name,
                           };

            var userr = userlist.ToList();

            userr.Add(new { Id = -1, Name = "Select" });


            ViewBag.UserList = userr.OrderBy(x => x.Id).ToList();

            ViewBag.Defaultdate = CurrentRequestData.Now.ToString("dd/MM/yyyy");
            return View(Page);
        }


        [HttpPost]
        public ActionResult IncomingClaims(FormCollection form)
        {
            //Get the form items
            string Provider_list = form["Provider_list"];
            string Month_list = form["Month_list"];
            string caption = form["caption"];
            string Year = form["Year"];
            string Delivered_By = form["Delivered_By"];
            string User_list = form["User_list"];
            string NoofEncounter = form["NoofEncounter"];
            string TotalAmount = form["TotalAmount"];
            string Note_txt = form["Note_txt"];
            string DateReceived = form["DateReceived"];

            string Idholder = form["Idholder"];
            DateTime daterecived = new DateTime();


            //validate

            if (string.IsNullOrEmpty(Month_list) || string.IsNullOrEmpty(DateReceived) || string.IsNullOrEmpty(Provider_list))
            {
                _pageMessageSvc.SetErrormessage("Ooops I can't take this ,kindly fill the form properly.");
                return _uniquePageService.RedirectTo<IncomingClaimsPage>();
            }

            if (!string.IsNullOrEmpty(DateReceived) && DateReceived.Split('/').Count() > 2)
            {


                //var year = Convert.ToInt32(DateReceived.Split('/')[2]);
                //var month = Convert.ToInt32(DateReceived.Split('/')[1]);
                //var day = Convert.ToInt32(DateReceived.Split('/')[0]);



                daterecived = !string.IsNullOrEmpty(DateReceived) ? Tools.ParseMilitaryTime(DateReceived) : CurrentRequestData.Now;

            }
            else
            {
                daterecived = CurrentRequestData.Now;
            }


            if (!string.IsNullOrEmpty(Provider_list))
            {

                IncomingClaims claim = new IncomingClaims
                {
                    providerid = Convert.ToInt32(Provider_list),
                    month = Convert.ToInt32(daterecived.Month),
                    month_string = Month_list,
                    year = Convert.ToInt32(Year),
                    deliveredby = string.IsNullOrEmpty(Delivered_By) ? "Unknown" : Delivered_By,
                    caption = caption,
                    transferedTo = Convert.ToInt32(User_list),
                    //noofencounter = Convert.ToInt32(NoofEncounter),
                    //totalamount = Convert.ToDecimal(TotalAmount),
                    Note = Note_txt,
                    fullDateofbill = daterecived,          //new DateTime(Convert.ToInt32(Year),Convert.ToInt32(Month_list),01,00,59,00,CurrentRequestData.CultureInfo.Calendar),
                    receivedBy = CurrentRequestData.CurrentUser.Id,
                    DateReceived = daterecived,
                    status = ReceivedClaimStatus.Received,

                };

                //update if it exist

                if (!string.IsNullOrEmpty(Idholder))
                {
                    IncomingClaims oldclaim = _claimsvc.GetIncomingClaim(Convert.ToInt32(Idholder));

                    oldclaim.providerid = claim.providerid;
                    oldclaim.deliveredby = claim.deliveredby;
                    //this value is not correct we are using month string instead
                    oldclaim.month = claim.month;
                    oldclaim.year = claim.year;
                    oldclaim.month_string = claim.month_string;
                    oldclaim.transferedTo = claim.transferedTo;
                    oldclaim.Note = claim.Note;

                    //Fulldateofbill is redundant and incorrect
                    oldclaim.fullDateofbill = claim.fullDateofbill;
                    oldclaim.receivedBy = claim.receivedBy;
                    oldclaim.caption = claim.caption;

                    if (oldclaim.DateReceived != claim.DateReceived)
                    {
                        Entities.ClaimBatch oldbatch = _claimsvc.GetClaimBatch(oldclaim.ClaimBatch.Id);
                        //_claimsvc.GetBatchForProvider(claim.providerid, Convert.ToDateTime(claim.DateReceived));


                        Utility.ClaimBatch batche = Tools.CheckClaimBatch(Convert.ToDateTime(claim.DateReceived));
                        oldbatch.ProviderId = claim.providerid;
                        oldbatch.Batch = batche;
                        oldbatch.month = Convert.ToDateTime(claim.DateReceived).Month;
                        oldbatch.year = Convert.ToDateTime(claim.DateReceived).Year;



                        //oldclaim.ClaimBatch = claimbatcho;
                        //claimbatcho.IncomingClaims.Add(oldclaim);

                        bool Reply = _claimsvc.UpdateClaimBatch(oldbatch); ;//_claimsvc.ReceiveNewIncomingClaim(claim);


                        //_claimsvc.ReceiveNewIncomingClaim(claim);

                    }




                    if (oldclaim != null)
                    {


                        bool resp2 = _claimsvc.DeleteIncomingClaim(claim);

                        if (resp2)
                        {
                            _pageMessageSvc.SetSuccessMessage(string.Format("Claims have been updated successfully.The ID is : {0} Please write this on the bill for easy tracking. ", oldclaim.ClaimBatch.Id));

                            return _uniquePageService.RedirectTo<IncomingClaimsPage>();
                        }
                        else
                        {
                            _pageMessageSvc.SetErrormessage("There was a problem updating claims.");
                            return _uniquePageService.RedirectTo<IncomingClaimsPage>();
                        }
                    }
                }

                //i added this
                ViewBag.MyUserId = CurrentRequestData.CurrentUser.Id;
                Entities.ClaimBatch claimbatch = new Entities.ClaimBatch();


                ////check the batch for the said claim
                //var exist = false;
                //if (claimbatch != null)
                //{
                //    exist = claimbatch.IncomingClaims.Where(x => x.month == claim.month && x.year == claim.year).Any();

                //}


                //if (exist && string.IsNullOrEmpty(Idholder))
                //{
                //    var bill = claimbatch.IncomingClaims.Where(x => x.month == claim.month && x.year == claim.year).FirstOrDefault();

                //    _pageMessageSvc.SetErrormessage(string.Format("There is a mix up ,it seems like this bill have been received previously by {0} with Id {1} on {2}. Thanks", _userservice.GetUser(bill.receivedBy).Name, bill.Id, Convert.ToDateTime(bill.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern)));
                //    return _uniquePageService.RedirectTo<IncomingClaimsPage>();
                //}


                //Check the status of the batch

                //if (claimbatch != null && !(claimbatch.status == ClaimBatchStatus.Capturing || claimbatch.status == ClaimBatchStatus.Default))
                //{
                //    _pageMessageSvc.SetErrormessage("Kindly note that the batch for this period has closed for the selected provider as payment is being processed.Please select a different batch for this bills.Thank you");
                //    return _uniquePageService.RedirectTo<IncomingClaimsPage>();
                //}



                //create new batch for the provider

                Utility.ClaimBatch batch = Tools.CheckClaimBatch(Convert.ToDateTime(claim.DateReceived));
                claimbatch.ProviderId = claim.providerid;
                claimbatch.Batch = batch;
                claimbatch.month = Convert.ToDateTime(claim.DateReceived).Month;
                claimbatch.year = Convert.ToDateTime(claim.DateReceived).Year;
                claimbatch.status = ClaimBatchStatus.Default;






                claim.ClaimBatch = claimbatch;
                claimbatch.IncomingClaims.Add(claim);

                bool resp = _claimsvc.AddClaimBatch(claimbatch);//_claimsvc.ReceiveNewIncomingClaim(claim);


                _claimsvc.ReceiveNewIncomingClaim(claim);

                if (resp)
                {




                }
                else
                {
                    _pageMessageSvc.SetErrormessage("There was an error logging claims.");
                }

                _pageMessageSvc.SetSuccessMessage(string.Format("Claims have been logged successfully.The ID is : {0} Please write this on the bill for easy tracking. ", claimbatch.Id));
            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was an error logging claims.Kindly Check the data enter and try again.");
            }


            return _uniquePageService.RedirectTo<IncomingClaimsPage>();


        }

        [HttpGet]
        public int providerGetClaimBatch(int providerid, int month, int year, int claimcount)
        {
            //how do you solve the break intransmission. stuff

            //check if theres any bill with the month and year that didn't complete
            //exist = claimbatch.IncomingClaims.Where(x => x.month == claim.month && x.year == claim.year).Any();

            IncomingClaims exist = _claimsvc.getincomingClaimByMonthandYear(providerid, month, year);
            if (exist != null && exist.Id > 0)
            {
                exist.ClaimBatch.claimscountfromclient = exist.ClaimBatch.Claims.Count() + claimcount;
                _claimsvc.UpdateClaimBatch(exist.ClaimBatch);
                return exist.ClaimBatch.Id;

            }
            else
            {
                IncomingClaims claim = new IncomingClaims
                {
                    providerid = providerid,
                    month = month,
                    month_string = month.ToString(),
                    year = year,
                    deliveredby = "Delivered Remotely from portal.",
                    caption = "",
                    transferedTo = 1,

                    Note = string.Empty,
                    fullDateofbill = CurrentRequestData.Now,          //new DateTime(Convert.ToInt32(Year),Convert.ToInt32(Month_list),01,00,59,00,CurrentRequestData.CultureInfo.Calendar),
                    receivedBy = 1,
                    DateReceived = CurrentRequestData.Now,
                    status = ReceivedClaimStatus.Received,
                    IsRemoteSubmission = true,


                };

                Entities.ClaimBatch claimbatch = new Entities.ClaimBatch();
                Utility.ClaimBatch batch = Tools.CheckClaimBatch(Convert.ToDateTime(claim.DateReceived));
                claimbatch.ProviderId = claim.providerid;
                claimbatch.Batch = batch;
                claimbatch.month = Convert.ToDateTime(claim.DateReceived).Month;
                claimbatch.year = Convert.ToDateTime(claim.DateReceived).Year;
                claimbatch.claimscountfromclient = claimcount;
                claimbatch.isremote = true;

                claimbatch.status = ClaimBatchStatus.Capturing;

                claim.ClaimBatch = claimbatch;
                claimbatch.IncomingClaims.Add(claim);

                bool resp = _claimsvc.AddClaimBatch(claimbatch);//_claimsvc.ReceiveNewIncomingClaim(claim);
                _claimsvc.ReceiveNewIncomingClaim(claim);
                return claimbatch.Id;
            }






        }

        [HttpGet]
        public JsonResult CloseClaimBatch(int batchid)
        {
            Entities.ClaimBatch Cbatch = _claimsvc.GetClaimBatch(batchid);
            ClaimBatchCloseResponse resp = new ClaimBatchCloseResponse();
            resp.code = 0;

            if (Cbatch != null)
            {
                //change cbatch to submitted to vetting 


                Cbatch.status = ClaimBatchStatus.Vetting;
                Cbatch.submitedVetbyUser = CurrentRequestData.CurrentUser != null ? CurrentRequestData.CurrentUser.Id : 1;
                Cbatch.VetDate = CurrentRequestData.Now;


                _claimsvc.UpdateClaimBatch(Cbatch);

                decimal totalservice = 0m;
                decimal totaldrug = 0m;
                decimal totalSum = 0m;
                int counttt = 0;
                foreach (Claim item in Cbatch.Claims)
                {
                    totalservice = Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount));
                    totaldrug = Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount));
                    counttt++;
                    totalSum = +totaldrug + totalservice;
                }



                //log email
                Provider provider = _providerSvc.GetProvider(Cbatch.ProviderId);
                string thename = provider != null ? provider.Name.ToUpper() : "--";
                string theaddress = provider != null ? provider.Address.ToLower() : "--";
                IncomingClaims areply = Cbatch.IncomingClaims.FirstOrDefault();
                string providerEmail = provider.providerlogin.email;


                string themonth = "--";
                if (!string.IsNullOrEmpty(areply.month_string) && areply.month_string.Split(',').Count() > 0)
                {
                    foreach (string itemmm in areply.month_string.Split(','))
                    {
                        themonth = themonth + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(itemmm)) + ",";
                    }
                    themonth = themonth + areply.year.ToString();


                }
                //_helperSvc.PushUserNotification 
                QueuedMessage emailmsg = new QueuedMessage();
                emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                emailmsg.ToAddress = "petera@novohealthafrica.org";
                emailmsg.Cc = "anthonya@novohealthafrica.org";
                emailmsg.Subject = "Provider Claim Submission ";
                emailmsg.FromName = "NOVOHUB";
                emailmsg.Body = string.Format("{0} - {1} just sent in their {2} claim ,with batch id : {3} .kindly attend to it. Thank you", thename, theaddress, themonth, Cbatch.Id);

                _emailSender.AddToQueue(emailmsg);

                if (!string.IsNullOrEmpty(providerEmail))
                {
                    QueuedMessage emailmsg2 = new QueuedMessage();
                    emailmsg2.FromAddress = _mailSettings.SystemEmailAddress;
                    emailmsg2.ToAddress = provider.providerlogin.email;
                    if (!string.IsNullOrEmpty(provider.providerlogin.Altemail))
                    {
                        emailmsg2.Cc = provider.providerlogin.Altemail;

                    }
                    emailmsg2.Subject = "Claim Submission ";
                    emailmsg2.FromName = "NOVOHUB";
                    emailmsg2.Body = string.Format("Dear Provider,You have successfully submitted your {2} claim ,with batch id : {3} .Your claim is being processed. Thank you", thename, theaddress, themonth, Cbatch.Id);

                    _emailSender.AddToQueue(emailmsg2);
                }

                resp.code = 1;
                resp.count = counttt;
                resp.total = totalSum.ToString("N");





            }

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetIncomingClaimsJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);

            string scrProvider = CurrentRequestData.CurrentContext.Request["scr_provider"];
            string addedBy = CurrentRequestData.CurrentContext.Request["addedBy"];

            string scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            string scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            string scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];
            string scrDeliveredBy = CurrentRequestData.CurrentContext.Request["scrDeliveredBy"];
            string month = CurrentRequestData.CurrentContext.Request["month"];
            string year = CurrentRequestData.CurrentContext.Request["year"];
            string transferedTo = CurrentRequestData.CurrentContext.Request["transferedTo"];
            string trackingidstrg = CurrentRequestData.CurrentContext.Request["trackingid"];
            DateTime fromdate = CurrentRequestData.Now;
            DateTime todate = CurrentRequestData.Now;
            bool usedate = false;
            if (!string.IsNullOrEmpty(scrFromDate) && !string.IsNullOrEmpty(scrToDate))
            {
                fromdate = Convert.ToDateTime(scrFromDate);
                todate = Convert.ToDateTime(scrToDate);
                usedate = Convert.ToBoolean(scruseDate);
            }
            int trackingid = -1;

            if (!string.IsNullOrEmpty(trackingidstrg) && int.TryParse(trackingidstrg, out trackingid))
            {

            }


            if (string.IsNullOrEmpty(scrProvider))
            {
                scrProvider = "-1";
            }

            int toltareccount = 0;
            int totalinresult = 0;
            IOrderedEnumerable<IncomingClaims> allincomingClaims = _claimsvc.QueryAllIncomingClaims(out toltareccount, out totalinresult, string.Empty,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, Convert.ToInt32(scrProvider), Convert.ToInt32(addedBy),
                                                                 scrDeliveredBy, Convert.ToInt32(month), Convert.ToInt32(year), Convert.ToInt32(transferedTo), usedate, fromdate, todate, 0, trackingid).OrderByDescending(x => x.DateReceived);


            List<IncomingClaimsResponse> output = new List<IncomingClaimsResponse>();
            DateTime today = CurrentRequestData.Now;
            foreach (IncomingClaims areply in allincomingClaims)
            {

                Provider provider = _providerSvc.GetProvider(areply.providerid);
                MrCMS.Entities.People.User user = _userservice.GetUser(areply.receivedBy);
                MrCMS.Entities.People.User user2 = _userservice.GetUser(areply.transferedTo);
                IncomingClaimsResponse model = new IncomingClaimsResponse();
                model.Id = areply.Id.ToString();
                model.GroupName = provider != null ? _helperSvc.GetzonebyId(Convert.ToInt32(provider.State.Zone)).Name : "--";
                model.Provider = provider != null ? provider.Name : "--";
                model.ClaimsPeriod = Convert.ToDateTime(areply.fullDateofbill).ToString("MMM yyyy");
                model.DeliveredBy = areply.deliveredby != null ? areply.deliveredby.ToUpper() : "--";
                model.ReceivedBy = user != null ? user.Name : "--";
                model.Caption = !string.IsNullOrEmpty(areply.caption) ? areply.caption.ToUpper() : "--";
                model.TransferedTo = user2 != null ? user2.Name : "--";
                model.trackingID = areply.ClaimBatch.Id.ToString();
                model.NoOfEncounter = areply.noofencounter.ToString();
                model.TotalAmount = "₦ " + Convert.ToDecimal(areply.totalamount).ToString("N");
                model.Note = string.IsNullOrEmpty(areply.Note) ? "--" : areply.Note;
                model.DateReceived = Convert.ToDateTime(areply.DateReceived).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                model.DateLogged = Convert.ToDateTime(areply.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);

                if (!string.IsNullOrEmpty(areply.month_string) && areply.month_string.Split(',').Count() > 0)
                {
                    foreach (string itemmm in areply.month_string.Split(','))
                    {
                        model.month_string = model.month_string + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(itemmm)) + ",";
                    }
                    model.month_string = model.month_string + areply.year.ToString();
                }



                output.Add(model);
            }


            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
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
        [HttpGet]
        public int Requestforauthcode(int providerid, string policynumber, string fullname, string companyname, string diagnosis, string reason)
        {
            if (!string.IsNullOrEmpty(fullname) && !string.IsNullOrEmpty(companyname) && !string.IsNullOrEmpty(diagnosis))
            {

                Provider provider = _providerSvc.GetProvider(providerid);

                if (provider != null)
                {


                    AuthorizationRequest authrequest = new AuthorizationRequest();
                    authrequest.providerid = provider.Id;
                    authrequest.policynumber = policynumber;
                    authrequest.fullname = fullname;
                    authrequest.company = companyname;
                    authrequest.providerName = provider.Name;
                    authrequest.diagnosis = diagnosis;
                    authrequest.reasonforcode = reason;
                    authrequest.isnew = true;
                    _claimsvc.addAuthRequest(authrequest);

                    QueuedMessage emailmsg2 = new QueuedMessage();
                    emailmsg2.FromAddress = _mailSettings.SystemEmailAddress;
                    emailmsg2.ToAddress = "callcentre@novohealthafrica.org";
                    // emailmsg2.Bcc = "anthonya@novohealthafrica.org";

                    emailmsg2.Subject = "Authorization Code Request";
                    emailmsg2.FromName = provider.Name.ToUpper();
                    emailmsg2.Body = string.Format("Dear Sir/Ma,{0} {1} have requested for authorization code for {2} ({3}) a staff of {4}. The diagnosis is {5} and the reason for the code is {6} .Thank You", provider.Name.ToUpper(), provider.Address.ToLower(), fullname, policynumber, companyname, diagnosis, reason);
                    _emailSender.AddToQueue(emailmsg2);



                }
                return 1;



            }
            return 0;
        }


        public JsonResult GetincomingClaimforEdit(int id)
        {
            IncomingClaims claim = _claimsvc.GetIncomingClaim(id);

            if (claim != null)
            {
                var model = new
                {
                    Id = claim.Id,
                    provider = claim.providerid,
                    month = claim.month,
                    year = claim.year,
                    caption = claim.caption,
                    month_string = claim.month_string,
                    deliveredby = claim.deliveredby,
                    transferedto = claim.transferedTo,
                    datereceived = Convert.ToDateTime(claim.DateReceived).ToString("dd/MM/yyyy"),
                    note = claim.Note,

                };
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetClaimBatchJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);

            string scrProvider = CurrentRequestData.CurrentContext.Request["Provider_list"];

            if (string.IsNullOrEmpty(scrProvider))
            {
                scrProvider = "-1";
            }
            string month = CurrentRequestData.CurrentContext.Request["Month_list"];
            string year = CurrentRequestData.CurrentContext.Request["year"];
            string batch = CurrentRequestData.CurrentContext.Request["Batch"];

            string zone = CurrentRequestData.CurrentContext.Request["Zone"];
            string claimbatchidd = CurrentRequestData.CurrentContext.Request["claimbatchidd"];
            int clambatchiddd = 0;
            int.TryParse(claimbatchidd, out clambatchiddd);
            int providercount = 0;
            decimal totalamount = 0m;
            decimal totalprocessed = 0m;
            int toltareccount = 0;
            int totalinresult = 0;
            IList<Entities.ClaimBatch> allincomingClaims = _claimsvc.QueryAllClaimBatch(out toltareccount, out totalinresult, string.Empty,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, Convert.ToInt32(scrProvider), Convert.ToInt32(month), Convert.ToInt32(year), (Utility.ClaimBatch)Enum.Parse(typeof(Utility.ClaimBatch), batch), zone, 0, ClaimBatchStatus.Default, out providercount, out totalamount, out totalprocessed, -1, clambatchiddd);

            //var claimm = _claimsvc.GetClaimBatch(1);
            List<ClaimsBatchResponse> output = new List<ClaimsBatchResponse>();
            DateTime today = CurrentRequestData.Now;
            foreach (Entities.ClaimBatch areply in allincomingClaims)
            {

                Provider provider = _providerSvc.GetProvider(areply.ProviderId);

                ClaimsBatchResponse model = new ClaimsBatchResponse();
                model.Id = areply.Id;
                model.GroupName = provider != null ? _helperSvc.GetzonebyId(Convert.ToInt32(provider.State.Zone)).Name : "--";
                model.Provider = provider != null ? provider.Name : "--";
                model.PRoviderAddress = provider != null ? provider.Address : "--";
                model.Batch = areply.Batch == Utility.ClaimBatch.BatchA ? "Batch A" : "Batch B";
                model.deliveryCount = areply.IncomingClaims.Where(x => x.IsDeleted == false).ToList().Count.ToString();
                model.claimscount = areply.Claims.Count.ToString();
                model.totalAmount = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))).ToString("N");
                IncomingClaims income = areply.IncomingClaims.FirstOrDefault();
                if (income != null)
                {

                    model.Caption = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().caption) ? areply.IncomingClaims.FirstOrDefault().caption : "--";
                    model.Note = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().Note) ? areply.IncomingClaims.FirstOrDefault().Note : "--";
                    model.isSubmittedRemotely = areply.IncomingClaims.FirstOrDefault().IsRemoteSubmission;
                    model.deliverydate = Convert.ToDateTime(income.DateReceived).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                    string monthd = "";
                    if (!string.IsNullOrEmpty(income.month_string) && income.month_string.Split(',').Count() > 0)
                    {
                        foreach (string itemmm in income.month_string.Split(','))
                        {
                            model.month_string = model.month_string + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(itemmm)) + ",";
                        }
                        model.month_string = model.month_string + income.year.ToString();


                    }
                }
                string narration = "----";
                try
                {
                    narration = Tools.GetClaimsNarrations(areply);
                }
                catch (Exception)
                {

                }

                model.narration = narration;

                output.Add(model);
            }

            output = output.OrderBy(x => x.GroupName).ToList();
            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
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
        public JsonResult GetAllClaimBatchJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);

            string scrProvider = CurrentRequestData.CurrentContext.Request["Provider_list"];
            string claimbatchidd = CurrentRequestData.CurrentContext.Request["claimbatchidd"];
            int clambatchiddd = 0;
            int.TryParse(claimbatchidd, out clambatchiddd);
            if (string.IsNullOrEmpty(scrProvider))
            {
                scrProvider = "-1";
            }
            string month = CurrentRequestData.CurrentContext.Request["Month_list"];
            string year = CurrentRequestData.CurrentContext.Request["year"];
            string batch = CurrentRequestData.CurrentContext.Request["Batch"];

            string zone = CurrentRequestData.CurrentContext.Request["Zone"];

            if (string.IsNullOrEmpty(month))
            {
                month = "-1";
            }
            if (string.IsNullOrEmpty(year))
            {
                year = "-1";
            }

            if (string.IsNullOrEmpty(batch))
            {
                batch = "-1";
            }
            int providercount = 0;
            decimal totalamount = 0m;
            decimal totalprocessed = 0m;
            int toltareccount = 0;
            int totalinresult = 0;
            IList<Entities.ClaimBatch> allincomingClaims = _claimsvc.QueryAllClaimBatch(out toltareccount, out totalinresult, string.Empty,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, Convert.ToInt32(scrProvider), Convert.ToInt32(month), Convert.ToInt32(year), (Utility.ClaimBatch)Enum.Parse(typeof(Utility.ClaimBatch), batch), zone, 0, ClaimBatchStatus.All, out providercount, out totalamount, out totalprocessed, -1, clambatchiddd);

            //var claimm = _claimsvc.GetClaimBatch(1);
            List<ClaimsBatchResponse> output = new List<ClaimsBatchResponse>();
            DateTime today = CurrentRequestData.Now;
            foreach (Entities.ClaimBatch areply in allincomingClaims)
            {

                Provider provider = _providerSvc.GetProvider(areply.ProviderId);

                ClaimsBatchResponse model = new ClaimsBatchResponse();
                model.Id = areply.Id;
                model.GroupName = provider != null ? _helperSvc.GetzonebyId(Convert.ToInt32(provider.State.Zone)).Name : "--";
                model.Provider = provider != null ? provider.Name : "--";
                model.PRoviderAddress = provider != null ? provider.Address : "--";
                model.ClaimStatus = Enum.GetName(typeof(ClaimBatchStatus), areply.status);
                model.Batch = areply.Batch == Utility.ClaimBatch.BatchA ? "Batch A" : "Batch B";
                model.deliveryCount = areply.IncomingClaims.Where(x => x.IsDeleted == false).ToList().Count.ToString();
                model.claimscount = areply.Claims.Count.ToString();
                model.totalAmount = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))).ToString("N");
                model.totalProccessed = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount))).ToString("N");
                model.difference = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount))) > 0 ? Convert.ToDecimal(Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))) - Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount)))).ToString("N") : "0.00";

                IncomingClaims income = areply.IncomingClaims.FirstOrDefault();
                if (income != null)
                {

                    model.Caption = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().caption) ? areply.IncomingClaims.FirstOrDefault().caption : "--";
                    model.Note = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().Note) ? areply.IncomingClaims.FirstOrDefault().Note : "--";
                    model.isSubmittedRemotely = areply.IncomingClaims.FirstOrDefault().IsRemoteSubmission;
                    model.deliverydate = Convert.ToDateTime(income.DateReceived).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                    string monthd = "";
                    if (!string.IsNullOrEmpty(income.month_string) && income.month_string.Split(',').Count() > 0)
                    {
                        foreach (string itemmm in income.month_string.Split(','))
                        {
                            model.month_string = model.month_string + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(itemmm)) + ",";
                        }
                        model.month_string = model.month_string + income.year.ToString();


                    }
                }
                string narration = "----";
                try
                {
                    narration = Tools.GetClaimsNarrations(areply);
                }
                catch (Exception)
                {

                }

                model.narration = narration;

                output.Add(model);
            }

            output = output.OrderBy(x => x.GroupName).ToList();
            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
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

        public JsonResult GetClaimBatchVetJson()
        {
            //added Bill State
            ViewBag.BillState = MrCMS.Web.Apps.Core.Utility.ClaimsBillStatus.Vetted;
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);

            string scrProvider = CurrentRequestData.CurrentContext.Request["Provider_list"];
            string month = CurrentRequestData.CurrentContext.Request["Month_list"];
            string year = CurrentRequestData.CurrentContext.Request["year"];
            string batch = CurrentRequestData.CurrentContext.Request["Batch"];

            string zone = CurrentRequestData.CurrentContext.Request["Zone"];
            string channel = CurrentRequestData.CurrentContext.Request["channel"];
            string claimbatchidd = CurrentRequestData.CurrentContext.Request["claimbatchidd"];
            int clambatchiddd = 0;
            int.TryParse(claimbatchidd, out clambatchiddd);

            int toltareccount = 0;
            int channelint = 0;

            int.TryParse(channel, out channelint);


            int totalinresult = 0;
            int providercount = 0;
            decimal totalamount = 0m;
            decimal totalprocessed = 0m;
            IList<Entities.ClaimBatch> allincomingClaims = _claimsvc.QueryAllClaimBatch(out toltareccount, out totalinresult, string.Empty,
                                                                 int.Parse(displayStart),
                                                                 int.Parse(displayLength), sortColumnnumber, sortOrder, int.Parse(scrProvider), int.Parse(month), int.Parse(year), (Utility.ClaimBatch)Enum.Parse(typeof(Utility.ClaimBatch), batch), zone, 0, ClaimBatchStatus.Vetting, out providercount, out totalamount, out totalprocessed, channelint, clambatchiddd);

            Entities.ClaimBatch claimm = _claimsvc.GetClaimBatch(1);
            List<ClaimsBatchResponse> output = new List<ClaimsBatchResponse>();
            DateTime today = CurrentRequestData.Now;

            foreach (Entities.ClaimBatch areply in allincomingClaims)
            {

                Provider provider = _providerSvc.GetProvider(areply.ProviderId);

                ClaimsBatchResponse model = new ClaimsBatchResponse();
                model.Id = areply.Id;
                model.GroupName = provider != null ? _helperSvc.GetzonebyId(Convert.ToInt32(provider.State.Zone)).Name : "--";
                model.Provider = provider != null ? provider.Name : "--";
                model.PRoviderAddress = provider != null ? provider.Address : "--";
                model.Batch = areply.Batch == Utility.ClaimBatch.BatchA ? "Batch A" : "Batch B";
                model.deliveryCount = areply.IncomingClaims.Count.ToString();
                model.claimscount = areply.Claims.Count.ToString();
                model.totalAmount = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))).ToString("N");
                model.totalProccessed = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount))).ToString("N");
                model.difference = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount))) > 0 ? Convert.ToDecimal(Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))) - Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount)))).ToString("N") : "0.00";
                model.CapturedBy = areply.submitedVetbyUser > 0 ? _userservice.GetUser(areply.submitedVetbyUser).Name : "--";

                model.DateSubmitedForVetting = Convert.ToDateTime(areply.VetDate).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                IncomingClaims income = areply.IncomingClaims.FirstOrDefault();
                if (income != null)
                {

                    model.Caption = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().caption) ? areply.IncomingClaims.FirstOrDefault().caption : "--";
                    model.Note = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().Note) ? areply.IncomingClaims.FirstOrDefault().Note : "--";
                    model.isSubmittedRemotely = areply.IncomingClaims.FirstOrDefault().IsRemoteSubmission;
                    model.deliverydate = Convert.ToDateTime(income.DateReceived).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                    string monthd = "";
                    if (!string.IsNullOrEmpty(income.month_string) && income.month_string.Split(',').Count() > 0)
                    {
                        foreach (string itemmm in income.month_string.Split(','))
                        {
                            model.month_string = model.month_string + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(itemmm)) + ",";
                        }
                        model.month_string = model.month_string + income.year.ToString();


                    }
                }
                output.Add(model);
            }
            output = output.OrderBy(x => x.GroupName).ToList();

            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
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
        [HttpGet]
        public ActionResult DeleteIncomingClaim(int id)
        {
            IncomingClaims item = _claimsvc.GetIncomingClaim(id);
            Provider provider = _providerSvc.GetProvider(item.providerid);
            if (item != null)
            {
                ViewBag.ProviderName = provider != null ? provider.Name.ToUpper() : "--";
                ViewBag.claimperiod = Convert.ToDateTime(item.fullDateofbill).ToString("MMM yyyy");

            }
            return PartialView("DeleteIncomingClaim", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteIncomingClaim(IncomingClaims Claim)
        {
            if (Claim.Id > 0)
            {
                bool resp = false;
                //new gig this shit deletes the claimbatch

                //check the claim bill count

                if (Claim.ClaimBatch.Claims.Count < 1)
                {

                    _claimsvc.DeleteClaimBatch(Claim.ClaimBatch);
                    resp = _claimsvc.DeleteIncomingClaim(Claim);
                }


                if (resp)
                {
                    _pageMessageSvc.SetSuccessMessage("The Claim was successfully deleted from the system.");
                }
                else
                {
                    _pageMessageSvc.SetErrormessage("There was an error deleting the Claim.It seems like the capturers have started working on the bill.Contact Admin");
                }

            }
            return _uniquePageService.RedirectTo<IncomingClaimsPage>();
        }

        [ActionName("CaptureClaimList")]
        public ActionResult CaptureClaimList(CaptureClaimsPage Page)
        {
            int year = CurrentRequestData.Now.Year;
            List<GenericReponse2> yealist = new List<GenericReponse2>();
            for (int i = 0; i < 20; i++)
            {
                yealist.Add(new GenericReponse2 { Id = year - i, Name = (year - i).ToString() });
            }
            ViewBag.YearList = yealist;
            List<GenericReponse2> plist = _providerSvc.GetProviderNameList().OrderBy(x => x.Name).ToList();
            ViewBag.MyProvidersList = _providerSvc.GetProviderNameList().OrderBy(x => x.Name);
            plist.Insert(0, new GenericReponse2 { Id = -1, Name = "All Providers" });
            ViewBag.PrvidersList = plist;
            IList<MrCMS.Entities.People.User> users = _chatservice.Getallusers();

            var userlist = from aereply in users
                           select new
                           {
                               Id = aereply.Id,
                               Name = aereply.Name,
                           };

            var userr = userlist.ToList();

            userr.Add(new { Id = -1, Name = "Select" });


            ViewBag.UserList = userr.OrderBy(x => x.Id).ToList();
            List<GenericReponse> batchlist = new List<GenericReponse>();
            foreach (string item in Enum.GetNames(typeof(Utility.ClaimBatch)))
            {
                batchlist.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(Utility.ClaimBatch), item)).ToString(), Name = item.ToUpper() });
            }
            ViewBag.BatchList = batchlist;
            ViewBag.Defaultdate = CurrentRequestData.Now.ToString("MM/dd/yyyy");
            IEnumerable<Zone> zones = _helperSvc.GetallZones();
            List<GenericReponse2> zonelist = new List<GenericReponse2>();

            foreach (Zone item in zones)
            {
                GenericReponse2 shii = new GenericReponse2()
                {
                    Id = item.Id,
                    Name = item.Name
                };
                zonelist.Add(shii);


            }

            zonelist.Insert(0, new GenericReponse2() { Id = -1, Name = "All Zones" });
            ViewBag.ZoneList = zonelist;
            return View(Page);
        }

        public JsonResult PostClaimdata()
        {
            string datastr = CurrentRequestData.CurrentContext.Request["dataStr"];
            return null;
        }

        [ActionName("CaptureClaimForm")]
        public ActionResult CaptureClaimt(int Id, CaptureClaimsFormPage Page)
        {

            Entities.ClaimBatch claimbatch = _claimsvc.GetClaimBatch(Id);

            if (claimbatch == null)
            {
                _pageMessageSvc.SetErrormessage("The Claim Batch selected does not exist.");
                return Redirect(string.Format(_uniquePageService.GetUniquePage<CaptureClaimsFormPage>().AbsoluteUrl + "?id={0}",
                                    Id));
            }

            Provider provider = _providerSvc.GetProvider(claimbatch.ProviderId);
            ViewBag.HasTariff = provider != null && provider.ProviderTariffs != null && !string.IsNullOrEmpty(provider.ProviderTariffs);

            ViewBag.Batchh = claimbatch.Batch.ToString().ToUpper();
            ViewBag.hospital = provider.Name.ToUpper();
            ViewBag.TotalCaptured = claimbatch.Claims.Count.ToString();
            ViewBag.TotalAmount = Convert.ToDecimal((claimbatch.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + claimbatch.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount)))).ToString("N");

            ViewBag.TotalAmountdigit = Convert.ToDecimal((claimbatch.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + claimbatch.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))));
            ViewBag.MonthYear = CurrentRequestData.CultureInfo.DateTimeFormat.GetMonthName(claimbatch.month).ToString() + " " + claimbatch.year;

            int tariffID = -1;

            if (!string.IsNullOrEmpty(provider.ProviderTariffs))
            {
                if (!int.TryParse(provider.ProviderTariffs.Split(',')[0], out tariffID))
                {
                    tariffID = -1;
                }

            }
            ViewBag.HasTariff = false;

            if (tariffID > 0)
            {
                ViewBag.HasTariff = true;
            }



            string[] tags = Enum.GetNames(typeof(ClaimsTAGS));
            List<GenericReponse> outtags = new List<GenericReponse>();
            foreach (string tag in tags)
            {
                outtags.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(ClaimsTAGS), tag)).ToString(), Name = tag.ToUpper() });

            }

            outtags.Insert(0, new GenericReponse() { Id = "-1", Name = "--Select Tag--" });


            ViewBag.treatmentTag = outtags;
            ViewBag.ProviderId = provider.Id;
            ViewBag.ClaimBatchId = Id;

            //tariff

            //var items = _claimsvc.GetServiceTariff(tariffID, string.Empty).OrderBy(x => x.Name);
            //var items2= _claimsvc.GetDrugTariff(tariffID, string.Empty).OrderBy(x => x.Name);
            ViewBag.ServiceShii = ""; ;//HttpUtility.HtmlDecode(tariff);
            ViewBag.DrugShii = "";


            return View(Page);
        }


        public JsonResult GetServiceTariffJson(int id, string phrase)
        {
            Provider provider = _providerSvc.GetProvider(id);
            int tariffID = string.IsNullOrEmpty(provider.ProviderTariffs) ? 0 : Convert.ToInt32(provider.ProviderTariffs.Split(',')[0]);

            IOrderedEnumerable<TariffGenericReponse> items = _claimsvc.GetServiceTariff(tariffID, phrase).OrderBy(x => x.Name);

            return Json(items, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetDrugTariffJson(int id, string phrase)
        {
            Provider provider = _providerSvc.GetProvider(id);
            int tariffID = string.IsNullOrEmpty(provider.ProviderTariffs) ? 0 : Convert.ToInt32(provider.ProviderTariffs.Split(',')[0]);

            IOrderedEnumerable<TariffGenericReponse> items = _claimsvc.GetDrugTariff(tariffID, phrase).OrderBy(x => x.Name);

            return Json(items, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitClaimsForm(FormCollection claimsform, CaptureClaimsFormPage page)
        {
            string FormNo = claimsform["FormNo"];
            string policynumber = claimsform["policynumber"];
            string evscode = claimsform["evscode"];
            string enrolleename = claimsform["enrolleename"];
            string companyname = claimsform["companyname"];
            string companyid = claimsform["CompanyID"];

            string enrolleesex = claimsform["enrolleesex"];
            string providerename = claimsform["providerename"];
            string doctorname = claimsform["doctorname"];
            string doctorID = claimsform["Idnumber"];
            string areaofspecialization = claimsform["areaofspecialization"];
            string Idnumber = claimsform["Idnumber"];
            string doctorsigned = claimsform["doctorsigned"];
            string doctorssigndate = claimsform["doctorssigndate"];
            string specialistname = claimsform["specialistname"];
            string areaofspecializationSpecialist = claimsform["areaofspecializationSpecialist"];

            string specialistphonenumber = claimsform["specialistphonenumber"];
            string specialistsigned = claimsform["specialistsigned"];
            string specialistsigndate = claimsform["specialistsigndate"];
            string dateofservice = claimsform["dateofservice"];
            string durationoftreatment = claimsform["durationoftreatment"];
            string dateofadmission = claimsform["dateofadmission"];
            string dateofdischarge = claimsform["dateofdischarge"];
            string diagnosis = claimsform["diagnosis"];
            string Treatmentgiven = claimsform["Treatmentgiven"];
            string treatmentcode = claimsform["treatmentcode"];
            string referralcode = claimsform["referralcode"];

            string servicebillCount = claimsform["servicebillCount"];
            string drugbillCount = claimsform["drugbillCount"];

            string Enrolleesigned = claimsform["Enrolleesigned"];
            string EnrolleesignedDate = claimsform["EnrolleesignedDate"];

            string AllprescibedDrugs = claimsform["AllprescibedDrugs"];
            string LaboratoryInvestigation = claimsform["LaboratoryInvestigation"];
            string Admission = claimsform["Admission"];
            string Feeding = claimsform["Feeding"];
            string enrolleeage = claimsform["enrolleeage"];
            string Remark = claimsform["Remark"];

            string providerID = claimsform["providerID"];
            string enrolleeID = claimsform["enrolleeID"];
            string ClaimsBatchid = claimsform["claimBatchID"];
            string treatmentTag = claimsform["treatmentTag"];

            //Convert the form
            List<Entities.ClaimService> ServiceList = new List<Entities.ClaimService>();
            List<ClaimDrug> drugList = new List<ClaimDrug>();

            int servicecount = string.IsNullOrEmpty(servicebillCount) ? 0 : Convert.ToInt32(servicebillCount.Split(',')[0]);
            int drugcount = string.IsNullOrEmpty(drugbillCount) ? 0 : Convert.ToInt32(drugbillCount.Split(',')[0]);
            Entities.ClaimBatch claimsbatch = _claimsvc.GetClaimBatch(Convert.ToInt32(ClaimsBatchid));


            if (servicecount == 0 && drugcount == 0)
            {
                _pageMessageSvc.SetErrormessage("The Claim has no bill in it.");
                return Redirect(string.Format(_uniquePageService.GetUniquePage<CaptureClaimsFormPage>().AbsoluteUrl + "?id={0}",
                                   Convert.ToInt32(ClaimsBatchid)));

            }
            Claim claimsobj = new Claim();
            //do your validation
            for (int i = 0; i <= servicecount; i++)
            {
                Entities.ClaimService servicee = new Entities.ClaimService();
                string itemDscr = claimsform[string.Format("servicebill{0}{1}", i, "ItemDescription")];
                string itemDuration = claimsform[string.Format("servicebill{0}{1}", i, "Duration")];
                string itemRate = claimsform[string.Format("servicebill{0}{1}", i, "Rate")];
                string itemAmount = claimsform[string.Format("servicebill{0}{1}", i, "amount")];
                string ItemId = claimsform[string.Format("servicebill{0}{1}", i, "itemID")];
                if (!string.IsNullOrEmpty(itemDscr) && !string.IsNullOrEmpty(itemDuration) && !string.IsNullOrEmpty(itemRate) && !string.IsNullOrEmpty(itemAmount))
                {
                    decimal amount = 0m;
                    int Srvid = 0;
                    int.TryParse(ItemId, out Srvid);

                    decimal.TryParse(itemAmount, out amount);
                    servicee.ServiceName = itemDscr;
                    servicee.ServiceDescription = itemDscr;
                    servicee.Duration = itemDuration;
                    servicee.rate = itemRate;
                    servicee.InitialAmount = amount;
                    servicee.ServiceId = Srvid;
                    servicee.Claim = claimsobj;
                    ServiceList.Add(servicee);

                }

            }

            //for drugs
            for (int i = 0; i <= drugcount; i++)
            {
                ClaimDrug drugg = new ClaimDrug();
                string itemDscr = claimsform[string.Format("drugbill{0}{1}", i, "ItemDescription")];
                string itemDuration = claimsform[string.Format("drugbill{0}{1}", i, "Unit")];
                string itemRate = claimsform[string.Format("drugbill{0}{1}", i, "Rate")];
                string itemAmount = claimsform[string.Format("drugbill{0}{1}", i, "amount")];
                string ItemId = claimsform[string.Format("drugbill{0}{1}", i, "itemID")];
                if (!string.IsNullOrEmpty(itemDscr) && !string.IsNullOrEmpty(itemDuration) && !string.IsNullOrEmpty(itemRate) && !string.IsNullOrEmpty(itemAmount))
                {
                    int drugid = 0;
                    decimal amount = 0m;
                    decimal.TryParse(itemAmount, out amount);
                    int.TryParse(ItemId, out drugid);
                    drugg.DrugName = itemDscr;
                    drugg.DrugDescription = itemDscr;
                    drugg.Quantity = itemDuration;
                    drugg.rate = itemRate;
                    drugg.InitialAmount = amount;
                    drugg.DrugId = drugid;
                    drugg.Claim = claimsobj;
                    drugList.Add(drugg);


                }


            }


            claimsobj.ProviderId = Convert.ToInt32(providerID);



            int enrolleeid = 0;
            int CompanyID = 0;

            int.TryParse(enrolleeID, out enrolleeid);
            int.TryParse(companyid, out CompanyID);
            claimsobj.Enrolleeid = enrolleeid;

            ViewBag.MyUser = CurrentRequestData.CurrentUser.Id;
            claimsobj.enrolleeage = enrolleeage;
            claimsobj.enrolleeFullname = enrolleename.ToUpper();
            claimsobj.enrolleeGender = enrolleesex;
            claimsobj.enrolleeCompanyName = companyname;
            claimsobj.enrolleeCompanyId = CompanyID;
            claimsobj.enrolleePolicyNumber = policynumber;
            claimsobj.ClaimsSerialNo = FormNo;
            claimsobj.EVSCode = evscode;
            claimsobj.DoctorsName = doctorname;
            claimsobj.DoctorsId = doctorID;
            claimsobj.AreaOfSpecialty = areaofspecialization;
            claimsobj.Signature = string.Empty;
            claimsobj.DoctorSigned = doctorsigned == null || doctorsigned.ToLower() != "on" ? false : true;
            claimsobj.DoctorSignecDate = !string.IsNullOrEmpty(doctorssigndate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(doctorssigndate.Substring(6, 4)), Convert.ToInt32(doctorssigndate.Substring(3, 2)), Convert.ToInt32(doctorssigndate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.specialistName = specialistname;
            claimsobj.specialistphonenumber = specialistphonenumber;
            claimsobj.specialistSigned = specialistsigned == null || specialistsigned.ToLower() != "on" ? false : true;
            claimsobj.specialistSignecDate = !string.IsNullOrEmpty(specialistsigndate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(specialistsigndate.Substring(6, 4)), Convert.ToInt32(specialistsigndate.Substring(3, 2)), Convert.ToInt32(specialistsigndate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.AreaOfSpecialtyforspecialist = areaofspecializationSpecialist;
            claimsobj.ServiceDate = !string.IsNullOrEmpty(dateofservice) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofservice.Substring(6, 4)), Convert.ToInt32(dateofservice.Substring(3, 2)), Convert.ToInt32(dateofservice.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.AdmissionDate = !string.IsNullOrEmpty(dateofadmission) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofadmission.Substring(6, 4)), Convert.ToInt32(dateofadmission.Substring(3, 2)), Convert.ToInt32(dateofadmission.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.DischargeDate = !string.IsNullOrEmpty(dateofdischarge) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofdischarge.Substring(6, 4)), Convert.ToInt32(dateofdischarge.Substring(3, 2)), Convert.ToInt32(dateofdischarge.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.Durationoftreatment = durationoftreatment;
            claimsobj.Diagnosis = diagnosis;
            claimsobj.TreatmentGiven = Treatmentgiven;
            claimsobj.TreatmentCode = treatmentcode;
            claimsobj.referalCode = referralcode;
            claimsobj.DrugList = drugList;
            claimsobj.ServiceList = ServiceList;
            claimsobj.enrolleeSigned = Enrolleesigned == null || Enrolleesigned.ToLower() != "on" ? false : true;
            claimsobj.EnrolleeSignDate = !string.IsNullOrEmpty(EnrolleesignedDate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(EnrolleesignedDate.Substring(6, 4)), Convert.ToInt32(EnrolleesignedDate.Substring(3, 2)), Convert.ToInt32(EnrolleesignedDate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.AllprescibedDrugs = AllprescibedDrugs == null || AllprescibedDrugs.ToLower() != "on" ? false : true;
            claimsobj.LaboratoryInvestigation = LaboratoryInvestigation == null || LaboratoryInvestigation.ToLower() != "on" ? false : true;
            claimsobj.Feeding = Feeding == null || Feeding.ToLower() != "on" ? false : true;
            claimsobj.Admission = Admission == null || Admission.ToLower() != "on" ? false : true;
            claimsobj.Note = Remark;
            claimsobj.Tag = (ClaimsTAGS)Enum.Parse(typeof(ClaimsTAGS), treatmentTag);


            Enrollee enrollee = _enrolleeService.GetEnrollee(claimsobj.Enrolleeid);

            //check if the enrollee exist
            if (enrollee != null)
            {
                Staff staff = _companyService.Getstaff(enrollee.Staffprofileid);
                string plancover =
                    _companyService.GetCompanyPlan(_companyService.Getstaff(enrollee.Staffprofileid).StaffPlanid).
                        AllowChildEnrollee
                        ? "FAMILY"
                        : "INDIVIDUAL";
                string healthplan =

                    _planService.GetPlan(
                        _companyService.GetCompanyPlan(_companyService.Getstaff(enrollee.Staffprofileid).StaffPlanid).
                            Planid).Name.ToUpper() + " " + plancover;
                claimsobj.EnrolleePlan = healthplan;
            }
            else
            {
                claimsobj.EnrolleePlan = "Unknown";
            }


            //add the owner details
            claimsobj.capturedBy = CurrentRequestData.CurrentUser.Id;
            claimsobj.capturedName = _userservice.GetUser(CurrentRequestData.CurrentUser.Id).Name;
            claimsobj.status = ClaimsBillStatus.Captured;
            if (claimsbatch != null)
            {
                claimsobj.ClaimBatch = claimsbatch;
                claimsbatch.Claims.Add(claimsobj);
                claimsbatch.status = ClaimBatchStatus.Capturing;
                bool resp = _claimsvc.AddClaimBatch(claimsbatch);
                _pageMessageSvc.SetSuccessMessage("Claim saved successfully.");
            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was an error saving claim.");
            }





            return Redirect(string.Format(_uniquePageService.GetUniquePage<CaptureClaimsFormPage>().AbsoluteUrl + "?id={0}",
                                       claimsbatch.Id));
        }

        public string SubmitClaimsForm2(FormCollection claimsform, CaptureClaimsFormPage page)
        {
            //used to logged failed posting for retry purpose.
            //Dictionary<string, int> failedbank = (Dictionary<string, int>)Session["faliedbak"];



            bool submitted = false;
            string hiddenID = claimsform["hiddenID"];
            string FormNo = claimsform["FormNo"];
            string policynumber = claimsform["policynumber"];
            string evscode = claimsform["evscode"];
            string enrolleename = claimsform["enrolleename"];
            string companyname = claimsform["companyname"];
            string companyid = claimsform["CompanyID"];

            string enrolleesex = claimsform["enrolleesex"];
            string providerename = claimsform["providerename"];
            string doctorname = claimsform["doctorname"];
            string doctorID = claimsform["Idnumber"];
            string areaofspecialization = claimsform["areaofspecialization"];
            string Idnumber = claimsform["Idnumber"];
            string doctorsigned = claimsform["doctorsigned"];
            string doctorssigndate = claimsform["doctorssigndate"];
            string specialistname = claimsform["specialistname"];
            string areaofspecializationSpecialist = claimsform["areaofspecializationSpecialist"];

            string specialistphonenumber = claimsform["specialistphonenumber"];
            string specialistsigned = claimsform["specialistsigned"];
            string specialistsigndate = claimsform["specialistsigndate"];
            string dateofservice = claimsform["dateofservice"];
            string durationoftreatment = claimsform["durationoftreatment"];
            string dateofadmission = claimsform["dateofadmission"];
            string dateofdischarge = claimsform["dateofdischarge"];
            string diagnosis = claimsform["diagnosis"];
            string Treatmentgiven = claimsform["Treatmentgiven"];
            string treatmentcode = claimsform["treatmentcode"];
            string referralcode = claimsform["referralcode"];

            string servicebillCount = claimsform["servicebillCount"];
            string drugbillCount = claimsform["drugbillCount"];

            string Enrolleesigned = claimsform["Enrolleesigned"];
            string EnrolleesignedDate = claimsform["EnrolleesignedDate"];

            string AllprescibedDrugs = claimsform["AllprescibedDrugs"];
            string LaboratoryInvestigation = claimsform["LaboratoryInvestigation"];
            string Admission = claimsform["Admission"];
            string Feeding = claimsform["Feeding"];
            string enrolleeage = claimsform["enrolleeage"];
            string Remark = claimsform["Remark"];

            string provideriD = claimsform["providerID"];
            string enrolleeID = claimsform["enrolleeID"];
            string ClaimsBatchid = claimsform["claimBatchID"];
            string treatmentTag = claimsform["treatmentTag"];

            //for remote for portal

            if (string.IsNullOrEmpty(treatmentTag))
            {
                treatmentTag = "11";
            }



            string diagnosisList = claimsform["diagnosisList"];

            if (string.IsNullOrEmpty(diagnosisList))
            {
                diagnosisList = string.Empty;

            }
            try
            {





                //Convert the form
                List<Entities.ClaimService> ServiceList = new List<Entities.ClaimService>();
                List<ClaimDrug> drugList = new List<ClaimDrug>();

                int servicecount = string.IsNullOrEmpty(servicebillCount) ? 0 : Convert.ToInt32(servicebillCount.Split(',')[0]);
                int drugcount = string.IsNullOrEmpty(drugbillCount) ? 0 : Convert.ToInt32(drugbillCount.Split(',')[0]);
                Entities.ClaimBatch claimsbatch = _claimsvc.GetClaimBatch(Convert.ToInt32(ClaimsBatchid));

                if (servicecount == 0 && drugcount == 0)
                {
                    //_pageMessageSvc.SetErrormessage("The Claim has no bill in it.");
                    return hiddenID;

                }


                if (string.IsNullOrEmpty(hiddenID))
                {
                    return hiddenID;
                }
                bool claimexist = _claimsvc.CheckClaimByClientID(hiddenID);

                if (claimexist)
                {
                    //not

                    return hiddenID;
                }


                Claim claimsobj = new Claim();
                //do your validation
                for (int i = 0; i <= servicecount; i++)
                {
                    Entities.ClaimService servicee = new Entities.ClaimService();
                    string itemDscr = claimsform[string.Format("servicebill{0}{1}", i, "ItemDescription")];
                    string itemDuration = claimsform[string.Format("servicebill{0}{1}", i, "Duration")];
                    string itemRate = claimsform[string.Format("servicebill{0}{1}", i, "Rate")];
                    string itemAmount = claimsform[string.Format("servicebill{0}{1}", i, "amount")];
                    string ItemId = claimsform[string.Format("servicebill{0}{1}", i, "itemID")];
                    if (!string.IsNullOrEmpty(itemDscr) && !string.IsNullOrEmpty(itemDuration) && !string.IsNullOrEmpty(itemRate) && !string.IsNullOrEmpty(itemAmount))
                    {
                        decimal amount = 0m;
                        int Srvid = 0;
                        int.TryParse(ItemId, out Srvid);

                        decimal.TryParse(itemAmount, out amount);
                        servicee.ServiceName = itemDscr;
                        servicee.ServiceDescription = itemDscr;
                        servicee.Duration = itemDuration;
                        servicee.rate = itemRate;
                        servicee.InitialAmount = amount;
                        servicee.ServiceId = Srvid;
                        servicee.Claim = claimsobj;
                        ServiceList.Add(servicee);

                    }


                }

                //for drugs
                for (int i = 0; i <= drugcount; i++)
                {
                    ClaimDrug drugg = new ClaimDrug();
                    string itemDscr = claimsform[string.Format("drugbill{0}{1}", i, "ItemDescription")];
                    string itemDuration = claimsform[string.Format("drugbill{0}{1}", i, "Unit")];
                    string itemRate = claimsform[string.Format("drugbill{0}{1}", i, "Rate")];
                    string itemAmount = claimsform[string.Format("drugbill{0}{1}", i, "amount")];
                    string ItemId = claimsform[string.Format("drugbill{0}{1}", i, "itemID")];
                    if (!string.IsNullOrEmpty(itemDscr) && !string.IsNullOrEmpty(itemDuration) && !string.IsNullOrEmpty(itemRate) && !string.IsNullOrEmpty(itemAmount))
                    {
                        int drugid = 0;
                        decimal amount = 0m;
                        decimal.TryParse(itemAmount, out amount);
                        int.TryParse(ItemId, out drugid);
                        drugg.DrugName = itemDscr;
                        drugg.DrugDescription = itemDscr;
                        drugg.Quantity = itemDuration;
                        drugg.rate = itemRate;
                        drugg.InitialAmount = amount;
                        drugg.DrugId = drugid;
                        drugg.Claim = claimsobj;
                        drugList.Add(drugg);


                    }


                }


                int provideridd = 0;
                int.TryParse(provideriD, out provideridd);


                claimsobj.ProviderId = provideridd;



                int enrolleeid = 0;
                int CompanyID = 0;

                int.TryParse(enrolleeID, out enrolleeid);
                int.TryParse(companyid, out CompanyID);
                claimsobj.Enrolleeid = enrolleeid;

                claimsobj.ClientsideID = hiddenID;
                claimsobj.enrolleeage = enrolleeage;
                claimsobj.enrolleeFullname = enrolleename.ToUpper();
                claimsobj.enrolleeGender = enrolleesex;

                claimsobj.enrolleeCompanyName = "Unknown";
                if (CompanyID > 0)
                {
                    Company companyobj = _companyService.GetCompany(CompanyID);
                    if (companyobj != null)
                    {
                        claimsobj.enrolleeCompanyName = companyobj.Name.ToUpper();
                    }

                }

                claimsobj.enrolleeCompanyId = CompanyID;
                claimsobj.enrolleePolicyNumber = policynumber;
                claimsobj.ClaimsSerialNo = FormNo;
                claimsobj.EVSCode = evscode;
                claimsobj.DoctorsName = doctorname;
                claimsobj.DoctorsId = doctorID;
                claimsobj.AreaOfSpecialty = areaofspecialization;
                claimsobj.Signature = string.Empty;
                claimsobj.DoctorSigned = doctorsigned == null || doctorsigned.ToLower() != "on" ? false : true;
                claimsobj.DoctorSignecDate = !string.IsNullOrEmpty(doctorssigndate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(doctorssigndate.Substring(6, 4)), Convert.ToInt32(doctorssigndate.Substring(3, 2)), Convert.ToInt32(doctorssigndate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
                claimsobj.specialistName = specialistname;
                claimsobj.specialistphonenumber = specialistphonenumber;
                claimsobj.specialistSigned = specialistsigned == null || specialistsigned.ToLower() != "on" ? false : true;
                claimsobj.specialistSignecDate = !string.IsNullOrEmpty(specialistsigndate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(specialistsigndate.Substring(6, 4)), Convert.ToInt32(specialistsigndate.Substring(3, 2)), Convert.ToInt32(specialistsigndate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
                claimsobj.AreaOfSpecialtyforspecialist = areaofspecializationSpecialist;
                claimsobj.ServiceDate = !string.IsNullOrEmpty(dateofservice) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofservice.Substring(6, 4)), Convert.ToInt32(dateofservice.Substring(3, 2)), Convert.ToInt32(dateofservice.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
                claimsobj.AdmissionDate = !string.IsNullOrEmpty(dateofadmission) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofadmission.Substring(6, 4)), Convert.ToInt32(dateofadmission.Substring(3, 2)), Convert.ToInt32(dateofadmission.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
                claimsobj.DischargeDate = !string.IsNullOrEmpty(dateofdischarge) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofdischarge.Substring(6, 4)), Convert.ToInt32(dateofdischarge.Substring(3, 2)), Convert.ToInt32(dateofdischarge.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
                claimsobj.Durationoftreatment = durationoftreatment;
                claimsobj.Diagnosis = diagnosis;
                claimsobj.TreatmentGiven = Treatmentgiven;
                claimsobj.TreatmentCode = treatmentcode;
                claimsobj.referalCode = referralcode;
                claimsobj.DrugList = drugList;
                claimsobj.ServiceList = ServiceList;
                claimsobj.enrolleeSigned = Enrolleesigned == null || Enrolleesigned.ToLower() != "on" ? false : true;
                claimsobj.EnrolleeSignDate = !string.IsNullOrEmpty(EnrolleesignedDate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(EnrolleesignedDate.Substring(6, 4)), Convert.ToInt32(EnrolleesignedDate.Substring(3, 2)), Convert.ToInt32(EnrolleesignedDate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
                claimsobj.AllprescibedDrugs = AllprescibedDrugs == null || AllprescibedDrugs.ToLower() != "on" ? false : true;
                claimsobj.LaboratoryInvestigation = LaboratoryInvestigation == null || LaboratoryInvestigation.ToLower() != "on" ? false : true;
                claimsobj.Feeding = Feeding == null || Feeding.ToLower() != "on" ? false : true;
                claimsobj.Admission = Admission == null || Admission.ToLower() != "on" ? false : true;
                claimsobj.Note = Remark;
                claimsobj.Tag = (ClaimsTAGS)Enum.Parse(typeof(ClaimsTAGS), treatmentTag);
                claimsobj.diagnosticsIDString = !string.IsNullOrEmpty(diagnosisList) ? diagnosisList : "";

                Enrollee enrollee = _enrolleeService.GetEnrollee(claimsobj.Enrolleeid);

                //check if the enrollee exist
                if (enrollee != null)
                {
                    Staff staff = _companyService.Getstaff(enrollee.Staffprofileid);
                    string plancover =
                        _companyService.GetCompanyPlan(_companyService.Getstaff(enrollee.Staffprofileid).StaffPlanid).
                            AllowChildEnrollee
                            ? "FAMILY"
                            : "INDIVIDUAL";
                    string healthplan =

                        _planService.GetPlan(
                            _companyService.GetCompanyPlan(_companyService.Getstaff(enrollee.Staffprofileid).StaffPlanid).
                                Planid).Name.ToUpper() + " " + plancover;
                    claimsobj.EnrolleePlan = healthplan;
                }
                else
                {
                    claimsobj.EnrolleePlan = "Unknown";
                }


                //add the owner details
                if (CurrentRequestData.CurrentUser != null)
                {

                    claimsobj.capturedBy = CurrentRequestData.CurrentUser.Id;
                    claimsobj.capturedName = _userservice.GetUser(CurrentRequestData.CurrentUser.Id).Name;
                }
                else
                {
                    claimsobj.capturedBy = 1;
                    claimsobj.capturedName = "Remote Capture";
                }

                claimsobj.status = ClaimsBillStatus.Captured;
                bool addresp = false;
                if (claimsbatch != null && claimsbatch.ProviderId == claimsobj.ProviderId)
                {
                    claimsobj.ClaimBatch = claimsbatch;
                    claimsbatch.Claims.Add(claimsobj);
                    //claimsbatch.status = ClaimBatchStatus.Capturing;
                    addresp = _claimsvc.AddClaimBatch(claimsbatch);
                    //_pageMessageSvc.SetSuccessMessage("Claim saved successfully.");
                }
                else
                {
                    //_pageMessageSvc.SetErrormessage("There was an error saving claim.");
                }



                if (claimsbatch != null && claimsbatch.claimscountfromclient > 0 && addresp)
                {
                    //its remote capture so delete from the provider backup.

                    Provider provider = _providerSvc.GetProvider(claimsbatch.ProviderId);

                    if (provider != null)
                    {
                        //delete all the keys in stored backup
                        List<ProviderClaimBK> items = provider.ClaimBK.Where(x => x.clientkey == claimsobj.ClientsideID).ToList();

                        foreach (ProviderClaimBK item in items)
                        {
                            provider.ClaimBK.Remove(item);
                            _providerSvc.UpdateProvider(provider);
                        }

                    }
                }

                if (claimsbatch != null && claimsbatch.claimscountfromclient > 0 && claimsbatch.claimscountfromclient == claimsbatch.Claims.Count())
                {
                    //this is remote send 

                    //close the balch now.
                    try
                    {
                        claimsbatch.status = ClaimBatchStatus.Vetting;
                        claimsbatch.submitedVetbyUser = CurrentRequestData.CurrentUser != null ? CurrentRequestData.CurrentUser.Id : 1;
                        claimsbatch.VetDate = CurrentRequestData.Now;
                        _claimsvc.UpdateClaimBatch(claimsbatch);

                        decimal totalservice = 0m;
                        decimal totaldrug = 0m;
                        decimal totalSum = 0m;
                        int counttt = 0;
                        foreach (Claim item in claimsbatch.Claims)
                        {
                            totalservice = Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount));
                            totaldrug = Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount));
                            counttt++;
                            totalSum = +totaldrug + totalservice;
                        }



                        //log email
                        Provider provider = _providerSvc.GetProvider(claimsbatch.ProviderId);
                        string thename = provider != null ? provider.Name.ToUpper() : "--";
                        string theaddress = provider != null ? provider.Address.ToLower() : "--";
                        IncomingClaims areply = claimsbatch.IncomingClaims.FirstOrDefault();
                        string providerEmail = provider.providerlogin.email;


                        string themonth = "--";
                        if (!string.IsNullOrEmpty(areply.month_string) && areply.month_string.Split(',').Count() > 0)
                        {
                            foreach (string itemmm in areply.month_string.Split(','))
                            {
                                themonth = themonth + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(itemmm)) + ",";
                            }
                            themonth = themonth + areply.year.ToString();


                        }
                        //_helperSvc.PushUserNotification 
                        QueuedMessage emailmsg = new QueuedMessage();
                        emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                        emailmsg.ToAddress = "silverlinea@novohealthafrica.org, adanmaa@novohealthafrica.org";
                        emailmsg.Cc = "lagoswest@novohealthafrica.org, northcentralregion@novohealthafrica.org, southsouthregion@novohealthafrica.org, northnorth@novohealthafrica.org, southwestregion@novohealthafrica.org";
                        emailmsg.Bcc = "provider@novohealthafrica.org";

                        emailmsg.Subject = "Provider Claim Submission ";
                        emailmsg.FromName = "NOVOHUB";
                        emailmsg.Body = string.Format("{0} - {1} just sent in their {2} claim ,with batch id : {3} .kindly attend to it. Thank you", thename, theaddress, themonth, claimsbatch.Id);

                        _emailSender.AddToQueue(emailmsg);

                        if (!string.IsNullOrEmpty(providerEmail))
                        {
                            QueuedMessage emailmsg2 = new QueuedMessage();
                            emailmsg2.FromAddress = _mailSettings.SystemEmailAddress;
                            emailmsg2.ToAddress = provider.providerlogin.email;
                            if (!string.IsNullOrEmpty(provider.providerlogin.Altemail))
                            {
                                emailmsg2.Cc = provider.providerlogin.Altemail;

                            }
                            if (!string.IsNullOrEmpty(provider.providerlogin.Altemail2))
                            {
                                emailmsg2.Bcc = provider.providerlogin.Altemail2;

                            }
                            emailmsg2.Subject = "Claim Submission ";
                            emailmsg2.FromName = "NOVOHUB";
                            emailmsg2.Body = string.Format("Dear Provider,You have successfully submitted your {2} claim ,with batch id : {3} .Your claim is being processed. Thank you", thename, theaddress, themonth, claimsbatch.Id);

                            _emailSender.AddToQueue(emailmsg2);
                        }

                    }
                    catch (Exception)
                    {

                    }

                }

                return hiddenID;

            }
            catch (Exception ex)
            {
                Log log = new Log();
                log.Message = string.Format("There was a problem saving claims with client id {0} {1}", hiddenID,
                    ex.Message);
                log.Type = LogEntryType.Audit;
                log.Detail = "Capture Claim Submit Error";
                _logger.Insert(log);

                return hiddenID;

            }
        }

        [HttpGet]
        [ActionName("ExpandClaim")]
        public ActionResult ExpandClaim(int? mode, int Id, ExpandClaimsPage page)
        {

            Entities.ClaimBatch claimbatch = _claimsvc.GetClaimBatch(Convert.ToInt32(Id));

            if (claimbatch != null)
            {
                page.Batch = claimbatch;

            }
            else
            {

                _pageMessageSvc.SetErrormessage("The Claim Batch selected does not exist.");
                return Redirect(string.Format(_uniquePageService.GetUniquePage<ExpandClaimsPage>().AbsoluteUrl + "?id={0}",
                                    Id));

            }

            Provider provider = _providerSvc.GetProvider(claimbatch.ProviderId);
            ViewBag.Batchh = claimbatch.Batch.ToString().ToUpper();
            ViewBag.hospital = provider.Name.ToUpper();
            ViewBag.TotalCaptured = claimbatch.Claims.Count.ToString() + " Claims Forms.";
            NewMethod(claimbatch);
            ViewBag.MonthYear = CurrentRequestData.CultureInfo.DateTimeFormat.GetMonthName(claimbatch.month).ToString() + " " + claimbatch.year;
            int modee = 0;
            ViewBag.showbuttons = int.TryParse(mode.ToString(), out modee) && modee == 2 ? false : true;
            return View(page);
        }

        private void NewMethod(Entities.ClaimBatch claimbatch)
        {
            ViewBag.TotalAmount = "₦ " + Convert.ToDecimal((claimbatch.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + claimbatch.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount)))).ToString("N");
        }

        [HttpGet]
        [ActionName("DeleteClaim")]
        public ActionResult DeleteClaim(int Id)
        {
            Claim claim = _claimsvc.GetClaim(Id);


            return PartialView("DeleteClaim", claim);

        }

        [HttpPost]
        [ActionName("DeleteClaim")]
        public ActionResult DeleteClaim(Claim claim)
        {
            Claim claimobj = _claimsvc.GetClaim(claim.Id);

            if (claim != null)
            {

                //get claim batch

                Claim obj = claimobj.ClaimBatch.Claims.Single(x => x.Id == claimobj.Id);
                claimobj.ClaimBatch.Claims.Remove(obj);
                _claimsvc.UpdateClaim(claimobj);


                //_claimsvc.DeleteClaim(claim);
                _pageMessageSvc.SetSuccessMessage("Bill was deleted successfully.");
                return Redirect(string.Format(_uniquePageService.GetUniquePage<ExpandClaimsPage>().AbsoluteUrl + "?id={0}",
                                       claim.ClaimBatch.Id));
            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was a problem deleting Bill.");
                return Redirect(string.Format(_uniquePageService.GetUniquePage<ExpandClaimsPage>().AbsoluteUrl + "?id={0}",
                                  claim.ClaimBatch.Id));
            }






        }


        [ActionName("EditClaimForm")]
        public ActionResult EditClaimForm(int id, EditClaimsFormPage page)
        {
            Claim claim = _claimsvc.GetClaim(id);
            Entities.ClaimBatch claimbatch = claim.ClaimBatch;
            Provider provider = _providerSvc.GetProvider(claimbatch.ProviderId);
            ViewBag.Batchh = claimbatch.Batch.ToString().ToUpper();
            ViewBag.hospital = provider.Name.ToUpper();
            ViewBag.TotalCaptured = claimbatch.Claims.Count.ToString() + " Claims Forms.";
            ViewBag.TotalAmount = "₦ " + Convert.ToDecimal((claimbatch.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + claimbatch.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount)))).ToString("N");

            int tariffID = string.IsNullOrEmpty(provider.ProviderTariffs) ? 0 : Convert.ToInt32(provider.ProviderTariffs.Split(',')[0]);

            ViewBag.HasTariff = provider != null && provider.ProviderTariffs != null && !string.IsNullOrEmpty(provider.ProviderTariffs);
            string[] tags = Enum.GetNames(typeof(ClaimsTAGS));
            List<GenericReponse2> outtags = new List<GenericReponse2>();
            foreach (string tag in tags)
            {
                outtags.Add(new GenericReponse2() { Id = (int)Enum.Parse(typeof(ClaimsTAGS), tag), Name = tag.ToUpper() });

            }

            outtags.Insert(0, new GenericReponse2() { Id = -1, Name = "--Select Tag--" });


            ViewBag.treatmentTagList = outtags;
            ViewBag.ProviderId = provider.Id;
            ViewBag.ClaimBatchId = claimbatch.Id;
            page.Claim = claim;
            return View(page);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditClaimForm(FormCollection claimsform, EditClaimsFormPage page)
        {
            string FormNo = claimsform["FormNo"];
            string policynumber = claimsform["policynumber"];
            string evscode = claimsform["evscode"];
            string enrolleename = claimsform["enrolleename"];
            string companyname = claimsform["companyname"];
            string companyid = claimsform["CompanyID"];

            string enrolleesex = claimsform["enrolleesex"];
            string providerename = claimsform["providerename"];
            string doctorname = claimsform["doctorname"];
            string doctorID = claimsform["Idnumber"];
            string areaofspecialization = claimsform["areaofspecialization"];
            string Idnumber = claimsform["Idnumber"];
            string doctorsigned = claimsform["doctorsigned"];
            string doctorssigndate = claimsform["doctorssigndate"];
            string specialistname = claimsform["specialistname"];
            string areaofspecializationSpecialist = claimsform["areaofspecializationSpecialist"];

            string specialistphonenumber = claimsform["specialistphonenumber"];
            string specialistsigned = claimsform["specialistsigned"];
            string specialistsigndate = claimsform["specialistsigndate"];
            string dateofservice = claimsform["dateofservice"];
            string durationoftreatment = claimsform["durationoftreatment"];
            string dateofadmission = claimsform["dateofadmission"];
            string dateofdischarge = claimsform["dateofdischarge"];
            string diagnosis = claimsform["diagnosis"];
            string Treatmentgiven = claimsform["Treatmentgiven"];
            string treatmentcode = claimsform["treatmentcode"];
            string referralcode = claimsform["referralcode"];

            string servicebillCount = claimsform["servicebillCount"];
            string drugbillCount = claimsform["drugbillCount"];

            string Enrolleesigned = claimsform["Enrolleesigned"];
            string EnrolleesignedDate = claimsform["EnrolleesignedDate"];

            string AllprescibedDrugs = claimsform["AllprescibedDrugs"];
            string LaboratoryInvestigation = claimsform["LaboratoryInvestigation"];
            string Admission = claimsform["Admission"];
            string Feeding = claimsform["Feeding"];
            string Remark = claimsform["Remark"];

            string providerID = claimsform["providerID"];
            string enrolleeID = claimsform["enrolleeID"];
            string ClaimsBatchid = claimsform["claimBatchID"];
            string treatmentTag = claimsform["treatmentTag"];
            string diagnosisList = claimsform["diagnosisList"];
            string ClaimsID = claimsform["ClaimsID"];
            //Convert the form
            List<Entities.ClaimService> ServiceList = new List<Entities.ClaimService>();
            List<ClaimDrug> drugList = new List<ClaimDrug>();

            int servicecount = string.IsNullOrEmpty(servicebillCount) ? 0 : Convert.ToInt32(servicebillCount.Split(',')[0]);
            int drugcount = string.IsNullOrEmpty(drugbillCount) ? 0 : Convert.ToInt32(drugbillCount.Split(',')[0]);
            Entities.ClaimBatch claimsbatch = _claimsvc.GetClaimBatch(Convert.ToInt32(ClaimsBatchid));


            if (servicecount == 0 && drugcount == 0)
            {
                _pageMessageSvc.SetErrormessage("The Claim has no bill in it.");
                return Redirect(string.Format(_uniquePageService.GetUniquePage<CaptureClaimsFormPage>().AbsoluteUrl + "?id={0}",
                                    Convert.ToInt32(ClaimsBatchid)));

            }
            Claim claimsobj = !string.IsNullOrEmpty(ClaimsID) ? claimsbatch.Claims.Single(x => x.Id == Convert.ToInt32(ClaimsID)) : new Claim();
            claimsobj.DrugList.Clear();
            claimsobj.ServiceList.Clear();

            //_claimsvc.CleanClaim(claimsobj);
            //do your validation
            for (int i = 0; i <= servicecount; i++)
            {
                Entities.ClaimService servicee = new Entities.ClaimService();
                string itemDscr = claimsform[string.Format("servicebill{0}{1}", i, "ItemDescription")];
                string itemDuration = claimsform[string.Format("servicebill{0}{1}", i, "Duration")];
                string itemRate = claimsform[string.Format("servicebill{0}{1}", i, "Rate")];
                string itemAmount = claimsform[string.Format("servicebill{0}{1}", i, "amount")];

                string itemVettedAmount = claimsform[string.Format("servicebill{0}{1}", i, "vettedamount")];
                string itemVettedcomment = claimsform[string.Format("servicebill{0}{1}", i, "vettedcomment")];
                string ItemId = claimsform[string.Format("servicebill{0}{1}", i, "itemID")];
                if (!string.IsNullOrEmpty(itemDscr) && !string.IsNullOrEmpty(itemDuration) && !string.IsNullOrEmpty(itemRate) && !string.IsNullOrEmpty(itemAmount))
                {
                    decimal amount = 0m;
                    decimal vettedamount = 0m;
                    int Srvid = 0;
                    int.TryParse(ItemId, out Srvid);

                    decimal.TryParse(itemAmount, out amount);
                    decimal.TryParse(itemVettedAmount, out vettedamount);
                    servicee.ServiceName = itemDscr;
                    servicee.ServiceDescription = itemDscr;
                    servicee.Duration = itemDuration;
                    servicee.rate = itemRate;
                    servicee.InitialAmount = amount;
                    servicee.VettedAmount = vettedamount;
                    servicee.VettingComment = !string.IsNullOrEmpty(itemVettedcomment) ? itemVettedcomment : "";
                    servicee.ServiceId = Srvid;
                    servicee.Claim = claimsobj;
                    claimsobj.ServiceList.Add(servicee);

                }


            }

            //for drugs
            for (int i = 0; i <= drugcount; i++)
            {
                ClaimDrug drugg = new ClaimDrug();
                string itemDscr = claimsform[string.Format("drugbill{0}{1}", i, "ItemDescription")];
                string itemDuration = claimsform[string.Format("drugbill{0}{1}", i, "Unit")];
                string itemRate = claimsform[string.Format("drugbill{0}{1}", i, "Rate")];
                string itemAmount = claimsform[string.Format("drugbill{0}{1}", i, "amount")];

                string itemVettedAmount = claimsform[string.Format("drugbill{0}{1}", i, "vettedamount")];
                string itemVettedComment = claimsform[string.Format("drugbill{0}{1}", i, "vettedcomment")];


                string ItemId = claimsform[string.Format("drugbill{0}{1}", i, "itemID")];
                if (!string.IsNullOrEmpty(itemDscr) && !string.IsNullOrEmpty(itemDuration) && !string.IsNullOrEmpty(itemRate) && !string.IsNullOrEmpty(itemAmount))
                {
                    int drugid = 0;
                    decimal amount = 0m;
                    decimal Vettedamount = 0m;
                    decimal.TryParse(itemAmount, out amount);
                    decimal.TryParse(itemVettedAmount, out Vettedamount);
                    int.TryParse(ItemId, out drugid);
                    drugg.DrugName = itemDscr;
                    drugg.DrugDescription = itemDscr;
                    drugg.Quantity = itemDuration;
                    drugg.rate = itemRate;
                    drugg.InitialAmount = amount;
                    drugg.DrugId = drugid;
                    drugg.VettedAmount = Vettedamount;
                    drugg.VettingComment = !string.IsNullOrEmpty(itemVettedComment) ? itemVettedComment : "";
                    drugg.Claim = claimsobj;

                    claimsobj.DrugList.Add(drugg);


                }


            }

            claimsobj.diagnosticsIDString = diagnosisList;
            //claimsobj.ProviderId = Convert.ToInt32(providerID);
            claimsobj.Enrolleeid = Convert.ToInt32(enrolleeID);
            claimsobj.enrolleeFullname = enrolleename.ToUpper();
            claimsobj.enrolleeGender = enrolleesex;
            //updated with companyName
            claimsobj.enrolleeCompanyName = "Unknown";
            if (Convert.ToInt32(companyid) > 0)
            {
                Company companyobj = _companyService.GetCompany(Convert.ToInt32(companyid));
                if (companyobj != null)
                {
                    claimsobj.enrolleeCompanyName = companyobj.Name.ToUpper();
                }

            }

            claimsobj.enrolleeCompanyId = Convert.ToInt32(companyid);
            claimsobj.enrolleePolicyNumber = policynumber;
            claimsobj.ClaimsSerialNo = FormNo;
            claimsobj.EVSCode = evscode;
            claimsobj.DoctorsName = doctorname;
            claimsobj.DoctorsId = doctorID;
            claimsobj.AreaOfSpecialty = areaofspecialization;
            claimsobj.Signature = string.Empty;
            claimsobj.DoctorSigned = doctorsigned == null || doctorsigned.ToLower() != "on" ? false : true;
            claimsobj.DoctorSignecDate = !string.IsNullOrEmpty(doctorssigndate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(doctorssigndate.Substring(6, 4)), Convert.ToInt32(doctorssigndate.Substring(3, 2)), Convert.ToInt32(doctorssigndate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.specialistName = specialistname;
            claimsobj.specialistphonenumber = specialistphonenumber;
            claimsobj.specialistSigned = specialistsigned == null || specialistsigned.ToLower() != "on" ? false : true;
            claimsobj.specialistSignecDate = !string.IsNullOrEmpty(specialistsigndate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(specialistsigndate.Substring(6, 4)), Convert.ToInt32(specialistsigndate.Substring(3, 2)), Convert.ToInt32(specialistsigndate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.AreaOfSpecialtyforspecialist = areaofspecializationSpecialist;
            claimsobj.ServiceDate = !string.IsNullOrEmpty(dateofservice) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofservice.Substring(6, 4)), Convert.ToInt32(dateofservice.Substring(3, 2)), Convert.ToInt32(dateofservice.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.AdmissionDate = !string.IsNullOrEmpty(dateofadmission) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofadmission.Substring(6, 4)), Convert.ToInt32(dateofadmission.Substring(3, 2)), Convert.ToInt32(dateofadmission.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.DischargeDate = !string.IsNullOrEmpty(dateofdischarge) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofdischarge.Substring(6, 4)), Convert.ToInt32(dateofdischarge.Substring(3, 2)), Convert.ToInt32(dateofdischarge.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.Durationoftreatment = durationoftreatment;
            claimsobj.Diagnosis = diagnosis;
            claimsobj.TreatmentGiven = Treatmentgiven;
            claimsobj.TreatmentCode = treatmentcode;
            claimsobj.referalCode = referralcode;
            //delete the old druglist

            claimsobj.enrolleeSigned = Enrolleesigned == null || Enrolleesigned.ToLower() != "on" ? false : true;
            claimsobj.EnrolleeSignDate = !string.IsNullOrEmpty(EnrolleesignedDate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(EnrolleesignedDate.Substring(6, 4)), Convert.ToInt32(EnrolleesignedDate.Substring(3, 2)), Convert.ToInt32(EnrolleesignedDate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            claimsobj.AllprescibedDrugs = AllprescibedDrugs == null || AllprescibedDrugs.ToLower() != "on" ? false : true;
            claimsobj.LaboratoryInvestigation = LaboratoryInvestigation == null || LaboratoryInvestigation.ToLower() != "on" ? false : true;
            claimsobj.Feeding = Feeding == null || Feeding.ToLower() != "on" ? false : true;
            claimsobj.Admission = Admission == null || Admission.ToLower() != "on" ? false : true;
            claimsobj.Note = Remark;
            claimsobj.Tag = (ClaimsTAGS)Enum.Parse(typeof(ClaimsTAGS), treatmentTag);
            Enrollee enrollee = _enrolleeService.GetEnrollee(claimsobj.Enrolleeid);


            //var staff = _companyService.Getstaff(enrollee.Staffprofileid);
            //var plancover =
            //   _companyService.GetCompanyPlan(_companyService.Getstaff(enrollee.Staffprofileid).StaffPlanid).
            //       AllowChildEnrollee
            //       ? "FAMILY"
            //       : "INDIVIDUAL";
            //var healthplan =

            //    _planService.GetPlan(
            //        _companyService.GetCompanyPlan(_companyService.Getstaff(enrollee.Staffprofileid).StaffPlanid).
            //            Planid).Name.ToUpper() + " " + plancover;
            //claimsobj.EnrolleePlan = healthplan;

            //add the owner details
            //claimsobj.capturedBy = CurrentRequestData.CurrentUser.Id;
            //claimsobj.capturedName = _userservice.GetUser(CurrentRequestData.CurrentUser.Id).Name;
            //claimsobj.status = ClaimsBillStatus.Captured;
            claimsobj.status = ClaimsBillStatus.Vetted;
            claimsobj.vettedBy = CurrentRequestData.CurrentUser.Id;

            if (claimsbatch != null)
            {
                claimsobj.ClaimBatch = claimsbatch;
                int index = claimsbatch.Claims.IndexOf(claimsobj);
                claimsbatch.Claims[index] = claimsobj;
                claimsbatch.Claims[index].diagnosticsIDString = diagnosisList;
                bool resp = _claimsvc.UpdateClaimBatch(claimsbatch);
                _pageMessageSvc.SetSuccessMessage("Claim saved successfully.");
            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was an error saving claim.");
            }





            return Redirect(string.Format(_uniquePageService.GetUniquePage<EditClaimsFormPage>().AbsoluteUrl + "?id={0}",
                                       Convert.ToInt32(claimsobj.Id)));
        }


        [HttpGet]
        [ActionName("SubmitClaimVet")]
        public ActionResult SubmitClaimVet(int Id)
        {
            Entities.ClaimBatch claim = _claimsvc.GetClaimBatch(Id);

            Provider provider = _providerSvc.GetProvider(claim.ProviderId);
            ViewBag.Batchh = claim.Batch.ToString().ToUpper();
            ViewBag.hospital = provider.Name.ToUpper();
            ViewBag.TotalCaptured = claim.Claims.Count.ToString() + " Claims Forms.";
            ViewBag.TotalAmount = "₦ " + Convert.ToDecimal((claim.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + claim.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount)))).ToString("N");
            return PartialView("SubmitClaimVet", claim);

        }

        [HttpPost]
        [ActionName("SubmitClaimVet")]
        public ActionResult SubmitClaimVet(Entities.ClaimBatch claim)
        {

            if (claim != null)
            {
                Entities.ClaimBatch batch = _claimsvc.GetClaimBatch(claim.Id);
                batch.status = ClaimBatchStatus.Vetting;
                batch.submitedVetbyUser = CurrentRequestData.CurrentUser.Id;
                batch.VetDate = CurrentRequestData.Now;


                _claimsvc.UpdateClaimBatch(batch);
                _pageMessageSvc.SetSuccessMessage("Claim batch was successfully submitted for vetting.");
                return _uniquePageService.RedirectTo<CaptureClaimsPage>();

            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was a oroblem submiting claims batch.");
                return _uniquePageService.RedirectTo<CaptureClaimsPage>();
            }






        }
        [ActionName("VetClaimList")]
        public ActionResult VetClaimList(VetClaimsPage page)
        {
            int year = CurrentRequestData.Now.Year;
            List<GenericReponse2> yealist = new List<GenericReponse2>();


            for (int i = 0; i < 20; i++)
            {
                yealist.Add(new GenericReponse2 { Id = year - i, Name = (year - i).ToString() });
            }
            ViewBag.YearList = yealist;
            List<GenericReponse2> plist = _providerSvc.GetProviderNameList().OrderBy(x => x.Name).ToList();
            ViewBag.MyProvidersList = _providerSvc.GetProviderNameList().OrderBy(x => x.Name);
            plist.Insert(0, new GenericReponse2 { Id = -1, Name = "All Providers" });
            ViewBag.PrvidersList = plist;
            IList<MrCMS.Entities.People.User> users = _chatservice.Getallusers();

            var userlist = from aereply in users
                           select new
                           {
                               Id = aereply.Id,
                               Name = aereply.Name,
                           };

            var userr = userlist.ToList();

            userr.Add(new { Id = -1, Name = "Select" });


            ViewBag.UserList = userr.OrderBy(x => x.Id).ToList();
            List<GenericReponse> batchlist = new List<GenericReponse>();
            foreach (string item in Enum.GetNames(typeof(Utility.ClaimBatch)))
            {
                batchlist.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(Utility.ClaimBatch), item)).ToString(), Name = item.ToUpper() });
            }
            ViewBag.BatchList = batchlist;
            ViewBag.Defaultdate = CurrentRequestData.Now.ToString("MM/dd/yyyy");

            IEnumerable<Zone> zones = _helperSvc.GetallZones();
            List<GenericReponse2> zonelist = new List<GenericReponse2>();

            foreach (Zone item in zones)
            {
                GenericReponse2 shii = new GenericReponse2()
                {
                    Id = item.Id,
                    Name = item.Name
                };
                zonelist.Add(shii);


            }

            zonelist.Insert(0, new GenericReponse2() { Id = -1, Name = "All Zones" });
            ViewBag.ZoneList = zonelist;
            return View(page);

        }
        [HttpGet]
        public ActionResult VetSingleClaim(int Id, VetSingleClaimsPage page)
        {
            Claim claim = _claimsvc.GetClaim(Id);


            Enrollee enrollee = _enrolleeService.GetEnrollee(claim.Enrolleeid);
            Staff staff = new Staff();
            Company company = new Company();
            CompanySubsidiary companySubsidiary = new CompanySubsidiary();


            if (enrollee != null)
            {
                staff = _companyService.Getstaff(Convert.ToInt32(enrollee.Staffprofileid));
                company = _companyService.GetCompany(claim.enrolleeCompanyId);
                companySubsidiary = _companyService.Getsubsidiary(staff.CompanySubsidiary);
            }
            else
            {
                //set this guy to null value
                company = null;
            }



            Provider provider = _providerSvc.GetProvider(claim.ProviderId);
            ViewBag.TariffID = !string.IsNullOrEmpty(provider.ProviderTariffs) ? provider.ProviderTariffs.Split(',')[0] : "-1";

            ViewBag.enrolleeimg = enrollee != null ? Convert.ToBase64String(enrollee.EnrolleePassport.Imgraw) : "";
            ViewBag.MaritalStatus = enrollee != null ? Enum.GetName(typeof(Utility.MaritalStatus), enrollee.Maritalstatus) : "Unknown";
            ViewBag.Sex = enrollee != null ? Enum.GetName(typeof(Sex), enrollee.Sex) : "Unknown";

            State state = enrollee != null ? _helperSvc.GetState(enrollee.Stateid) : null;
            Lga lga = enrollee != null ? _helperSvc.GetLgabyId(enrollee.Lgaid) : null;

            ViewBag.State = enrollee != null && state != null ? state.Name.ToString() : "Unknown";
            ViewBag.LGA = enrollee != null && lga == null ? lga.Name.ToUpper() : "Unknown";

            ViewBag.SponsorshipType = enrollee != null ? Enum.GetName(typeof(Sponsorshiptype), enrollee.Sponsorshiptype) : "Unknown";
            PlanVm plan = null;
            plan = enrollee != null ? _planService.GetPlan(_companyService.GetCompanyPlan(staff.StaffPlanid).Planid) : null;
            ViewBag.SubscriptionType = plan != null ? plan.Name.ToUpper() : "Unknown";
            string comp = "Unknown";
            if (claim.enrolleeCompanyName != null)
            {
                comp = claim.enrolleeCompanyName;
            }
            else
            {
                comp = null;
            }



            ViewBag.Company = company != null ? company.Name.ToUpper() : comp;
            ViewBag.CompanySubsidiary = enrollee != null && companySubsidiary != null ? companySubsidiary.Subsidaryname.ToUpper() : "Unknown";
            ViewBag.ProviderChoice = provider.Name.ToUpper();
            ViewBag.DateAddedEnrolle = enrollee != null ? Convert.ToDateTime(enrollee.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern) : "Unknown";
            ViewBag.AddedByEnrollee = enrollee != null && enrollee.Createdby > 0 ? _userservice.GetUser(enrollee.Createdby).Name : "Auto Upload";
            ViewBag.companyPlanId = staff.StaffPlanid;
            if (enrollee != null && enrollee.Isexpundged)
            {
                ViewBag.ExpungedByEnrollee = enrollee != null && enrollee.Expungedby > 0 ? _userservice.GetUser(enrollee.Expungedby).Name : "--";
                ViewBag.DateExpungedEnrollee = enrollee != null ? Convert.ToDateTime(enrollee.Dateexpunged).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern) : "Unknown";
            }
            ViewBag.Batchh = claim.ClaimBatch.Batch.ToString().ToUpper();
            ViewBag.hospital = provider.Name.ToUpper();
            ViewBag.TotalCaptured = claim.ClaimBatch.Claims.Count.ToString() + " Claims Forms.";
            ViewBag.TotalVetted = claim.ClaimBatch.Claims.Count(x => x.status == ClaimsBillStatus.Vetted) + " Vetted Bills.";
            ViewBag.MonthYear = CurrentRequestData.CultureInfo.DateTimeFormat.GetMonthName(claim.ClaimBatch.month).ToString() + " " + claim.ClaimBatch.year;
            ViewBag.ClaimBatchId = claim.ClaimBatch.Id;

            foreach (ClaimDrug item in claim.DrugList)
            {

                if (item.DrugId > 0)
                {
                    DrugTariff druggg = _tariffSvc.GetDrug(item.DrugId);
                    item.costofdrug = druggg != null ? druggg.Price : 0m;
                }

                //added else
                else
                {

                }

                if (item.InitialAmount > item.costofdrug)
                {
                    item.flagred = true;
                }
                //added else
                else
                {

                }
            }

            foreach (Entities.ClaimService item in claim.ServiceList)
            {

                if (item.ServiceId > 0)
                {
                    ServiceTariff druggg = _tariffSvc.GetServiceTariff(item.ServiceId);
                    item.costofdrug = druggg != null ? druggg.Price : 0m;
                }
                //added else
                else
                {

                }

                if (item.InitialAmount > item.costofdrug)
                {
                    item.flagred = true;
                }
                //added else
                else
                {

                }

            }


            page.Claim = claim;
            page.Enrolleemodel = enrollee;

            //smart vetting stuff
            string expungedstring = "";
            string visitstring = string.Empty;
            System.Text.StringBuilder protocolstring = new System.Text.StringBuilder();
            if (enrollee != null && !string.IsNullOrEmpty(enrollee.Policynumber))
            {

                DateTime d = CurrentRequestData.Now;
                //the policy number is not empty 
                DateTime start = new DateTime(d.Year, 1, 1);
                DateTime end = new DateTime(d.Year, 12, 31);

                IList<ClaimHistory> visits = _claimsvc.GetClaimHistoryByPolicyNumber(enrollee.Policynumber, start, end);

                if (visits.Any())
                {
                    visitstring = string.Format("The Enrollee has visited the hospital <b> {0}</b> times this year. Check enrollee medical history to see more.", visits.Count());

                }
                //added else
                else
                {

                }

                if (enrollee.Isexpundged)
                {
                    expungedstring = "<span style='background-color:red;color:white'>Enrollee has been Expunged.</span>";

                }
                //added else
                else
                {

                }

            }
            //added else
            else
            {

            }

            //vetting protocol

            if (!string.IsNullOrEmpty(claim.diagnosticsIDString))
            {
                string[] split = claim.diagnosticsIDString.Split(',');

                protocolstring.AppendLine("The system detected some diagnoses tag ,I have stated out the guidelines using the vetting protocol V 1.0.");
                protocolstring.AppendLine("<br>");
                foreach (string vet in split)
                {
                    int vetid = 0;
                    int.TryParse(vet, out vetid);

                    VettingProtocol getvet = _claimsvc.getVettingPRotocol(vetid);

                    if (getvet != null)
                    {
                        protocolstring.AppendFormat("<summary><p><b>{0}</b></p> </summary>", getvet.Diagnosis.ToUpper());
                        protocolstring.AppendLine("<details>");
                        protocolstring.AppendLine("");
                        protocolstring.AppendFormat("<p>Investigations - {0} </p>", getvet.investigations);
                        protocolstring.AppendFormat("<p>Treatment - {0} </p>", getvet.treatment);
                        protocolstring.AppendFormat("<p>Specialist - {0} </p>", getvet.specialist);
                        protocolstring.AppendLine("</details>");
                    }
                    //added else
                    else
                    {
                        getvet = null;
                    }

                }
            }



            //get the next and previous







            IOrderedEnumerable<Claim> allarranged = claim.ClaimBatch.Claims.OrderBy(x => x.Id);
            Claim nextclaim = allarranged.Where(x => x.Id > Id).FirstOrDefault();
            Claim prevclaim = allarranged.Where(x => x.Id < Id).FirstOrDefault();

            ViewBag.nextclaim = "#";
            ViewBag.prevclaim = "#";

            if (nextclaim != null)
            {
                ViewBag.nextclaim = string.Format(_uniquePageService.GetUniquePage<VetSingleClaimsPage>().AbsoluteUrl + "?id={0}",
                                       nextclaim.Id);
            }
            if (prevclaim != null)
            {
                ViewBag.prevclaim = string.Format(_uniquePageService.GetUniquePage<VetSingleClaimsPage>().AbsoluteUrl + "?id={0}",
                                       prevclaim.Id);
            }

            ViewBag.hospitalvisit = visitstring;
            ViewBag.expungedstring = expungedstring;
            ViewBag.vettingprotocol = protocolstring.ToString();

            return View(page);
        }

        [HttpPost]
        public ActionResult VetSingleClaim(FormCollection claimsform, VetSingleClaimsPage page)
        {
            string FormNo = claimsform["FormNo"];
            string policynumber = claimsform["policynumber"];
            string evscode = claimsform["evscode"];
            string enrolleename = claimsform["enrolleename"];
            string companyname = claimsform["companyname"];
            string companyid = claimsform["CompanyID"];

            string enrolleesex = claimsform["enrolleesex"];
            string providerename = claimsform["providerename"];
            string doctorname = claimsform["doctorname"];
            string doctorID = claimsform["Idnumber"];
            string areaofspecialization = claimsform["areaofspecialization"];
            string Idnumber = claimsform["Idnumber"];
            string doctorsigned = claimsform["doctorsigned"];
            string doctorssigndate = claimsform["doctorssigndate"];
            string specialistname = claimsform["specialistname"];
            string areaofspecializationSpecialist = claimsform["areaofspecializationSpecialist"];

            string specialistphonenumber = claimsform["specialistphonenumber"];
            string specialistsigned = claimsform["specialistsigned"];
            string specialistsigndate = claimsform["specialistsigndate"];
            string dateofservice = claimsform["dateofservice"];
            string durationoftreatment = claimsform["durationoftreatment"];
            string dateofadmission = claimsform["dateofadmission"];
            string dateofdischarge = claimsform["dateofdischarge"];
            string diagnosis = claimsform["diagnosis"];
            string Treatmentgiven = claimsform["Treatmentgiven"];
            string treatmentcode = claimsform["treatmentcode"];
            string referralcode = claimsform["referralcode"];

            string servicebillCount = claimsform["servicebillCount"];
            string drugbillCount = claimsform["drugbillCount"];

            string Enrolleesigned = claimsform["Enrolleesigned"];
            string EnrolleesignedDate = claimsform["EnrolleesignedDate"];

            string AllprescibedDrugs = claimsform["AllprescibedDrugs"];
            string LaboratoryInvestigation = claimsform["LaboratoryInvestigation"];
            string Admission = claimsform["Admission"];
            string Feeding = claimsform["Feeding"];
            string Remark = claimsform["Remark"];

            string providerID = claimsform["providerID"];
            string enrolleeID = claimsform["enrolleeID"];
            string ClaimsBatchid = claimsform["claimBatchID"];
            string treatmentTag = claimsform["treatmentTag"];
            string ClaimsID = claimsform["ClaimsID"];
            //Convert the form


            int servicecount = string.IsNullOrEmpty(servicebillCount) ? 0 : Convert.ToInt32(servicebillCount.Split(',')[0]);
            int drugcount = string.IsNullOrEmpty(drugbillCount) ? 0 : Convert.ToInt32(drugbillCount.Split(',')[0]);
            Entities.ClaimBatch claimsbatch = _claimsvc.GetClaimBatch(Convert.ToInt32(ClaimsBatchid));




            if (servicecount == 0 && drugcount == 0)
            {
                _pageMessageSvc.SetErrormessage("The Claim has no bill in it.");
                return Redirect(string.Format(_uniquePageService.GetUniquePage<VetSingleClaimsPage>().AbsoluteUrl + "?id={0}",
                                    Convert.ToInt32(ClaimsID)));

            }
            Claim claimsobj = !string.IsNullOrEmpty(ClaimsID) ? claimsbatch.Claims.Single(x => x.Id == Convert.ToInt32(ClaimsID)) : new Claim();
            //_claimsvc.CleanClaim(claimsobj);
            //do your validation
            for (int i = 0; i <= servicecount; i++)
            {

                string ServiceID = claimsform[string.Format("servicebill{0}{1}", i, "serviceid")];
                string itemDscr = claimsform[string.Format("servicebill{0}{1}", i, "ItemDescription")];
                string itemDuration = claimsform[string.Format("servicebill{0}{1}", i, "Duration")];
                string itemRate = claimsform[string.Format("servicebill{0}{1}", i, "Rate")];
                string itemAmount = claimsform[string.Format("servicebill{0}{1}", i, "amount")];
                string ItemId = claimsform[string.Format("servicebill{0}{1}", i, "itemID")];

                //processed price
                string processedPrice = claimsform[string.Format("servicebill{0}{1}", i, "finalamount")];
                //remark
                string remark = claimsform[string.Format("servicebill{0}{1}", i, "remark")];

                Entities.ClaimService servicee = !string.IsNullOrEmpty(ServiceID) ? claimsobj.ServiceList.Single(x => x.Id == Convert.ToInt32(ServiceID)) : null;
                if (servicee != null && !string.IsNullOrEmpty(itemDscr) && !string.IsNullOrEmpty(itemDuration) && !string.IsNullOrEmpty(itemRate) && !string.IsNullOrEmpty(itemAmount))
                {

                    decimal Pamount = 0m;

                    decimal.TryParse(processedPrice, out Pamount);
                    servicee.VettedAmount = Pamount;
                    servicee.VettingComment = remark;
                    servicee.Claim = claimsobj;
                    servicee.status = ClaimsBillStatus.Vetted;
                }


            }

            //for drugs
            for (int i = 0; i <= drugcount; i++)
            {
                string DrugID = claimsform[string.Format("drugbill{0}{1}", i, "drugid")];

                string itemDscr = claimsform[string.Format("drugbill{0}{1}", i, "ItemDescription")];
                string itemDuration = claimsform[string.Format("drugbill{0}{1}", i, "Unit")];
                string itemRate = claimsform[string.Format("drugbill{0}{1}", i, "Rate")];
                string itemAmount = claimsform[string.Format("drugbill{0}{1}", i, "amount")];
                string ItemId = claimsform[string.Format("drugbill{0}{1}", i, "itemID")];

                //processed price
                string processedPrice = claimsform[string.Format("drugbill{0}{1}", i, "finalamount")];
                //remark
                string remark = claimsform[string.Format("drugbill{0}{1}", i, "remark")];
                ClaimDrug drugg = !string.IsNullOrEmpty(DrugID) ? claimsobj.DrugList.Single(x => x.Id == Convert.ToInt32(DrugID)) : null;
                if (drugg != null && !string.IsNullOrEmpty(itemDscr) && !string.IsNullOrEmpty(itemDuration) && !string.IsNullOrEmpty(itemRate) && !string.IsNullOrEmpty(itemAmount))
                {

                    decimal Pamount = 0m;
                    decimal.TryParse(processedPrice, out Pamount);
                    drugg.VettedAmount = Pamount;
                    drugg.VettingComment = remark;
                    drugg.Claim = claimsobj;



                }


            }


            //claimsobj.ProviderId = Convert.ToInt32(providerID);
            //claimsobj.Enrolleeid = Convert.ToInt32(enrolleeID);
            //claimsobj.enrolleeFullname = enrolleename.ToUpper();
            //claimsobj.enrolleeGender = enrolleesex;
            //claimsobj.enrolleeCompanyName = companyname;
            ////claimsobj.enrolleeCompanyId = Convert.ToInt32(companyid);
            //claimsobj.enrolleePolicyNumber = policynumber;
            //claimsobj.ClaimsSerialNo = FormNo;
            //claimsobj.EVSCode = evscode;
            //claimsobj.DoctorsName = doctorname;
            //claimsobj.DoctorsId = doctorID;
            //claimsobj.AreaOfSpecialty = areaofspecialization;
            //claimsobj.Signature = string.Empty;
            //claimsobj.DoctorSigned = doctorsigned == null || doctorsigned.ToLower() != "on" ? false : true;
            //claimsobj.DoctorSignecDate = !string.IsNullOrEmpty(doctorssigndate) ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(doctorssigndate.Substring(6, 4)), Convert.ToInt32(doctorssigndate.Substring(3, 2)), Convert.ToInt32(doctorssigndate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            //claimsobj.specialistName = specialistname;
            //claimsobj.specialistphonenumber = specialistphonenumber;
            //claimsobj.specialistSigned = specialistsigned == null || specialistsigned.ToLower() != "on" ? false : true;
            //claimsobj.specialistSignecDate = !string.IsNullOrEmpty(specialistsigndate) ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(specialistsigndate.Substring(6, 4)), Convert.ToInt32(specialistsigndate.Substring(3, 2)), Convert.ToInt32(specialistsigndate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            //claimsobj.AreaOfSpecialtyforspecialist = areaofspecializationSpecialist;
            //claimsobj.ServiceDate = !string.IsNullOrEmpty(dateofservice) ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofservice.Substring(6, 4)), Convert.ToInt32(dateofservice.Substring(3, 2)), Convert.ToInt32(dateofservice.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            //claimsobj.AdmissionDate = !string.IsNullOrEmpty(dateofadmission) ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofadmission.Substring(6, 4)), Convert.ToInt32(dateofadmission.Substring(3, 2)), Convert.ToInt32(dateofadmission.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            //claimsobj.DischargeDate = !string.IsNullOrEmpty(dateofdischarge) ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateofdischarge.Substring(6, 4)), Convert.ToInt32(dateofdischarge.Substring(3, 2)), Convert.ToInt32(dateofdischarge.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            //claimsobj.Durationoftreatment = durationoftreatment;
            //claimsobj.Diagnosis = diagnosis;
            //claimsobj.TreatmentGiven = Treatmentgiven;
            //claimsobj.TreatmentCode = treatmentcode;
            //claimsobj.referalCode = referralcode;
            //delete the old druglist

            //claimsobj.enrolleeSigned = Enrolleesigned == null || Enrolleesigned.ToLower() != "on" ? false : true;
            //claimsobj.EnrolleeSignDate = !string.IsNullOrEmpty(EnrolleesignedDate) ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(EnrolleesignedDate.Substring(6, 4)), Convert.ToInt32(EnrolleesignedDate.Substring(3, 2)), Convert.ToInt32(EnrolleesignedDate.Substring(0, 2))) : CurrentRequestData.Now.AddYears(-100);
            //claimsobj.AllprescibedDrugs = AllprescibedDrugs == null || AllprescibedDrugs.ToLower() != "on" ? false : true;
            //claimsobj.LaboratoryInvestigation = LaboratoryInvestigation == null || LaboratoryInvestigation.ToLower() != "on" ? false : true;
            //claimsobj.Feeding = Feeding == null || Feeding.ToLower() != "on" ? false : true;
            //claimsobj.Admission = Admission == null || Admission.ToLower() != "on" ? false : true;
            //claimsobj.Note = Remark;
            //claimsobj.Tag = (ClaimsTAGS)Enum.Parse(typeof(ClaimsTAGS), treatmentTag);
            Enrollee enrollee = _enrolleeService.GetEnrollee(claimsobj.Enrolleeid);



            if (enrollee != null)
            {
                Staff staff = _companyService.Getstaff(enrollee.Staffprofileid);

                string plancover =
               _companyService.GetCompanyPlan(_companyService.Getstaff(enrollee.Staffprofileid).StaffPlanid).
                   AllowChildEnrollee
                   ? "FAMILY"
                   : "INDIVIDUAL";
                string healthplan =

                    _planService.GetPlan(
                        _companyService.GetCompanyPlan(_companyService.Getstaff(enrollee.Staffprofileid).StaffPlanid).
                            Planid).Name.ToUpper() + " " + plancover;
                claimsobj.EnrolleePlan = healthplan;
            }
            else
            {
                claimsobj.EnrolleePlan = "Unknown";
            }
            //add the owner details
            //claimsobj.capturedBy = CurrentRequestData.CurrentUser.Id;
            //claimsobj.capturedName = _userservice.GetUser(CurrentRequestData.CurrentUser.Id).Name;
            claimsobj.status = ClaimsBillStatus.Vetted;
            claimsobj.vettedBy = CurrentRequestData.CurrentUser.Id;
            claimsobj.VettedDate = CurrentRequestData.Now;
            //added Vetter
            ViewBag.Vetter = CurrentRequestData.CurrentUser.Id;
            if (claimsbatch != null)
            {
                claimsobj.ClaimBatch = claimsbatch;
                int index = claimsbatch.Claims.IndexOf(claimsobj);
                claimsbatch.Claims[index] = claimsobj;
                bool resp = _claimsvc.UpdateClaimBatch(claimsbatch);
                _pageMessageSvc.SetSuccessMessage("Claim saved successfully.");
            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was an error saving claim.");
            }



            //redirect to the next  unvetted claim


            int hasunverted = claimsbatch.Claims.Where(x => x.IsDeleted == false && x.status == ClaimsBillStatus.Captured).Count();






            if (hasunverted < 1)
            {

                _pageMessageSvc.SetSuccessMessage("All bills have been vetted.");
                IOrderedEnumerable<Claim> allarranged = claimsbatch.Claims.OrderBy(x => x.Id);
                Claim nextclaim = allarranged.Where(x => x.Id > claimsobj.Id).FirstOrDefault();
                Claim prevclaim = allarranged.Where(x => x.Id < claimsobj.Id).FirstOrDefault();


                if (nextclaim != null)
                {
                    return Redirect(string.Format(_uniquePageService.GetUniquePage<VetSingleClaimsPage>().AbsoluteUrl + "?id={0}",
                                                          nextclaim.Id));
                }
                else
                {
                    _pageMessageSvc.SetSuccessMessage("You have vetted all the bills in this batch.");
                    return Redirect(string.Format(_uniquePageService.GetUniquePage<VetSingleClaimsPage>().AbsoluteUrl + "?id={0}",
                                                          claimsobj.Id));

                }

            }
            else
            {
                Claim item = claimsbatch.Claims.FirstOrDefault(x => x.IsDeleted == false && x.status == ClaimsBillStatus.Captured);
                return Redirect(string.Format(_uniquePageService.GetUniquePage<VetSingleClaimsPage>().AbsoluteUrl + "?id={0}",
                                       item.Id));
            }


        }


        [HttpGet]
        [ActionName("SubmitClaimReview")]
        public ActionResult SubmitClaimReview(int Id)
        {
            Entities.ClaimBatch claim = _claimsvc.GetClaimBatch(Id);

            Provider provider = _providerSvc.GetProvider(claim.ProviderId);
            ViewBag.Batchh = claim.Batch.ToString().ToUpper();
            ViewBag.hospital = provider.Name.ToUpper();
            ViewBag.TotalCaptured = claim.Claims.Count.ToString() + " Claims Forms.";
            ViewBag.TotalAmount = "₦ " + Convert.ToDecimal((claim.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + claim.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount)))).ToString("N");
            return PartialView("SubmitClaimReview", claim);

        }

        [HttpPost]
        [ActionName("SubmitClaimReview")]
        public ActionResult SubmitClaimReview(Entities.ClaimBatch claim)
        {

            if (claim != null)
            {
                Entities.ClaimBatch batch = _claimsvc.GetClaimBatch(claim.Id);
                batch.status = ClaimBatchStatus.Reviewing;
                batch.submitedReviewbyUser = CurrentRequestData.CurrentUser.Id;

                batch.SubmitedForReviewDate = CurrentRequestData.Now;


                if (_claimsvc.UpdateClaimBatch(batch))
                {
                    MrCMS.Entities.People.UserRole role = _rolesvc.GetRoleByName("INTERNAL CONTROL");
                    MrCMS.Entities.People.UserRole role2 = _rolesvc.GetRoleByName("INTERNAL CONTROL HEAD");

                    foreach (MrCMS.Entities.People.User boss in role.Users)
                    {
                        QueuedMessage emailmsg = new QueuedMessage();
                        emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                        emailmsg.ToAddress = boss.Email;
                        emailmsg.Subject = "New Bill submitted for review";
                        emailmsg.FromName = "NovoHub";
                        emailmsg.Body = string.Format("Dear Sir/Ma , A new bill with batch id {0} have been submitted for you to review.", batch.Id);
                        emailmsg.IsHtml = true;
                        _emailSender.AddToQueue(emailmsg);
                    }

                }
                _pageMessageSvc.SetSuccessMessage("Claim batch was successfully submitted for review.");
                return _uniquePageService.RedirectTo<VetClaimsPage>();

            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was a oroblem submiting claims batch.");
                return _uniquePageService.RedirectTo<VetClaimsPage>();
            }






        }


        [ActionName("ReviewClaimList")]
        public ActionResult ReviewClaimList(ReviewClaimsPage page)
        {
            int year = CurrentRequestData.Now.Year;
            List<GenericReponse2> yealist = new List<GenericReponse2>();


            for (int i = 0; i < 20; i++)
            {
                yealist.Add(new GenericReponse2 { Id = year - i, Name = (year - i).ToString() });
            }
            ViewBag.YearList = yealist;
            List<GenericReponse2> plist = _providerSvc.GetProviderNameList().OrderBy(x => x.Name).ToList();

            plist.Insert(0, new GenericReponse2 { Id = -1, Name = "All Providers" });
            ViewBag.PrvidersList = plist;
            IList<MrCMS.Entities.People.User> users = _chatservice.Getallusers();

            var userlist = from aereply in users
                           select new
                           {
                               Id = aereply.Id,
                               Name = aereply.Name,
                           };

            var userr = userlist.ToList();

            userr.Add(new { Id = -1, Name = "Select" });


            ViewBag.UserList = userr.OrderBy(x => x.Id).ToList();
            List<GenericReponse> batchlist = new List<GenericReponse>();
            foreach (string item in Enum.GetNames(typeof(Utility.ClaimBatch)))
            {
                batchlist.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(Utility.ClaimBatch), item)).ToString(), Name = item.ToUpper() });
            }
            ViewBag.BatchList = batchlist;
            ViewBag.Defaultdate = CurrentRequestData.Now.ToString("MM/dd/yyyy");

            IEnumerable<Zone> zones = _helperSvc.GetallZones();
            List<GenericReponse2> zonelist = new List<GenericReponse2>();

            foreach (Zone item in zones)
            {
                GenericReponse2 shii = new GenericReponse2()
                {
                    Id = item.Id,
                    Name = item.Name
                };
                zonelist.Add(shii);


            }

            zonelist.Insert(0, new GenericReponse2() { Id = -1, Name = "All Zones" });
            ViewBag.ZoneList = zonelist;


            return View(page);

        }

        public JsonResult GetClaimBatchReviewJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);

            string scrProvider = CurrentRequestData.CurrentContext.Request["Provider_list"];
            string month = CurrentRequestData.CurrentContext.Request["Month_list"];
            string year = CurrentRequestData.CurrentContext.Request["year"];
            string batch = CurrentRequestData.CurrentContext.Request["Batch"];

            string Zone = CurrentRequestData.CurrentContext.Request["Zone"];
            string claimbatchidd = CurrentRequestData.CurrentContext.Request["claimbatchidd"];
            int clambatchiddd = 0;
            int.TryParse(claimbatchidd, out clambatchiddd);
            int toltareccount = 0;
            int totalinresult = 0;
            int providercount = 0;
            decimal totalamount = 0m;
            decimal totalprocessed = 0m;
            IList<Entities.ClaimBatch> allincomingClaims = _claimsvc.QueryAllClaimBatch(out toltareccount, out totalinresult, string.Empty,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, Convert.ToInt32(scrProvider), Convert.ToInt32(month), Convert.ToInt32(year), (Utility.ClaimBatch)Enum.Parse(typeof(Utility.ClaimBatch), batch), Zone, 0, ClaimBatchStatus.Reviewing, out providercount, out totalamount, out totalprocessed, -1, clambatchiddd);

            Entities.ClaimBatch claimm = _claimsvc.GetClaimBatch(1);
            List<ClaimsBatchResponse> output = new List<ClaimsBatchResponse>();
            DateTime today = CurrentRequestData.Now;
            foreach (Entities.ClaimBatch areply in allincomingClaims)
            {

                string narration = "";
                try
                {
                    narration = Tools.GetClaimsNarrations(areply);
                }
                catch (Exception)
                {

                }


                Provider provider = _providerSvc.GetProvider(areply.ProviderId);
                ClaimsBatchResponse model = new ClaimsBatchResponse();
                model.Id = areply.Id;
                model.GroupName = provider != null ? _helperSvc.GetzonebyId(Convert.ToInt32(provider.State.Zone)).Name : "--";
                model.Provider = provider != null ? provider.Name : "--";
                model.PRoviderAddress = provider != null ? provider.Address : "--";
                model.narration = narration;
                model.Batch = areply.Batch == Utility.ClaimBatch.BatchA ? "Batch A" : "Batch B";
                model.deliveryCount = areply.IncomingClaims.Where(x => x.IsDeleted == false).ToList().Count.ToString();
                model.claimscount = areply.Claims.Count.ToString();
                model.totalAmount = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))).ToString("N");
                model.totalProccessed = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount))).ToString("N");
                model.difference = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount))) > 0 ? Convert.ToDecimal(Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))) - Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount)))).ToString("N") : "0.00";
                model.CapturedBy = areply.submitedReviewbyUser > 0 ? _userservice.GetUser(areply.submitedReviewbyUser).Name : "--";

                model.DateSubmitedForVetting = Convert.ToDateTime(areply.SubmitedForReviewDate).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                IncomingClaims income = areply.IncomingClaims.FirstOrDefault();
                if (income != null)
                {

                    model.Caption = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().caption) ? areply.IncomingClaims.FirstOrDefault().caption : "--";
                    model.Note = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().Note) ? areply.IncomingClaims.FirstOrDefault().Note : "--";
                    model.isSubmittedRemotely = areply.IncomingClaims.FirstOrDefault().IsRemoteSubmission;
                    model.deliverydate = Convert.ToDateTime(income.DateReceived).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                    string monthd = "";
                    if (!string.IsNullOrEmpty(income.month_string) && income.month_string.Split(',').Count() > 0)
                    {
                        foreach (string itemmm in income.month_string.Split(','))
                        {
                            model.month_string = model.month_string + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(itemmm)) + ",";
                        }
                        model.month_string = model.month_string + income.year.ToString();


                    }
                }
                output.Add(model);
            }


            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                totalcaptured = totalamount.ToString("N"),
                totalprocessed = totalprocessed.ToString("N"),
                difference = Convert.ToDecimal(totalamount - totalprocessed).ToString("N"),
                providercount = providercount,
                batch = Enum.GetName(typeof(Utility.ClaimBatch), Convert.ToInt32(batch)),
                aaData = response.Data
            });




        }

        public ActionResult DoApproveReview(FormCollection form)
        {
            string ids = form["hidden_selectedIDs"];
            string[] claimbatchlist = ids.Split(',');

            foreach (string item in claimbatchlist)
            {

                if (string.IsNullOrEmpty(item.Trim()))
                {
                    continue;
                }
                Entities.ClaimBatch batch = _claimsvc.GetClaimBatch(Convert.ToInt32(item));
                batch.status = ClaimBatchStatus.AwaitingApproval;
                batch.reviewDate = CurrentRequestData.Now;
                batch.reviewedBy = CurrentRequestData.CurrentUser.Id;

                batch.AuthorizationStatus = AuthorizationStatus.Pending;


                _claimsvc.UpdateClaimBatch(batch);


            }

            return _uniquePageService.RedirectTo<ReviewClaimsPage>();
        }



        [HttpGet]
        [ActionName("RejectToRevet")]
        public ActionResult RejectToRevet(int Id)
        {
            Entities.ClaimBatch claim = _claimsvc.GetClaimBatch(Id);

            Provider provider = _providerSvc.GetProvider(claim.ProviderId);
            ViewBag.Batchh = claim.Batch.ToString().ToUpper();
            ViewBag.hospital = provider.Name.ToUpper();
            ViewBag.TotalCaptured = claim.Claims.Count.ToString() + " Claims Forms.";
            ViewBag.TotalAmount = "₦ " + Convert.ToDecimal((claim.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + claim.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount)))).ToString("N");
            return PartialView("RejectClaimRevet", claim);

        }

        [HttpPost]
        [ActionName("RejectToRevet")]
        public ActionResult RejectToRevet(Entities.ClaimBatch claim)
        {

            if (claim != null)
            {
                Entities.ClaimBatch batch = _claimsvc.GetClaimBatch(claim.Id);
                batch.status = ClaimBatchStatus.Vetting;
                //batch.submitedReviewbyUser = CurrentRequestData.CurrentUser.Id;

                //batch.SubmitedForReviewDate = CurrentRequestData.Now;


                _claimsvc.UpdateClaimBatch(batch);


                MrCMS.Entities.People.User user = _userservice.GetUser(batch.submitedReviewbyUser);
                Provider provider = _providerSvc.GetProvider(batch.ProviderId);

                if (user != null)
                {
                    string narration = Tools.GetClaimsNarrations(batch);

                    QueuedMessage emailmsg = new QueuedMessage();
                    emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                    emailmsg.ToAddress = user.Email;
                    emailmsg.Subject = "Claim returned - " + provider.Name.ToUpper();
                    emailmsg.FromName = "NOVOHUB";
                    emailmsg.Body = string.Format("Dear {0}, the {1} claim with batch id {2} for {3} was returned to you for your attention.Thank you", user.FirstName, narration.ToUpper(), batch.Id, provider.Name.ToUpper());

                    _emailSender.AddToQueue(emailmsg);
                }

                _pageMessageSvc.SetSuccessMessage("Claim batch was successfully submitted for revet.");
                return _uniquePageService.RedirectTo<ReviewClaimsPage>();

            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was a problem submiting claims batch.");
                return _uniquePageService.RedirectTo<ReviewClaimsPage>();
            }






        }
        [HttpGet]
        public ActionResult returntoreview(int id)
        {
            if (id > 0)
            {
                Entities.ClaimBatch batch = _claimsvc.GetClaimBatch(id);
                batch.status = ClaimBatchStatus.Reviewing;
                //batch.submitedReviewbyUser = CurrentRequestData.CurrentUser.Id;

                //batch.SubmitedForReviewDate = CurrentRequestData.Now;


                _claimsvc.UpdateClaimBatch(batch);
                _pageMessageSvc.SetSuccessMessage("Claim batch was successfully submitted for review.");

                MrCMS.Entities.People.User user = _userservice.GetUser(batch.submitedReviewbyUser);
                Provider provider = _providerSvc.GetProvider(batch.ProviderId);

                if (user != null)
                {
                    string narration = Tools.GetClaimsNarrations(batch);

                    QueuedMessage emailmsg = new QueuedMessage();
                    emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                    emailmsg.ToAddress = user.Email;
                    emailmsg.Subject = "Claim returned - " + provider.Name.ToUpper();
                    emailmsg.FromName = "NOVOHUB";
                    emailmsg.Body = string.Format("Dear {0}, the {1} claim with batch id {2} for {3} was returned to you for your attention.Thank you", user.FirstName, narration.ToUpper(), batch.Id, provider.Name.ToUpper());

                    _emailSender.AddToQueue(emailmsg);
                }


                return _uniquePageService.RedirectTo<ReviewClaimsPage>();

            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was a problem sending claims back to review.");
                return _uniquePageService.RedirectTo<ReviewClaimsPage>();
            }

        }

        [ActionName("ReviewedClaimList")]
        public ActionResult ReviewedClaimList(ReviewedClaimsPage page)
        {
            int year = CurrentRequestData.Now.Year;
            List<GenericReponse2> yealist = new List<GenericReponse2>();


            for (int i = 0; i < 20; i++)
            {
                yealist.Add(new GenericReponse2 { Id = year - i, Name = (year - i).ToString() });
            }
            ViewBag.YearList = yealist;
            List<GenericReponse2> plist = _providerSvc.GetProviderNameList().OrderBy(x => x.Name).ToList();

            plist.Insert(0, new GenericReponse2 { Id = -1, Name = "All Providers" });
            ViewBag.PrvidersList = plist;
            IList<MrCMS.Entities.People.User> users = _chatservice.Getallusers();

            var userlist = from aereply in users
                           select new
                           {
                               Id = aereply.Id,
                               Name = aereply.Name,
                           };

            var userr = userlist.ToList();

            userr.Add(new { Id = -1, Name = "Select" });


            ViewBag.UserList = userr.OrderBy(x => x.Id).ToList();
            List<GenericReponse> batchlist = new List<GenericReponse>();
            foreach (string item in Enum.GetNames(typeof(Utility.ClaimBatch)))
            {
                batchlist.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(Utility.ClaimBatch), item)).ToString(), Name = item.ToUpper() });
            }
            ViewBag.BatchList = batchlist;
            ViewBag.Defaultdate = CurrentRequestData.Now.ToString("MM/dd/yyyy");

            IEnumerable<Zone> zones = _helperSvc.GetallZones();
            List<GenericReponse2> zonelist = new List<GenericReponse2>();

            foreach (Zone item in zones)
            {
                GenericReponse2 shii = new GenericReponse2()
                {
                    Id = item.Id,
                    Name = item.Name
                };
                zonelist.Add(shii);


            }

            zonelist.Insert(0, new GenericReponse2() { Id = -1, Name = "All Zones" });
            ViewBag.ZoneList = zonelist;


            return View(page);

        }

        public JsonResult GetClaimBatchReviewedJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);

            string scrProvider = CurrentRequestData.CurrentContext.Request["Provider_list"];
            string month = CurrentRequestData.CurrentContext.Request["Month_list"];
            string year = CurrentRequestData.CurrentContext.Request["year"];
            string batch = CurrentRequestData.CurrentContext.Request["Batch"];
            string Zone = CurrentRequestData.CurrentContext.Request["Zone"];

            string claimbatchidd = CurrentRequestData.CurrentContext.Request["claimbatchidd"];
            int clambatchiddd = 0;
            int.TryParse(claimbatchidd, out clambatchiddd);
            int toltareccount = 0;
            int totalinresult = 0;
            int providercount = 0;
            decimal totalamount = 0m;
            decimal totalprocessed = 0m;
            IList<Entities.ClaimBatch> allincomingClaims = _claimsvc.QueryAllClaimBatch(out toltareccount, out totalinresult, string.Empty,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, Convert.ToInt32(scrProvider), Convert.ToInt32(month), Convert.ToInt32(year), (Utility.ClaimBatch)Enum.Parse(typeof(Utility.ClaimBatch), batch), Zone, 0, ClaimBatchStatus.AwaitingApproval, out providercount, out totalamount, out totalprocessed, -1, clambatchiddd);

            Entities.ClaimBatch claimm = _claimsvc.GetClaimBatch(1);
            List<ClaimsBatchResponse> output = new List<ClaimsBatchResponse>();
            DateTime today = CurrentRequestData.Now;
            foreach (Entities.ClaimBatch areply in allincomingClaims)
            {
                string narration = "";
                try
                {
                    narration = Tools.GetClaimsNarrations(areply);
                }
                catch (Exception)
                {

                }

                Provider provider = _providerSvc.GetProvider(areply.ProviderId);

                ClaimsBatchResponse model = new ClaimsBatchResponse();
                model.Id = areply.Id;
                model.GroupName = provider != null ? _helperSvc.GetzonebyId(Convert.ToInt32(provider.State.Zone)).Name : "--";
                model.Provider = provider != null ? provider.Name : "--";
                model.PRoviderAddress = provider != null ? provider.Address : "--";
                model.narration = narration;
                model.Batch = areply.Batch == Utility.ClaimBatch.BatchA ? "Batch A" : "Batch B";

                model.deliveryCount = areply.IncomingClaims.Where(x => x.IsDeleted == false).ToList().Count.ToString();
                model.claimscount = areply.Claims.Count.ToString();
                model.totalAmount = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))).ToString("N");
                model.totalProccessed = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount))).ToString("N");
                model.difference = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount))) > 0 ? Convert.ToDecimal(Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))) - Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount)))).ToString("N") : "0.00";
                model.CapturedBy = areply.reviewedBy > 0 ? _userservice.GetUser(areply.reviewedBy).Name : "--";
                model.DateSubmitedForVetting = Convert.ToDateTime(areply.reviewDate).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                IncomingClaims income = areply.IncomingClaims.FirstOrDefault();
                if (income != null)
                {

                    model.Caption = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().caption) ? areply.IncomingClaims.FirstOrDefault().caption : "--";
                    model.Note = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().Note) ? areply.IncomingClaims.FirstOrDefault().Note : "--";
                    model.isSubmittedRemotely = areply.IncomingClaims.FirstOrDefault().IsRemoteSubmission;
                    model.deliverydate = Convert.ToDateTime(income.DateReceived).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                    string monthd = "";
                    if (!string.IsNullOrEmpty(income.month_string) && income.month_string.Split(',').Count() > 0)
                    {
                        foreach (string itemmm in income.month_string.Split(','))
                        {
                            model.month_string = model.month_string + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(itemmm)) + ",";
                        }
                        model.month_string = model.month_string + income.year.ToString();


                    }
                }
                output.Add(model);
            }


            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                totalcaptured = totalamount.ToString("N"),
                totalprocessed = totalprocessed.ToString("N"),
                difference = Convert.ToDecimal(totalamount - totalprocessed).ToString("N"),
                providercount = providercount,
                batch = Enum.GetName(typeof(Utility.ClaimBatch), Convert.ToInt32(batch)),
                aaData = response.Data
            });




        }


        [HttpGet]
        [ActionName("ExportMemo")]
        public ActionResult ExportMemo(int mode)
        {
            Claim claim = new Claim();

            int year = CurrentRequestData.Now.Year;
            List<GenericReponse2> yealist = new List<GenericReponse2>();
            ViewBag.Mode = mode;

            for (int i = 0; i < 20; i++)
            {
                yealist.Add(new GenericReponse2 { Id = year - i, Name = (year - i).ToString() });
            }
            ViewBag.YearList = yealist;
            List<GenericReponse2> plist = _providerSvc.GetProviderNameList().OrderBy(x => x.Name).ToList();
            ViewBag.MyProvidersList = _providerSvc.GetProviderNameList().OrderBy(x => x.Name);
            plist.Insert(0, new GenericReponse2 { Id = -1, Name = "All Providers" });
            ViewBag.PrvidersList = plist;
            //var users = _chatservice.Getallusers();

            //var userlist = from aereply in users
            //               select new
            //               {
            //                   Id = aereply.Id,
            //                   Name = aereply.Name,
            //               };

            //var userr = userlist.ToList();

            //userr.Add(new { Id = -1, Name = "Select" });


            //ViewBag.UserList = userr.OrderBy(x => x.Id).ToList();
            List<GenericReponse> batchlist = new List<GenericReponse>();

            foreach (string item in Enum.GetNames(typeof(Utility.ClaimBatch)))
            {
                batchlist.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(Utility.ClaimBatch), item)).ToString(), Name = item.ToUpper() });
            }
            ViewBag.BatchList = batchlist;
            ViewBag.Defaultdate = CurrentRequestData.Now.ToString("MM/dd/yyyy");

            IEnumerable<Zone> zones = _helperSvc.GetallZones();
            List<GenericReponse2> zonelist = new List<GenericReponse2>();
            GenericReponse2 shiii = new GenericReponse2()
            {
                Id = -1,
                Name = "All"
            };
            zonelist.Add(shiii);
            foreach (Zone item in zones)
            {
                GenericReponse2 shii = new GenericReponse2()
                {
                    Id = item.Id,
                    Name = item.Name
                };
                zonelist.Add(shii);


            }

            //zonelist.Insert(0, new Utility.GenericReponse2() { Id = -1, Name = "All Zones" });
            ViewBag.ZoneList = zonelist;

            return PartialView("ExportMemo", claim);

        }

        [HttpPost]
        [ActionName("ExportMemo")]
        public ActionResult ExportMemo(FormCollection form)
        {
            string batchid = form["Batch"];
            string Provider_list = form["Provider_list"];
            string Month_list = form["Month_list"];
            string Year = form["Year"];
            string Zone = form["Zone"];
            string mode = form["mode"];

            int toltareccount = 0;
            int totalinresult = 0;
            int displayStart = 0;
            int displayLength = 5000;
            int providercount = 0;
            decimal totalprocessed = 0m;
            decimal totalamount = 0m;
            ClaimBatchStatus themode = ClaimBatchStatus.AwaitingApproval;

            int modeint = 0;
            int.TryParse(mode, out modeint);
            if (modeint == 2)
            {
                //awitingpayment
                themode = ClaimBatchStatus.AwaitingPayment;

            }

            IOrderedEnumerable<Entities.ClaimBatch> allincomingClaims = _claimsvc.QueryAllClaimBatch(out toltareccount, out totalinresult, string.Empty,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), "", "", -1, Convert.ToInt32(Month_list), Convert.ToInt32(Year), (Utility.ClaimBatch)Enum.Parse(typeof(Utility.ClaimBatch), batchid), Zone, 0, themode, out providercount, out totalamount, out totalprocessed, -1, -1).OrderBy(x => x.CreatedOn);







            //var narrartion = Utility.Tools.GetClaimsNarrations(allincomingClaims.FirstOrDefault());

            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath, "App_Data");
            string foldername = Guid.NewGuid().ToString();
            string fullpath = Path.Combine(appdatafolder, foldername);
            Directory.CreateDirectory(fullpath);


            //write  the excels 

            //export all enrollees to excel
            DataTable test = new DataTable();
            test.Columns.Add("S/N", typeof(string));
            test.Columns.Add("NAME OF PROVIDER", typeof(string));
            test.Columns.Add("MONTH", typeof(string));
            test.Columns.Add("AMOUNT SUBMITTED", typeof(decimal));
            test.Columns.Add("PROCESSED AMOUNT", typeof(decimal));
            test.Columns.Add("DIFFERENCE", typeof(decimal));
            int count = 1;
            decimal totalsumited = 0m;
            decimal totalproccessed = 0m;
            decimal totaldiff = 0m;
            string batchName = Enum.GetName(typeof(Utility.ClaimBatch), Convert.ToInt32(batchid));
            foreach (Entities.ClaimBatch batch in allincomingClaims)
            {
                Provider provider = _providerSvc.GetProvider(batch.ProviderId);
                string providername = provider.Name.ToUpper();
                string month = Tools.GetClaimsNarrations(batch);
                decimal Iamount = 0m;
                decimal Pamount = 0m;
                decimal Difference = 0m;
                foreach (Claim item in batch.Claims)
                {
                    Iamount += Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount) + item.ServiceList.Sum(x => x.InitialAmount));
                    Pamount += Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount) + item.ServiceList.Sum(x => x.VettedAmount));
                    Difference = Iamount - Pamount;

                }


                // totalamount+= Iamount;
                totalproccessed += Pamount;
                totaldiff = totaldiff + Difference;


                test.Rows.Add(count.ToString(), providername, month, Iamount, Pamount, Difference);



                count++;
            }


            test.Rows.Add(string.Empty, string.Empty, "TOTAL", totalamount, totalproccessed, totaldiff);


            string the_month = "ALL REVIEWED IN ";

            if (Convert.ToInt32(Month_list) > -1)
            {
                //jessica said it must be a month ahead.
                the_month = CurrentRequestData.CultureInfo.DateTimeFormat.GetMonthName(Convert.ToInt32(Month_list));

            }
            string zonename = "";

            if (!string.IsNullOrEmpty(Zone))


            {



                foreach (string item in Zone.Split(','))
                {
                    int intt = -1;
                    int.TryParse(item, out intt);

                    if (intt > -1)
                    {


                        Zone shiit = _helperSvc.GetzonebyId(intt);

                        if (shiit != null)
                        {
                            zonename = zonename + " " + shiit.Name.ToUpper() + ",";
                        }
                    }
                }


            }

            string str = string.Format("{4} REGION FEE FOR SERVICES PAYMENT (N{3})  FOR {0} {1} {2} ", the_month, Year, batchName, totalproccessed.ToString("N"), zonename);




            byte[] excelarray = Tools.DumpExcelGetByte(test, str);

            //write excel to folder 

            System.IO.File.WriteAllBytes(Path.Combine(fullpath, foldername + ".xlsx"), excelarray);

            //zip folder and send to client

            //string zipPath = Path.Combine(appdatafolder, string.Format("{0}.zip", foldername));

            //ZipFile.CreateFromDirectory(fullpath, zipPath);

            //send back to user
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=ExportedMemo.xlsx");
            Response.BinaryWrite(System.IO.File.ReadAllBytes(Path.Combine(fullpath, foldername + ".xlsx")));


            //return _uniquePageService.RedirectTo<ReviewedClaimsPage>();
            return null;
        }

        [ActionName("ExportPaymentAdvice")]
        public ActionResult ExportPaymentAdvice(int id)
        {
            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath, "App_Data");
            string foldername = Guid.NewGuid().ToString();
            string fullpath = Path.Combine(appdatafolder, foldername);
            Directory.CreateDirectory(fullpath);
            Entities.ClaimBatch claimbatch = _claimsvc.GetClaimBatch(Convert.ToInt32(id));
            DataTable test = new DataTable();

            test.Columns.Add("S/N", typeof(string));

            test.Columns.Add("Client Name", typeof(string));
            test.Columns.Add("Company", typeof(string));
            test.Columns.Add("Policy Number", typeof(string));
            test.Columns.Add("Health Plan", typeof(string));
            test.Columns.Add("Encounter Date", typeof(string));
            test.Columns.Add("Date Received", typeof(string));
            test.Columns.Add("Diagnosis", typeof(string));
            test.Columns.Add("Class", typeof(string));
            test.Columns.Add("Amount Submited", typeof(decimal));
            test.Columns.Add("Amount Processed", typeof(decimal));
            test.Columns.Add("Difference", typeof(decimal));
            test.Columns.Add("Comment", typeof(string));

            string datereceived = "";
            if (claimbatch != null)
            {
                Provider provider = _providerSvc.GetProvider(claimbatch.ProviderId);
                IncomingClaims firstcoming = claimbatch.IncomingClaims.FirstOrDefault();
                string providername = provider.Name.ToUpper();
                string provideraddress = provider.Address.ToUpper();
                decimal totalsubmittedSum = 0m;
                decimal totalProccessedSum = 0m;
                decimal totaldiffenrenceSum = 0m;


                if (firstcoming != null)
                {
                    datereceived = Convert.ToDateTime(firstcoming.DateReceived).ToLongDateString();
                }


                int count = 1;

                foreach (Claim item in claimbatch.Claims)
                {
                    string ClaimsFormNo = !string.IsNullOrEmpty(item.ClaimsSerialNo) ? item.ClaimsSerialNo : "";
                    string EnrolleeName = !string.IsNullOrEmpty(item.enrolleeFullname) ? item.enrolleeFullname : "";
                    string PolicyNumber = !string.IsNullOrEmpty(item.enrolleePolicyNumber) ? item.enrolleePolicyNumber : "";
                    string HealthPlan = !string.IsNullOrEmpty(item.EnrolleePlan) ? item.EnrolleePlan : "";
                    string Company = !string.IsNullOrEmpty(item.enrolleeCompanyName) ? item.enrolleeCompanyName : "";
                    string EncounterDate = Convert.ToDateTime(item.ServiceDate).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                    string Diagnosis = !string.IsNullOrEmpty(item.Diagnosis) ? item.Diagnosis : "";
                    string DurationofTreatment = !string.IsNullOrEmpty(item.Durationoftreatment) ? item.Durationoftreatment : "";
                    string TreatmentTag = Enum.GetName(typeof(ClaimsTAGS), item.Tag);
                    string ServiceCharge = Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount)).ToString("N");
                    string DrugCharge = Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)).ToString("N");
                    string TotalCharge = Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount))).ToString("N");
                    string ProcessedCharge = Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.VettedAmount))).ToString("N");
                    string ChargeDifference = @Convert.ToDecimal((Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount))) - (Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.VettedAmount)))).ToString("N");
                    string comment = "";
                    if (TotalCharge != ProcessedCharge)
                    {
                        //vetting went down
                        //loop through vetting

                        foreach (Entities.ClaimService service in item.ServiceList)
                        {
                            if (!string.IsNullOrEmpty(service.VettingComment))
                            {
                                comment = comment + service.ServiceName.ToUpper() + " - " + service.VettingComment + ", ";

                            }

                        }

                        foreach (ClaimDrug drug in item.DrugList)
                        {
                            if (!string.IsNullOrEmpty(drug.VettingComment))
                            {
                                comment = comment + drug.DrugName.ToUpper() + " - " + drug.VettingComment + ", ";

                            }
                        }
                    }



                    test.Rows.Add(count.ToString(), EnrolleeName.ToUpper(), Company.ToUpper(), PolicyNumber, HealthPlan, EncounterDate, datereceived, Diagnosis.ToUpper(), TreatmentTag, TotalCharge, ProcessedCharge, ChargeDifference, comment);



                    count++;

                    totalsubmittedSum += Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount)));
                    totalProccessedSum += Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.VettedAmount)));
                    totaldiffenrenceSum = totalsubmittedSum - totalProccessedSum;

                }
                test.Rows.Add(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "TOTAL", totalsubmittedSum, totalProccessedSum, totaldiffenrenceSum, string.Empty);

                byte[] excelarray = Tools.DumpExcelGetByteAdvice(test, providername, provideraddress);

                //write excel to folder

                System.IO.File.WriteAllBytes(Path.Combine(fullpath, foldername + ".xlsx"), excelarray);

                //zip folder and send to client

                //string zipPath = Path.Combine(appdatafolder, string.Format("{0}.zip", foldername));

                //ZipFile.CreateFromDirectory(fullpath, zipPath);

                //send back to user
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=ExportedPayAdvice.xlsx");
                Response.BinaryWrite(System.IO.File.ReadAllBytes(Path.Combine(fullpath, foldername + ".xlsx")));



            }
            //return _uniquePageService.RedirectTo<ReviewedClaimsPage>();
            return null;
        }
        [ActionName("ExportBatchAnalysis")]
        public ActionResult ExportBatchAnalysis(FormCollection form)
        {
            string batchid = form["BatchClaimAnalysis"];
            string Month_list = form["Month_BatchClaimAnalysis"];
            string Year = form["Year_BatchClaimAnalysis"];
            //var modee = form["mode_BatchClaimAnalysis"];
            //var Zone = form["Zone"];


            int toltareccount = 0;
            int totalinresult = 0;
            int displayStart = 0;
            int displayLength = 5000;
            int providercount = 0;
            decimal totalprocessed = 0m;
            decimal totalamount = 0m;
            ClaimBatchStatus themode = ClaimBatchStatus.AwaitingApproval;



            IOrderedEnumerable<Entities.ClaimBatch> allincomingClaims = _claimsvc.QueryAllClaimBatch(out toltareccount, out totalinresult, string.Empty,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), "", "", -1, Convert.ToInt32(Month_list), Convert.ToInt32(Year), (Utility.ClaimBatch)Enum.Parse(typeof(Utility.ClaimBatch), batchid), string.Empty, 0, themode, out providercount, out totalamount, out totalprocessed, -1, -1).OrderBy(x => x.CreatedOn);


            DataTable claimdatabase = new DataTable();

            claimdatabase.Columns.Add("S/N", typeof(string));

            claimdatabase.Columns.Add("Client Name", typeof(string));
            claimdatabase.Columns.Add("Company", typeof(string));
            claimdatabase.Columns.Add("Policy Number", typeof(string));
            claimdatabase.Columns.Add("Health Plan", typeof(string));
            claimdatabase.Columns.Add("Encounter Date", typeof(string));
            claimdatabase.Columns.Add("Date Received", typeof(string));
            claimdatabase.Columns.Add("Diagnosis", typeof(string));
            claimdatabase.Columns.Add("Class", typeof(string));
            claimdatabase.Columns.Add("Amount Submited", typeof(decimal));
            claimdatabase.Columns.Add("Amount Processed", typeof(decimal));
            claimdatabase.Columns.Add("Difference", typeof(decimal));
            claimdatabase.Columns.Add("Comment", typeof(string));
            byte[] excelarray;
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = null;
                Dictionary<int, ExcelWorksheet> zonediction = new Dictionary<int, ExcelWorksheet>();
                Dictionary<int, int> SheetCount = new Dictionary<int, int>();
                IEnumerable<Zone> zones = _helperSvc.GetallZones();
                foreach (Zone zone in zones)
                {
                    zonediction.Add(zone.Id, pck.Workbook.Worksheets.Add(zone.Name.ToUpper()));
                    SheetCount.Add(zone.Id, 1);
                }

                int totalrowcount = 1;
                string providername = "";
                string provideraddress = "";
                int zoneiddd = -1;
                foreach (Entities.ClaimBatch claimbatch in allincomingClaims.OrderBy(x => x.ProviderId))
                {

                    string datereceived = "";
                    if (claimbatch != null)
                    {
                        Provider provider = _providerSvc.GetProvider(claimbatch.ProviderId);
                        ws = zonediction[Convert.ToInt32(provider.State.Zone)];
                        zoneiddd = Convert.ToInt32(provider.State.Zone);
                        totalrowcount = SheetCount[Convert.ToInt32(provider.State.Zone)];
                        IncomingClaims firstcoming = claimbatch.IncomingClaims.FirstOrDefault();
                        providername = provider.Name.ToUpper();
                        provideraddress = provider.Address.ToUpper();
                        decimal totalsubmittedSum = 0m;
                        decimal totalProccessedSum = 0m;
                        decimal totaldiffenrenceSum = 0m;


                        if (firstcoming != null)
                        {
                            datereceived = Convert.ToDateTime(firstcoming.DateReceived).ToLongDateString();
                        }


                        int countit = 1;

                        foreach (Claim item in claimbatch.Claims)
                        {
                            string ClaimsFormNo = !string.IsNullOrEmpty(item.ClaimsSerialNo) ? item.ClaimsSerialNo : "";
                            string EnrolleeName = !string.IsNullOrEmpty(item.enrolleeFullname) ? item.enrolleeFullname : "";
                            string PolicyNumber = !string.IsNullOrEmpty(item.enrolleePolicyNumber) ? item.enrolleePolicyNumber : "";
                            string HealthPlan = !string.IsNullOrEmpty(item.EnrolleePlan) ? item.EnrolleePlan : "";
                            string Company = !string.IsNullOrEmpty(item.enrolleeCompanyName) ? item.enrolleeCompanyName : "";
                            string EncounterDate = Convert.ToDateTime(item.ServiceDate).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                            string Diagnosis = !string.IsNullOrEmpty(item.Diagnosis) ? item.Diagnosis : "";
                            string DurationofTreatment = !string.IsNullOrEmpty(item.Durationoftreatment) ? item.Durationoftreatment : "";
                            string TreatmentTag = Enum.GetName(typeof(ClaimsTAGS), item.Tag);
                            string ServiceCharge = Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount)).ToString("N");
                            string DrugCharge = Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)).ToString("N");
                            string TotalCharge = Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount))).ToString("N");
                            string ProcessedCharge = Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.VettedAmount))).ToString("N");
                            string ChargeDifference = @Convert.ToDecimal((Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount))) - (Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.VettedAmount)))).ToString("N");
                            string comment = "";
                            if (TotalCharge != ProcessedCharge)
                            {
                                //vetting went down
                                //loop through vetting

                                foreach (Entities.ClaimService service in item.ServiceList)
                                {
                                    if (!string.IsNullOrEmpty(service.VettingComment))
                                    {
                                        comment = comment + service.ServiceName.ToUpper() + " - " + service.VettingComment + ", ";

                                    }

                                }

                                foreach (ClaimDrug drug in item.DrugList)
                                {
                                    if (!string.IsNullOrEmpty(drug.VettingComment))
                                    {
                                        comment = comment + drug.DrugName.ToUpper() + " - " + drug.VettingComment + ", ";

                                    }
                                }
                            }



                            claimdatabase.Rows.Add(countit.ToString(), EnrolleeName.ToUpper(), Company.ToUpper(), PolicyNumber, HealthPlan, EncounterDate, datereceived, Diagnosis.ToUpper(), TreatmentTag, TotalCharge, ProcessedCharge, ChargeDifference, comment);
                            //totalrowcount++;


                            countit++;

                            totalsubmittedSum += Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount)));
                            totalProccessedSum += Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.VettedAmount)));
                            totaldiffenrenceSum = totalsubmittedSum - totalProccessedSum;

                        }

                        claimdatabase.Rows.Add(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "TOTAL", totalsubmittedSum, totalProccessedSum, totaldiffenrenceSum, string.Empty);

                        //add two empty rows



                    }

                    //add two rows
                    //if (totalrowcount > 500)
                    //{
                    //    //change the sheet when exceeded
                    //    var nameofsheet = ws.Name;
                    //    nameofsheet = ws.Name + "-CT" + Guid.NewGuid().ToString();
                    //    zonediction[zoneiddd] = pck.Workbook.Worksheets.Add(nameofsheet.ToUpper());

                    //    totalrowcount = 1;

                    //    ws = zonediction[zoneiddd];

                    //}

                    int diff = totalrowcount;



                    //diff = diff + 2;
                    //Add the provider header
                    ws.Cells["A" + (diff).ToString()].Value = providername + " - " + provideraddress.ToLower();
                    ws.Cells["A" + (diff).ToString()].Style.Font.Bold = true;
                    ws.Cells["A" + (diff).ToString()].Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    ws.Cells["A" + (diff).ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                    ws.Cells["A" + (diff).ToString()].Style.Font.Color.SetColor(Color.White);


                    //leave two spaces
                    // diff = diff + 2;
                    //diff = diff + 1;



                    if (true)
                    {
                        using (ExcelRange rng = ws.Cells[diff + 1, 1, diff + 1, 13])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                            rng.Style.Font.Color.SetColor(Color.White);
                        }



                        ws.Cells["A" + (diff + 1).ToString()].LoadFromDataTable(claimdatabase, true);

                    }


                    totalrowcount = totalrowcount + claimdatabase.Rows.Count + 3;
                    SheetCount[zoneiddd] = totalrowcount;


                    claimdatabase.Rows.Clear();
                }
                //var narrartion = Utility.Tools.GetClaimsNarrations(allincomingClaims.FirstOrDefault());
                excelarray = pck.GetAsByteArray();
            }
            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath, "App_Data");
            string foldername = Guid.NewGuid().ToString();
            string fullpath = Path.Combine(appdatafolder, foldername);
            Directory.CreateDirectory(fullpath);

            //var excelarray = Utility.Tools.DumpExcelGetByte(claimdatabase);

            //write excel to folder

            System.IO.File.WriteAllBytes(Path.Combine(fullpath, foldername + ".xlsx"), excelarray);

            //send back to user
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=ExportedMemo.xlsx");
            Response.BinaryWrite(System.IO.File.ReadAllBytes(Path.Combine(fullpath, foldername + ".xlsx")));


            //return _uniquePageService.RedirectTo<ReviewedClaimsPage>();
            return null;
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DoApproveForPayment(FormCollection form)
        {

            string ids = form["hidden_selectedIDs"];
            string paymentbatchid = form["paymentbatchlst"];


            string[] claimbatchlist = ids.Split(',');
            int count = 0;
            int providercount = 0;
            decimal totalamountapproved = 0m;
            int payid = 0;

            if (string.IsNullOrEmpty(paymentbatchid) || int.TryParse(paymentbatchid, out payid) == false)
            {
                _pageMessageSvc.SetErrormessage("You have not selected a payment batch.");

                return _uniquePageService.RedirectTo<ReviewedClaimsPage>();
            }
            PaymentBatch paymentbatch = _claimsvc.getpaymentbatch(payid);


            if (paymentbatch == null)
            {
                _pageMessageSvc.SetErrormessage("You have selected an invalid payment batch.");

                return _uniquePageService.RedirectTo<ReviewedClaimsPage>();
            }
            foreach (string item in claimbatchlist)
            {

                if (string.IsNullOrEmpty(item.Trim()))
                {
                    continue;
                }
                Entities.ClaimBatch batch = _claimsvc.GetClaimBatch(Convert.ToInt32(item));


                foreach (Claim claim in batch.Claims)
                {
                    totalamountapproved = totalamountapproved + Convert.ToDecimal(claim.DrugList.Sum(x => x.VettedAmount) + Convert.ToDecimal(claim.ServiceList.Sum(x => x.VettedAmount)));

                }

                batch.status = ClaimBatchStatus.AwaitingPayment;
                batch.AuthorizationStatus = AuthorizationStatus.Authorized;
                batch.AuthorizedBy = CurrentRequestData.CurrentUser.Id;
                batch.AuthorizedDate = CurrentRequestData.Now;




                paymentbatch.ClaimBatchList.Add(batch);
                batch.paymentbatch = paymentbatch;


                _claimsvc.UpdateClaimBatch(batch);
                _claimsvc.addPaymentBatch(paymentbatch);

                count++;


            }


            _pageMessageSvc.SetSuccessMessage(string.Format("You have approved a Total of ₦ {0} for Payment.", totalamountapproved.ToString("N")));

            return _uniquePageService.RedirectTo<ReviewedClaimsPage>();
        }


        public ActionResult Authorization(AuthorizationPage page)
        {


            IList<Company> list = _companyService.GetallCompany();
            list.Add(new Company() { Id = -1, Name = "--Select--" });
            ViewBag.CompanyList = list.AsEnumerable();
            IList<GenericReponse2> providers = _providerSvc.GetProviderNameList();

            ViewBag.providerlistsss = new SelectList(providers.AsEnumerable(), "Id", "Name");

            return View(page);

        }
        [HttpGet]
        public ActionResult GenerateAuthorizationCode()
        {
            string code = _helperSvc.GenerateAuthorizactionCode();
            bool exist = _claimsvc.getAuthorizationByCode(code) != null;
            ViewBag.AuthCode = "";
            IList<MrCMS.Entities.People.User> users = _chatservice.Getallusers();

            var userlist = from aereply in users
                           select new
                           {
                               Id = aereply.Id,
                               Name = aereply.Name,
                           };

            var userr = userlist.ToList();





            ViewBag.usersList = userr.OrderBy(x => x.Id).ToList().OrderBy(x => x.Name);

            List<Company> companylist = _companyService.GetallCompany().OrderBy(x => x.Name).ToList();
            companylist.Insert(0, new Company() { Id = -1, Name = "All Companies" });
            ViewBag.Companylist = companylist;
            ViewBag.PrvidersList = _providerSvc.GetProviderNameList().OrderBy(x => x.Name);
            if (!exist)
            {
                //
                ViewBag.AuthCode = code.ToUpper();

            }

            return PartialView("GenerateAuthorizationCode");
        }

        [HttpPost]
        //this was made for the design quick design
        public ActionResult GenerateAuthorizationCode(FormCollection form)
        {
            string Id = form["Id"];


            string authorizationcode = form["authorizationcode"];
            string provider_list = form["provider_list"];
            string policynumber = form["policynumber"];
            string enrolleeName = form["enrolleeName"];
            string company_list = form["company_list"];
            string Plan = form["Plan"];
            string enrolleeAge = form["enrolleeAge"];
            string Diagnosis = form["Diagnosis"];
            string category = form["TypeofAuthorization"];
            string authorizedby = form["User_list"];
            string Note_txt = form["Note_txt"];
            string enrolleeID = form["enrolleeId"];

            string RequesterName = form["RequesterName"];
            string RequesterPhone = form["RequesterPhone"];

            string isadmission = form["isadmission"];
            string admissionDate = form["admissiondate"];
            string dischargeDate = form["dischargedate"];
            string numofdays = form["numodays"];
            string Whatyouauthorized = form["treatmentAuthorized"];

            string nhisswitch = form["NHISSwitch"];

            AuthorizationCode exist = _claimsvc.getAuthorizationByCode(authorizationcode);

            if (exist != null && string.IsNullOrEmpty((Id)))
            {
                _pageMessageSvc.SetErrormessage("There was a problem logging authorization code ,kindly check the value you entered.");


                return _uniquePageService.RedirectTo<AuthorizationPage>();
            }

            //var authorizationcode = form["authorizationcode"];
            int age = 0;
            int En_id = -1;
            AuthorizationCode authCode = new AuthorizationCode();

            if (Convert.ToInt32(company_list) < 0)
            {

                _pageMessageSvc.SetErrormessage("Kindly select a company from the list before saving.");

                return _uniquePageService.RedirectTo<AuthorizationPage>();

            }
            int iddint = 0;
            if (!string.IsNullOrEmpty((Id)) && int.TryParse(Id, out iddint))
            {


                authCode = _claimsvc.getAuthorization(iddint);
            }
            int provideridd = 0;
            if (!string.IsNullOrEmpty(authorizationcode) && !string.IsNullOrEmpty(provider_list) && int.TryParse(provider_list, out provideridd))
            {

                authCode.authorizationCode = authorizationcode;
                authCode.provider = provideridd;
                authCode.policyNumber = policynumber;
                authCode.enrolleeName = enrolleeName;
                authCode.EnrolleeCompany = company_list;
                authCode.Plan = Plan;
                authCode.requestername = RequesterName;
                authCode.requesterphone = RequesterPhone;
                authCode.enrolleeAge = int.TryParse(enrolleeAge, out age) ? age : age;
                authCode.Diagnosis = Diagnosis;
                authCode.TypeofAuthorization = category;
                int authbyint = 0;
                int.TryParse(authorizedby, out authbyint);
                if (!string.IsNullOrEmpty(Whatyouauthorized))
                {
                    authCode.treatmentAuthorized = Whatyouauthorized;

                }
                authCode.Authorizedby = authbyint;
                authCode.Note = Note_txt;
                authCode.IsNHIS = nhisswitch == "on";
                authCode.enrolleeID = int.TryParse(enrolleeID, out En_id) ? En_id : En_id;
                string opt = isadmission.Split(',')[0];
                if (opt == "true")
                {
                    //isadmission
                    DateTime admisionday = CurrentRequestData.Now;
                    DateTime dischargedate = CurrentRequestData.Now;
                    if (string.IsNullOrEmpty(Id))
                    {


                        authCode.admissionStatus = admissionStatus.Admitted;
                    }

                    if (!string.IsNullOrEmpty(admissionDate))
                    {
                        admisionday = !string.IsNullOrEmpty(admissionDate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(admissionDate.Substring(6, 4)), Convert.ToInt32(admissionDate.Substring(3, 2)), Convert.ToInt32(admissionDate.Substring(0, 2))) : CurrentRequestData.Now;
                        authCode.AdmissionDate = admisionday;
                    }



                    if (!string.IsNullOrEmpty(dischargeDate))
                    {
                        dischargedate = !string.IsNullOrEmpty(dischargeDate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dischargeDate.Substring(6, 4)), Convert.ToInt32(dischargeDate.Substring(3, 2)), Convert.ToInt32(dischargeDate.Substring(0, 2))) : CurrentRequestData.Now;
                        authCode.DischargeDate = dischargedate;
                        authCode.admissionStatus = admissionStatus.Discharged;
                    }


                    authCode.Isadmission = true;
                    int numofdaysint = 0;
                    int.TryParse(numofdays, out numofdaysint);

                    authCode.DaysApprovded = numofdaysint;





                }
                else
                {
                    authCode.Isadmission = false;
                }
                if (string.IsNullOrEmpty(Id))
                {


                    authCode.generatedby = CurrentRequestData.CurrentUser.Id;
                }
                else
                {
                    authCode.Id = iddint;
                }


            }

            if (_claimsvc.addAuthorization(authCode))
            {



                _pageMessageSvc.SetSuccessMessage("Authorization Code was logged successfully.");
            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was a problem logging authorization code ,kindly check the value you entered.");
            }

            return _uniquePageService.RedirectTo<AuthorizationPage>();


        }
        public string GenerateAuthorizationCode2(FormCollection form)
        {
            if(form == null)
            {
                throw new Exception("form object is null");
            }

            string Id = form["Id"];

            #region -- initialize variables -----
            string authorizationcode = form["auth_authorizationcode"];
            string provider_list = form["auth_provider_list"];
            string policynumber = form["auth_policynumber"];
            string enrolleeName = form["auth_enrolleeName"];
            string company_list = form["auth_company_list"];
            string Plan = form["auth_Plan"];
            string enrolleeAge = form["auth_enrolleeAge"];
            string Diagnosis = form["auth_Diagnosis"];
            string category = form["auth_TypeofAuthorization"];
            string authorizedby = form["auth_User_list"];
            string Note_txt = form["auth_Note_txt"];
            string enrolleeID = form["auth_enrolleeId"];

            string RequesterName = form["auth_RequesterName"];
            string RequesterPhone = form["auth_RequesterPhone"];

            string isadmission = form["auth_isadmis"];
            string isdelivery = form["auth_isdelivery"];

            string admissionDate = form["auth_admissiondate"];
            string dischargeDate = form["auth_dischargedate"];
            string numofdays = form["auth_numodays"];
            string nhisswitch = form["NHISSwitch"];
            string Whatyouauthorized = form["Whatyouauthorized"];

            string requestid = form["auth_requestid"];
            string requestid2 = form["auth_requestid4444"];
            AuthorizationCode exist = _claimsvc.getAuthorizationByCode(authorizationcode);
            #endregion

            if (exist != null || string.IsNullOrEmpty(authorizationcode))
            {
                //exist  log the code

                Log log = new Log();
                log.Type = LogEntryType.Error;
                log.Message = "Authentication code Duplicate";
                log.Detail = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}", authorizationcode, provider_list, policynumber,
                    enrolleeName, company_list, Plan, Diagnosis, category, authorizedby,
                    Note_txt, enrolleeID, RequesterName, RequesterPhone, isadmission, admissionDate,
                    numofdays, nhisswitch);
                //_logger.Insert(log);

                return authorizationcode;
            }
            //added else
            else
            {
                exist = null;

            }

            //var authorizationcode = form["authorizationcode"];
            int age = 0;
            int En_id = -1;
            AuthorizationCode authCode = new AuthorizationCode();
            int company_listint = 0;
            if (string.IsNullOrEmpty(company_list) || !int.TryParse(company_list, out company_listint))
            {

                // _pageMessageSvc.SetErrormessage("Kindly select a company from the list before saving.");

                return "error";

            }
            int intid = 0;
            if (!string.IsNullOrEmpty(Id) && int.TryParse(Id, out intid))
            {
                authCode = _claimsvc.getAuthorization(intid);
            }
            if (!string.IsNullOrEmpty(authorizationcode))
            {

                authCode.authorizationCode = authorizationcode;
                authCode.provider = Convert.ToInt32(provider_list);
                authCode.policyNumber = policynumber;
                authCode.enrolleeName = enrolleeName;
                authCode.EnrolleeCompany = company_list;
                authCode.Plan = Plan;
                authCode.requestername = RequesterName;
                authCode.requesterphone = RequesterPhone;
                authCode.enrolleeAge = int.TryParse(enrolleeAge, out age) ? age : age;
                authCode.Diagnosis = Diagnosis;
                authCode.TypeofAuthorization = category;
                int authby = 0;
                int.TryParse(authorizedby, out authby);

                authCode.Authorizedby = authby < 1 ? 1 : authby;
                authCode.Note = Note_txt;
                authCode.IsNHIS = nhisswitch == "on";
                authCode.isdelivery = isdelivery == "on";
                authCode.enrolleeID = int.TryParse(enrolleeID, out En_id) ? En_id : En_id;

                if (!string.IsNullOrEmpty(Whatyouauthorized))
                {
                    authCode.treatmentAuthorized = Whatyouauthorized;

                }
                string opt = isadmission;
                if (opt == "on")
                {
                    //isadmission
                    DateTime admisionday = CurrentRequestData.Now;
                    DateTime dischargedate = CurrentRequestData.Now;
                    if (string.IsNullOrEmpty(Id))
                    {


                        authCode.admissionStatus = admissionStatus.Admitted;
                    }

                    if (!string.IsNullOrEmpty(admissionDate))
                    {
                        admisionday = !string.IsNullOrEmpty(admissionDate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(admissionDate.Substring(6, 4)), Convert.ToInt32(admissionDate.Substring(3, 2)), Convert.ToInt32(admissionDate.Substring(0, 2))) : CurrentRequestData.Now;
                        authCode.AdmissionDate = admisionday;
                    }



                    if (!string.IsNullOrEmpty(dischargeDate))
                    {
                        dischargedate = !string.IsNullOrEmpty(dischargeDate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dischargeDate.Substring(6, 4)), Convert.ToInt32(dischargeDate.Substring(3, 2)), Convert.ToInt32(dischargeDate.Substring(0, 2))) : CurrentRequestData.Now;
                        authCode.DischargeDate = dischargedate;
                        authCode.admissionStatus = admissionStatus.Discharged;
                    }


                    authCode.Isadmission = true;
                    int numofdayss = 0;
                    int.TryParse(numofdays, out numofdayss);
                    authCode.DaysApprovded = numofdayss;




                }
                else
                {
                    authCode.Isadmission = false;
                }
                if (string.IsNullOrEmpty(Id))
                {


                    authCode.generatedby = CurrentRequestData.CurrentUser.Id;
                }
                else
                {
                    authCode.Id = Convert.ToInt32(Id);
                }


            }

            if (_claimsvc.addAuthorization(authCode))
            {
                //send sms to the guy

                if (true && authCode.enrolleeID > 0)
                {

                    int providerid = 0;
                    int.TryParse(provider_list, out providerid);
                    Enrollee enrolleee = _enrolleeService.GetEnrollee(authCode.enrolleeID);


                    Provider provider = _providerSvc.GetProvider(providerid);

                    if (provider != null && enrolleee != null && !string.IsNullOrEmpty(enrolleee.Mobilenumber))
                    {

                        try
                        {
                            Sms sms = new Sms();
                            sms.FromId = "NHA";
                            sms.DeliveryDate = CurrentRequestData.Now;
                            sms.Message = string.Format("Authorization code was given to {0} to access care at {1}. If this is not you, call or sms 08185809483", enrolleeName, provider.Name.ToUpper());

                            sms.DateDelivered = CurrentRequestData.Now;
                            sms.CreatedBy = 1;
                            sms.Msisdn = enrolleee.Mobilenumber;
                            sms.Status = SmsStatus.Pending;
                            sms.Type = SmsType.Others;

                            bool resp = _smsservice.SendSms(sms);
                        }
                        catch (Exception)
                        {

                        }

                    }

                }


                //update the guy
                int reuestidd = 0;
                if (!string.IsNullOrEmpty(requestid) && int.TryParse(requestid, out reuestidd))
                {
                    AuthorizationRequest reuest = _claimsvc.GetAuthRequest(reuestidd);

                    if (reuest != null)
                    {
                        reuest.isnew = false;
                        _claimsvc.addAuthRequest(reuest);

                    }
                }

                //_pageMessageSvc.SetSuccessMessage("Authorization Code was logged successfully.");
            }
            else
            {
                //_pageMessageSvc.SetErrormessage("There was a problem logging authorization code ,kindly check the value you entered.");
            }

            return authorizationcode;
        }

        public JsonResult GetAuthorizationCodes()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);


            string policynumber_s = CurrentRequestData.CurrentContext.Request["policynumber_s"];

            string provider_searchint = CurrentRequestData.CurrentContext.Request["provider_searchint"];
            string company_searchint = CurrentRequestData.CurrentContext.Request["company_searchint"];

            string auth_searchstr = CurrentRequestData.CurrentContext.Request["auth_searchstr"];



            int scrProvider = !string.IsNullOrEmpty(provider_searchint) ? Convert.ToInt32(provider_searchint) : -1; //CurrentRequestData.CurrentContext.Request["scr_provider"];

            string scruseDate = ""; //CurrentRequestData.CurrentContext.Request["scr_useDate"];
            string scrFromDate = "";//CurrentRequestData.CurrentContext.Request["datepicker"];
            string scrToDate = ""; //CurrentRequestData.CurrentContext.Request["datepicker2"];
            string scrotherFilter = ""; //CurrentRequestData.CurrentContext.Request["scr_otherFilter"];
            string search = auth_searchstr; //CurrentRequestData.CurrentContext.Request["sSearch"];
            string scr_user = ""; //CurrentRequestData.CurrentContext.Request["scr_users"];
            string displayLength2 = CurrentRequestData.CurrentContext.Request["iDisplayLength2"];
            int toltareccount = 0;
            int totalinresult = 0;
            int showexpunge = 0;
            DateTime fromdate = CurrentRequestData.Now;
            DateTime todate = CurrentRequestData.Now;
            bool usedate = false;
            int addedby = 0;
            int authorizedby = 0;
            int companyid = !string.IsNullOrEmpty(company_searchint) ? Convert.ToInt32(company_searchint) : -1;
            string Policynumber = policynumber_s;
            int otherFilters = 0;
            string opmode = CurrentRequestData.CurrentContext.Request["opmode"];
            string startdate = CurrentRequestData.CurrentContext.Request["startdate"];
            string enddate = CurrentRequestData.CurrentContext.Request["enddate"];

            if (!string.IsNullOrEmpty(startdate) && !string.IsNullOrEmpty(enddate))
            {
                fromdate = Convert.ToDateTime(startdate);
                todate = Convert.ToDateTime(enddate);
                usedate = true;
            }

            int opmodee = 0;
            if (!string.IsNullOrEmpty(opmode))
            {
                int.TryParse(opmode, out opmodee);
            }



            IList<AuthorizationCode> query = _claimsvc.QueryAuthorization(out toltareccount, out totalinresult, search,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, Convert.ToInt32(scrProvider), addedby, authorizedby, Policynumber, companyid, usedate, fromdate, todate, Convert.ToInt32(otherFilters), opmodee);
            List<AuthorizationcodeResponse> output = new List<AuthorizationcodeResponse>();
            foreach (AuthorizationCode item in query)
            {


                MrCMS.Entities.People.User user = _userservice.GetUser(item.generatedby);
                Company company = !string.IsNullOrEmpty(item.EnrolleeCompany) ? _companyService.GetCompany(Convert.ToInt32(item.EnrolleeCompany)) : null;
                Provider provider = _providerSvc.GetProvider(item.provider);


                AuthorizationcodeResponse hold = new AuthorizationcodeResponse();
                hold.Id = item.Id;
                hold.providerid = Convert.ToString(item.provider);
                hold.provider = provider != null ? provider.Name.ToUpper() : "--";
                hold.authorizationCode = item.authorizationCode;
                hold.policynumber = !string.IsNullOrEmpty(item.policyNumber) ? item.policyNumber.ToUpper() : "--";
                hold.enrolleename = item.enrolleeName;
                hold.authorizationfor = item.treatmentAuthorized;

                hold.companyname = company != null ? company.Name.ToUpper() : "--";
                hold.plan = item.Plan;
                hold.age = Convert.ToString(item.enrolleeAge);
                hold.diagnosis = item.Diagnosis;

                hold.category = item.TypeofAuthorization;
                hold.authorizeduser = _userservice.GetUser(item.Authorizedby).Name.ToUpper();
                hold.requestersname = !string.IsNullOrEmpty(item.requestername) ? item.requestername.ToUpper() : "--";
                hold.requestersphone = !string.IsNullOrEmpty(item.requesterphone) ? item.requesterphone : "--";

                hold.isadmission = item.Isadmission ? "Yes" : "No";
                hold.AdmissionDate = item.AdmissionDate != null ? Convert.ToDateTime(item.AdmissionDate).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern) : "--";
                hold.DischargeDateshort = Convert.ToDateTime(item.DischargeDate).Year < 2016 ? "" : Convert.ToDateTime(item.DischargeDate).ToString("dd/MM/yyyy");
                hold.AdmissionStatus = item.Isadmission ? Enum.GetName(typeof(admissionStatus), item.admissionStatus) : "--";
                hold.DaysApproved = item.Isadmission ? item.DaysApprovded.ToString() : "--";
                hold.whatwasauthorized = item.treatmentAuthorized;

                string daysused = "--";

                if (item.admissionStatus == admissionStatus.Admitted)
                {
                    hold.DaysUsed = item.Isadmission && CurrentRequestData.Now.Subtract(Convert.ToDateTime(item.AdmissionDate)).Days > 0 ? CurrentRequestData.Now.Subtract(Convert.ToDateTime(item.AdmissionDate)).Days.ToString() : "--";
                }
                else
                {
                    hold.DaysUsed = item.Isadmission && Convert.ToDateTime(item.DischargeDate).Subtract(Convert.ToDateTime(item.AdmissionDate)).Days > 0 ? Convert.ToDateTime(item.DischargeDate).Subtract(Convert.ToDateTime(item.AdmissionDate)).Days.ToString() : "--";
                }




                hold.DischargeDate = item.DischargeDate != null ? Convert.ToDateTime(item.DischargeDate).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern) : "--";

                hold.note = item.Note;
                hold.createdby = user != null ? user.Name.ToUpper() : "--";
                hold.createdDate = Convert.ToDateTime(item.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                output.Add(hold);
            }

            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
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
        public ActionResult Admission(AdmissionMonitorPage page)
        {
            return View(page);
        }
        public ActionResult EditAuthorizationCode(int id)
        {
            AuthorizationCode authcode = _claimsvc.getAuthorization(id);
            IList<MrCMS.Entities.People.User> users = _chatservice.Getallusers();

            var userlist = from aereply in users
                           select new
                           {
                               Id = aereply.Id,
                               Name = aereply.Name,
                           };

            var userr = userlist.ToList();


            userr.Add(new { Id = -1, Name = "Select" });




            List<GenericReponse> itemlist = new List<GenericReponse>();

            itemlist.Add(new GenericReponse { Id = "0", Name = "No" });
            itemlist.Add(new GenericReponse { Id = "1", Name = "Yes" });
            ViewBag.Admisionoption = itemlist;
            ViewBag.isadmissionn = new GenericReponse { Id = "0", Name = "No" };
            string datestr = "";
            string disdatestr = "";
            if (authcode.Isadmission)
            {
                datestr = Convert.ToDateTime(authcode.AdmissionDate).ToString("dd/MM/yyyy");

                if (Convert.ToDateTime(authcode.DischargeDate).Year > 2016)
                {
                    disdatestr = Convert.ToDateTime(authcode.DischargeDate).ToString("dd/MM/yyyy");
                }

                ViewBag.isadmissionn = new GenericReponse { Id = "1", Name = "Yes" };
            }

            ViewBag.datestr = datestr;
            ViewBag.disdatestr = disdatestr;

            ViewBag.usersList = userr.OrderBy(x => x.Id).ToList().OrderBy(x => x.Name);
            List<Company> companylist = _companyService.GetallCompany().OrderBy(x => x.Name).ToList();
            companylist.Insert(0, new Company() { Id = -1, Name = "All Companies" });
            ViewBag.Companylist = companylist;
            ViewBag.PrvidersList = _providerSvc.GetProviderNameList().OrderBy(x => x.Name);

            return PartialView("EditAuthorizationCode", authcode);
        }

        public JsonResult UpdateAuthorizationCode()
        {
            string id = CurrentRequestData.CurrentContext.Request["id"];
            string dischargeDate = CurrentRequestData.CurrentContext.Request["DischargeDate"];
            string comment = CurrentRequestData.CurrentContext.Request["CommentTXT"];

            DateTime dischargedate;


            if (!string.IsNullOrEmpty(id))
            {
                AuthorizationCode authCode = _claimsvc.getAuthorization(Convert.ToInt32(id));
                if (authCode != null)
                {
                    if (true)
                    {
                        dischargedate = !string.IsNullOrEmpty(dischargeDate) ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dischargeDate.Substring(6, 4)), Convert.ToInt32(dischargeDate.Substring(3, 2)), Convert.ToInt32(dischargeDate.Substring(0, 2))) : CurrentRequestData.Now;
                        authCode.DischargeDate = dischargedate;
                        authCode.admissionStatus = admissionStatus.Discharged;
                        authCode.Note = comment;
                        _claimsvc.updateAuthorization(authCode);
                    }


                }
                //added else
                else
                {

                }
            }
            //added else
            else
            {

            }

            return Json("Updated successful.", JsonRequestBehavior.AllowGet);


        }
        [HttpGet]
        public ActionResult DeleteAuthorizationCode(int id)
        {
            AuthorizationCode item = _claimsvc.getAuthorization(id);

            return PartialView("DeleteAuthorizationCode", item);
        }

        [HttpPost]
        public ActionResult DeleteAuthorizationCode(FormCollection form)
        {
            string id = form["Id"];
            if (!string.IsNullOrEmpty(id))
            {
                AuthorizationCode shii = _claimsvc.getAuthorization(Convert.ToInt32(id));

                if (_claimsvc.deleteAuthorization(shii))
                {
                    _pageMessageSvc.SetSuccessMessage("Authorization Code was deleted successful.");
                }
                else
                {
                    _pageMessageSvc.SetErrormessage("There was an error deleting authorization.");
                }
            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was an error deleting authorization.");
            }

            return _uniquePageService.RedirectTo<AuthorizationPage>();
            //return _uniquePageService.RedirectTo<AuthorizationCode>();
        }

        [HttpGet]
        public JsonResult VerifyAuthorizationCode(string code)
        {
            AuthorizationCode valid = new AuthorizationCode();
            if (!string.IsNullOrEmpty(code))
            {
                valid = _claimsvc.getAuthorizationByCode(code);
                if (valid == null)
                {
                    valid = new AuthorizationCode();
                }//added else
                else
                {

                }

            }

            return Json(valid, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetAllVettingProtocol()
        {
            IList<VettingProtocol> vetting = _claimsvc.GetallVettingProtocol();
            return Json(new
            {
                aaData = vetting
            });


            return Json(vetting, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllVettingProtocolforlist()
        {
            IList<VettingProtocol> vetting = _claimsvc.GetallVettingProtocol();
            return Json(new
            {
                aaData = vetting
            });



        }


        public string UploadVetProtocol()
        {
            IList<VettingProtocol> vetting = _claimsvc.GetallVettingProtocol();
            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
               "App_Data");

            string folderpath = Path.Combine(appdatafolder, "DropUpload");
            string filename = Path.Combine(folderpath, "vetprotocol.xlsx");
            string exception = Path.Combine(folderpath, "ExceptionList.txt");
            List<string> errorlist = new List<string>();
            List<string> duplicatelist = new List<string>();
            StreamReader file2 = new StreamReader(exception);
            string txt = file2.ReadToEnd();


            byte[] file = System.IO.File.ReadAllBytes(filename);
            MemoryStream ms = new MemoryStream(file);
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    string strError = "Your Excel file does not contain any work sheets";
                }
                else
                {
                    int count = 1;
                    foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                    {
                        ExcelCellAddress start = worksheet.Dimension.Start;
                        ExcelCellAddress end = worksheet.Dimension.End;
                        for (int row = start.Row; row <= end.Row; row++)
                        {
                            // Row by row...

                            string diagnosis = "";
                            string investigation = "";
                            string treatment = "";
                            string specialistvisit = "";

                            diagnosis = worksheet.Cells[row, 1].Text; // This got me the actual value I needed.
                            investigation = worksheet.Cells[row, 2].Text;
                            treatment = worksheet.Cells[row, 3].Text;
                            specialistvisit = worksheet.Cells[row, 4].Text;





                            if (!string.IsNullOrEmpty(diagnosis))
                            {
                                VettingProtocol objshii = new VettingProtocol();
                                objshii.Diagnosis = diagnosis;
                                objshii.investigations = investigation;
                                objshii.treatment = treatment;
                                objshii.specialist = specialistvisit;

                                _claimsvc.addVettingPRotocol(objshii);
                            }
                        }
                    }
                }
            }

            return "done";
        }

        public string UploadClaimHistory()
        {
            IList<VettingProtocol> vetting = _claimsvc.GetallVettingProtocol();
            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
               "App_Data");

            string folderpath = Path.Combine(appdatafolder, "DropUpload");
            string filename = Path.Combine(folderpath, "claimhistory.xlsx");
            string exception = Path.Combine(folderpath, "ExceptionList.txt");
            List<string> errorlist = new List<string>();
            List<string> duplicatelist = new List<string>();
            StreamReader file2 = new StreamReader(exception);
            string txt = file2.ReadToEnd();
            int maxcount = _claimsvc.MaxClaimHistory();

            byte[] file = System.IO.File.ReadAllBytes(filename);
            MemoryStream ms = new MemoryStream(file);
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    string strError = "Your Excel file does not contain any work sheets";
                }
                else
                {
                    int count = 1;
                    foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                    {
                        ExcelCellAddress start = worksheet.Dimension.Start;
                        ExcelCellAddress end = worksheet.Dimension.End;
                        for (int row = start.Row; row <= end.Row; row++)
                        {
                            // Row by row...

                            if (row <= maxcount)
                            {
                                continue;
                            }

                            string SN = "";
                            string PROVIDER = "";
                            string LOCATION = "";
                            string CLIENTNAME = "";
                            string COMPANY = "";
                            string POLICYNUMBER = "";
                            string HEALTHPLAN = "";
                            string ENCOUNTERDATE = "";
                            string DATERECEIVED = "";
                            string DIAGNOSIS = "";
                            string CLASS = "";
                            string AMOUNTSUBMITTED = "";
                            string AMOUNTPROCESSED = "";
                            string TREATMENT = "";

                            //diagnosis = worksheet.Cells[row, 1].Text; // This got me the actual value I needed.
                            PROVIDER = worksheet.Cells[row, 2].Text;
                            LOCATION = worksheet.Cells[row, 3].Text;
                            CLIENTNAME = worksheet.Cells[row, 4].Text;
                            COMPANY = worksheet.Cells[row, 5].Text;
                            POLICYNUMBER = worksheet.Cells[row, 6].Text;
                            HEALTHPLAN = worksheet.Cells[row, 7].Text;
                            ENCOUNTERDATE = worksheet.Cells[row, 8].Text;
                            DATERECEIVED = worksheet.Cells[row, 9].Text;
                            DIAGNOSIS = worksheet.Cells[row, 10].Text != null ? worksheet.Cells[row, 10].Text : "";
                            CLASS = worksheet.Cells[row, 11].Text != null ? worksheet.Cells[row, 11].Text : "";

                            try
                            {
                                AMOUNTSUBMITTED = worksheet.Cells[row, 12].Text != null ? worksheet.Cells[row, 12].Text : "";
                            }
                            catch (Exception)
                            {

                            }

                            try
                            {
                                AMOUNTPROCESSED = worksheet.Cells[row, 13].Text != null ? worksheet.Cells[row, 13].Text : "";
                            }
                            catch (Exception)
                            {

                            }

                            try
                            {
                                TREATMENT = worksheet.Cells[row, 14].Text != null ? worksheet.Cells[row, 14].Text : "";

                            }
                            catch (Exception)
                            {

                            }
                            if (!string.IsNullOrEmpty(PROVIDER) && !string.IsNullOrEmpty(POLICYNUMBER))
                            {
                                Provider providerr = _providerSvc.GetProviderByName(PROVIDER.Trim());
                                ClaimHistory chistory = new ClaimHistory();
                                chistory.PROVIDER = PROVIDER.Trim();

                                chistory.PROVIDERID = providerr != null ? providerr.Id : -1;
                                chistory.LOCATION = LOCATION;
                                chistory.CLIENTNAME = CLIENTNAME;
                                chistory.COMPANY = COMPANY;
                                chistory.POLICYNUMBER = POLICYNUMBER.Trim();
                                chistory.HEALTHPLAN = HEALTHPLAN;
                                chistory.SerialNo = row;

                                DateTime encontDate = CurrentRequestData.Now.AddYears(-100);
                                chistory.ENCOUNTERDATE = encontDate;
                                DateTime receDate = CurrentRequestData.Now.AddYears(-100);
                                chistory.DATERECEIVED = receDate;
                                if (!string.IsNullOrEmpty(ENCOUNTERDATE) && DateTime.TryParse(ENCOUNTERDATE, out encontDate))
                                {
                                    chistory.ENCOUNTERDATE = encontDate;
                                }

                                if (!string.IsNullOrEmpty(DATERECEIVED) && DateTime.TryParse(DATERECEIVED, out receDate))
                                {
                                    chistory.DATERECEIVED = receDate;
                                }
                                chistory.DIAGNOSIS = DIAGNOSIS;

                                chistory.CLASS = CLASS;
                                decimal amountsubmited = 0m;
                                decimal amountprocessed = 0m;

                                if (!string.IsNullOrEmpty(AMOUNTSUBMITTED) && decimal.TryParse(AMOUNTSUBMITTED, out amountsubmited))
                                {
                                    chistory.AMOUNTSUBMITTED = amountsubmited;

                                }
                                if (!string.IsNullOrEmpty(AMOUNTPROCESSED) && decimal.TryParse(AMOUNTPROCESSED, out amountprocessed))
                                {
                                    chistory.AMOUNTPROCESSED = amountprocessed;

                                }

                                chistory.TREATMENT = TREATMENT;
                                try
                                {
                                    _claimsvc.addClaimHistory(chistory);
                                }
                                catch (Exception)
                                {

                                }



                            }
                        }
                    }
                }
            }

            return "done";
        }
        [HttpGet]
        [ActionName("RejectToCapture")]
        public ActionResult RejectToCapture(int id)
        {

            if (true)
            {
                Entities.ClaimBatch batch = _claimsvc.GetClaimBatch(id);
                batch.status = ClaimBatchStatus.Capturing;
                //batch.submitedReviewbyUser = CurrentRequestData.CurrentUser.Id;

                //batch.SubmitedForReviewDate = CurrentRequestData.Now;
                MrCMS.Entities.People.User user = _userservice.GetUser(batch.submitedVetbyUser);
                Provider provider = _providerSvc.GetProvider(batch.ProviderId);

                if (user != null)
                {
                    string narration = Tools.GetClaimsNarrations(batch);

                    QueuedMessage emailmsg = new QueuedMessage();
                    emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                    emailmsg.ToAddress = user.Email;
                    emailmsg.Subject = "Claim returned - " + provider.Name.ToUpper();
                    emailmsg.FromName = "NOVOHUB";
                    emailmsg.Body = string.Format("Dear {0}, the {1} claim with batch id {2} for {3} was returned to you for your attention.Thank you", user.FirstName, narration.ToUpper(), batch.Id, provider.Name.ToUpper());

                    _emailSender.AddToQueue(emailmsg);
                }


                _claimsvc.UpdateClaimBatch(batch);
                _pageMessageSvc.SetSuccessMessage("Claim batch was successfully submitted for re-capturing.");
                return _uniquePageService.RedirectTo<VetClaimsPage>();

            }
            else
            {
                // _pageMessageSvc.SetErrormessage("There was a problem submiting claims batch for re-capturing.");
                //return _uniquePageService.RedirectTo<VetClaimsPage>();
            }






        }

        public ActionResult VettingProtocol(VettingProtocolPage page)
        {
            return View(page);
        }

        [HttpPost]
        public ActionResult AddvetProtocol(FormCollection Form)
        {
            string diagnosis = Form["diagnosis"];
            string Investigation = Form["Investigation"];
            string Treatment = Form["treatment"];
            string Specialist = Form["Specialist"];
            bool result = false;
            if (!string.IsNullOrEmpty(diagnosis) && !string.IsNullOrEmpty(Treatment))
            {
                VettingProtocol vpobj = new VettingProtocol();
                vpobj.Diagnosis = diagnosis;
                vpobj.investigations = Investigation;
                vpobj.treatment = Treatment;
                vpobj.specialist = Specialist;


                result = _claimsvc.addVettingPRotocol(vpobj);
                if (result)
                {
                    _pageMessageSvc.SetSuccessMessage("Vetting Protocol was added successfully.");

                }
                else
                {
                    _pageMessageSvc.SetErrormessage("Kindly fill the form properly,Diagnosis and Treatment is required.");
                }
            }
            else
            {
                _pageMessageSvc.SetErrormessage("Kindly fill the form properly,Diagnosis and Treatment is required.");

            }
            return _uniquePageService.RedirectTo<VettingProtocolPage>();

        }
        [HttpGet]
        public ActionResult DeletevetProtocol(int Id)
        {


            if (true)
            {
                VettingProtocol vpobj = _claimsvc.getVettingPRotocol(Id);

                if (vpobj != null)
                {
                    _claimsvc.deleteVettingProtocol(vpobj);
                    _pageMessageSvc.SetSuccessMessage("Protocol was deleted successfully.");
                }
                else
                {
                    _pageMessageSvc.SetErrormessage("Protocol was not found.");
                }

            }
            return _uniquePageService.RedirectTo<VettingProtocolPage>();
        }

        [HttpPost]
        public ActionResult EditvetProtocol(FormCollection Form)
        {
            string id = Form["Id"];
            string diagnosis = Form["diagnosis"];
            string Investigation = Form["Investigation"];
            string Treatment = Form["treatment"];
            string Specialist = Form["Specialist"];
            bool result = false;
            int intid = 0;
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(diagnosis) && !string.IsNullOrEmpty(Treatment) && int.TryParse(id, out intid))
            {
                VettingProtocol vpobj = _claimsvc.getVettingPRotocol(intid);

                if (vpobj == null)
                {
                    _pageMessageSvc.SetErrormessage("Cannot edit vetting protocol,the protocol is missing.");
                    return _uniquePageService.RedirectTo<VettingProtocolPage>();
                }

                vpobj.Diagnosis = diagnosis;
                vpobj.investigations = Investigation;
                vpobj.treatment = Treatment;
                vpobj.specialist = Specialist;


                result = _claimsvc.UpdateVettingProtocol(vpobj);
                if (result)
                {
                    _pageMessageSvc.SetSuccessMessage("Vetting Protocol was updated successfully.");

                }
                else
                {
                    _pageMessageSvc.SetErrormessage("Kindly fill the form properly,Diagnosis and Treatment is required.");
                }
            }
            else
            {
                _pageMessageSvc.SetErrormessage("Kindly fill the form properly,Diagnosis and Treatment is required.");

            }
            return _uniquePageService.RedirectTo<VettingProtocolPage>();
        }


        [ActionName("PendingPaymentClaimList")]
        public ActionResult PendingPaymentClaimList(PendingPaymentClaimsPage page)
        {
            int year = CurrentRequestData.Now.Year;
            List<GenericReponse2> yealist = new List<GenericReponse2>();


            for (int i = 0; i < 20; i++)
            {
                yealist.Add(new GenericReponse2 { Id = year - i, Name = (year - i).ToString() });
            }
            ViewBag.YearList = yealist;
            List<GenericReponse2> plist = _providerSvc.GetProviderNameList().OrderBy(x => x.Name).ToList();

            plist.Insert(0, new GenericReponse2 { Id = -1, Name = "All Providers" });
            ViewBag.PrvidersList = plist;
            IList<MrCMS.Entities.People.User> users = _chatservice.Getallusers();

            var userlist = from aereply in users
                           select new
                           {
                               Id = aereply.Id,
                               Name = aereply.Name,
                           };

            var userr = userlist.ToList();

            userr.Add(new { Id = -1, Name = "Select" });


            ViewBag.UserList = userr.OrderBy(x => x.Id).ToList();
            List<GenericReponse> batchlist = new List<GenericReponse>();
            foreach (string item in Enum.GetNames(typeof(Utility.ClaimBatch)))
            {
                batchlist.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(Utility.ClaimBatch), item)).ToString(), Name = item.ToUpper() });
            }
            ViewBag.BatchList = batchlist;
            ViewBag.Defaultdate = CurrentRequestData.Now.ToString("MM/dd/yyyy");

            IEnumerable<Zone> zones = _helperSvc.GetallZones();
            List<GenericReponse2> zonelist = new List<GenericReponse2>();

            foreach (Zone item in zones)
            {
                GenericReponse2 shii = new GenericReponse2()
                {
                    Id = item.Id,
                    Name = item.Name
                };
                zonelist.Add(shii);


            }

            zonelist.Insert(0, new GenericReponse2() { Id = -1, Name = "All Zones" });
            ViewBag.ZoneList = zonelist;


            return View(page);

        }
        public JsonResult GetClaimBatchAwaitingPaymentJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);

            string scrProvider = CurrentRequestData.CurrentContext.Request["Provider_list"];
            string month = CurrentRequestData.CurrentContext.Request["Month_list"];
            string year = CurrentRequestData.CurrentContext.Request["year"];
            string batch = CurrentRequestData.CurrentContext.Request["Batch"];

            string Zone = CurrentRequestData.CurrentContext.Request["Zone"];
            string claimbatchidd = CurrentRequestData.CurrentContext.Request["claimbatchidd"];
            int clambatchiddd = 0;
            int.TryParse(claimbatchidd, out clambatchiddd);
            int toltareccount = 0;
            int totalinresult = 0;
            int providercount = 0;
            decimal totalamount = 0m;
            decimal totalprocessed = 0m;
            IList<Entities.ClaimBatch> allincomingClaims = _claimsvc.QueryAllClaimBatch(out toltareccount, out totalinresult, string.Empty,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, Convert.ToInt32(scrProvider), Convert.ToInt32(month), Convert.ToInt32(year), (Utility.ClaimBatch)Enum.Parse(typeof(Utility.ClaimBatch), batch), Zone, 0, ClaimBatchStatus.AwaitingPayment, out providercount, out totalamount, out totalprocessed, -1, clambatchiddd);

            Entities.ClaimBatch claimm = _claimsvc.GetClaimBatch(1);
            List<ClaimsBatchResponse> output = new List<ClaimsBatchResponse>();
            DateTime today = CurrentRequestData.Now;
            foreach (Entities.ClaimBatch areply in allincomingClaims)
            {

                string narration = "";
                try
                {
                    narration = Tools.GetClaimsNarrations(areply);
                }
                catch (Exception)
                {

                }


                Provider provider = _providerSvc.GetProvider(areply.ProviderId);
                ClaimsBatchResponse model = new ClaimsBatchResponse();
                model.Id = areply.Id;
                model.GroupName = provider != null ? _helperSvc.GetzonebyId(Convert.ToInt32(provider.State.Zone)).Name : "--";
                model.Provider = provider != null ? provider.Name : "--";
                model.PRoviderAddress = provider != null ? provider.Address : "--";
                model.narration = narration;
                model.Batch = areply.Batch == Utility.ClaimBatch.BatchA ? "Batch A" : "Batch B";
                model.deliveryCount = areply.IncomingClaims.Where(x => x.IsDeleted == false).ToList().Count.ToString();
                model.claimscount = areply.Claims.Count.ToString();
                model.totalAmount = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))).ToString("N");
                model.totalProccessed = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount))).ToString("N");
                model.difference = Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount))) > 0 ? Convert.ToDecimal(Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount))) - Convert.ToDecimal(areply.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + areply.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount)))).ToString("N") : "0.00";
                model.CapturedBy = areply.submitedReviewbyUser > 0 ? _userservice.GetUser(areply.submitedReviewbyUser).Name : "--";
                model.DateSubmitedForVetting = Convert.ToDateTime(areply.SubmitedForReviewDate).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                IncomingClaims income = areply.IncomingClaims.FirstOrDefault();
                if (income != null)
                {

                    model.Caption = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().caption) ? areply.IncomingClaims.FirstOrDefault().caption : "--";
                    model.Note = !string.IsNullOrEmpty(areply.IncomingClaims.FirstOrDefault().Note) ? areply.IncomingClaims.FirstOrDefault().Note : "--";
                    model.isSubmittedRemotely = areply.IncomingClaims.FirstOrDefault().IsRemoteSubmission;
                    model.deliverydate = Convert.ToDateTime(income.DateReceived).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                    string monthd = "";
                    if (!string.IsNullOrEmpty(income.month_string) && income.month_string.Split(',').Count() > 0)
                    {
                        foreach (string itemmm in income.month_string.Split(','))
                        {
                            model.month_string = model.month_string + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(itemmm)) + ",";
                        }
                        model.month_string = model.month_string + income.year.ToString();


                    }
                }
                output.Add(model);
            }


            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                totalcaptured = totalamount.ToString("N"),
                totalprocessed = totalprocessed.ToString("N"),
                difference = Convert.ToDecimal(totalamount - totalprocessed).ToString("N"),
                providercount = providercount,
                batch = Enum.GetName(typeof(Utility.ClaimBatch), Convert.ToInt32(batch)),
                aaData = response.Data
            });




        }


        [ActionName("AllClaimList")]
        public ActionResult AllClaimList(AllClaimsPage Page)
        {




            int year = CurrentRequestData.Now.Year;
            List<GenericReponse2> yealist = new List<GenericReponse2>();


            for (int i = 0; i < 20; i++)
            {
                yealist.Add(new GenericReponse2 { Id = year - i, Name = (year - i).ToString() });
            }
            ViewBag.YearList = yealist;
            List<GenericReponse2> plist = _providerSvc.GetProviderNameList().OrderBy(x => x.Name).ToList();

            plist.Insert(0, new GenericReponse2 { Id = -1, Name = "All Providers" });
            ViewBag.PrvidersList = plist;
            IList<MrCMS.Entities.People.User> users = _chatservice.Getallusers();

            var userlist = from aereply in users
                           select new
                           {
                               Id = aereply.Id,
                               Name = aereply.Name,
                           };

            var userr = userlist.ToList();

            userr.Add(new { Id = -1, Name = "Select" });


            ViewBag.UserList = userr.OrderBy(x => x.Id).ToList();
            List<GenericReponse> batchlist = new List<GenericReponse>();
            foreach (string item in Enum.GetNames(typeof(Utility.ClaimBatch)))
            {
                batchlist.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(Utility.ClaimBatch), item)).ToString(), Name = item.ToUpper() });
            }
            ViewBag.BatchList = batchlist;
            ViewBag.Defaultdate = CurrentRequestData.Now.ToString("MM/dd/yyyy");
            IEnumerable<Zone> zones = _helperSvc.GetallZones();
            List<GenericReponse2> zonelist = new List<GenericReponse2>();

            foreach (Zone item in zones)
            {
                GenericReponse2 shii = new GenericReponse2()
                {
                    Id = item.Id,
                    Name = item.Name
                };
                zonelist.Add(shii);


            }

            zonelist.Insert(0, new GenericReponse2() { Id = -1, Name = "All Zones" });
            ViewBag.ZoneList = zonelist;
            return View(Page);
        }
        public ActionResult PaymentBatchList(PaymentBatchListPage Page)
        {

            //load the payment status
            List<GenericReponse> statuslist = new List<GenericReponse>();

            foreach (string item in Enum.GetNames(typeof(PaymentStatus)))
            {
                statuslist.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(PaymentStatus), item)).ToString(), Name = item.ToUpper() });
            }
            statuslist.Insert(0, new GenericReponse() { Id = "-1", Name = "All" });
            ViewBag.statuslist = statuslist;

            //get the return url

            string url = _uniquePageService.GetUniquePage<ExpandPaymentBatchPage>().AbsoluteUrl;
            Page.urlReturn = url;



            return View(Page);

        }

        [HttpPost]
        public ActionResult AddPaymentBatch(FormCollection form)
        {
            string title = form["title"];
            string descr = form["desc"];

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(descr))
            {
                PaymentBatch paymentbatch = new PaymentBatch();

                paymentbatch.Title = title.ToUpper();
                paymentbatch.Description = descr;
                paymentbatch.status = PaymentStatus.Default;

                paymentbatch.createdBy = CurrentRequestData.CurrentUser.Id;

                _claimsvc.addPaymentBatch(paymentbatch);
                _pageMessageSvc.SetSuccessMessage("Payment Batch Added");


            }
            else
            {
                _pageMessageSvc.SetErrormessage("Kindly fill the form properly.");

            }


            return _uniquePageService.RedirectTo<PaymentBatchListPage>();

        }
        [HttpGet]
        public ActionResult DeletePaymentBatch(int id)
        {
            PaymentBatch paymentbatch = _claimsvc.getpaymentbatch(id);

            if (paymentbatch != null)
            {
                if (paymentbatch.status != PaymentStatus.Default)
                {
                    _pageMessageSvc.SetErrormessage("The payment batch is being processed,you cannot deleted it.");

                }

                if (paymentbatch.ClaimBatchList.Any())
                {
                    _pageMessageSvc.SetErrormessage("The payment batch has bills in it,you cannot deleted it.");
                }

                if (!paymentbatch.ClaimBatchList.Any() && paymentbatch.status == PaymentStatus.Default)
                {
                    _claimsvc.deletepaymentbatch(paymentbatch);
                    _pageMessageSvc.SetSuccessMessage("Payment batch was deleted.");

                }
            }

            return _uniquePageService.RedirectTo<PaymentBatchListPage>();
        }
        [HttpPost]
        public ActionResult EditPaymentBatch(FormCollection form)
        {
            string id = form["batchid"];
            string title = form["titleed"];
            string descr = form["desced"];
            int idd = 0;

            if (int.TryParse(id, out idd) && !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(descr))
            {
                PaymentBatch paymentbatch = _claimsvc.getpaymentbatch(idd);

                if (paymentbatch != null)
                {
                    paymentbatch.Title = title.ToUpper();
                    paymentbatch.Description = descr;
                    _claimsvc.addPaymentBatch(paymentbatch);
                    _pageMessageSvc.SetSuccessMessage("Payment Batch updated");

                }



            }
            else
            {
                _pageMessageSvc.SetErrormessage("Kindly fill the form properly.");

            }


            return _uniquePageService.RedirectTo<PaymentBatchListPage>();
        }



        public ActionResult MarkPaymentBatch(int batchid, int status)
        {
            //1=pending 2=paying 3= paid

            PaymentBatch batch = _claimsvc.getpaymentbatch(batchid);

            if (batch != null)
            {
                switch (status)
                {
                    case 1:
                        batch.status = PaymentStatus.Pending;
                        break;
                    case 2:
                        batch.status = PaymentStatus.Paying;
                        batch.datepaymentstarted = CurrentRequestData.Now;

                        break;

                    case 3:
                        batch.status = PaymentStatus.Paid;
                        batch.datepaymentcompleted = CurrentRequestData.Now;
                        batch.paidby = CurrentRequestData.CurrentUser.Id;

                        break;

                    default:
                        break;

                }



                _claimsvc.addPaymentBatch(batch);
                _pageMessageSvc.SetSuccessMessage("Payment batch marked");


            }

            return _uniquePageService.RedirectTo<PaymentBatchListPage>();


        }
        public JsonResult QueryPaymentBatchJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);



            string namestring = CurrentRequestData.CurrentContext.Request["titlestring"];
            string status = CurrentRequestData.CurrentContext.Request["status"];
            string descstring = CurrentRequestData.CurrentContext.Request["descstring"];
            string scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            string scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            string scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];


            DateTime fromdate = CurrentRequestData.Now;
            DateTime todate = CurrentRequestData.Now;
            bool usedate = false;
            if (!string.IsNullOrEmpty(scrFromDate) && !string.IsNullOrEmpty(scrToDate))
            {
                fromdate = Convert.ToDateTime(scrFromDate);
                todate = Convert.ToDateTime(scrToDate);
                usedate = Convert.ToBoolean(scruseDate);
            }

            bool usestatus = false;
            int statusint = -1;
            if (int.TryParse(status, out statusint) && statusint > -1)
            {
                usestatus = true;
            }

            PaymentStatus paymentstatus = PaymentStatus.Default;

            if (!string.IsNullOrEmpty(status))
            {
                object ddd = Enum.Parse(typeof(PaymentStatus), status);
                paymentstatus = (PaymentStatus)ddd;

            }
            int toltareccount = 0;
            int totalinresult = 0;
            IList<PaymentBatch> allpaymentbatch = _claimsvc.Queryallpaymentbatch(out toltareccount, out totalinresult, string.Empty,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, namestring, usestatus, paymentstatus, usedate, fromdate, todate);



            List<PaymentBatchResponse> output = new List<PaymentBatchResponse>();

            foreach (PaymentBatch item in allpaymentbatch)
            {
                PaymentBatchResponse obj = new PaymentBatchResponse();
                obj.Id = item.Id.ToString();
                obj.Title = item.Title.ToUpper();
                obj.description = item.Description;
                obj.Claimcount = item.ClaimBatchList.Count().ToString();
                decimal? hsit = item.ClaimBatchList.Sum(x => x.Claims.Sum(y => y.DrugList.Sum(z => z.VettedAmount))) + item.ClaimBatchList.Sum(x => x.Claims.Sum(y => y.ServiceList.Sum(z => z.VettedAmount)));
                obj.TotalAmount = Convert.ToDecimal(hsit).ToString("N");
                obj.TotalPaid = Convert.ToDecimal(item.ClaimBatchList.Sum(x => x.AmountPaid)).ToString("N");
                obj.datepaymentstarted = "--";
                obj.datepaymentcompleted = Convert.ToDateTime(item.datepaymentcompleted).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                obj.Status = Enum.GetName(typeof(PaymentStatus), item.status);

                MrCMS.Entities.People.User createduser = item.createdBy > 0 ? _userservice.GetUser(item.createdBy) : null;
                MrCMS.Entities.People.User paidbyuser = item.paidby > 0 ? _userservice.GetUser(item.paidby) : null;
                obj.createdby = createduser != null ? createduser.Name : "--";
                obj.paidby = paidbyuser != null ? paidbyuser.Name : "--";
                obj.datecreated = Convert.ToDateTime(item.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                output.Add(obj);




            }
            DateTime today = CurrentRequestData.Now;



            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
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

        public JsonResult GetrecentPaymentbatchJson()
        {
            IList<PaymentBatch> list = _claimsvc.getrecentpaymentbatch();

            List<GenericReponse2> output = new List<GenericReponse2>();

            foreach (PaymentBatch item in list)
            {
                GenericReponse2 itemm = new GenericReponse2
                {
                    Id = item.Id,
                    Name = item.Title.ToUpper() + " - " + Convert.ToDateTime(item.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern).ToLower() + " - " + item.ClaimBatchList.Count().ToString() + " Claims."
                };
                output.Add(itemm);
            }


            return Json(output, JsonRequestBehavior.AllowGet);


        }

        public ActionResult ExpandPaymentBatch(ExpandPaymentBatchPage page, int? batchid)
        {


            int idd = Convert.ToInt32(batchid);

            PaymentBatch item = _claimsvc.getpaymentbatch(idd);

            if (item != null)
            {
                PaymentBatchResponse obj = new PaymentBatchResponse();
                obj.Id = item.Id.ToString();
                obj.Title = item.Title.ToUpper();
                obj.description = item.Description;
                obj.Claimcount = item.ClaimBatchList.Count().ToString();
                decimal? hsit = item.ClaimBatchList.Sum(x => x.Claims.Sum(y => y.DrugList.Sum(z => z.VettedAmount))) + item.ClaimBatchList.Sum(x => x.Claims.Sum(y => y.ServiceList.Sum(z => z.VettedAmount)));
                obj.TotalAmount = Convert.ToDecimal(hsit).ToString("N");
                obj.TotalPaid = Convert.ToDecimal(item.ClaimBatchList.Sum(x => x.AmountPaid)).ToString("N");
                obj.datepaymentstarted = "--";
                obj.datepaymentcompleted = Convert.ToDateTime(item.datepaymentcompleted).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                obj.Status = Enum.GetName(typeof(PaymentStatus), item.status);

                MrCMS.Entities.People.User createduser = item.createdBy > 0 ? _userservice.GetUser(item.createdBy) : null;
                MrCMS.Entities.People.User paidbyuser = item.paidby > 0 ? _userservice.GetUser(item.paidby) : null;
                obj.createdby = createduser != null ? createduser.Name : "--";
                obj.paidby = paidbyuser != null ? paidbyuser.Name : "--";
                obj.datecreated = Convert.ToDateTime(item.CreatedOn).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                ViewBag.Modelsmall = obj;

                int count = 0;
                List<Entities.ClaimBatch> newlist = new List<Entities.ClaimBatch>();

                foreach (Entities.ClaimBatch itemo in item.ClaimBatchList)
                {
                    Provider provider = _providerSvc.GetProvider(itemo.ProviderId);

                    if (provider != null)
                    {
                        itemo.ProviderName = provider.Name;

                    }

                    newlist.Add(itemo);


                }
                item.ClaimBatchList = newlist;
            }



            page.paymentbatch = item;


            return View(page);
        }

        [HttpGet]
        public ActionResult removeItemPaymentBatch(int batchid, int itemid)
        {
            if (batchid > 0 && itemid > 0)
            {
                PaymentBatch batch = _claimsvc.getpaymentbatch(batchid);


                if (batch != null)
                {
                    List<Entities.ClaimBatch> items = batch.ClaimBatchList.Where(x => x.Id == itemid).ToList();

                    if (items.Any())
                    {
                        Entities.ClaimBatch item = items.First();

                        batch.ClaimBatchList.Remove(item);
                        _claimsvc.addPaymentBatch(batch);

                        item.status = ClaimBatchStatus.AwaitingApproval;
                        item.paymentbatch = null;


                        _claimsvc.UpdateClaimBatch(item);
                        _pageMessageSvc.SetSuccessMessage("Item was removed from the payment batch ,you can find item in reviewed.");



                    }
                }
                else
                {
                    _pageMessageSvc.SetErrormessage("The payment batch does not exist.");

                }



            }


            return Redirect(string.Format(_uniquePageService.GetUniquePage<ExpandPaymentBatchPage>().AbsoluteUrl + "?batchid={0}",
                                  batchid));
        }


        public ActionResult AuthRequestList(AuthRequestPage page)
        {
            return View(page);
        }
        [HttpGet]
        public JsonResult QueryAuthRequestJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);



            //var search = CurrentRequestData.CurrentContext.Request["search"];
            //var status = CurrentRequestData.CurrentContext.Request["status"];

            string scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            string scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            string scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];


            DateTime fromdate = CurrentRequestData.Now;
            DateTime todate = CurrentRequestData.Now;
            bool usedate = false;
            if (!string.IsNullOrEmpty(scrFromDate) && !string.IsNullOrEmpty(scrToDate))
            {
                fromdate = Convert.ToDateTime(scrFromDate);
                todate = Convert.ToDateTime(scrToDate);
                usedate = Convert.ToBoolean(scruseDate);
            }


            int toltareccount = 0;
            int totalinresult = 0;
            IList<AuthorizationRequest> allrequest = _claimsvc.QueryallAuthRequest(out toltareccount, out totalinresult, string.Empty,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, "", usedate, fromdate, todate);




            JsonResult response = Json(allrequest, JsonRequestBehavior.AllowGet);
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

        [HttpGet]
        public JsonResult getalldeletedclaim(int providerid)
        {
            List<string> resilt = _claimsvc.getAllDeletedClaimsForProvider(providerid);
            return Json(resilt, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ExportPaymentBatchCSV(int paymentid)
        {

            PaymentBatch batch = _claimsvc.getpaymentbatch(paymentid);
            List<PaymentBatchCSVEXPORT> outputlist = new List<PaymentBatchCSVEXPORT>();

            if (batch != null)
            {
                foreach (Entities.ClaimBatch item in batch.ClaimBatchList)
                {
                    PaymentBatchCSVEXPORT newobj = new PaymentBatchCSVEXPORT();
                    Provider provider = _providerSvc.GetProvider(item.ProviderId);

                    newobj.Id = item.Id;
                    IncomingClaims income = item.IncomingClaims.FirstOrDefault();
                    string claimperiod = "";
                    string claimbatchstring = Enum.GetName(typeof(Utility.ClaimBatch), item.Batch);

                    decimal? summ2 = item.Claims.Sum(x => x.DrugList.Sum(y => y.VettedAmount)) + item.Claims.Sum(x => x.ServiceList.Sum(y => y.VettedAmount));
                    decimal? summ1 = item.Claims.Sum(x => x.DrugList.Sum(y => y.InitialAmount)) + item.Claims.Sum(x => x.ServiceList.Sum(y => y.InitialAmount));
                    decimal? diffence = summ2 - summ1;

                    string claimstatus = Enum.GetName(typeof(ClaimBatchStatus), item.status);
                    string paymentmethod = string.IsNullOrEmpty(item.paymentmethodstring) ? "--" : item.paymentmethodstring; //Enum.GetName(typeof(MrCMS.Web.Apps.Core.Utility.PaymentMethod), item.paymentmethod);
                    string paymentref = string.IsNullOrEmpty(item.paymentref) ? "--" : item.paymentref;
                    string sourcebankname = string.IsNullOrEmpty(item.sourceBankName) ? "--" : item.sourceBankName;
                    string sourcebankacc = string.IsNullOrEmpty(item.sourceBankAccountNo) ? "--" : item.sourceBankAccountNo;
                    string destbankname = string.IsNullOrEmpty(item.DestBankName) ? "--" : item.DestBankName;
                    string destbankacc = string.IsNullOrEmpty(item.DestBankAccountNo) ? "--" : item.DestBankAccountNo;


                    DateTime paymentdate = Convert.ToDateTime(item.paymentdate);
                    string paymentdatestring = "--";
                    string paidbystr = "--";
                    paidbystr = !string.IsNullOrEmpty(item.paidby) ? item.paidby.ToUpper() : "--";


                    if (paymentdate.Year > 1990)
                    {
                        paymentdatestring = paymentdate.ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                    }
                    string markpaidby = "--";

                    if (income != null && !string.IsNullOrEmpty(income.month_string) && income.month_string.Split(',').Count() > 0)
                    {

                        foreach (string itemmm in income.month_string.Split(','))
                        {
                            claimperiod = claimperiod + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(itemmm)) + ",";
                        }
                        claimperiod = claimperiod + income.year.ToString();


                    }
                    string zonename = "--";
                    if (provider != null)
                    {
                        Zone zone = _helperSvc.GetzonebyId(Convert.ToInt32(provider.State.Zone));
                        if (zone != null)
                        {
                            zonename = zone.Name;

                        }
                    }

                    newobj.ClaimsPeriod = claimperiod;
                    newobj.UPN = item.ProviderId.ToString();
                    newobj.Provider = provider != null ? provider.Name.ToUpper() : "--";
                    newobj.ProviderAddress = provider != null ? provider.Address.ToUpper() : "--";
                    newobj.Providerzone = zonename;
                    newobj.ClaimBatch = claimbatchstring;
                    newobj.InitialAmount = Convert.ToDecimal(summ1).ToString("N");
                    newobj.ProcessedAmount = Convert.ToDecimal(summ2).ToString("N");
                    newobj.Status = claimstatus;
                    newobj.AmountPaid = Convert.ToDecimal(item.AmountPaid).ToString("N");
                    newobj.PaymentMethod = paymentmethod;
                    newobj.PaymentReference = paymentref;
                    newobj.SourceBankName = item.sourceBankName;
                    newobj.SourceAccountNumber = item.sourceBankAccountNo;
                    newobj.DestinationBankName = item.DestBankName;
                    newobj.DestinationAccountNumber = item.DestBankAccountNo;
                    newobj.PaymentDate = Convert.ToDateTime(item.paymentdate).ToString("dd/MM/yyyy");
                    newobj.paidby = item.paidby;
                    newobj.remark = item.remark;
                    outputlist.Add(newobj);




                }
            }

            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath, "App_Data");
            string foldername = Guid.NewGuid().ToString();
            string fullpath = Path.Combine(appdatafolder, foldername);
            Directory.CreateDirectory(fullpath);

            //var excelarray = Utility.Tools.DumpExcelGetByte(claimdatabase);

            //write excel to folder


            StreamWriter writer = System.IO.File.CreateText(Path.Combine(fullpath, foldername + ".csv"));
            CsvWriter csv = new CsvWriter(writer);
            csv.WriteRecords(outputlist);
            csv.Flush();
            writer.Close();

            Response.ContentType = "attachment;filename=myfilename.csv";
            Response.AddHeader("content-disposition", "attachment;  filename=ExportedPaymentSheet.csv");
            Response.BinaryWrite(System.IO.File.ReadAllBytes(Path.Combine(fullpath, foldername + ".csv")));


            return null;
        }

        [HttpPost]
        public ActionResult ImportPaymentMadeCSV(FormCollection form)
        {

            string paymentid = form["paymentid"];
            HttpPostedFileBase file = CurrentRequestData.CurrentContext.Request.Files.Count > 0 ? CurrentRequestData.CurrentContext.Request.Files[0] : null;

            if (string.IsNullOrEmpty(paymentid) || file.ContentLength < 1)
            {
                _pageMessageSvc.SetErrormessage("There was an error,kindly check the file uploaded.");

                return _uniquePageService.RedirectTo<PaymentBatchListPage>();

            }
            else
            {
                TextReader tr = new StreamReader(file.InputStream);
                CsvReader csv = new CsvReader(tr);
                IEnumerable<PaymentBatchCSVEXPORT> records = csv.GetRecords<PaymentBatchCSVEXPORT>();

                foreach (PaymentBatchCSVEXPORT item in records)
                {
                    Entities.ClaimBatch thereal = _claimsvc.GetClaimBatch(item.Id);
                    if (thereal != null)
                    {
                        //update the stuffs
                        thereal.AmountPaid = Convert.ToDecimal(item.AmountPaid);
                        thereal.paymentmethodstring = item.PaymentMethod;
                        thereal.paymentref = item.PaymentReference;
                        thereal.sourceBankName = item.SourceBankName;
                        thereal.sourceBankAccountNo = item.SourceAccountNumber;
                        thereal.DestBankName = item.DestinationBankName;
                        thereal.DestBankAccountNo = item.DestinationAccountNumber;
                        thereal.paymentdate = DateTime.ParseExact(item.PaymentDate, "d/M/yyyy", CultureInfo.InvariantCulture);
                        thereal.paidby = item.paidby;
                        thereal.remark = item.remark;

                        if (Convert.ToDecimal(item.AmountPaid) > 0)
                        {
                            thereal.status = ClaimBatchStatus.Paid;
                        }

                        _claimsvc.UpdateClaimBatch(thereal);
                    }
                }
            }

            _pageMessageSvc.SetSuccessMessage("File processed successfully.");
            return Redirect(_uniquePageService.GetUniquePage<ExpandPaymentBatchPage>().AbsoluteUrl + "?batchid=" + paymentid);

        }


        [ActionName("ExportPaymentAnalysis")]
        public ActionResult ExportPaymentAnalysis(int id)
        {

            PaymentBatch paymentbatchid = _claimsvc.getpaymentbatch(id);

            IList<Entities.ClaimBatch> allincomingClaims = paymentbatchid.ClaimBatchList;


            DataTable claimdatabase = new DataTable();

            claimdatabase.Columns.Add("S/N", typeof(string));

            claimdatabase.Columns.Add("Client Name", typeof(string));
            claimdatabase.Columns.Add("Company", typeof(string));
            claimdatabase.Columns.Add("Policy Number", typeof(string));
            claimdatabase.Columns.Add("Health Plan", typeof(string));
            claimdatabase.Columns.Add("Encounter Date", typeof(string));
            claimdatabase.Columns.Add("Date Received", typeof(string));
            claimdatabase.Columns.Add("Diagnosis", typeof(string));
            claimdatabase.Columns.Add("Class", typeof(string));
            claimdatabase.Columns.Add("Amount Submited", typeof(decimal));
            claimdatabase.Columns.Add("Amount Processed", typeof(decimal));
            claimdatabase.Columns.Add("Difference", typeof(decimal));
            claimdatabase.Columns.Add("Comment", typeof(string));
            claimdatabase.Columns.Add("providername", typeof(string));
            claimdatabase.Columns.Add("providerUPN", typeof(string));
            claimdatabase.Columns.Add("Region", typeof(string));
            byte[] excelarray;
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = null;
                Dictionary<int, ExcelWorksheet> zonediction = new Dictionary<int, ExcelWorksheet>();
                Dictionary<int, int> SheetCount = new Dictionary<int, int>();
                IEnumerable<Zone> zones = _helperSvc.GetallZones();
                foreach (Zone zone in zones)
                {
                    zonediction.Add(zone.Id, pck.Workbook.Worksheets.Add(zone.Name.ToUpper()));
                    SheetCount.Add(zone.Id, 1);
                }

                int totalrowcount = 1;
                string providername = "";
                string provideraddress = "";
                int zoneiddd = -1;
                foreach (Entities.ClaimBatch claimbatch in allincomingClaims.OrderBy(x => x.ProviderId))
                {

                    string datereceived = "";
                    if (claimbatch != null)
                    {
                        Provider provider = _providerSvc.GetProvider(claimbatch.ProviderId);
                        ws = zonediction[Convert.ToInt32(provider.State.Zone)];
                        zoneiddd = Convert.ToInt32(provider.State.Zone);
                        totalrowcount = SheetCount[Convert.ToInt32(provider.State.Zone)];
                        IncomingClaims firstcoming = claimbatch.IncomingClaims.FirstOrDefault();
                        providername = provider.Name.ToUpper();
                        provideraddress = provider.Address.ToUpper();
                        decimal totalsubmittedSum = 0m;
                        decimal totalProccessedSum = 0m;
                        decimal totaldiffenrenceSum = 0m;


                        if (firstcoming != null)
                        {
                            datereceived = Convert.ToDateTime(firstcoming.DateReceived).ToString("dd/MM/yyyy");
                        }


                        int countit = 1;

                        foreach (Claim item in claimbatch.Claims)
                        {
                            string ClaimsFormNo = !string.IsNullOrEmpty(item.ClaimsSerialNo) ? item.ClaimsSerialNo : "";
                            string EnrolleeName = !string.IsNullOrEmpty(item.enrolleeFullname) ? item.enrolleeFullname : "";
                            string PolicyNumber = !string.IsNullOrEmpty(item.enrolleePolicyNumber) ? item.enrolleePolicyNumber : "";
                            string HealthPlan = !string.IsNullOrEmpty(item.EnrolleePlan) ? item.EnrolleePlan : "";
                            string Company = !string.IsNullOrEmpty(item.enrolleeCompanyName) ? item.enrolleeCompanyName : "";
                            string EncounterDate = Convert.ToDateTime(item.ServiceDate).ToString("dd/MM/yyyy"); //Convert.ToDateTime(item.ServiceDate).ToString(MrCMS.Website.CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                            string Diagnosis = !string.IsNullOrEmpty(item.Diagnosis) ? item.Diagnosis : "";
                            string DurationofTreatment = !string.IsNullOrEmpty(item.Durationoftreatment) ? item.Durationoftreatment : "";
                            string TreatmentTag = Enum.GetName(typeof(ClaimsTAGS), item.Tag);
                            string ServiceCharge = Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount)).ToString("N");
                            string DrugCharge = Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)).ToString("N");
                            string TotalCharge = Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount))).ToString("N");
                            string ProcessedCharge = Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.VettedAmount))).ToString("N");
                            string ChargeDifference = @Convert.ToDecimal((Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount))) - (Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.VettedAmount)))).ToString("N");
                            string comment = "";

                            string providerupn = provider.Id.ToString();
                            Zone region = zones.Where(x => x.Id == Convert.ToInt32(provider.State.Zone)).First();


                            string regionname = "";
                            if (region != null)
                            {
                                regionname = region.Name;

                            }
                            if (TotalCharge != ProcessedCharge)
                            {
                                //vetting went down
                                //loop through vetting

                                foreach (Entities.ClaimService service in item.ServiceList)
                                {
                                    if (!string.IsNullOrEmpty(service.VettingComment))
                                    {
                                        comment = comment + service.ServiceName.ToUpper() + " - " + service.VettingComment + ", ";

                                    }

                                }

                                foreach (ClaimDrug drug in item.DrugList)
                                {
                                    if (!string.IsNullOrEmpty(drug.VettingComment))
                                    {
                                        comment = comment + drug.DrugName.ToUpper() + " - " + drug.VettingComment + ", ";

                                    }
                                }
                            }



                            claimdatabase.Rows.Add(countit.ToString(), EnrolleeName.ToUpper(), Company.ToUpper(), PolicyNumber, HealthPlan, EncounterDate, datereceived, Diagnosis.ToUpper(), TreatmentTag, TotalCharge, ProcessedCharge, ChargeDifference, comment, providername, providerupn, regionname);
                            //totalrowcount++;


                            countit++;

                            totalsubmittedSum += Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount)));
                            totalProccessedSum += Convert.ToDecimal(Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount)) + Convert.ToDecimal(item.ServiceList.Sum(x => x.VettedAmount)));
                            totaldiffenrenceSum = totalsubmittedSum - totalProccessedSum;

                        }

                        claimdatabase.Rows.Add(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "TOTAL", totalsubmittedSum, totalProccessedSum, totaldiffenrenceSum, string.Empty);

                        //add two empty rows



                    }

                    //add two rows
                    //if (totalrowcount > 500)
                    //{
                    //    //change the sheet when exceeded
                    //    var nameofsheet = ws.Name;
                    //    nameofsheet = ws.Name + "-CT" + Guid.NewGuid().ToString();
                    //    zonediction[zoneiddd] = pck.Workbook.Worksheets.Add(nameofsheet.ToUpper());

                    //    totalrowcount = 1;

                    //    ws = zonediction[zoneiddd];

                    //}

                    int diff = totalrowcount;



                    //diff = diff + 2;
                    //Add the provider header
                    ws.Cells["A" + (diff).ToString()].Value = providername + " - " + provideraddress.ToLower();
                    ws.Cells["A" + (diff).ToString()].Style.Font.Bold = true;
                    ws.Cells["A" + (diff).ToString()].Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    ws.Cells["A" + (diff).ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                    ws.Cells["A" + (diff).ToString()].Style.Font.Color.SetColor(Color.White);


                    //leave two spaces
                    // diff = diff + 2;
                    //diff = diff + 1;



                    if (true)
                    {
                        using (ExcelRange rng = ws.Cells[diff + 1, 1, diff + 1, 13])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                            rng.Style.Font.Color.SetColor(Color.White);
                        }



                        ws.Cells["A" + (diff + 1).ToString()].LoadFromDataTable(claimdatabase, true);

                    }


                    totalrowcount = totalrowcount + claimdatabase.Rows.Count + 3;
                    SheetCount[zoneiddd] = totalrowcount;


                    claimdatabase.Rows.Clear();
                }
                //var narrartion = Utility.Tools.GetClaimsNarrations(allincomingClaims.FirstOrDefault());
                excelarray = pck.GetAsByteArray();
            }
            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath, "App_Data");
            string foldername = Guid.NewGuid().ToString();
            string fullpath = Path.Combine(appdatafolder, foldername);
            Directory.CreateDirectory(fullpath);

            //var excelarray = Utility.Tools.DumpExcelGetByte(claimdatabase);

            //write excel to folder

            System.IO.File.WriteAllBytes(Path.Combine(fullpath, foldername + ".xlsx"), excelarray);

            //send back to user
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=ExportedMemo.xlsx");
            Response.BinaryWrite(System.IO.File.ReadAllBytes(Path.Combine(fullpath, foldername + ".xlsx")));


            //return _uniquePageService.RedirectTo<ReviewedClaimsPage>();
            return null;
        }

        public ActionResult ExportMemoPaymentBatch(int id)
        {
            PaymentBatch paymentbatch = _claimsvc.getpaymentbatch(id);
            IList<Entities.ClaimBatch> allincomingClaims = paymentbatch.ClaimBatchList;

            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath, "App_Data");
            string foldername = Guid.NewGuid().ToString();
            string fullpath = Path.Combine(appdatafolder, foldername);
            Directory.CreateDirectory(fullpath);


            //write  the excels 

            //export all enrollees to excel
            DataTable test = new DataTable();
            test.Columns.Add("S/N", typeof(string));
            test.Columns.Add("NAME OF PROVIDER", typeof(string));
            test.Columns.Add("MONTH", typeof(string));
            test.Columns.Add("AMOUNT SUBMITTED", typeof(decimal));
            test.Columns.Add("PROCESSED AMOUNT", typeof(decimal));
            test.Columns.Add("DIFFERENCE", typeof(decimal));
            int count = 1;
            decimal totalsumited = 0m;
            decimal totalproccessed = 0m;

            decimal totaldiff = 0m;
            //var batchName = Enum.GetName(typeof(Utility.ClaimBatch), Convert.ToInt32(batchid));
            foreach (Entities.ClaimBatch batch in allincomingClaims)
            {
                Provider provider = _providerSvc.GetProvider(batch.ProviderId);
                string providername = provider.Name.ToUpper();
                string month = Tools.GetClaimsNarrations(batch);
                decimal Iamount = 0m;
                decimal Pamount = 0m;
                decimal Difference = 0m;
                foreach (Claim item in batch.Claims)
                {
                    Iamount += Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount) + item.ServiceList.Sum(x => x.InitialAmount));
                    Pamount += Convert.ToDecimal(item.DrugList.Sum(x => x.VettedAmount) + item.ServiceList.Sum(x => x.VettedAmount));
                    Difference = Iamount - Pamount;

                }


                // totalamount+= Iamount;
                totalproccessed += Pamount;
                totaldiff = totaldiff + Difference;
                totalsumited = totalsumited + Iamount;

                test.Rows.Add(count.ToString(), providername, month, Iamount, Pamount, Difference);



                count++;
            }


            test.Rows.Add(string.Empty, string.Empty, "TOTAL", totalsumited, totalproccessed, totaldiff);


            string the_month = "ALL BILLS IN ( " + paymentbatch.Title.ToUpper() + " ) PAYMENT BATCH";




            string str = string.Format("FEE FOR SERVICES PAYMENT (N{1})  FOR {0} ", the_month, totalproccessed.ToString("N"));




            byte[] excelarray = Tools.DumpExcelGetByte(test, str);

            //write excel to folder 

            System.IO.File.WriteAllBytes(Path.Combine(fullpath, foldername + ".xlsx"), excelarray);

            //zip folder and send to client

            //string zipPath = Path.Combine(appdatafolder, string.Format("{0}.zip", foldername));

            //ZipFile.CreateFromDirectory(fullpath, zipPath);

            //send back to user
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=ExportedMemo.xlsx");
            Response.BinaryWrite(System.IO.File.ReadAllBytes(Path.Combine(fullpath, foldername + ".xlsx")));


            //return _uniquePageService.RedirectTo<ReviewedClaimsPage>();
            return null;
        }

        [HttpGet]
        public JsonResult GetClaimbatchdetails(int id)
        {
            Entities.ClaimBatch claimbatch = _claimsvc.GetClaimBatch(id);
            ClaimBatchCloseResponse resp = new ClaimBatchCloseResponse();
            if (claimbatch != null)
            {
                decimal totalservice = 0m;
                decimal totaldrug = 0m;
                decimal totalSum = 0m;
                int counttt = 0;
                foreach (Claim item in claimbatch.Claims)
                {
                    totalservice = Convert.ToDecimal(item.ServiceList.Sum(x => x.InitialAmount));
                    totaldrug = Convert.ToDecimal(item.DrugList.Sum(x => x.InitialAmount));
                    counttt++;
                    totalSum = totalSum + totaldrug + totalservice;
                }
                resp.code = 0;
                resp.count = claimbatch.Claims.Count();
                resp.total = totalSum.ToString("N");
            }

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

    }




}
