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
    public class Startup //StartupProject
    {
        //Create-----------------------------------------------

        [Key]
        [Display(Name = "Project ID")]
        public string StartupID { get; set; }

        [Display(Name = "Idea carrier")]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Country")]
        public int CountryID { get; set; }
        public virtual Country Country { get; set; }

        [Display(Name = "Swedish region (Län)")]
        public int? SwedishRegionID { get; set; }
        //public virtual SwedishRegion SwedishRegion { get; set; }

        //Project-----------------------------------------------------

        [Required]
        [Display(Name = "Name")]
        public string StartupName { get; set; }

        [Display(Name = "Domain")]
        public int? ProjectDomainID { get; set; }
        public virtual ProjectDomain ProjectDomain { get; set; }

        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Deadline")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? DeadlineDate { get; set; }

        [MaxLength(1500)]
        [AllowHtml]
        [Display(Name = "Project summary")]
        //[RegularExpression("^[^0-9@]+$", ErrorMessage = "No numbers or special characters")]
        public string ProjectSummary { get; set; }

        [Display(Name = "Allow sharing?")]
        public bool AllowSharing { get; set; }

        [Display(Name = "Allowed investors")]
        public virtual ICollection<AllowedInvestor> AllowedInvestors { get; set; }

        //Funding-----------------------------------------------------

        [Display(Name = "Funding phase")]
        public int? FundingPhaseID { get; set; }
        public virtual FundingPhase FundingPhase { get; set; }

        [Display(Name = "Funding need")]
        public int? FundingAmountID { get; set; }
        public virtual FundingAmount FundingAmount { get; set; }

        [Display(Name = "Future funding needed?")]
        public bool FutureFundingNeeded { get; set; }

        [Display(Name = "Already spent time")]
        public int? AlreadySpentTime { get; set; }

        [Display(Name = "Already spent money")]
        public int? AlreadySpentMoney { get; set; }

        [Display(Name = "Will spend own money?")]
        public bool WillSpendOwnMoney { get; set; }

        //Budget-----------------------------------------------------------

        [Display(Name = "Funding divisions")]
        public virtual ICollection<FundingDivisionStartup> ProjectFundingDivisions { get; set; }

        [Display(Name = "Estimated exit plan")]
        public int? EstimatedExitPlanID { get; set; }
        public virtual EstimatedExitPlan EstimatedExitPlan { get; set; }

        [Display(Name = "Estimated break even")]
        public int? EstimatedBreakEven { get; set; }

        [Display(Name = "Income streams")]
        public int? PossibleIncomeStreams { get; set; }

        [Display(Name = "Have paying customers?")]
        public bool HavePayingCustomers { get; set; }

        //Team-------------------------------------------------------------

        [Display(Name = "Team member size")]
        public int? TeamMemberSize { get; set; }

        [Display(Name = "Team experience")]
        public int? TeamExperience { get; set; }

        [Display(Name = "Team vision shared?")]
        public bool TeamVisionShared { get; set; }

        [Display(Name = "Have fixed roles?")]
        public bool HaveFixedRoles { get; set; }

        [Display(Name = "Team weaknesses")]
        public virtual ICollection<TeamWeakness> TeamWeaknesses { get; set; }
                         
        [Display(Name = "Looking for active investors?")]
        public bool LookingForActiveInvestors { get; set; }

        //Outcome-------------------------------------------------------------

        [Display(Name = "Outcomes")]
        public virtual ICollection<Outcome> Outcomes { get; set; }

        [Display(Name = "Level of innovation")]
        public int? InnovationLevelID { get; set; }
        public virtual InnovationLevel InnovationLevel { get; set; }

        [Display(Name = "Required scalability")]
        public int? ScalabilityID { get; set; }
        public virtual Scalability Scalability { get; set; }

        //-----------------------------------------------------------------

        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Last saved")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime LastSavedDate { get; set; }

        [Editable(false)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Created")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Locked?")]
        public bool Locked { get; set; }

        [Editable(true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Last locked")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? LastLockedDate { get; internal set; }

        [Display(Name = "Approved?")]
        public bool Approved { get; set; }

        [Display(Name = "Approved by")]
        public string ApprovedByID { get; set; }
        
        /*public virtual ICollection<Service> Services { get; set; }*/
    }

    public class StartupProjectViewModel
    {
        //[Key]
        [Display(Name = "Project ID")]
        public string StartupID { get; set; }

        //Project-----------------------------------------------------
        
        [Display(Name = "Project name")]
        public string ProjectName { get; set; }

        [Display(Name = "Project domain")]
        public int? ProjectDomainID { get; set; }
        public SelectList ProjectDomainList { get; set; }
        
        [Editable(true)]
        [DataType(DataType.DateTime, ErrorMessage = "Use date format: yyyy-MM-dd")]
        [DisplayFormat(ApplyFormatInEditMode = true, NullDisplayText = "yyyy-MM-dd", DataFormatString = "{0:yyyy-MM-dd}")]
        [Display(Name = "Deadline")]
        public DateTime? DeadlineDate { get; set; }

        [MaxLength(1500)]
        [AllowHtml]
        [Display(Name = "Project summary (max 1500 characters) in plain text")]
        [RegularExpression("^[^0-9@]+$", ErrorMessage = "No numbers or special characters")]
        public string ProjectSummary { get; set; }

        [Display(Name = "We allow Enablers of Sweden to share this project information given, to at least one type of investor mention below")]
        public bool AllowSharing { get; set; }

        /*...*/

        public List<AllowedInvestorViewModel> AllowedInvestors { get; internal set; }

        //Funding-----------------------------------------------------

        [Display(Name = "Funding phase")]
        public int? FundingPhaseID { get; set; }
        public SelectList FundingPhaseList { get; set; }

        [Display(Name = "Funding need")]
        public int? FundingAmountID { get; set; } //FundingNeedID
        public SelectList FundingAmountList { get; set; } //FundingNeedList

        [Display(Name = "Do you see a need of more funding in the future?")]
        public bool FutureFundingNeeded { get; set; }

        [Display(Name = "How much time is already been spent?")]
        public int? AlreadySpentTime { get; set; }

        [Display(Name = "How much money is already been spent?")]
        public int? AlreadySpentMoney { get; set; }

        [Display(Name = "Will the founder(s) spend more of their own money ahead?")]
        public bool WillSpendOwnMoney { get; set; }

        //Budget-----------------------------------------------------------

        public List<FundingDivisionStartup> FundingDivisions { get; internal set; }

        [Display(Name = "Estimated exit plan")]
        public int? EstimatedExitPlanID { get; set; }
        public SelectList EstimatedExitPlanList { get; set; }

        [Range(1, 10, ErrorMessage = "Please enter a number between 0 and 10 for estimated break even")]
        [Display(Name = "Estimated break even")]
        public int? EstimatedBreakEven { get; set; }

        [Range(0, 10, ErrorMessage = "Please enter a number between 0 and 10 for possible income streams")]
        [Display(Name = "Possible income streams")]
        public int? PossibleIncomeStreams { get; set; }

        [Display(Name = "Do you have paying customers?")]
        public bool HavePayingCustomers { get; set; }

        //Team-------------------------------------------------------------

        [Range(1, 10, ErrorMessage = "Please enter a number between 1 and 10 for team member size")]
        [Display(Name = "Team member size")]
        public int? TeamMemberSize { get; set; }

        [Range(0, 250, ErrorMessage = "Please enter a number between 0 and 250 for team experience")]
        [Display(Name = "Team experience")]
        public int? TeamExperience { get; set; }

        [Display(Name = "Do all team members share the same vision?")]
        public bool TeamVisionShared { get; set; }

        [Display(Name = "Do you have fixed roles in the team?")]
        public bool HaveFixedRoles { get; set; }
        
        public List<TeamWeaknessViewModel> TeamWeaknesses { get; internal set; }

        //[Required]
        [Display(Name = "Are you looking for active investors?")]
        public bool LookingForActiveInvestors { get; set; }
        
        //Outcome-------------------------------------------------------------
        
        public List<StartupOutcomeViewModel> Outcomes { get; internal set; }
        //public bool OutcomesUnanswered { get; set; }

        [Display(Name = "Level of innovation")]
        public int? InnovationLevelID { get; set; }
        public SelectList InnovationLevelList { get; set; }

        [Display(Name = "Required scalability")]
        public int? ScalabilityID { get; set; }
        public SelectList ScalabilityList { get; set; }

        //-----------------------------------------------------------------

        public bool Updated { get; set; }

        public string Message { get; set; }
        public string UnansweredQuestion { get; set; }
    }

    public class StartupProjectPostViewModel : StartupProjectViewModel //Inheretence here is a good idea, works very well indeed!
    {
        //Project
        public string[] AllowedInvestorIDs { get; set; }
        public new string AllowSharing { get; set; }
        //Funding
        public new string FutureFundingNeeded { get; set; }
        public new string WillSpendOwnMoney { get; set; }
        //Budget
        public string[] FundingDivisionPercentages { get; set; }
        public new string HavePayingCustomers { get; set; }
        //Team
        public string[] TeamWeaknessIDs { get; set; }
        public new string TeamVisionShared { get; set; }
        public new string HaveFixedRoles { get; set; }
        public new string LookingForActiveInvestors { get; set; }
        //Outcome
        public string[] OutcomeIDs { get; set; }
        //-----------------------------------
        public string ActiveTab { get; set; }
    }

    public class AddNewProjectViewModel
    {
        [AllowHtml]
        [Display(Name = "Message for the Idea carrier")]
        public string IdeaCarrierMessage { get; set; }

        [Required]
        [Display(Name = "Project name")]
        public string StartupName { get; set; }

        [Required]
        [Display(Name = "Country")]
        public int CountryID { get; set; }
        public SelectList CountryList { get; set; }

        [Display(Name = "Swedish country ID")]
        public int SwedishCountryID { get; set; }

        [Display(Name = "Swedish region (Län)")]
        public int? SwedishRegionID { get; set; }
        public SelectList SwedishRegionList { get; set; }
    }
}
