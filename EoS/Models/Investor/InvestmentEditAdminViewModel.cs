using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class InvestmentEditAdminViewModel
    {
        //[Key]
        [Display(Name = "Investment ID")]
        public string InvestmentId { get; set; }

        [Display(Name = "Investor name")]
        public string InvestorName { get; set; }

        [Editable(true)]
        [DataType(DataType.DateTime, ErrorMessage = "Use date format: yyyy-MM-dd")]
        [DisplayFormat(ApplyFormatInEditMode = true, NullDisplayText = "yyyy-MM-dd", DataFormatString = "{0:yyyy-MM-dd}")]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Locked")]
        public bool Locked { get; set; }
    }
}
