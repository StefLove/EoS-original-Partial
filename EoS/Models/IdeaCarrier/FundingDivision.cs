using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.IdeaCarrier
{
    public class FundingDivision
    {
        [Key]
        public int FundingDivisionID { get; set; }

        [Required]
        [Display(Name = "Funding division name ")]
        public string FundingDivisionName { get; set; }

        //public virtual ICollection<Models.IdeaCarrier.Startup> Startups { get; set; }
        public virtual ICollection<FundingDivisionStartup> FundingDivisionStartups { get; set; } //<----necessary??
    }
}