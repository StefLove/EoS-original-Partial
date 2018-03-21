using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.IdeaCarrier
{
    public class StartupEditAdminViewModel
    {
        //[Key]
        [Display(Name = "Startup Code")]
        public string StartupID { get; set; }

        [Display(Name = "Allow Sharing Display Name")]
        public string AllowSharingDisplayName { get; set; }

        [MaxLength(1500)] //<-----------------increase?
        [Display(Name = "Project Summary")]
        [RegularExpression("^[^0-9@]+$", ErrorMessage = "No numbers or special characters")]
        public string ProjectSummary { get; set; }
    }
}