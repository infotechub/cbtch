﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Web.Apps.Core.Pages
{
    public class TariffListViewPage : Webpage, IUniquePage
    {
        public virtual string TarrifName { get; set; }
    }
}