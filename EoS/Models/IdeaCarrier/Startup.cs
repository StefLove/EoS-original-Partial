using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EoS.Models.IdeaCarrier
{
    public class Startup : IValidatableObject
    {
        //Create-----------------------------------------------

        [Key]
        [Display(Name = "Project code")]
        public string StartupID { get; set; }

        [ForeignKey("User")]
        [Display(Name = "Idea Carrier id")]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Country")]
        public int CountryID { get; set; }
        public virtual Shared.Country Country { get; set; }

        [ForeignKey("SwedishRegion")]
        [Display(Name = "Swedish region (Län)")]
        public int? SwedishRegionID { get; set; }
        public virtual Shared.SwedishRegion SwedishRegion { get; set; }

        //Project-----------------------------------------------------

        [Required]
        [Display(Name = "Project name")]
        public string StartupName { get; set; }

        //[Required]
        [Display(Name = "Project domain")]
        public int? ProjectDomainID { get; set; }
        public virtual Shared.ProjectDomain ProjectDomain { get; set; }

        //[Required]
        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Deadline date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? DeadlineDate { get; set; }

        //[Required]
        [MaxLength(1500)]
        [Display(Name = "Project summary (max 1500 characters)")]
        [RegularExpression("^[^0-9@]+$", ErrorMessage = "No numbers or special characters")]
        public string ProjectSummary { get; set; }

        //[Required]
        [Display(Name = "We allow Enablers of Sweden to share this project information given, to at least one type of investor mention below")]
        public bool AllowSharing { get; set; }

        public string AllowSharingDisplayName { get; set; }

        [Display(Name = "Allowed investors")]
        public virtual ICollection<Models.IdeaCarrier.AllowedInvestor> AllowedInvestors { get; set; }

        //Funding-----------------------------------------------------

        //[Required]
        [Display(Name = "Funding phase")]
        public int? FundingPhaseID { get; set; }
        public virtual Shared.FundingPhase FundingPhase { get; set; }

        //[Required]
        [Display(Name = "Funding need")] //<---------
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
        //public virtual ICollection<Models.Shared.FundingDivision> FundingDivisions { get; set; }
        //public virtual ICollection<Models.Shared.FundingDivisionStartup> FundingDivisionStartups { get; set; }

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

        //[Required]
        [Range(1, 10, ErrorMessage = "Please enter a number between 1 and 10 for team member size")]
        [Display(Name = "Team Member Size")]
        public int? TeamMemberSize { get; set; }

        //[Required]
        //[Range(1, 20, ErrorMessage = "Please enter a number between 1 and 20 for Goal Member Size")]
        //[Display(Name = "Goal Member Size")]
        //public int? GoalTeamSize { get; set; }

        //[Required]
        [Range(0, 250, ErrorMessage = "Please enter a number between 0 and 250 for team experience")]
        [Display(Name = "Team experience")]
        public int? TeamExperience { get; set; }

        [Display(Name = "Do all team members share the same vision?")]
        public bool TeamVisionShared { get; set; }

        [Display(Name = "Do you have fixed roles in the team?")]
        public bool HaveFixedRoles { get; set; }

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
        [Display(Name = "Last saved")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime LastSavedDate { get; set; } //internal set

        [Editable(false)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Created")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime CreatedDate { get; set; } //internal set

        [Display(Name = "Locked")]
        public bool Locked { get; set; }

        //[Display(Name = "Log")]
        //public string Log { get; set; }

        [Display(Name = "Approved")]
        public bool Approved { get; set; }

        [ForeignKey("ApprovedBy")]
        [Display(Name = "Approved by")]
        public string ApprovedByID { get; set; }
        public virtual ApplicationUser ApprovedBy { get; set; } //string ApplicationUserId

        [Display(Name = "Matched Investment profiles")]
        public virtual ICollection<MMM.MatchMaking> MatchMakings { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Locked && !ProjectDomainID.HasValue)
            {
                yield return new ValidationResult("Project domain is required.", new List < string > { "ProjectDomainID" }); 
            }

            if (Locked && string.IsNullOrEmpty(ProjectSummary))
            {
                yield return new ValidationResult("Project summary is required.", new List<string> { "ProjectSummary" });
            }

            if (Locked && !FundingPhaseID.HasValue)
            {
                yield return new ValidationResult("Funding phase is required.", new List<string> { "FundingPhaseID" });
            }

            if (Locked && !FundingAmountID.HasValue)
            {
                yield return new ValidationResult("Funding need is required.", new List<string> { "FundingNeedID" });
            }

            if (Locked && !EstimatedExitPlanID.HasValue)
            {
                yield return new ValidationResult("Estimated exit plan is required.", new List<string> { "EstimatedExitPlanID" });
            }

            if (Locked && !EstimatedBreakEven.HasValue)
            {
                yield return new ValidationResult("Estimated break even is required.", new List<string> { "EstimatedBreakEven" });
            }

            if (Locked && !TeamMemberSize.HasValue)
            {
                yield return new ValidationResult("Team member size is required.", new List<string> { "TeamMemberSize" });
            }

            //if (Locked && !GoalTeamSize.HasValue)
            //{
            //    yield return new ValidationResult("Goal Team Size is required.", new List<string> { "GoalTeamSize" });
            //}

            if (Locked && !TeamExperience.HasValue)
            {
                yield return new ValidationResult("Team experience is required.", new List<string> { "TeamExperience" });
            }

            if (Locked && !PossibleIncomeStreams.HasValue)
            {
                yield return new ValidationResult("Possible income streams is required.", new List<string> { "PossibleIncomeStreams" });
            }

            if (Locked && !InnovationLevelID.HasValue)
            {
                yield return new ValidationResult("Level of innovation is required.", new List<string> { "InnovationLevelID" });
            }

            if (Locked && !ScalabilityID.HasValue)
            {
                yield return new ValidationResult("Scalability is required.", new List<string> { "ScalabilityID" });
            }

            if (Locked && !DeadlineDate.HasValue)
            {
                yield return new ValidationResult("Deadline date is required.", new List<string> { "DeadlineDate" });
            }

            if (Locked && !AlreadySpentMoney.HasValue)
            {
                yield return new ValidationResult("Already spent money is required.", new List<string> { "AlreadySpentMoney" });
            }
            if (Locked && !AlreadySpentTime.HasValue)
            {
                yield return new ValidationResult("Already spent time is required.", new List<string> { "AlreadySpentTime" });
            }
            if (Locked && !AllowSharing)
            {
                yield return new ValidationResult("Allowing Enablers of Sweden to share this project information is required.", new List<string> { "AllowSharing" });
            }
        }
    }
}