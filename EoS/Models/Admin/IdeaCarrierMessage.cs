using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "Message for Idea Carrier")]
        public string Text { get; set; }
    }
}