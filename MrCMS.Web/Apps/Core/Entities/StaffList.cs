using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class Staff : SiteEntity
    {
        public virtual string CompanyId { get; set; }
        public virtual int CompanySubsidiary { get; set; }
        public virtual string StaffFullname { get; set; }
        public virtual int StaffPlanid { get; set; }
        public virtual bool HasProfile { get; set; }
        public virtual bool IsExpunged { get; set; }
        public virtual int Profileid { get; set; }
        public virtual int Createdby { get; set; }
        public virtual string StaffId { get; set; }
        public virtual int NewStaffId { get; set; }

        public virtual DateTime? stafflinkDate { get; set; }

        public virtual int stafflinkUSer { get; set; }

        public virtual int StaffJobId { get; set; }
    }
}