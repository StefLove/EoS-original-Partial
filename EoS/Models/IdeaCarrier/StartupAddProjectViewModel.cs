using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EoS.Models.IdeaCarrier
{
    public class StartupAddProjectViewModel
    {
        [Required]
        [Display(Name = "Startup project name")]
        public string StartupName { get; set; }

        [Required]
        [Display(Name = "Country")]
        public int CountryID { get; set; }
        public SelectList Countries { get; set; }

        [Display(Name = "Swedish region (Län)")]
        public int? SwedishRegionID { get; set; }
        public SelectList SwedishRegions { get; set; }

        [Display(Name = "Message")]
        public string IdeaCarrierMessage { get; set; }
    }
}
