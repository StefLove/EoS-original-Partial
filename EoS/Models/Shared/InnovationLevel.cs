using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Shared
{
    public class InnovationLevel
    {
        [Key]
        public int InnovationLevelID { get; set; }
        [Required]
        [Display(Name = "Name of Level of innovation")]
        public string InnovationLevelName { get; set; }

        public virtual ICollection<Investor.Investment> Investments { get; set; }
    }
}