using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Shared
{
    public class FundingPhase
    {
        [Key]
        public int FundingPhaseID { get; set; }
        [Required]
        [Display(Name = "Funding phase name ")]
        public string FundingPhaseName { get; set; }

        public virtual ICollection<Investor.Investment> Investments { get; set; }
    }
}