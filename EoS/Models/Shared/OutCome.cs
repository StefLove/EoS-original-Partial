using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Shared
{
    public class Outcome
    {
        [Key]
        public int OutcomeID { get; set; }
        [Required]
        [Display(Name = "Outcome name")]
        public string OutcomeName { get; set; }

        public virtual ICollection<IdeaCarrier.Startup> Startups { get; set; }
        /*public virtual ICollection<X.Y> Ys { get; set; }*/
    }
}
