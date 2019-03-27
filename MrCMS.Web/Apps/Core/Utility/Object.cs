using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Website;
using System.Data;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using MrCMS.Web.Apps.Core.Entities;
using System.Globalization;

namespace MrCMS.Web.Apps.Core.Utility
{
    public class MyCustomDateProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
                return this;

            return null;
        }



        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!(arg is DateTime)) throw new NotSupportedException();

            DateTime dt = (DateTime)arg;

            string suffix;

            if (new[] { 11, 12, 13 }.Contains(dt.Day))
            {
                suffix = "th";
            }
            else if (dt.Day % 10 == 1)
            {
                suffix = "st";
            }
            else if (dt.Day % 10 == 2)
            {
                suffix = "nd";
            }
            else if (dt.Day % 10 == 3)
            {
                suffix = "rd";
            }
            else
            {
                suffix = "th";
            }

            return string.Format("{1}{2} {0:MMM}, {0:yyyy}", arg, dt.Day, suffix);
        }
    }
    public class EnrolleePolicyName
    {
        public virtual int Id { get; set; }
        public virtual string Policynumber { get; set; }
        public virtual string Name { get; set; }

    }
    public class connectCareSponsorResponse
    {
        public virtual ConnectCareSponsor Sponsor { get; set; }
        public virtual IList<ConnectCareBeneficiary> beneficiaries { get; set; }
        public virtual int benecount { get; set; }

    }
    public class UploadTariffCSV
    {
        public virtual string itemname { get; set; }
        public virtual string unit { get; set; }
        public virtual decimal price { get; set; }
        public virtual string authorizationRequired { get; set; }
        public virtual string remark { get; set; }


    }

    public class PaymentBatchResponse
    {

        public virtual string Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string description { get; set; }
        public virtual string Claimcount { get; set; }
        public virtual string TotalAmount { get; set; }
        public virtual string TotalPaid { get; set; }
        public virtual string datepaymentstarted { get; set; }
        public virtual string datepaymentcompleted { get; set; }
        public virtual string Status { get; set; }
        public virtual string createdby { get; set; }
        public virtual string paidby { get; set; }
        public virtual string datecreated { get; set; }


    }


    public class PaymentBatchCSVEXPORT
    {

        public virtual int Id { get; set; }
        public virtual string ClaimsPeriod { get; set; }
        public virtual string UPN { get; set; }
        public virtual string Provider { get; set; }
        public virtual string ProviderAddress { get; set; }
        public virtual string Providerzone { get; set; }
        public virtual string ClaimBatch { get; set; }
        public virtual string InitialAmount { get; set; }
        public virtual string ProcessedAmount { get; set; }
        public virtual string Status { get; set; }
        public virtual string AmountPaid { get; set; }
        public virtual string PaymentMethod { get; set; }
        public virtual string PaymentReference { get; set; }
        public virtual string SourceBankName { get; set; }
        public virtual string SourceAccountNumber { get; set; }
        public virtual string DestinationBankName { get; set; }
        public virtual string DestinationAccountNumber { get; set; }
        public virtual string PaymentDate { get; set; }
        public virtual string paidby { get; set; }
        public virtual string remark { get; set; }



    }

    //public class namevalue
    //{
    //    public virtual 
    //}
    public class claimbkresp
    {
        public virtual string key { get; set; }
        public virtual object data { get; set; }

    }
    public class CompanyObj
    {

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual string State { get; set; }
        public virtual string Address { get; set; }
        public virtual string Website { get; set; }
        public virtual string Email { get; set; }
        public virtual string subsidiary { get; set; }
        public virtual string SubscriptionStatus { get; set; }
        public virtual string CreatedOn { get; set; }
    }
    public class VerificationCodeResponse
    {
        public virtual string Id { get; set; }
        public virtual string Enrolleeid { get; set; }
        public virtual bool EnrolleeisChild { get; set; }
        public virtual string StaffProfileId { get; set; }
        public virtual string EnrolleePolicy { get; set; }
        public virtual string Enrolleename { get; set; }
        public virtual string visittype { get; set; }
        public virtual string Dateencountered { get; set; }

        public virtual string Verificationcode { get; set; }
        public virtual string Verficationstatus { get; set; }
        public virtual string Providerused { get; set; }
        public virtual string Channel { get; set; }
        public virtual string Purpose { get; set; }
        public virtual string Dateauthenticated { get; set; }
        public virtual string Dateexpired { get; set; }

        public virtual bool Showcall { get; set; }
        public virtual bool ShowEdit { get; set; }
        public virtual bool ShowCallToUser { get; set; }
    }
    public class EnrolleeModel
    {
        public virtual int Id { get; set; }
        public virtual string Img { get; set; }
        public virtual string Name { get; set; }
        public virtual string PolicyNum { get; set; }
        public virtual string DoB { get; set; }
        public virtual string Sex { get; set; }
        public virtual string Occupation { get; set; }
        public virtual string Maritalstatus { get; set; }
        public virtual string Hospital { get; set; }
        public virtual string Mobile { get; set; }
        public virtual string State { get; set; }
        public virtual string Address { get; set; }
        public virtual string Company { get; set; }
        public virtual string Subsidiary { get; set; }
        public virtual string HealthPlan { get; set; }
        public virtual string Provider { get; set; }
        public virtual bool IsExpunged { get; set; }
        public virtual bool HasSubscription { get; set; }
        public virtual string Email { get; set; }
        public virtual bool IsChild { get; set; }
        public virtual bool IsCommunity { get; set; }
        public virtual bool IsDuplicate { get; set; }
        public virtual int StaffProfileId { get; set; }
        public virtual string SubscriptionExpirationDate { get; set; }
        public virtual bool IdPrinted { get; set; }
        public virtual bool AboveLimit { get; set; }
        public virtual string DateCreated { get; set; }
        public virtual bool isrenewal { get; set; }



    }

    public class EnrolleeDetailsClaim
    {
        public virtual string Id { get; set; }
        public virtual string EnrolleeName { get; set; }
        public virtual string CompanyId { get; set; }
        public virtual string EnrolleeCompany { get; set; }
        public virtual string EnrolleeSubsidiary { get; set; }
        public virtual string EnrolleeGender { get; set; }
        public virtual string EnrolleePlan { get; set; }
        public virtual bool isexpunged { get; set; }
        public virtual int respCode { get; set; }
        public virtual string errorMsg { get; set; }
    }

    public class PendingDependantbox
    {
        public virtual int Id { get; set; }
        public virtual string ImgRaw { get; set; }
        public virtual string DependantFullname { get; set; }
        public virtual string principalpolicynumber { get; set; }
        public virtual string principalplan { get; set; }
        public virtual int noofdep { get; set; }
        public virtual string dob { get; set; }
        public virtual string relationship { get; set; }
        public virtual string Gender { get; set; }
        public virtual string provider { get; set; }
        public virtual string submitted { get; set; }
    }
    public class VerificationResult
    {

        public virtual string Passport { get; set; }
        public virtual string Fullname { get; set; }
        public virtual string PolicyNumber { get; set; }
        public virtual string Gender { get; set; }
        public virtual string Company { get; set; }
        public virtual string Verificationcode { get; set; }
        public virtual string Hospital { get; set; }
        public virtual string Dategenerated { get; set; }
        public virtual string Dateauthenticated { get; set; }
        public virtual string rcode { get; set; }
        public virtual string rmsg { get; set; }
    }
    public class MobilesignupResponse
    {
        public virtual string Code { get; set; }
        public virtual string Message { get; set; }
        public virtual string VerificationCode { get; set; }
    }
    public class BulkUploadModel
    {
        public virtual string Code { get; set; }
        public virtual string Message { get; set; }
        public virtual string VerificationCode { get; set; }
    }
    public class MobileProfileResponse
    {
        public virtual string Code { get; set; }
        public virtual string Message { get; set; }
        public virtual string Passport { get; set; }
        public virtual string EnrolleeFullName { get; set; }
        public virtual string Dob { get; set; }
        public virtual string MaritalStatus { get; set; }
        public virtual string Sex { get; set; }
        public virtual string Email { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Address { get; set; }
        public virtual string Company { get; set; }
        public virtual string Provider { get; set; }
        public virtual string Plan { get; set; }
        public virtual bool Subscriptionstatus { get; set; }
        public virtual bool IsPrincipal { get; set; }
        public virtual bool IsExpunged { get; set; }
    }
    public class MobileEnrolleeTied
    {
        public virtual string PolicyNumber { get; set; }
        public virtual string FullName { get; set; }

    }
    public class ClaimBatchCloseResponse
    {
        public virtual int code { get; set; }
        public virtual int count { get; set; }
        public virtual string total { get; set; }
    }
    public class MobileVisitPurpose
    {
        public virtual string PurposeName { get; set; }


    }
    //                        groupp= _companyService.GetCompany(areply.Companyid).Name.ToUpper(),
    //                        img = Convert.ToBase64String(areply.EnrolleePassport.Imgraw),
    //                        name= areply.Surname +" " +  areply.Othernames,
    //                        policynum = areply.Policynumber,
    //                        dob = Convert.ToDateTime(areply.Dob).ToString("MMM dd yyyy"),
    //                        sex = Enum.GetName(typeof(Sex), areply.Sex),
    //                        occupation = areply.Occupation,
    //                       MaritalStatus =Enum.GetName(typeof(MaritalStatus),areply.Maritalstatus),
    //                        hospital = _providerSvc.GetProvider(areply.Primaryprovider).Name.ToUpper(),
    //                        mobile = areply.Mobilenumber,
    //                       State = _helperSvc.GetState( areply.Stateid ).Name,
    //                       address = areply.Residentialaddress,
    //                        subsidiary = _companyService.Getstaff(areply.Staffprofileid).CompanySubsidiary < 1 ?  _companyService.Getsubsidiary( _companyService.Getstaff(areply.Staffprofileid).CompanySubsidiary).Subsidaryname : string.Empty,
    //                        healthplan = _planService.GetPlan(_companyService.GetCompanyPlan(_companyService.Getstaff(areply.Staffprofileid).StaffPlanid).Planid).Name.ToUpper(),
    //                        provider=_providerSvc.GetProvider(areply.Primaryprovider).Name.ToUpper(),

    //}





    public class CompanyPlanRespomse
    {
        public virtual string Id { get; set; }

        public virtual string groupname { get; set; }
        public virtual string plantype { get; set; }
        public virtual string name { get; set; }
        public virtual string description { get; set; }
        public virtual string datecreated { get; set; }
        public virtual string subsidiary { get; set; }
    }

    public class GenericReponse
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
    }
    public class TariffGenericReponse
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Price { get; set; }
    }
    public class GenericReponse2
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
    }
    public class ProviderReponse
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string State { get; set; }
        public virtual string Address { get; set; }
    }

    public class Downloadfilemodel
    {

        public virtual int Id { get; set; }
        public virtual string Filename { get; set; }
        public virtual string Datecreated { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual string DownloadCount { get; set; }
        public virtual string Downloadlink { get; set; }

    }
    public class IncomingClaimsResponse
    {
        public virtual string Id { get; set; }
        public virtual string GroupName { get; set; }
        public virtual string Provider { get; set; }
        public virtual string ClaimsPeriod { get; set; }
        public virtual string DeliveredBy { get; set; }
        public virtual string ReceivedBy { get; set; }
        public virtual string TransferedTo { get; set; }
        public virtual string NoOfEncounter { get; set; }
        public virtual string TotalAmount { get; set; }
        public virtual string Caption { get; set; }
        public virtual string trackingID { get; set; }
        public virtual string month_string { get; set; }
        public virtual string Note { get; set; }
        public virtual string DateReceived { get; set; }
        public virtual string DateLogged { get; set; }
    }

    public class ClaimsBatchResponse
    {

        public virtual int Id { get; set; }
        public virtual string GroupName { get; set; }
        public virtual string Provider { get; set; }
        public virtual string PRoviderAddress { get; set; }
        public virtual string Batch { get; set; }
        public virtual string deliveryCount { get; set; }
        public virtual string claimscount { get; set; }
        public virtual string totalAmount { get; set; }
        public virtual string totalProccessed { get; set; }
        public virtual string difference { get; set; }
        public virtual string CapturedBy { get; set; }
        public virtual string Caption { get; set; }
        public virtual string Note { get; set; }
        public virtual bool isSubmittedRemotely { get; set; }
        public virtual string DateSubmitedForVetting { get; set; }
        public virtual string narration { get; set; }
        public virtual string deliverydate { get; set; }

        public virtual string month_string { get; set; }
        public virtual string ClaimStatus { get; set; }
    }

    public class AuthorizationcodeResponse
    {
        public virtual int Id { get; set; }
        public virtual string authorizationCode { get; set; }
        public virtual string provider { get; set; }
        public virtual string providerid { get; set; }
        public virtual string policynumber { get; set; }
        public virtual string enrolleename { get; set; }
        public virtual string companyname { get; set; }
        public virtual string plan { get; set; }
        public virtual string age { get; set; }
        public virtual string diagnosis { get; set; }
        public virtual string authorizationfor { get; set; }
        public virtual string category { get; set; }
        public virtual string requestersname { get; set; }
        public virtual string requestersphone { get; set; }
        public virtual string isadmission { get; set; }
        public virtual string AdmissionStatus { get; set; }
        public virtual string AdmissionDate { get; set; }
        public virtual string DaysApproved { get; set; }
        public virtual string DaysUsed { get; set; }
        public virtual string DischargeDate { get; set; }
        public virtual string DischargeDateshort { get; set; }
        public virtual string authorizeduser { get; set; }
        public virtual string whatwasauthorized { get; set; }
        public virtual string note { get; set; }
        public virtual string createdby { get; set; }
        public virtual string createdDate { get; set; }

    }


    public class DependantInfomation
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string dob { get; set; }
        public virtual string sex { get; set; }
        public virtual string hospital { get; set; }
        public virtual string mobile { get; set; }
        public virtual string preexisting { get; set; }
        public virtual string relationship { get; set; }
        public virtual string img { get; set; }
        public virtual int providerID { get; set; }
        public virtual string policynum { get; set; }
        public virtual bool aboveage { get; set; }
        public virtual bool ispending { get; set; }



    }
    public class staffshit
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string staffPlan { get; set; }
        public virtual string staffidcardno { get; set; }
    }
    public class StaffnameandPlan
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string PlanId { get; set; }
        public virtual string PlanName { get; set; }
        public virtual bool Expunged { get; set; }
        public virtual bool hasprofile { get; set; }
        public virtual int Subsidiary { get; set; }
        public virtual IList<EnrolleeModel> DependantEnrollee { get; set; }
        public virtual bool CanAddDependants { get; set; }

    }
    public class TariffObject
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string Status { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual string CreatedDate { get; set; }
        public virtual string authstatus { get; set; }
        public virtual string authBy { get; set; }
        public virtual string AuthorizedDate { get; set; }
        public virtual string Empty { get; set; }
    }


    public class MedicalHistory
    {
        public virtual string EnrolleePolicynumber { get; set; }
        public virtual string EnrolleeName { get; set; }

        public virtual string providerName { get; set; }
        public virtual string providerAddress { get; set; }
        public virtual string providerID { get; set; }
        public virtual string Tags { get; set; }
        public virtual string Diagnosis { get; set; }
        public virtual bool isdental { get; set; }
        public virtual bool isOptical { get; set; }
        public virtual string InitialAmount { get; set; }
        public virtual string ProcessedAmount { get; set; }
        public virtual string DateEncounter { get; set; }
        public virtual string DateReceived { get; set; }
    }
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }
    }
    public static class Tools
    {

        public static byte[] DumpExcelGetByte(DataTable tbl)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("EnrolleeList");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(tbl, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:C1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                //Example how to Format Column 1 as numeric 
                using (ExcelRange col = ws.Cells[2, 1, 2 + tbl.Rows.Count, 1])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                //Write it back to the client
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                // Response.AddHeader("content-disposition", "attachment;  filename=EnrolleeListDatabase.xlsx");
                //Response.BinaryWrite(pck.GetAsByteArray());

                return pck.GetAsByteArray();


            }
        }

        public static float compareNames(string name1, string name2)
        {
            string content = Regex.Replace(name1.Trim(), @"\s+", ":");
            string[] namelist = content.Split(':');
            int existcount = 0;
            float percent = 0;

            foreach (string name in namelist)
            {
                if (name2.ToLower().Contains(name.ToLower()))
                {
                    existcount++;
                }


            }


            float dividend = existcount;
            float divisor = namelist.Length;
            percent = dividend / divisor;
            percent = percent * 100;

            return percent;

        }
        public static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
        public static string GetClaimsNarrations(Entities.ClaimBatch batch)
        {
            //if (batch == null)
            //{
            //    return "";
            //}
            //var incomingclaimsorder = batch.Claims.OrderBy(x => x.ServiceDate);
            //var response = "";
            //var year = new List<int>();
            //foreach (var item in incomingclaimsorder)
            //{
            //    if (!year.Contains(Convert.ToDateTime(item.ServiceDate).Year) && Convert.ToDateTime(item.ServiceDate).Year > 2014)
            //    {
            //        year.Add(Convert.ToDateTime(item.ServiceDate).Year);
            //    }
            //}
            //var count = 0;
            //foreach (var item in year)
            //{

            //    if (count > 0)
            //    {
            //        response = response + "|";
            //    }
            //    count++;
            //    var monthlist = new List<int>();
            //    var claimsforyear = incomingclaimsorder.Where(x => Convert.ToDateTime(x.ServiceDate).Year == item).OrderBy(x => x.ServiceDate);


            //    foreach (var itemo in claimsforyear)
            //    {
            //        if (!monthlist.Contains(Convert.ToDateTime(itemo.ServiceDate).Month))
            //        {
            //            monthlist.Add(Convert.ToDateTime(itemo.ServiceDate).Month);

            //        }
            //    }


            //    foreach (var month in monthlist)
            //    {

            //        response = response + string.Format("{0},", CurrentRequestData.CultureInfo.DateTimeFormat.GetMonthName(month));

            //    }

            //    response = response + " " + item.ToString();

            //}
            string response = "";
            IncomingClaims income = batch.IncomingClaims.FirstOrDefault();

            if (income != null)
            {
                if (!string.IsNullOrEmpty(income.month_string) && income.month_string.Split(',').Count() > 0)
                {
                    foreach (string itemmm in income.month_string.Split(','))
                    {
                        response = response + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(itemmm)) + ",";
                    }
                    response = response + income.year.ToString();


                }
            }

            return response;

        }
        public static void GenerateHeapPermutations(int n, ref string[] itemList, List<string> sList)
        {
            if (n == 1)
            {

                string str = "";
                foreach (string item in itemList)
                {
                    str = str + " " + item;
                }
                sList.Add(str);
            }
            else
            {
                for (int i = 0; i < n - 1; i++)
                {
                    GenerateHeapPermutations(n - 1, ref itemList, sList);

                    if (n % 2 == 0)
                    {
                        // swap the positions of two characters

                        string temp = itemList[i];
                        itemList[i] = itemList[n - 1];
                        itemList[n - 1] = temp;
                        //itemList  = new String(charArray);
                    }
                    else
                    {

                        string temp = itemList[0];
                        itemList[0] = itemList[n - 1];
                        itemList[n - 1] = temp;
                        //s = new String(charArray);
                    }
                }

                GenerateHeapPermutations(n - 1, ref itemList, sList);
            }
        }

        public static string ConvertToLongDate(DateTime input)
        {
            return input.ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
        }
        public static DateTime ParseMilitaryTime(string time, int year, int month, int day)
        {
            //
            // Convert hour part of string to integer.
            //
            string hour = time.Substring(0, 2);
            int hourInt = int.Parse(hour);
            if (hourInt >= 24)
            {
                throw new ArgumentOutOfRangeException("Invalid hour");
            }
            //
            // Convert minute part of string to integer.
            //
            string minute = time.Substring(2, 2);
            int minuteInt = int.Parse(minute);
            if (minuteInt >= 60)
            {
                throw new ArgumentOutOfRangeException("Invalid minute");
            }
            //
            // Return the DateTime.
            //
            return new DateTime(year, month, day, hourInt, minuteInt, 0);
        }
        public static DateTime ParseMilitaryTime(string datetime)
        {
            return ParseMilitaryTime("0101", Convert.ToInt32(datetime.Substring(6, 4)), Convert.ToInt32(datetime.Substring(3, 2)), Convert.ToInt32(datetime.Substring(0, 2)));

        }

        public static ClaimBatch CheckClaimBatch(DateTime ReceivedDate)
        {
            if (ReceivedDate.Day > 15)
            {
                return ClaimBatch.BatchB;
            }
            else
            {
                return ClaimBatch.BatchA;
            }
        }

        public static byte[] DumpExcelGetByte(DataTable tbl, string batchstr)
        {

            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath, "App_Data");

            string folderpath = Path.Combine(appdatafolder, "DropUpload");
            string filename = Path.Combine(folderpath, "memoTemplate.xlsx");
            byte[] file = File.ReadAllBytes(filename);
            MemoryStream ms = new MemoryStream(file);
            using (ExcelPackage pck = new ExcelPackage(ms))
            {
                if (pck.Workbook.Worksheets.Count == 0)
                {
                    string strError = "Your Excel file does not contain any work sheets";
                }

                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.FirstOrDefault();

                ws.InsertRow(32, tbl.Rows.Count, 31);
                ws.Cells["D31"].LoadFromDataTable(tbl, false);

                //set date
                ws.Cells["G12"].Value = CurrentRequestData.Now;
                ws.Cells["E23"].Value = batchstr;



                //Format the header for column 1-3
                //using (ExcelRange rng = ws.Cells["D32:I32"])
                //{
                //    rng.Style.Font.Bold = true;
                //    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                //    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                //    rng.Style.Font.Color.SetColor(Color.White);
                //}

                //Example how to Format Column 1 as numeric 
                //using (ExcelRange col = ws.Cells[32, 1, 2 + tbl.Rows.Count, 1])
                //{
                //    col.Style.Numberformat.Format = "#,##0.00";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}
                //ws.InsertRow(34, 4,34);

                //ws.Cells[35,6].FormulaR1C1 = ws.Cells[34, 6].FormulaR1C1;



                return pck.GetAsByteArray();


            }
        }
        public static byte[] DumpExcelGetByteAdvice(DataTable tbl, string providername, string provideraddress)
        {

            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath, "App_Data");

            string folderpath = Path.Combine(appdatafolder, "DropUpload");
            string filename = Path.Combine(folderpath, "adviceTemplate.xlsx");
            byte[] file = File.ReadAllBytes(filename);
            MemoryStream ms = new MemoryStream(file);
            using (ExcelPackage pck = new ExcelPackage(ms))
            {
                if (pck.Workbook.Worksheets.Count == 0)
                {
                    string strError = "Your Excel file does not contain any work sheets";
                }

                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.FirstOrDefault();

                ws.InsertRow(29, tbl.Rows.Count, 28);
                ws.Cells["D28"].LoadFromDataTable(tbl, false);

                //set date
                ws.Cells["M12"].Value = CurrentRequestData.Now;
                ws.Cells["E15"].Value = provideraddress;
                ws.Cells["E13"].Value = providername;



                //Format the header for column 1-3
                //using (ExcelRange rng = ws.Cells["D32:I32"])
                //{
                //    rng.Style.Font.Bold = true;
                //    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                //    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                //    rng.Style.Font.Color.SetColor(Color.White);
                //}

                //Example how to Format Column 1 as numeric 
                //using (ExcelRange col = ws.Cells[32, 1, 2 + tbl.Rows.Count, 1])
                //{
                //    col.Style.Numberformat.Format = "#,##0.00";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}
                //ws.InsertRow(34, 4,34);

                //ws.Cells[35,6].FormulaR1C1 = ws.Cells[34, 6].FormulaR1C1;



                return pck.GetAsByteArray();


            }
        }

        public static string NumberToCurrencyText(decimal number, MidpointRounding midpointRounding)
        {
            // Round the value just in case the decimal value is longer than two digits
            number = decimal.Round(number, 2, midpointRounding);

            string wordNumber = string.Empty;

            // Divide the number into the whole and fractional part strings
            string[] arrNumber = number.ToString().Split('.');

            // Get the whole number text
            long wholePart = long.Parse(arrNumber[0]);
            string strWholePart = NumberToText(wholePart);

            // For amounts of zero dollars show 'No Dollars...' instead of 'Zero Dollars...'
            wordNumber = (wholePart == 0 ? "No" : strWholePart) + (wholePart == 1 ? " Naira and " : " Naira and ");

            // If the array has more than one element then there is a fractional part otherwise there isn't
            // just add 'No Cents' to the end
            if (arrNumber.Length > 1)
            {
                // If the length of the fractional element is only 1, add a 0 so that the text returned isn't,
                // 'One', 'Two', etc but 'Ten', 'Twenty', etc.
                long fractionPart = long.Parse((arrNumber[1].Length == 1 ? arrNumber[1] + "0" : arrNumber[1]));
                string strFarctionPart = NumberToText(fractionPart);

                wordNumber += (fractionPart == 0 ? " No" : strFarctionPart) + (fractionPart == 1 ? " Kobo" : " Kobo");
            }
            else
                wordNumber += "No Kobo";

            return wordNumber;
        }


        public static string NumberToText(long number)
        {
            StringBuilder wordNumber = new StringBuilder();

            string[] powers = new string[] { "Thousand ", "Million ", "Billion " };
            string[] tens = new string[] { "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
            string[] ones = new string[] { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                                       "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };

            if (number == 0) { return "Zero"; }
            if (number < 0)
            {
                wordNumber.Append("Negative ");
                number = -number;
            }

            long[] groupedNumber = new long[] { 0, 0, 0, 0 };
            int groupIndex = 0;

            while (number > 0)
            {
                groupedNumber[groupIndex++] = number % 1000;
                number /= 1000;
            }

            for (int i = 3; i >= 0; i--)
            {
                long group = groupedNumber[i];

                if (group >= 100)
                {
                    wordNumber.Append(ones[group / 100 - 1] + " Hundred ");
                    group %= 100;

                    if (group == 0 && i > 0)
                        wordNumber.Append(powers[i - 1]);
                }

                if (group >= 20)
                {
                    if ((group % 10) != 0)
                        wordNumber.Append(tens[group / 10 - 2] + " " + ones[group % 10 - 1] + " ");
                    else
                        wordNumber.Append(tens[group / 10 - 2] + " ");
                }
                else if (group > 0)
                    wordNumber.Append(ones[group - 1] + " ");

                if (group != 0 && i > 0)
                    wordNumber.Append(powers[i - 1]);
            }

            return wordNumber.ToString().Trim();
        }




        public class ActionableList<T> : IList
        {
            private Action<T> action;

            public ActionableList(Action<T> action)
            {
                this.action = action;
            }

            public int Add(object value)
            {
                action((T)value);
                return -1;
            }

            public bool Contains(object value)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public int IndexOf(object value)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, object value)
            {
                throw new NotImplementedException();
            }

            public void Remove(object value)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public object this[int index]
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public bool IsReadOnly { get; }
            public bool IsFixedSize { get; }

            // ...
            public IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public int Count { get; }
            public object SyncRoot { get; }
            public bool IsSynchronized { get; }
        }
    }

    [Serializable]
    public class TempEnrolleeResponse
    {

        public virtual int Id { get; set; }
        public virtual TempEnrollee enrollee { get; set; }
        public virtual string mainimage { get; set; }
        public virtual string spouseimg { get; set; }
        public virtual string child1img { get; set; }
        public virtual string child2img { get; set; }
        public virtual string child3img { get; set; }
        public virtual string child4img { get; set; }
        public virtual string dob { get; set; }
        public virtual string spousedob { get; set; }
        public virtual string child1dob { get; set; }
        public virtual string child2dob { get; set; }
        public virtual string child3dob { get; set; }
        public virtual string child4dob { get; set; }

        public virtual string companyname { get; set; }
    }

    public class homechart
    {
        public virtual decimal value { get; set; }
        public virtual string color { get; set; }
        public virtual string highlight { get; set; }
        public virtual string label { get; set; }

    }

}

