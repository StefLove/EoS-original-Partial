using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.IdeaCarrier
{
    public class FundingDivisionStartup //ProjectFundingDivision is a better name
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Funding division ID")]
        public int FundingDivisionID { get; set; } //public string FundingDivisionName { get; set; } // is better to use
        public virtual FundingDivision FundingDivision { get; set; } //ought to be removed

        [Display(Name = "Startup project ID")]
        public string StartupID { get; set; }
        public virtual Startup Startup { get; set; }

        [Display(Name = "Percentage")]
        [Range(0, 100, ErrorMessage = "Please enter a number between 0 and 100 for Funding division values")]
        public int Percentage { get; set; }
    }
}
