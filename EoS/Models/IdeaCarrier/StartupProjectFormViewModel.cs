using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EoS.Models.IdeaCarrier
{
    /*This class is the old one, it has been replaced by StartupProjectViewModel, se code in Startup.cs*/
    
    public class StartupProjectFormViewModel : IValidatableObject /*this does not work in AJAX!*/
    {
        //[Key]
        [Display(Name = "Startup project code")]
        public string StartupID { get; set; }

        //[ForeignKey("User")]
        [Display(Name = "User")]
        public string UserID { get; set; }

        //Project-----------------------------------------------------

        [Required]
        [Display(Name = "Startup project name")] //Already in AddProfile
        public string StartupName { get; set; }
        
        [Display(Name = "Project Domain")]
        public int? ProjectDomainID { get; set; }
        public virtual Shared.ProjectDomain ProjectDomain { get; set; }

        //[Required]
        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Deadline")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? DeadlineDate { get; set; }

        //[Required]
        [MaxLength(1500)]
        [Display(Name = "Project summary (max 1500 characters)")]
        [RegularExpression("^[^0-9@]+$", ErrorMessage = "No numbers or special characters")]
        public string ProjectSummary { get; set; }

        //[Required]
        [Display(Name = "We allow Enablers of Sweden to share this project information given, to at least one type of investor mention below")]
        public bool? AllowSharing { get; set; }

        public string AllowSharingDisplayName { get; set; }

        [Display(Name = "Allowed Investors")]
        public virtual ICollection<Models.IdeaCarrier.AllowedInvestor> AllowedInvestors { get; set; }

        //Funding-----------------------------------------------------

        //[Required]
        [Display(Name = "Funding phase")]
        public int? FundingPhaseID { get; set; }
        public virtual Shared.FundingPhase FundingPhase { get; set; }

        //[Required]
        [Display(Name = "Funding need")]
        public int? FundingAmountID { get; set; }
        public virtual Shared.FundingAmount FundingAmount { get; set; }

        //[Required]
        [Display(Name = "Estimated exit plan")]
        public int? EstimatedExitPlanID { get; set; }
        public virtual Shared.EstimatedExitPlan EstimatedExitPlan { get; set; }

        [Display(Name = "Do you see a need of more funding in the future?")]
        public bool FutureFundingNeeded { get; set; }

        //[Required]
        [Display(Name = "How much time has already been spent?")]
        public int? AlreadySpentTime { get; set; }

        //[Required]
        [Display(Name = "How much money has already been spent?")]
        public int? AlreadySpentMoney { get; set; }

        [Display(Name = "Will the founder(s) spend more of their own money ahead?")]
        public bool WillSpendOwnMoney { get; set; }

        //Budget-----------------------------------------------------------

        [Display(Name = "Project funding divisions")]
        public virtual List<FundingDivisionStartup> ProjectFundingDivisions { get; set; }

        //[Required]
        [Range(1, 10, ErrorMessage = "Please enter a number between 0 and 10 for estimated break even")]
        [Display(Name = "Estimated break even")]
        public int? EstimatedBreakEven { get; set; }

        //[Required]
        [Range(0, 10, ErrorMessage = "Please enter a number between 0 and 10 for possible income streams")]
        [Display(Name = "Possible income streams")]
        public int? PossibleIncomeStreams { get; set; }

        [Display(Name = "Do you have paying customers")]
        public bool HavePayingCustomers { get; set; }

        //Team-------------------------------------------------------------

        [Range(1, 10, ErrorMessage = "Please enter a number between 1 and 10 for team member size")]
        [Display(Name = "Team Member Size")]
        public int? TeamMemberSize { get; set; }

        [Range(0, 250, ErrorMessage = "Please enter a number between 0 and 250 for team experience")]
        [Display(Name = "Team Experience")]
        public int? TeamExperience { get; set; }

        [Display(Name = "Do all team members share the same vision?")]
        public bool? TeamVisionShared { get; set; }

        [Display(Name = "Do you have fixed roles in the team?")]
        public bool? HaveFixedRoles { get; set; }

        [Display(Name = "Team weaknesses")]
        public virtual ICollection<TeamWeakness> TeamWeaknesses { get; set; }

        [Display(Name = "Are you looking for active investors?")]
        public bool LookingForActiveInvestors { get; set; }

        //Outcome-------------------------------------------------------------

        [Display(Name = "Outcomes")]
        public virtual ICollection<Models.Shared.Outcome> Outcomes { get; set; }

        //[Required]
        [Display(Name = "Level of innovation")]
        public int? InnovationLevelID { get; set; }
        [Display(Name = "Level of innovation")]
        public virtual Shared.InnovationLevel InnovationLevel { get; set; }

        //[Required]
        [Display(Name = "Scalability")]
        public int? ScalabilityID { get; set; }
        public virtual Shared.Scalability Scalability { get; set; }

        //-----------------------------------------------------------------

        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Last Saved")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime LastSavedDate { get; internal set; }

        [Editable(false)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Created")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime CreatedDate { get; internal set; }

        [Display(Name = "Locked")]
        public bool Locked { get; internal set; } //<---------internal set

        [Display(Name = "Matched Investment Profiles")]
        public virtual ICollection<MMM.MatchMaking> MatchMakings { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Locked && !ProjectDomainID.HasValue)
            {
                yield return new ValidationResult("Project Domain is required.", new List <string> { "ProjectDomainID" }); 
            }

            if (Locked && string.IsNullOrEmpty(ProjectSummary))
            {
                yield return new ValidationResult("Project Summary is required.", new List<string> { "ProjectSummary" });
            }

            if (Locked && !FundingPhaseID.HasValue)
            {
                yield return new ValidationResult("Funding Phase is required.", new List<string> { "FundingPhaseID" });
            }

            if (Locked && !FundingAmountID.HasValue)
            {
                yield return new ValidationResult("Funding Need is required.", new List<string> { "FundingNeedID" });
            }

            if (Locked && !FundingAmountID.HasValue)
            {
                yield return new ValidationResult("Funding Need is required.", new List<string> { "FundingNeedID" });
            }

            if (Locked && !EstimatedExitPlanID.HasValue)
            {
                yield return new ValidationResult("Estimated Exit Plan is required.", new List<string> { "EstimatedExitPlanID" });
            }

            if (Locked && !EstimatedBreakEven.HasValue)
            {
                yield return new ValidationResult("Estimated Break Even is required.", new List<string> { "EstimatedBreakEven" });
            }

            if (Locked && !TeamMemberSize.HasValue)
            {
                yield return new ValidationResult("Team Member Size is required.", new List<string> { "TeamMemberSize" });
            }
            
            if (Locked && !TeamExperience.HasValue)
            {
                yield return new ValidationResult("Team Experience is required.", new List<string> { "TeamExperience" });
            }

            if (Locked && !PossibleIncomeStreams.HasValue)
            {
                yield return new ValidationResult("Income Stream is required.", new List<string> { "PossibleIncomeStreams" });
            }

            if (Locked && !InnovationLevelID.HasValue)
            {
                yield return new ValidationResult("Innovation Level is required.", new List<string> { "InnovationLevelID" });
            }

            if (Locked && !ScalabilityID.HasValue)
            {
                yield return new ValidationResult("Scalability is required.", new List<string> { "ScalabilityID" });
            }

            if (Locked && !DeadlineDate.HasValue)
            {
                yield return new ValidationResult("Deadline Date is required.", new List<string> { "DeadlineDate" });
            }

            if (Locked && !AlreadySpentMoney.HasValue)
            {
                yield return new ValidationResult("Already spent money question is required.", new List<string> { "AlreadySpentMoney" });
            }
            if (Locked && !AlreadySpentTime.HasValue)
            {
                yield return new ValidationResult("Already spent time question is required.", new List<string> { "AlreadySpentTime" });
            }

            //if (Locked && !WillSpendOwnMoney.HasValue)
            //{
            //    yield return new ValidationResult("Already spent time question is required.", new List<string> { "WillSpendOwnMoney" });
            //}
            //if (Locked && !FutureFundingNeeded.HasValue)
            //{
            //    yield return new ValidationResult("Already spent time question is required.", new List<string> { "FutureFundingNeeded" });
            //}

            if (Locked && (!AllowSharing.HasValue || AllowSharing.HasValue && !AllowSharing.Value)) //<--------
            {
                yield return new ValidationResult("Allowing Enablers of Sweden to share this project information is required.", new List<string> { "AllowSharing" });
            }
        }
    }
}
