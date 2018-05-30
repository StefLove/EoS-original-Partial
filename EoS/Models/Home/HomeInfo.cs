using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EoS.Models.Home
{
    public class HomeInfo
    {
        [Key]
        public int Id { get; set; }

        //[Required]
        //[Display(Name = "Language")]
        //public string Language { get; set; } //Enum or List

        [Required]
        [Display(Name = "Info Text")]
        [AllowHtml] //<---------------------------------------
        public string Text { get; set; }
    }
}