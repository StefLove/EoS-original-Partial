using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class InvestorFundingPhaseViewModel
    {
        [Display(Name = "Funding phase ID")]
        public int FundingPhaseID { get; set; }

        [Display(Name = "Funding phase ID")]
        public string FundingPhaseName { get; set; }

        [Display(Name = "Assigned")]
        public bool Assigned { get; set; }
    }
}