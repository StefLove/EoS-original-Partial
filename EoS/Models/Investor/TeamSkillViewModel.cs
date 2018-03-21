using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class TeamSkillViewModel
    {     
        public int SkillID { get; set; }
               
        public string SkillName { get; set; }

        public bool Assigned { get; set; }
    }
}