namespace EoS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AllowedInvestors",
                c => new
                    {
                        AllowedInvestorID = c.Int(nullable: false, identity: true),
                        AllowedInvestorName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.AllowedInvestorID);
            
            CreateTable(
                "dbo.Startups",
                c => new
                    {
                        StartupID = c.String(nullable: false, maxLength: 128),
                        UserID = c.String(maxLength: 128),
                        CountryID = c.Int(nullable: false),
                        SwedishRegionID = c.Int(),
                        StartupName = c.String(nullable: false),
                        ProjectDomainID = c.Int(),
                        DeadlineDate = c.DateTime(),
                        ProjectSummary = c.String(maxLength: 1500),
                        AllowSharing = c.Boolean(nullable: false),
                        FundingPhaseID = c.Int(),
                        FundingAmountID = c.Int(),
                        EstimatedExitPlanID = c.Int(),
                        FutureFundingNeeded = c.Boolean(nullable: false),
                        AlreadySpentTime = c.Int(),
                        AlreadySpentMoney = c.Int(),
                        WillSpendOwnMoney = c.Boolean(nullable: false),
                        EstimatedBreakEven = c.Int(),
                        PossibleIncomeStreams = c.Int(),
                        HavePayingCustomers = c.Boolean(nullable: false),
                        TeamMemberSize = c.Int(),
                        TeamExperience = c.Int(),
                        TeamVisionShared = c.Boolean(nullable: false),
                        HaveFixedRoles = c.Boolean(nullable: false),
                        LookingForActiveInvestors = c.Boolean(nullable: false),
                        InnovationLevelID = c.Int(),
                        ScalabilityID = c.Int(),
                        LastSavedDate = c.DateTime(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        Locked = c.Boolean(nullable: false),
                        Approved = c.Boolean(nullable: false),
                        ApprovedByID = c.String(),
                        SwedishRegion_RegionID = c.Int(),
                    })
                .PrimaryKey(t => t.StartupID)
                .ForeignKey("dbo.Countries", t => t.CountryID, cascadeDelete: true)
                .ForeignKey("dbo.EstimatedExitPlans", t => t.EstimatedExitPlanID)
                .ForeignKey("dbo.FundingAmounts", t => t.FundingAmountID)
                .ForeignKey("dbo.FundingPhases", t => t.FundingPhaseID)
                .ForeignKey("dbo.InnovationLevels", t => t.InnovationLevelID)
                .ForeignKey("dbo.ProjectDomains", t => t.ProjectDomainID)
                .ForeignKey("dbo.Scalabilities", t => t.ScalabilityID)
                .ForeignKey("dbo.SwedishRegions", t => t.SwedishRegion_RegionID)
                .ForeignKey("dbo.AspNetUsers", t => t.UserID)
                .Index(t => t.UserID)
                .Index(t => t.CountryID)
                .Index(t => t.ProjectDomainID)
                .Index(t => t.FundingPhaseID)
                .Index(t => t.FundingAmountID)
                .Index(t => t.EstimatedExitPlanID)
                .Index(t => t.InnovationLevelID)
                .Index(t => t.ScalabilityID)
                .Index(t => t.SwedishRegion_RegionID);
            
            CreateTable(
                "dbo.Countries",
                c => new
                    {
                        CountryID = c.Int(nullable: false, identity: true),
                        CountryName = c.String(nullable: false),
                        CountryAbbreviation = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.CountryID);
            
            CreateTable(
                "dbo.EstimatedExitPlans",
                c => new
                    {
                        EstimatedExitPlanID = c.Int(nullable: false, identity: true),
                        EstimatedExitPlanName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.EstimatedExitPlanID);
            
            CreateTable(
                "dbo.Investments",
                c => new
                    {
                        InvestmentID = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(maxLength: 128),
                        CountryID = c.Int(nullable: false),
                        SwedishRegionID = c.Int(),
                        ProfileName = c.String(),
                        ProjectDomainID = c.Int(),
                        FutureFundingNeeded = c.Boolean(nullable: false),
                        EstimatedBreakEven = c.Int(),
                        PossibleIncomeStreams = c.Int(),
                        TeamMemberSizeMoreThanOne = c.Boolean(nullable: false),
                        TeamHasExperience = c.Boolean(nullable: false),
                        ActiveInvestor = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        LastSavedDate = c.DateTime(nullable: false),
                        DueDate = c.DateTime(),
                        Locked = c.Boolean(nullable: false),
                        Active = c.Boolean(nullable: false),
                        SwedishRegion_RegionID = c.Int(),
                    })
                .PrimaryKey(t => t.InvestmentID)
                .ForeignKey("dbo.Countries", t => t.CountryID, cascadeDelete: true)
                .ForeignKey("dbo.ProjectDomains", t => t.ProjectDomainID)
                .ForeignKey("dbo.SwedishRegions", t => t.SwedishRegion_RegionID)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.CountryID)
                .Index(t => t.ProjectDomainID)
                .Index(t => t.SwedishRegion_RegionID);
            
            CreateTable(
                "dbo.FundingAmounts",
                c => new
                    {
                        FundingAmountID = c.Int(nullable: false, identity: true),
                        FundingAmountValue = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.FundingAmountID);
            
            CreateTable(
                "dbo.FundingPhases",
                c => new
                    {
                        FundingPhaseID = c.Int(nullable: false, identity: true),
                        FundingPhaseName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.FundingPhaseID);
            
            CreateTable(
                "dbo.InnovationLevels",
                c => new
                    {
                        InnovationLevelID = c.Int(nullable: false, identity: true),
                        InnovationLevelName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.InnovationLevelID);
            
            CreateTable(
                "dbo.MatchMakings",
                c => new
                    {
                        MatchMakingId = c.Int(nullable: false, identity: true),
                        MatchMakingDate = c.DateTime(nullable: false),
                        InvestmentId = c.String(maxLength: 128),
                        StartupId = c.String(maxLength: 128),
                        ProjectDomainMatched = c.Boolean(),
                        FundingPhaseMatched = c.Boolean(),
                        FundingAmountMatched = c.Boolean(),
                        EstimatedExitPlanMatched = c.Boolean(),
                        OutcomesMatched = c.Boolean(),
                        InnovationLevelMatched = c.Boolean(),
                        ScalabilityMatched = c.Boolean(),
                        TeamSkillsMatched = c.Boolean(),
                        NoOfMatches = c.Int(nullable: false),
                        MaxNoOfMatches = c.Int(nullable: false),
                        Sent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.MatchMakingId)
                .ForeignKey("dbo.Investments", t => t.InvestmentId)
                .ForeignKey("dbo.Startups", t => t.StartupId)
                .Index(t => t.InvestmentId)
                .Index(t => t.StartupId);
            
            CreateTable(
                "dbo.Outcomes",
                c => new
                    {
                        OutcomeID = c.Int(nullable: false, identity: true),
                        OutcomeName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.OutcomeID);
            
            CreateTable(
                "dbo.ProjectDomains",
                c => new
                    {
                        ProjectDomainID = c.Int(nullable: false, identity: true),
                        ProjectDomainName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ProjectDomainID);
            
            CreateTable(
                "dbo.Scalabilities",
                c => new
                    {
                        ScalabilityID = c.Int(nullable: false, identity: true),
                        ScalabilityName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ScalabilityID);
            
            CreateTable(
                "dbo.SwedishRegions",
                c => new
                    {
                        RegionID = c.Int(nullable: false, identity: true),
                        RegionName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.RegionID);
            
            CreateTable(
                "dbo.TeamSkills",
                c => new
                    {
                        SkillID = c.Int(nullable: false, identity: true),
                        SkillName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.SkillID);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserFirstName = c.String(),
                        UserLastName = c.String(),
                        ExternalId = c.String(),
                        UserStartDate = c.DateTime(nullable: false),
                        LastLoginDate = c.DateTime(),
                        ExpiryDate = c.DateTime(),
                        CountryName = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.BlogComments",
                c => new
                    {
                        BlogCommentId = c.Int(nullable: false, identity: true),
                        BlogCommentText = c.String(nullable: false),
                        BlogCommentDate = c.DateTime(nullable: false),
                        CreatorId = c.String(maxLength: 128),
                        BlogId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BlogCommentId)
                .ForeignKey("dbo.Blogs", t => t.BlogId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatorId)
                .Index(t => t.CreatorId)
                .Index(t => t.BlogId);
            
            CreateTable(
                "dbo.Blogs",
                c => new
                    {
                        BlogId = c.Int(nullable: false, identity: true),
                        BlogText = c.String(nullable: false),
                        BlogDate = c.DateTime(nullable: false),
                        CreatorId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.BlogId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatorId)
                .Index(t => t.CreatorId);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.FundingDivisionStartups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FundingDivisionID = c.Int(nullable: false),
                        StartupID = c.String(maxLength: 128),
                        Percentage = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FundingDivisions", t => t.FundingDivisionID, cascadeDelete: true)
                .ForeignKey("dbo.Startups", t => t.StartupID)
                .Index(t => t.FundingDivisionID)
                .Index(t => t.StartupID);
            
            CreateTable(
                "dbo.FundingDivisions",
                c => new
                    {
                        FundingDivisionID = c.Int(nullable: false, identity: true),
                        FundingDivisionName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.FundingDivisionID);
            
            CreateTable(
                "dbo.TeamWeaknesses",
                c => new
                    {
                        TeamWeaknessID = c.Int(nullable: false, identity: true),
                        TeamWeaknessName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.TeamWeaknessID);
            
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        DocId = c.Int(nullable: false, identity: true),
                        DocName = c.String(),
                        DocDescription = c.String(),
                        DocTimestamp = c.DateTime(nullable: false),
                        UserId = c.String(maxLength: 128),
                        StartupID = c.String(maxLength: 128),
                        DocURL = c.String(),
                    })
                .PrimaryKey(t => t.DocId)
                .ForeignKey("dbo.Startups", t => t.StartupID)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.StartupID);
            
            CreateTable(
                "dbo.HomeInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.IdeaCarrierMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InvestorMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.SmtpClients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MailRecipient = c.String(nullable: false),
                        CredentialUserName = c.String(nullable: false),
                        CredentialPassword = c.String(nullable: false),
                        Host = c.String(nullable: false),
                        Port = c.Int(nullable: false),
                        EnableSsl = c.Boolean(nullable: false),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StartupAllowedInvestors",
                c => new
                    {
                        Startup_StartupID = c.String(nullable: false, maxLength: 128),
                        AllowedInvestor_AllowedInvestorID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Startup_StartupID, t.AllowedInvestor_AllowedInvestorID })
                .ForeignKey("dbo.Startups", t => t.Startup_StartupID, cascadeDelete: true)
                .ForeignKey("dbo.AllowedInvestors", t => t.AllowedInvestor_AllowedInvestorID, cascadeDelete: true)
                .Index(t => t.Startup_StartupID)
                .Index(t => t.AllowedInvestor_AllowedInvestorID);
            
            CreateTable(
                "dbo.InvestmentEstimatedExitPlans",
                c => new
                    {
                        Investment_InvestmentID = c.String(nullable: false, maxLength: 128),
                        EstimatedExitPlan_EstimatedExitPlanID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Investment_InvestmentID, t.EstimatedExitPlan_EstimatedExitPlanID })
                .ForeignKey("dbo.Investments", t => t.Investment_InvestmentID, cascadeDelete: true)
                .ForeignKey("dbo.EstimatedExitPlans", t => t.EstimatedExitPlan_EstimatedExitPlanID, cascadeDelete: true)
                .Index(t => t.Investment_InvestmentID)
                .Index(t => t.EstimatedExitPlan_EstimatedExitPlanID);
            
            CreateTable(
                "dbo.FundingAmountInvestments",
                c => new
                    {
                        FundingAmount_FundingAmountID = c.Int(nullable: false),
                        Investment_InvestmentID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.FundingAmount_FundingAmountID, t.Investment_InvestmentID })
                .ForeignKey("dbo.FundingAmounts", t => t.FundingAmount_FundingAmountID, cascadeDelete: true)
                .ForeignKey("dbo.Investments", t => t.Investment_InvestmentID, cascadeDelete: true)
                .Index(t => t.FundingAmount_FundingAmountID)
                .Index(t => t.Investment_InvestmentID);
            
            CreateTable(
                "dbo.FundingPhaseInvestments",
                c => new
                    {
                        FundingPhase_FundingPhaseID = c.Int(nullable: false),
                        Investment_InvestmentID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.FundingPhase_FundingPhaseID, t.Investment_InvestmentID })
                .ForeignKey("dbo.FundingPhases", t => t.FundingPhase_FundingPhaseID, cascadeDelete: true)
                .ForeignKey("dbo.Investments", t => t.Investment_InvestmentID, cascadeDelete: true)
                .Index(t => t.FundingPhase_FundingPhaseID)
                .Index(t => t.Investment_InvestmentID);
            
            CreateTable(
                "dbo.InnovationLevelInvestments",
                c => new
                    {
                        InnovationLevel_InnovationLevelID = c.Int(nullable: false),
                        Investment_InvestmentID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.InnovationLevel_InnovationLevelID, t.Investment_InvestmentID })
                .ForeignKey("dbo.InnovationLevels", t => t.InnovationLevel_InnovationLevelID, cascadeDelete: true)
                .ForeignKey("dbo.Investments", t => t.Investment_InvestmentID, cascadeDelete: true)
                .Index(t => t.InnovationLevel_InnovationLevelID)
                .Index(t => t.Investment_InvestmentID);
            
            CreateTable(
                "dbo.OutcomeInvestments",
                c => new
                    {
                        Outcome_OutcomeID = c.Int(nullable: false),
                        Investment_InvestmentID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Outcome_OutcomeID, t.Investment_InvestmentID })
                .ForeignKey("dbo.Outcomes", t => t.Outcome_OutcomeID, cascadeDelete: true)
                .ForeignKey("dbo.Investments", t => t.Investment_InvestmentID, cascadeDelete: true)
                .Index(t => t.Outcome_OutcomeID)
                .Index(t => t.Investment_InvestmentID);
            
            CreateTable(
                "dbo.OutcomeStartups",
                c => new
                    {
                        Outcome_OutcomeID = c.Int(nullable: false),
                        Startup_StartupID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Outcome_OutcomeID, t.Startup_StartupID })
                .ForeignKey("dbo.Outcomes", t => t.Outcome_OutcomeID, cascadeDelete: true)
                .ForeignKey("dbo.Startups", t => t.Startup_StartupID, cascadeDelete: true)
                .Index(t => t.Outcome_OutcomeID)
                .Index(t => t.Startup_StartupID);
            
            CreateTable(
                "dbo.ScalabilityInvestments",
                c => new
                    {
                        Scalability_ScalabilityID = c.Int(nullable: false),
                        Investment_InvestmentID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Scalability_ScalabilityID, t.Investment_InvestmentID })
                .ForeignKey("dbo.Scalabilities", t => t.Scalability_ScalabilityID, cascadeDelete: true)
                .ForeignKey("dbo.Investments", t => t.Investment_InvestmentID, cascadeDelete: true)
                .Index(t => t.Scalability_ScalabilityID)
                .Index(t => t.Investment_InvestmentID);
            
            CreateTable(
                "dbo.TeamSkillInvestments",
                c => new
                    {
                        TeamSkill_SkillID = c.Int(nullable: false),
                        Investment_InvestmentID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.TeamSkill_SkillID, t.Investment_InvestmentID })
                .ForeignKey("dbo.TeamSkills", t => t.TeamSkill_SkillID, cascadeDelete: true)
                .ForeignKey("dbo.Investments", t => t.Investment_InvestmentID, cascadeDelete: true)
                .Index(t => t.TeamSkill_SkillID)
                .Index(t => t.Investment_InvestmentID);
            
            CreateTable(
                "dbo.TeamWeaknessStartups",
                c => new
                    {
                        TeamWeakness_TeamWeaknessID = c.Int(nullable: false),
                        Startup_StartupID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.TeamWeakness_TeamWeaknessID, t.Startup_StartupID })
                .ForeignKey("dbo.TeamWeaknesses", t => t.TeamWeakness_TeamWeaknessID, cascadeDelete: true)
                .ForeignKey("dbo.Startups", t => t.Startup_StartupID, cascadeDelete: true)
                .Index(t => t.TeamWeakness_TeamWeaknessID)
                .Index(t => t.Startup_StartupID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Documents", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Documents", "StartupID", "dbo.Startups");
            DropForeignKey("dbo.Startups", "UserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.TeamWeaknessStartups", "Startup_StartupID", "dbo.Startups");
            DropForeignKey("dbo.TeamWeaknessStartups", "TeamWeakness_TeamWeaknessID", "dbo.TeamWeaknesses");
            DropForeignKey("dbo.Startups", "SwedishRegion_RegionID", "dbo.SwedishRegions");
            DropForeignKey("dbo.Startups", "ScalabilityID", "dbo.Scalabilities");
            DropForeignKey("dbo.FundingDivisionStartups", "StartupID", "dbo.Startups");
            DropForeignKey("dbo.FundingDivisionStartups", "FundingDivisionID", "dbo.FundingDivisions");
            DropForeignKey("dbo.Startups", "ProjectDomainID", "dbo.ProjectDomains");
            DropForeignKey("dbo.Startups", "InnovationLevelID", "dbo.InnovationLevels");
            DropForeignKey("dbo.Startups", "FundingPhaseID", "dbo.FundingPhases");
            DropForeignKey("dbo.Startups", "FundingAmountID", "dbo.FundingAmounts");
            DropForeignKey("dbo.Startups", "EstimatedExitPlanID", "dbo.EstimatedExitPlans");
            DropForeignKey("dbo.Investments", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogComments", "CreatorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Blogs", "CreatorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogComments", "BlogId", "dbo.Blogs");
            DropForeignKey("dbo.TeamSkillInvestments", "Investment_InvestmentID", "dbo.Investments");
            DropForeignKey("dbo.TeamSkillInvestments", "TeamSkill_SkillID", "dbo.TeamSkills");
            DropForeignKey("dbo.Investments", "SwedishRegion_RegionID", "dbo.SwedishRegions");
            DropForeignKey("dbo.ScalabilityInvestments", "Investment_InvestmentID", "dbo.Investments");
            DropForeignKey("dbo.ScalabilityInvestments", "Scalability_ScalabilityID", "dbo.Scalabilities");
            DropForeignKey("dbo.Investments", "ProjectDomainID", "dbo.ProjectDomains");
            DropForeignKey("dbo.OutcomeStartups", "Startup_StartupID", "dbo.Startups");
            DropForeignKey("dbo.OutcomeStartups", "Outcome_OutcomeID", "dbo.Outcomes");
            DropForeignKey("dbo.OutcomeInvestments", "Investment_InvestmentID", "dbo.Investments");
            DropForeignKey("dbo.OutcomeInvestments", "Outcome_OutcomeID", "dbo.Outcomes");
            DropForeignKey("dbo.MatchMakings", "StartupId", "dbo.Startups");
            DropForeignKey("dbo.MatchMakings", "InvestmentId", "dbo.Investments");
            DropForeignKey("dbo.InnovationLevelInvestments", "Investment_InvestmentID", "dbo.Investments");
            DropForeignKey("dbo.InnovationLevelInvestments", "InnovationLevel_InnovationLevelID", "dbo.InnovationLevels");
            DropForeignKey("dbo.FundingPhaseInvestments", "Investment_InvestmentID", "dbo.Investments");
            DropForeignKey("dbo.FundingPhaseInvestments", "FundingPhase_FundingPhaseID", "dbo.FundingPhases");
            DropForeignKey("dbo.FundingAmountInvestments", "Investment_InvestmentID", "dbo.Investments");
            DropForeignKey("dbo.FundingAmountInvestments", "FundingAmount_FundingAmountID", "dbo.FundingAmounts");
            DropForeignKey("dbo.InvestmentEstimatedExitPlans", "EstimatedExitPlan_EstimatedExitPlanID", "dbo.EstimatedExitPlans");
            DropForeignKey("dbo.InvestmentEstimatedExitPlans", "Investment_InvestmentID", "dbo.Investments");
            DropForeignKey("dbo.Investments", "CountryID", "dbo.Countries");
            DropForeignKey("dbo.Startups", "CountryID", "dbo.Countries");
            DropForeignKey("dbo.StartupAllowedInvestors", "AllowedInvestor_AllowedInvestorID", "dbo.AllowedInvestors");
            DropForeignKey("dbo.StartupAllowedInvestors", "Startup_StartupID", "dbo.Startups");
            DropIndex("dbo.TeamWeaknessStartups", new[] { "Startup_StartupID" });
            DropIndex("dbo.TeamWeaknessStartups", new[] { "TeamWeakness_TeamWeaknessID" });
            DropIndex("dbo.TeamSkillInvestments", new[] { "Investment_InvestmentID" });
            DropIndex("dbo.TeamSkillInvestments", new[] { "TeamSkill_SkillID" });
            DropIndex("dbo.ScalabilityInvestments", new[] { "Investment_InvestmentID" });
            DropIndex("dbo.ScalabilityInvestments", new[] { "Scalability_ScalabilityID" });
            DropIndex("dbo.OutcomeStartups", new[] { "Startup_StartupID" });
            DropIndex("dbo.OutcomeStartups", new[] { "Outcome_OutcomeID" });
            DropIndex("dbo.OutcomeInvestments", new[] { "Investment_InvestmentID" });
            DropIndex("dbo.OutcomeInvestments", new[] { "Outcome_OutcomeID" });
            DropIndex("dbo.InnovationLevelInvestments", new[] { "Investment_InvestmentID" });
            DropIndex("dbo.InnovationLevelInvestments", new[] { "InnovationLevel_InnovationLevelID" });
            DropIndex("dbo.FundingPhaseInvestments", new[] { "Investment_InvestmentID" });
            DropIndex("dbo.FundingPhaseInvestments", new[] { "FundingPhase_FundingPhaseID" });
            DropIndex("dbo.FundingAmountInvestments", new[] { "Investment_InvestmentID" });
            DropIndex("dbo.FundingAmountInvestments", new[] { "FundingAmount_FundingAmountID" });
            DropIndex("dbo.InvestmentEstimatedExitPlans", new[] { "EstimatedExitPlan_EstimatedExitPlanID" });
            DropIndex("dbo.InvestmentEstimatedExitPlans", new[] { "Investment_InvestmentID" });
            DropIndex("dbo.StartupAllowedInvestors", new[] { "AllowedInvestor_AllowedInvestorID" });
            DropIndex("dbo.StartupAllowedInvestors", new[] { "Startup_StartupID" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Documents", new[] { "StartupID" });
            DropIndex("dbo.Documents", new[] { "UserId" });
            DropIndex("dbo.FundingDivisionStartups", new[] { "StartupID" });
            DropIndex("dbo.FundingDivisionStartups", new[] { "FundingDivisionID" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.Blogs", new[] { "CreatorId" });
            DropIndex("dbo.BlogComments", new[] { "BlogId" });
            DropIndex("dbo.BlogComments", new[] { "CreatorId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.MatchMakings", new[] { "StartupId" });
            DropIndex("dbo.MatchMakings", new[] { "InvestmentId" });
            DropIndex("dbo.Investments", new[] { "SwedishRegion_RegionID" });
            DropIndex("dbo.Investments", new[] { "ProjectDomainID" });
            DropIndex("dbo.Investments", new[] { "CountryID" });
            DropIndex("dbo.Investments", new[] { "UserId" });
            DropIndex("dbo.Startups", new[] { "SwedishRegion_RegionID" });
            DropIndex("dbo.Startups", new[] { "ScalabilityID" });
            DropIndex("dbo.Startups", new[] { "InnovationLevelID" });
            DropIndex("dbo.Startups", new[] { "EstimatedExitPlanID" });
            DropIndex("dbo.Startups", new[] { "FundingAmountID" });
            DropIndex("dbo.Startups", new[] { "FundingPhaseID" });
            DropIndex("dbo.Startups", new[] { "ProjectDomainID" });
            DropIndex("dbo.Startups", new[] { "CountryID" });
            DropIndex("dbo.Startups", new[] { "UserID" });
            DropTable("dbo.TeamWeaknessStartups");
            DropTable("dbo.TeamSkillInvestments");
            DropTable("dbo.ScalabilityInvestments");
            DropTable("dbo.OutcomeStartups");
            DropTable("dbo.OutcomeInvestments");
            DropTable("dbo.InnovationLevelInvestments");
            DropTable("dbo.FundingPhaseInvestments");
            DropTable("dbo.FundingAmountInvestments");
            DropTable("dbo.InvestmentEstimatedExitPlans");
            DropTable("dbo.StartupAllowedInvestors");
            DropTable("dbo.SmtpClients");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.InvestorMessages");
            DropTable("dbo.IdeaCarrierMessages");
            DropTable("dbo.HomeInfoes");
            DropTable("dbo.Documents");
            DropTable("dbo.TeamWeaknesses");
            DropTable("dbo.FundingDivisions");
            DropTable("dbo.FundingDivisionStartups");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.Blogs");
            DropTable("dbo.BlogComments");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.TeamSkills");
            DropTable("dbo.SwedishRegions");
            DropTable("dbo.Scalabilities");
            DropTable("dbo.ProjectDomains");
            DropTable("dbo.Outcomes");
            DropTable("dbo.MatchMakings");
            DropTable("dbo.InnovationLevels");
            DropTable("dbo.FundingPhases");
            DropTable("dbo.FundingAmounts");
            DropTable("dbo.Investments");
            DropTable("dbo.EstimatedExitPlans");
            DropTable("dbo.Countries");
            DropTable("dbo.Startups");
            DropTable("dbo.AllowedInvestors");
        }
    }
}
