using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Pages
{
    public class ExpandSponsorPage : TextPage, IUniquePage
    {
        public virtual string Sponsor { get; set; }
    }
}