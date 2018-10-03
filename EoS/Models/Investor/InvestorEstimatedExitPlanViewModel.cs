using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class InvestorEstimatedExitPlanViewModel
    {
        [Display(Name = "Estimated exit plan ID")]
        public int EstimatedExitPlanID { get; set; }

        [Display(Name = "Estimated exit plan name")]
        public string EstimatedExitPlanName { get; set; }

        [Display(Name = "Assigned")]
        public bool Assigned { get; set; }
    }
}