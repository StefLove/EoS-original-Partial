using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EoS.Models;
using EoS.Models.Shared;
using EoS.Models.Investor;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Net.Mail;

namespace EoS.Controllers
{
    [Authorize]
    public class InvestmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Investments
        [Authorize(Roles = "Admin, Investor")]
        public ActionResult Index(string id, bool? matchable, string orderBy = "") //id==investorId
        {
            List<Investment> investmentProfiles = null;

            ViewBag.Matchable = null; //false;
            ViewBag.UserRole = "";
            ViewBag.InvestorId = "";
            ViewBag.InvestorUserName = "";
            ViewBag.InvestorExternalId = "";
            ViewBag.ActiveInvestor = null; //false;

            if (User.IsInRole(Role.Admin.ToString()))
            {
                ViewBag.Matchable = null;
                ViewBag.UserRole = Role.Admin.ToString();

                if (!string.IsNullOrEmpty(id)) //The Admin looks at a special Investor's investment profiles
                {
                    ApplicationUser investor = db.Users.Find(id);
                    bool investorIsARealInvestor = investor.Roles.Where(ur => ur.RoleId == db.Roles.Where(r => r.Name == Role.Investor.ToString()).FirstOrDefault().Id).Any(); //Role check
                    if (investor != null && investorIsARealInvestor)
                    {
                        ViewBag.InvestorId = id;
                        ViewBag.InvestorUserName = investor.UserName;
                        ViewBag.InvestorExternalId = investor.ExternalId;
                        ViewBag.ActiveInvestor = true; //investor.ActiveInvestor; //<---to be implemented

                        if (!matchable.HasValue)
                        {
                            investmentProfiles = investor.Investments.ToList(); //db.Investments.Where(i => i.UserId == id).ToList(); <-----
                        }
                        else if (matchable.Value)
                        {
                            ViewBag.Matchable = true;
                            //investmentProfiles = db.Investments.Where(i => i.UserId == id && (i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0))).OrderBy(i => i.InvestmentID).ToList();
                            investmentProfiles = investor.Investments.Where(i => i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID).ToList();
                        }
                        else
                        {
                            ViewBag.Matchable = false;
                            //investmentProfiles = db.Investments.Where(i => i.UserId == id && (!i.Locked || !i.Active || (i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) <= 0))).OrderBy(i => i.InvestmentID).ToList();
                            investmentProfiles = investor.Investments.Where(i => !i.Locked || !i.Active || (i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) <= 0)).OrderBy(i => i.InvestmentID).ToList();
                        }

                        //return View(investmentProfiles);
                    }
                }
                else //Show all Investment profiles
                {
                    if (!matchable.HasValue)
                    {
                        investmentProfiles = db.Investments.ToList();
                    }
                    else if (matchable.Value)
                    {
                        ViewBag.Matchable = true;
                        investmentProfiles = db.Investments.Where(i => i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID).ToList();
                    }
                    else
                    {
                        ViewBag.Matchable = false;
                        investmentProfiles = db.Investments.Where(i => !i.Locked || !i.Active || (i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) <= 0)).OrderBy(i => i.InvestmentID).ToList();
                    }
                    //.UserRole = Role.Admin.ToString();
                    //return View(investmentProfiles);
                }
            }
            else if (User.IsInRole(Role.Investor.ToString()))
            {
                string investorID = User.Identity.GetUserId();
                ApplicationUser investor = db.Users.Find(investorID);
                investmentProfiles = investor.Investments.ToList(); //db.Investments.Where(u => u.UserId == currentUser.Id).ToList(); //<---------!!!
                ViewBag.InvestorID = investorID;
                ViewBag.UserRole = Role.Investor.ToString();
                ViewBag.InvestorExternalId = investor.ExternalId;
                ViewBag.ActiveInvestor = true; //if investor.ActiveInvestor.HasValue <-------to be implemented
                //return View(investments);
            }

            if (investmentProfiles == null) investmentProfiles = db.Investments.ToList();

            //ViewBag.NoOfInvestmentProfiles = InvestmentProfiles.Count();

            if (!string.IsNullOrEmpty(orderBy))
                switch (orderBy.ToUpper())
                {
                    case "INVESTORUSERNAME": return View(investmentProfiles.OrderBy(iv => iv.User.UserName)); //break;
                    case "INVESTMENTID": return View(investmentProfiles.OrderBy(iv => iv.InvestmentID));
                    case "PROFILENAME": return View(investmentProfiles.OrderBy(iv => iv.ProfileName));
                    case "COUNTRY": return View(investmentProfiles.OrderBy(iv => iv.Country.CountryName));
                    case "SWEDISHREGION": return View(investmentProfiles.OrderBy(iv => iv.SwedishRegion?.RegionName));
                    case "PROFILEDOMAINNAME": return View(investmentProfiles.OrderBy(iv => iv.ProjectDomain?.ProjectDomainName));
                    case "MATCHMAKINGCOUNT": return View(investmentProfiles.OrderBy(iv => iv.MatchMakings?.Count()));
                    case "LASTSAVEDDATE": return View(investmentProfiles.OrderByDescending(iv => iv.LastSavedDate));
                    case "DUEDATE": return View(investmentProfiles.OrderByDescending(iv => iv.DueDate));
                    case "CREATEDDATE": return View(investmentProfiles.OrderByDescending(iv => iv.CreatedDate));
                    case "LASTLOCKEDDATE": return View(investmentProfiles.OrderByDescending(iv => iv.LastLockedDate));
                    case "ACTIVE": return View(investmentProfiles.OrderByDescending(iv => iv.Active));
                }

            return View(investmentProfiles);
        }

        // GET: Investments/ProfileDetails/5
        [Authorize(Roles = "Admin, Investor")]
        public ActionResult ProfileDetails(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment investmentProfile = db.Investments.Find(id);
            if (investmentProfile == null)
            {
                return HttpNotFound();
            }

            ViewBag.SwedishRegionName = "";
            if (investmentProfile.SwedishRegionID.HasValue) ViewBag.SwedishRegionName = db.SwedishRegions.Where(sr => sr.RegionID == investmentProfile.SwedishRegionID).FirstOrDefault().RegionName;

            ViewBag.UserRole = Role.Investor.ToString();

            if (User.IsInRole(Role.Admin.ToString()))
            {
                ViewBag.UserRole = Role.Admin.ToString();
                ViewBag.InvestorUserName = investmentProfile.User.UserName;
                return View(investmentProfile);
            }
            
            return View(investmentProfile);
        }

        // GET: Investments/AddNewProfile
        [Authorize(Roles = "Investor")]
        public ActionResult AddNewProfile()
        {

            AddNewProfileViewModel newProfileModel = new AddNewProfileViewModel()
            {
                InvestorMessage = db.InvestorMessages.FirstOrDefault().Text,
                CountryList = new SelectList(db.Countries, "CountryID", "CountryName"),
                SwedishCountryID = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single(),
                SwedishRegionList = new SelectList(db.SwedishRegions, "RegionID", "RegionName")
            };

            //these ViewBags for dropdownlist for create view that handle the selection options
            //ViewBag.CountryId = new SelectList(db.Countries, "CountryID", "CountryName");
            //ViewBag.RegionsId = new SelectList(db.SwedishRegions, "RegionID", "RegionName");
            //ViewBag.SwedishCountryId = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();
            //ViewBag.InvestorMessage = db.InvestorMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();

            return View(newProfileModel);
        }

        // POST: Investments/AddNewProfile
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Investor")] //InvestmentID,UserId,TeamMemberSize,ExtraProjectDomains,TeamExperience,CreatedDate,Locked
        //public async Task<ActionResult> AddNewProfile([Bind(Include = "ProfileName,CountryID,SwedishRegionID")] Investment investment, string submitCommand)
        public async Task<ActionResult> AddNewProfile(AddNewProfileViewModel newProfileModel, string submit_command)
        {
            //if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Submit"))
            //{   //if user submit the form, lock it for editable
            //this must be before ModelState.IsValid
            //    investment.Locked = true;
            //    TryValidateModel(investment);
            //}

            if (ModelState.IsValid)
            {
                string countryAbbreviation = db.Countries.Find(newProfileModel.CountryID).CountryAbbreviation;
                string newInvestmentProfileID = "IV" + countryAbbreviation + HelpFunctions.GetShortCode();
                //check if the code is already exist
                while (db.Investments.Any(i => i.InvestmentID == newInvestmentProfileID))
                {
                    newInvestmentProfileID = "IV" + countryAbbreviation + HelpFunctions.GetShortCode();
                }

                string UserId = User.Identity.GetUserId();
                string UserName = User.Identity.GetUserName();
                string IdentityName = User.Identity.Name; //==Email

                DateTime currentDate = DateTime.Now.Date;

                Investment newInvestmentProfile = new Investment()
                {
                    ProfileName = newProfileModel.ProfileName,
                    InvestmentID = newInvestmentProfileID,
                    UserId = UserId,
                    CountryID = newProfileModel.CountryID,
                    SwedishRegionID = newProfileModel.SwedishRegionID,
                    CreatedDate = currentDate,
                    LastSavedDate = currentDate,
                    //Locked = false,
                    Active = true
                };
                               
                //var userInvestments = db.Investments.Where(i => i.UserId == UserId);
                if (db.Investments.Where(i => i.UserId == UserId).Any()) newInvestmentProfile.DueDate = DateTime.Now.Date.AddDays(-1);

                db.Investments.Add(newInvestmentProfile);
                db.SaveChanges();
            
                //if (investment.Active) //<--------remove
                //{
                    //if (investment.Locked) ?
                    //investment.DueDate = DateTime.Now;

                //SendEmailToAdmins();

                    var smtpClients = db.SmtpClients.ToList();

                foreach (Models.SMTP.SmtpClient smtpClient in smtpClients) //send email to Admin
                {
                    if (smtpClient.Active)
                    {
                        try
                        {
                            //string bodyFormat = "\nNy investeringsprofil\n\nInvesterarens id: {0}\n\nInvesterarens Användarnamn: {1}\n\nProfilens id: {2}";
                            MailMessage message = new MailMessage
                            {
                                From = new MailAddress(IdentityName),
                                Subject = "Ny profil påbörjad av " + UserName,
                                Body = $"Ny investeringsprofil påbörjad.\n\nInvesterarens användarnamn: {UserName}\nInvesterarens id: {UserId}\n\nProfilens id: {newInvestmentProfileID}",
                                //string.Format(bodyFormat, User.Identity.GetUserId(), User.Identity.GetUserName(), investment.InvestmentID),
                                IsBodyHtml = false
                            };
                            message.To.Add(new MailAddress(smtpClient.MailRecipient));
                            //var Admins = db.Users.Where(u => u.Roles.Where(r1 => r1.RoleId == db.Roles.Where(r2 => r2.Name == Role.Admin.ToString()).FirstOrDefault().Id).Any()).ToList();
                            var Admins = db.Users.Where(u => u.Roles.Where(r1 => r1.RoleId == db.Roles.Where(r2 => r2.Name == Role.Admin.ToString()).FirstOrDefault().Id).Any() && u.Email != smtpClient.MailRecipient).ToList();
                            foreach (var admin in Admins) //Blind carbon copy to all Admins except smtpClient.MailRecipient
                            {
                                message.To.Add(admin.Email);
                            }

                            using (SmtpClient smtp = new SmtpClient())
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
                            }
                        }
                        catch { }
                    }
                }
                //}

                if (!string.IsNullOrEmpty(submit_command) && submit_command.StartsWith("Proceed")) //<-----------"Proceed to the Profile form"
                {
                    return RedirectToAction("ProfileForm", new { id = newInvestmentProfile.InvestmentID }); // "Investments",
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            //these ViewBags for dropdownlist for create view that handle the selection options
            //model.CountryId = new SelectList(db.Countries, "CountryID", "CountryName");
            //model.RegionsId = new SelectList(db.SwedishRegions, "RegionID", "RegionName");
            //model.SwedishCountryId = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();
            //model.InvestorMessage = db.InvestorMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();

            newProfileModel.InvestorMessage = db.InvestorMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();
            newProfileModel.CountryList = new SelectList(db.Countries, "CountryID", "CountryName");
            newProfileModel.SwedishCountryID = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();
            newProfileModel.SwedishRegionList = new SelectList(db.SwedishRegions, "RegionID", "RegionName");

            return View(newProfileModel);
        }

        // GET: Investments/ProfileForm/5
        [Authorize(Roles = "Admin, Investor")]
        public ActionResult ProfileForm(string id) //, string tab = "Profile")
        {
            if (User.IsInRole(Role.Admin.ToString())) return RedirectToAction("EditAdmin", new { id });

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment investmentProfile = db.Investments.Find(id);
            if (investmentProfile == null)
            {
                return HttpNotFound();
            }

            if (investmentProfile.Locked) return RedirectToAction("ProfileDetails", new { id });
            
            ViewBag.Message = "";
            //ViewBag.Tab = "";
            ViewBag.Unanswered = "";

            if (TempData.Any())
            {
                if (TempData.ContainsKey("message")) ViewBag.Message = TempData["message"] as string;
                //if (TempData.ContainsKey("tab")) ViewBag.Tab = TempData["tab"] as string;
                if (TempData.ContainsKey("unanswered")) ViewBag.Unanswered = TempData["unanswered"] as string;
                TempData.Clear();
            }

            //These viewBags to handel the current values of the investment properties if exist and provide by other selections
            //ViewBag.projectDomainIdEdit = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", investment.ProjectDomainID != null ? investment.ProjectDomainID : null);

            //Get the already checked value if it exists
            //PopulateAssignedCheckBoxsData(investment); //<------to be removed

            //return View(investment);
            return View(GetInvestmentProfileViewModel(investmentProfile));
        }

        // POST: Investments/ProfileForm/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Investor")] //CreatedDate,
        //public ActionResult ProfileForm([Bind(Include = "InvestmentID,UserId,CountryID,SwedishRegionID,ProfileName,ProjectDomainID,FutureFundingNeeded,EstimatedBreakEven,TeamMemberSizeMoreThanOne,TeamHasExperience,ActiveInvestor,PossibleIncomeStreams,LastSavedDate,DueDate,Locked,LastLockedDate,Active")] Investment investment,
        //    string[] SelectedFundingPhases, string[] SelectedFundingAmounts, string[] SelectedEstimatedExitPlans, string[] SelectedTeamSkills, string[] SelectedOutcomes, string[] SelectedInnovationLevels, string[] SelectedScalabilities, string ActiveTab/*, string submitCommand*/)
        public ActionResult ProfileForm(InvestmentProfilePostViewModel profilePostModel)
        {
            //Investment currentInvestment = db.Investments.Find(investment.InvestmentID);
            //if (db.Investments.Find(investment.InvestmentID).Locked)
            //if (IsLocked(investment.InvestmentID)) return RedirectToAction("ProfileDetails", new { id = investment.InvestmentID }); //<----------

            //if (!string.IsNullOrEmpty(submitCommand) && submitCommand.ToUpper().StartsWith("SUBMIT"))
            //{   //if user submit the form, lock it for editable
            //this must be before ModelState.IsValid

            //  investment.Locked = true;

            //
            //   if (selectedFundingAmount == null) //!selectedFundingAmount.Any()
            //   {
            //       ModelState.AddModelError("FundingAmounts", "Select at least one Funding amount");
            //   }
            //   if (selectedFundingPhases == null)
            //   {
            //       ModelState.AddModelError("FundingPhases", "Select at least one Funding phase");
            //   }
            //   if (selectedOutcomes == null)
            //   {
            //       ModelState.AddModelError("Outcomes", "Select at least one Outcome");
            //   }
            //   if (selectedInnovationLevels == null)
            //   {
            //       ModelState.AddModelError("InnovationLevels", "Select at least one Level of innovation");
            //   }
            //   if (selectedScalabilities == null)
            //   {
            //       ModelState.AddModelError("Scalabilities", "Select at least one Scalability");
            //   }
            //   if (selectedEstimatedExitPlans == null)
            //   {
            //       ModelState.AddModelError("EstimatedExitPlans", "Select at least one Estimated exit plan");
            //   } 
            //   TryValidateModel(investment);
            //}

            //InvestmentProfileGetViewModel getModel = new InvestmentProfileGetViewModel();

            bool updated = false;

            Investment investmentProfile = db.Investments.Find(profilePostModel.InvestmentID);

            if (ModelState.IsValid)
            {
                if (UpdateActiveTab(investmentProfile, profilePostModel))
                {
                    investmentProfile.LastSavedDate = DateTime.Now.Date;
                    db.Entry(investmentProfile).State = EntityState.Modified;
                    db.SaveChanges();
                    updated = true;
                }


                //Funding
                //UpdateFundingPhases(selectedFundingPhases, investmentProfile);
                //UpdateFundingAmount(selectedFundingAmount, investmentProfile);
                //Budget
                //UpdateEstimatedExitPlans(selectedEstimatedExitPlans, investmentProfile);
                //Team
                //UpdateTeamSkills(selectedTeamSkills, investmentProfile)
                //Outcome
                //UpdateInnovationLevels(selectedInnovationLevels, investmentProfile);
                //UpdateScalabilities(selectedScalabilities, investmentProfile);

                //To be removed
                //UpdateInvestmentCheckBoxesData(SelectedTeamSkills, SelectedFundingAmounts, SelectedFundingPhases, SelectedOutcomes, SelectedInnovationLevels, SelectedScalabilities, SelectedEstimatedExitPlans, investment);
                //selectedFundingPhases, selectedFundingAmount, selectedEstimatedExitPlans, selectedTeamSkills, selectedOutcomes, selectedInnovationLevels, selectedScalabilities))
                //DateTime currentDate = DateTime.Now.Date;

                //db.Entry(investmentProfile).State = EntityState.Modified;
                //db.SaveChanges();

                //if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Submit"))
                //currentInvestment = db.Investments.Find(investment.InvestmentID);

                //if (db.Investments.Find(investment.InvestmentID).Locked) return RedirectToAction("ProfileDetails", new { id = investment.InvestmentID });

            }
            else ModelState.AddModelError("", "Validation Error !!");

            //if (IsLocked(investment.InvestmentID)) return RedirectToAction("ProfileDetails", new { id = investment.InvestmentID });

            //ViewBag.Message = "";
            //ViewBag.Tab = profilePostModel.ActiveTab; //<-------------
            //ViewBag.Unanswered = "";

            //These viewBags to handel the current values of the investment properties if exist and provide by other selections

            //ViewBag.projectDomainIdEdit = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", investment.ProjectDomainID != null ? investment.ProjectDomainID : null); //<-------------simplify!!
            //getModel.ProjectDomainIdSelectList = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", investment.ProjectDomainID);

            //Get the already checked value again if it exists to pass the ViewBags
            //PopulateAssignedCheckBoxsData(investment, SelectedTeamSkills, SelectedFundingAmounts, SelectedFundingPhases, SelectedOutcomes, SelectedInnovationLevels, SelectedScalabilities, SelectedEstimatedExitPlans); //<--to be removed

            //return View(investment);

            return View(GetInvestmentProfileViewModel(investmentProfile, updated)); //, profilePostModel.ActiveTab, updated) //db.Investments.Find(profilePostModel.InvestmentID)
        }

        private InvestmentProfileViewModel GetInvestmentProfileViewModel(Investment investmentProfile, bool updated = false) //, string ActiveTab, bool updated = false)
        {
            if (investmentProfile == null) return new InvestmentProfileViewModel(); //return empty ViewModel
            
            return new InvestmentProfileViewModel() //InvestmentProfileGetViewModel()
            {
                InvestmentID = investmentProfile.InvestmentID,

                //Profile
                ProfileName = investmentProfile.ProfileName,
                ProfileDomainID = investmentProfile.ProjectDomainID,
                ProfileDomainList = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName"/*, investment.ProjectDomainID*/),

                //Funding
                FundingPhases = GetFundingPhases(investmentProfile),
                //FundingPhasesUnanswered = !investmentProfile.FundingPhases.Any(),
                FundingAmounts = GetFundingAmounts(investmentProfile),
                //FundingAmountsUnanswered = !investmentProfile.FundingAmounts.Any(),
                FutureFundingNeeded = investmentProfile.FutureFundingNeeded,

                //Budget
                EstimatedExitPlans = GetEstimatedExitPlans(investmentProfile),
                //EstimatedExitPlansUnanswered = !investmentProfile.EstimatedExitPlans.Any(),
                EstimatedBreakEven = investmentProfile.EstimatedBreakEven,
                PossibleIncomeStreams = investmentProfile.PossibleIncomeStreams,

                //Team
                TeamHasExperience = investmentProfile.TeamHasExperience,
                TeamMemberSizeMoreThanOne = investmentProfile.TeamMemberSizeMoreThanOne,
                ActiveInvestor = investmentProfile.ActiveInvestor,
                TeamSkills = GetTeamSkills(investmentProfile),
                //TeamSkillsUnanswered = !investmentProfile.TeamSkills.Any(),

                //Outcome
                Outcomes = GetOutcomes(investmentProfile),
                //OutcomesUnanswered = !investmentProfile.Outcomes.Any(),
                InnovationLevels = GetInnovationLevels(investmentProfile),
                //InnovationLevelsUnanswered = !investmentProfile.InnovationLevels.Any(),
                Scalabilities = GetScalabilities(investmentProfile),
                //ScalabiltiesUnanswered = !investmentProfile.investmentProfile.Scalabilities.Any(),

                Updated = updated

                //AllQuestionsAnswered = GetAllQuestionsAnswered(investmentProfile)
            };
        }

        //private bool GetAllQuestionsAnswered(Investment investmentProfile)
        //{
        //    return //investmentProfile.DueDate.HasValue &&

        //        !string.IsNullOrEmpty(investmentProfile.ProfileName) &&
        //        investmentProfile.ProjectDomainID.HasValue; //&&

        //        //investmentProfile.FundingPhases.Any() &&
        //        //investmentProfile.FundingAmounts.Any() &&
        //        ////investmentProfile.FutureFundingNeeded.HasValue &&

        //        //investmentProfile.EstimatedExitPlans.Any() &&
        //        //investmentProfile.EstimatedBreakEven.HasValue &&               
        //        //investmentProfile.PossibleIncomeStreams.HasValue &&

        //        ////investmentProfile.TeamMemberSizeMoreThanOne.HasValue &&
        //        ////investmentProfile.TeamHasExperience.HasValue &&
        //        ////investmentProfile.ActiveInvestor.HasValue &&
        //        //investmentProfile.TeamSkills.Any() &&

        //        //investmentProfile.Outcomes.Any() &&
        //        //investmentProfile.InnovationLevels.Any() &&
        //        //investmentProfile.Scalabilities.Any();
        //}

        private bool UpdateActiveTab(Investment investmentProfile, InvestmentProfilePostViewModel profilePostModel)
        {
            if (investmentProfile == null || profilePostModel == null) return false;

            bool updated = false;

            switch (profilePostModel.ActiveTab)
            {
                case "tab_Profile": updated = UpdateTabProfile(investmentProfile, profilePostModel); break;
                case "tab_Funding": updated = UpdateTabFunding(investmentProfile, profilePostModel); break;
                case "tab_Budget": updated = UpdateTabBudget(investmentProfile, profilePostModel); break;
                case "tab_Team": updated = UpdateTabTeam(investmentProfile, profilePostModel); break;
                case "tab_Outcome": updated = UpdateTabOutcome(investmentProfile, profilePostModel); break;
                default: break;
            }

            return updated;
        }

        private bool UpdateTabProfile(Investment investmentProfile, InvestmentProfilePostViewModel profilePostModel)
        {
            bool updated = false;

            if (investmentProfile.ProfileName != profilePostModel.ProfileName)
            {
                investmentProfile.ProfileName = profilePostModel.ProfileName;
                updated = true;
            }

            if (investmentProfile.ProjectDomainID != profilePostModel.ProfileDomainID)
            {
                investmentProfile.ProjectDomainID = profilePostModel.ProfileDomainID;
                updated = true;
            }

            return updated;
        }

        private bool UpdateTabFunding(Investment investmentProfile, InvestmentProfilePostViewModel profilePostModel)
        {
            bool updated = false;
            
            updated = UpdateFundingPhases(profilePostModel.SelectedFundingPhaseIDs, investmentProfile);
            //if (UpdateTeamSkills(profilePostModel.SelectedTeamSkillIDs, investmentProfile) && !updated) updated = true;
           if(UpdateFundingAmounts(profilePostModel.SelectedFundingAmountIDs, investmentProfile) && !updated) updated= true;

            if (investmentProfile.FutureFundingNeeded != profilePostModel.FutureFundingNeeded)
            {
                investmentProfile.FutureFundingNeeded = profilePostModel.FutureFundingNeeded;
                updated = true;
            }

            return updated;
        }

        private bool UpdateFundingPhases(string[] selectedFundingPhaseIDs, Investment investmentProfile) //<-----------finished
        {
            bool updated = false;

            if (selectedFundingPhaseIDs != null)
            {
                List<int> selectedFundingPhaseIDList = Array.ConvertAll(selectedFundingPhaseIDs, s => int.Parse(s)).ToList();

                List<FundingPhase> profileFundingPhases = investmentProfile.FundingPhases.ToList();
                foreach (FundingPhase profileFundingPhase in profileFundingPhases)
                {
                    if (!selectedFundingPhaseIDList.Contains(profileFundingPhase.FundingPhaseID))
                    {
                        investmentProfile.FundingPhases.Remove(profileFundingPhase);
                        updated = true;
                    }
                }

                List<FundingPhase> fundingPhases = db.FundingPhases.ToList();
                FundingPhase fundingPhase = null;
                foreach (int selectedFundingPhaseID in selectedFundingPhaseIDList)
                {
                    fundingPhase = fundingPhases.Where(fa => fa.FundingPhaseID == selectedFundingPhaseID).FirstOrDefault();

                    if (!investmentProfile.FundingPhases.Contains(fundingPhase))
                    {
                        investmentProfile.FundingPhases.Add(fundingPhase);
                        updated = true;
                    }
                }
            }
            else //if (selectedFundingPhaseIDs == null)
            {
                investmentProfile.FundingPhases.Clear();
                updated = true;
            }

            //if (selectedFundingPhaseIDs != null)
            //{
            //    List<int> selectedFundingPhaseIDList = Array.ConvertAll(selectedFundingPhaseIDs, s => int.Parse(s)).ToList();

            //    List<int> currentFundingPhaseIDList = investmentProfile.FundingPhases.Select(fp => fp.FundingPhaseID).ToList();

            //    List<Models.Shared.FundingPhase> fundingPhases = db.FundingPhases.ToList();

            //    foreach (var phase in fundingPhases)
            //    {
            //        if (selectedFundingPhaseIDList.Contains(phase.FundingPhaseID) && !currentFundingPhaseIDList.Contains(phase.FundingPhaseID))
            //        {
            //            investmentProfile.FundingPhases.Add(phase);
            //            updated = true;
            //        }
            //        else if (currentFundingPhaseIDList.Contains(phase.FundingPhaseID) && investmentProfile.FundingPhases.Remove(phase))
            //            updated = true;
            //    }
            //}
            //else if (investmentProfile.FundingPhases.Any())
            //{
            //    List<Models.Shared.FundingPhase> fundingPhases = db.FundingPhases.ToList();

            //    foreach (var phase in fundingPhases)
            //    {
            //        if (investmentProfile.FundingPhases.Remove(phase)) updated = true;
            //    }
            //}

            return updated;
        }

        private bool UpdateFundingAmounts(string[] selectedFundingAmountIDs, Investment investmentProfile) //<---------finished
        {
            bool updated = false;

            if (selectedFundingAmountIDs != null)
            {
                List<int> selectedFundingAmountIDList = Array.ConvertAll(selectedFundingAmountIDs, s => int.Parse(s)).ToList();

                List<FundingAmount> profileFundingAmounts = investmentProfile.FundingAmounts.ToList();
                foreach (FundingAmount profileFundingAmount in profileFundingAmounts)
                {
                    if (!selectedFundingAmountIDList.Contains(profileFundingAmount.FundingAmountID))
                    {
                        investmentProfile.FundingAmounts.Remove(profileFundingAmount);
                        updated = true;
                    }
                }

                List<FundingAmount> fundingAmounts = db.FundingAmounts.ToList();
                FundingAmount fundingAmount = null;
                foreach (int selectedFundingAmountID in selectedFundingAmountIDList)
                {
                    fundingAmount = fundingAmounts.Where(fa => fa.FundingAmountID == selectedFundingAmountID).FirstOrDefault();

                    if (!investmentProfile.FundingAmounts.Contains(fundingAmount))
                    {
                        investmentProfile.FundingAmounts.Add(fundingAmount);
                        updated = true;
                    }
                }
            }
            else //if (selectedFundingAmountIDs == null)
            {
                investmentProfile.FundingAmounts.Clear();
                updated = true;
            }

            //if (selectedFundingAmountIDs != null)
            //{
            //    List<int> selectedFundingAmountIDList = Array.ConvertAll(selectedFundingAmountIDs, s => int.Parse(s)).ToList();

            //    List<int> currentFundingAmountIDList = investmentProfile.FundingAmounts.Select(fa => fa.FundingAmountID).ToList();

            //    List<Models.Shared.FundingAmount> fundingAmounts = db.FundingAmounts.ToList();

            //    foreach (var amount in fundingAmounts)
            //    {
            //        if (selectedFundingAmountIDList.Contains(amount.FundingAmountID) && !currentFundingAmountIDList.Contains(amount.FundingAmountID))
            //        {
            //            investmentProfile.FundingAmounts.Add(amount);
            //            updated = true;
            //        }
            //        else if (currentFundingAmountIDList.Contains(amount.FundingAmountID) && investmentProfile.FundingAmounts.Remove(amount))
            //            updated = true;
            //    }
            //}
            //else if (investmentProfile.FundingAmounts.Any())
            //{
            //    List<Models.Shared.FundingAmount> fundingAmounts = db.FundingAmounts.ToList();

            //    foreach (var amount in fundingAmounts)
            //    {
            //        if (investmentProfile.FundingAmounts.Remove(amount)) updated = true;
            //    }
            //}

            return updated;
        }

        private bool UpdateTabBudget(Investment investmentProfile, InvestmentProfilePostViewModel profilePostModel/*, string[] selectedEstimatedExitPlans*/)
        {
            bool updated = false;
            
            updated = UpdateEstimatedExitPlans(profilePostModel.SelectedEstimatedExitPlanIDs, investmentProfile);

            if (investmentProfile.EstimatedBreakEven != profilePostModel.EstimatedBreakEven)
            {
                investmentProfile.EstimatedBreakEven = profilePostModel.EstimatedBreakEven;
                updated = true;
            }

            if (investmentProfile.PossibleIncomeStreams != profilePostModel.PossibleIncomeStreams)
            {
                investmentProfile.PossibleIncomeStreams = profilePostModel.PossibleIncomeStreams;
                updated = true;
            }

            return updated;
        }

        private bool UpdateEstimatedExitPlans(string[] selectedEstimatedExitPlanIDs, Investment investmentProfile) //<-------finished
        {
            bool updated = false;

            if (selectedEstimatedExitPlanIDs != null)
            {
                List<int> selectedEstimatedExitplanIDList = Array.ConvertAll(selectedEstimatedExitPlanIDs, s => int.Parse(s)).ToList();

                List<EstimatedExitPlan> profileEstimatedExitplans = investmentProfile.EstimatedExitPlans.ToList();
                foreach (EstimatedExitPlan profileEstimatedExitplan in profileEstimatedExitplans)
                {
                    if (!selectedEstimatedExitplanIDList.Contains(profileEstimatedExitplan.EstimatedExitPlanID))
                    {
                        investmentProfile.EstimatedExitPlans.Remove(profileEstimatedExitplan);
                        updated = true;
                    }
                }

                List<EstimatedExitPlan> estimatedExitplans = db.EstimatedExitPlans.ToList();
                EstimatedExitPlan estimatedExitplan = null;
                foreach (int selectedEstimatedExitplanID in selectedEstimatedExitplanIDList)
                {
                    estimatedExitplan = estimatedExitplans.Where(eep => eep.EstimatedExitPlanID == selectedEstimatedExitplanID).FirstOrDefault();

                    if (!investmentProfile.EstimatedExitPlans.Contains(estimatedExitplan))
                    {
                        investmentProfile.EstimatedExitPlans.Add(estimatedExitplan);
                        updated = true;
                    }
                }
            }
            else //if (selectedEstimatedExitPlanIDs == null)
            {
                investmentProfile.EstimatedExitPlans.Clear();
                updated = true;
            }

            //if (selectedEstimatedExitPlanIDs != null)
            //{
            //    List<int> selectedEstimatedExitPlanIDList = Array.ConvertAll(selectedEstimatedExitPlanIDs, s => int.Parse(s)).ToList();

            //    List<int> currentEstimatedExitPlanIDList = investmentProfile.EstimatedExitPlans.Select(eep => eep.EstimatedExitPlanID).ToList();

            //    List<Models.Shared.EstimatedExitPlan> estimatedExitPlans = db.EstimatedExitPlans.ToList();

            //    foreach (var exitPlan in estimatedExitPlans)
            //    {
            //        if (selectedEstimatedExitPlanIDList.Contains(exitPlan.EstimatedExitPlanID) && !currentEstimatedExitPlanIDList.Contains(exitPlan.EstimatedExitPlanID))
            //        {
            //            investmentProfile.EstimatedExitPlans.Add(exitPlan);
            //            updated = true;
            //        }
            //        else if (currentEstimatedExitPlanIDList.Contains(exitPlan.EstimatedExitPlanID) && investmentProfile.EstimatedExitPlans.Remove(exitPlan))
            //            updated = true;
            //    }
            //}
            //else if (investmentProfile.EstimatedExitPlans.Any())
            //{
            //    List<Models.Shared.EstimatedExitPlan> estimatedExitPlans = db.EstimatedExitPlans.ToList();

            //    foreach (var plan in estimatedExitPlans)
            //    {
            //        if (investmentProfile.EstimatedExitPlans.Remove(plan)) updated = true;
            //    }
            //}

            return updated;
        }

        private bool UpdateTabTeam(Investment investmentProfile, InvestmentProfilePostViewModel profilePostModel)
        {
            bool updated = false;

            if (investmentProfile.TeamMemberSizeMoreThanOne != profilePostModel.TeamMemberSizeMoreThanOne)
            {
                investmentProfile.TeamMemberSizeMoreThanOne = profilePostModel.TeamMemberSizeMoreThanOne;
                updated = true;
            }

            if (investmentProfile.TeamHasExperience != profilePostModel.TeamHasExperience)
            {
                investmentProfile.TeamHasExperience = profilePostModel.TeamHasExperience;
                updated = true;
            }

            if (investmentProfile.ActiveInvestor != profilePostModel.ActiveInvestor)
            {
                investmentProfile.ActiveInvestor = profilePostModel.ActiveInvestor;
                updated = true;
            }

            if (UpdateTeamSkills(profilePostModel.SelectedTeamSkillIDs, investmentProfile) && !updated) updated = true;

            return updated;
        }

        private bool UpdateTeamSkills(string[] selectedTeamSkillIDs, Investment investmentProfile) //<---------finished
        {
            bool updated = false;
            
            if (selectedTeamSkillIDs != null)
            {
                List<int> selectedTeamSkillIDList = Array.ConvertAll(selectedTeamSkillIDs, s => int.Parse(s)).ToList();

                List<TeamSkill> profileTeamSkills = investmentProfile.TeamSkills.ToList();
                foreach (TeamSkill profileTeamSkill in profileTeamSkills)
                {
                    if (!selectedTeamSkillIDList.Contains(profileTeamSkill.SkillID))
                    {
                        investmentProfile.TeamSkills.Remove(profileTeamSkill);
                        updated = true;
                    }
                }

                List<TeamSkill> teamSkills = db.TeamSkills.ToList();
                TeamSkill teamSkill = null;
                foreach (int selectedTeamSkillID in selectedTeamSkillIDList)
                {
                    teamSkill = teamSkills.Where(ts => ts.SkillID == selectedTeamSkillID).FirstOrDefault();

                    if (!investmentProfile.TeamSkills.Contains(teamSkill))
                    {
                        investmentProfile.TeamSkills.Add(teamSkill);
                        updated = true;
                    }
                }
            }
            else //if (selectedTeamSkillIDs == null)
            {
                investmentProfile.TeamSkills.Clear();
                updated = true;
            }

            return updated;
        }

        private bool UpdateTabOutcome(Investment investmentProfile, InvestmentProfilePostViewModel profilePostModel)
        {
            bool updated = false;

            updated = UpdateOutcomes(profilePostModel.SelectedOutcomeIDs, investmentProfile);

            if (UpdateInnovationLevels(profilePostModel.SelectedInnovationLevelIDs, investmentProfile) && !updated) updated = true;

            if (UpdateScalabilities(profilePostModel.SelectedScalabilityIDs, investmentProfile) && !updated) updated = true;

            return updated;
        }

        private bool UpdateOutcomes(string[] selectedOutcomeIDs, Investment investmentProfile) //<-------finished
        {
            bool updated = false;

            if (selectedOutcomeIDs != null)
            {
                List<int> selectedOutcomeIDList = Array.ConvertAll(selectedOutcomeIDs, s => int.Parse(s)).ToList();

                List<Models.Shared.Outcome> profileOutcomes = investmentProfile.Outcomes.ToList();
                foreach (Models.Shared.Outcome profileOutcome in profileOutcomes)
                {
                    if (!selectedOutcomeIDList.Contains(profileOutcome.OutcomeID))
                    {
                        investmentProfile.Outcomes.Remove(profileOutcome);
                        updated = true;
                    }
                }
                
                List<Models.Shared.Outcome> outcomes = db.Outcomes.ToList();
                Models.Shared.Outcome outcome = null;
                foreach (int selectedOutcomeID in selectedOutcomeIDList)
                {
                    outcome = outcomes.Where(o => o.OutcomeID == selectedOutcomeID).FirstOrDefault();

                    if (!investmentProfile.Outcomes.Contains(outcome))
                    {
                        investmentProfile.Outcomes.Add(outcome);
                        updated = true;
                    }
                }
            }
            else //if (selectedOutcomeIDs == null)
            {
                investmentProfile.Outcomes.Clear();
                updated = true;
            }

            //if (selectedOutcomeIDs != null)
            //{
            //    List<int> selectedOutcomeIDList = Array.ConvertAll(selectedOutcomeIDs, s => int.Parse(s)).ToList();

            //    List<int> currentOutcomeIDList = investmentProfile.Outcomes.Select(o => o.OutcomeID).ToList();

            //    List<Models.Shared.Outcome> outcomes = db.Outcomes.ToList();

            //    foreach (var outcome in outcomes)
            //    {
            //        if (selectedOutcomeIDList.Contains(outcome.OutcomeID) && !currentOutcomeIDList.Contains(outcome.OutcomeID))
            //        {
            //            investmentProfile.Outcomes.Add(outcome);
            //            updated = true;
            //            //}
            //        }
            //        else if (currentOutcomeIDList.Contains(outcome.OutcomeID) && investmentProfile.Outcomes.Remove(outcome))
            //        {
            //            updated = true;
            //        }
            //    }
            //}
            //else if (investmentProfile.Outcomes.Any())
            //{
            //    List<Models.Shared.Outcome> outcomes = db.Outcomes.ToList();

            //    foreach (var outcome in outcomes)
            //    {
            //        if (investmentProfile.Outcomes.Remove(outcome)) updated = true;
            //    }
            //}

            return updated;
        }

        private bool UpdateInnovationLevels(string[] selectedInnovationLevelIDs, Investment investmentProfile) //<------finished
        {
            bool updated = false;

            if (selectedInnovationLevelIDs != null)
            {
                List<int> selectedInnovationLevelIDList = Array.ConvertAll(selectedInnovationLevelIDs, s => int.Parse(s)).ToList();

                List<InnovationLevel> profileInnovationLevels = investmentProfile.InnovationLevels.ToList();
                foreach (InnovationLevel profileInnovationLevel in profileInnovationLevels)
                {
                    if (!selectedInnovationLevelIDList.Contains(profileInnovationLevel.InnovationLevelID))
                    {
                        investmentProfile.InnovationLevels.Remove(profileInnovationLevel);
                        updated = true;
                    }
                }

                List<InnovationLevel> innovationLevels = db.InnovationLevels.ToList();
                InnovationLevel innovationLevel = null;
                foreach (int selectedInnovationLevelID in selectedInnovationLevelIDList)
                {
                    innovationLevel = innovationLevels.Where(il => il.InnovationLevelID == selectedInnovationLevelID).FirstOrDefault();

                    if (!investmentProfile.InnovationLevels.Contains(innovationLevel))
                    {
                        investmentProfile.InnovationLevels.Add(innovationLevel);
                        updated = true;
                    }
                }
            }
            else //if (selectedInnovationLevelIDs == null)
            {
                investmentProfile.InnovationLevels.Clear();
                updated = true;
            }

            //if (selectedInnovationLevelIDs != null) //==> UpdateSelectedInnovationLevels(Investment investmentProfile)
            //{
            //    List<int> selectedInnovationLevelIDList = Array.ConvertAll(selectedInnovationLevelIDs, s => int.Parse(s)).ToList();

            //    //this is the current or previous saved selections
            //    //Investment currentInnovationLevelsInvestment = db.Investments.Include(nl => nl.InnovationLevels).Where(i => i.InvestmentID == investmentProfile.InvestmentID).Single();

            //    List<int> currentInnovationLevelIDList = investmentProfile.InnovationLevels.Select(il => il.InnovationLevelID).ToList();

            //    List<Models.Shared.InnovationLevel> innovationLevels = db.InnovationLevels.ToList();

            //    foreach (var innovationLevel in innovationLevels)
            //    {
            //        if (selectedInnovationLevelIDList.Contains(innovationLevel.InnovationLevelID) && !currentInnovationLevelIDList.Contains(innovationLevel.InnovationLevelID))
            //        {
            //            investmentProfile.InnovationLevels.Add(innovationLevel);
            //            updated = true;
            //            //}
            //        }
            //        else if (currentInnovationLevelIDList.Contains(innovationLevel.InnovationLevelID) && investmentProfile.InnovationLevels.Remove(innovationLevel))
            //            updated = true;
            //    }
            //}
            //else if (investmentProfile.InnovationLevels != null && investmentProfile.InnovationLevels.Any()) //<---2018-08-08 remove?
            //{
            //    List<Models.Shared.InnovationLevel> innovationLevels = db.InnovationLevels.ToList();

            //    foreach (var innovationLevel in innovationLevels)
            //    {
            //        if (investmentProfile.InnovationLevels.Remove(innovationLevel)) updated = true;
            //    }
            //}

            return updated;
        }

        private bool UpdateScalabilities(string[] selectedScalabilityIDs, Investment investmentProfile) //<-------finished
        {
            bool updated = false;

            if (selectedScalabilityIDs != null)
            {
                List<int> selectedScalabilityIDList = Array.ConvertAll(selectedScalabilityIDs, s => int.Parse(s)).ToList();

                List<Scalability> profileScalabilities = investmentProfile.Scalabilities.ToList();
                foreach (Scalability profileScalibility in profileScalabilities)
                {
                    if (!selectedScalabilityIDList.Contains(profileScalibility.ScalabilityID))
                    {
                        investmentProfile.Scalabilities.Remove(profileScalibility);
                        updated = true;
                    }
                }

                List<Scalability> scalabilities = db.Scalabilities.ToList();
                Scalability scalability = null;
                foreach (int selectedScalabilityID in selectedScalabilityIDList)
                {
                    scalability = scalabilities.Where(s => s.ScalabilityID == selectedScalabilityID).FirstOrDefault();

                    if (!investmentProfile.Scalabilities.Contains(scalability))
                    {
                        investmentProfile.Scalabilities.Add(scalability);
                        updated = true;
                    }
                }
            }
            else //if (selectedScalabilityIDs == null)
            {
                investmentProfile.Scalabilities.Clear();
                updated = true;
            }

            //if (selectedScalabilityIDs != null)
            //{
            //    List<int> selectedScalabilityIDList = Array.ConvertAll(selectedScalabilityIDs, s => int.Parse(s)).ToList();

            //    List<int> currentScalabilitieIDList = investmentProfile.Scalabilities.Select(sc => sc.ScalabilityID).ToList();

            //    List<Models.Shared.Scalability> scalabilities = db.Scalabilities.ToList();

            //    foreach (var scalability in scalabilities)
            //    {
            //        if (selectedScalabilityIDList.Contains(scalability.ScalabilityID) && !currentScalabilitieIDList.Contains(scalability.ScalabilityID))
            //        {
            //            investmentProfile.Scalabilities.Add(scalability);
            //            updated = true;
            //        }
            //        else if (currentScalabilitieIDList.Contains(scalability.ScalabilityID) && investmentProfile.Scalabilities.Remove(scalability))
            //        {
            //            updated = true;
            //        }
            //    }
            //}
            //else if (investmentProfile.Scalabilities != null && investmentProfile.Scalabilities.Any()) //2018-08-08
            //{
            //    List<Models.Shared.Scalability> scalabilities = db.Scalabilities.ToList();

            //    foreach (var scalability in scalabilities)
            //    {
            //        if (investmentProfile.Scalabilities.Remove(scalability)) updated = true;
            //    }
            //}

            return updated;
        }

        //[HttpGet]
        [Authorize(Roles = "Investor")]
        public ActionResult SubmitProfileForm(string id, bool cancel = false, string redirect = "", string redirectTab = "")
        {
            if (cancel)
            {
                TempData["message"] = "Submission of form cancelled!";
                //TempData["tab"] = redirectTab;
                if (string.IsNullOrEmpty(redirectTab)) return RedirectToAction("ProfileForm", new { id });
                else return Redirect(Url.Action("ProfileForm", new { id }) + "#" + redirectTab);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment investmentProfile = db.Investments.Find(id);

            if (investmentProfile == null)
            {
                return HttpNotFound();
            }
            //Profile
            if (string.IsNullOrEmpty(investmentProfile.ProfileName))
            {
                TempData["message"] = "Profile name can't be empty!";
                //TempData["tab"] = "Profile";
                TempData["unanswered"] = "ProfileName";
                //return RedirectToAction("ProfileForm", new { id });
                return Redirect(Url.Action("ProfileForm", new { id }) + "#Profile");

                //return RedirectToAction("ProfileForm", new { id, tab = "Project" });
                //return Redirect("~/Investments/ProfileForm/" + investment.InvestmentID + "#Project");
            }
            if (!investmentProfile.ProjectDomainID.HasValue)
            {
                TempData["message"] = "Select the Profile domain!";
                //TempData["tab"] = "Profile";
                TempData["unanswered"] = "ProfileDomain";
                //return RedirectToAction("ProfileForm", new { id });
                return Redirect(Url.Action("ProfileForm", new { id }) + "#Profile");
            }
            //Funding
            if (investmentProfile.FundingPhases == null || !investmentProfile.FundingPhases.Any())
            {
                TempData["message"] = "Select at least one Funding phase!";
                //TempData["tab"] = "Funding";
                TempData["unanswered"] = "FundingPhases";
                //return RedirectToAction("ProfileForm", new { id });
                return Redirect(Url.Action("ProfileForm", new { id }) + "#Funding");
            }
            if (investmentProfile.FundingAmounts == null || !investmentProfile.FundingAmounts.Any())
            {
                TempData["message"] = "Select at least one Funding amount!";
                //TempData["tab"] = "Funding";
                TempData["unanswered"] = "FundingAmounts";
                //return RedirectToAction("ProfileForm", new { id });
                return Redirect(Url.Action("ProfileForm", new { id }) + "#Funding");
            }
            //Budget
            if (investmentProfile.EstimatedExitPlans == null || !investmentProfile.EstimatedExitPlans.Any())
            {
                TempData["message"] = "Select at least one Estimated exit plan!";
                //TempData["tab"] = "Budget";
                TempData["unanswered"] = "EstimatedExitPlans";
                //return RedirectToAction("ProfileForm", new { id });
                return Redirect(Url.Action("ProfileForm", new { id }) + "#Budget");
            }
            if (!investmentProfile.EstimatedBreakEven.HasValue)
            {
                TempData["message"] = "When will Estimated break even happen?";
                //TempData["tab"] = "Budget";
                TempData["unanswered"] = "EstimatedBreakEven";
                //return RedirectToAction("ProfileForm", new { id });
                return Redirect(Url.Action("ProfileForm", new { id }) + "#Budget");
            }
            if (!investmentProfile.PossibleIncomeStreams.HasValue)
            {
                TempData["message"] = "How many Possible income streams?";
                //TempData["tab"] = "Budget";
                TempData["unanswered"] = "PossibleIncomeStreams";
                //return RedirectToAction("ProfileForm", new { id });
                return Redirect(Url.Action("ProfileForm", new { id }) + "#Budget");
            }
            //Team
            if (investmentProfile.TeamSkills == null || !investmentProfile.TeamSkills.Any())
            {
                TempData["message"] = "Select at least one Team skill!";
                //TempData["tab"] = "Team";
                TempData["unanswered"] = "TeamSkills";
                //return RedirectToAction("ProfileForm", new { id });
                return Redirect(Url.Action("ProfileForm", new { id }) + "#Team");
            }
            //Outcome
            if (investmentProfile.Outcomes == null || !investmentProfile.Outcomes.Any())
            {
                TempData["message"] = "Select at least one Outcome!";
                //TempData["tab"] = "Outcome";
                TempData["unanswered"] = "Outcomes";
                //return RedirectToAction("ProfileForm", new { id });
                return Redirect(Url.Action("ProfileForm", new { id }) + "#Outcome");
            }
            if (investmentProfile.InnovationLevels == null || !investmentProfile.InnovationLevels.Any())
            {
                TempData["message"] = "Select at least one Level of innovation!";
                //TempData["tab"] = "Outcome";
                TempData["unanswered"] = "InnovationLevels";
                //return RedirectToAction("ProfileForm", new { id });
                return Redirect(Url.Action("ProfileForm", new { id }) + "#Outcome");
            }
            if (investmentProfile.Scalabilities == null || !investmentProfile.Scalabilities.Any())
            {
                TempData["message"] = "Select at least one Scalability!";
                //TempData["tab"] = "Outcome";
                TempData["unanswered"] = "Scalabilities";
                //return RedirectToAction("ProfileForm", new { id });
                return Redirect(Url.Action("ProfileForm", new { id }) + "#Outcome");
            }

            investmentProfile.Locked = true;
            investmentProfile.LastLockedDate = DateTime.Now.Date;

            db.Entry(investmentProfile).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect)) return RedirectToAction(redirect, new { id });

            return RedirectToAction("ProfileDetails", new { id });
        }

        // GET: Investments/EditAdmin/5
        [Authorize(Roles = "Admin")]
        public ActionResult EditAdmin(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment investmentProfile = db.Investments.Find(id);

            if (investmentProfile == null)
            {
                return HttpNotFound();
            }

            if (!investmentProfile.DueDate.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);  //RedirectToAction("ProfileDetails", new { id });  //<---Doesn't work?
            }

            InvestmentEditAdminViewModel editAdminmodel = new InvestmentEditAdminViewModel //ProfileFormAdminViewModel
            {
                InvestmentId = investmentProfile.InvestmentID,
                InvestorName = investmentProfile.User.UserName,
                DueDate = investmentProfile.DueDate,
                Locked = investmentProfile.Locked
            };

            return View(editAdminmodel);
        }

        // POST: Investments/EditAdmin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] /*[Bind(Include = "StartupID,ProjectSummary,Locked,Approved")]*/
        public ActionResult EditAdmin(InvestmentEditAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                Investment investmentProfile = db.Investments.Find(model.InvestmentId);
                if (investmentProfile == null)
                {
                    return HttpNotFound();
                }

                //if (model.DueDate.HasValue) investment.DueDate = model.DueDate.Value.Date;
                //else if (investment.DueDate.HasValue) investment.DueDate = null;

                //if (investment.DueDate != model.DueDate) //<---------------------------------!!!!
                //{
                //    investment.DueDate = model.DueDate;
                //    db.Entry(investment).State = EntityState.Modified;
                //    db.SaveChanges();
                //}

                bool updated = false;

                if (investmentProfile.DueDate != model.DueDate)
                {
                    investmentProfile.DueDate = model.DueDate;
                    updated = true;
                }

                if (investmentProfile.Locked != model.Locked)
                {
                    investmentProfile.Locked = model.Locked;
                    updated = true;
                }

                if (updated)
                {
                    db.Entry(investmentProfile).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return RedirectToAction("ProfileDetails", new { id = investmentProfile.InvestmentID });
            }

            return View(model);
        }

        // GET: Investments/RemoveProfile/5
        [Authorize(Roles = "Admin,Investor")]
        public ActionResult RemoveProfile(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Investment investmentProfile = db.Investments.Find(id);
            if (investmentProfile == null)
            {
                return HttpNotFound();
            }
            return View(investmentProfile);
        }

        // POST: Investments/RemoveProfile/5
        [Authorize(Roles = "Admin, Investor")]
        [HttpPost, ActionName("RemoveProfile")]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveConfirmed(string id)
        {
            Investment investmentProfile = db.Investments.Find(id);
            db.Investments.Remove(investmentProfile);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
  
        
        //bool  to be deleted
        //private void UpdateInvestmentCheckBoxesData(string[] selectedFundingPhases, string[] selectedFundingAmounts, string[] selectedEstimatedExitPlans,
        //    string[] selectedTeamSkills, string[] selectedOutcomes, string[] selectedInnovationLevels, string[] selectedScalabilities, Investment investment)
        //{
        //    //bool updated = false;

        //    //if (activeTab =="tab_Budget") {

        //    if (selectedEstimatedExitPlans != null) //==> UpdateSelectedEstimatedExitPlans(Investment investmentProfile)
        //    {
        //        //this is the new selection list
        //        var selectedEstimatedExitPlansHS = new HashSet<string>(selectedEstimatedExitPlans);
        //        //List<int> selectedEstimatedExitPlanIDs = Array.ConvertAll(SelectedEstimatedExitPlans, s => int.Parse(s)).ToList();
        //        //this is the current or previous saved selections
        //        Investment currentEstimatedExitPlansInvestment = db.Investments.Include(e => e.EstimatedExitPlans).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        var investmentCurrentEstimatedExitPlans = new HashSet<int>(currentEstimatedExitPlansInvestment.EstimatedExitPlans.Select(e => e.EstimatedExitPlanID));

        //        foreach (var plan in db.EstimatedExitPlans)
        //        {
        //            if (selectedEstimatedExitPlansHS.Contains(plan.EstimatedExitPlanID.ToString()))
        //            {
        //                //if the selection not in previous selections, add it
        //                if (!investmentCurrentEstimatedExitPlans.Contains(plan.EstimatedExitPlanID))
        //                {
        //                    investment.EstimatedExitPlans.Add(plan);
        //                    //updated = true;
        //                }
        //            }
        //            else
        //            {
        //                //if the selection in previous selections but not in the new selected list, remove it 
        //                if (investmentCurrentEstimatedExitPlans.Contains(plan.EstimatedExitPlanID))
        //                {
        //                    investment.EstimatedExitPlans.Remove(plan);
        //                    //updated = true;
        //                }
        //            }
        //        }
        //    }
        //    else if (investment.EstimatedExitPlans != null) //2018-08-08 <--------------------???
        //    {
        //        //Delete Any previous selection if any
        //        //this is the current or previous saved selections
        //        //var currentEstimatedExitPlans = db.Investments.Include(e => e.EstimatedExitPlans).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        //var investomentCurrentEstimatedExitPlans = new HashSet<int>(currentEstimatedExitPlans.EstimatedExitPlans.Select(e => e.EstimatedExitPlanID));

        //        foreach (var plan in db.EstimatedExitPlans)
        //        {
        //            investment.EstimatedExitPlans.Remove(plan);
        //            //updated = true;
        //        }
        //    }
    
        //    //********************************************************************************//

        //    //if (activeTab = "tab_Funding")

        //    if (selectedFundingPhases != null) //==> UpdateSelectedFundingPhases(Investment investmentProfile)
        //    {
        //        //this is the new selection list
        //        var selectedFundingPhasestHS = new HashSet<string>(selectedFundingPhases);
        //        //List<int> selectedFundingPhaseIDs = Array.ConvertAll(SelectedFundingPhases, s => int.Parse(s)).ToList();
        //        //this is the current or previous saved selections
        //        Investment currentFundingPhasesInvestment = db.Investments.Include(f => f.FundingPhases).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        var investomentCurrentFundingPhaseIDs = new HashSet<int>(currentFundingPhasesInvestment.FundingPhases.Select(f => f.FundingPhaseID));

        //        foreach (var phase in db.FundingPhases)
        //        {
        //            if (selectedFundingPhasestHS.Contains(phase.FundingPhaseID.ToString())) //<----------
        //            {
        //                //if the selection not in previous selections, add it
        //                if (!investomentCurrentFundingPhaseIDs.Contains(phase.FundingPhaseID))
        //                {
        //                    investment.FundingPhases.Add(phase);
        //                    //updated = true;
        //                }
        //            }
        //            else
        //            {
        //                //if the selection in previous selections but not in the new selected list, remove it 
        //                if (investomentCurrentFundingPhaseIDs.Contains(phase.FundingPhaseID))
        //                {
        //                    investment.FundingPhases.Remove(phase);
        //                    //updated = true;
        //                }
        //            }
        //        }
        //    }
        //    else if (investment.FundingPhases != null) //2018-08-08
        //    {
        //        //Delete Any previous selection if any
        //        //this is the current or previous saved selections
        //        //var currentFundingPhases = db.Investments.Include(f => f.FundingPhases).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        //var investomentCurrentFundingPhases = new HashSet<int>(currentFundingPhases.FundingPhases.Select(f => f.FundingPhaseID));

        //        foreach (var phase in db.FundingPhases)
        //        {

        //            investment.FundingPhases.Remove(phase);
        //            //updated = true;
        //        }
        //    }

        //    //if (activeTab = "tab_Funding")

        //    if (selectedFundingAmounts != null) //==> UpdateSelectedFundingAmounts(Investment investmentProfile)
        //    {
        //        //this is the new selection list
        //        var selectedFundingAmountHS = new HashSet<string>(selectedFundingAmounts);
        //        //List<int> selectedFundingAmountIDs = Array.ConvertAll(SelectedFundingAmounts, s => int.Parse(s)).ToList();
        //        //this is the current or previous saved selections
        //        Investment currentFundingAmountsInvestment = db.Investments.Include(f => f.FundingAmounts).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        var investomentCurrentFundingAmountIDs = new HashSet<int>(currentFundingAmountsInvestment.FundingAmounts.Select(f => f.FundingAmountID));

        //        foreach (var amount in db.FundingAmounts)
        //        {
        //            if (selectedFundingAmountHS.Contains(amount.FundingAmountID.ToString()))
        //            {
        //                //if the selection not in previous selections, add it
        //                if (!investomentCurrentFundingAmountIDs.Contains(amount.FundingAmountID))
        //                {
        //                    investment.FundingAmounts.Add(amount);
        //                    //updated = true;
        //                }
        //            }
        //            else
        //            {
        //                //if the selection in previous selections but not in the new selected list, remove it 
        //                if (investomentCurrentFundingAmountIDs.Contains(amount.FundingAmountID))
        //                {
        //                    investment.FundingAmounts.Remove(amount);
        //                    //updated = true;
        //                }
        //            }
        //        }
        //    }
        //    else if (investment.FundingAmounts != null) //2018-08-08
        //    {
        //        //Delete Any previous selection if any
        //        //this is the current or previous saved selections
        //        //var currentFundingAmount = db.Investments.Include(f => f.FundingAmounts).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        //var investmentCurrentFundingAmount = new HashSet<int>(currentFundingAmount.FundingAmounts.Select(f => f.FundingAmountID));

        //        foreach (var amount in db.FundingAmounts)
        //        {
                    
        //            investment.FundingAmounts.Remove(amount);
        //            //updated = true;
        //        }
        //    }

        //    //********************************************************************************//

        //    //if (activeTab == "tab_Team") <---------------------------Added
        //    if (selectedTeamSkills != null) //==> UpdateSelectedTeamSkills(Investment investmentProfile)
        //    {
        //        //this is the new selection list
        //        var selectedTeamSkillsHS = new HashSet<string>(selectedTeamSkills);
        //        //List<int> selectedTeamSkillIDs = Array.ConvertAll(SelectedTeamSkills, s => int.Parse(s)).ToList();
        //        Investment currentSkillsInvestment = db.Investments.Include(w => w.TeamSkills).Where(s => s.InvestmentID == investment.InvestmentID).Single();
        //        var investmentCurrentSkillIDs = new HashSet<int>(currentSkillsInvestment.TeamSkills.Select(w => w.SkillID));

        //        foreach (var skill in db.TeamSkills)
        //        {
        //            if (selectedTeamSkillsHS.Contains(skill.SkillID.ToString()))
        //            {
        //                //if the selection not in previous selections, add it
        //                if (!investmentCurrentSkillIDs.Contains(skill.SkillID))
        //                {
        //                    investment.TeamSkills.Add(skill);
        //                    //updated = true;
        //                }
        //            }
        //            else
        //            {
        //                //if the selection in previous selections but not in the new selected list, remove it
        //                if (investmentCurrentSkillIDs.Contains(skill.SkillID))
        //                {
        //                    investment.TeamSkills.Remove(skill);
        //                    //updated = true;
        //                }
        //            }
        //        }
        //    }
        //    else if (investment.TeamSkills != null) //<--------2018-08-08
        //    {
        //        //Delete Any previous selection if any
        //        //this is the current or previous saved selections
        //        //var currentSkillsInvestment = db.Investments.Include(w => w.TeamSkills).Where(s => s.InvestmentID == investment.InvestmentID).Single();
        //        //var investmentSkillIDs = new HashSet<int>(currentSkillsInvestment.TeamSkills.Select(w => w.SkillID));

        //        foreach (var skill in db.TeamSkills)
        //        {
        //            investment.TeamSkills.Remove(skill);
        //            //updated = true;
        //        }
        //    }

        //    // if (activeTab == "tab_Outcome")

        //    if (selectedOutcomes != null) //==> UpdateSelectedOutcomes(Investment investmentProfile)
        //    {
        //        //this is the new selection list
        //        var selectedOutcomesHS = new HashSet<string>(selectedOutcomes);
        //        //List<int> selectedOutcomeIDs = Array.ConvertAll(SelectedOutcomes, s => int.Parse(s)).ToList();
        //        //this is the current or previous saved selections
        //        Investment currentInvestmentOutcomesInvestment = db.Investments.Include(o => o.Outcomes).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        var investmentCurrentOutcomeIDs = new HashSet<int>(currentInvestmentOutcomesInvestment.Outcomes.Select(o => o.OutcomeID));

        //        foreach (var outcome in db.Outcomes)
        //        {
        //            if (selectedOutcomesHS.Contains(outcome.OutcomeID.ToString()))
        //            {
        //                //if the selection not in previous selections, add it
        //                if (!investmentCurrentOutcomeIDs.Contains(outcome.OutcomeID))
        //                {
        //                    investment.Outcomes.Add(outcome);
        //                    //updated = true;
        //                }
        //            }
        //            else
        //            {
        //                //if the selection in previous selections but not in the new selected list, remove it
        //                if (investmentCurrentOutcomeIDs.Contains(outcome.OutcomeID))
        //                {
        //                    investment.Outcomes.Remove(outcome);
        //                    //updated = true;
        //                }
        //            }
        //        }
        //    }
        //    else if (investment.Outcomes != null) //2018-08-08
        //    {
        //        //Delete Any previous selection if any
        //        //this is the current or previous saved selections
        //        //var currentInvestmentOutcomes = db.Investments.Include(o => o.Outcomes).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        //var investmentCurrentOutcomes = new HashSet<int>(currentInvestmentOutcomes.Outcomes.Select(o => o.OutcomeID));

        //        foreach (var outcome in db.Outcomes)
        //        {
        //            investment.Outcomes.Remove(outcome);
        //            //updated = true;
        //        }
        //    }

        //    //********************************************************************************//

        //    // if (activeTab == "tab_Outcome")

        //    if (selectedInnovationLevels != null) //==> UpdateSelectedInnovationLevels(Investment investmentProfile)
        //    {
        //        //this is the new selection list
        //        var selectedInnovationLevelsHS = new HashSet<String>(selectedInnovationLevels);
        //        //List<int> selectedInnovationLevelIDs = Array.ConvertAll(SelectedInnovationLevels, s => int.Parse(s)).ToList();
        //        //this is the current or previous saved selections
        //        Investment currentInnovationLevelsInvestment = db.Investments.Include(nl => nl.InnovationLevels).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        var investmentCurrentInnovationLevelIDs = new HashSet<int>(currentInnovationLevelsInvestment.InnovationLevels.Select(nl => nl.InnovationLevelID));

        //        foreach (var level in db.InnovationLevels)
        //        {
        //            if (selectedInnovationLevelsHS.Contains(level.InnovationLevelID.ToString()))
        //            {
        //                //if the selection not in previous selections, add it
        //                if (!investmentCurrentInnovationLevelIDs.Contains(level.InnovationLevelID))
        //                {
        //                    investment.InnovationLevels.Add(level);
        //                    //updated = true;
        //                }
        //            }
        //            else
        //            {
        //                //if the selection in previous selections but not in the new selected list, remove it 
        //                if (investmentCurrentInnovationLevelIDs.Contains(level.InnovationLevelID))
        //                {
        //                    investment.InnovationLevels.Remove(level);
        //                    //updated = true;
        //                }
        //            }
        //        }
        //    }
        //    else if (investment.InnovationLevels != null) //2018-08-08
        //    {
        //        //Delete Any previous selection if any
        //        //this is the current or previous saved selections
        //        //var currentInnovationLevels = db.Investments.Include(nl => nl.InnovationLevels).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        //var investomentCurrentInnovationLevels = new HashSet<int>(currentInnovationLevels.InnovationLevels.Select(nl => nl.InnovationLevelID));

        //        foreach (var level in db.InnovationLevels)
        //        {

        //            investment.InnovationLevels.Remove(level);
        //            //updated = true;
        //        }
        //    }

        //    // if (activeTab == "tab_Outcome")

        //    if (selectedScalabilities != null) //==> UpdateSelectedScalabilities(Investment investmentProfile)
        //    {
        //        //this is the new selection list
        //        var selectedScalabilitiesHS = new HashSet<String>(selectedScalabilities);
        //        //List<int> selectedScalabilityIDs = Array.ConvertAll(SelectedScalabilities, s => int.Parse(s)).ToList();
        //        //this is the current or previous saved selections
        //        Investment currentScalabilitiesInvestment = db.Investments.Include(s => s.Scalabilities).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        var investmentCurrentScalabilitieIDs = new HashSet<int>(currentScalabilitiesInvestment.Scalabilities.Select(s => s.ScalabilityID));

        //        foreach (var scalability in db.Scalabilities)
        //        {
        //            if (selectedScalabilitiesHS.Contains(scalability.ScalabilityID.ToString()))
        //            {
        //                //if the selection not in previous selections, add it
        //                if (!investmentCurrentScalabilitieIDs.Contains(scalability.ScalabilityID))
        //                {
        //                    investment.Scalabilities.Add(scalability);
        //                    //updated = true;
        //                }
        //            }
        //            else
        //            {
        //                //if the selection in previous selections but not in the new selected list, remove it 
        //                if (investmentCurrentScalabilitieIDs.Contains(scalability.ScalabilityID))
        //                {
        //                    investment.Scalabilities.Remove(scalability);
        //                    //updated = true;
        //                }
        //            }
        //        }
        //    }
        //    else if (investment.Scalabilities != null) //2018-08-08
        //    {
        //        //Delete Any previous selection if any
        //        //this is the current or previous saved selections
        //        //var currentScalabilities = db.Investments.Include(s => s.Scalabilities).Where(i => i.InvestmentID == investment.InvestmentID).Single();
        //        //var investomentCurrentScalabilities = new HashSet<int>(currentScalabilities.Scalabilities.Select(s => s.ScalabilityID));

        //        foreach (var scalability in db.Scalabilities)
        //        {

        //            investment.Scalabilities.Remove(scalability);
        //            //updated = true;
        //        }
        //    }

        //    //********************************************************************************//

        //    //return updated;
        //}

        //-----------------------------------------------------------------------------------
        //Funding
        private List<InvestorFundingPhaseViewModel> GetFundingPhases(Investment investmentProfile)
        {
            List<InvestorFundingPhaseViewModel> fundingPhaseViewModels = new List<InvestorFundingPhaseViewModel>();

            List<int> profileFundingPhaseIDList = investmentProfile.FundingPhases.Select(fp => fp.FundingPhaseID).ToList();

            //if (investmentProfile.FundingPhases != null && investmentProfile.FundingPhases.Any())
            //{
            //    currentFundingPhaseIDList = investmentProfile.FundingPhases.Select(fp => fp.FundingPhaseID).ToList();
            //}
            //var currentFundingPhaseInvestment = db.Investments.Include(fpi => fpi.FundingPhases).Where(i => i.InvestmentID == investmentID).Single();
            //var investmentFundingPhases = new HashSet<int>(currentFundingPhaseInvestment.FundingPhases.Select(fp => fp.FundingPhaseID)); //Why HasSet?

            List<FundingPhase> fundingPhases = db.FundingPhases.ToList();
            foreach (var phase in fundingPhases)
            {
                fundingPhaseViewModels.Add(new InvestorFundingPhaseViewModel
                {
                    FundingPhaseID = phase.FundingPhaseID,
                    FundingPhaseName = phase.FundingPhaseName,
                    //Assigned = currentFundingPhaseIDList.Contains(phase.FundingPhaseID)
                    Assigned = profileFundingPhaseIDList.Any() ? profileFundingPhaseIDList.Contains(phase.FundingPhaseID) : false
                });
            }
            //}

            return fundingPhaseViewModels;
        }
        private List<InvestorFundingAmountViewModel> GetFundingAmounts(Investment investmentProfile)
        {
            List<InvestorFundingAmountViewModel> fundingAmountViewModels = new List<InvestorFundingAmountViewModel>();

            List<int> profileFundingAmountIDList = investmentProfile.FundingAmounts.Select(fa => fa.FundingAmountID).ToList();

            //if (investmentProfile.FundingAmounts != null && investmentProfile.FundingAmounts.Any())
            //{
            //    currentFundingAmountIDList = investmentProfile.FundingAmounts.Select(fa => fa.FundingAmountID).ToList();
            //}
            //var currentFundingAmountInvestment = db.Investments.Include(fai => fai.FundingAmounts).Where(i => i.InvestmentID == investmentID).Single();
            //var investmentFundingAmounts = new HashSet<int>(currentFundingAmountInvestment.FundingAmounts.Select(fa => fa.FundingAmountID));

            List<FundingAmount> fundingAmounts = db.FundingAmounts.ToList();
            foreach (var amount in fundingAmounts)
            {
                fundingAmountViewModels.Add(new InvestorFundingAmountViewModel
                {
                    FundingAmountID = amount.FundingAmountID,
                    FundingAmountValue = amount.FundingAmountValue,
                    //Assigned = currentFundingAmountIDList.Contains(amount.FundingAmountID)
                    Assigned = profileFundingAmountIDList.Any() ? profileFundingAmountIDList.Contains(amount.FundingAmountID) : false
                });
            }
            //}

            return fundingAmountViewModels;
        }

        //Budget
        private List<InvestorEstimatedExitPlanViewModel> GetEstimatedExitPlans(Investment investmentProfile)
        {
            List<InvestorEstimatedExitPlanViewModel> estimatedExitPlanViewModels = new List<InvestorEstimatedExitPlanViewModel>();

            List<int> profileEstimatedExitPlanIDList = investmentProfile.EstimatedExitPlans.Select(eep => eep.EstimatedExitPlanID).ToList();

            //if (investmentProfile.EstimatedExitPlans != null && investmentProfile.EstimatedExitPlans.Any())
            //{
            //    currentEstimatedExitPlanIDList = investmentProfile.EstimatedExitPlans.Select(eep => eep.EstimatedExitPlanID).ToList();
            //}
            //var currentEstimatedExitPlanInvestment = db.Investments.Include(eepi => eepi.EstimatedExitPlans).Where(i => i.InvestmentID == investmentID).Single();
            //var investmentEstimatedExitPlans = new HashSet<int>(currentEstimatedExitPlanInvestment.EstimatedExitPlans.Select(eep => eep.EstimatedExitPlanID));

            List<EstimatedExitPlan> estimatedExitPlans = db.EstimatedExitPlans.ToList();
            foreach (var exitPlan in estimatedExitPlans)
            {
                estimatedExitPlanViewModels.Add(new InvestorEstimatedExitPlanViewModel
                {
                    EstimatedExitPlanID = exitPlan.EstimatedExitPlanID,
                    EstimatedExitPlanName = exitPlan.EstimatedExitPlanName,
                    //Assigned = currentEstimatedExitPlanIDList.Contains(exitPlan.EstimatedExitPlanID)
                    Assigned = profileEstimatedExitPlanIDList.Any() ? profileEstimatedExitPlanIDList.Contains(exitPlan.EstimatedExitPlanID) : false
                });
            }
            //}

            return estimatedExitPlanViewModels;
        }

        //Team
        private List<TeamSkillViewModel> GetTeamSkills(Investment investmentProfile)
        {
            List<TeamSkillViewModel> teamSkillViewModels = new List<TeamSkillViewModel>();

            List<int> profileTeamSkillIDList = investmentProfile.TeamSkills.Select(ts => ts.SkillID).ToList();

            //if (investmentProfile.TeamSkills != null && investmentProfile.TeamSkills.Any())
            //{
            //    currentTeamSkillIDList = investmentProfile.TeamSkills.Select(ts => ts.SkillID).ToList();
            //}
            //var currentTeamSkillsInvestment = db.Investments.Include(tsi => tsi.TeamSkills).Where(i => i.InvestmentID == investmentID).Single();
            //var investmentTeamSkills = new HashSet<int>(currentTeamSkillsInvestment.TeamSkills.Select(ts => ts.SkillID));

            List<TeamSkill> teamSkills = db.TeamSkills.ToList();
            foreach (var teamSkill in teamSkills)
            {
                teamSkillViewModels.Add(new TeamSkillViewModel
                {
                    SkillID = teamSkill.SkillID,
                    SkillName = teamSkill.SkillName,
                    //Assigned = currentTeamSkillIDList.Contains(skill.SkillID)
                    Assigned = profileTeamSkillIDList.Any() ? profileTeamSkillIDList.Contains(teamSkill.SkillID) : false
                });
            }
            //}

            return teamSkillViewModels;
        }

        //Outcome
        private List<InvestorOutcomeViewModel> GetOutcomes(Investment investmentProfile)
        {
            List<InvestorOutcomeViewModel> outcomeViewModels = new List<InvestorOutcomeViewModel>();

            List<int> profileOutcomeIDList = investmentProfile.Outcomes.Select(o => o.OutcomeID).ToList();

            //if (investmentProfile.Outcomes != null && investmentProfile.Outcomes.Any())
            //{
            //    currentOutcomeIDList = investmentProfile.Outcomes.Select(o => o.OutcomeID).ToList();
            //}

            //var currentOutcomeInvestment = db.Investments.Include(oi => oi.Outcomes).Where(i => i.InvestmentID == investmentID).Single();
            //var investmentOutcomes = new HashSet<int>(currentOutcomeInvestment.Outcomes.Select(o => o.OutcomeID));

            List<Models.Shared.Outcome> outcomes = db.Outcomes.ToList();
            foreach (var outcome in outcomes)
            {
                outcomeViewModels.Add(new InvestorOutcomeViewModel
                {
                    OutcomeID = outcome.OutcomeID,
                    OutcomeName = outcome.OutcomeName,
                    //Assigned = currentOutcomeIDList.Contains(outcome.OutcomeID)
                    Assigned = profileOutcomeIDList.Any() ? profileOutcomeIDList.Contains(outcome.OutcomeID) : false
                });
            }
            //}

            return outcomeViewModels;
        }
        private List<InvestorInnovationLevelViewModel> GetInnovationLevels(Investment investmentProfile)
        {
            List<InvestorInnovationLevelViewModel> innovationLevelViewModels = new List<InvestorInnovationLevelViewModel>();

            List<int> profileInnovationLevelIDList = investmentProfile.InnovationLevels.Select(il => il.InnovationLevelID).ToList();

            //if (investmentProfile.InnovationLevels != null && investmentProfile.InnovationLevels.Any())
            //{
            //    currentInnovationLevelIDList = investmentProfile.InnovationLevels.Select(il => il.InnovationLevelID).ToList();
            //}

            //var currentInnovationLevelInvestment = db.Investments.Include(ili => ili.InnovationLevels).Where(i => i.InvestmentID == investmentID).Single();
            //var investmentInnovationLevels = new HashSet<int>(currentInnovationLevelInvestment.InnovationLevels.Select(il => il.InnovationLevelID));

            List<InnovationLevel> innovationLevels = db.InnovationLevels.ToList();
            foreach (var innovationlevel in innovationLevels)
            {
                innovationLevelViewModels.Add(new InvestorInnovationLevelViewModel
                {
                    InnovationLevelID = innovationlevel.InnovationLevelID,
                    InnovationLevelName = innovationlevel.InnovationLevelName,
                    //Assigned = currentInnovationLevelIDList.Contains(innovationlevel.InnovationLevelID)
                    Assigned = profileInnovationLevelIDList.Any() ? profileInnovationLevelIDList.Contains(innovationlevel.InnovationLevelID) : false
                });
            }
            //}

            return innovationLevelViewModels;
        }
        private List<InvestorScalabilityViewModel> GetScalabilities(Investment investmentProfile)
        {
            List<InvestorScalabilityViewModel> scalabilityViewModels = new List<InvestorScalabilityViewModel>();

            List<int> profileScalabilityIDList = investmentProfile.Scalabilities.Select(sc => sc.ScalabilityID).ToList();

            //if (investmentProfile.Scalabilities != null && investmentProfile.Scalabilities.Any())
            //{
            //    currentScalabilityIDList = investmentProfile.Scalabilities.Select(sc => sc.ScalabilityID).ToList();
            //}
            //var currentScalabilityInvestment = db.Investments.Include(si => si.Scalabilities).Where(i => i.InvestmentID == investmentID).Single();
            //var investmentScalabilities = new HashSet<int>(currentScalabilityInvestment.Scalabilities.Select(s => s.ScalabilityID));

            List<Scalability> scalabilities = db.Scalabilities.ToList();
            foreach (var scalability in scalabilities)
            {
                scalabilityViewModels.Add(new InvestorScalabilityViewModel
                {
                    ScalabilityID = scalability.ScalabilityID,
                    ScalabilityName = scalability.ScalabilityName,
                    //Assigned = currentScalabilityIDList.Contains(scalability.ScalabilityID)
                    Assigned = profileScalabilityIDList.Any() ? profileScalabilityIDList.Contains(scalability.ScalabilityID) : false
                });
            }
            //}

            return scalabilityViewModels;
        }


        //---------------------------------------------------------------
        //<-----Remove
        private void PopulateAssignedCheckBoxsData(Investment investment)
        {
            //private List<TeamSkillViewModel> GetTeamSkills(int investmentID) {
            var allSkills = db.TeamSkills; //
            var currentSkillsInvestment = db.Investments.Include(s => s.TeamSkills).Where(i => i.InvestmentID == investment.InvestmentID).Single(); //<----------Added
            var investmentSkills = new HashSet<int>(currentSkillsInvestment.TeamSkills.Select(w => w.SkillID));
            var skillsViewModel = new List<TeamSkillViewModel>();
            foreach (var skill in allSkills) //<-----------------------------Added
            {
                skillsViewModel.Add(new TeamSkillViewModel
                {
                    SkillID = skill.SkillID,
                    SkillName = skill.SkillName,
                    Assigned = investmentSkills.Contains(skill.SkillID)
                });
            }
            ViewBag.skillsViewModel = skillsViewModel; //return skillsViewModel; }
            //----------------------------------------
            //private List<InvestorFundingAmountViewModel> GetFundingAmounts(int investmentID) {
            var allFundingAmounts = db.FundingAmounts;
            var currentInvestmentFundingAmount = db.Investments.Include(f => f.FundingAmounts).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var investmentFundingAmount = new HashSet<int>(currentInvestmentFundingAmount.FundingAmounts.Select(F => F.FundingAmountID));
            var fundingAmountViewModel = new List<InvestorFundingAmountViewModel>();
            foreach (var amount in allFundingAmounts)
            {
                fundingAmountViewModel.Add(new InvestorFundingAmountViewModel
                {
                    FundingAmountID = amount.FundingAmountID,
                    FundingAmountValue = amount.FundingAmountValue,
                    Assigned = investmentFundingAmount.Contains(amount.FundingAmountID)
                });
            }
            ViewBag.fundingAmountViewModel = fundingAmountViewModel; //return FundingAmountViewModel; }
            //--------------------------------------
            //private List<InvestorFundingAmountPhaseModel> GetFundingPhases(int investmentID) {
            var allFundingPhases = db.FundingPhases;
            var currentInvestmentFundingPhase = db.Investments.Include(f => f.FundingPhases).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var investmentFundingPhase = new HashSet<int>(currentInvestmentFundingPhase.FundingPhases.Select(F => F.FundingPhaseID));
            var fundingPhaseViewModel = new List<InvestorFundingPhaseViewModel>();
            foreach (var phase in allFundingPhases)
            {
                fundingPhaseViewModel.Add(new InvestorFundingPhaseViewModel
                {
                    FundingPhaseID = phase.FundingPhaseID,
                    FundingPhaseName = phase.FundingPhaseName,
                    Assigned = investmentFundingPhase.Contains(phase.FundingPhaseID)
                });
            }
            ViewBag.fundingPhaseViewModel = fundingPhaseViewModel; // return FundingPhaseViewModel; }
            //----------------------------
            //private List<InvestorOutcomeViewModel> GetOutcomes(int investmentID) {
            var allOutcomes = db.Outcomes;
            var currentInvestmentOutcome = db.Investments.Include(o => o.Outcomes).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var investmentOutcome = new HashSet<int>(currentInvestmentOutcome.Outcomes.Select(o => o.OutcomeID));
            var outcomeViewModel = new List<InvestorOutcomeViewModel>();
            foreach (var outcome in allOutcomes)
            {
                outcomeViewModel.Add(new InvestorOutcomeViewModel
                {
                    OutcomeID = outcome.OutcomeID,
                    OutcomeName = outcome.OutcomeName,
                    Assigned = investmentOutcome.Contains(outcome.OutcomeID)
                });
            }
            ViewBag.outcomeViewModel = outcomeViewModel; // return OutcomeViewModel; } 
            //--------------------------------------------
            //private List<InvestorInnovationLevelViewModel> GetInnovationLevels(int investmentID) {
            var allInnovationLevels = db.InnovationLevels;
            var currentInnovationLevels = db.Investments.Include(inn => inn.InnovationLevels).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var investmentInnovationLevels = new HashSet<int>(currentInnovationLevels.InnovationLevels.Select(inn => inn.InnovationLevelID));
            var innovationLevelsViewModel = new List<InvestorInnovationLevelViewModel>();
            foreach (var level in allInnovationLevels)
            {
                innovationLevelsViewModel.Add(new InvestorInnovationLevelViewModel
                {
                    InnovationLevelID = level.InnovationLevelID,
                    InnovationLevelName = level.InnovationLevelName,
                    Assigned = investmentInnovationLevels.Contains(level.InnovationLevelID)
                });
            }
            ViewBag.innovationLevelsViewModel = innovationLevelsViewModel; // return InnovationLevelViewModel; }
            //--------------------------------------
            //private List<InvestorScalabilityViewModel> GetScalabilities(int investmentID) {
            var allScalabilities = db.Scalabilities;
            var currentInvestorScalabilities = db.Investments.Include(s => s.Scalabilities).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var investmentScalabilities = new HashSet<int>(currentInvestorScalabilities.Scalabilities.Select(s => s.ScalabilityID));
            var scalabilitiesViewModel = new List<InvestorScalabilityViewModel>();
            foreach (var scalability in allScalabilities)
            {
                scalabilitiesViewModel.Add(new InvestorScalabilityViewModel
                {
                    ScalabilityID = scalability.ScalabilityID,
                    ScalabilityName = scalability.ScalabilityName,
                    Assigned = investmentScalabilities.Contains(scalability.ScalabilityID)
                });
            }
            ViewBag.scalabilitiesViewModel = scalabilitiesViewModel; //return ... }
            //------------------------------------------------
            //private List<InvestorEstimatedExitPlanViewModel> GetEstimatedExitPlans(int investmentID) {
            var allEstimatedExitPlans = db.EstimatedExitPlans;
            var currentInvestorEstimatedExitPlans = db.Investments.Include(e => e.EstimatedExitPlans).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var investmentEstimatedExitPlans = new HashSet<int>(currentInvestorEstimatedExitPlans.EstimatedExitPlans.Select(e => e.EstimatedExitPlanID));
            var estimatedExitPlanViewModel = new List<InvestorEstimatedExitPlanViewModel>();
            foreach (var exitPlan in allEstimatedExitPlans)
            {                                                    
                estimatedExitPlanViewModel.Add(new InvestorEstimatedExitPlanViewModel
                {
                    EstimatedExitPlanID = exitPlan.EstimatedExitPlanID,
                    EstimatedExitPlanName = exitPlan.EstimatedExitPlanName,
                    Assigned = investmentEstimatedExitPlans.Contains(exitPlan.EstimatedExitPlanID)
                });
            }
            ViewBag.estimatedExitPlanViewModel = estimatedExitPlanViewModel; // return ...; }
        }



        //<-------delete alltogether, will not be used anymore
        //private void PopulateAssignedCheckBoxsData(Investment investment,string[] selectedSkills, string[] selectedFundingAmount, string[] selectedFundingPhases,
        //    string[] selectedOutcomes, string[] selectedInnovationLevels, string[] selectedScalabilities, string[] selectedEstimatedExitPlans)
        //{
        //    //populate the selected checkbox from selectedstring after validation errors
        //    //GetTeamSkills(selectedSkills)
        //    var allSkills = db.TeamSkills; //<-------------------------Added
        //    var selectedSkillsHS = new HashSet<String>();
        //    var skillsViewModel = new List<TeamSkillViewModel>();
        //    if (selectedSkills != null)
        //    {
        //        selectedSkillsHS = new HashSet<String>(selectedSkills);
        //    }
        //    foreach (var skill in allSkills) //<----------------------------Added
        //    {
        //        skillsViewModel.Add(new TeamSkillViewModel
        //        {
        //            SkillID = skill.SkillID,
        //            SkillName = skill.SkillName,
        //            Assigned = selectedSkillsHS.Contains(skill.SkillID.ToString())
        //        });
        //    }
        //    ViewBag.skillsViewModel = skillsViewModel;
        //    //---------------------------------------
        //    //GetFundingAmounts(selectedFundingAmounts)
        //    var allFundingAmounts = db.FundingAmounts;
        //    var selectedInvestmentFundingAmountHS = new HashSet<String>();
        //    var FundingAmountViewModel = new List<InvestorFundingAmountViewModel>();
        //    if (selectedFundingAmount != null)
        //    {
        //        selectedInvestmentFundingAmountHS = new HashSet<String>(selectedFundingAmount);
        //    }
        //    foreach (var amount in allFundingAmounts)
        //    {
        //        FundingAmountViewModel.Add(new InvestorFundingAmountViewModel
        //        {
        //            FundingAmountID = amount.FundingAmountID,
        //            FundingAmountValue = amount.FundingAmountValue,
        //            Assigned = selectedInvestmentFundingAmountHS.Contains(amount.FundingAmountID.ToString())
        //        });
        //    }
        //    ViewBag.fundingAmountViewModel = FundingAmountViewModel;
        //    //--------------------------------------
        //    //GetFundingPhases(selectedFundingPhases)
        //    var allFundingPhases = db.FundingPhases;
        //    var selectedInvestmentFundingPhaseHS = new HashSet<String>();
        //    var FundingPhaseViewModel = new List<InvestorFundingPhaseViewModel>();
        //    if (selectedFundingPhases != null)
        //    {
        //        selectedInvestmentFundingPhaseHS = new HashSet<String>(selectedFundingPhases);
        //    }
        //    foreach (var phase in allFundingPhases)
        //    {
        //        FundingPhaseViewModel.Add(new InvestorFundingPhaseViewModel
        //        {
        //            FundingPhaseID = phase.FundingPhaseID,
        //            FundingPhaseName = phase.FundingPhaseName,
        //            Assigned = selectedInvestmentFundingPhaseHS.Contains(phase.FundingPhaseID.ToString())
        //        });
        //    }
        //    ViewBag.fundingPhaseViewModel = FundingPhaseViewModel;
        //    //----------------------------
        //    //GetOutcomes(selectedOutcomes)
        //    var allOutcomes = db.Outcomes;
        //    var selectedInvestmentOutcomeHS = new HashSet<String>();
        //    var OutcomeViewModel = new List<InvestorOutcomeViewModel>();
        //    if (selectedOutcomes != null)
        //    {
        //        selectedInvestmentOutcomeHS = new HashSet<String>(selectedOutcomes);
        //    }
        //    foreach (var outcome in allOutcomes)
        //    {
        //        OutcomeViewModel.Add(new InvestorOutcomeViewModel
        //        {
        //            OutcomeID = outcome.OutcomeID,
        //            OutcomeName = outcome.OutcomeName,
        //            Assigned = selectedInvestmentOutcomeHS.Contains(outcome.OutcomeID.ToString())
        //        });
        //    }
        //    ViewBag.outcomeViewModel = OutcomeViewModel;
        //    //--------------------------------------------
        //    //GetInnovationlevels(selectedInnovationLevels)
        //    var allInnovationLevels = db.InnovationLevels;
        //    var selectedInvestmentInnovationLevelsHS = new HashSet<String>();
        //    var InnovationLevelsViewModel = new List<InvestorInnovationLevelViewModel>();
        //    if (selectedInnovationLevels != null)
        //    {
        //        selectedInvestmentInnovationLevelsHS = new HashSet<String>(selectedInnovationLevels);
        //    }
        //    foreach (var level in allInnovationLevels)
        //    {
        //        InnovationLevelsViewModel.Add(new InvestorInnovationLevelViewModel
        //        {
        //            InnovationLevelID = level.InnovationLevelID,
        //            InnovationLevelName = level.InnovationLevelName,
        //            Assigned = selectedInvestmentInnovationLevelsHS.Contains(level.InnovationLevelID.ToString())
        //        });
        //    }
        //    ViewBag.innovationLevelsViewModel = InnovationLevelsViewModel;
        //    //--------------------------------------
        //    //GetScalabilities(selectedScalabilities)
        //    var allScalabilities = db.Scalabilities;
        //    var selectedInvestmentScalabilitiesHS = new HashSet<String>();
        //    var ScalabilitiesViewModel = new List<InvestorScalabilityViewModel>();
        //    if (selectedScalabilities != null)
        //    {
        //        selectedInvestmentScalabilitiesHS = new HashSet<String>(selectedScalabilities);
        //    }
        //    foreach (var scalability in allScalabilities)
        //    {
        //        ScalabilitiesViewModel.Add(new InvestorScalabilityViewModel
        //        {
        //            ScalabilityID = scalability.ScalabilityID,
        //            ScalabilityName = scalability.ScalabilityName,
        //            Assigned = selectedInvestmentScalabilitiesHS.Contains(scalability.ScalabilityID.ToString())
        //        });
        //    }
        //    ViewBag.scalabilitiesViewModel = ScalabilitiesViewModel;
        //    //------------------------------------------------
        //    //GetEstimatedExitPlans(selectedEstimatedExitPlans)
        //    var allEstimatedExitPlans = db.EstimatedExitPlans;
        //    var selectedInvestmentEstimatedExitPlansHS = new HashSet<String>();
        //    var EstimatedExitPlanViewModel = new List<InvestorEstimatedExitPlanViewModel>();
        //    if (selectedEstimatedExitPlans != null)
        //    {
        //        selectedInvestmentEstimatedExitPlansHS = new HashSet<String>(selectedEstimatedExitPlans);
        //    }        
        //    foreach (var exitPlan in allEstimatedExitPlans)
        //    {
        //        EstimatedExitPlanViewModel.Add(new InvestorEstimatedExitPlanViewModel
        //        {
        //            EstimatedExitPlanID = exitPlan.EstimatedExitPlanID,
        //            EstimatedExitPlanName = exitPlan.EstimatedExitPlanName,
        //            Assigned = selectedInvestmentEstimatedExitPlansHS.Contains(exitPlan.EstimatedExitPlanID.ToString())
        //        });
        //    }
        //    ViewBag.estimatedExitPlanViewModel = EstimatedExitPlanViewModel;
        //}



        [Authorize(Roles = "Investor")]
        public ActionResult Activate(string id, string redirect = "Index") //ActivateOrUnactivate(
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment investmentProfile = db.Investments.Find(id);

            if (investmentProfile != null)
            {
                investmentProfile.Active = !investmentProfile.Active;

                db.Entry(investmentProfile).State = EntityState.Modified;
                db.SaveChanges();
            }

            if (redirect != "Index")
            {
                return RedirectToAction(redirect, new { id });
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Investor")]
        public bool IsLocked(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false; //new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment investment = db.Investments.Find(id);

            if (investment == null)
            {
                return false;
            }

            //try
            //investment.Locked = true;

            //db.Entry(investment).State = EntityState.Modified;
            //db.SaveChanges();

            //investment = db.Investments.Find(id);

            return investment.Locked;
        }

        //[Authorize(Roles = "Investor")]
        //public ActionResult ActivateInvestor(string redirect = "Index") //ActivateOrUnactivateInvestor(
        //{

        //    string investorID = User.Identity.GetUserId();
        //    ApplicationUser investor = db.Users.Find(investorID);

        //    if (investor!= null)
        //    {
        //        investor.ActiveInvestor = !investor.ActiveInvestor;
        //        db.Entry(investor).State = EntityState.Modified;
        //        db.SaveChanges();
        //    }

        //    return RedirectToAction(redirect);
        //}

        [Authorize(Roles = "Admin")]
        public ActionResult Unlock(string id, string redirect = "")
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment investment = db.Investments.Find(id);

            if (investment == null)
            {
                return HttpNotFound();
            }
            //try
            investment.Locked = false;
            
            db.Entry(investment).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect))
            {
                if (redirect.ToUpper().Contains("DETAILS")) return RedirectToAction("ProfileDetails", new { id }); //<-----------------Changed

                /*if (redirect == "Details")*/
                //return RedirectToAction(, new { id }); 
            }

            return RedirectToAction("Index"); //<---check user
        }

        // GET: Investments/Reminder
        [Authorize(Roles = ("Admin"))]
        public ActionResult Reminder(string id, string subject = "", string message = "", string redirect = "") //id==investmentId
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment investment = db.Investments.Find(id);

            if (investment == null)
            {
                return HttpNotFound();
            }
            else
            {
                ReminderInvestmentViewModel model = new ReminderInvestmentViewModel
                {
                    InvestmentId = id, //==investment.InvestmentId
                    InvestorEmail = investment.User.Email,
                    InvestorId = investment.UserId,
                    Subject = subject,
                    Message = message,
                    Redirect = redirect
                    
                };
                return View(model);
            }

            //return View();
        }

        // POST: Investments/Reminder
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = ("Admin"))]
        [HttpPost]
        [ValidateAntiForgeryToken] //[Bind(Include = "StartupId,InvestorEmail,Text")]
        [ValidateInput(false)]
        public async Task<ActionResult> Reminder(ReminderInvestmentViewModel model)
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
                                Body = /*model.InvestmentId + "\n\n" + */model.Message,
                                IsBodyHtml = false
                            };

                            message.To.Add(new MailAddress(model.InvestorEmail));
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

                                ModelState.AddModelError("", "The reminder about " + model.InvestmentId + " has been sent to " + model.InvestorEmail);
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