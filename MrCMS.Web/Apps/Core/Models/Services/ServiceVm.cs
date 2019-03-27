using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Models.Services
{
    public class ServiceVm
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required"), DisplayName("Service Name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Service Code is required."), DisplayName("Service Code")]
        public string Code { get; set; }
        [DisplayName("Active")]
        public bool Status { get; set; }
        [Required, DisplayName("Service Description")]
        public string Description { get; set; }
        [Required, DisplayName("Date Added")]
        public DateTime? Dateadded { get; set; }
    }
}