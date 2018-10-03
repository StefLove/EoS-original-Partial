using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{                 
    public class InvestorOutcomeViewModel
    {
        [Display(Name = "Outcome ID")]
        public int OutcomeID { get; set; }

        [Display(Name = "Outcome name")]
        public string OutcomeName { get; set; }

        [Display(Name = "Assigned")]
        public bool Assigned { get; set; }
    }
}