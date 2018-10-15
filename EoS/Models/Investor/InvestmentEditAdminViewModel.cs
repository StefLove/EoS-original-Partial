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
        public string InvestmentID { get; set; }

        [Display(Name = "Investor user name")]
        public string InvestorUserName { get; set; }

        [Display(Name = "Investor user ID")]
        public string InvestorUserID { get; internal set; }

        [Editable(true)]
        [DataType(DataType.DateTime, ErrorMessage = "Use date format: yyyy-MM-dd")]
        [DisplayFormat(ApplyFormatInEditMode = true, NullDisplayText = "yyyy-MM-dd", DataFormatString = "{0:yyyy-MM-dd}")]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Locked")]
        public bool Locked { get; set; }
    }
}
