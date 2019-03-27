using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Models.Provider;

namespace MrCMS.Web.Apps.Core.Pages
{
    public class ProviderPortalPage : Webpage, IUniquePage
    {
        public virtual string ProviderId { get; set; }
        public virtual Provider providermodel { get; set; }

    }


}