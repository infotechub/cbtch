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
    public class EnrolleePage : Webpage, IUniquePage
    {



        public virtual IList<State> States { get; set; }
        public virtual IList<Lga> Lgas { get; set; }
        //I used viewbags and formcollection to populate the model @ the bakend.


    }


}