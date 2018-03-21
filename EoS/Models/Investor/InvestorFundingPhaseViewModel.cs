using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class InvestorFundingPhaseViewModel
    {
        public int FundingPhaseID { get; set; }

        public string FundingPhaseName { get; set; }

        public bool Assigned { get; set; }
    }
}