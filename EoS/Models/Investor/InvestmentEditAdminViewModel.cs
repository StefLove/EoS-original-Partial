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
        [Display(Name = "Investment Code")]
        public string InvestmentId { get; set; }

        [Display(Name = "Investor Name")]
        public string InvestorName { get; set; }

        [Editable(true)]
        [DataType(DataType.DateTime, ErrorMessage = "Use Date Format: YYYY-MM-DD")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }
    }
}
