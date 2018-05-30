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
//using EoS.ViewModels;
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
        public ActionResult Index(string id, bool? matchable, string orderBy) //id==startupId
        {
            List<Models.IdeaCarrier.Startup> startups = null;

            ViewBag.Matchable = null;
            ViewBag.UserRole = "";
            ViewBag.IdeaCarrierId = "";
            ViewBag.IdeaCarrierUserName = "";

            if (User.IsInRole(Role.Admin.ToString()))
            {
                if (!string.IsNullOrEmpty(id)) //The Admin looks at a special IdeaCarrier's startup projectss
                {
                    if (!matchable.HasValue)
                    {
                        startups = db.Startups.Where(i => i.UserID == id).ToList();
                    }
                    else if (matchable.Value)
                    {
                        ViewBag.Matchable = true;
                        startups = db.Startups.Where(s => s.UserID == id && s.Locked && s.Approved && (!s.DeadlineDate.HasValue || s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).OrderBy(s => s.StartupID).ToList();
                    }
                    else
                    {
                        ViewBag.Matchable = false;
                        startups = db.Startups.Where(s => s.UserID == id && (!s.Locked || !s.Approved || (s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) <= 0))).OrderBy(s => s.StartupID).ToList();
                    }
                    ViewBag.UserRole = Role.Admin.ToString();
                    ViewBag.IdeaCarrierId = id; //==Startups.FirstOrDefault().User.Id
                    ApplicationUser ideaCarrier = db.Users.Find(id);
                    if (ideaCarrier != null) ViewBag.IdeaCarrierUserName = ideaCarrier.UserName;
                    //return View(startups);
                }
                else
                {
                    if (!matchable.HasValue)
                    {
                        startups = db.Startups.ToList();
                    }
                    else if (matchable.Value)
                    {
                        ViewBag.Matchable = true;
                        startups = db.Startups.Where(s => s.Locked && s.Approved && (!s.DeadlineDate.HasValue || s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).OrderBy(s => s.StartupID).ToList();
                    }
                    else
                    {
                        ViewBag.Matchable = false;
                        startups = db.Startups.Where(s => !s.Locked || !s.Approved || (s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) <= 0)).OrderBy(s => s.StartupID).ToList();
                    }
                    ViewBag.UserRole = Role.Admin.ToString();
                    //return View(startups);
                }
            }
            else //if (User.IsInRole("IdeaCarrier"))
            {
                string currentUserId = User.Identity.GetUserId();
                //ApplicationUser currentUser = db.Users.Find(currentUserId); //not necessary
                startups = db.Startups.Where(u => u.UserID == currentUserId).ToList();
                ViewBag.UserRole = Role.IdeaCarrier.ToString();
                //return View(UserStartups);
            }
            if (startups == null) startups = db.Startups.ToList();

            if (!string.IsNullOrEmpty(orderBy))
                switch (orderBy.ToUpper())
                {
                    case "USERNAME": return View(startups.OrderBy(su => su.User.UserName)); //break;
                    case "STARTUPID": return View(startups.OrderBy(su => su.StartupID));
                    case "COUNTRY": return View(startups.OrderBy(su => su.Country.CountryName));
                    case "SWEDISHREGION": return View(startups.OrderBy(su => su.SwedishRegion?.RegionName));
                    case "PROJECTDOMAINNAME": return View(startups.OrderBy(su => su.ProjectDomain?.ProjectDomainName));
                    case "FUNDINGAMOUNTVALUE": return View(startups.OrderBy(su => su.FundingAmount?.FundingAmountValue));
                    case "MATCHMAKINGCOUNT": return View(startups.OrderBy(su => su.MatchMakings?.Count()));
                    case "LASTSAVEDDATE": return View(startups.OrderBy(su => su.LastSavedDate));
                    case "DEADLINEDDATE": return View(startups.OrderByDescending(su => su.DeadlineDate));
                    case "CREATEDDATE": return View(startups.OrderByDescending(su => su.CreatedDate));
                }

            return View(startups);
        }

        // GET: Startups/ProjectDetails/5
        [Authorize(Roles = "Admin, IdeaCarrier")]
        public ActionResult ProjectDetails(string id)
        {
            ViewBag.UserRole = Role.IdeaCarrier.ToString();
            ViewBag.ApprovedBy = "";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.IdeaCarrier.Startup startup = db.Startups.Find(id);
            if (startup == null)
            {
                return HttpNotFound();
            }

            if (User.IsInRole(Role.Admin.ToString()))
            {
                ViewBag.UserRole = Role.Admin.ToString();
                ViewBag.IdeaCarrierUserName = startup.User.UserName;
                if (!string.IsNullOrEmpty(startup.ApprovedByID))
                {
                    string approvedBy = db.Users.Where(u => u.Id == startup.ApprovedByID).FirstOrDefault().UserName;
                    if (string.IsNullOrEmpty(approvedBy))
                    {
                        ViewBag.ApprovedBy = "a formal user";
                    }
                }
                return View(startup);
            }

            return View(startup);
        }

        // GET: Startups/Create
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult AddNewProject()
        {
            //these ViewBags for dropdownlist for create view that handle the selection options
            ViewBag.FundingPhaseId = new SelectList(db.FundingPhases, "FundingPhaseID", "FundingPhaseName");
            ViewBag.FundingNeedId = new SelectList(db.FundingAmounts, "FundingAmountID", "FundingAmountValue");
            ViewBag.EstimatedExitPlanId = new SelectList(db.EstimatedExitPlans, "EstimatedExitPlanID", "EstimatedExitPlanName");
            ViewBag.InnovationLevelId = new SelectList(db.InnovationLevels, "InnovationLevelID", "InnovationLevelName");
            ViewBag.projectDomainId = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName");
            ViewBag.ScalabilityId = new SelectList(db.Scalabilities, "ScalabilityID", "ScalabilityName");

            ViewBag.IdeaCarrierMessage = db.IdeaCarrierMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();

            ViewBag.CountryId = new SelectList(db.Countries, "CountryID", "CountryName");
            ViewBag.RegionsId = new SelectList(db.SwedishRegions, "RegionID", "RegionName"); //<----------
            ViewBag.SwedishCountryId = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();

            return View();
        }

        // POST: Startups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult AddNewProject([Bind(Include = "StartupID,UserId,StartupName,CountryID,SwedishRegionID,ProjectDomainID,ProjectSummary,FundingPhaseID,FundingAmountID,FutureFundingNeeded,EstimatedExitPlanID,EstimatedBreakEven,TeamMemberSize,GoalTeamSize,TeamExperience,TeamVisionShared,HaveFixedRoles,PossibleIncomeStreams,InnovationLevelID,ScalabilityID,DeadlineDate,LastSavedDate,CreatedDate,Locked,WillSpendOwnMoney,AlreadySpentMoney,AlreadySpentTime")] Models.IdeaCarrier.Startup model, string submitCommand)
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
                string countryAbbreviation = db.Countries.Find(model.CountryID).CountryAbbreviation;
                string startupRandomCode = "IC" + countryAbbreviation + HelpFunctions.GetShortCode();
                //check if the code is already exist
                while (db.Startups.Any(u => u.StartupID == startupRandomCode))
                {
                    startupRandomCode = "IC" + countryAbbreviation + HelpFunctions.GetShortCode();
                }
                model.StartupID = startupRandomCode;
                model.UserID = User.Identity.GetUserId();
                model.CreatedDate = DateTime.Now;
                model.LastSavedDate = DateTime.Now;
                db.Startups.Add(model);
                db.SaveChanges();

                if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Proceed")) //<-----------"Proceed to the Project form"
                {
                    //return Content(startup.StartupID.ToString());
                    return RedirectToAction("ProjectForm", "Startups", new { id = model.StartupID });
                }
                else {
                  return RedirectToAction("Index");
                }
            }

            //In case of validation error, rerwite the ViewBags for dropdownlist for create view that handle the selection options
            ViewBag.CountryId = new SelectList(db.Countries, "CountryID", "CountryName");
            ViewBag.projectDomainId = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName");
            ViewBag.FundingPhaseId = new SelectList(db.FundingPhases, "FundingPhaseID", "FundingPhaseName");
            ViewBag.FundingNeedId = new SelectList(db.FundingAmounts, "FundingAmountID", "FundingAmountValue");
            ViewBag.EstimatedExitPlanId = new SelectList(db.EstimatedExitPlans, "EstimatedExitPlanID", "EstimatedExitPlanName");
            ViewBag.InnovationLevelId = new SelectList(db.InnovationLevels, "InnovationLevelID", "InnovationLevelName"); //<-------------------------!!!
            ViewBag.ScalabilityId = new SelectList(db.Scalabilities, "ScalabilityID", "ScalabilityName");
            ViewBag.RegionsId = new SelectList(db.SwedishRegions, "RegionID", "RegionName");
            ViewBag.SwedishCountryId = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();
            ViewBag.IdeaCarrierMessage = db.IdeaCarrierMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();

            return View(model);
        }

        // GET: Startups/Edit/5
        //[HttpGet]
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult ProjectForm(string id) //Edit
        {
            if (User.IsInRole(Role.Admin.ToString())) RedirectToAction("EditAdmin", new { id });

            if (id == null)
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
            //ViewBag.AllowedToUploadMore = AllowedToUploadMore(id);

            Models.IdeaCarrier.Startup startup = db.Startups.Find(id);
            if (startup == null)
            {
                return HttpNotFound();
            }

            //PopulateAssignedWeaknessesData(startup);
            //this query has been modified to handle weaknesses checked boxes
            PopulateAssignedCheckBoxsData(startup);

            ViewBag.projectDomainIdEdit = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", startup.ProjectDomainID != null ? startup.ProjectDomainID : null);
            ViewBag.FundingPhaseIdEdit = new SelectList(db.FundingPhases, "FundingPhaseID", "FundingPhaseName", startup.FundingPhaseID != null ? startup.FundingPhaseID : null);
            ViewBag.FundingNeedIdEdit = new SelectList(db.FundingAmounts, "FundingAmountID", "FundingAmountValue", startup.FundingAmountID != null ? startup.FundingAmountID : null);
            ViewBag.EstimatedExitPlanIdEdit = new SelectList(db.EstimatedExitPlans, "EstimatedExitPlanID", "EstimatedExitPlanName", startup.EstimatedExitPlanID != null ? startup.EstimatedExitPlanID : null);
            ViewBag.InnovationLevelIdEdit = new SelectList(db.InnovationLevels, "InnovationLevelID", "InnovationLevelName", startup.InnovationLevelID != null ? startup.InnovationLevelID : null);
            ViewBag.ScalabilityIdEdit = new SelectList(db.Scalabilities, "ScalabilityID", "ScalabilityName", startup.ScalabilityID != null ? startup.ScalabilityID : null);

            return View(startup);
        }

        // POST: Startups/ProjectForm/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        [Authorize(Roles = "IdeaCarrier")]
        //TeamWeaknesses -------->ProjectFundingDivisions<----------  ,AllowSharingDisplayName,CountryID,SwedishRegionID,DeadlineDate,CreatedDate  StartupID,UserId,StartupName,ProjectDomainID,ProjectSummary,AllowSharing,FundingPhaseID,FundingAmountID,FutureFundingNeeded,AlreadySpentTime,AlreadySpentMoney,WillSpendOwnMoney,EstimatedExitPlanID,EstimatedBreakEven,PossibleIncomeStreams,HavePayingCustomers,TeamMemberSize,TeamExperience,TeamVision,HaveFixedRoles,LookingForActiveInvestors,InnovationLevelID,ScalabilityID,Locked,LastSavedDate
        public ActionResult ProjectForm([Bind(Include = "StartupID,UserId,CountryID,SwedishRegionID,StartupName,ProjectDomainID,DeadlineDate,ProjectSummary,AllowSharing,FundingPhaseID,FundingAmountID,EstimatedExitPlanID,FutureFundingNeeded,AlreadySpentTime,AlreadySpentMoney,WillSpendOwnMoney,EstimatedBreakEven,PossibleIncomeStreams,HavePayingCustomers,TeamMemberSize,TeamExperience,TeamVisionShared,HaveFixedRoles,LookingForActiveInvestors,InnovationLevelID,ScalabilityID,LastSavedDate,CreatedDate,Locked")] Models.IdeaCarrier.Startup model, string[] selectedSharedToInvestors, string[] selectedWeaknesses, string[] selectedOutcomes, string activeTab, string submitCommand)
        {
            //if (model.Locked) return RedirectToAction("ProjectDetails", new { id = model.StartupID }); //<----------

            if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Submit"))
            {   //if user submits the form, lock it in order to be editable
                //this must be before ModelState.IsValid
                model.Locked = true; //<----------------------IsBeeingSubmitted
                if (selectedOutcomes == null)
                {
                    ModelState.AddModelError("Outcomes", "Select at least one Outcome");
                }
                if (selectedSharedToInvestors == null)
                {
                    ModelState.AddModelError("AllowedInvestors", "Select at least one Investor");
                }
                TryValidateModel(model);
            }

            if (ModelState.IsValid)
            {
                //if &&!model.Locked {
                model.LastSavedDate = DateTime.Now;
                //model.Locked
                db.Entry(model).State = EntityState.Modified;

                //UpdateStartupWeaknesses(selectedWeaknesses, startup);
                UpdateStartupCheckBoxsData(selectedWeaknesses, selectedOutcomes, selectedSharedToInvestors, model);

                db.SaveChanges();

                //if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Upload"))
                //{   //save the data of the form before upload the file to prevent loss of data
                //    return RedirectToAction("UploadFile", "Startups", new { StartupID = model.StartupID });  
                //}

                if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Submit")) return RedirectToAction("ProjectDetails", new { id = model.StartupID });
                                                                                              //return RedirectToAction("Index");

                //}
                //else ModelState.AddModelError("Locked", "Form locked, not possible to change anything in it, please contact Admin by the contact form.");
            }
            else ModelState.AddModelError("", "Validation Error !!");

            //In Case of validation error, reload the existing selected options if any, and provide by new selection options
            Models.IdeaCarrier.Startup currentStartup = db.Startups.Find(model.StartupID);

            ViewBag.projectDomainIdEdit = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", currentStartup.ProjectDomainID != null ? currentStartup.ProjectDomainID : null);
            ViewBag.FundingPhaseIdEdit = new SelectList(db.FundingPhases, "FundingPhaseID", "FundingPhaseName", currentStartup.FundingPhaseID != null ? currentStartup.FundingPhaseID : null);
            ViewBag.FundingNeedIdEdit = new SelectList(db.FundingAmounts, "FundingAmountID", "FundingAmountValue", currentStartup.FundingAmountID != null ? currentStartup.FundingAmountID : null);
            ViewBag.EstimatedExitPlanIdEdit = new SelectList(db.EstimatedExitPlans, "EstimatedExitPlanID", "EstimatedExitPlanName", currentStartup.EstimatedExitPlanID != null ? currentStartup.EstimatedExitPlanID : null);
            ViewBag.InnovationLevelIdEdit = new SelectList(db.InnovationLevels, "InnovationLevelID", "InnovationLevelName", currentStartup.InnovationLevelID != null ? currentStartup.InnovationLevelID : null); //<--------------------!!
            ViewBag.ScalabilityIdEdit = new SelectList(db.Scalabilities, "ScalabilityID", "ScalabilityName", currentStartup.ScalabilityID != null ? currentStartup.ScalabilityID : null);
            //Get the uploaded files size
            ViewBag.AllowedToUploadMore = AllowedToUploadMore(model.StartupID);

            PopulateAssignedCheckBoxsData(model, selectedWeaknesses, selectedOutcomes, selectedSharedToInvestors);

            return View(model);
        }

        //private void updateFundingDivisions(List<FundingDivisionStartup> fundingDivisionsList, Models.IdeaCarrier.Startup model)
        //{
        //    var currentStartupFundingDivisions =  db.FundingDivisionStartups.Where(s => s.StartupID == model.StartupID);

        //    foreach (var currentValueFundingDivision in currentStartupFundingDivisions)
        //    {
        //        foreach (var fundingDivision in fundingDivisionsList)
        //        {

        //            if (currentValueFundingDivision.Id == fundingDivision.Id) {
        //                currentValueFundingDivision.Percentage = fundingDivision.Percentage;
        //            }
        //            //if (currentValueFundingDivision.FundingDivisionID.Equals(fundingDivision.FundingDivisionID) && currentValueFundingDivision.StartupID.Equals(fundingDivision.StartupID) && !currentValueFundingDivision.Percentage.Equals(fundingDivision.Percentage))
        //            //{
                        
        //            //    currentValueFundingDivision.Percentage = fundingDivision.Percentage;
        //            //    //currentValueFundingDivision.Percentage = 20;
        //            //}
        //            //currentValueFundingDivision.Percentage = 10;
        //        }
        //    }
        //    db.SaveChanges();
        //}

        // GET: Startups/EditAdmin/5
        //[HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditAdmin(string id)
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

            StartupEditAdminViewModel model = new StartupEditAdminViewModel
            {
                StartupID = startup.StartupID, //==id
                //AllowSharingDisplayName = startup.AllowSharingDisplayName,
                ProjectSummary = startup.ProjectSummary,
            };

            return View(model);
        }

        // POST: Startups/EditAdmin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateInput(false)] //<-------------------------------------------------------------------------Will not crash because of HTML tags!
        [Authorize(Roles = "Admin")] /*[Bind(Include = "StartupID,ProjectSummary,Locked,Approved")]*/
        public ActionResult EditAdmin(StartupEditAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                Models.IdeaCarrier.Startup startup = db.Startups.Find(model.StartupID);
                if (startup == null)
                {
                    return HttpNotFound();
                }

                //startup.AllowSharingDisplayName = model.AllowSharingDisplayName;
                startup.ProjectSummary = model.ProjectSummary;
                 
                db.Entry(startup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ProjectDetails", new { id = startup.StartupID });
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
        public ActionResult DeleteConfirmed(string id)
        {   

            Models.IdeaCarrier.Startup startup = db.Startups.Find(id);
            //Find all Associated files and delete them
            var documents = db.Documents.Where(d => d.StartupID == id).ToList();
            foreach (var document in documents)
            {
                DeleteDocument(document.DocId, document.DocURL, null);
            }

            //db.AllowedInvestors
            //db.EstimatedExitPlans
            //db.FundingAmounts
            //db.FundingDivisions

            //Delete FundingDivisionStartups <--------------------------------------------
            //var fundigDivisions = db.FundingDivisions.Where(fd => fd...).ToList();
            var fundingDivisionStartups = db.FundingDivisionStartups.Where(fds => fds.StartupID == id).ToList(); //<------------------------
            foreach (var fundingDivisionStartup in fundingDivisionStartups)
            {
                db.FundingDivisionStartups.Remove(fundingDivisionStartup);
            }

            //db.FundingPhases
            //db.InnovationLevels?
            //db.TeamWeaknesses?
            //db.Outcomes?
            //db.Scalabilities

            db.Startups.Remove(startup);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //save the user url to returnback to the previous page 
        private string RedirectCheck()
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
        [Authorize(Roles = "Admin, IdeaCarrier")] //UploadDocumentFile(
        public ActionResult UploadDocument(string id) //id==startupID
        {
            ViewBag.StartupID = id;
            ViewBag.RedirectString = RedirectCheck() + "#RelatedFiles";
            return View();
        }

        // POST: 
        [HttpPost]
        [Authorize(Roles = "Admin, IdeaCarrier")] //UploadDocumentFile(
        public ActionResult UploadDocument([Bind(Include = "DocId,DocName,DocDescription,DocTimestamp,StartupID,DocURL")] Document document, HttpPostedFileBase documentFile, string RedirectString, string description, string startupID)
        {
            if (documentFile != null && documentFile.ContentLength > 0)
            {
                try
                {
                    var timeStamp = DateTime.Now.Ticks;
                    string path = Path.Combine(Server.MapPath("~/Upload"),
                    Path.GetFileName(timeStamp + "_" + documentFile.FileName));
                    documentFile.SaveAs(path);

                    document.DocName = Path.GetFileName(documentFile.FileName);
                    document.DocDescription = description;
                    document.DocTimestamp = DateTime.Now;
                    document.StartupID = startupID;
                    document.UserId = User.Identity.GetUserId();
                    //rename the file
                    document.DocURL = Path.GetFileName("/Upload/" + timeStamp + "_" + documentFile.FileName);
                    db.Documents.Add(document);
                    db.SaveChanges();
                    ViewBag.Message = "Document uploaded successfully";
                    return Redirect(RedirectString);
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a document.";
            }
            ViewBag.StartupID = startupID;
            return View();
        }

        //this class to list all documents related to the startup
        [Authorize(Roles = "Admin, IdeaCarrier")]
        public ActionResult DocumentList(string id) //Id of the startup project.
        {          
            ViewBag.locked = db.Startups.Find(id).Locked;
            var documents = db.Documents.Where(d => d.StartupID == id);

            if (documents == null)
            {
                return HttpNotFound("No documents found."); //<--------------------------added
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
        public ActionResult DeleteDocument(int docId, string docLink, string redirectString)
        {
            var FileVirtualPath = "~/Upload/" + docLink;
            //System.IO.File.Delete(Path.GetFileName(FileVirtualPath));
            System.IO.File.Delete(Server.MapPath(FileVirtualPath));

            Document document = db.Documents.Find(docId);
            
            db.Documents.Remove(document);
            db.SaveChanges();
            redirectString = RedirectCheck() + "#RelatedFiles";
            return Redirect(redirectString);
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
        private void UpdateStartupCheckBoxsData(string[] selectedWeaknesses, string[] selectedOutcomes, string[] selectedSharedToInvestors, Models.IdeaCarrier.Startup startup)
        {
            //********************************************************************************//
            if (selectedWeaknesses != null)
            {
                //this is the new selection list
                var selectedWeaknessesHS = new HashSet<String>(selectedWeaknesses);
                var currentStartupWeaknesses = db.Startups.Include(w => w.TeamWeaknesses).Where(s => s.StartupID == startup.StartupID).Single();
                var startupCurrentWeaknesses = new HashSet<int>(currentStartupWeaknesses.TeamWeaknesses.Select(w => w.TeamWeaknessID));

                foreach (var weakness in db.TeamWeaknesses)
                {
                    if (selectedWeaknessesHS.Contains(weakness.TeamWeaknessID.ToString()))
                    {
                        //if the selection not in previous selections, add it
                        if (!startupCurrentWeaknesses.Contains(weakness.TeamWeaknessID))
                        {
                            startup.TeamWeaknesses.Add(weakness);
                        }
                    }
                    else
                    {
                        //if the selection in previous selections but not in the new selected list, remove it 
                        if (startupCurrentWeaknesses.Contains(weakness.TeamWeaknessID))
                        {
                            startup.TeamWeaknesses.Remove(weakness);
                        }
                    }
                }
            }
            else
            {
                //Delete Any previous selection if any
                //this is the current or previous saved selections
                var currentStartupWeaknesses = db.Startups.Include(w => w.TeamWeaknesses).Where(s => s.StartupID == startup.StartupID).Single();
                var startupCurrentWeaknesses = new HashSet<int>(currentStartupWeaknesses.TeamWeaknesses.Select(w => w.TeamWeaknessID));

                foreach (var weakness in db.TeamWeaknesses)
                {
                    startup.TeamWeaknesses.Remove(weakness);
                }
            }

            //********************************************************************************//

            if (selectedOutcomes != null)
            {
                //this is the new selection list
                var selectedOutcomesHS = new HashSet<String>(selectedOutcomes);
                var currentStartupOutcomes = db.Startups.Include(o => o.Outcomes).Where(s => s.StartupID == startup.StartupID).Single();
                var startupCurrentOutcomes = new HashSet<int>(currentStartupOutcomes.Outcomes.Select(o => o.OutcomeID));

                foreach (var outcome in db.Outcomes)
                {
                    if (selectedOutcomesHS.Contains(outcome.OutcomeID.ToString()))
                    {
                        //if the selection not in previous selections, add it
                        if (!startupCurrentOutcomes.Contains(outcome.OutcomeID))
                        {
                            startup.Outcomes.Add(outcome);
                        }
                    }
                    else
                    {
                        //if the selection in previous selections but not in the new selected list, remove it 
                        if (startupCurrentOutcomes.Contains(outcome.OutcomeID))
                        {
                            startup.Outcomes.Remove(outcome);
                        }
                    }
                }
            }
            else
            {
                //Delete Any previous selection if any
                //this is the current or previous saved selections
                var currentStartupOutcomes = db.Startups.Include(o => o.Outcomes).Where(s => s.StartupID == startup.StartupID).Single();
                var startupCurrentOutcomes = new HashSet<int>(currentStartupOutcomes.Outcomes.Select(o => o.OutcomeID));

                foreach (var outcome in db.Outcomes)
                {

                    startup.Outcomes.Remove(outcome);
                }
            }

            //********************************************************************************//

            if (selectedSharedToInvestors != null)
            {
                //this is the new selection list
                var selectedSharedToInvestorsHS = new HashSet<String>(selectedSharedToInvestors);
                //this is the current or previous saved selections
                var currentStartupShareToInvestors = db.Startups.Include(a => a.AllowedInvestors).Where(s => s.StartupID == startup.StartupID).Single();
                var startupCurrentShareToInvestors = new HashSet<int>(currentStartupShareToInvestors.AllowedInvestors.Select(a => a.AllowedInvestorID));

                foreach (var investor in db.AllowedInvestors)
                {
                    if (selectedSharedToInvestorsHS.Contains(investor.AllowedInvestorID.ToString()))
                    {
                        //if the selection not in previous selections, add it
                        if (!startupCurrentShareToInvestors.Contains(investor.AllowedInvestorID))
                        {
                            startup.AllowedInvestors.Add(investor);
                        }
                    }
                    else
                    {
                        //if the selection in previous selections but not in the new selected list, remove it 
                        if (startupCurrentShareToInvestors.Contains(investor.AllowedInvestorID))
                        {
                            startup.AllowedInvestors.Remove(investor);
                        }
                    }
                }
            }
            else
            {
                //Delete Any previous selection if any
                //this is the current or previous saved selections
                var currentStartupShareToInvestors = db.Startups.Include(a => a.AllowedInvestors).Where(s => s.StartupID == startup.StartupID).Single();
                var startupCurrentShareToInvestors = new HashSet<int>(currentStartupShareToInvestors.AllowedInvestors.Select(a => a.AllowedInvestorID));

                foreach (var investor in db.AllowedInvestors)
                {
                    startup.AllowedInvestors.Remove(investor);
                }
            }

            //********************************************************************************//           

        }

        private void PopulateAssignedCheckBoxsData(Models.IdeaCarrier.Startup startup)
        {
            var allWeaknesses = db.TeamWeaknesses;
            var currentWeaknessesStartup = db.Startups.Include(w => w.TeamWeaknesses).Where(s => s.StartupID == startup.StartupID).Single();
            var startupWeaknesses = new HashSet<int>(currentWeaknessesStartup.TeamWeaknesses.Select(w => w.TeamWeaknessID));
            var weaknessesViewModel = new List<TeamWeaknessesViewModel>();

            var allOutcomes = db.Outcomes; 
            var currentStartupOutcomes = db.Startups.Include(o => o.Outcomes).Where(s => s.StartupID == startup.StartupID).Single();
            var startupOutcomes = new HashSet<int>(currentStartupOutcomes.Outcomes.Select(o => o.OutcomeID));
            var OutcomesViewModel = new List<StartupOutcomeViewModel>();

            var allAllowedInvestors = db.AllowedInvestors;
            var currentStartupAllowedInvestors = db.Startups.Include(a => a.AllowedInvestors).Where(s => s.StartupID == startup.StartupID).Single();
            var startupAllowedInvestors = new HashSet<int>(currentStartupAllowedInvestors.AllowedInvestors.Select(a => a.AllowedInvestorID));
            var AllowedInvestorsViewModel = new List<AllowedInvestorViewModel>();

            //this for Funding Divisions
            var allFundingDivisions = db.FundingDivisions;
            //var currentStartupFundingDivisions = db.Startups.Include(f => f.FundingDivisionStartups).Where(s => s.StartupID == startup.StartupID).Single();
            var currentStartupFundingDivisions = db.Startups.Include(f => f.ProjectFundingDivisions).Where(s => s.StartupID == startup.StartupID).Single();
            var startupFundingDivisions = new HashSet<int>(currentStartupFundingDivisions.ProjectFundingDivisions.Select(f => f.FundingDivisionID));
            var FundingDivisionsList = new List<FundingDivisionStartup>();

            foreach (var weakness in allWeaknesses)
            {
                weaknessesViewModel.Add(new TeamWeaknessesViewModel
                {
                    WeaknessID = weakness.TeamWeaknessID,
                    WeaknessName = weakness.TeamWeaknessName,
                    Assigned = startupWeaknesses.Contains(weakness.TeamWeaknessID)
                });
            }
            ViewBag.weaknessesViewModel = weaknessesViewModel;


            foreach (var outcome in allOutcomes)
            {
                OutcomesViewModel.Add(new StartupOutcomeViewModel { OutcomeID = outcome.OutcomeID, OutcomeName = outcome.OutcomeName, Assigned = startupOutcomes.Contains(outcome.OutcomeID) });
            }

            ViewBag.outcomesViewModel = OutcomesViewModel;

            foreach (var investors in allAllowedInvestors)
            {
                AllowedInvestorsViewModel.Add(new AllowedInvestorViewModel { AllowedInvestorID = investors.AllowedInvestorID, AllowedInvestorName = investors.AllowedInvestorName, Assigned = startupAllowedInvestors.Contains(investors.AllowedInvestorID) });
            }

            ViewBag.AllowedInvestorsViewModel = AllowedInvestorsViewModel;

            foreach (var fundingDivision in allFundingDivisions)
            {
                if (!startupFundingDivisions.Contains(fundingDivision.FundingDivisionID))
                {
                    db.FundingDivisionStartups.Add(new FundingDivisionStartup { FundingDivisionID = fundingDivision.FundingDivisionID, StartupID = startup.StartupID, Percentage = 0 });

                }

            }
            db.SaveChanges();
            ////Model.FundingDivisionStartups.Where(fds => fds.StartupID.Equals(Model.StartupID)
            ViewBag.FundingDivisionsList = db.FundingDivisionStartups.Where(fs => fs.StartupID == startup.StartupID).ToList();
           
        }
        /******************************************************************************/
        //this function will be called if there is validation error, so can we add the latest selection of checkboxes
        private void PopulateAssignedCheckBoxsData(Models.IdeaCarrier.Startup startup, string[]  selectedWeaknesses, string[] selectedOutcomes, string[] selectedSharedToInvestors)
        {
            var allWeaknesses = db.TeamWeaknesses;
            var selectedWeaknessesHS = new HashSet<String>();
            var weaknessesViewModel = new List<TeamWeaknessesViewModel>();

            if (selectedWeaknesses != null)
            {
                selectedWeaknessesHS = new HashSet<String>(selectedWeaknesses);

            }

            var allOutcomes = db.Outcomes;
            var selectedOutcomesHS = new HashSet<String>();
            var OutcomesViewModel = new List<StartupOutcomeViewModel>();


            if (selectedOutcomes != null)
            {
                selectedOutcomesHS = new HashSet<String>(selectedOutcomes);

            }

            var allAllowedInvestors = db.AllowedInvestors;
            var selectedSharedToInvestorsHS = new HashSet<String>();
            var AllowedInvestorsViewModel = new List<AllowedInvestorViewModel>();


            if (selectedSharedToInvestors != null)
            {
                selectedSharedToInvestorsHS = new HashSet<String>(selectedSharedToInvestors);

            }

            foreach (var weakness in allWeaknesses)
            {
                weaknessesViewModel.Add(new TeamWeaknessesViewModel
                {
                    WeaknessID = weakness.TeamWeaknessID,
                    WeaknessName = weakness.TeamWeaknessName,
                    Assigned = selectedWeaknessesHS.Contains(weakness.TeamWeaknessID.ToString()) });
            }
            ViewBag.weaknessesViewModel = weaknessesViewModel;


            foreach (var outcome in allOutcomes)
            {
                OutcomesViewModel.Add(new StartupOutcomeViewModel { OutcomeID = outcome.OutcomeID, OutcomeName = outcome.OutcomeName, Assigned = selectedOutcomesHS.Contains(outcome.OutcomeID.ToString()) });
            }

            ViewBag.outcomesViewModel = OutcomesViewModel;

            foreach (var investors in allAllowedInvestors)
            {
                AllowedInvestorsViewModel.Add(new AllowedInvestorViewModel { AllowedInvestorID = investors.AllowedInvestorID, AllowedInvestorName = investors.AllowedInvestorName, Assigned = selectedSharedToInvestorsHS.Contains(investors.AllowedInvestorID.ToString()) });
            }

            ViewBag.AllowedInvestorsViewModel = AllowedInvestorsViewModel;

            /************************************************************************/          
            
            ViewBag.FundingDivisionsList = db.FundingDivisionStartups.Where(fs => fs.StartupID == startup.StartupID).ToList();

        }

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
        public ActionResult ChangeApproval(string id, string redirect)
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

            startup.Approved = !(startup.Approved);
            if (startup.Approved) startup.ApprovedByID = User.Identity.GetUserId(); //<-------------------------
            else startup.ApprovedByID = "";

            db.Entry(startup).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect))
            {
                if (redirect == "Details") return RedirectToAction("ProjectDetails", new { id });
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Unlock(string id, string redirect)
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
            //try
            startup.Locked = false;
            startup.Approved = false;
            startup.ApprovedByID = null;

            db.Entry(startup).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect))
            {
                if (redirect == "Details") return RedirectToAction("ProjectDetails", new { id });
            }

            return RedirectToAction("Index");
        }

        // GET: Startups/Reminder
        [Authorize(Roles = ("Admin"))]
        public ActionResult Reminder(string id, string subject, string message, string redirect) //id==startupId
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