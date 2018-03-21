using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.IdeaCarrier
{
    public class ReminderStartupViewModel
    {
        [Required]
        [Display(Name = "Startup Project Id")]
        public string StartupId { get; set; }

        [Required]
        [Display(Name = "Startup Project Name")]
        public string StartupName { get; set; }

        [Required]
        [Display(Name = "Idea Carrier Email")]
        public string IdeaCarrierEmail { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Message")]
        [MaxLength(500)]
        public string Message { get; set; }

        //[Display(Name = "Idea Carrier Id")]
        public string IdeaCarrierId { get; set; }

        //[Display(Name = "Redirect")]
        public string Redirect { get; set; }
    }
}