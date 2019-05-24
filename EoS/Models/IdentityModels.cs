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
    public class ApplicationUser : IdentityUser
    {
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserFullName { get { return UserFirstName + " " + UserLastName; } }

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

        public virtual ICollection<IdeaCarrier.Startup> Startups { get; set; } //StartupProjects for IdeaCarriers

        /*...*/

        public virtual ICollection<Admin.Blog> Blogs { get; set; } //for Admins

        public virtual ICollection<Shared.BlogComment> BlogComments { get; set; } //for IdeaCarriers and others

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    public enum Role //Could also be List<Role>
    {
        [Display(Name = "Admin")]
        Admin,
        [Display(Name = "Idea Carrier")]
        IdeaCarrier
        /*...*/
    }

    public enum RegisterRole
    {
        [Display(Name = "Idea Carrier")]
        IdeaCarrier,
        /*...*/
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("FS", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        //Add tables to the database named FS

        public DbSet<Admin.Blog> Blogs { get; set; }
        public DbSet<Admin.IdeaCarrierMessage> IdeaCarrierMessages { get; set; }
        /*...*/

        public DbSet<IdeaCarrier.Startup> Startups { get; set; }
        
        /*...*/

        /*...*/
        /*...*/

        public DbSet<IdeaCarrier.FundingDivision> FundingDivisions { get; set; }
        public DbSet<IdeaCarrier.FundingDivisionStartup> FundingDivisionStartups { get; set; }
        /*...*/

        public DbSet<IdeaCarrier.TeamWeakness> TeamWeaknesses { get; set; }
        /*...*/

        /*...*/
        /*...*/
        /*...*/

        /*...*/

        public DbSet<Shared.BlogComment> BlogComments { get; set; }
        public DbSet<Shared.Country> Countries { get; set; }
        public DbSet<Shared.SwedishRegion> SwedishRegions { get; set; }

        public DbSet<Home.HomeInfo> HomeInfos { get; set; }
        public DbSet<SMTP.SmtpClient> SmtpClients { get; set; }

        /*Table for the main service here*/
    }
}
