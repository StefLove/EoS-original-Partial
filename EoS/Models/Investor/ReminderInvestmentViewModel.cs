using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class ReminderInvestmentViewModel
    {
        [Required]
        [Display(Name = "Investment Profile Id")]
        public string InvestmentId { get; set; }

        [Required]
        [Display(Name = "Investor Email")]
        public string InvestorEmail { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Message")]
        [MaxLength(500)]
        public string Message { get; set; }

        //[Display(Name = "Investor Id")]
        public string InvestorId { get; set; }

        //[Display(Name = "Redirect")]
        public string Redirect { get; set; }
    }
}