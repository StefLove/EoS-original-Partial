using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.IdeaCarrier
{
    public class FundingDivisionStartup
    {
        [Key]
        public int Id { get; set; }

        public int FundingDivisionID { get; set; }

        public string StartupID { get; set; }

        [Range(0, 100, ErrorMessage = "Please enter a number between 0 and 100 for funding division values")]
        public int Percentage { get; set; }

        public virtual FundingDivision FundingDivision { get; set; }
        public virtual IdeaCarrier.Startup  Startup { get; set; }
    }
}