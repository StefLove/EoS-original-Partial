using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EoS.Models.Shared;

namespace EoS.Models.Investor
{
    public class Investment //: IValidatableObject
    {
        //Create-----------------------------------------------

        [Key]
        [Display(Name = "ID")]
        public string InvestmentID { get; set; } //ProfileID

        //[ForeignKey("User")]
        [Display(Name = "Investor internal ID")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Country")]
        public int CountryID { get; set; }
        public virtual Country Country { get; set; }

        [Display(Name = "Swedish region (Län)")]
        public int? SwedishRegionID { get; set; }
        //public virtual SwedishRegion SwedishRegion { get; set; }

        //Profile----------------------------------------------

        //[Required]
        [Display(Name = "Name")]
        public string ProfileName { get; set; }

        [Display(Name = "Domain")]
        public int? ProjectDomainID { get; set; }
        public virtual ProjectDomain ProjectDomain { get; set; }

        //Funding----------------------------------------------

        //public virtual FundingPhase FundingPhase { get; set; }
        [Display(Name = "Funding phases")]
        public virtual ICollection<FundingPhase> FundingPhases { get; set; }

        //[Display(Name = "Funding Support")]
        [Display(Name = "Funding amounts")]
        public virtual ICollection<FundingAmount> FundingAmounts { get; set; }

        [Display(Name = "Needs future funding?")]
        public bool FutureFundingNeeded { get; set; } //<--------bool?

        //Budget-----------------------------------------------

        [Display(Name = "Estimated exit plans")]
        public virtual ICollection<EstimatedExitPlan> EstimatedExitPlans { get; set; }

        //[Range(1, 10, ErrorMessage = "Please enter a number between 1 and 10 for for estimated break even")]
        [Display(Name = "Estimated break even")]
        public int? EstimatedBreakEven { get; set; } //10 equals to any number

        //[Range(0, 10, ErrorMessage = "Please enter a number between 1 and 10 for possible income streams")]
        [Display(Name = "Possible income streams")]
        public int? PossibleIncomeStreams { get; set; }

        //Team-------------------------------------------------

        [Display(Name = "Team size > 1 person?")]
        public bool TeamMemberSizeMoreThanOne { get; set; } //<-----bool?

        [Display(Name = "Team has experience?")]
        public bool TeamHasExperience { get; set; } //<--------bool?

        [Display(Name = "Active investor?")]
        public bool ActiveInvestor { get; set; } //<--------bool?

        [Display(Name = "Team skills")]
        public virtual ICollection<TeamSkill> TeamSkills { get; set; }

        //Outcome----------------------------------------------

        [Display(Name = "Outcomes")]
        public virtual ICollection<Outcome> Outcomes { get; set; }

        [Display(Name = "Levels of innovation")]
        public virtual ICollection<InnovationLevel> InnovationLevels { get; set; }

        [Display(Name = "Required scalabilities")]
        public virtual ICollection<Scalability> Scalabilities { get; set; }

        //-----------------------------------------------------

        [Editable(false)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Created")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime CreatedDate { get; internal set; }

        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Last saved")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime LastSavedDate { get; set; } //internal set;

        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Due")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Locked?")]
        public bool Locked { get; set; }

        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Last locked")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? LastLockedDate { get; internal set; }

        [Display(Name = "Active?")] //<-----------
        public bool Active { get; set; } //bool?

        [Display(Name = "Matched Startup projects")]
        public virtual ICollection<MMM.MatchMaking> MatchMakings { get; set; }
    }

    public class InvestmentProfileViewModel
    {
        //[Key]
        [Display(Name = "Profile ID")]
        public string InvestmentID { get; set; }

        //Profile----------------------------------------------

        //[Required]
        [Display(Name = "Profile name")]
        public string ProfileName { get; set; }

        //[Required]
        [Display(Name = "Profile domain")] //<--------
        public int? ProfileDomainID { get; set; }
        //public virtual Shared.ProfileDomain ProjectDomain { get; set; }
        public SelectList ProfileDomainList { get; set; }

        //Funding----------------------------------------------

        //public virtual Shared.FundingPhase FundingPhase { get; set; }
        //[Display(Name = "Funding phases")]
        //public virtual ICollection<Models.Shared.FundingPhase> FundingPhases { get; set; }
        public List<InvestorFundingPhaseViewModel> FundingPhases { get; set; }

        //[Display(Name = "Funding Support")]
        //public int? FundingAmountID { get; set; }
        //[Display(Name = "Funding amounts")]
        //public virtual ICollection<Models.Shared.FundingAmount> FundingAmounts { get; set; }
        public List<InvestorFundingAmountViewModel> FundingAmounts { get; set; }
        //public bool FundingAmountsUnanswered { get; set; }

        //[Required]
        [Display(Name = "Are you interested in projects that might have a need for future funding?")]  //<------------------!!!
        public bool FutureFundingNeeded { get; set; } //<--------bool?

        //Budget-----------------------------------------------

        //[Display(Name = "Estimated exit plans")]
        //public virtual ICollection<Models.Shared.EstimatedExitPlan> EstimatedExitPlans { get; set; }
        public List<InvestorEstimatedExitPlanViewModel> EstimatedExitPlans { get; set; }
        //public bool EstimatedExitPlansUnanswered { get; set; }

        //[Required]
        [Range(1, 10, ErrorMessage = "Enter a number between 1 and 10 for for Estimated break even")]
        [Display(Name = "Estimated break even")]
        public int? EstimatedBreakEven { get; set; } //10 equals to any number

        //[Required]
        [Range(0, 10, ErrorMessage = "Enter a number between 1 and 10 for Possible income streams")]
        [Display(Name = "Possible income streams")]
        public int? PossibleIncomeStreams { get; set; }

        //Team-------------------------------------------------

        //[Required]
        [Display(Name = "Is it important that the team consists of more than 1 person?")]
        public bool TeamMemberSizeMoreThanOne { get; set; } //<--------bool?

        //[Required]
        [Display(Name = "Is it important that the team has experience in the field?")]
        public bool TeamHasExperience { get; set; } //<--------bool?

        //[Required]
        [Display(Name = "Are you an active investor?")]
        public bool ActiveInvestor { get; set; } //<--------bool?

        //[Display(Name = "Skills you might bring to the team")]
        //public virtual ICollection<TeamSkill> TeamSkills { get; set; }
        public List<TeamSkillViewModel> TeamSkills { get; set; }

        //Outcome----------------------------------------------

        //[Display(Name = "Outcomes")]
        //public virtual ICollection<Models.Shared.Outcome> Outcomes { get; set; }
        public List<InvestorOutcomeViewModel> Outcomes { get; set; }

        //[Display(Name = "Levels of innovation")]
        //public virtual ICollection<Models.Shared.InnovationLevel> InnovationLevels { get; set; }
        public List<InvestorInnovationLevelViewModel> InnovationLevels { get; set; }

        //[Display(Name = "Required scalabilities")]
        //public virtual ICollection<Models.Shared.Scalability> Scalabilities { get; set; }
        public List<InvestorScalabilityViewModel> Scalabilities { get; set; }

        //-----------------------------------------------------

        public bool Updated { get; set; }

        public string Message { get; set; }
        public string UnansweredQuestion { get; set; }
    }

    public class InvestmentProfilePostViewModel : InvestmentProfileViewModel
    {
        //Funding
        public string[] FundingPhaseIDs { get; set; }
        public string[] FundingAmountIDs { get; set; }
        public new string FutureFundingNeeded { get; set; } //<---!!
        //Budget
        public string[] EstimatedExitPlanIDs { get; set; }
        //Team
        public string[] TeamSkillIDs { get; set; }
        public new string TeamMemberSizeMoreThanOne { get; set; }
        public new string TeamHasExperience { get; set; }
        public new string ActiveInvestor { get; set; }
        //Outcomes
        public string[] OutcomeIDs { get; set; }
        public string[] InnovationLevelIDs { get; set; }
        public string[] ScalabilityIDs { get; set; }
        //-----------------------------------
        public string ActiveTab { get; set; }
    }

    public class AddNewProfileViewModel
    {
        [AllowHtml]
        [Display(Name = "Message for Investor")]
        public string InvestorMessage { get; set; }

        [Required]
        [Display(Name = "Project name")]
        public string ProfileName { get; set; }

        [Required]
        [Display(Name = "Country")]
        public int CountryID { get; set; }
        public SelectList CountryList { get; set; }

        public int SwedishCountryID { get; set; }

        [Display(Name = "Swedish region (Län)")]
        public int? SwedishRegionID { get; set; }
        public SelectList SwedishRegionList { get; set; }
    }
}


