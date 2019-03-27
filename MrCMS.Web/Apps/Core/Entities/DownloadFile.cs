using MrCMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class DownloadFile : SiteEntity
    {

        public virtual string fileName { get; set; }
        public virtual string filelink { get; set; }
        public virtual int filestaus { get; set; }
        public virtual int createdby { get; set; }
        public virtual int downloadedBy { get; set; }

        public virtual int downloadCount { get; set; }


    }
}