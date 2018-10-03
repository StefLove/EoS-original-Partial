using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EoS.Models.Admin
{
    public class IdeaCarrierMessage
    {
        [Key]
        public int Id { get; set; }

        //[Required]
        //[Display(Name = "Language")]
        //public string Language { get; set; } //Enum or List

        [Required]
        [AllowHtml]
        [Display(Name = "Message to the Idea carriers")]
        public string Text { get; set; }

        [AllowHtml]
        [Display(Name = "Display name for Allow sharing in tab Project in the Startup project form")]
        public string AllowSharing_DisplayName { get; set; }
    }
}