using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class TeamSkillViewModel
    {
        [Display(Name = "Skill ID")]
        public int SkillID { get; set; }

        [Display(Name = "Skill name")]
        public string SkillName { get; set; }

        [Display(Name = "Assigned")]
        public bool Assigned { get; set; }
    }
}