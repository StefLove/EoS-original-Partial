using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class InvestorFundingAmountViewModel
    {
        [Display(Name = "Funding amount ID")]
        public int FundingAmountID { get; set; }

        [Display(Name = "Funding amount value")]
        public string FundingAmountValue { get; set; }

        [Display(Name = "Assigned")]
        public bool Assigned { get; set; }
    }
}

