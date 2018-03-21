using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class InvestorInnovationLevelViewModel
    {
        public int InnovationLevelID { get; set; }

        //[Display(Name = "Level of Innovation Name ")]
        public string InnovationLevelName { get; set; }

        public bool Assigned { get; set; }
    }
}