//using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Shared
{
    public class SwedishRegion
    {
        [Key]
        public int RegionID { get; set; }

        [Required]
        [Display(Name = "Swedish region name (Län)")]
        public string RegionName { get; set; }
    }
}
