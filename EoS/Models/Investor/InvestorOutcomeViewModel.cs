using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{                 
    public class InvestorOutcomeViewModel
    {
        public int OutcomeID { get; set; }

        public string OutcomeName { get; set; }

        public bool Assigned { get; set; }
    }
}