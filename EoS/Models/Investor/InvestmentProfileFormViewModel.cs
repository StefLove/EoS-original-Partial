using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EoS.Models.Investor
{
    public class InvestmentProfileFormViewModel : IValidatableObject
    {
        //InvestmentID,ProfileName,ProjectDomainID,TeamMemberSize,ExtraProjectDomains,TeamExperience,AlreadySpentTime,AlreadySpentMoney,TeamVisionShared,HaveFixedRoles,EstimatedBreakEven,IncomeStreams,HavePayingCustomers,Active
        
        [Key]
        [Display(Name = "Investment code")]
        public string InvestmentID { get; set; }
        
        //[ForeignKey("User")]
        //[Display(Name = "User")] //<--------------------------- i InvestmentsController
        //public string UserId { get; set; }
        //public virtual ApplicationUser User { get; set; }

        //Profile----------------------------------------------

        //[Required]
        [Display(Name = "Investment profile name")]
        public string ProfileName { get; set; }

        [Display(Name = "Project domain")] //Profile domain?
        public int? ProjectDomainID { get; set; }
        public virtual Shared.ProjectDomain ProjectDomain { get; set; }

        //Funding----------------------------------------------


        [Display(Name = "Funding amounts")]
        public virtual ICollection<Models.Shared.FundingAmount> FundingAmounts { get; set; }
        [Display(Name = "Funding phases")]
        public virtual ICollection<Models.Shared.FundingPhase> FundingPhases { get; set; }

        [Display(Name = "Are you interested in projects that might have a need for future funding?")]  //<------------------!!! +++
        public bool FutureFundingNeeded { get; set; }

        //Budget-----------------------------------------------

        [Display(Name = "Estimated exit plans")]
        public virtual ICollection<Models.Shared.EstimatedExitPlan> EstimatedExitPlans { get; set; }

        [Range(1, 10, ErrorMessage = "Please enter a number between 1 and 10 for for estimated break even")]
        [Display(Name = "Estimated break even")]
        public int? EstimatedBreakEven { get; set; } //10 equals to any number

        [Range(0, 10, ErrorMessage = "Please enter a number between 0 and 10 for Possible Income Streams")]
        [Display(Name = "Possible Income Streams?")]
        public int? PossibleIncomeStreams { get; set; }

        //Team-------------------------------------------------

        //Required
        [Display(Name = "Is it important that the team consists of more than 1 person?")]
        public bool TeamMemberSizeMoreThanOne { get; set; }

        //[Required]
        [Display(Name = "Team Experience (in Years)")]
        public int? TeamExperience { get; set; }

        [Display(Name = "Skills you might bring to the team")]
        public virtual ICollection<TeamSkill> TeamSkills { get; set; } //YourTeamSkills

        //Outcome----------------------------------------------

        [Display(Name = "Outcomes")]
        public virtual ICollection<Models.Shared.Outcome> Outcomes { get; set; }

        [Display(Name = "Levels of innovation")]
        public virtual ICollection<Models.Shared.InnovationLevel> InnovationLevels { get; set; }

        [Display(Name = "Required scalabilities")]
        public virtual ICollection<Models.Shared.Scalability> Scalabilities { get; set; }

        //-----------------------------------------------------

        [Editable(false)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Created")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime CreatedDate { get; set; } //internal set;

        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Last Saved")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime LastSavedDate { get; set; } //internal set;

        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Due")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Locked")]
        public bool Locked { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Matched Startup Projects")]
        public virtual ICollection<MMM.MatchMaking> MatchMakings { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Locked && !ProjectDomainID.HasValue)
            {
                yield return new ValidationResult("Project Domain is required.", new List<string> { "ProjectDomainID" });
            }

            //if (Lock && FundingAmounts == null)
            //{
            //    yield return new ValidationResult("Select at least one Funding Support.", new List<string> { "FundingAmounts" });
            //}

            //if (Locked && !TeamMemberSizeMoreThanOne.HasValue)
            //{
            //    yield return new ValidationResult("Team Member Size is required.", new List<string> { "TeamMemberSize" });
            //}

            if (Locked && !TeamExperience.HasValue)
            {
                yield return new ValidationResult("Team Experience is required.", new List<string> { "TeamExperience" });
            }
        }
    }
}


