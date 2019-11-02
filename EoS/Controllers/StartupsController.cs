using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EoS.Models;
using EoS.Models.IdeaCarrier;
using Microsoft.AspNet.Identity;
using System.IO;
using EoS.Models.Shared;
/* ... */
using System.Threading.Tasks;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace EoS.Controllers
{
    [Authorize]
    public class StartupsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Startups
        [Authorize(Roles = "Admin, IdeaCarrier")]
        public ActionResult Index(string id, bool? matchable, string orderBy) //id==ideaCarrierId
        {
            List<Models.IdeaCarrier.Startup> startupProjects = null;

            /*...*/
            ViewBag.UserRole = "";
            ViewBag.IdeaCarrierId = "";
            ViewBag.IdeaCarrierUserName = "";

            if (User.IsInRole(Role.Admin.ToString()))
            {
                ViewBag.UserRole = Role.Admin.ToString();

                if (!string.IsNullOrEmpty(id)) //The Admin takes a look at a special IdeaCarrier's startup projects
                {
                    ApplicationUser ideaCarrier = db.Users.Find(id);
                    if (ideaCarrier != null)
                    {
                        ViewBag.IdeaCarrierId = id;
                        ViewBag.IdeaCarrierUserName = ideaCarrier.UserName;

                        if (!matchable.HasValue)
                        {
                            startupProjects = ideaCarrier.Startups.ToList();
                        }
                        else if (matchable.Value)
                        {
                            ViewBag.Matchable = true;
                            startupProjects = ideaCarrier.Startups.Where(s => s.Locked && s.Approved && (!s.DeadlineDate.HasValue || s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).OrderBy(s => s.StartupID).ToList();
                        }
                        else
                        {
                            ViewBag.Matchable = false;
                            startupProjects = ideaCarrier.Startups.Where(s => !s.Locked || !s.Approved || (s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) <= 0)).OrderBy(s => s.StartupID).ToList();
                        }

                        /*...*/
                    }
                }
                else //Show all Startup projects
                {
                    if (!matchable.HasValue)
                    {
                        startupProjects = db.Startups.ToList();
                    }
                    else if (matchable.Value)
                    {
                        ViewBag.Matchable = true;
                        startupProjects = db.Startups.Where(s => s.Locked && s.Approved && (!s.DeadlineDate.HasValue || s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).OrderBy(s => s.StartupID).ToList();
                    }
                    else
                    {
                        ViewBag.Matchable = false;
                        startupProjects = db.Startups.Where(s => !s.Locked || !s.Approved || (s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) <= 0)).OrderBy(s => s.StartupID).ToList();
                    }
                    /*...*/
                    /*...*/
                }
            }
            else //if (User.IsInRole("IdeaCarrier"))
            {
                string currentUserId = User.Identity.GetUserId();
                //ApplicationUser currentUser = db.Users.Find(currentUserId); //not necessary
                startupProjects = db.Startups.Where(u => u.UserID == currentUserId).ToList();
                ViewBag.UserRole = Role.IdeaCarrier.ToString();
                //return View(UserStartups);
            }

            if (startupProjects == null) startupProjects = db.Startups.ToList();

            if (!string.IsNullOrEmpty(orderBy))
                switch (orderBy.ToUpper())
                {
                    case "USERNAME": return View(startupProjects.OrderBy(su => su.User.UserName)); //break;
                    case "STARTUPID": return View(startupProjects.OrderBy(su => su.StartupID));
                    case "COUNTRY": return View(startupProjects.OrderBy(su => su.Country.CountryName));
                    /*...*/
                    case "PROJECTDOMAINNAME": return View(startupProjects.OrderBy(su => su.ProjectDomain?.ProjectDomainName));
                    /*...*/
                    /*...*/
                    case "LASTSAVEDDATE": return View(startupProjects.OrderByDescending(su => su.LastSavedDate));
                    case "DEADLINEDDATE": return View(startupProjects.OrderByDescending(su => su.DeadlineDate));
                    case "CREATEDDATE": return View(startupProjects.OrderByDescending(su => su.CreatedDate));
                }

            return View(startupProjects);
        }

        // GET: Startups/ProjectDetails/5
        [Authorize(Roles = "Admin, IdeaCarrier")]
        public ActionResult ProjectDetails(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(id);
            if (startupProject == null)
            {
                return HttpNotFound();
            }

            ViewBag.SwedishRegionName = "";
            if (startupProject.SwedishRegionID.HasValue)
                ViewBag.SwedishRegionName = db.SwedishRegions.Where(sr => sr.RegionID == startupProject.SwedishRegionID).FirstOrDefault().RegionName;

            ViewBag.UserRole = Role.IdeaCarrier.ToString();
            ViewBag.ApprovedBy = "";
            ViewBag.FormIsFinished = IsFormFinished(startupProject);

            ViewBag.TheOnlyProject = false;
            if (startupProject.User.Startups.Count() == 1) ViewBag.TheOnlyProject = true;

            if (User.IsInRole(Role.Admin.ToString()))
            {
                ViewBag.UserRole = Role.Admin.ToString();
                ViewBag.IdeaCarrierUserName = startupProject.User.UserName;
                if (!string.IsNullOrEmpty(startupProject.ApprovedByID))
                {
                    if (User.Identity.GetUserId() == startupProject.ApprovedByID) ViewBag.ApprovedBy = "You self";
                    else ViewBag.ApprovedBy = db.Users.Where(u => u.Id == startupProject.ApprovedByID).FirstOrDefault().UserName;
                }

                return View(startupProject);
            }

            return View(startupProject);
        }

        private bool IsFormFinished(Models.IdeaCarrier.Startup startupProject)
        {
            return
                //Project
                !string.IsNullOrEmpty(startupProject.StartupName) &&
                startupProject.ProjectDomainID.HasValue &&
                startupProject.DeadlineDate.HasValue &&
                !string.IsNullOrEmpty(startupProject.ProjectSummary) &&
                (startupProject.AllowedInvestors != null && startupProject.AllowedInvestors.Any()) &&
                //startupProject.AllowSharing.HasValue &&
                //Funding
                startupProject.FundingPhaseID.HasValue &&
                startupProject.FundingAmountID.HasValue &&
                //startupProject.FutureFundingNeeded.HasValue &&
                startupProject.AlreadySpentTime.HasValue &&
                startupProject.AlreadySpentMoney.HasValue &&
                //startupProject.WillSpendOwnMoney.HasValue &&
                //Budget
                (startupProject.ProjectFundingDivisions != null && startupProject.ProjectFundingDivisions.Sum(pfd => pfd.Percentage) == 100) &&
                startupProject.EstimatedExitPlanID.HasValue &&
                startupProject.EstimatedBreakEven.HasValue &&
                startupProject.PossibleIncomeStreams.HasValue &&
                //startupProject.HavePayingCustomers.HasValue &&
                //Team
                startupProject.TeamMemberSize.HasValue &&
                startupProject.TeamExperience.HasValue &&
                //startupProject.TeamVisionShared.HasValue &&
                //startupProject.HaveFixedRoles.HasValue &&
                (startupProject.TeamWeaknesses != null && startupProject.TeamWeaknesses.Any()) &&
                //startupProject.LookingForActiveInvestors.HasValue &&
                //Outcome
                (startupProject.Outcomes != null && startupProject.Outcomes.Any()) &&
                startupProject.InnovationLevelID.HasValue &&
                startupProject.ScalabilityID.HasValue;
        }

        // GET: Startups/Create
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult AddNewProject()
        {
            AddNewProjectViewModel newProjectmodel = new AddNewProjectViewModel()
            {
                StartupName = "",
                IdeaCarrierMessage = db.IdeaCarrierMessages.FirstOrDefault().Text,
                CountryList = new SelectList(db.Countries, "CountryID", "CountryName"),
                SwedishCountryID = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single(),
                SwedishRegionList = new SelectList(db.SwedishRegions, "RegionID", "RegionName")
            };
            
            return  View(newProjectmodel);
        }

        // POST: Startups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult AddNewProject(AddNewProjectViewModel newProjectModel, string submit_command)
        {
            if (ModelState.IsValid)
            {
                string countryAbbreviation = db.Countries.Find(newProjectModel.CountryID).CountryAbbreviation;
                string newStartupProjectID = "IC" + countryAbbreviation + HelpFunctions.GetShortCode();
                //check if the code is already exist
                while (db.Startups.Any(u => u.StartupID == newStartupProjectID))
                {
                    newStartupProjectID = "IC" + countryAbbreviation + HelpFunctions.GetShortCode();
                }

                DateTime currentDate = DateTime.Now.Date;

                Models.IdeaCarrier.Startup newStartupProject = new Models.IdeaCarrier.Startup()
                {
                    StartupName = newProjectModel.StartupName,
                    StartupID = newStartupProjectID,
                    UserID = User.Identity.GetUserId(),
                    CountryID = newProjectModel.CountryID,
                    SwedishRegionID = newProjectModel.SwedishRegionID,
                    ProjectSummary = "",
                    CreatedDate = currentDate,
                    LastSavedDate = currentDate
                    //Locked = false
                };

                List<FundingDivision> fundingDivisions = db.FundingDivisions.ToList();
                foreach (FundingDivision fundingDivision in fundingDivisions)
                {
                    db.FundingDivisionStartups.Add(new FundingDivisionStartup //db.ProjectFundingDivisions is a better name
                    {
                         FundingDivisionID = fundingDivision.FundingDivisionID, //fundingDivision.FundingDivisionName is better, makes the code simpler
                         Percentage = 0,
                         StartupID = newStartupProjectID
                    });
                }

                db.Startups.Add(newStartupProject);
                db.SaveChanges();

                if (!string.IsNullOrEmpty(submit_command) && submit_command.StartsWith("Proceed")) //"Proceed to the Project form"
                {
                    /*...*/
                    return RedirectToAction("ProjectForm", new { id = newStartupProjectID });
                }
                else return RedirectToAction("Index");
            }

            newProjectModel.StartupName = "";
            newProjectModel.IdeaCarrierMessage = db.IdeaCarrierMessages.FirstOrDefault().Text;
            newProjectModel.CountryList = new SelectList(db.Countries, "CountryID", "CountryName");
            newProjectModel.SwedishCountryID = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();
            newProjectModel.SwedishRegionList = new SelectList(db.SwedishRegions, "RegionID", "RegionName");

            return View(newProjectModel);
        }

        // GET: Startups/Edit/5
        //[HttpGet]
        [Authorize(Roles = "Admin, IdeaCarrier")]
        public ActionResult ProjectForm(string id)
        {
            if (User.IsInRole(Role.Admin.ToString())) return RedirectToAction("EditAdmin", new { id });

            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(id); //startupProject
            if (startupProject == null)
            {
                return HttpNotFound();
            }

            if (startupProject.Locked) return RedirectToAction("ProjectDetails", new { id });

            string message = "";
            string unansweredQuestion = "";
            /*...*/

            if (TempData.Any())
            {
                if (TempData.ContainsKey("message")) message = TempData["message"] as string;
                if (TempData.ContainsKey("unanswered")) unansweredQuestion = TempData["unanswered"] as string;
                /*...*/
                TempData.Clear();
            }

            return View(GetStartupProjectViewModel(startupProject, false, message, unansweredQuestion));
        }

        // POST: Startups/ProjectForm/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult ProjectForm(StartupProjectPostViewModel projectPostModel)
        {
            bool updated = true;

            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(projectPostModel.StartupID);

            if (ModelState.IsValid)
            {
                if (UpdateActiveTab(startupProject, projectPostModel))
                {
                    startupProject.LastSavedDate = DateTime.Now.Date;
                    db.Entry(startupProject).State = EntityState.Modified;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex) //invalid data
                    {
                        ViewBag.Message = ex.Message;
                        updated = false;
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Validation Error !!");
                updated = false;
            }

            return View(GetStartupProjectViewModel(startupProject, updated));
        }

        private StartupProjectViewModel GetStartupProjectViewModel(Models.IdeaCarrier.Startup startupProject, bool updated = false, string message = "", string unansweredQuestion = "")
        {
            return new StartupProjectViewModel()
            {
                StartupID = startupProject.StartupID,

                //Project
                ProjectName = startupProject.StartupName,
                ProjectDomainID = startupProject.ProjectDomainID,
                ProjectDomainList = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName"),
                DeadlineDate = startupProject.DeadlineDate,
                ProjectSummary = startupProject.ProjectSummary,
                AllowSharing_DisplayName = db.IdeaCarrierMessages.FirstOrDefault().AllowSharing_DisplayName,
                AllowSharing = startupProject.AllowSharing, //!isNewProject ? startupProject.AllowSharing : null,
                AllowedInvestors = GetAllowedInvestors(startupProject),

                //Funding
                FundingPhaseID = startupProject.FundingPhaseID,
                FundingPhaseList = new SelectList(db.FundingPhases, "FundingPhaseID", "FundingPhaseName"),
                FundingAmountID = startupProject.FundingAmountID,
                FundingAmountList = new SelectList(db.FundingAmounts, "FundingAmountID", "FundingAmountValue"),
                FutureFundingNeeded = startupProject.FutureFundingNeeded,
                AlreadySpentTime = startupProject.AlreadySpentTime,
                AlreadySpentMoney = startupProject.AlreadySpentMoney,
                WillSpendOwnMoney = startupProject.WillSpendOwnMoney, 

                //Budget
                FundingDivisions = startupProject.ProjectFundingDivisions.Distinct().ToList(),
                EstimatedExitPlanID = startupProject.EstimatedExitPlanID,
                EstimatedExitPlanList = new SelectList(db.EstimatedExitPlans, "EstimatedExitPlanID", "EstimatedExitPlanName"),
                EstimatedBreakEven = startupProject.EstimatedBreakEven,
                PossibleIncomeStreams = startupProject.PossibleIncomeStreams,
                HavePayingCustomers = startupProject.HavePayingCustomers,

                //Team
                TeamMemberSize = startupProject.TeamMemberSize,
                TeamExperience = startupProject.TeamExperience,
                TeamVisionShared = startupProject.TeamVisionShared,
                HaveFixedRoles = startupProject.HaveFixedRoles,
                TeamWeaknesses = GetTeamWeaknesses(startupProject),
                LookingForActiveInvestors = startupProject.LookingForActiveInvestors,

                //Outcome
                Outcomes = GetOutcomes(startupProject),
                InnovationLevelID = startupProject.InnovationLevelID,
                InnovationLevelList = new SelectList(db.InnovationLevels, "InnovationLevelID", "InnovationLevelName"),
                ScalabilityID = startupProject.ScalabilityID,
                ScalabilityList = new SelectList(db.Scalabilities, "ScalabilityID", "ScalabilityName"),

                Updated = updated,

                Message = message,
                UnansweredQuestion = unansweredQuestion
            };
        }

        private List<AllowedInvestorViewModel> GetAllowedInvestors(Models.IdeaCarrier.Startup startupProject)
        {
            List<AllowedInvestorViewModel> allowedInvestorViewModels = new List<AllowedInvestorViewModel>();

            List<int> projectAllowedInvestorIDList = startupProject.AllowedInvestors.Select(ai => ai.AllowedInvestorID).ToList();

            List<AllowedInvestor> allowedInvestors = db.AllowedInvestors.ToList();
            foreach (var allowedInvestor in allowedInvestors)
            {
                allowedInvestorViewModels.Add(new AllowedInvestorViewModel
                {
                    AllowedInvestorID = allowedInvestor.AllowedInvestorID,
                    AllowedInvestorName = allowedInvestor.AllowedInvestorName,
                    Assigned = projectAllowedInvestorIDList.Any() ? projectAllowedInvestorIDList.Contains(allowedInvestor.AllowedInvestorID) : false
                });
            }

            return allowedInvestorViewModels;
        }

        private List<TeamWeaknessViewModel> GetTeamWeaknesses(Models.IdeaCarrier.Startup startupProject)
        {
            List<TeamWeaknessViewModel> teamWeaknessViewModels = new List<TeamWeaknessViewModel>();

            List<int> projectTeamWeaknessIDList = startupProject.TeamWeaknesses.Select(tw => tw.TeamWeaknessID).ToList();

            List<TeamWeakness> teamWeaknesses = db.TeamWeaknesses.ToList();
            foreach (var teamWeakness in teamWeaknesses)
            {
                teamWeaknessViewModels.Add(new TeamWeaknessViewModel
                {
                    WeaknessID = teamWeakness.TeamWeaknessID,
                    WeaknessName = teamWeakness.TeamWeaknessName,
                    Assigned = projectTeamWeaknessIDList.Any() ? projectTeamWeaknessIDList.Contains(teamWeakness.TeamWeaknessID) : false
                });
            }

            return teamWeaknessViewModels;
        }

        private List<StartupOutcomeViewModel> GetOutcomes(Models.IdeaCarrier.Startup startupProject)
        {
            List<StartupOutcomeViewModel> outcomeViewModels = new List<StartupOutcomeViewModel>();

            List<int> projectOutcomeIDList = startupProject.Outcomes.Select(o => o.OutcomeID).ToList();

                List<Outcome> outcomes = db.Outcomes.ToList();
                foreach (var outcome in outcomes)
                {
                    outcomeViewModels.Add(new StartupOutcomeViewModel
                    {
                        OutcomeID = outcome.OutcomeID,
                        OutcomeName = outcome.OutcomeName,
                        Assigned = projectOutcomeIDList.Any() ? projectOutcomeIDList.Contains(outcome.OutcomeID) : false
                    });
                }
            //}

            return outcomeViewModels;
        }

        private bool UpdateActiveTab(Models.IdeaCarrier.Startup startupProject, StartupProjectPostViewModel projectPostModel)
        {
            bool updated = false;

            switch (projectPostModel.ActiveTab)
            {
                case "tab_Project": updated = UpdateTabProject(startupProject, projectPostModel); break;
                case "tab_Funding": updated = UpdateTabFunding(startupProject, projectPostModel); break;
                case "tab_Budget": updated = UpdateTabBudget(startupProject, projectPostModel); break;
                case "tab_Team": updated = UpdateTabTeam(startupProject, projectPostModel); break;
                case "tab_Outcome": updated = UpdateTabOutcome(startupProject, projectPostModel); break;
                default: break;
            }

            return updated;
        }

        private bool UpdateTabProject(Models.IdeaCarrier.Startup startupProject, StartupProjectPostViewModel projectPostModel)
        {
            bool updated = false;

            if (startupProject.StartupName != projectPostModel.ProjectName)
            {
                startupProject.StartupName = projectPostModel.ProjectName;
                updated = true;
            }
            
            if (startupProject.ProjectDomainID != projectPostModel.ProjectDomainID)
            {
                startupProject.ProjectDomainID = projectPostModel.ProjectDomainID;
                updated = true;
            }
            
            if (startupProject.DeadlineDate != projectPostModel.DeadlineDate)
            {
                startupProject.DeadlineDate = projectPostModel.DeadlineDate;
                updated = true;
            }
            
            if (startupProject.ProjectSummary != projectPostModel.ProjectSummary)
            {
                startupProject.ProjectSummary = projectPostModel.ProjectSummary;
                updated = true;
            }

            if (!string.IsNullOrEmpty(projectPostModel.AllowSharing))
            {
                bool postedAllowSharing = projectPostModel.AllowSharing.ToUpper() == "YES";
                if (postedAllowSharing && !startupProject.AllowSharing)
                {
                    startupProject.AllowSharing = true;
                    updated = true;
                }
                else if (!postedAllowSharing && startupProject.AllowSharing)
                {
                    startupProject.AllowSharing = false;
                    updated = true;
                }
            }
            else if (startupProject.AllowSharing) //!profilePostModel.AllowSharing.HasValue (bool?)
            {
                startupProject.AllowSharing = false; //profilePostModel.AllowSharing = null;
                updated = true;
            }

            if (UpdateAllowedToInvestors(projectPostModel.AllowedInvestorIDs, startupProject) && !updated) updated = true;

            return updated;
        }

        private bool UpdateAllowedToInvestors(string[] postedAllowedInvestorIDs, Models.IdeaCarrier.Startup startupProject)
        {
            bool updated = false;

            if (postedAllowedInvestorIDs != null)
            {
                List<int> postedAllowedInvestorIDList = Array.ConvertAll(postedAllowedInvestorIDs, s => int.Parse(s)).ToList();

                List<AllowedInvestor> projectAllowedInvestors = startupProject.AllowedInvestors.ToList();
                foreach (AllowedInvestor projectAllowedInvestor in projectAllowedInvestors)
                {
                    if (!postedAllowedInvestorIDList.Contains(projectAllowedInvestor.AllowedInvestorID))
                    {
                        startupProject.AllowedInvestors.Remove(projectAllowedInvestor);
                        updated = true;
                    }
                }

                List<AllowedInvestor> allowedInvestors = db.AllowedInvestors.ToList();
                AllowedInvestor allowedInvestor = null;
                foreach (int postedAllowedInvestorID in postedAllowedInvestorIDList)
                {
                    allowedInvestor = allowedInvestors.Where(ai => ai.AllowedInvestorID == postedAllowedInvestorID).FirstOrDefault();

                    if (!startupProject.AllowedInvestors.Contains(allowedInvestor))
                    {
                        startupProject.AllowedInvestors.Add(allowedInvestor);
                        updated = true;
                    }
                }
            }
            else //if (postedAllowedInvestorIDs == null)
            {
                startupProject.AllowedInvestors.Clear();
                updated = true;
            }

            return updated;
        }

        private bool UpdateTabFunding(Models.IdeaCarrier.Startup startupProject, StartupProjectPostViewModel projectPostModel)
        {
            bool updated = false;

            if (startupProject.FundingPhaseID != projectPostModel.FundingPhaseID)
            {
                startupProject.FundingPhaseID = projectPostModel.FundingPhaseID;
                updated = true;
            }

            if (startupProject.FundingAmountID != projectPostModel.FundingAmountID)
            {
                startupProject.FundingAmountID = projectPostModel.FundingAmountID;
                updated = true;
            }

            if (!string.IsNullOrEmpty(projectPostModel.FutureFundingNeeded))
            {
                bool postedFutureFundingNeeded = projectPostModel.FutureFundingNeeded.ToUpper() == "YES";
                if (postedFutureFundingNeeded && !startupProject.FutureFundingNeeded)
                {
                    startupProject.FutureFundingNeeded = true;
                    updated = true;
                }
                else if (!postedFutureFundingNeeded && startupProject.FutureFundingNeeded)
                {
                    startupProject.FutureFundingNeeded = false;
                    updated = true;
                }
            }
            else if (startupProject.FutureFundingNeeded) 
            {
                startupProject.FutureFundingNeeded = false;
                updated = true;
            }

            if (startupProject.AlreadySpentTime != projectPostModel.AlreadySpentTime)
            {
                startupProject.AlreadySpentTime = projectPostModel.AlreadySpentTime;
                updated = true;
            }

            if (startupProject.AlreadySpentMoney != projectPostModel.AlreadySpentMoney)
            {
                startupProject.AlreadySpentMoney = projectPostModel.AlreadySpentMoney;
                updated = true;
            }

            if (!string.IsNullOrEmpty(projectPostModel.WillSpendOwnMoney))
            {
                bool postedWillSpendOwnMoney = projectPostModel.WillSpendOwnMoney.ToUpper() == "YES";
                if (postedWillSpendOwnMoney && !startupProject.WillSpendOwnMoney)
                {
                    startupProject.WillSpendOwnMoney = true;
                    updated = true;
                }
                else if (!postedWillSpendOwnMoney && startupProject.WillSpendOwnMoney)
                {
                    startupProject.WillSpendOwnMoney = false;
                    updated = true;
                }
            }
            else if (startupProject.WillSpendOwnMoney)
            {
                startupProject.WillSpendOwnMoney = false;
                updated = true;
            }

            return updated;
        }

        private bool UpdateTabBudget(Models.IdeaCarrier.Startup startupProject, StartupProjectPostViewModel projectPostModel)
        {
            bool updated = false;

            updated = UpdateFundingDivisionPercentages(projectPostModel.FundingDivisionPercentages, startupProject);

            if (startupProject.EstimatedExitPlanID != projectPostModel.EstimatedExitPlanID)
            {
                startupProject.EstimatedExitPlanID = projectPostModel.EstimatedExitPlanID;
                updated = true;
            }

            if (startupProject.EstimatedBreakEven != projectPostModel.EstimatedBreakEven)
            {
                startupProject.EstimatedBreakEven = projectPostModel.EstimatedBreakEven;
                updated = true;
            }

            if (startupProject.PossibleIncomeStreams != projectPostModel.PossibleIncomeStreams)
            {
                startupProject.PossibleIncomeStreams = projectPostModel.PossibleIncomeStreams;
                updated = true;
            }

            if (!string.IsNullOrEmpty(projectPostModel.HavePayingCustomers))
            {
                bool postedHavePayingCustomers = projectPostModel.HavePayingCustomers.ToUpper() == "YES";
                if (postedHavePayingCustomers && !startupProject.HavePayingCustomers)
                {
                    startupProject.HavePayingCustomers = true;
                    updated = true;
                }
                else if (!postedHavePayingCustomers && startupProject.HavePayingCustomers)
                {
                    startupProject.HavePayingCustomers = false;
                    updated = true;
                }
            }
            else if (startupProject.HavePayingCustomers)
            {
                startupProject.HavePayingCustomers = false;
                updated = true;
            }

            return updated;
        }

        private bool UpdateFundingDivisionPercentages(string[] fundingDivisionPercentages, Models.IdeaCarrier.Startup startupProject)
        {
            bool updated = false;
            
            if (fundingDivisionPercentages != null)
            {
                List<int> fundingDivisionPercentageList = new List<int>();

                int fundingDivisionPercentage = 0;
                foreach (string fundingDivisionPercentageString in fundingDivisionPercentages)
                {
                    if (Int32.TryParse(fundingDivisionPercentageString, out fundingDivisionPercentage))
                    {
                        if (fundingDivisionPercentage < 0) fundingDivisionPercentageList.Add(0);
                        else fundingDivisionPercentageList.Add(fundingDivisionPercentage);
                    }
                    else fundingDivisionPercentageList.Add(0);
                }

                if (fundingDivisionPercentageList.Sum() <= 100)
                {
                    for (int i = 0; i < fundingDivisionPercentageList.Count(); i++)
                    {
                        if (startupProject.ProjectFundingDivisions.ElementAt(i).Percentage != fundingDivisionPercentageList.ElementAt(i))
                        {
                            startupProject.ProjectFundingDivisions.ElementAt(i).Percentage = fundingDivisionPercentageList.ElementAt(i);
                            //db.Entry(startupFundingDivisionList.ElementAt(i)).State = EntityState.Modified; //not necessary
                            updated = true;
                        }
                    }
                } 
                
                /*...*/
            }
            else //if (fundingDivisionPercentages == null)
            {
                foreach (FundingDivisionStartup fundingDivision in startupProject.ProjectFundingDivisions) fundingDivision.Percentage = 0;
                updated = true;
            }

            return updated;
        }

        private bool UpdateTabTeam(Models.IdeaCarrier.Startup startupProject, StartupProjectPostViewModel projectPostModel)
        {
            bool updated = false;

            if (startupProject.TeamMemberSize != projectPostModel.TeamMemberSize)
            {
                startupProject.TeamMemberSize = projectPostModel.TeamMemberSize;
                updated = true;
            }

            if (startupProject.TeamExperience != projectPostModel.TeamExperience)
            {
                startupProject.TeamExperience = projectPostModel.TeamExperience;
                updated = true;
            }

            if (!string.IsNullOrEmpty(projectPostModel.TeamVisionShared))
            {
                bool postedTeamVisionShared = projectPostModel.TeamVisionShared.ToUpper() == "YES";
                if (postedTeamVisionShared && !startupProject.TeamVisionShared)
                {
                    startupProject.TeamVisionShared = true;
                    updated = true;
                }
                else if (!postedTeamVisionShared && startupProject.TeamVisionShared) 
                {
                    startupProject.TeamVisionShared = false;
                    updated = true;
                }
            }
            else if (startupProject.TeamVisionShared) //!profilePostModel.TeamVisionShared.HasValue (bool?)
            {
                startupProject.TeamVisionShared = false; //profilePostModel.TeamVisionShared = null;
                updated = true;
            }

            if (!string.IsNullOrEmpty(projectPostModel.HaveFixedRoles))
            {
                bool postedTeamVisionShared = projectPostModel.HaveFixedRoles.ToUpper() == "YES";
                if (postedTeamVisionShared && !startupProject.HaveFixedRoles)
                {
                    startupProject.HaveFixedRoles= true;
                    updated = true;
                }
                else if (!postedTeamVisionShared && startupProject.HaveFixedRoles)
                {
                    startupProject.HaveFixedRoles = false;
                    updated = true;
                }
            }
            else if (startupProject.HaveFixedRoles)
            {
                startupProject.HaveFixedRoles = false;
                updated = true;
            }

            if (UpdateTeamWeaknesses(projectPostModel.TeamWeaknessIDs, startupProject) && !updated) updated = true;

            if (!string.IsNullOrEmpty(projectPostModel.LookingForActiveInvestors))
            {
                bool postedLookingForActiveInvestors = projectPostModel.LookingForActiveInvestors.ToUpper() == "YES";
                if (postedLookingForActiveInvestors && !startupProject.LookingForActiveInvestors)
                {
                    startupProject.LookingForActiveInvestors = true;
                    updated = true;
                }
                else if (!postedLookingForActiveInvestors && startupProject.LookingForActiveInvestors)
                {
                    startupProject.LookingForActiveInvestors = false;
                    updated = true;
                }
            }
            else if (startupProject.LookingForActiveInvestors)
            {
                startupProject.LookingForActiveInvestors = false;
                updated = true;
            }

            return updated;
        }

        private bool UpdateTeamWeaknesses(string[] postedTeamWeaknessIDs, Models.IdeaCarrier.Startup startupProject)
        {
            bool updated = false;

            if (postedTeamWeaknessIDs != null)
            {
                List<int> postedTeamWeaknessIDList = Array.ConvertAll(postedTeamWeaknessIDs, s => int.Parse(s)).ToList();

                List<TeamWeakness> projectTeamWeaknesses = startupProject.TeamWeaknesses.ToList();
                foreach (TeamWeakness projectTeamWeakness in projectTeamWeaknesses)
                {
                    if (!postedTeamWeaknessIDList.Contains(projectTeamWeakness.TeamWeaknessID))
                    {
                        startupProject.TeamWeaknesses.Remove(projectTeamWeakness);
                        updated = true;
                    }
                }

                List<TeamWeakness> teamWeaknesses = db.TeamWeaknesses.ToList();
                TeamWeakness teamWeakness = null;
                foreach (int postedTeamWeaknessID in postedTeamWeaknessIDList)
                {
                    teamWeakness = teamWeaknesses.Where(tw => tw.TeamWeaknessID == postedTeamWeaknessID).FirstOrDefault();

                    if (!startupProject.TeamWeaknesses.Contains(teamWeakness))
                    {
                        startupProject.TeamWeaknesses.Add(teamWeakness);
                        updated = true;
                    }
                }
            }
            else //if (postedTeamWeaknessIDs == null)
            {
                startupProject.TeamWeaknesses.Clear();
                updated = true;
            }

            return updated;
        }

        private bool UpdateTabOutcome(Models.IdeaCarrier.Startup startupProject, StartupProjectPostViewModel projectPostModel)
        {
            bool updated = false;

            updated = UpdateOutcomes(projectPostModel.OutcomeIDs, startupProject);

            if (startupProject.InnovationLevelID != projectPostModel.InnovationLevelID)
            {
                startupProject.InnovationLevelID = projectPostModel.InnovationLevelID;
                updated = true;
            }

            if (startupProject.ScalabilityID != projectPostModel.ScalabilityID)
            {
                startupProject.ScalabilityID = projectPostModel.ScalabilityID;
                updated = true;
            }

            return updated;
        }

        //Outcomes
        private bool UpdateOutcomes(string[] postedOutcomeIDs, Models.IdeaCarrier.Startup startupProject)
        {
            bool updated = false;

            if (postedOutcomeIDs != null)
            {
                List<int> postedOutcomeIDList = Array.ConvertAll(postedOutcomeIDs, s => int.Parse(s)).ToList();

                List<Outcome> projectOutcomes = startupProject.Outcomes.ToList();
                foreach (Outcome projectOutcome in projectOutcomes)
                {
                    if (!postedOutcomeIDList.Contains(projectOutcome.OutcomeID))
                    {
                        startupProject.Outcomes.Remove(projectOutcome);
                        updated = true;
                    }
                }

                List<Outcome> outcomes = db.Outcomes.ToList();
                Outcome outcome = null;
                foreach (int postedOutcomeID in postedOutcomeIDList)
                {
                    outcome = outcomes.Where(o => o.OutcomeID == postedOutcomeID).FirstOrDefault();

                    if (!startupProject.Outcomes.Contains(outcome))
                    {
                        startupProject.Outcomes.Add(outcome);
                        updated = true;
                    }
                }
            }
            else //if (postedOutcomeIDs == null)
            {
                startupProject.Outcomes.Clear();
                updated = true;
            }

            return updated;
        }

        //[HttpGet]
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult SubmitProjectForm(string id, bool cancel = false, string redirect = "", string redirectTab = "")
        {
            if (cancel)
            {
                if (string.IsNullOrEmpty(redirectTab)) return RedirectToAction("ProjectForm", new { id });
                else return Redirect(Url.Action("ProjectForm", new { id }) + "#" + redirectTab);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(id);

            if (startupProject == null)
            {
                return HttpNotFound();
            }

            //Project
            if (string.IsNullOrEmpty(startupProject.StartupName))
            {
                TempData["message"] = "Project name can't be empty!";
                TempData["unanswered"] = "ProjectName";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Project");
            }
            if (!startupProject.ProjectDomainID.HasValue)
            {
                TempData["message"] = "Select the Project domain!";
                TempData["unanswered"] = "ProjectDomain";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Project");
            }
            if (!startupProject.DeadlineDate.HasValue)
            {
                TempData["message"] = "Dead line date has to be set!";
                TempData["unanswered"] = "DeadlineDate";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Project");
            }
            if (string.IsNullOrEmpty(startupProject.ProjectSummary))
            {
                TempData["message"] = "Project summary can't be empty!";
                TempData["unanswered"] = "ProjectSummary";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Project");
            }
            if (startupProject.AllowedInvestors == null || !startupProject.AllowedInvestors.Any())
            {
                TempData["message"] = "Select at least one Allowed investor!";
                TempData["unanswered"] = "AllowedInvestors";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Project");
            }
            //Funding
            if (!startupProject.FundingPhaseID.HasValue)
            {
                TempData["message"] = "Select the Funding phase!";
                TempData["unanswered"] = "FundingPhase";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Funding");
            }
            if (!startupProject.FundingAmountID.HasValue)
            {
                TempData["message"] = "Select the Funding amount!";
                TempData["unanswered"] = "FundingAmount";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Funding");
            }
            if (!startupProject.AlreadySpentTime.HasValue)
            {
                TempData["message"] = "Answer the question \"Alreday spent time\"!";
                TempData["unanswered"] = "AlreadySpentTime";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Funding");
            }
            if (!startupProject.AlreadySpentMoney.HasValue)
            {
                TempData["message"] = "Answer the question \"Alreday spent money\"!";
                TempData["unanswered"] = "AlreadySpentMoney";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Funding");
            }
            //Budget
            if (startupProject.ProjectFundingDivisions != null && startupProject.ProjectFundingDivisions.Sum(pfd => pfd.Percentage) != 100)
            {
                TempData["message"] = "In the question \"How will the funding be spent?\" the sum of the percentages must 100!"; //Max 100%
                TempData["unanswered"] = "FundingDivisions";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Budget");
            }
            if (!startupProject.EstimatedExitPlanID.HasValue)
            {
                TempData["message"] = "Select the EstimatedExitPlan!";
                TempData["unanswered"] = "EstimatedExitPlan";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Budget");
            }
            if (!startupProject.EstimatedBreakEven.HasValue)
            {
                TempData["message"] = "Answer the question \"Estimated break even\"!";
                TempData["unanswered"] = "EstimatedBreakEven";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Budget");
            }
            if (!startupProject.PossibleIncomeStreams.HasValue)
            {
                TempData["message"] = "Answer the question \"Possible income streams\"!";
                TempData["unanswered"] = "PossibleIncomeStreams";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Budget");
            }
            //Team
            if (!startupProject.TeamMemberSize.HasValue)
            {
                TempData["message"] = "Answer the question \"Team member size\"!";
                TempData["unanswered"] = "TeamMemberSize";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Team");
            }
            if (!startupProject.TeamExperience.HasValue)
            {
                TempData["message"] = "Answer the question \"Team experience\"!";
                TempData["unanswered"] = "TeamExperience";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Team");
            }
            if (startupProject.TeamWeaknesses == null || !startupProject.TeamWeaknesses.Any())
            {
                TempData["message"] = "Select the Team weaknesses!";
                TempData["unanswered"] = "TeamWeaknesses";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Team");
            }
            //Outcome
            if (startupProject.Outcomes == null || !startupProject.Outcomes.Any())
            {
                TempData["message"] = "Select at least one Outcome!";
                TempData["unanswered"] = "Outcomes";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Outcome");
            }
            if (!startupProject.InnovationLevelID.HasValue)
            {
                TempData["message"] = "Select the Level of innovation!";
                TempData["unanswered"] = "InnovationLevel";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Outcome");
            }
            if (!startupProject.ScalabilityID.HasValue)
            {
                TempData["message"] = "Select the Scalability!";
                TempData["unanswered"] = "Scalability";
                return Redirect(Url.Action("ProjectForm", new { id }) + "#Outcome");
            }

            startupProject.Locked = true;
            startupProject.LastLockedDate = DateTime.Now.Date;

            db.Entry(startupProject).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect)) return RedirectToAction(redirect, new { id });

            return RedirectToAction("ProjectDetails", new { id });
        }

        // GET: Startups/EditAdmin/5
        //[HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditAdmin(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(id);
            if (startupProject == null)
            {
                return HttpNotFound();
            }
            
            StartupEditAdminViewModel model = new StartupEditAdminViewModel
            {
                StartupID = startupProject.StartupID,
                IdeaCarrierUserID = startupProject.UserID,
                IdeaCarrierUserName = startupProject.User.UserName,
                ProjectSummary = startupProject.ProjectSummary,
                Locked = startupProject.Locked
            };

            return View(model);
        }

        // POST: Startups/EditAdmin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        [Authorize(Roles = "Admin")] /*[Bind(Include = "StartupID,ProjectSummary,Locked,Approved")]*/
        public ActionResult EditAdmin(StartupEditAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                Models.IdeaCarrier.Startup startupProject = db.Startups.Find(model.StartupID);
                if (startupProject == null)
                {
                    return HttpNotFound();
                }

                bool updated = false;

                if (startupProject.ProjectSummary != model.ProjectSummary)
                {
                    startupProject.ProjectSummary = model.ProjectSummary;
                    updated = true;
                }

                if (startupProject.Locked != model.Locked)
                {
                    startupProject.Locked = model.Locked;
                    updated = true;
                }

                if (updated)
                {
                    db.Entry(startupProject).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return RedirectToAction("ProjectDetails", new { id = startupProject.StartupID });
            }
            return View(model);
        }

        // GET: Startups/RemoveProject/5
        [Authorize(Roles = "IdeaCarrier")] //Admin, 
        //[HttpDelete]
        public ActionResult RemoveProject(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(id);
            if (startupProject == null)
            {
                return HttpNotFound();
            }

            return View(startupProject);
        }

        // POST: Startups/RemoveProject/5
        [Authorize(Roles = "IdeaCarrier")] //Admin, 
        [HttpPost, ActionName("RemoveProject")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {   
            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(id);

            List<FundingDivisionStartup> projectFundingDivisions = startupProject.ProjectFundingDivisions.ToList();
            foreach (FundingDivisionStartup projectFundingDivision in projectFundingDivisions)
            {
                db.FundingDivisionStartups.Remove(projectFundingDivision);
            }

            db.Startups.Remove(startupProject);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        /*...*/

        [Authorize(Roles = "Admin")]
        public ActionResult ChangeApproval(string id, string redirect = "")
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.IdeaCarrier.Startup startup = db.Startups.Find(id);

            if (startup == null)
            {
                return HttpNotFound();
            }

            startup.Approved = !(startup.Approved);
            if (startup.Approved) startup.ApprovedByID = User.Identity.GetUserId();
            else startup.ApprovedByID = "";

            db.Entry(startup).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect) && redirect.ToUpper() != "INDEX")
            {
                if (redirect.ToUpper().Contains("DETAILS")) return RedirectToAction("ProjectDetails", new { id });
                else return RedirectToAction(redirect, new { id });
            }

            if (string.IsNullOrWhiteSpace(redirect) || redirect.ToUpper() == "INDEX") return RedirectToAction("Index", new { id });

            return RedirectToAction("ProjectDetails", new { id });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Unlock(string id, string redirect = "", bool isSingleUser = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(id);

            if (startupProject == null)
            {
                return HttpNotFound();
            }
            
            startupProject.Locked = false;
            startupProject.Approved = false;
            startupProject.ApprovedByID = "";

            List<MatchMaking> startupProjectMatchMakings = startupProject.MatchMakings.ToList();
            foreach (MatchMaking startupProjectMatchMaking in startupProjectMatchMakings)
                db.MatchMakings.Remove(startupProjectMatchMaking);

            db.Entry(startupProject).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect) && redirect.ToUpper() != "INDEX")
            {
                if (redirect.ToUpper().Contains("DETAILS")) return RedirectToAction("ProjectDetails", new { id });
                else return RedirectToAction(redirect, new { id });
            }

            if (string.IsNullOrWhiteSpace(redirect) || redirect.ToUpper() == "INDEX")
                return RedirectToAction("Index", new { id = (isSingleUser ? startupProject.UserID : "") });

            return RedirectToAction("ProjectDetails", new { id });
        }
        
        /*...*/
        
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult ProlongDeadlineDate(string id, int months = 6, string redirect = "")
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(id);

            if (startupProject == null)
            {
                return HttpNotFound();
            }
            //try
            startupProject.DeadlineDate = DateTime.Now.AddMonths(months);

            db.Entry(startupProject).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect) && redirect.ToUpper() != "INDEX")
            {
                if (redirect.ToUpper().Contains("DETAILS")) return RedirectToAction("ProjectDetails", new { id });
                else return RedirectToAction(redirect, new { id });
            }

            if (string.IsNullOrWhiteSpace(redirect) || redirect.ToUpper() == "INDEX")
                return RedirectToAction("Index", new { id = startupProject.UserID });

            return RedirectToAction("ProjectDetails", new { id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
