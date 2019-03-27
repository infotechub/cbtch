using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MrCMS.Entities;



namespace MrCMS.Web.Apps.Core.Entities
{
    public class Service : SiteEntity
    {
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual bool Status { get; set; }
        public virtual string Description { get; set; }
        public virtual int CreatedBy { get; set; }

        public virtual string Dateadded
        {
            get { return CreatedOn.ToString("dd MMM yyyy"); }
        }

        public virtual int AuthorizationStatus { get; set; }
        public virtual string AuthorizationNote { get; set; }
        public virtual string DisapprovalNote { get; set; }
        public virtual int AuthorizedBy { get; set; }
        public virtual int DisapprovedBy { get; set; }
        public virtual DateTime? AuthorizedDate { get; set; }
        public virtual DateTime? DisapprovalDate { get; set; }

    }
}