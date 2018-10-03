using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace EoS.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserFullName { get { return UserFirstName + " " + UserLastName; } }

        [Editable(false)]
        public string ExternalId { get; internal set; } //for Investors

        [Editable(false)]
        [DataType(DataType.DateTime)]
        public DateTime UserStartDate { get; internal set; }

        [Editable(true)]
        [DataType(DataType.DateTime)]
        public DateTime? LastLoginDate { get; internal set; }

        [Editable(true)]
        [DataType(DataType.DateTime)]
        public DateTime? ExpiryDate { get; set; }

        public string CountryName { get; set; }

        public virtual ICollection<IdeaCarrier.Startup> Startups { get; set; } //for IdeaCarriers

        public virtual ICollection<Investor.Investment> Investments { get; set; } //for Investors

        public virtual ICollection<Admin.Blog> Blogs { get; set; } //for Admins

        public virtual ICollection<Shared.BlogComment> BlogComments { get; set; } //for Investors and IdeaCarriers

        //public bool? ActiveInvestor { get; set; } //for Investors <----to be implemented?

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public enum Role //Could also be List<Role>
    {
        [Display(Name = "Admin")]
        Admin,
        [Display(Name = "Idea Carrier")]
        IdeaCarrier,
        [Display(Name = "Investor")]
        Investor,
    }

    public enum RegisterRole
    {
        [Display(Name = "Idea Carrier")]
        IdeaCarrier,
        [Display(Name = "Investor")]
        Investor,
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("MMM", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        //Add tables to the MMM DB

        public DbSet<Admin.Blog> Blogs { get; set; }
        public DbSet<Admin.IdeaCarrierMessage> IdeaCarrierMessages { get; set; }
        public DbSet<Admin.InvestorMessage> InvestorMessages { get; set; }

        public DbSet<Investor.Investment> Investments { get; set; }
        public DbSet<IdeaCarrier.Startup> Startups { get; set; }

        public DbSet<Shared.ProjectDomain> ProjectDomains { get; set; } //<-----DbSet<Shared.Domain> Domains { get; set; }
        public DbSet<IdeaCarrier.AllowedInvestor> AllowedInvestors { get; set; }

        public DbSet<Shared.FundingAmount> FundingAmounts { get; set; }
        public DbSet<Shared.FundingPhase> FundingPhases { get; set; }

        public DbSet<IdeaCarrier.FundingDivision> FundingDivisions { get; set; }
        public DbSet<IdeaCarrier.FundingDivisionStartup> FundingDivisionStartups { get; set; }
        public DbSet<Shared.EstimatedExitPlan> EstimatedExitPlans { get; set; }

        public DbSet<Investor.TeamSkill> TeamSkills { get; set; }
        public DbSet<IdeaCarrier.TeamWeakness> TeamWeaknesses { get; set; }

        public DbSet<Shared.Outcome> Outcomes { get; set; }
        public DbSet<Shared.InnovationLevel> InnovationLevels { get; set; }
        public DbSet<Shared.Scalability> Scalabilities { get; set; }

        public DbSet<IdeaCarrier.Document> Documents { get; set; }

        public DbSet<Shared.BlogComment> BlogComments { get; set; }
        public DbSet<Shared.Country> Countries { get; set; }
        public DbSet<Shared.SwedishRegion> SwedishRegions { get; set; }

        public DbSet<Home.HomeInfo> HomeInfos { get; set; }
        public DbSet<SMTP.SmtpClient> SmtpClients { get; set; }

        public DbSet<MMM.MatchMaking> MatchMakings { get; set; }
    }
}