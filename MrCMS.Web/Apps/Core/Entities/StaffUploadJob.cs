using MrCMS.Entities;
using MrCMS.Web.Apps.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class StaffUploadJob : SiteEntity
    {

        public virtual string filelink { get; set; }
        public virtual JobStatus JobStatus { get; set; }
        public virtual int UploadedBy { get; set; }
        public virtual int CompanyID { get; set; }
        public virtual int Subsidiary { get; set; }
        public virtual JobExpungeMode ExpungeMode { get; set; }
        public virtual int TotalRecord { get; set; }
        public virtual int TotalRecordDone { get; set; }
        public virtual int TotalRecordSuccess { get; set; }
        public virtual int TotalRecordFailed { get; set; }
        public virtual int TotalStaffForExpunged { get; set; }
        public virtual int TotalStaffAdded { get; set; }
        public virtual string Analysislink { get; set; }
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? FinishTime { get; set; }
        public virtual bool approved { get; set; }
        public virtual DateTime? dateapproved { get; set; }
        public virtual int approvedby { get; set; }

        public virtual string errorlist { get; set; }
    }
}