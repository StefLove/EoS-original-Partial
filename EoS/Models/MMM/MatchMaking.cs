using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EoS.Models.MMM
{
    public class MatchMaking
    {
        public MatchMaking()
        {
            MaxNoOfMatches = this.GetType().GetMembers().Count() - 7; //<-------------
        }

        [Key]
        public int MatchMakingId { get; set; }

        [Editable(false)]
        [Display(Name = "Date of Match making")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime MatchMakingDate { get; internal set; }

        public string InvestmentId { get; set; }
        public virtual Investor.Investment Investment { get; set; }

        public string StartupId { get; set; }
        public virtual IdeaCarrier.Startup Startup { get; set; }

        [Display(Name = "Project domain")] //1
        public bool? ProjectDomainMatched { get; set; } //ProjectDomainsMatch

        [Display(Name = "Funding phase")] //2
        public bool? FundingPhaseMatched { get; set; } //FundingPhasesMatch

        [Display(Name = "Funding amount")] //3
        public bool? FundingAmountMatched { get; set; } //FundingAmountsMatch

        [Display(Name = "Estimated exit plan")] //4
        public bool? EstimatedExitPlanMatched { get; set; } //EstimatedExitPlansMatch

        [Display(Name = "Outcomes")] //5
        public bool? OutcomesMatched { get; set; } //OutcomesMatch

        [Display(Name = "Level of innovation")] //6
        public bool? InnovationLevelMatched { get; set; } //InnovationLevelsMatch

        [Display(Name = "Required scalability")] //7
        public bool? ScalabilityMatched { get; set; } //ScalabilitiesMatch

        [Display(Name = "Team skills")] //8
        public bool? TeamSkillsMatched { get; set; } //TeamSkillsMatch

        [Display(Name = "Number of matches")]
        public int NoOfMatches { get; internal set; }

        [Display(Name = "Maximal number of matches")]
        public int MaxNoOfMatches { get; internal set; }

        [Display(Name = "Sent to investor")]
        public bool Sent { get; set; }
    }
}