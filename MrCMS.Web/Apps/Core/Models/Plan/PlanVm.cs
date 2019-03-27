using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MrCMS.Web.Apps.Core.Models.Plan
{
    public class PlanVm
    {

        public int Id { get; set; }
        [Display(Name = "Plan Name")]
        public string Name { get; set; }
        [Display(Name = "Plan Code")]
        public string Code { get; set; }
        [DisplayName("Active")]
        public bool Status { get; set; }
        [Required, DisplayName("Plan Description")]
        public string Description { get; set; }
        [Required, DisplayName("Date Added")]
        public DateTime CreatedOn { get; set; }

    }
}