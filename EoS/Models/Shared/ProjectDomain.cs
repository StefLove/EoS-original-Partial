using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Shared
{
    public class ProjectDomain
    {
        [Key]
        public int ProjectDomainID { get; set; }

        [Required]
        [Display(Name = "Project domain name")]
        public string ProjectDomainName { get; set; }

        public virtual ICollection<Investor.Investment> Investments { get; set; }
    }
}