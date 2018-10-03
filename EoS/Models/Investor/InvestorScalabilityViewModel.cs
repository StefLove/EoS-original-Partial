using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class InvestorScalabilityViewModel
    {
        [Display(Name = "Scalability ID")]
        public int ScalabilityID { get; set; }

        [Display(Name = "Scalability name")]
        public string ScalabilityName { get; set; }

        [Display(Name = "Assigned")]
        public bool Assigned { get; set; }
    }
}