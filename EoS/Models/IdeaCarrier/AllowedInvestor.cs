using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.IdeaCarrier
{
    public class AllowedInvestor
    {
        [Key]
        public int AllowedInvestorID { get; set; }
        [Required]
        [Display(Name = "Allowed investor name")]
        public string AllowedInvestorName { get; set; }

        public virtual ICollection<Models.IdeaCarrier.Startup> Startups { get; set; }
    }
}