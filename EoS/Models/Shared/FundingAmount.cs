using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Shared
{
    public class FundingAmount
    {
        [Key]
        public int FundingAmountID { get; set; }

        [Required]
        [Display(Name = "Funding amount value")]
        public string FundingAmountValue { get; set; }

        public virtual ICollection<Investor.Investment> Investments { get; set; }
    }
}