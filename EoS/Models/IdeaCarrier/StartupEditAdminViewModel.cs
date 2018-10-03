using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EoS.Models.IdeaCarrier
{
    public class StartupEditAdminViewModel
    {
        //[Key]
        [Display(Name = "Startup code")]
        public string StartupID { get; set; }

        [Display(Name = "Investor name")]
        public string IdeaCarrierName { get; set; }

        [MaxLength(1500)] //<-----------------increase?
        //[AllowHtml] //<--------------------------------------------------------------------
        [Display(Name = "Project summary (max 1500 characters) in plain text or HTML")]
        [RegularExpression("^[^0-9@]+$", ErrorMessage = "No numbers or special characters")]
        public string ProjectSummary { get; set; }

        [Display(Name = "Locked")]
        public bool Locked { get; set; }

        [Display(Name = "Approved")]
        public bool? Approved { get; set; }
    }
}