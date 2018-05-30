using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Shared
{
    public class Country
    {
        [Key]
        public int CountryID { get; set; }

        [Required]
        [Display(Name = "Country name")]
        public string CountryName { get; set; }

        [Required]
        [Display(Name = "Country abbreviation")]
        public string CountryAbbreviation { get; set; }
    }
}