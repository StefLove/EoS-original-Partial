using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EoS.Models.Shared;

namespace EoS.Models.IdeaCarrier
{
    public class Startup //: IValidatableObject
    {
        //Create-----------------------------------------------

        [Key]
        [Display(Name = "Project ID")]
        public string StartupID { get; set; } //ProjectID

        [ForeignKey("User")]
        [Display(Name = "Idea carrier")]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Country")]
        public int CountryID { get; set; }
        public virtual Country Country { get; set; }

        [Display(Name = "Swedish region (Län)")]
        public int? SwedishRegionID { get; set; }
        public virtual SwedishRegion SwedishRegion { get; set; } //<---------remove

        //Project-----------------------------------------------------

        [Required]
        [Display(Name = "Name")]
        public string StartupName { get; set; }

        //[Required]
        [Display(Name = "Domain")]
        public int? ProjectDomainID { get; set; }
        public virtual ProjectDomain ProjectDomain { get; set; }

        //[Required]
        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Deadline")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? DeadlineDate { get; set; }

        //[Required]
        [MaxLength(1500)]
        //[AllowHtml] //<-----------?
        [Display(Name = "Project summary")]
        //[RegularExpression("^[^0-9@]+$", ErrorMessage = "No numbers or special characters")]
        public string ProjectSummary { get; set; }

        //[Required]
        [Display(Name = "Allow sharing?")]
        public bool AllowSharing { get; set; }
        //public string AllowSharingDisplayName { get; set; }

        [Display(Name = "Allowed investors")]
        public virtual ICollection<AllowedInvestor> AllowedInvestors { get; set; }

        //Funding-----------------------------------------------------

        //[Required]
        [Display(Name = "Funding phase")]
        public int? FundingPhaseID { get; set; }
        public virtual FundingPhase FundingPhase { get; set; }

        //[Required]
        [Display(Name = "Funding need")] //<---------
        public int? FundingAmountID { get; set; }
        public virtual FundingAmount FundingAmount { get; set; }

        [Display(Name = "Future funding needed?")]
        public bool FutureFundingNeeded { get; set; }

        //[Required]
        [Display(Name = "Already spent time")]
        public int? AlreadySpentTime { get; set; }

        //[Required]
        [Display(Name = "Already spent money")]
        public int? AlreadySpentMoney { get; set; }

        [Display(Name = "Will spend own money?")]
        public bool WillSpendOwnMoney { get; set; }

        //Budget-----------------------------------------------------------

        //[Required]
        [Display(Name = "Funding divisions")]
        public virtual ICollection<FundingDivisionStartup> ProjectFundingDivisions { get; set; }

        //[Required]
        [Display(Name = "Estimated exit plan")]
        public int? EstimatedExitPlanID { get; set; }
        public virtual EstimatedExitPlan EstimatedExitPlan { get; set; }

        //[Required]
        //[Range(1, 10, ErrorMessage = "Please enter a number between 0 and 10 for estimated break even")]
        [Display(Name = "Estimated break even")]
        public int? EstimatedBreakEven { get; set; }

        //[Required]
        //[Range(0, 10, ErrorMessage = "Please enter a number between 0 and 10 for possible income streams")]
        [Display(Name = "Possible income streams")]
        public int? PossibleIncomeStreams { get; set; }

        [Display(Name = "Have paying customers?")]
        public bool HavePayingCustomers { get; set; }

        //Team-------------------------------------------------------------

        //[Required]
        //[Range(1, 10, ErrorMessage = "Please enter a number between 1 and 10 for team member size")]
        [Display(Name = "Team member size")]
        public int? TeamMemberSize { get; set; }

        //[Required]
        //[Range(1, 20, ErrorMessage = "Please enter a number between 1 and 20 for Goal Member Size")]
        //[Display(Name = "Goal Member Size")]
        //public int? GoalTeamSize { get; set; }

        //[Required]
        //[Range(0, 250, ErrorMessage = "Please enter a number between 0 and 250 for team experience")]
        [Display(Name = "Team experience")]
        public int? TeamExperience { get; set; }

        [Display(Name = "Team vision shared?")]
        public bool TeamVisionShared { get; set; }

        [Display(Name = "Have fixed roles?")]
        public bool HaveFixedRoles { get; set; }

        [Display(Name = "Team weaknesses")]
        public virtual ICollection<TeamWeakness> TeamWeaknesses { get; set; }

        [Display(Name = "Active investors?")] //"Looking for active investors?"
        public bool LookingForActiveInvestors { get; set; }

        //Outcome-------------------------------------------------------------

        [Display(Name = "Outcomes")]
        public virtual ICollection<Outcome> Outcomes { get; set; }

        //[Required]
        [Display(Name = "Level of innovation")]
        public int? InnovationLevelID { get; set; }
        [Display(Name = "Level of innovation")]
        public virtual InnovationLevel InnovationLevel { get; set; }

        //[Required]
        [Display(Name = "Scalability")]
        public int? ScalabilityID { get; set; }
        public virtual Scalability Scalability { get; set; }

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

        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Last locked date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? LastLockedDate { get; internal set; }

        //[Display(Name = "Log")]
        //public string Log { get; set; }

        [Display(Name = "Approved")]
        public bool Approved { get; set; }

        //[ForeignKey("ApprovedBy")]
        //[ForeignKey("User")]
        [Display(Name = "Approved by")]
        public string ApprovedByID { get; set; }
        //public virtual ApplicationUser ApprovedBy { get; set; }

        //[Display(Name = "Documents")]
        //public virtual List<Document> Documents { get; set; } //<-------------

        [Display(Name = "Matched Investment profiles")]
        public virtual ICollection<MMM.MatchMaking> MatchMakings { get; set; } //<----------------------

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Locked && !ProjectDomainID.HasValue)
        //    {
        //        yield return new ValidationResult("Project domain is required.", new List < string > { "ProjectDomainID" }); 
        //    }

        //    if (Locked && string.IsNullOrEmpty(ProjectSummary))
        //    {
        //        yield return new ValidationResult("Project summary is required.", new List<string> { "ProjectSummary" });
        //    }

        //    if (Locked && !FundingPhaseID.HasValue)
        //    {
        //        yield return new ValidationResult("Funding phase is required.", new List<string> { "FundingPhaseID" });
        //    }

        //    if (Locked && !FundingAmountID.HasValue)
        //    {
        //        yield return new ValidationResult("Funding need is required.", new List<string> { "FundingNeedID" });
        //    }

        //    if (Locked && !EstimatedExitPlanID.HasValue)
        //    {
        //        yield return new ValidationResult("Estimated exit plan is required.", new List<string> { "EstimatedExitPlanID" });
        //    }

        //    if (Locked && !EstimatedBreakEven.HasValue)
        //    {
        //        yield return new ValidationResult("Estimated break even is required.", new List<string> { "EstimatedBreakEven" });
        //    }

        //    if (Locked && !TeamMemberSize.HasValue)
        //    {
        //        yield return new ValidationResult("Team member size is required.", new List<string> { "TeamMemberSize" });
        //    }

        //    //if (Locked && !GoalTeamSize.HasValue)
        //    //{
        //    //    yield return new ValidationResult("Goal Team Size is required.", new List<string> { "GoalTeamSize" });
        //    //}

        //    if (Locked && !TeamExperience.HasValue)
        //    {
        //        yield return new ValidationResult("Team experience is required.", new List<string> { "TeamExperience" });
        //    }

        //    if (Locked && !PossibleIncomeStreams.HasValue)
        //    {
        //        yield return new ValidationResult("Possible income streams is required.", new List<string> { "PossibleIncomeStreams" });
        //    }

        //    if (Locked && !InnovationLevelID.HasValue)
        //    {
        //        yield return new ValidationResult("Level of innovation is required.", new List<string> { "InnovationLevelID" });
        //    }

        //    if (Locked && !ScalabilityID.HasValue)
        //    {
        //        yield return new ValidationResult("Scalability is required.", new List<string> { "ScalabilityID" });
        //    }

        //    if (Locked && !DeadlineDate.HasValue)
        //    {
        //        yield return new ValidationResult("Deadline date is required.", new List<string> { "DeadlineDate" });
        //    }

        //    if (Locked && !AlreadySpentMoney.HasValue)
        //    {
        //        yield return new ValidationResult("Already spent money is required.", new List<string> { "AlreadySpentMoney" });
        //    }
        //    if (Locked && !AlreadySpentTime.HasValue)
        //    {
        //        yield return new ValidationResult("Already spent time is required.", new List<string> { "AlreadySpentTime" });
        //    }
        //    if (Locked && !AllowSharing)
        //    {
        //        yield return new ValidationResult("Allowing Enablers of Sweden to share this project information is required.", new List<string> { "AllowSharing" });
        //    }
        //}
    }

    public class StartupProjectViewModel
    {
        //[Key]
        [Display(Name = "Project ID")]
        public string StartupID { get; set; }

        //Project-----------------------------------------------------

        //[Required]
        [Display(Name = "Project name")]
        public string ProjectName { get; set; }

        //[Required]
        [Display(Name = "Project domain")]
        public int? ProjectDomainID { get; set; }
        //public virtual Shared.ProjectDomain ProjectDomain { get; set; }
        public SelectList ProjectDomainList { get; set; }

        //[Required]
        [Editable(true)]
        [DataType(DataType.DateTime, ErrorMessage = "Use date format: yyyy-MM-dd")]
        [DisplayFormat(ApplyFormatInEditMode = true, NullDisplayText = "yyyy-MM-dd", DataFormatString = "{0:yyyy-MM-dd}")]
        [Display(Name = "Deadline date")]
        public DateTime? DeadlineDate { get; set; }

        //[Required]
        [MaxLength(1500)]
        //[AllowHtml] //<-----------?
        [Display(Name = "Project summary (max 1500 characters) in plain text")]
        [RegularExpression("^[^0-9@]+$", ErrorMessage = "No numbers or special characters")]
        public string ProjectSummary { get; set; }

        //[Required]
        [Display(Name = "We allow Enablers of Sweden to share this project information given, to at least one type of investor mention below")]
        public bool AllowSharing { get; set; }

        //[MaxLength(500)] //<---?
        [AllowHtml]
        public string AllowSharing_DisplayName { get; set; } //Get from IdeaCarrierMessage

        //[Display(Name = "Allowed investors")]
        //public virtual ICollection<AllowedInvestor> AllowedInvestors { get; set; }
        public List<AllowedInvestorViewModel> AllowedInvestors { get; internal set; }
        //public bool AllowedInvestorsUnanswered { get; set; }

        //Funding-----------------------------------------------------

        //[Required]
        [Display(Name = "Funding phase")]
        public int? FundingPhaseID { get; set; }
        //public virtual Shared.FundingPhase FundingPhase { get; set; }
        public SelectList FundingPhaseList { get; set; }

        //[Required]
        [Display(Name = "Funding need")] //<---------
        public int? FundingAmountID { get; set; } //==>FundingNeedID
        //public virtual Shared.FundingAmount FundingAmount { get; set; }
        public SelectList FundingAmountList { get; set; } //==>FundingNeedList

        //[Required]
        [Display(Name = "Do you see a need of more funding in the future?")]
        public bool FutureFundingNeeded { get; set; }

        //[Required]
        [Display(Name = "How much time has already been spent?")]
        public int? AlreadySpentTime { get; set; }

        //[Required]
        [Display(Name = "How much money has already been spent?")]
        public int? AlreadySpentMoney { get; set; }

        //[Required]
        [Display(Name = "Will the founder(s) spend more of their own money ahead?")]
        public bool WillSpendOwnMoney { get; set; }

        //Budget-----------------------------------------------------------

        //[Required]
        //[Display(Name = "Project funding divisions")]
        //public virtual ICollection<FundingDivisionStartup> ProjectFundingDivisions { get; set; }
        public List<FundingDivisionStartup> FundingDivisions { get; internal set; }
        //public bool ProjectFundingDivisionsUnanswered { get; set; }

        //[Required]
        [Display(Name = "Estimated exit plan")]
        public int? EstimatedExitPlanID { get; set; }
        //public virtual Shared.EstimatedExitPlan EstimatedExitPlan { get; set; }
        public SelectList EstimatedExitPlanList { get; set; }

        //[Required]
        [Range(1, 10, ErrorMessage = "Please enter a number between 0 and 10 for estimated break even")]
        [Display(Name = "Estimated break even")]
        public int? EstimatedBreakEven { get; set; }

        //[Required]
        [Range(0, 10, ErrorMessage = "Please enter a number between 0 and 10 for possible income streams")]
        [Display(Name = "Possible income streams")]
        public int? PossibleIncomeStreams { get; set; }

        //[Required]
        [Display(Name = "Do you have paying customers")]
        public bool HavePayingCustomers { get; set; }

        //Team-------------------------------------------------------------

        //[Required]
        [Range(1, 10, ErrorMessage = "Please enter a number between 1 and 10 for team member size")]
        [Display(Name = "Team member size")]
        public int? TeamMemberSize { get; set; }

        //[Required]
        [Range(0, 250, ErrorMessage = "Please enter a number between 0 and 250 for team experience")]
        [Display(Name = "Team experience")]
        public int? TeamExperience { get; set; }

        //[Required]
        [Display(Name = "Do all team members share the same vision?")]
        public bool TeamVisionShared { get; set; }

        //[Required]
        [Display(Name = "Do you have fixed roles in the team?")]
        public bool HaveFixedRoles { get; set; }

        //[Display(Name = "Team weaknesses")]
        //public virtual ICollection<TeamWeakness> TeamWeaknesses { get; set; }
        public List<TeamWeaknessViewModel> TeamWeaknesses { get; internal set; }
        //public bool TeamWeaknessesUnanswered { get; set; }

        //[Required]
        [Display(Name = "Are you looking for active investors?")]
        public bool LookingForActiveInvestors { get; set; }

        //Outcome-------------------------------------------------------------

        //[Display(Name = "Outcomes")]
        //public virtual ICollection<Models.Shared.Outcome> Outcomes { get; set; } //<--------
        public List<StartupOutcomeViewModel> Outcomes { get; internal set; }
        //public bool OutcomesUnanswered { get; set; }

        //[Required]
        [Display(Name = "Level of innovation")]
        public int? InnovationLevelID { get; set; }
        //[Display(Name = "Level of innovation")]
        //public virtual Shared.InnovationLevel InnovationLevel { get; set; }
        public SelectList InnovationLevelList { get; set; }

        //[Required]
        [Display(Name = "Scalability")]
        public int? ScalabilityID { get; set; }
        //public virtual Shared.Scalability Scalability { get; set; }
        public SelectList ScalabilityList { get; set; }

        //-----------------------------------------------------------------

        //[Editable(true)]
        //[DataType(DataType.DateTime)]
        //[Display(Name = "Last saved")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        //public DateTime LastSavedDate { get; set; } //internal set

        //[Editable(false)]
        //[DataType(DataType.DateTime)]
        //[Display(Name = "Created")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        //public DateTime CreatedDate { get; set; } //internal set

        //[Display(Name = "Locked")]
        //public bool Locked { get; set; }

        //[Editable(true)]
        //[DataType(DataType.DateTime)]
        //[Display(Name = "Last locked date")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        //public DateTime? LastLockedDate { get; internal set; }

        //[Display(Name = "Log")]
        //public string Log { get; set; }

        //[Display(Name = "Approved")]
        //public bool Approved { get; set; }

        //[ForeignKey("ApprovedBy")]
        //[Display(Name = "Approved by")]
        //public string ApprovedByID { get; set; }
        //public virtual ApplicationUser ApprovedBy { get; set; }

        //[Display(Name = "Matched Investment profiles")]
        //public virtual ICollection<MMM.MatchMaking> MatchMakings { get; set; } //<----------------------

        public bool Updated { get; set; }

        //public bool AllQuestionsAnswered { get; internal set; } //<--------!!!
    }

    //public class StartupProjectGetViewModel : StartupProjectViewModel
    //{
        //Project
        //public SelectList ProjectDomainList { get; internal set; }
        //public List<AllowedInvestorViewModel> AllowedInvestors { get; internal set; }
        //Funding
        //public SelectList FundingPhaseList { get; internal set; }
        //public SelectList FundingAmountList { get; internal set; }
        //Budget
        //public List<FundingDivisionStartup> ProjectFundingDivisions { get; internal set; }
        //public List<FundingDivisionStartup> FundingDivisionList { get; internal set; }
        //public SelectList EstimatedExitPlanList { get; internal set; }
        //Team
        //public List<TeamWeaknessViewModel> TeamWeaknesses { get; internal set; }
        //Outcome
        //public List<StartupOutcomeViewModel> Outcomes { get; internal set; }
        //public SelectList InnovationLevelList { get; internal set; }
        //public SelectList ScalabilityList { get; internal set; }
    //}

    public class StartupProjectPostViewModel : StartupProjectViewModel
    {
        //Project
        public string[] SelectedAllowedInvestorIDs { get; set; } //SharedToInvestors
        //Funding
        public string[] FundingDivisionPercentages { get; set; }
        //Team
        public string[] SelectedTeamWeaknessIDs { get; set; }
        //Outcome
        public string[] SelectedOutcomeIDs { get; set; }

        public string ActiveTab { get; set; } //<--??
    }

    public class AddNewProjectViewModel
    {
        [AllowHtml]
        [Display(Name = "Message for Idea carrier")]
        public string IdeaCarrierMessage { get; set; }

        [Required]
        [Display(Name = "Project name")]
        public string StartupName { get; set; }

        [Required]
        [Display(Name = "Country")]
        public int CountryID { get; set; }
        //public virtual Shared.Country Country { get; set; }
        public SelectList CountryList { get; set; }

        [Display(Name = "Swedish country ID")]
        public int SwedishCountryID { get; set; }

        [Display(Name = "Swedish region (Län)")]
        public int? SwedishRegionID { get; set; }
        //public virtual Shared.SwedishRegion SwedishRegion { get; set; }
        public SelectList SwedishRegionList { get; set; }
    }
}