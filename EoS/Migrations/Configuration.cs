namespace EoS.Migrations
{
    using EoS.Models;
    using EoS.Models.Admin;
    using EoS.Models.IdeaCarrier;
    using EoS.Models.Investor;
    using EoS.Models.Shared;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<EoS.Models.ApplicationDbContext>
    {
        public Configuration() 
        {
            AutomaticMigrationsEnabled = true;
        }
        
        protected override void Seed(EoS.Models.ApplicationDbContext context)
        {
            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            var roleNames = new[] { Role.Admin.ToString(), Role.IdeaCarrier.ToString() /*...and so on*/ };
            foreach (var roleName in roleNames)
            {
                if (!context.Roles.Any(r => r.Name == roleName))
                {
                    //creates Roll
                    var role = new IdentityRole { Name = roleName };

                    var result = roleManager.Create(role);
                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join("\n", result.Errors));
                    }
                }
            }

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            var emails = new[] { "admin@fs.se", "ideacarrier@ideacarrier.se" /*...and so on*/ };

            foreach (var email in emails)
            {
                if (!context.Users.Any(u => u.UserName == email))
                {
                    //creating user
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        UserStartDate = DateTime.Now,
                    };

                    var result = userManager.Create(user, "_Xyz09tr&r9%pf");
                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join("\n", result.Errors));
                    }
                }
            }

            var adminUser = userManager.FindByName("admin@fs.se");
            userManager.AddToRole(adminUser.Id, Role.Admin.ToString());
            adminUser.UserFirstName = "Admin";
            adminUser.EmailConfirmed = true;

            var ideacarrierUser = userManager.FindByName("ideacarrier@ideacarrier.se");
            userManager.AddToRole(ideaCarrierUser.Id, Role.IdeaCarrier.ToString());
            ideaCarrierUser.UserFirstName = "IdeaCarrier";
            ideaCarrierUser.EmailConfirmed = true;
            
            /*...more users here*/

            context.Countries.AddOrUpdate(
                c => c.CountryName,
                new Country { CountryName = "Sweden", CountryAbbreviation = "SE" }, //CountryID = 1 is better
                new Country { CountryName = "Austria", CountryAbbreviation = "AT" }, //CountryID = 2 ...and so on
                new Country { CountryName = "Belgium", CountryAbbreviation = "BE" },
                new Country { CountryName = "Brazil", CountryAbbreviation = "BR" },
                new Country { CountryName = "Bulgaria", CountryAbbreviation = "BG" },
                new Country { CountryName = "China", CountryAbbreviation = "CN" },
                new Country { CountryName = "Croatia", CountryAbbreviation = "HR" },
                new Country { CountryName = "Republic of Cyprus", CountryAbbreviation = "CY" },
                new Country { CountryName = "Czech Republic", CountryAbbreviation = "CZ" },
                new Country { CountryName = "Denmark", CountryAbbreviation = "DK" },
                new Country { CountryName = "Estonia", CountryAbbreviation = "EE" },
                new Country { CountryName = "Finland", CountryAbbreviation = "FI" },
                new Country { CountryName = "France", CountryAbbreviation = "FR" },
                new Country { CountryName = "Germany", CountryAbbreviation = "DE" },
                new Country { CountryName = "Greece", CountryAbbreviation = "EL" },
                new Country { CountryName = "Hungary", CountryAbbreviation = "HU" },
                new Country { CountryName = "Iceland", CountryAbbreviation = "IS" },
                new Country { CountryName = "India", CountryAbbreviation = "IN" },
                new Country { CountryName = "Ireland", CountryAbbreviation = "IE" },
                new Country { CountryName = "Italy", CountryAbbreviation = "IT" },
                new Country { CountryName = "Latvia", CountryAbbreviation = "LV" },
                new Country { CountryName = "Lithuania", CountryAbbreviation = "LT" },
                new Country { CountryName = "Liechtenstein", CountryAbbreviation = "LI" },
                new Country { CountryName = "Luxembourg", CountryAbbreviation = "LU" },
                new Country { CountryName = "Malta", CountryAbbreviation = "MT" },
                new Country { CountryName = "Netherlands", CountryAbbreviation = "NL" },
                new Country { CountryName = "Norway", CountryAbbreviation = "NO" },
                new Country { CountryName = "Poland", CountryAbbreviation = "PL" },
                new Country { CountryName = "Portugal", CountryAbbreviation = "PT" },
                new Country { CountryName = "Romania", CountryAbbreviation = "RO" },
                new Country { CountryName = "Russia", CountryAbbreviation = "RU" },
                new Country { CountryName = "Slovakia", CountryAbbreviation = "SK" },
                new Country { CountryName = "Slovenia", CountryAbbreviation = "SI" },
                new Country { CountryName = "Spain", CountryAbbreviation = "ES" },
                new Country { CountryName = "Switzerland", CountryAbbreviation = "CH" },
                new Country { CountryName = "United Kingdom", CountryAbbreviation = "UK" },
                new Country { CountryName = "United States of America", CountryAbbreviation = "US" }
                );

            context.SwedishRegions.AddOrUpdate(
                sr => sr.RegionName,
                new SwedishRegion { RegionName = "Blekinge" },
                new SwedishRegion { RegionName = "Dalarna" },
                new SwedishRegion { RegionName = "Gotland" },
                new SwedishRegion { RegionName = "Gävleborg" },
                new SwedishRegion { RegionName = "Halland" },
                new SwedishRegion { RegionName = "Jämtland" },
                new SwedishRegion { RegionName = "Jönköping" },
                new SwedishRegion { RegionName = "Kalmar" },
                new SwedishRegion { RegionName = "Kronoberg" },
                new SwedishRegion { RegionName = "Norrbotten" },
                new SwedishRegion { RegionName = "Skåne" },
                new SwedishRegion { RegionName = "Stockholm" },
                new SwedishRegion { RegionName = "Södermanland" },
                new SwedishRegion { RegionName = "Uppsala" },
                new SwedishRegion { RegionName = "Värmland" },
                new SwedishRegion { RegionName = "Västerbotten" },
                new SwedishRegion { RegionName = "Västernorrland" },
                new SwedishRegion { RegionName = "Västmanland" },
                new SwedishRegion { RegionName = "Västra Götaland" },
                new SwedishRegion { RegionName = "Örebro" },
                new SwedishRegion { RegionName = "Östergötland" }
                );

            context.ProjectDomains.AddOrUpdate( //Domains is a better name
                 pd => pd.ProjectDomainName,
                 new ProjectDomain { ProjectDomainName = "Agriculture" },
                 new ProjectDomain { ProjectDomainName = "Advertising" },
                 new ProjectDomain { ProjectDomainName = "Banking" },
                 new ProjectDomain { ProjectDomainName = "Business Services" },
                 new ProjectDomain { ProjectDomainName = "Constructions" },
                 new ProjectDomain { ProjectDomainName = "Consulting" },
                 new ProjectDomain { ProjectDomainName = "Culture, Fun & Leisure" },
                 new ProjectDomain { ProjectDomainName = "Education" },
                 new ProjectDomain { ProjectDomainName = "Electricity" },
                 new ProjectDomain { ProjectDomainName = "Economics" },
                 new ProjectDomain { ProjectDomainName = "Finance" },
                 new ProjectDomain { ProjectDomainName = "Food production" },
                 new ProjectDomain { ProjectDomainName = "Hotel" },
                 new ProjectDomain { ProjectDomainName = "Hair & Beauty" },
                 new ProjectDomain { ProjectDomainName = "Health & Medical" },
                 new ProjectDomain { ProjectDomainName = "Hunting & Fishing" },
                 new ProjectDomain { ProjectDomainName = "IT" },
                 new ProjectDomain { ProjectDomainName = "Insurance" },
                 new ProjectDomain { ProjectDomainName = "Media" },
                 new ProjectDomain { ProjectDomainName = "Motor vehicle trade" },
                 new ProjectDomain { ProjectDomainName = "Marketing" },
                 new ProjectDomain { ProjectDomainName = "Manufacturing & Industry" },
                 new ProjectDomain { ProjectDomainName = "Public Administration & Society" },
                 new ProjectDomain { ProjectDomainName = "Retail" },
                 new ProjectDomain { ProjectDomainName = "Real Estate" },
                 new ProjectDomain { ProjectDomainName = "Restaurants" },
                 new ProjectDomain { ProjectDomainName = "Research & Development" },
                 new ProjectDomain { ProjectDomainName = "Rental & Leasing" },
                 new ProjectDomain { ProjectDomainName = "Repair & Installation" },
                 new ProjectDomain { ProjectDomainName = "Sales" },
                 new ProjectDomain { ProjectDomainName = "Staffing & Employment" },
                 new ProjectDomain { ProjectDomainName = "Telecommunications" },
                 new ProjectDomain { ProjectDomainName = "Travel Agency & Tourism" },
                 new ProjectDomain { ProjectDomainName = "Technical consultancy" },
                 new ProjectDomain { ProjectDomainName = "Transport" },
                 new ProjectDomain { ProjectDomainName = "Wholesale" },
                 new ProjectDomain { ProjectDomainName = "Warehousing" },
                 new ProjectDomain { ProjectDomainName = "Water" },
                 new ProjectDomain { ProjectDomainName = "Waste water" },
                 new ProjectDomain { ProjectDomainName = "Other" }
                 );

            context.AllowedInvestors.AddOrUpdate(
              ai => ai.AllowedInvestorName,
              new AllowedInvestor { AllowedInvestorName = "Within Sweden" },
              new AllowedInvestor { AllowedInvestorName = "Globally" }
              );

            context.EstimatedExitPlans.AddOrUpdate(
              eep => eep.EstimatedExitPlanName,
                new EstimatedExitPlan { EstimatedExitPlanName = "Budget" },
                new EstimatedExitPlan { EstimatedExitPlanName = "Break even" },
                new EstimatedExitPlan { EstimatedExitPlanName = "Possible income streams" },
                new EstimatedExitPlan { EstimatedExitPlanName = "ASAP" },
                new EstimatedExitPlan { EstimatedExitPlanName = "< 3 years" },
                new EstimatedExitPlan { EstimatedExitPlanName = "3 - 5 years" },
                new EstimatedExitPlan { EstimatedExitPlanName = "5 - 10 years" },
                new EstimatedExitPlan { EstimatedExitPlanName = "Long Term Involvement" }
                );

            context.FundingAmounts.AddOrUpdate(
                    fa => fa.FundingAmountValue,
                    new FundingAmount { FundingAmountValue = "0-500,000" },
                    new FundingAmount { FundingAmountValue = "500,000 - 1,000,000" },
                    new FundingAmount { FundingAmountValue = "1,000,000 - 2,000,000" },
                    new FundingAmount { FundingAmountValue = "2,000,000 - 4,000,000" },
                    new FundingAmount { FundingAmountValue = "4,000,000 - 8,000,000" },
                    new FundingAmount { FundingAmountValue = "8,000,000 - 16,000,000" },
                    new FundingAmount { FundingAmountValue = "16,000,000 - 32,000,000" },
                    new FundingAmount { FundingAmountValue = "32,000,000 - 64,000,000" },
                    new FundingAmount { FundingAmountValue = "64 000 000 and above" }
                    );

            context.FundingPhases.AddOrUpdate(
                  fp => fp.FundingPhaseName,
                  new FundingPhase { FundingPhaseName = "Idea Development" },
                  new FundingPhase { FundingPhaseName = "Prototype Development" },
                  new FundingPhase { FundingPhaseName = "Seed" },
                  new FundingPhase { FundingPhaseName = "Early growth" },
                  new FundingPhase { FundingPhaseName = "Round A" },
                  new FundingPhase { FundingPhaseName = "Round B" },
                  new FundingPhase { FundingPhaseName = "Round C" },
                  new FundingPhase { FundingPhaseName = "Round D" }
                  );

            context.FundingDivisions.AddOrUpdate(
                    fd => fd.FundingDivisionName,
                    new FundingDivision { FundingDivisionName = "M & A" },
                    new FundingDivision { FundingDivisionName = "R & D" },
                    new FundingDivision { FundingDivisionName = "Sales" },
                    new FundingDivision { FundingDivisionName = "Marketing" },
                    new FundingDivision { FundingDivisionName = "Administration" },
                    new FundingDivision { FundingDivisionName = "Other" }
                    );

            context.TeamWeaknesses.AddOrUpdate(
               tw => tw.TeamWeaknessName,
                new TeamWeakness { TeamWeaknessName = "M & A" },
                new TeamWeakness { TeamWeaknessName = "R & D" },
                new TeamWeakness { TeamWeaknessName = "Sales" },
                new TeamWeakness { TeamWeaknessName = "Marketing" },
                new TeamWeakness { TeamWeaknessName = "Administration" },
                new TeamWeakness { TeamWeaknessName = "Investment Readiness" },
                new TeamWeakness { TeamWeaknessName = "Manufactoring" },
                new TeamWeakness { TeamWeaknessName = "Distribution" },
                new TeamWeakness { TeamWeaknessName = "Team" },
                new TeamWeakness { TeamWeaknessName = "Other" }
                );

            context.TeamSkills.AddOrUpdate(
                ts => ts.SkillName,
                new TeamSkill { SkillName = "M & A" },
                new TeamSkill { SkillName = "R & D" },
                new TeamSkill { SkillName = "Sales" },
                new TeamSkill { SkillName = "Marketing" },
                new TeamSkill { SkillName = "Administration" },
                new TeamSkill { SkillName = "Investment readiness" },
                new TeamSkill { SkillName = "Manufacturing" },
                new TeamSkill { SkillName = "Distribution" },
                new TeamSkill { SkillName = "Team members" },
                new TeamSkill { SkillName = "Other" }
                );

            context.Outcomes.AddOrUpdate(
                o => o.OutcomeName,
                  new Outcome { OutcomeName = "Product" },
                  new Outcome { OutcomeName = "Service" },
                  new Outcome { OutcomeName = "Process" },
                  new Outcome { OutcomeName = "Software" },
                  new Outcome { OutcomeName = "Hardware" },
                  new Outcome { OutcomeName = "Other" }
                  );

            context.InnovationLevels.AddOrUpdate(
              il => il.InnovationLevelName,
               new InnovationLevel { InnovationLevelName = "0 - 3 already proven business model" },
               new InnovationLevel { InnovationLevelName = "4-6 innovation that could lead to a fair market share" },
               new InnovationLevel { InnovationLevelName = "7-8 this will for sure make a change" },
               new InnovationLevel { InnovationLevelName = "9 disruptive change" },
               new InnovationLevel { InnovationLevelName = "10 gamechanger \"It is nothing like it out there\"" }
               );

            context.Scalabilities.AddOrUpdate(
                sc => sc.ScalabilityName,
                new Scalability { ScalabilityName = "Local" },
                new Scalability { ScalabilityName = "Continental" },
                new Scalability { ScalabilityName = "Global" }
                );

            context.Startups.AddOrUpdate(
              su => su.StartupID,
              new Startup
              {
                  StartupID = "ICSE12346",
                  UserID = userManager.FindByName("ideacarrier@ideacarrier.se").Id,
                  StartupName = "Project1",
                  CountryID = 3,
                  ProjectDomainID = 2,
                  ProjectSummary = "Summary",
                  FundingPhaseID = 1,
                  FundingAmountID = 2,
                  EstimatedExitPlanID = 1,
                  EstimatedBreakEven = 1,
                  TeamMemberSize = 4,
                  TeamExperience = 4,
                  PossibleIncomeStreams = 2,
                  InnovationLevelID = 1,
                  DeadlineDate = DateTime.Now.AddMonths(1),
                  LastSavedDate = DateTime.Now,
                  CreatedDate = DateTime.Now
              });

            List<FundingDivision> fundingDivisions = context.FundingDivisions.ToList();

            foreach (var fundingDivision in fundingDivisions)
            {
                context.FundingDivisionStartups.AddOrUpdate(
                    fds => fds.FundingDivisionID,
                    new FundingDivisionStartup
                    {
                        FundingDivisionID = fundingDivision.FundingDivisionID, //fundingDivision.FundingDivisionName is better, makes the code simpler
                        Percentage = 0,
                        StartupID = "ICSE12346"
                    });
            }

            context.IdeaCarrierMessages.AddOrUpdate(
            icm => icm.Id,
            new IdeaCarrierMessage
            {
                Text =
                "Kul att ha dig här. Vi brinner för att möjliggöra så att fler svenska startups skall förverkligas och få möjligheten.<br />" +
                "Tips: Det går bra att lägga upp flera olika projekt i din användarprofil.<br />&nbsp;</br />" +
                "Fun to have you here. We are burning to enable more Swedish startups to be implemented and get the opportunity.<br />" +
                "Tips It is helpful to post several projects in your user profile.",
                AllowSharing_DisplayName = ""
            });
            
            context.SaveChanges();

        }
    }
}
