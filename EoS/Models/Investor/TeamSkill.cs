using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Investor
{
    public class TeamSkill
    {
        [Key]
        public int SkillID { get; set; }

        [Required]
        [Display(Name = "Skill name")]
        public string SkillName { get; set; }

        public virtual ICollection<Investment> Investments { get; set; } //<---necessary??
    }
}