using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class InvestorEstimatedExitPlanViewModel
    {
        public int EstimatedExitPlanID { get; set; }

        public string EstimatedExitPlanName { get; set; }

        public bool Assigned { get; set; }
    }
}