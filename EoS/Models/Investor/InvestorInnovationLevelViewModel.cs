using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class InvestorInnovationLevelViewModel
    {
        [Display(Name = "Level of innovation ID")]
        public int InnovationLevelID { get; set; }

        [Display(Name = "Level of innovation name ")]
        public string InnovationLevelName { get; set; }

        [Display(Name = "Assigned")]
        public bool Assigned { get; set; }
    }
}