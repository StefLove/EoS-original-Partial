using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EoS.Models.Investor
{
    public class InvestmentAddProfileViewModel
    {
        //ProfileName,CountryID,SwedishRegionID,

        //[Key]
        //[Display(Name = "Investment Code")]
        //public string InvestmentID { get; set; }

        //[ForeignKey("User")]
        //[Display(Name = "User")]
        //public string UserId { get; set; }

        [Required]
        [Display(Name = "Investment Profile Name")]
        public string ProfileName { get; set; }

        [Required]
        [Display(Name = "Country")]
        public int CountryID { get; set; }
        public SelectList Countries { get; set; }

        //[ForeignKey("SwedishRegion")]
        [Display(Name = "Region (Län)")]
        public int? SwedishRegionID { get; set; }
        public SelectList SwedishRegions { get; set; }

        [Display(Name = "Message")]
        public string InvestorMessage { get; set; }
    }
}