using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EoS.Models.Admin
{
    public class InvestorMessage
    {
        [Key]
        public int Id { get; set; }

        //[Required]
        //[Display(Name = "Language")]
        //public string Language { get; set; } //Enum or List

        [Required]
        [AllowHtml]
        [Display(Name = "Message for the Investors")]
        public string Text { get; set; }
    }
}