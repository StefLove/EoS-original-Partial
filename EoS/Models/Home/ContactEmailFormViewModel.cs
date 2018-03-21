using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Home
{
    public class ContactEmailFormViewModel
    {
        [Required]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        //[Required]
        [Display(Name= "Message")]
        [MaxLength(500)]
        public string Message { get; set; } //textarea
    }
}