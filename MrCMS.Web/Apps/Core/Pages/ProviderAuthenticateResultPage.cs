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
    public class ProviderAuthenticateResultPage : Webpage, IUniquePage
    {

        public virtual string Message { get; set; }
        public virtual string Passport { get; set; }
        public virtual string Fullname { get; set; }
        public virtual string PolicyNumber { get; set; }
        public virtual string Gender { get; set; }
        public virtual string Company { get; set; }
        public virtual string Verificationcode { get; set; }
        public virtual string Hospital { get; set; }
    }


}