using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.IdeaCarrier
{
    public class AllowedInvestorViewModel
    {
        [Display(Name = "Allowed investor ID")]
        public int AllowedInvestorID { get; set; }

        [Display(Name = "Allowed investor name")]
        public string AllowedInvestorName { get; set; }

        [Display(Name = "Assigned")]
        public bool Assigned { get; set; }
    }
}