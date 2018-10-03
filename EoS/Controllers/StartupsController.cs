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
using System.Threading.Tasks;
using System.Net.Mail;

namespace EoS.Controllers
{
    //[Authorize]
    public class StartupsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Startups
        [Authorize(Roles = "Admin, IdeaCarrier")]
        public ActionResult Index(string id, bool? matchable, string orderBy) //id==ideaCarrierId
        {
            List<Models.IdeaCarrier.Startup> startupProjects = null;

            ViewBag.Matchable = null; //false;
            ViewBag.UserRole = "";
            ViewBag.IdeaCarrierId = "";
            ViewBag.IdeaCarrierUserName = "";

            if (User.IsInRole(Role.Admin.ToString()))
            {
                ViewBag.UserRole = Role.Admin.ToString();

                if (!string.IsNullOrEmpty(id)) //The Admin looks at a special IdeaCarrier's startup projects
                {
                    ApplicationUser ideaCarrier = db.Users.Find(id);
                    if (ideaCarrier != null) //<--------implement Role check?
                    {
                        ViewBag.IdeaCarrierId = id;
                        ViewBag.IdeaCarrierUserName = ideaCarrier.UserName;

                        if (!matchable.HasValue)
                        {
                            startupProjects = ideaCarrier.Startups.ToList(); //db.Startups.Where(i => i.UserID == id).ToList();
                        }
                        else if (matchable.Value)
                        {
                            ViewBag.Matchable = true;
                            //startupProjects = db.Startups.Where(s => s.UserID == id && s.Locked && s.Approved && (!s.DeadlineDate.HasValue || s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).OrderBy(s => s.StartupID).ToList();
                            startupProjects = ideaCarrier.Startups.Where(s => s.Locked && s.Approved && (!s.DeadlineDate.HasValue || s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).OrderBy(s => s.StartupID).ToList();
                        }
                        else
                        {
                            ViewBag.Matchable = false;
                            //startupProjects = db.Startups.Where(s => s.UserID == id && (!s.Locked || !s.Approved || (s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) <= 0))).OrderBy(s => s.StartupID).ToList();
                            startupProjects = ideaCarrier.Startups.Where(s => !s.Locked || !s.Approved || (s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) <= 0)).OrderBy(s => s.StartupID).ToList();
                        }

                        //return View(startups);
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
                    //ViewBag.UserRole = Role.Admin.ToString();
                    //return View(startups);
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
                    case "SWEDISHREGION": return View(startupProjects.OrderBy(su => su.SwedishRegion?.RegionName));
                    case "PROJECTDOMAINNAME": return View(startupProjects.OrderBy(su => su.ProjectDomain?.ProjectDomainName));
                    case "FUNDINGAMOUNTVALUE": return View(startupProjects.OrderBy(su => su.FundingAmount?.FundingAmountValue));
                    case "MATCHMAKINGCOUNT": return View(startupProjects.OrderBy(su => su.MatchMakings?.Count()));
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
            if (startupProject.SwedishRegionID.HasValue) ViewBag.SwedishRegionName = db.SwedishRegions.Where(sr => sr.RegionID == startupProject.SwedishRegionID).FirstOrDefault().RegionName;

            ViewBag.UserRole = Role.IdeaCarrier.ToString();
            ViewBag.ApprovedBy = "";

            if (User.IsInRole(Role.Admin.ToString()))
            {
                ViewBag.UserRole = Role.Admin.ToString();
                ViewBag.IdeaCarrierUserName = startupProject.User.UserName;
                if (!string.IsNullOrEmpty(startupProject.ApprovedByID))
                //{
                    ViewBag.ApprovedBy = db.Users.Where(u => u.Id == startupProject.ApprovedByID).FirstOrDefault().UserName;
                    //if (string.IsNullOrEmpty(approvedBy))
                    //{
                    //    ViewBag.ApprovedBy = "a formal user";
                    //}
                    //else ViewBag.ApprovedBy = approvedBy;Descending
                //}
                return View(startupProject);
            }

            return View(startupProject);
        }

        // GET: Startups/Create
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult AddNewProject()
        {
            AddNewProjectViewModel newProjectmodel = new AddNewProjectViewModel()
            {
                IdeaCarrierMessage = db.IdeaCarrierMessages.FirstOrDefault().Text,
                CountryList = new SelectList(db.Countries, "CountryID", "CountryName"),
                SwedishCountryID = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single(),
                SwedishRegionList = new SelectList(db.SwedishRegions, "RegionID", "RegionName")
            };

            //ViewBag.CountryID = new SelectList(db.Countries, "CountryID", "CountryName");
            //ViewBag.SwedishRegionsID = new SelectList(db.SwedishRegions, "RegionID", "RegionName"); //<----------
            //ViewBag.SwedishCountryId = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();
            //ViewBag.IdeaCarrierMessage = db.IdeaCarrierMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();

            return  View(newProjectmodel);
        }

        // POST: Startups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "IdeaCarrier")] //StartupID,UserId,,ProjectDomainID,ProjectSummary,FundingPhaseID,FundingAmountID,FutureFundingNeeded,EstimatedExitPlanID,EstimatedBreakEven,TeamMemberSize,GoalTeamSize,TeamExperience,TeamVisionShared,HaveFixedRoles,PossibleIncomeStreams,InnovationLevelID,ScalabilityID,DeadlineDate,LastSavedDate,CreatedDate,Locked,WillSpendOwnMoney,AlreadySpentMoney,AlreadySpentTime
        //public ActionResult AddNewProject([Bind(Include = "StartupName,CountryID,SwedishRegionID")] Models.IdeaCarrier.Startup model, string submitCommand)
        public ActionResult AddNewProject(AddNewProjectViewModel newProjectModel, string submit_command)
        {
            //if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Submit"))
            //{   //if user submit the form, lock it for editable
            //this must be before ModelState.IsValid
            //    model.Locked = true;
            //    TryValidateModel(model);
            //return Content(startup.Lock.ToString());
            //}

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

                //------------------------------------

                //List<FundingDivision> fundingDivisions = db.FundingDivisions.ToList(); //initFundingDivisions(newStartupProject.StartupID)
                //foreach (var fundingDivision in fundingDivisions)
                //{
                //    db.FundingDivisionStartups.Add(new FundingDivisionStartup
                //    {
                //        FundingDivisionID = fundingDivision.FundingDivisionID,
                //        Percentage = 0,
                //        StartupID = newStartupProjectID //<---necessary?
                //    });
                //}

                List<FundingDivision> fundingDivisions = db.FundingDivisions.ToList();
                foreach (FundingDivision fundingDivision in fundingDivisions)
                {
                    newStartupProject.ProjectFundingDivisions.Add(new FundingDivisionStartup
                    {
                         FundingDivisionID = fundingDivision.FundingDivisionID,
                         Percentage = 0,
                         StartupID = newStartupProjectID //<---necessary?
                    });
                }
                //---------------------------------

                db.Startups.Add(newStartupProject);
                db.SaveChanges();

                if (!string.IsNullOrEmpty(submit_command) && submit_command.StartsWith("Proceed")) //<------"Proceed to the Project form"
                {
                    //return Content(startup.StartupID.ToString());
                    return RedirectToAction("ProjectForm", new { id = newStartupProjectID });
                }
                else {
                  return RedirectToAction("Index");
                }
            }

            //In case of validation error, rerwite the ViewBags for dropdownlist for create view that handle the selection options
            //ViewBag.CountryId = new SelectList(db.Countries, "CountryID", "CountryName");
            //ViewBag.RegionsId = new SelectList(db.SwedishRegions, "RegionID", "RegionName");
            //ViewBag.SwedishCountryId = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();
            //ViewBag.IdeaCarrierMessage = db.IdeaCarrierMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();

            newProjectModel.IdeaCarrierMessage = db.IdeaCarrierMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();
            newProjectModel.CountryList = new SelectList(db.Countries, "CountryID", "CountryName");
            newProjectModel.SwedishCountryID = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();
            newProjectModel.SwedishRegionList = new SelectList(db.SwedishRegions, "RegionID", "RegionName");

            return View(newProjectModel);
        }

        // GET: Startups/Edit/5
        //[HttpGet]
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult ProjectForm(string id)
        {
            if (User.IsInRole(Role.Admin.ToString())) return RedirectToAction("EditAdmin", new { id });

            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //These viewBags to handel the current values of the startup properies if exist and provide by other selections
            //Models.IdeaCarrier.Startup currentStartup = db.Startups.Find(id);
            //ViewBag.projectDomainIdEdit = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", currentStartup.ProjectDomainID != null ? currentStartup.ProjectDomainID : null);
            //ViewBag.FundingPhaseIdEdit = new SelectList(db.FundingPhases, "FundingPhaseID", "FundingPhaseName", currentStartup.FundingPhaseID != null ? currentStartup.FundingPhaseID : null);
            //ViewBag.FundingNeedIdEdit = new SelectList(db.FundingAmounts, "FundingAmountID", "FundingAmountValue", currentStartup.FundingAmountID != null ? currentStartup.FundingAmountID : null);
            //ViewBag.EstimatedExitPlanIdEdit = new SelectList(db.EstimatedExitPlans, "EstimatedExitPlanID", "EstimatedExitPlanName", currentStartup.EstimatedExitPlanID != null ? currentStartup.EstimatedExitPlanID : null);
            //ViewBag.InnovationLevelIdEdit = new SelectList(db.InnovationLevels, "InnovationLevelID", "InnovationLevelName", currentStartup.InnovationLevelID != null ? currentStartup.InnovationLevelID : null);
            //ViewBag.ScalabilityIdEdit = new SelectList(db.Scalabilities, "ScalabilityID", "ScalabilityName", currentStartup.ScalabilityID != null ? currentStartup.ScalabilityID : null);

            //Get the uploaded files size
            //ViewBag.AllowedToUploadMore = AllowedToUploadMore(id); <--- move to Index

            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(id); //startupProject
            if (startupProject == null)
            {
                return HttpNotFound();
            }

            if (startupProject.Locked) return RedirectToAction("ProjectDetails", new { id });

            ViewBag.Message = "";
            //ViewBag.Tab = "tab_Project";
            ViewBag.Unanswered = "";

            if (TempData.Any())
            {
                if (TempData.ContainsKey("message")) ViewBag.Message = TempData["message"] as string;
                //if (TempData.ContainsKey("tab")) ViewBag.Tab = TempData["tab"] as string;
                if (TempData.ContainsKey("unanswered")) ViewBag.Unanswered = TempData["unanswered"] as string;
                TempData.Clear();
            }

            //to be deleted----------------------------
            //ViewBag.projectDomainIdEdit = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", startup.ProjectDomainID != null ? startup.ProjectDomainID : null);
            //ViewBag.FundingPhaseIdEdit = new SelectList(db.FundingPhases, "FundingPhaseID", "FundingPhaseName", startup.FundingPhaseID != null ? startup.FundingPhaseID : null);
            //ViewBag.FundingNeedIdEdit = new SelectList(db.FundingAmounts, "FundingAmountID", "FundingAmountValue", startup.FundingAmountID != null ? startup.FundingAmountID : null);
            //ViewBag.EstimatedExitPlanIdEdit = new SelectList(db.EstimatedExitPlans, "EstimatedExitPlanID", "EstimatedExitPlanName", startup.EstimatedExitPlanID != null ? startup.EstimatedExitPlanID : null);
            //ViewBag.InnovationLevelIdEdit = new SelectList(db.InnovationLevels, "InnovationLevelID", "InnovationLevelName", startup.InnovationLevelID != null ? startup.InnovationLevelID : null);
            //ViewBag.ScalabilityIdEdit = new SelectList(db.Scalabilities, "ScalabilityID", "ScalabilityName", startup.ScalabilityID != null ? startup.ScalabilityID : null);

            //PopulateAssignedWeaknessesData(startup);
            //this query has been modified to handle weaknesses checked boxes
            //PopulateAssignedCheckBoxsData(startup); //to be replaced

            //return View(startup);
            return View(GetStartupProjectViewModel(startupProject));
        }

        // POST: Startups/ProjectForm/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        [Authorize(Roles = "IdeaCarrier")]
        //CreatedDate, AllowSharingDisplayName?
        //public ActionResult ProjectForm([Bind(Include = "StartupID,UserId,CountryID,SwedishRegionID,StartupName,ProjectDomainID,DeadlineDate,ProjectSummary,AllowSharing,FundingPhaseID,FundingAmountID,EstimatedExitPlanID,FutureFundingNeeded,AlreadySpentTime,AlreadySpentMoney,WillSpendOwnMoney,EstimatedBreakEven,PossibleIncomeStreams,HavePayingCustomers,TeamMemberSize,TeamExperience,TeamVisionShared,HaveFixedRoles,LookingForActiveInvestors,InnovationLevelID,ScalabilityID,LastSavedDate,Locked,LastLockedDate")] Models.IdeaCarrier.Startup model,
        //    string[] SelectedSharedToInvestors, string[] SelectedTeamWeaknesses, string[] SelectedOutcomes, string ActiveTab, string submitCommand)
        public ActionResult ProjectForm(StartupProjectPostViewModel projectPostModel)
        {
            //if (model.Locked) return RedirectToAction("ProjectDetails", new { id = model.StartupID }); //<----------

            //if (!string.IsNullOrEmpty(submitCommand) && submitCommand.ToUpper().StartsWith("SUBMIT"))
            //{   //if user submits the form, lock it in order to be editable
            //this must be before ModelState.IsValid
            //    model.Locked = true; //<----------------------IsBeingSubmitted
            //    if (selectedOutcomes == null)
            //    {
            //        ModelState.AddModelError("Outcomes", "Select at least one Outcome");
            //    }
            //    if (selectedSharedToInvestors == null)
            //    {
            //        ModelState.AddModelError("AllowedInvestors", "Select at least one Investor");
            //    }
            //    TryValidateModel(model);
            //}

            bool updated = false;

            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(projectPostModel.StartupID);

            if (ModelState.IsValid)
            {
                //UpdateStartupWeaknesses(selectedWeaknesses, startup);
                //UpdateStartupCheckBoxsData(SelectedSharedToInvestors, SelectedTeamWeaknesses, SelectedOutcomes, model);

                //if &&!model.Locked {               

                if (UpdateActiveTab(startupProject, projectPostModel))
                {
                    startupProject.LastSavedDate = DateTime.Now.Date;
                    db.Entry(startupProject).State = EntityState.Modified;
                    db.SaveChanges();

                    updated = true;
                }

                //if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Upload"))
                //{   //save the data of the form before upload the file to prevent loss of data
                //    return RedirectToAction("UploadFile", "Startups", new { StartupID = model.StartupID });  
                //}

                //if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Submit")) return RedirectToAction("ProjectDetails", new { id = model.StartupID });
                                                                                                //return RedirectToAction("Index");

                //}
                //else ModelState.AddModelError("Locked", "Form locked, not possible to change anything in it, please contact Admin by the contact form.");
            }
            else ModelState.AddModelError("", "Validation Error !!");

            //ViewBag.Message = "";
            ViewBag.Tab = projectPostModel.ActiveTab; //<---------------
            //ViewBag.Unanswered = "";

            //In Case of validation error, reload the existing selected options if any, and provide by new selection options
            //Models.IdeaCarrier.Startup currentStartup = db.Startups.Find(model.StartupID);
          
            //ViewBag.projectDomainIdEdit = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", currentStartup.ProjectDomainID != null ? currentStartup.ProjectDomainID : null); //<-------------simplify!!
            //ViewBag.FundingPhaseIdEdit = new SelectList(db.FundingPhases, "FundingPhaseID", "FundingPhaseName", currentStartup.FundingPhaseID != null ? currentStartup.FundingPhaseID : null);
            //ViewBag.FundingNeedIdEdit = new SelectList(db.FundingAmounts, "FundingAmountID", "FundingAmountValue", currentStartup.FundingAmountID != null ? currentStartup.FundingAmountID : null);
            //ViewBag.EstimatedExitPlanIdEdit = new SelectList(db.EstimatedExitPlans, "EstimatedExitPlanID", "EstimatedExitPlanName", currentStartup.EstimatedExitPlanID != null ? currentStartup.EstimatedExitPlanID : null);
            //ViewBag.InnovationLevelIdEdit = new SelectList(db.InnovationLevels, "InnovationLevelID", "InnovationLevelName", currentStartup.InnovationLevelID != null ? currentStartup.InnovationLevelID : null); //<--------------------!!
            //ViewBag.ScalabilityIdEdit = new SelectList(db.Scalabilities, "ScalabilityID", "ScalabilityName", currentStartup.ScalabilityID != null ? currentStartup.ScalabilityID : null);
            //Get the uploaded files size
            //ViewBag.AllowedToUploadMore = AllowedToUploadMore(model.StartupID); //<--------------flytta!!!

            //PopulateAssignedCheckBoxsData(model/*, selectedWeaknesses, selectedOutcomes, selectedSharedToInvestors*/); //<-----------------!!!

            //return View(model);
            return View(GetStartupProjectViewModel(startupProject, updated));
        }

        private StartupProjectViewModel GetStartupProjectViewModel(Models.IdeaCarrier.Startup startupProject, bool updated = false)
        {
            return new StartupProjectViewModel()
            {
                StartupID = startupProject.StartupID,

                //Project
                ProjectName = startupProject.StartupName,
                ProjectDomainID = startupProject.ProjectDomainID,
                ProjectDomainList = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName"), //, startup.ProjectDomainID)
                DeadlineDate = startupProject.DeadlineDate,
                ProjectSummary = startupProject.ProjectSummary,
                AllowSharing_DisplayName = db.IdeaCarrierMessages.FirstOrDefault().AllowSharing_DisplayName, //<-----
                AllowSharing = startupProject.AllowSharing,
                AllowedInvestors = GetAllowedInvestors(startupProject), //<--------------!!!
                //AllowedInvestorsUnanswered = !startupProject.AllowedInvestors.Any(),

                //Funding
                FundingPhaseID = startupProject.FundingPhaseID,
                FundingPhaseList = new SelectList(db.FundingPhases, "FundingPhaseID", "FundingPhaseName"/*, startup.FundingPhaseID != null ? startup.FundingPhaseID : null*/),
                FundingAmountID = startupProject.FundingAmountID,
                FundingAmountList = new SelectList(db.FundingAmounts, "FundingAmountID", "FundingAmountValue"/*, startup.FundingAmountID != null ? startup.FundingAmountID : null*/),
                FutureFundingNeeded = startupProject.FutureFundingNeeded,
                AlreadySpentTime = startupProject.AlreadySpentTime,
                AlreadySpentMoney = startupProject.AlreadySpentMoney,
                WillSpendOwnMoney = startupProject.WillSpendOwnMoney,

                //Budget
                FundingDivisions = startupProject.ProjectFundingDivisions.Distinct().ToList(), //GetFundingDivisions(startupProject), //<-------------!!!
                //ProjectFundingDivisionsUnanswered = startupProject.ProjectFundingDivisions.Count(pfd => pfd.Percentage == 0) == startupProject.ProjectFundingDivisions.Count(),
                EstimatedExitPlanID = startupProject.EstimatedExitPlanID,
                EstimatedExitPlanList = new SelectList(db.EstimatedExitPlans, "EstimatedExitPlanID", "EstimatedExitPlanName"/*, startup.EstimatedExitPlanID != null ? startup.EstimatedExitPlanID : null*/),
                EstimatedBreakEven = startupProject.EstimatedBreakEven,
                PossibleIncomeStreams = startupProject.PossibleIncomeStreams,
                HavePayingCustomers = startupProject.HavePayingCustomers,

                //Team
                TeamMemberSize = startupProject.TeamMemberSize,
                TeamExperience = startupProject.TeamExperience,
                TeamVisionShared = startupProject.TeamVisionShared,
                HaveFixedRoles = startupProject.HaveFixedRoles,
                TeamWeaknesses = GetTeamWeaknesses(startupProject), //<-------------------------!!!
                //TeamWeaknessesUnanswered = !startupProject.TeamWesaknesses.Any(),
                LookingForActiveInvestors = startupProject.LookingForActiveInvestors,

                //Outcome
                Outcomes = GetOutcomes(startupProject), //<------------------------------!!!
                //OutcomesUnanswered = !startupProject.Outcomes.Any(),
                InnovationLevelID = startupProject.InnovationLevelID,
                InnovationLevelList = new SelectList(db.InnovationLevels, "InnovationLevelID", "InnovationLevelName"/*, startup.InnovationLevelID != null ? startup.InnovationLevelID : null*/),
                ScalabilityID = startupProject.ScalabilityID,
                ScalabilityList = new SelectList(db.Scalabilities, "ScalabilityID", "ScalabilityName"/*, startup.ScalabilityID != null ? startup.ScalabilityID : null*/),

                Updated = updated,

                //AllQuestionsAnswered = GetAllQuestionsAnswered(startupProject)
            };
        }

        //private bool GetAllQuestionsAnswered(Models.IdeaCarrier.Startup startupProject)
        //{
        //    return
        //         startupProject.ProjectDomainID.HasValue &&
        //        !string.IsNullOrEmpty(startupProject.ProjectSummary) &&
        //        //startupProject.AllowSharing.HassValue &&
        //        startupProject.AllowedInvestors.Any() &&
             
        //        startupProject.FundingPhaseID.HasValue &&
        //        startupProject.FundingAmountID.HasValue &&
        //        //startupProject.FutureFundingNeeded.HasValue &&
        //        startupProject.AlreadySpentTime.HasValue &&
        //        startupProject.AlreadySpentMoney.HasValue &&
        //        //startupProject.WillSpendOwnMoney.HasValue &&

        //        startupProject.ProjectFundingDivisions.Count(pfd => pfd.Percentage > 0) <= startupProject.ProjectFundingDivisions.Count() &&
        //        startupProject.EstimatedExitPlanID.HasValue &&
        //        startupProject.EstimatedBreakEven.HasValue &&
        //        startupProject.PossibleIncomeStreams.HasValue &&
        //        //startupProject.HavePayingCustomers.HasValue &&

        //        startupProject.TeamMemberSize.HasValue &&
        //        startupProject.TeamExperience.HasValue &&
        //        //startupProject.TeamVisionShared.HasValue &&
        //        //startupProject.HaveFixedRoles.HasValue &&
        //        startupProject.TeamWeaknesses.Any() &&
        //        //startupProject.LookingForActiveInvestors.HasValue &&

        //        startupProject.Outcomes.Any() &&
        //        startupProject.InnovationLevelID.HasValue &&
        //        startupProject.ScalabilityID.HasValue;
        //}

        private List<AllowedInvestorViewModel> GetAllowedInvestors(Models.IdeaCarrier.Startup startupProject)
        {
            //Models.IdeaCarrier.Startup currentAllowedInvestorsStartup = db.Startups.Include(a => a.AllowedInvestors).Where(s => s.StartupID == startupID).Single();
            //var startupAllowedInvestors = new HashSet<int>(currentAllowedInvestorsStartup.AllowedInvestors.Select(a => a.AllowedInvestorID));

            List<AllowedInvestorViewModel> allowedInvestorViewModels = new List<AllowedInvestorViewModel>();

            List<int> projectAllowedInvestorIDList = startupProject.AllowedInvestors.Select(ai => ai.AllowedInvestorID).ToList();

            //if (/*startupProject.AllowedInvestors != null && */startupProject.AllowedInvestors.Any())
            //{
            //currentAllowedInvestorIDList = startupProject.AllowedInvestors.Select(ai => ai.AllowedInvestorID).ToList();
            //}

            List<AllowedInvestor> allowedInvestors = db.AllowedInvestors.ToList();
            foreach (var allowedInvestor in allowedInvestors)
            {
                allowedInvestorViewModels.Add(new AllowedInvestorViewModel
                {
                    AllowedInvestorID = allowedInvestor.AllowedInvestorID,
                    AllowedInvestorName = allowedInvestor.AllowedInvestorName,
                    //Assigned = currentAllowedInvestorIDList.Contains(allowedInvestor.AllowedInvestorID)
                    Assigned = projectAllowedInvestorIDList.Any() ? projectAllowedInvestorIDList.Contains(allowedInvestor.AllowedInvestorID) : false
                });
            }
            //}

            return allowedInvestorViewModels;
        }

        //private List<FundingDivisionStartup> GetFundingDivisions(Models.IdeaCarrier.Startup startupProject)
        //{
            //if (startupProject.ProjectFundingDivisions == null || !startupProject.ProjectFundingDivisions.Any()) //Initiation
            //{
                //var currentStartupFundingDivisionsStartup = db.Startups.Include(f => f.ProjectFundingDivisions).Where(s => s.StartupID == startupProject.StartupID).Single();
                //var startupFundingDivisions = new HashSet<int>(currentStartupFundingDivisionsStartup.ProjectFundingDivisions.Select(f => f.FundingDivisionID));

                //var FundingDivisionList = new List<FundingDivisionStartup>();

                //List<int> currentFundingDivisionIDList = null;
                //if (startupProject.ProjectFundingDivisions != null && !startupProject.ProjectFundingDivisions.Any())
                //{
                //    currentFundingDivisionIDList = startupProject.ProjectFundingDivisions.Select(fd => fd.FundingDivisionID).ToList();
                //}

                //bool updated = false;

                //List<FundingDivision> fundingDivisions = db.FundingDivisions.ToList(); //==>AddNewProject
                //foreach (var fundingDivision in fundingDivisions)
                //{
                //    if (currentFundingDivisionIDList == null || !currentFundingDivisionIDList.Contains(fundingDivision.FundingDivisionID))
                //    {
                //        db.FundingDivisionStartups.Add(new FundingDivisionStartup
                //        {
                //            FundingDivisionID = fundingDivision.FundingDivisionID,
                //            Percentage = 0,
                //            StartupID = startupProject.StartupID
                //        });
                //        updated = true;
                //    }
                //}
                //if (updated) db.SaveChanges();
            //}
            //return db.FundingDivisionStartups.Where(fs => fs.StartupID == startupProject.StartupID).ToList();
            //return startupProject.ProjectFundingDivisions.ToList();
        //}

        private List<TeamWeaknessViewModel> GetTeamWeaknesses(Models.IdeaCarrier.Startup startupProject)
        {
            List<TeamWeaknessViewModel> teamWeaknessViewModels = new List<TeamWeaknessViewModel>();

            List<int> projectTeamWeaknessIDList = startupProject.TeamWeaknesses.Select(tw => tw.TeamWeaknessID).ToList();

            //if (startupProject.TeamWeaknesses != null && startupProject.TeamWeaknesses.Any())
            //{
            //    currentTeamWeaknessIDList = startupProject.TeamWeaknesses.Select(tw => tw.TeamWeaknessID).ToList();
            //}
            //var currentWeaknessesStartup = db.Startups.Include(w => w.TeamWeaknesses).Where(s => s.StartupID == startupID).Single();
            //var startupWeaknesses = new HashSet<int>(currentWeaknessesStartup.TeamWeaknesses.Select(w => w.TeamWeaknessID));

            List<TeamWeakness> teamWeaknesses = db.TeamWeaknesses.ToList();
            foreach (var teamWeakness in teamWeaknesses)
            {
                teamWeaknessViewModels.Add(new TeamWeaknessViewModel
                {
                    WeaknessID = teamWeakness.TeamWeaknessID,
                    WeaknessName = teamWeakness.TeamWeaknessName,
                    //Assigned = currentTeamWeaknessIDList.Contains(teamWeakness.TeamWeaknessID)
                    Assigned = projectTeamWeaknessIDList.Any() ? projectTeamWeaknessIDList.Contains(teamWeakness.TeamWeaknessID) : false
                });
            }
            //}

            return teamWeaknessViewModels;
        }

        private List<StartupOutcomeViewModel> GetOutcomes(Models.IdeaCarrier.Startup startupProject)
        {
            List<StartupOutcomeViewModel> outcomeViewModels = new List<StartupOutcomeViewModel>();

            List<int> projectOutcomeIDList = startupProject.Outcomes.Select(o => o.OutcomeID).ToList();

            //if (startupProject.Outcomes != null && startupProject.Outcomes.Any())
            //{
            //    currentOutcomeIDList = startupProject.Outcomes.Select(o => o.OutcomeID).ToList();
            //}
                //var currentOutcomesStartup = db.Startups.Include(o => o.Outcomes).Where(s => s.StartupID == startupID).Single();
                //var startupOutcomes = new HashSet<int>(currentOutcomesStartup.Outcomes.Select(o => o.OutcomeID));
                
                List<Outcome> outcomes = db.Outcomes.ToList();
                foreach (var outcome in outcomes)
                {
                    outcomeViewModels.Add(new StartupOutcomeViewModel
                    {
                        OutcomeID = outcome.OutcomeID,
                        OutcomeName = outcome.OutcomeName,
                        //Assigned = currentOutcomeIDList.Contains(outcome.OutcomeID)
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
            
            if (startupProject.AllowSharing != projectPostModel.AllowSharing)
            {
                startupProject.AllowSharing = projectPostModel.AllowSharing;
                updated = true;
            }

            if (UpdateAllowedToInvestors(projectPostModel.SelectedAllowedInvestorIDs, startupProject) && !updated) updated = true;

            return updated;
        }

        private bool UpdateAllowedToInvestors(string[] selectedAllowedInvestorIDs, Models.IdeaCarrier.Startup startupProject)
        {
            bool updated = false;

            if (selectedAllowedInvestorIDs != null)
            {
                List<int> selectedAllowedInvestorIDList = Array.ConvertAll(selectedAllowedInvestorIDs, s => int.Parse(s)).ToList();

                List<AllowedInvestor> projectAllowedInvestors = startupProject.AllowedInvestors.ToList();
                foreach (AllowedInvestor projectAllowedInvestor in projectAllowedInvestors)
                {
                    if (!selectedAllowedInvestorIDList.Contains(projectAllowedInvestor.AllowedInvestorID))
                    {
                        startupProject.AllowedInvestors.Remove(projectAllowedInvestor);
                        updated = true;
                    }
                }

                List<AllowedInvestor> allowedInvestors = db.AllowedInvestors.ToList();
                AllowedInvestor allowedInvestor = null;
                foreach (int selectedAllowedInvestorID in selectedAllowedInvestorIDList)
                {
                    allowedInvestor = allowedInvestors.Where(ai => ai.AllowedInvestorID == selectedAllowedInvestorID).FirstOrDefault();

                    if (!startupProject.AllowedInvestors.Contains(allowedInvestor))
                    {
                        startupProject.AllowedInvestors.Add(allowedInvestor);
                        updated = true;
                    }
                }
            }
            else //if (selectedAllowedInvestorIDs == null)
            {
                startupProject.AllowedInvestors.Clear();
                updated = true;
            }

            //if (selectedAllowedInvestorIDs != null)
            //{
            //    List<int> selectedAllowedInvestorIDList = Array.ConvertAll(selectedAllowedInvestorIDs, s => int.Parse(s)).ToList();

            //    List<int> currentAllowedInvestorIDList = startupProject.AllowedInvestors.Select(ai => ai.AllowedInvestorID).ToList();

            //    List<AllowedInvestor> allowedInvestors = db.AllowedInvestors.ToList();

            //    if (currentAllowedInvestorIDList.Any())
            //    {
            //        foreach (var allowedInvestor in allowedInvestors)
            //        {
            //            if (selectedAllowedInvestorIDList.Contains(allowedInvestor.AllowedInvestorID) && !currentAllowedInvestorIDList.Contains(allowedInvestor.AllowedInvestorID)) //selectedSharedToInvestorsHS
            //            {
            //                startupProject.AllowedInvestors.Add(allowedInvestor);
            //                updated = true;
            //            }
            //            else if (currentAllowedInvestorIDList.Contains(allowedInvestor.AllowedInvestorID) && startupProject.AllowedInvestors.Remove(allowedInvestor))
            //                updated = true;
            //        }
            //    }
            //}
            //else if (startupProject.AllowedInvestors.Any())
            //{
            //    List<AllowedInvestor> allowedInvestors = db.AllowedInvestors.ToList();

            //    foreach (var allowedInvestor in allowedInvestors)
            //    {
            //        if (startupProject.AllowedInvestors.Remove(allowedInvestor)) updated = true;
            //    }
            //}

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

            if (startupProject.FutureFundingNeeded != projectPostModel.FutureFundingNeeded)
            {
                startupProject.FutureFundingNeeded = projectPostModel.FutureFundingNeeded;
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

            if (startupProject.WillSpendOwnMoney != projectPostModel.WillSpendOwnMoney)
            {
                startupProject.WillSpendOwnMoney = projectPostModel.WillSpendOwnMoney;
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
            
            if (startupProject.HavePayingCustomers != projectPostModel.HavePayingCustomers)
            {
                startupProject.HavePayingCustomers = projectPostModel.HavePayingCustomers;
                updated = true;
            }

            return updated;
        }

        private bool UpdateFundingDivisionPercentages(string[] fundingDivisionPercentages, Models.IdeaCarrier.Startup startupProject)
        {
            bool updated = false;

            if (fundingDivisionPercentages != null)
            {
                List<int> fundingDivisionPercentageList = Array.ConvertAll(fundingDivisionPercentages, s => int.Parse(s)).ToList();

                //List<FundingDivisionStartup> startupFundingDivisionList = db.FundingDivisionStartups.Where(fds => fds.StartupID == startupProject).Distinct().ToList();
                //List<FundingDivisionStartup> startupFundingDivisionList = startupProject.ProjectFundingDivisions.Distinct().ToList();

                //int fundingDivisionPercentage = 0;

                //if (startupProject.ProjectFundingDivisions != null && startupProject.ProjectFundingDivisions.Sum(pfd => pfd.Percentage) > 100)
                //{
                //    TempData["message"] = "In the question \"How will the funding be spent?\" the sum of the percentages can't be over 100!"; //Max 100%
                //    TempData["unanswered"] = "ProjectFundingDivisions";
                //return Redirect(Url.Action("ProjectForm", new { startupProject.StartupID }) + "#Budget");
                //}

                if (fundingDivisionPercentageList.Sum() <= 100) //<--------------!!!
                {
                    for (int i = 0; i < fundingDivisionPercentageList.Count(); i++)
                    {
                        //fundingDivisionPercentage = fundingDivisionPercentageList.ElementAt(i);
                        if (startupProject.ProjectFundingDivisions.ElementAt(i).Percentage != fundingDivisionPercentageList.ElementAt(i))
                        {
                            startupProject.ProjectFundingDivisions.ElementAt(i).Percentage = fundingDivisionPercentageList.ElementAt(i);
                            //db.Entry(startupFundingDivisionList.ElementAt(i)).State = EntityState.Modified; //<------!!!
                            updated = true;
                        }
                    }
                } 
                else TempData["message"] = "In tab BudgetI \"How will the funding be spent?\" the sum of the percentages can't be over 100!";
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

            if (startupProject.TeamVisionShared != projectPostModel.TeamVisionShared)
            {
                startupProject.TeamVisionShared = projectPostModel.TeamVisionShared;
                updated = true;
            }

            if (startupProject.HaveFixedRoles != projectPostModel.HaveFixedRoles)
            {
                startupProject.HaveFixedRoles = projectPostModel.HaveFixedRoles;
                updated = true;
            }

            ////if (UpdateStartupCheckBoxsData(null, projectPostModel.SelectedTeamWeaknessIDs, null, startupProject) && !updated) updated = true;
            if (UpdateTeamWeaknesses(projectPostModel.SelectedTeamWeaknessIDs, startupProject) && !updated) updated = true;

            if (startupProject.LookingForActiveInvestors != projectPostModel.LookingForActiveInvestors)
            {
                startupProject.LookingForActiveInvestors = projectPostModel.LookingForActiveInvestors;
                updated = true;
            }

            return updated;
        }

        private bool UpdateTeamWeaknesses(string[] selectedTeamWeaknessIDs, Models.IdeaCarrier.Startup startupProject)
        {
            bool updated = false;

            if (selectedTeamWeaknessIDs != null)
            {
                List<int> selectedTeamWeaknessIDList = Array.ConvertAll(selectedTeamWeaknessIDs, s => int.Parse(s)).ToList();

                List<TeamWeakness> projectTeamWeaknesses = startupProject.TeamWeaknesses.ToList();
                foreach (TeamWeakness projectTeamWeakness in projectTeamWeaknesses)
                {
                    if (!selectedTeamWeaknessIDList.Contains(projectTeamWeakness.TeamWeaknessID))
                    {
                        startupProject.TeamWeaknesses.Remove(projectTeamWeakness);
                        updated = true;
                    }
                }

                List<TeamWeakness> teamSkills = db.TeamWeaknesses.ToList();
                TeamWeakness teamWeakness = null;
                foreach (int selectedTeamWeaknessID in selectedTeamWeaknessIDList)
                {
                    teamWeakness = teamSkills.Where(tw => tw.TeamWeaknessID == selectedTeamWeaknessID).FirstOrDefault();

                    if (!startupProject.TeamWeaknesses.Contains(teamWeakness))
                    {
                        startupProject.TeamWeaknesses.Add(teamWeakness);
                        updated = true;
                    }
                }
            }
            else //if (selectedTeamWeaknessIDs == null)
            {
                startupProject.TeamWeaknesses.Clear();
                updated = true;
            }

            return updated;
        }

        private bool UpdateTabOutcome(Models.IdeaCarrier.Startup startupProject, StartupProjectPostViewModel projectPostModel)
        {
            bool updated = false;

            updated = UpdateOutcomes(projectPostModel.SelectedOutcomeIDs, startupProject);

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
        private bool UpdateOutcomes(string[] SelectedOutcomeIDs, Models.IdeaCarrier.Startup startupProject)
        {
            bool updated = false;

            if (SelectedOutcomeIDs != null)
            {
                List<int> selectedOutcomeIDList = Array.ConvertAll(SelectedOutcomeIDs, s => int.Parse(s)).ToList();

                List<Outcome> projectOutcomes = startupProject.Outcomes.ToList();
                foreach (Outcome projectOutcome in projectOutcomes)
                {
                    if (!selectedOutcomeIDList.Contains(projectOutcome.OutcomeID))
                    {
                        startupProject.Outcomes.Remove(projectOutcome);
                        updated = true;
                    }
                }

                List<Outcome> outcomes = db.Outcomes.ToList();
                Outcome outcome = null;
                foreach (int selectedOutcomeID in selectedOutcomeIDList)
                {
                    outcome = outcomes.Where(o => o.OutcomeID == selectedOutcomeID).FirstOrDefault();

                    if (!startupProject.Outcomes.Contains(outcome))
                    {
                        startupProject.Outcomes.Add(outcome);
                        updated = true;
                    }
                }
            }
            else //if (selectedOutcomeIDs == null)
            {
                startupProject.Outcomes.Clear();
                updated = true;
            }

            //if (SelectedOutcomeIDs != null)
            //{
            //    List<int> selectedOutcomeIDList = Array.ConvertAll(SelectedOutcomeIDs, s => int.Parse(s)).ToList();

            //    var currentOutcomeIDList = startupProject.Outcomes.Select(o => o.OutcomeID).ToList();

            //    var outcomes = db.Outcomes.ToList();

            //    foreach (var outcome in outcomes)
            //    {
            //        if (selectedOutcomeIDList.Contains(outcome.OutcomeID) && !currentOutcomeIDList.Contains(outcome.OutcomeID))
            //        {
            //            startupProject.Outcomes.Add(outcome);
            //            updated = true;
            //        }
            //        else if (currentOutcomeIDList.Contains(outcome.OutcomeID) && startupProject.Outcomes.Remove(outcome))
            //            updated = true;
            //    }
            //}
            //else if (startupProject.Outcomes.Any())
            //{
            //    var outcomes = db.Outcomes.ToList();

            //    foreach (var outcome in outcomes)
            //    {
            //        if (startupProject.Outcomes.Remove(outcome)) updated = true;
            //    }
            //}

            return updated;
        }

        //[HttpGet]
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult SubmitProjectForm(string id, bool cancel = false, string redirect = "", string redirectTab = "")
        {
            if (cancel)
            {
                TempData["message"] = "Submission of form cancelled!";
                //TempData["tab"] = redirectTab;
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
            if (startupProject.ProjectFundingDivisions != null && startupProject.ProjectFundingDivisions.Sum(pfd => pfd.Percentage) > 100)
            {
                TempData["message"] = "In the question \"How will the funding be spent?\" the sum of the percentages can't be over 100!"; //Max 100%
                TempData["unanswered"] = "ProjectFundingDivisions";
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

            bool? approved = startupProject.Approved;
            if (startupProject.Approved && startupProject.ApprovedByID != User.Identity.GetUserId()) approved = null;

            StartupEditAdminViewModel model = new StartupEditAdminViewModel
            {
                StartupID = startupProject.StartupID,
                IdeaCarrierName = startupProject.User.UserName,
                ProjectSummary = startupProject.ProjectSummary,
                Approved =  approved,
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

                if (model.Approved.HasValue && startupProject.Approved != model.Approved.Value)
                {
                    startupProject.Approved = model.Approved.Value;
                    startupProject.ApprovedByID = User.Identity.GetUserId();
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
        [Authorize(Roles = "Admin, IdeaCarrier")]
        //[HttpDelete]
        public ActionResult RemoveProject(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.IdeaCarrier.Startup startup = db.Startups.Find(id);
            if (startup == null)
            {
                return HttpNotFound();
            }
            return View(startup);
        }

        // POST: Startups/RemoveProject/5
        [Authorize(Roles = "Admin, IdeaCarrier")]
        [HttpPost, ActionName("RemoveProject")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id) //
        {   
            Models.IdeaCarrier.Startup startupProject = db.Startups.Find(id);

            //Find all Associated files and delete them
            //var documents = db.Documents.Where(d => d.StartupID == id).ToList(); //<-----startupProject.Documents.ToList();
            //foreach (var document in documents)
            //{
            //    DeleteDocument(document.DocId, document.DocURL, null);
            //}

            //startup.ProjectFundingDivisions.Clear(); //<-----??

            //foreach (var projectFundingDivision in startup.ProjectFundingDivisions) //projectFundingDivisions
            //{
            //    if (db.FundingDivisionStartups.Contains(projectFundingDivision))
            //        db.FundingDivisionStartups.Remove(projectFundingDivision);
            //}
            
            //db.AllowedInvestors (virtual) X
            //db.FundingPhase X
            //db.InnovationLevel X
            //db.TeamWeaknesses (virtual) X
            //db.Outcomes (virtual) X
            //db.Scalability X

            db.Startups.Remove(startupProject);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        //save the user url to return to the previous page
        private string RedirectCheck() //<--------to be removed
        {
            if (Request.UrlReferrer != null)
            {
                return Request.UrlReferrer.ToString();
            }
            else
            {
                return "Empty";
            }
        }

        //--------------------------------------------------------------
        // for Uploading single files:
        // GET:
        [HttpGet]
        [Authorize(Roles = "Admin, IdeaCarrier")]
        public ActionResult UploadDocument(string id)
        {
            ViewBag.StartupID = id;
            //ViewBag.RedirectString = "Index"; //RedirectCheck(); // + "#RelatedFiles"; <----"ProjectDetails"???
            return View();
        }

        // POST: 
        [HttpPost]
        [Authorize(Roles = "Admin, IdeaCarrier")] //UploadDocumentFile(DocumentViewModel documentModel,... <------------------!!!
        public ActionResult UploadDocument(DocumentUploadViewModel documentModel/*, string RedirectString*/)
        {   /*[Bind(Include = "DocId,DocName,DocDescription,DocTimestamp,StartupID,DocURL")]*/
            //HttpPostedFileBase documentFile, string RedirectString, string description, string startupID

            if (documentModel.DocFile != null && documentModel.DocFile.ContentLength > 0)
            {
                try
                {
                    var timeStamp = DateTime.Now.Ticks;
                    string path = Path.Combine(Server.MapPath("~/Upload"),
                    Path.GetFileName(timeStamp + "_" + documentModel.DocFile.FileName));
                    documentModel.DocFile.SaveAs(path);

                    Document document = new Document()
                    {
                        DocName = Path.GetFileName(documentModel.DocFile.FileName), // documentModel.DocName,
                        DocDescription = documentModel.DocDescription,
                        DocTimestamp = DateTime.Now,

                        //Rename the file
                        DocURL = Path.GetFileName("/Upload/" + timeStamp + "_" + documentModel.DocFile.FileName),

                        StartupID = documentModel.StartupID,
                        UserId = User.Identity.GetUserId()
                    };

                    //document.DocName = Path.GetFileName(documentFile.FileName);
                    //document.DocDescription = description;
                    //document.DocTimestamp = DateTime.Now;
                    //document.StartupID = startupID;
                    //document.UserId = User.Identity.GetUserId();
                    //rename the file
                    //document.DocURL = Path.GetFileName("/Upload/" + timeStamp + "_" + documentFile.FileName);

                    db.Documents.Add(document);
                    db.SaveChanges();
                    ViewBag.Message = "Document uploaded successfully!";
                    //return Redirect(RedirectString); //"Index" "ProjectDetails",
                    //RedirectToAction("ProjectDetails", new { id = documentModel.StartupID });
                    RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a document.";
            }
            ViewBag.StartupID = documentModel.StartupID;
            return View();
        }

        //this class to list all documents related to the startup
        [Authorize(Roles = "Admin, IdeaCarrier")]
        public ActionResult DocumentList(string id) //Id of the startup project.
        {
            ViewBag.UserIsInRoleAdmin = User.IsInRole("Admin");
            ViewBag.Locked = db.Startups.Find(id).Locked;
            ViewBag.UserId = User.Identity.GetUserId();

            var documents = db.Documents.Where(d => d.StartupID == id); //<------startupProject.Documents.ToList();

            if (documents == null)
            {
                return HttpNotFound("No documents found."); //<---------------added
            }

            return PartialView(documents.ToList());
        }

        //Download Documents
        [Authorize(Roles = "Admin, IdeaCarrier")]
        public FileResult DownloadDocument(string docLink, string name)
        {
            var FileVirtualPath = "/Upload/" + docLink;
            return File(FileVirtualPath, "application/force-download", name);
        }

        //Delete Documents
        public ActionResult DeleteDocument(int docId, string docLink, string redirectString) //<--------Add: string startupId
        {
            var FileVirtualPath = "~/Upload/" + docLink;
            //System.IO.File.Delete(Path.GetFileName(FileVirtualPath));
            System.IO.File.Delete(Server.MapPath(FileVirtualPath));

            Document document = db.Documents.Find(docId);
            
            db.Documents.Remove(document);
            db.SaveChanges();
            //redirectString = RedirectCheck() + "#RelatedFiles"; //<-----------------!!!!
            //return Redirect(redirectString);
            //return RedirectToAction("DocumentList");
            return RedirectToAction("Index"); //"ProjectDetails", new { id = startupID });
        }

        private Boolean AllowedToUploadMore(string id)
        {
            var documents = db.Documents.Where(d => d.StartupID == id);
            long maxDocumentLSize = 1133800; // in bytes, This equal 3 Megabyte 3000000
            long filesSize = 0;
            bool permission = false;
            foreach (var document in documents)
            {
                var FileVirtualPath = "/Upload/" + document.DocURL;
                FileInfo File = new FileInfo(Server.MapPath(FileVirtualPath));
                filesSize += File.Length;
            }

            if (filesSize < maxDocumentLSize)
            {
                permission = true;
            }
            else
            {
                permission = false;
            }

            return permission;
        }


        //-----------------------------------------------------------------
        //private void UpdateStartupCheckBoxsData(string[] SelectedSharedToInvestors, string[] SelectedTeamWeaknesses, string[] SelectedOutcomes, Models.IdeaCarrier.Startup startup)
        //{
        //    //Profile
        //    if (SelectedSharedToInvestors != null) //==> UpdateSelectedSharedToInvestors(Models.IdeaCarrier.Startup startupProject)
        //    {
        //        //this is the new selection list
        //        var selectedSharedToInvestorsHS = new HashSet<string>(SelectedSharedToInvestors);
        //        //List<int> selectedSharedToInvestorsIDs = Array.ConvertAll(SelectedSharedToInvestors, s => int.Parse(s)).ToList();
        //        //this is the current or previous saved selections
        //        Models.IdeaCarrier.Startup currentStartupSharedToInvestorsStartup = db.Startups.Include(a => a.AllowedInvestors).Where(s => s.StartupID == startup.StartupID).Single();
        //        var startupCurrentSharedToInvestorIDs = new HashSet<int>(currentStartupSharedToInvestorsStartup.AllowedInvestors.Select(a => a.AllowedInvestorID));

        //        foreach (var allowedInvestor in db.AllowedInvestors)
        //        {
        //            if (selectedSharedToInvestorsHS.Contains(allowedInvestor.AllowedInvestorID.ToString()))
        //            {
        //                //if the selection not in previous selections, add it
        //                if (!startupCurrentSharedToInvestorIDs.Contains(allowedInvestor.AllowedInvestorID))
        //                {
        //                    startup.AllowedInvestors.Add(allowedInvestor);
        //                }
        //            }
        //            else
        //            {
        //                //if the selection in previous selections but not in the new selected list, remove it
        //                if (startupCurrentSharedToInvestorIDs.Contains(allowedInvestor.AllowedInvestorID))
        //                {
        //                    startup.AllowedInvestors.Remove(allowedInvestor);
        //                }
        //            }
        //        }
        //    }
        //    else if (startup.AllowedInvestors != null)
        //    {
        //        //Delete Any previous selection if any
        //        //this is the current or previous saved selections
        //        //var currentStartupShareToInvestors = db.Startups.Include(a => a.AllowedInvestors).Where(s => s.StartupID == startup.StartupID).Single();
        //        //var startupCurrentShareToInvestors = new HashSet<int>(currentStartupShareToInvestors.AllowedInvestors.Select(a => a.AllowedInvestorID));

        //        foreach (var allowedInvestor in db.AllowedInvestors)
        //        {
        //            startup.AllowedInvestors.Remove(allowedInvestor);
        //        }
        //    }

        //    //********************************************************************************//
        //    //Team
        //    if (SelectedTeamWeaknesses != null) //==> UpdateSelectedTeamWeaknesses(Models.IdeaCarrier.Startup startup)
        //    {
        //        //this is the new selection list
        //        var selectedWeaknessesHS = new HashSet<string>(SelectedTeamWeaknesses);
        //        //List<int> selectedTeamWeaknessIDs = Array.ConvertAll(SelectedTeamWeaknesses, s => int.Parse(s)).ToList();
        //        Models.IdeaCarrier.Startup currentStartupWeaknessesStartup = db.Startups.Include(w => w.TeamWeaknesses).Where(s => s.StartupID == startup.StartupID).Single();
        //        var startupCurrentWeaknessIDs = new HashSet<int>(currentStartupWeaknessesStartup.TeamWeaknesses.Select(w => w.TeamWeaknessID));

        //        foreach (var teamWeakness in db.TeamWeaknesses)
        //        {
        //            if (selectedWeaknessesHS.Contains(teamWeakness.TeamWeaknessID.ToString()))
        //            {
        //                //if the selection not in previous selections, add it
        //                if (!startupCurrentWeaknessIDs.Contains(teamWeakness.TeamWeaknessID))
        //                {
        //                    startup.TeamWeaknesses.Add(teamWeakness);
        //                }
        //            }
        //            else
        //            {
        //                //if the selection in previous selections but not in the new selected list, remove it 
        //                if (startupCurrentWeaknessIDs.Contains(teamWeakness.TeamWeaknessID))
        //                {
        //                    startup.TeamWeaknesses.Remove(teamWeakness);
        //                }
        //            }
        //        }
        //    }
        //    else if (startup.TeamWeaknesses != null)
        //    {
        //        //Delete Any previous selection if any
        //        //this is the current or previous saved selections
        //        //var currentStartupWeaknesses = db.Startups.Include(w => w.TeamWeaknesses).Where(s => s.StartupID == startup.StartupID).Single();
        //        //var startupCurrentWeaknesses = new HashSet<int>(currentStartupWeaknesses.TeamWeaknesses.Select(w => w.TeamWeaknessID));

        //        foreach (var teamWeakness in db.TeamWeaknesses)
        //        {
        //            startup.TeamWeaknesses.Remove(teamWeakness);
        //        }
        //    }

        //    //********************************************************************************//
        //    //Outcome

        //    if (SelectedOutcomes != null) //==> UpdateSelectedOutcomes(Models.IdeaCarrier.Startup startupProject)
        //    {
        //        //this is the new selection list
        //        var selectedOutcomesHS = new HashSet<String>(SelectedOutcomes);
        //        //List<int> selectedOutcomes = Array.ConvertAll(SelectedOutcomes, s => int.Parse(s)).ToList();
        //        Models.IdeaCarrier.Startup currentStartupOutcomesStartup = db.Startups.Include(o => o.Outcomes).Where(s => s.StartupID == startup.StartupID).Single();
        //        var startupCurrentOutcomeIDs = new HashSet<int>(currentStartupOutcomesStartup.Outcomes.Select(o => o.OutcomeID));
                
        //        foreach (var outcome in db.Outcomes)
        //        {
        //            if (selectedOutcomesHS.Contains(outcome.OutcomeID.ToString()))
        //            {
        //                //if the selection not in previous selections, add it
        //                if (!startupCurrentOutcomeIDs.Contains(outcome.OutcomeID))
        //                {
        //                    startup.Outcomes.Add(outcome);
        //                }
        //            }
        //            else
        //            {
        //                //if the selection in previous selections but not in the new selected list, remove it 
        //                if (startupCurrentOutcomeIDs.Contains(outcome.OutcomeID))
        //                {
        //                    startup.Outcomes.Remove(outcome);
        //                }
        //            }
        //        }
        //    }
        //    else if (startup.Outcomes != null)
        //    {
        //        //Delete Any previous selection if any
        //        //this is the current or previous saved selections
        //        //var currentStartupOutcomes = db.Startups.Include(o => o.Outcomes).Where(s => s.StartupID == startup.StartupID).Single();
        //        //var startupCurrentOutcomes = new HashSet<int>(currentStartupOutcomes.Outcomes.Select(o => o.OutcomeID));

        //        foreach (var outcome in db.Outcomes)
        //        {
        //            startup.Outcomes.Remove(outcome);
        //        }
        //    }

        //    //********************************************************************************//
        //}

        //to be deleted
        private void PopulateAssignedCheckBoxsData(Models.IdeaCarrier.Startup startup)
        {
            var allWeaknesses = db.TeamWeaknesses;
            var currentWeaknessesStartup = db.Startups.Include(w => w.TeamWeaknesses).Where(s => s.StartupID == startup.StartupID).Single();
            var startupWeaknesses = new HashSet<int>(currentWeaknessesStartup.TeamWeaknesses.Select(w => w.TeamWeaknessID));
            var weaknessesViewModel = new List<TeamWeaknessViewModel>();

            foreach (var weakness in allWeaknesses)
            {
                weaknessesViewModel.Add(new TeamWeaknessViewModel
                {
                    WeaknessID = weakness.TeamWeaknessID,
                    WeaknessName = weakness.TeamWeaknessName,
                    Assigned = startupWeaknesses.Contains(weakness.TeamWeaknessID)
                });
            }

            ViewBag.weaknessesViewModel = weaknessesViewModel;
            //----------------------------
            var allOutcomes = db.Outcomes; 
            var currentStartupOutcomes = db.Startups.Include(o => o.Outcomes).Where(s => s.StartupID == startup.StartupID).Single();
            var startupOutcomes = new HashSet<int>(currentStartupOutcomes.Outcomes.Select(o => o.OutcomeID));
            var OutcomesViewModel = new List<StartupOutcomeViewModel>();

            foreach (var outcome in allOutcomes)
            {
                OutcomesViewModel.Add(new StartupOutcomeViewModel
                {
                    OutcomeID = outcome.OutcomeID,
                    OutcomeName = outcome.OutcomeName,
                    Assigned = startupOutcomes.Contains(outcome.OutcomeID)
                });
            }

            ViewBag.outcomesViewModel = OutcomesViewModel;
            //--------------------------------------------
            var allAllowedInvestors = db.AllowedInvestors;
            var currentStartupAllowedInvestors = db.Startups.Include(a => a.AllowedInvestors).Where(s => s.StartupID == startup.StartupID).Single();
            var startupAllowedInvestors = new HashSet<int>(currentStartupAllowedInvestors.AllowedInvestors.Select(a => a.AllowedInvestorID));
            var AllowedInvestorsViewModel = new List<AllowedInvestorViewModel>();

            foreach (var investors in allAllowedInvestors)
            {
                AllowedInvestorsViewModel.Add(new AllowedInvestorViewModel
                {
                    AllowedInvestorID = investors.AllowedInvestorID,
                    AllowedInvestorName = investors.AllowedInvestorName,
                    Assigned = startupAllowedInvestors.Contains(investors.AllowedInvestorID)
                });
            }

            ViewBag.AllowedInvestorsViewModel = AllowedInvestorsViewModel;
            //--------------------------------------------
            //if (startup.ProjectFundingDivisions == null) { //getFundingDivisionList(startupProject)
            var allFundingDivisions = db.FundingDivisions; 
            var currentStartupFundingDivisions = db.Startups.Include(f => f.ProjectFundingDivisions).Where(s => s.StartupID == startup.StartupID).Single();
            var startupFundingDivisions = new HashSet<int>(currentStartupFundingDivisions.ProjectFundingDivisions.Select(f => f.FundingDivisionID));
            var FundingDivisionsList = new List<FundingDivisionStartup>();

            foreach (var fundingDivision in allFundingDivisions)
            {
                if (!startupFundingDivisions.Contains(fundingDivision.FundingDivisionID))
                {
                    db.FundingDivisionStartups.Add(new FundingDivisionStartup
                    {
                        FundingDivisionID = fundingDivision.FundingDivisionID,
                        Percentage = 0,
                        StartupID = startup.StartupID
                    });
                }
            }
            db.SaveChanges(); //<-----------------------------------------------------------------------????
                                      //model.FundingDivisionStartups.Where(fds => fds.StartupID.Equals(Model.StartupID)
            ViewBag.FundingDivisionsList = db.FundingDivisionStartups.Where(fs => fs.StartupID == startup.StartupID).ToList();          
        }



        /******************************************************************************/
        //to be deleted
        //this function will be called if there is validation error, so can we add the latest selection of checkboxes
        //private void PopulateAssignedCheckBoxsData(Models.IdeaCarrier.Startup startup, string[]  selectedWeaknesses, string[] selectedOutcomes, string[] selectedSharedToInvestors)
        //{
        //    var allWeaknesses = db.TeamWeaknesses;
        //    var selectedWeaknessesHS = new HashSet<String>();
        //    var weaknessesViewModel = new List<TeamWeaknessViewModel>();

        //    if (selectedWeaknesses != null)
        //    {
        //        selectedWeaknessesHS = new HashSet<String>(selectedWeaknesses);
        //    }

        //    foreach (var weakness in allWeaknesses)
        //    {
        //        weaknessesViewModel.Add(new TeamWeaknessViewModel
        //        {
        //            WeaknessID = weakness.TeamWeaknessID,
        //            WeaknessName = weakness.TeamWeaknessName,
        //            Assigned = selectedWeaknessesHS.Contains(weakness.TeamWeaknessID.ToString())
        //        });
        //    }
        //    ViewBag.weaknessesViewModel = weaknessesViewModel;
        //    //----------------------------
        //    var allOutcomes = db.Outcomes;
        //    var selectedOutcomesHS = new HashSet<String>();
        //    var OutcomesViewModel = new List<StartupOutcomeViewModel>();

        //    if (selectedOutcomes != null)
        //    {
        //        selectedOutcomesHS = new HashSet<String>(selectedOutcomes);
        //    }

        //    foreach (var outcome in allOutcomes)
        //    {
        //        OutcomesViewModel.Add(new StartupOutcomeViewModel
        //        {
        //            OutcomeID = outcome.OutcomeID,
        //            OutcomeName = outcome.OutcomeName,
        //            Assigned = selectedOutcomesHS.Contains(outcome.OutcomeID.ToString())
        //        });
        //    }

        //    ViewBag.outcomesViewModel = OutcomesViewModel;
        //    //--------------------------------------------
        //    var allAllowedInvestors = db.AllowedInvestors;
        //    var selectedSharedToInvestorsHS = new HashSet<String>();
        //    var AllowedInvestorsViewModel = new List<AllowedInvestorViewModel>();

        //    if (selectedSharedToInvestors != null)
        //    {
        //        selectedSharedToInvestorsHS = new HashSet<String>(selectedSharedToInvestors);
        //    }
            
        //    foreach (var investors in allAllowedInvestors)
        //    {
        //        AllowedInvestorsViewModel.Add(new AllowedInvestorViewModel
        //        {
        //            AllowedInvestorID = investors.AllowedInvestorID,
        //            AllowedInvestorName = investors.AllowedInvestorName,
        //            Assigned = selectedSharedToInvestorsHS.Contains(investors.AllowedInvestorID.ToString())
        //        });
        //    }

        //    ViewBag.AllowedInvestorsViewModel = AllowedInvestorsViewModel;
        //    //------------------------------------------------------------
        //    ViewBag.FundingDivisionsList = db.FundingDivisionStartups.Where(fs => fs.StartupID == startup.StartupID).ToList();
        //}



        /************************************************************************/
        // GET:
        [HttpGet]  
        [Authorize(Roles = "Admin")] //<--------------------
        public ActionResult CasePreview(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.IdeaCarrier.Startup startup = db.Startups.Find(id);
            if (startup == null)
            {
                return HttpNotFound();
            }
            return View(startup);
        }

        [Authorize(Roles = "Admin")] //,Investor
        public ActionResult GeneratePDF(string id)
        {
            //return new Rotativa.ActionAsPdf("CasePreview/"+id); ViewAsPdf
            try //<-----------------------------------------------------------------------
            {
                return new Rotativa.ActionAsPdf("CasePreview", new { id });
            }
            catch (Exception)
            {
                return RedirectToAction("CasePreview", new { id });
            }
        }

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
            if (startup.Approved) startup.ApprovedByID = User.Identity.GetUserId(); //<-------------------------
            else startup.ApprovedByID = "";

            db.Entry(startup).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect))
            {
                if (redirect.ToUpper().Contains("DETAILS")) return RedirectToAction("ProjectDetails", new { id });
                else return RedirectToAction(redirect, new { id });
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Unlock(string id, string redirect = "")
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
            //try
            startup.Locked = false;
            startup.Approved = false;
            startup.ApprovedByID = "";

            db.Entry(startup).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect))
            {
                if (redirect.ToUpper().Contains("DETAILS")) return RedirectToAction("ProjectDetails", new { id });
                else return RedirectToAction(redirect, new { id });
            }

            return RedirectToAction("Index");
        }

        // GET: Startups/Reminder
        [Authorize(Roles = ("Admin"))]
        public ActionResult Reminder(string id, string subject = "", string message = "", string redirect = "") //id==startupId
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.IdeaCarrier.Startup Startup = db.Startups.Find(id);

            if (Startup == null)
            {
                return HttpNotFound();
            }
            else
            {
                ReminderStartupViewModel model = new ReminderStartupViewModel
                {
                    StartupId = id, //==Startup.StartupID,
                    StartupName = Startup.StartupName,
                    IdeaCarrierEmail = Startup.User.Email,
                    IdeaCarrierId = Startup.UserID,
                    Subject = subject,
                    Message = message,
                    Redirect = redirect
                };
                return View(model);
            }

            //return View();
        }

        // POST: Startups/Reminder
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = ("Admin"))]
        [HttpPost]
        [ValidateAntiForgeryToken] //[Bind(Include = "StartupId,InvestorEmail,Text")]
        [ValidateInput(false)]
        public async Task<ActionResult> Reminder(ReminderStartupViewModel model)
        {
            if (ModelState.IsValid)
            {
                var smtpClients = db.SmtpClients.ToList();

                foreach (Models.SMTP.SmtpClient smtpClient in smtpClients)
                {
                    if (smtpClient.Active)
                    {
                        try
                        {
                            MailMessage message = new MailMessage
                            {
                                From = new MailAddress(User.Identity.Name),
                                Subject = model.Subject,
                                Body = model.StartupId + (string.IsNullOrEmpty(model.StartupName) ? "" : " (" + model.StartupName + ")") + "\n\n"+ model.Message,
                                IsBodyHtml = false
                            };

                            message.To.Add(new MailAddress(model.IdeaCarrierEmail));
                            message.Bcc.Add(new MailAddress(smtpClient.MailRecipient));
                            message.Bcc.Add(new MailAddress(User.Identity.GetUserName()));

                            using (var smtp = new System.Net.Mail.SmtpClient())
                            {
                                var credential = new NetworkCredential
                                {
                                    UserName = smtpClient.CredentialUserName,
                                    Password = smtpClient.CredentialPassword
                                };
                                smtp.Credentials = credential;
                                smtp.Host = smtpClient.Host;
                                smtp.Port = smtpClient.Port;
                                smtp.EnableSsl = smtpClient.EnableSsl;
                                await smtp.SendMailAsync(message);

                                ModelState.AddModelError("", "The reminder about " + model.StartupId + " (" + model.StartupName + ") has been sent to " + model.IdeaCarrierEmail);
                                return View(model);
                            }
                        }
                        catch
                        {
                            ModelState.AddModelError("", "The reminder could not be sent (Email recipient probably non-existent).");
                        }
                    }
                }
                ModelState.AddModelError("", "The reminder could not be sent (Smtp Client probably not set).");
            }
            else ModelState.AddModelError("", "The Model State is invalid.");

            return View(model);
        }

        //ProlongDeadlineDate", , new { id = Model.StartupID, period = 6 }
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

            if (!string.IsNullOrWhiteSpace(redirect))
            {
                if (redirect.ToUpper().Contains("DETAILS")) return RedirectToAction("ProjectDetails", new { id });
                else return RedirectToAction(redirect, new { id });
            }

            return RedirectToAction("Index");
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