using MrCMS.Entities.Messaging;
using MrCMS.Services;
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace MrCMS.Web.Apps.Core.Controllers
{
    public class ConnectCarePageController : MrCMSAppUIController<CoreApp>
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
        private readonly ServiceReference1.WebServiceSoapClient serv;


        public ConnectCarePageController(IPlanService planService, IUniquePageService uniquepageService,
                                      IPageMessageSvc pageMessageSvc, IHelperService helperService,
                                      IServicesService serviceSvc, IProviderService Providersvc, ILogAdminService logger,
                                      ITariffService tariffService, ICompanyService companyService,
                                      IEnrolleeService enrolleeService, ISmsService smsSvc, IUserChat UserChat, IClaimService claimservice, IUserService userservice)
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
            serv = new ServiceReference1.WebServiceSoapClient();
        }
        [HttpGet]
        public ActionResult AddSponsor(ConnectCareNewSponsorPage page)
        {
            return View(page);
        }

        public ActionResult TestService(FormCollection Form)
        {
            string idd = Form["previousid"];
            string fullname = Form["fullnametxt"];
            string gender = Form["gender"];
            string country = Form["country"];
            string state = Form["state"];
            string zipcode = Form["zipcode"];
            string address = Form["Address"];
            string phonenumber = Form["phonenumber"];
            string emaill = Form["email"];
            string occupation = Form["occupation"];
            string secondaryemail = Form["ssemail"];
            string addon = Form["addon"]; ;
            string instalment = Form["Instalment"];
            string policystart = Form["policystartdate"];
            string policyend = Form["policyenddate"];
            string policynotificationConfig = Form["Policynotificationconfig"];

            string bencount = Form["beneCount"];
            List<ConnectCareBeneficiary> benelist = new List<ConnectCareBeneficiary>();

            bool notnew = false;


            ConnectCareSponsor sponsordetails = new ConnectCareSponsor();

            bool sendpolicynumber = false;

            int previousid = 0;

            if (!string.IsNullOrEmpty(idd) && int.TryParse(idd, out previousid))
            {
                ConnectCareSponsor prsponsor = _helperSvc.GetSponsor(previousid);

                if (prsponsor == null)
                {
                    _pageMessageSvc.SetErrormessage("Seem the sponsor have been deleted from the system.");
                    return _uniquePageService.RedirectTo<ConnectCareNewSponsorPage>();

                }
                notnew = true;
                sponsordetails = prsponsor;

                DateTime startdate = Convert.ToDateTime(prsponsor.PolicyStartDate);
                DateTime enddate = Convert.ToDateTime(prsponsor.PolicyEndDate);


                if (!string.IsNullOrEmpty(policystart) && !string.IsNullOrEmpty(policyend))
                {
                    if (startdate.Year > 2012 && enddate.Year > 2012 && (prsponsor.PolicyStartDate != Tools.ParseMilitaryTime(policystart) || prsponsor.PolicyEndDate != Tools.ParseMilitaryTime(policyend)))
                    {
                        sendpolicynumber = true;
                    }
                }

            }


            sponsordetails.fullname = fullname;
            sponsordetails.gender = gender;
            sponsordetails.country = country;
            sponsordetails.state = state;
            sponsordetails.zipcode = zipcode;
            sponsordetails.address = address;
            sponsordetails.phonenumber = phonenumber;
            sponsordetails.email = emaill;
            sponsordetails.occupation = occupation;
            sponsordetails.secondarysponsor = secondaryemail;
            sponsordetails.Addon = addon == "0" ? false : true; ;
            sponsordetails.instalment = Convert.ToInt32(instalment);
            sponsordetails.PolicyStartDate = !string.IsNullOrEmpty(policystart) ? Tools.ParseMilitaryTime(policystart) : CurrentRequestData.Now.AddYears(-100);
            sponsordetails.PolicyEndDate = !string.IsNullOrEmpty(policyend) ? Tools.ParseMilitaryTime(policyend) : CurrentRequestData.Now.AddYears(-100);
            sponsordetails.PolicynotificationConfig = Convert.ToInt32(policynotificationConfig);

            if (!notnew)
            {
                sponsordetails.policynumber = _helperSvc.GenerateCCPolicyNumber();
                sponsordetails.sponsorSTRID = "";
                _helperSvc.addSponsor(sponsordetails);
            }
            else
            {
                _helperSvc.UpdateSponsor(sponsordetails);
            }





            List<ConnectCareBeneficiary> beneficiarylist = new List<ConnectCareBeneficiary>();



            int bene_count = 1;
            if (int.TryParse(bencount, out bene_count))
            {
                for (int i = 1; i <= bene_count; i++)
                {

                    bool benenotnew = false;
                    //Beneficiary
                    string prID = Form["bIDtxt_" + i.ToString()];

                    int prIDint = 0;


                    string bfirstname = Form["bfirstnametxt_" + i.ToString()];
                    string blastname = Form["blastnametxt_" + i.ToString()];
                    string bgender = Form["bgender_" + i.ToString()];
                    string bcategory = Form["bcat_" + i.ToString()];
                    string bdob = Form["bdob_" + i.ToString()];
                    string brelationship = Form["brelationship_" + i.ToString()];
                    string bcountry = Form["bcountry_" + i.ToString()];
                    string bstate = Form["bstate_" + i.ToString()];
                    string bcity = Form["bcity_" + i.ToString()];
                    string blga = Form["blga_" + i.ToString()];
                    string baddress = Form["baddress_" + i.ToString()];
                    string bemail = Form["bemail_" + i.ToString()];
                    string bphonnumber = Form["bphonenumber_" + i.ToString()];
                    string bgaudemail = Form["bgemail_" + i.ToString()];
                    string bgphone = Form["bgphonenumber_" + i.ToString()];

                    string suggprovider = Form["bsuggestedprovider_" + i.ToString()];
                    string suggplan = Form["bsuggestedplan_" + i.ToString()];
                    string verificationstatus = Form["bverificationstatus_" + i.ToString()];
                    string baddon = Form["baddon_" + i.ToString()];



                    ConnectCareBeneficiary benefit = new ConnectCareBeneficiary();
                    if (!string.IsNullOrEmpty(prID) && int.TryParse(prID, out prIDint))
                    {
                        ConnectCareBeneficiary pbene = _helperSvc.getBeneficiary(prIDint);

                        if (pbene != null)
                        {
                            benenotnew = true;
                            benefit = pbene;

                        }
                    }
                    benefit.sponsorid = sponsordetails.Id;
                    benefit.sponsoridstring = sponsordetails.sponsorSTRID;
                    benefit.firstname = bfirstname;
                    benefit.lastname = blastname;
                    benefit.gender = bgender;
                    if (!benenotnew)
                    {
                        benefit.PolicyNumber = sponsordetails.policynumber + "-" + i.ToString();
                    }

                    benefit.Category = bcategory;
                    benefit.dob = Tools.ParseMilitaryTime(bdob);
                    benefit.relationship = brelationship;
                    benefit.country = bcountry;
                    benefit.state = bstate;
                    benefit.city = bcity;
                    benefit.LGA = blga;
                    benefit.address = baddress;
                    benefit.email = bemail;
                    benefit.phonenumber = bphonnumber;
                    benefit.GuardianEmail = bgaudemail;
                    benefit.GuardianPhonenumber = bgphone;
                    benefit.suggestedPlan = suggplan;
                    benefit.SuggestedProvider = suggprovider;
                    benefit.VerificationStatus = verificationstatus == "1" ? true : false; ;
                    benefit.BeneficiaryID = "NHACCB-" + benefit.Id.ToString();
                    benefit.addon = baddon == "1" ? true : false;

                    //add beneficiary


                    beneficiarylist.Add(benefit);


                    _helperSvc.addBeneficiary(benefit);

                    benefit.BeneficiaryID = "NHACCB-" + benefit.Id.ToString();


                    _helperSvc.addBeneficiary(benefit);
                    //update

                }


            }

            //save to database
            //var resp = _helperSvc.addSponsor(sponsordetails);

            if (true)
            {
                ServiceReference1.sponsordata sponsor = new ServiceReference1.sponsordata();
                List<ServiceReference1.beneficiarydata> beneficiaries = new List<ServiceReference1.beneficiarydata>();
                //double[] balance = new double[10];

                sponsor.fullname = sponsordetails.fullname;
                sponsor.phonenumber = sponsordetails.phonenumber;
                sponsor.sponsorid = "NHACC-" + sponsordetails.Id.ToString();
                sponsor.email = sponsordetails.email;
                sponsor.country = sponsordetails.country;
                sponsor.state = sponsordetails.state;
                sponsor.gender = sponsordetails.gender;
                sponsor.address = sponsordetails.address;
                sponsor.zipcode = sponsordetails.zipcode;
                sponsor.occupation = sponsordetails.occupation;
                sponsor.Policynumber = string.Empty; //sponsordetails.policynumber;  no policynumber initially.
                sponsor.Instalment = sponsordetails.instalment;
                sponsor.addon = sponsordetails.Addon;
                sponsor.Policynotificationconfig = sponsordetails.PolicynotificationConfig;
                sponsor.SponsorStartDate = CurrentRequestData.Now.AddYears(-100); //Convert.ToDateTime(sponsordetails.PolicyStartDate);
                sponsor.PolicyStartDate = CurrentRequestData.Now.AddYears(-100); //Convert.ToDateTime(sponsordetails.PolicyStartDate);
                sponsor.PolicyEndDate = CurrentRequestData.Now.AddYears(-100); //Convert.ToDateTime(sponsordetails.PolicyEndDate);
                sponsor.SecondarySponsor = sponsordetails.secondarysponsor;

                foreach (ConnectCareBeneficiary item in beneficiarylist)
                {


                    ServiceReference1.beneficiarydata beneficiary = new ServiceReference1.beneficiarydata();

                    //beneficiary details
                    beneficiary.beneficiaryid = "NHACCB-" + item.Id.ToString();
                    beneficiary.sponsorid = sponsor.sponsorid;
                    beneficiary.fullname = item.firstname + " " + item.lastname;
                    beneficiary.email = item.email;
                    beneficiary.gender = item.gender;
                    beneficiary.phonenumber = item.phonenumber;
                    beneficiary.relationship = item.relationship;
                    beneficiary.category = item.Category;
                    beneficiary.dateofbirth = item.dob;
                    beneficiary.country = item.country;
                    beneficiary.state = item.state;
                    beneficiary.lga = item.LGA;
                    beneficiary.city = item.city;
                    beneficiary.guardianemail = item.GuardianEmail;
                    beneficiary.guardianphone = item.GuardianPhonenumber;
                    beneficiary.suggestedprovider = item.SuggestedProvider;
                    beneficiary.suggestedplan = item.suggestedPlan;
                    beneficiary.Policynumber = item.PolicyNumber;
                    beneficiary.addon = item.addon;
                    beneficiary.BeneficiaryStartDate = DateTime.Now.AddYears(2);
                    beneficiary.VerificationStatus = item.VerificationStatus;

                    beneficiaries.Add(beneficiary);

                    if (notnew && beneficiary.VerificationStatus)
                    {
                        //update the shit
                        string resp = serv.SENDVERIFIEDBENDTLS(beneficiary);
                    }


                }
                ServiceReference1.ArrayOfBeneficiarydata beneficairyyyydata = new ServiceReference1.ArrayOfBeneficiarydata();
                beneficairyyyydata.AddRange(beneficiaries);

                //send to Matontine
                try
                {

                    if (!notnew)
                    {//"success";


                        string respon = serv.SENDINSURDTLS(sponsor, beneficairyyyydata);

                        if (respon.ToLower().Contains("success"))
                        {
                            sponsordetails.sponsorSTRID = sponsor.sponsorid;
                            sponsordetails.pushedtoMatontine = true;
                            sponsordetails.pushedDate = CurrentRequestData.Now;
                            _helperSvc.UpdateSponsor(sponsordetails);

                        }
                    }
                    //not new and date changed


                    if (notnew && sendpolicynumber)
                    {

                        sponsor.Policynumber = sponsordetails.policynumber;
                        string respon = serv.SENDPOLICYNUMBER(sponsor, beneficairyyyydata);

                    }

                }
                catch (Exception ex)
                {
                    _pageMessageSvc.SetErrormessage("There was an error,sending to Matontine the data have been saved in the database.You can rety later by clicking on the push button.");

                    return _uniquePageService.RedirectTo<ConnectCareNewSponsorPage>();
                }



                //var receive = serv.RECVBENDTLS();


                //foreach(var item in receive)
                //{
                //    item.suggestedplan = "Bronze-001";
                //    item.Policynumber = "NHA-010";
                //    item.suggestedprovider = "Testing";
                //    item.country = "SN";
                //    item.state = "State";
                //    item.lga = "dere";
                //    item.BeneficiaryStartDate = CurrentRequestData.Now;
                //    item.gender = "Male";
                //    item.beneficiaryid = "NHACCB-0081";
                //    item.VerificationStatus = true;


                //    //var tesst=serv.SENDVERIFIEDBENDTLS(item);

                //}

                //var payment = serv.RECVPAYMTDTLS();



            }
            if (notnew)
            {
                _pageMessageSvc.SetSuccessMessage("Sponsor Updated successfully.");
            }
            else
            {
                _pageMessageSvc.SetSuccessMessage("Sponsor Added successfully.");
            }


            return _uniquePageService.RedirectTo<ConnectCareNewSponsorPage>();



        }


        public ActionResult PendingBene(ConnectCarePendingBenePage page)
        {

            try
            {
                ServiceReference1.ArrayOfBeneficiarydata receive = serv.RECVBENDTLS();

                //var shii = receive[6];

                foreach (ServiceReference1.beneficiarydata shii in receive)
                {
                    ConnectCareBeneficiary benefit = new ConnectCareBeneficiary();

                    string sponsorid = shii.sponsorid.Split('-').Last();
                    int sponsoridint = 0;

                    int.TryParse(sponsorid, out sponsoridint);

                    ConnectCareSponsor sponsordetails = _helperSvc.GetSponsor(sponsoridint);
                    IList<ConnectCareBeneficiary> beneList = _helperSvc.GetBeneficiariesBySponsorID(sponsoridint);

                    bool exist = beneList.Where(x => x.dob == shii.dateofbirth).Where(x => x.fullname.Contains(shii.fullname)).Any();


                    if (!exist && sponsordetails != null)
                    {
                        benefit.sponsorid = sponsordetails.Id;
                        benefit.sponsoridstring = sponsordetails.sponsorSTRID;
                        benefit.firstname = shii.fullname.Split(' ')[0];
                        benefit.lastname = shii.fullname.Split(' ')[1];
                        benefit.fullname = shii.fullname;
                        benefit.gender = shii.gender;
                        benefit.PolicyNumber = sponsordetails.policynumber + "-" + (beneList.Count() + 1).ToString();


                        benefit.Category = shii.category;
                        benefit.dob = shii.dateofbirth;
                        benefit.relationship = shii.relationship;
                        benefit.country = shii.country;
                        benefit.state = shii.state;
                        benefit.city = shii.city;
                        benefit.LGA = shii.lga;
                        benefit.address = string.Empty;
                        benefit.email = shii.email;
                        benefit.phonenumber = shii.phonenumber;
                        benefit.GuardianEmail = shii.guardianemail;
                        benefit.GuardianPhonenumber = shii.guardianphone;
                        benefit.suggestedPlan = string.Empty;
                        benefit.SuggestedProvider = string.Empty;
                        benefit.VerificationStatus = false; ;

                        benefit.addon = true;
                        _helperSvc.addBeneficiary(benefit);
                        benefit.BeneficiaryID = "NHACCB-" + benefit.Id.ToString();
                        //update the beneficiary id
                        _helperSvc.addBeneficiary(benefit);


                    }


                }

                ViewBag.exported = receive;

            }
            catch (Exception ex)
            {
                _pageMessageSvc.SetErrormessage("There was an error connecting to Matontine Server.Please try again.");

            }


            return View(page);
        }


        public string ReceivePayment()
        {
            ServiceReference1.ArrayOfPaymentdata receive = serv.RECVPAYMTDTLS();
            ConnectcarePaymentDetails payobj2 = new ConnectcarePaymentDetails();

            //payobj2.paymentid = "TXT08949488955";
            //payobj2.sponsorID = 5;
            //payobj2.sponsorIDString = "NHACC-5";
            //payobj2.beneficiaryID = "NHA-ACCB-49";
            //payobj2.policyid = "NHACC-008";
            //payobj2.amountpaid = 250;
            //payobj2.addon = true;
            //payobj2.planpurchased = "Gold-003";
            //payobj2.paymentDate = CurrentRequestData.Now;
            //_helperSvc.addpaymentdetails(payobj2);
            foreach (ServiceReference1.paymentdata payment in receive)
            {
                //add only if it does not exist
                if (!_helperSvc.checkpaymentuniqueID(payment.paymentid))
                {
                    ConnectcarePaymentDetails payobj = new ConnectcarePaymentDetails();
                    string sponsorid = payment.sponsorid.Split('-').Last();
                    int sponsoridint = 0;
                    int.TryParse(sponsorid, out sponsoridint);
                    payobj.paymentid = payment.paymentid;
                    payobj.sponsorID = sponsoridint;
                    payobj.sponsorIDString = payment.sponsorid;
                    payobj.beneficiaryID = payment.beneficiaryid;
                    payobj.policyid = payment.policyid;
                    payobj.amountpaid = payment.amountpaid;
                    payobj.addon = payment.addon;
                    payobj.planpurchased = payment.planpurchased;
                    payobj.paymentDate = payment.paymentdate;

                    _helperSvc.addpaymentdetails(payobj);

                    //send email to connectcare guys
                }





            }

            return "Done";
        }

        public JsonResult QueryAllPayment()
        {

            IList<ConnectcarePaymentDetails> output = _helperSvc.getAllPaymentDetails();

            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });
        }

        public JsonResult GetAllPendingBeneficiary()
        {
            IOrderedEnumerable<ConnectCareBeneficiary> output = _helperSvc.getAllUnverifiedBeneficiary().OrderByDescending(x => x.CreatedOn);

            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });
        }

        [HttpGet]
        public ActionResult PaymentList(ConnectCarePaymentListPage page)
        {
            return View(page);
        }

        [HttpPost]
        public ActionResult UpdateBeneficiary(FormCollection Form)
        {
            bool success = false;
            for (int i = 1; i <= 1; i++)
            {
                //Beneficiary

                string bfirstname = Form["bfirstnametxt_" + "1"];
                string blastname = Form["blastnametxt_" + "1"];
                string bgender = Form["bgender_" + "1"];
                string bcategory = Form["bcat_" + "1"];
                string bdob = Form["bdob_" + "1"];
                string brelationship = Form["brelationship_" + "1"];
                string bcountry = Form["bcountry_" + "1"];
                string bstate = Form["bstate_" + "1"];
                string bcity = Form["bcity_" + "1"];
                string blga = Form["blga_" + "1"];
                string baddress = Form["baddress_" + "1"];
                string bemail = Form["bemail_" + "1"];
                string bphonnumber = Form["bphonenumber_" + "1"];
                string bgaudemail = Form["bgemail_" + "1"];
                string bgphone = Form["bgphonenumber_" + "1"];

                string suggprovider = Form["bsuggestedprovider_" + "1"];
                string suggplan = Form["bsuggestedplan_" + "1"];
                string verificationstatus = Form["bverificationstatus_" + "1"];
                string baddon = Form["baddon_" + "1"];

                ServiceReference1.beneficiarydata beneficiary = new ServiceReference1.beneficiarydata();
                //beneficiary details
                beneficiary.beneficiaryid = Form["beneid"];
                beneficiary.sponsorid = Form["sponsorid"];
                beneficiary.fullname = bfirstname;
                beneficiary.email = bemail;
                beneficiary.gender = bgender;
                beneficiary.phonenumber = bphonnumber;
                beneficiary.relationship = brelationship;
                beneficiary.category = bcategory;
                beneficiary.dateofbirth = Tools.ParseMilitaryTime(bdob);
                beneficiary.country = bcountry;
                beneficiary.state = bstate;
                beneficiary.lga = blga;
                beneficiary.city = bcity;
                beneficiary.guardianemail = bgaudemail;
                beneficiary.guardianphone = bgphone;
                beneficiary.suggestedprovider = suggprovider;
                beneficiary.suggestedplan = suggplan;
                beneficiary.Policynumber = string.Empty; //item.PolicyNumber;
                beneficiary.addon = baddon == "1" ? true : false;
                beneficiary.BeneficiaryStartDate = DateTime.Now.AddYears(2);
                beneficiary.VerificationStatus = verificationstatus == "1" ? true : false;
                try
                {
                    string resp = serv.SENDVERIFIEDBENDTLS(beneficiary);
                    success = true;
                }
                catch (Exception ex)
                {

                }

            }



            if (success)
            {
                _pageMessageSvc.SetSuccessMessage("Beneficiary Updated Successfully.");


            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was an error updating.");
            }
            return _uniquePageService.RedirectTo<ConnectCarePendingBenePage>();


        }



        public ActionResult ManageSponsor(ManageSponsorPage page)
        {
            return View(page);
        }


        public JsonResult QueryConnectCareSponsors()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];

            string scrPolicynumber = CurrentRequestData.CurrentContext.Request["src_policynumber"];

            string fullname = CurrentRequestData.CurrentContext.Request["scr_fullname"];
            string senttomatontine = CurrentRequestData.CurrentContext.Request["scr_senttomatontine"];
            string scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            string scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            string scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];

            string search = CurrentRequestData.CurrentContext.Request["sSearch"];

            string displayLength2 = CurrentRequestData.CurrentContext.Request["iDisplayLength2"];
            int toltareccount = 0;
            int totalinresult = 0;

            DateTime fromdate = CurrentRequestData.Now;
            DateTime todate = CurrentRequestData.Now;
            bool usedate = false;

            int senttotine = -1;
            int.TryParse(senttomatontine, out senttotine);


            IList<ConnectCareSponsor> queryresult = _helperSvc.QueryConnectCareSponsor(out toltareccount, out totalinresult, search, Convert.ToInt32(displayStart), Convert.ToInt32(displayLength), fullname, scrPolicynumber, senttotine, usedate, fromdate, todate);

            List<connectCareSponsorResponse> output = new List<connectCareSponsorResponse>();



            foreach (ConnectCareSponsor item in queryresult)
            {
                connectCareSponsorResponse obbb = new connectCareSponsorResponse();
                obbb.Sponsor = item;
                obbb.beneficiaries = _helperSvc.GetBeneficiariesBySponsorID(item.Id);
                obbb.benecount = obbb.beneficiaries.Count();
                output.Add(obbb);
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
        public ActionResult ExpandSponsor(int Id, ExpandSponsorPage page)
        {
            ConnectCareSponsor sponsor = _helperSvc.GetSponsor(Id);
            IList<ConnectCareBeneficiary> beneficiary = _helperSvc.GetBeneficiariesBySponsorID(Id);


            if (sponsor != null)
            {
                connectCareSponsorResponse shii = new connectCareSponsorResponse()
                {
                    Sponsor = sponsor,
                    beneficiaries = beneficiary,
                    benecount = beneficiary.Count()
                };
                ViewBag.sponsor = shii;

            }



            return View(page);
        }
    }
}