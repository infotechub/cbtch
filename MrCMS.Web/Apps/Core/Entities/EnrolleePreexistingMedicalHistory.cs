using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Core.Entities
{
    public class EnrolleePreexistingMedicalHistory : SiteEntity
    {
        public virtual int Pemhid { get; set; }
        public virtual int Enrolleeid { get; set; }
        public virtual bool Answeryesno { get; set; }
        public virtual string Answerstring { get; set; }
        public virtual bool Status { get; set; }
    }
}