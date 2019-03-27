using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class AutomaticExpungeStaff : SiteEntity
    {

        public virtual int StaffId { get; set; }
        public virtual int Subsidiary { get; set; }
        public virtual int Companyid { get; set; }
        public virtual bool Showtouser { get; set; }
        public virtual int CreatedBy { get; set; }

    }
}