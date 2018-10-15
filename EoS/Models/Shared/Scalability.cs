using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Shared
{
    public class Scalability
    {
        [Key]
        public int ScalabilityID { get; set; }

        [Required]
        [Display(Name = "Scalability name")]
        public string ScalabilityName { get; set; }

        public virtual ICollection<Investor.Investment> Investments { get; set; }
    }
}