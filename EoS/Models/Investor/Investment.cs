using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class Investment : IValidatableObject
    {
        //Create-----------------------------------------------

        [Key]
        [Display(Name = "Profile code")]
        public string InvestmentID { get; set; }

        [ForeignKey("User")]
        [Display(Name = "Investor")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Country")]
        public int CountryID { get; set; }
        public virtual Shared.Country Country { get; set; }

        [ForeignKey("SwedishRegion")]
        [Display(Name = "Swedish region (Län)")]
        public int? SwedishRegionID { get; set; }
        public virtual Shared.SwedishRegion SwedishRegion { get; set; }

        //Profile----------------------------------------------

        //[Required]
        [Display(Name = "Profile name")]
        public string ProfileName { get; set; }

        [Display(Name = "Project domain")]
        public int? ProjectDomainID { get; set; }
        public virtual Shared.ProjectDomain ProjectDomain { get; set; }

        //[Display(Name = "Extra Project Domains")]
        //public string ExtraProjectDomains { get; set; }

        //Funding----------------------------------------------

        //public virtual Shared.FundingPhase FundingPhase { get; set; }
        [Display(Name = "Funding phases")]
        public virtual ICollection<Models.Shared.FundingPhase> FundingPhases { get; set; }

        //[Display(Name = "Funding Support")]
        //public int? FundingAmountID { get; set; }
        [Display(Name = "Funding amounts")]
        public virtual ICollection<Models.Shared.FundingAmount> FundingAmounts { get; set; }

        [Display(Name = "Are you interested in projects that might have a need for future funding?")]  //<------------------!!! +++
        public bool FutureFundingNeeded { get; set; }

        //[Display(Name = "Are you interested in projects that you already spent time working with?")]
        //public bool AlreadySpentTime { get; set; }

        //[Display(Name = "Are you interested in projects that you have already spent money in?")]
        //public bool AlreadySpentMoney { get; set; }

        //Budget-----------------------------------------------

        [Display(Name = "Estimated exit plans")]
        public virtual ICollection<Models.Shared.EstimatedExitPlan> EstimatedExitPlans { get; set; }

        [Range(1, 10, ErrorMessage = "Please enter a number between 1 and 10 for for estimated break even")]
        [Display(Name = "Estimated break even")]
        public int? EstimatedBreakEven { get; set; } //10 equals to any number

        [Range(0, 10, ErrorMessage = "Please enter a number between 1 and 10 for possible income streams")]
        [Display(Name = "Possible income streams?")]
        public int? PossibleIncomeStreams { get; set; }

        //Team-------------------------------------------------

        [Display(Name = "Is it important that the team consists of more than 1 person?")]
        public bool TeamMemberSizeMoreThanOne { get; set; }

        [Display(Name = "Is it important that the team has exprience in the field?")]
        public bool TeamHasExperience { get; set; }

        [Display(Name = "Are you an active investor?")]
        public bool ActiveInvestor { get; set; }

        [Display(Name = "Skills you might bring to the team")]
        public virtual ICollection<TeamSkill> TeamSkills { get; set; }


        //Outcome----------------------------------------------

        [Display(Name = "Outcomes")]
        public virtual ICollection<Models.Shared.Outcome> Outcomes { get; set; }

        [Display(Name = "Level of innovation")]
        public virtual ICollection<Models.Shared.InnovationLevel> InnovationLevels { get; set; }

        [Display(Name = "Required scalability")]
        public virtual ICollection<Models.Shared.Scalability> Scalabilities { get; set; }

        //[Display(Name = "Is it important to have paying customers?")]
        //public bool HavePayingCustomers { get; set; }

        //-----------------------------------------------------

        [Editable(false)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Created")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime CreatedDate { get; set; } //internal set;

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

        [Display(Name = "Locked")]
        public bool Locked { get; set; }

        [Display(Name = "Profile is active")]
        public bool Active { get; set; }

        [Display(Name = "Matched Startup projects")]
        public virtual ICollection<MMM.MatchMaking> MatchMakings { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Locked && !ProjectDomainID.HasValue)
            {
                yield return new ValidationResult("Project Domain is required.", new List<string> { "ProjectDomainID" });
            }

            //if (Locked && FundingAmounts == null)
            //{
            //    yield return new ValidationResult("Select at least one Funding Support.", new List<string> { "FundingAmounts" });
            //}

            //if (Locked && !TeamMemberSize.HasValue)
            //{
            //    yield return new ValidationResult("Team Member Size is required.", new List<string> { "TeamMemberSize" });
            //}

            //if (Locked && !TeamHasExperience.HasValue)
            //{
            //    yield return new ValidationResult("Team Experience is required.", new List<string> { "TeamHasExperience" });
            //}
        }
    }
}


