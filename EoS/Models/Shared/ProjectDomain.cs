using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Shared
{
    public class ProjectDomain //Domain
    {
        [Key]
        public int ProjectDomainID { get; set; }

        [Required]
        [Display(Name = "Project domain name")]
        public string ProjectDomainName { get; set; }

        public virtual ICollection<IdeaCarrier.Startup> Startups { get; set; }
        /*public virtual ICollection<X.Y> Ys { get; set; }*/
    }
}
