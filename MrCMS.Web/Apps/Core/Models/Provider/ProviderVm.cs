using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Models.Provider
{
    public class ProviderVm
    {
        public int Id { get; set; }
        public virtual string Name { get; set; }

        public virtual string Code { get; set; }

        public virtual string SubCode { get; set; }

        public virtual string Email { get; set; }

        public virtual string Phone { get; set; }

        public virtual string Phone2 { get; set; }

        public virtual string Website { get; set; }
        public virtual string Address { get; set; }
        public virtual string Area { get; set; }
        public virtual State State { get; set; }
        public virtual int Lgaid { get; set; }
        public virtual string Lganame { get; set; }
        public virtual string Zone { get; set; }
        public virtual long Numberofenrollees { get; set; }
        public virtual string BankName { get; set; }
        public virtual string BankName2 { get; set; }
        public virtual ProviderAccount Provideraccount { get; set; }
        public virtual ProviderAccount Provideraccount2 { get; set; }
        public virtual string ProviderservicesStr { get; set; }
        public virtual string ProviderplansStr { get; set; }
        public virtual string ProvidertariffsStr { get; set; }
        public virtual string AssigneeName { get; set; }
        public virtual int Assignee { get; set; }
        public virtual string paymentemail1 { get; set; }
        public virtual string paymentemail2 { get; set; }
        public virtual string Providergpscordinate { get; set; }
        public virtual List<string> Providerservices { get; set; }
        public virtual List<string> Providerplans { get; set; }
        public virtual List<string> ProviderTariffs { get; set; }
        public virtual List<string> consessionslist { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual int AuthorizationStatus { get; set; }
        public virtual string AuthorizationStatusStr { get; set; }
        public virtual string AuthorizationNote { get; set; }
        public virtual string DisapprovalNote { get; set; }
        public virtual int AuthorizedBy { get; set; }
        public virtual string AuthorizedByString { get; set; }
        public virtual int DisapprovedBy { get; set; }
        public virtual DateTime? AuthorizedDate { get; set; }
        public virtual DateTime? DisapprovalDate { get; set; }
        public virtual long Parentid { get; set; }
        public virtual bool Status { get; set; }
        public virtual string CreatedDate { get; set; }
        public virtual IList<ProviderServices> services { get; set; }
        public virtual string CategoryString { get; set; }
        public virtual ProviderCategory category { get; set; }
        public virtual bool isdelisted { get; set; }
        public virtual string DelistNote { get; set; }
        public virtual DateTime? datedelisted { get; set; }
        public virtual int delistedBy { get; set; }
        public virtual string consessions { get; set; }



    }
}