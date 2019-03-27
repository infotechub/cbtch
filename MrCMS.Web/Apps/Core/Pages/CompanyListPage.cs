using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Apps.Core.Entities;

namespace MrCMS.Web.Apps.Core.Pages
{
    public class CompanyListPage : TextPage, IUniquePage
    {
        public virtual IList<Company> Companies { get; set; }
    }
}