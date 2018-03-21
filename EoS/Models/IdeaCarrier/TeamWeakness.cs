using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.IdeaCarrier
{
    public class TeamWeakness
    {
        [Key]
        public int TeamWeaknessID { get; set; }

        [Required]
        [Display(Name = "Team weakness name")]
        public string TeamWeaknessName { get; set; }

        public virtual ICollection<Startup> Startups { get; set; }
    }
}