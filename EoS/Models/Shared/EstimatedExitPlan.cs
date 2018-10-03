using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Shared
{
    public class EstimatedExitPlan
    {
        [Key]
        public int EstimatedExitPlanID { get; set; }

        [Required]
        [Display(Name = "Estimated exit plan")]
        public string EstimatedExitPlanName { get; set; }

        public virtual ICollection<Investor.Investment> Investments { get; set; } //<---necessary??
    }
}