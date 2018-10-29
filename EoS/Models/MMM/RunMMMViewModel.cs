using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EoS.Models.MMM
{
    public class RunMMMViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Investment profile")]
        public string MatchableInvestmentProfileId { get; set; }
        public SelectList MatchableInvestmentProfileList { get; set; }

        [Display(Name = "Startup project")]
        public string MatchableStartupProjectId { get; set; }
        public SelectList MatchableStartupProjectList { get; set; }

        [Display(Name = "Domain")]
        public bool ProjectDomainSelected { get; set; } //1

        [Display(Name = "Funding phase")]
        public bool FundingPhaseSelected { get; set; } //2

        [Display(Name = "Funding amount/need")]
        public bool FundingAmountSelected { get; set; } //3

        [Display(Name = "Estimated exit plan")]
        public bool EstimatedExitPlanSelected { get; set; } //4

        //[Display(Name = "Team Member Size")]
        //public bool TeamMemberSizeSelected { get; set; }

        //[Display(Name = "Team Experience")]
        //public bool TeamExperienceSelected { get; set; }

        [Display(Name = "Team skills/weaknesses")]
        public bool TeamSkillsSelected { get; set; } //5

        [Display(Name = "Outcomes")]
        public bool OutcomesSelected { get; set; } //6

        [Display(Name = "Level of innovation")]
        public bool InnovationLevelSelected { get; set; } //7

        [Display(Name = "Scalability")]
        public bool ScalabilitySelected { get; set; } //8  
    }
}
