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
                    //ViewBag.UserRole = Role.Admin.ToString();
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
                    //case "SWEDISHREGION": return View(investmentProfiles.OrderBy(iv => iv.SwedishRegion.RegionName));
                    case "PROFILEDOMAINNAME": return View(investmentProfiles.OrderBy(iv => iv.ProjectDomain.ProjectDomainName));
                    case "MATCHMAKINGCOUNT": return View(investmentProfiles.OrderByDescending(iv => iv.MatchMakings.Count()));
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
                ProfileName = "",
                InvestorMessage = db.InvestorMessages.FirstOrDefault().Text,
                CountryList = new SelectList(db.Countries, "CountryID", "CountryName"),
                SwedishCountryID = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single(),
                SwedishRegionList = new SelectList(db.SwedishRegions, "RegionID", "RegionName")
            };

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
                               
                if (db.Investments.Where(i => i.UserId == UserId).Any()) newInvestmentProfile.DueDate = DateTime.Now.Date.AddDays(-1);

                db.Investments.Add(newInvestmentProfile);
                db.SaveChanges();

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
                    //TempData["is_new_profile"] = true;
                    return RedirectToAction("ProfileForm", new { id = newInvestmentProfile.InvestmentID }); // "Investments",
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            newProfileModel.ProfileName = "";
            newProfileModel.InvestorMessage = db.InvestorMessages.FirstOrDefault().Text;
            newProfileModel.CountryList = new SelectList(db.Countries, "CountryID", "CountryName");
            newProfileModel.SwedishCountryID = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();
            newProfileModel.SwedishRegionList = new SelectList(db.SwedishRegions, "RegionID", "RegionName");

            return View(newProfileModel);
        }

        // GET: Investments/ProfileForm/5
        [Authorize(Roles = "Admin, Investor")]
        public ActionResult ProfileForm(string id)
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

            string message = "";
            string unansweredQuestion = "";
            //bool isNewProfile = false; //<----------??

            if (TempData.Any())
            {
                if (TempData.ContainsKey("message")) message = TempData["message"] as string;
                //if (TempData.ContainsKey("tab")) ViewBag.Tab = TempData["tab"] as string;
                if (TempData.ContainsKey("unanswered")) unansweredQuestion = TempData["unanswered"] as string;
                //if (TempData.ContainsKey("is_new_profile")) isNewProfile = TempData["is_new_profile"] as bool? ?? false;
                TempData.Clear();
            }

            return View(GetInvestmentProfileViewModel(investmentProfile, false, message, unansweredQuestion/*, isNewProfile*/));
        }

        // POST: Investments/ProfileForm/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Investor")]
        public ActionResult ProfileForm(InvestmentProfilePostViewModel profilePostModel)
        {
            bool updated = true;

            Investment investmentProfile = db.Investments.Find(profilePostModel.InvestmentID);

            if (ModelState.IsValid)
            {
                if (UpdateActiveTab(investmentProfile, profilePostModel))
                {
                    investmentProfile.LastSavedDate = DateTime.Now.Date;
                    db.Entry(investmentProfile).State = EntityState.Modified;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
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

            return View(GetInvestmentProfileViewModel(investmentProfile, updated));
        }

        private InvestmentProfileViewModel GetInvestmentProfileViewModel(Investment investmentProfile, bool updated = false, string message = "", string unansweredQuestion = ""/*, bool isNewProfile = false*/)
        {
            if (investmentProfile == null) return new InvestmentProfileViewModel(); //return empty ViewModel
            
            return new InvestmentProfileViewModel()
            {
                InvestmentID = investmentProfile.InvestmentID,

                //Profile
                ProfileName = investmentProfile.ProfileName,
                ProfileDomainID = investmentProfile.ProjectDomainID,
                ProfileDomainList = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName"),

                //Funding
                FundingPhases = GetFundingPhases(investmentProfile),
                FundingAmounts = GetFundingAmounts(investmentProfile),
                FutureFundingNeeded = investmentProfile.FutureFundingNeeded, //!isNewProfile ? investmentProfile.FutureFundingNeeded : null,

                //Budget
                EstimatedExitPlans = GetEstimatedExitPlans(investmentProfile),
                EstimatedBreakEven = investmentProfile.EstimatedBreakEven,
                PossibleIncomeStreams = investmentProfile.PossibleIncomeStreams,

                //Team
                TeamMemberSizeMoreThanOne = investmentProfile.TeamMemberSizeMoreThanOne, //!isNewProfile ? investmentProfile.TeamMemberSizeMoreThanOne : null,
                TeamHasExperience = investmentProfile.TeamHasExperience, //!isNewProfile ? investmentProfile.TeamHasExperience : null,
                ActiveInvestor = investmentProfile.ActiveInvestor, //!isNewProfile ? investmentProfile.ActiveInvestor : null,
                TeamSkills = GetTeamSkills(investmentProfile),

                //Outcome
                Outcomes = GetOutcomes(investmentProfile),
                InnovationLevels = GetInnovationLevels(investmentProfile),
                Scalabilities = GetScalabilities(investmentProfile),

                Updated = updated,

                Message = message,
                UnansweredQuestion = unansweredQuestion
            };
        }

        //Funding
        private List<InvestorFundingPhaseViewModel> GetFundingPhases(Investment investmentProfile)
        {
            List<InvestorFundingPhaseViewModel> fundingPhaseViewModels = new List<InvestorFundingPhaseViewModel>();

            List<int> profileFundingPhaseIDList = investmentProfile.FundingPhases.Select(fp => fp.FundingPhaseID).ToList();

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
            
            updated = UpdateFundingPhases(profilePostModel.FundingPhaseIDs, investmentProfile);
            
            if (UpdateFundingAmounts(profilePostModel.FundingAmountIDs, investmentProfile) && !updated) updated= true;

            if (!string.IsNullOrEmpty(profilePostModel.FutureFundingNeeded))
            {
                bool postedFutureFundingNeeded = profilePostModel.FutureFundingNeeded.ToUpper() == "YES";
                if (postedFutureFundingNeeded && !investmentProfile.FutureFundingNeeded) // && profilePostModel.FutureFundingNeeded.HasValue && !profilePostModel.FutureFundingNeeded.Value
                {
                    investmentProfile.FutureFundingNeeded = true;
                    updated = true;
                }
                else if (!postedFutureFundingNeeded && investmentProfile.FutureFundingNeeded) // && profilePostModel.FutureFundingNeeded.HasValue && !profilePostModel.FutureFundingNeeded.Value
                {
                    investmentProfile.FutureFundingNeeded = false;
                    updated = true;
                }
            }
            else if (investmentProfile.FutureFundingNeeded) //!profilePostModel.FutureFundingNeeded.HasValue (bool?)
            {
                investmentProfile.FutureFundingNeeded = false; //profilePostModel.FutureFundingNeeded = null;
                updated = true;
            }
            //if (investmentProfile.FutureFundingNeeded != profilePostModel.FutureFundingNeeded)
            //{
            //    investmentProfile.FutureFundingNeeded = profilePostModel.FutureFundingNeeded;
            //     updated = true;
            //}

            return updated;
        }

        private bool UpdateFundingPhases(string[] postedFundingPhaseIDs, Investment investmentProfile) //<-----------finished
        {
            bool updated = false;

            if (postedFundingPhaseIDs != null)
            {
                List<int> postedFundingPhaseIDList = Array.ConvertAll(postedFundingPhaseIDs, s => int.Parse(s)).ToList();

                List<FundingPhase> profileFundingPhases = investmentProfile.FundingPhases.ToList();
                foreach (FundingPhase profileFundingPhase in profileFundingPhases)
                {
                    if (!postedFundingPhaseIDList.Contains(profileFundingPhase.FundingPhaseID))
                    {
                        investmentProfile.FundingPhases.Remove(profileFundingPhase);
                        updated = true;
                    }
                }

                List<FundingPhase> fundingPhases = db.FundingPhases.ToList();
                FundingPhase fundingPhase = null;
                foreach (int postedFundingPhaseID in postedFundingPhaseIDList)
                {
                    fundingPhase = fundingPhases.Where(fa => fa.FundingPhaseID == postedFundingPhaseID).FirstOrDefault();

                    if (!investmentProfile.FundingPhases.Contains(fundingPhase))
                    {
                        investmentProfile.FundingPhases.Add(fundingPhase);
                        updated = true;
                    }
                }
            }
            else //if (postedFundingPhaseIDs == null)
            {
                investmentProfile.FundingPhases.Clear();
                updated = true;
            }

            return updated;
        }

        private bool UpdateFundingAmounts(string[] postedFundingAmountIDs, Investment investmentProfile) //<---------finished
        {
            bool updated = false;

            if (postedFundingAmountIDs != null)
            {
                List<int> postedFundingAmountIDList = Array.ConvertAll(postedFundingAmountIDs, s => int.Parse(s)).ToList();

                List<FundingAmount> profileFundingAmounts = investmentProfile.FundingAmounts.ToList();
                foreach (FundingAmount profileFundingAmount in profileFundingAmounts)
                {
                    if (!postedFundingAmountIDList.Contains(profileFundingAmount.FundingAmountID))
                    {
                        investmentProfile.FundingAmounts.Remove(profileFundingAmount);
                        updated = true;
                    }
                }

                List<FundingAmount> fundingAmounts = db.FundingAmounts.ToList();
                FundingAmount fundingAmount = null;
                foreach (int postedFundingAmountID in postedFundingAmountIDList)
                {
                    fundingAmount = fundingAmounts.Where(fa => fa.FundingAmountID == postedFundingAmountID).FirstOrDefault();

                    if (!investmentProfile.FundingAmounts.Contains(fundingAmount))
                    {
                        investmentProfile.FundingAmounts.Add(fundingAmount);
                        updated = true;
                    }
                }
            }
            else //if (postedFundingAmountIDs == null)
            {
                investmentProfile.FundingAmounts.Clear();
                updated = true;
            }

            return updated;
        }

        private bool UpdateTabBudget(Investment investmentProfile, InvestmentProfilePostViewModel profilePostModel/*, string[] postedEstimatedExitPlans*/)
        {
            bool updated = false;
            
            updated = UpdateEstimatedExitPlans(profilePostModel.EstimatedExitPlanIDs, investmentProfile);

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

        private bool UpdateEstimatedExitPlans(string[] postedEstimatedExitPlanIDs, Investment investmentProfile) //<-------finished
        {
            bool updated = false;

            if (postedEstimatedExitPlanIDs != null)
            {
                List<int> postedEstimatedExitplanIDList = Array.ConvertAll(postedEstimatedExitPlanIDs, s => int.Parse(s)).ToList();

                List<EstimatedExitPlan> profileEstimatedExitplans = investmentProfile.EstimatedExitPlans.ToList();
                foreach (EstimatedExitPlan profileEstimatedExitplan in profileEstimatedExitplans)
                {
                    if (!postedEstimatedExitplanIDList.Contains(profileEstimatedExitplan.EstimatedExitPlanID))
                    {
                        investmentProfile.EstimatedExitPlans.Remove(profileEstimatedExitplan);
                        updated = true;
                    }
                }

                List<EstimatedExitPlan> estimatedExitplans = db.EstimatedExitPlans.ToList();
                EstimatedExitPlan estimatedExitplan = null;
                foreach (int postedEstimatedExitplanID in postedEstimatedExitplanIDList)
                {
                    estimatedExitplan = estimatedExitplans.Where(eep => eep.EstimatedExitPlanID == postedEstimatedExitplanID).FirstOrDefault();

                    if (!investmentProfile.EstimatedExitPlans.Contains(estimatedExitplan))
                    {
                        investmentProfile.EstimatedExitPlans.Add(estimatedExitplan);
                        updated = true;
                    }
                }
            }
            else //if (postedEstimatedExitPlanIDs == null)
            {
                investmentProfile.EstimatedExitPlans.Clear();
                updated = true;
            }

            return updated;
        }

        private bool UpdateTabTeam(Investment investmentProfile, InvestmentProfilePostViewModel profilePostModel)
        {
            bool updated = false;

            if (!string.IsNullOrEmpty(profilePostModel.TeamMemberSizeMoreThanOne))
            {
                bool postedTeamMemberSizeMoreThanOne = profilePostModel.TeamMemberSizeMoreThanOne.ToUpper() == "YES";
                if (postedTeamMemberSizeMoreThanOne && !investmentProfile.TeamMemberSizeMoreThanOne) // && profilePostModel.TeamMemberSizeMoreThanOne.HasValue && !profilePostModel.TeamMemberSizeMoreThanOne.Value
                {
                    investmentProfile.TeamMemberSizeMoreThanOne = true;
                    updated = true;
                }
                else if (!postedTeamMemberSizeMoreThanOne && investmentProfile.TeamMemberSizeMoreThanOne) // && profilePostModel.TeamMemberSizeMoreThanOne.HasValue && !profilePostModel.TeamMemberSizeMoreThanOne.Value
                {
                    investmentProfile.TeamMemberSizeMoreThanOne = false;
                    updated = true;
                }
            }
            else if (investmentProfile.FutureFundingNeeded) //!profilePostModel.TeamMemberSizeMoreThanOne.HasValue (bool?)
            {
                investmentProfile.FutureFundingNeeded = false; //profilePostModel.TeamMemberSizeMoreThanOne = null;
                updated = true;
            }
            //if (investmentProfile.TeamMemberSizeMoreThanOne != profilePostModel.TeamMemberSizeMoreThanOne)
            //{
            //    investmentProfile.TeamMemberSizeMoreThanOne = profilePostModel.TeamMemberSizeMoreThanOne;
            //    updated = true;
            //}

            if (!string.IsNullOrEmpty(profilePostModel.TeamHasExperience))
            {
                bool postedTeanHasExperience = profilePostModel.TeamHasExperience.ToUpper() == "YES";
                if (postedTeanHasExperience && !investmentProfile.TeamHasExperience) // && profilePostModel.TeamHasExperience.HasValue && !profilePostModel..ValueTeamHasExperience
                {
                    investmentProfile.TeamHasExperience = true;
                    updated = true;
                }
                else if (!postedTeanHasExperience && investmentProfile.TeamHasExperience) // && profilePostModel.TeamHasExperience.HasValue && !profilePostModel.TeamHasExperience.Value
                {
                    investmentProfile.TeamHasExperience = false;
                    updated = true;
                }
            }
            else if (investmentProfile.TeamHasExperience) //!profilePostModel.TeamHasExperience.HasValue (bool?)
            {
                investmentProfile.TeamHasExperience = false; //profilePostModel.TeamHasExperience = null;
                updated = true;
            }
            //if (investmentProfile.TeamHasExperience != profilePostModel.TeamHasExperience)
            //{
            //    investmentProfile.TeamHasExperience = profilePostModel.TeamHasExperience;
            //    updated = true;
            //}

            if (!string.IsNullOrEmpty(profilePostModel.ActiveInvestor))
            {
                bool postedActiveInvestor = profilePostModel.ActiveInvestor.ToUpper() == "YES";
                if (postedActiveInvestor && !investmentProfile.ActiveInvestor) // && profilePostModel..ActiveInvestor.HasValue && !profilePostModel..ActiveInvestor.Value
                {
                    investmentProfile.ActiveInvestor = true;
                    updated = true;
                }
                else if (!postedActiveInvestor && investmentProfile.ActiveInvestor) // && profilePostModel..ActiveInvestor.HasValue && !profilePostModel..Value.ActiveInvestor
                {
                    investmentProfile.ActiveInvestor = false;
                    updated = true;
                }
            }
            else if (investmentProfile.ActiveInvestor) //!profilePostModel.ActiveInvestor.HasValue (bool?)
            {
                investmentProfile.ActiveInvestor = false; //profilePostModel.ActiveInvestor = null;
                updated = true;
            }
            //if (investmentProfile.ActiveInvestor != profilePostModel.ActiveInvestor)
            //{
            //    investmentProfile.ActiveInvestor = profilePostModel.ActiveInvestor;
            //    updated = true;
            //}

            if (UpdateTeamSkills(profilePostModel.TeamSkillIDs, investmentProfile) && !updated) updated = true;

            return updated;
        }

        private bool UpdateTeamSkills(string[] postedTeamSkillIDs, Investment investmentProfile) //<---------finished
        {
            bool updated = false;
            
            if (postedTeamSkillIDs != null)
            {
                List<int> postedTeamSkillIDList = Array.ConvertAll(postedTeamSkillIDs, s => int.Parse(s)).ToList();

                List<TeamSkill> profileTeamSkills = investmentProfile.TeamSkills.ToList();
                foreach (TeamSkill profileTeamSkill in profileTeamSkills)
                {
                    if (!postedTeamSkillIDList.Contains(profileTeamSkill.SkillID))
                    {
                        investmentProfile.TeamSkills.Remove(profileTeamSkill);
                        updated = true;
                    }
                }

                List<TeamSkill> teamSkills = db.TeamSkills.ToList();
                TeamSkill teamSkill = null;
                foreach (int postedTeamSkillID in postedTeamSkillIDList)
                {
                    teamSkill = teamSkills.Where(ts => ts.SkillID == postedTeamSkillID).FirstOrDefault();

                    if (!investmentProfile.TeamSkills.Contains(teamSkill))
                    {
                        investmentProfile.TeamSkills.Add(teamSkill);
                        updated = true;
                    }
                }
            }
            else //if (postedTeamSkillIDs == null)
            {
                investmentProfile.TeamSkills.Clear();
                updated = true;
            }

            return updated;
        }

        private bool UpdateTabOutcome(Investment investmentProfile, InvestmentProfilePostViewModel profilePostModel)
        {
            bool updated = false;

            updated = UpdateOutcomes(profilePostModel.OutcomeIDs, investmentProfile);

            if (UpdateInnovationLevels(profilePostModel.InnovationLevelIDs, investmentProfile) && !updated) updated = true;

            if (UpdateScalabilities(profilePostModel.ScalabilityIDs, investmentProfile) && !updated) updated = true;

            return updated;
        }

        private bool UpdateOutcomes(string[] postedOutcomeIDs, Investment investmentProfile) //<-------finished
        {
            bool updated = false;

            if (postedOutcomeIDs != null)
            {
                List<int> postedOutcomeIDList = Array.ConvertAll(postedOutcomeIDs, s => int.Parse(s)).ToList();

                List<Models.Shared.Outcome> profileOutcomes = investmentProfile.Outcomes.ToList();
                foreach (Models.Shared.Outcome profileOutcome in profileOutcomes)
                {
                    if (!postedOutcomeIDList.Contains(profileOutcome.OutcomeID))
                    {
                        investmentProfile.Outcomes.Remove(profileOutcome);
                        updated = true;
                    }
                }
                
                List<Models.Shared.Outcome> outcomes = db.Outcomes.ToList();
                Models.Shared.Outcome outcome = null;
                foreach (int postedOutcomeID in postedOutcomeIDList)
                {
                    outcome = outcomes.Where(o => o.OutcomeID == postedOutcomeID).FirstOrDefault();

                    if (!investmentProfile.Outcomes.Contains(outcome))
                    {
                        investmentProfile.Outcomes.Add(outcome);
                        updated = true;
                    }
                }
            }
            else //if (postedOutcomeIDs == null)
            {
                investmentProfile.Outcomes.Clear();
                updated = true;
            }

            return updated;
        }

        private bool UpdateInnovationLevels(string[] postedInnovationLevelIDs, Investment investmentProfile) //<------finished
        {
            bool updated = false;

            if (postedInnovationLevelIDs != null)
            {
                List<int> postedInnovationLevelIDList = Array.ConvertAll(postedInnovationLevelIDs, s => int.Parse(s)).ToList();

                List<InnovationLevel> profileInnovationLevels = investmentProfile.InnovationLevels.ToList();
                foreach (InnovationLevel profileInnovationLevel in profileInnovationLevels)
                {
                    if (!postedInnovationLevelIDList.Contains(profileInnovationLevel.InnovationLevelID))
                    {
                        investmentProfile.InnovationLevels.Remove(profileInnovationLevel);
                        updated = true;
                    }
                }

                List<InnovationLevel> innovationLevels = db.InnovationLevels.ToList();
                InnovationLevel innovationLevel = null;
                foreach (int postedInnovationLevelID in postedInnovationLevelIDList)
                {
                    innovationLevel = innovationLevels.Where(il => il.InnovationLevelID == postedInnovationLevelID).FirstOrDefault();

                    if (!investmentProfile.InnovationLevels.Contains(innovationLevel))
                    {
                        investmentProfile.InnovationLevels.Add(innovationLevel);
                        updated = true;
                    }
                }
            }
            else //if (postedInnovationLevelIDs == null)
            {
                investmentProfile.InnovationLevels.Clear();
                updated = true;
            }

            return updated;
        }

        private bool UpdateScalabilities(string[] postedScalabilityIDs, Investment investmentProfile) //<-------finished
        {
            bool updated = false;

            if (postedScalabilityIDs != null)
            {
                List<int> postedScalabilityIDList = Array.ConvertAll(postedScalabilityIDs, s => int.Parse(s)).ToList();

                List<Scalability> profileScalabilities = investmentProfile.Scalabilities.ToList();
                foreach (Scalability profileScalibility in profileScalabilities)
                {
                    if (!postedScalabilityIDList.Contains(profileScalibility.ScalabilityID))
                    {
                        investmentProfile.Scalabilities.Remove(profileScalibility);
                        updated = true;
                    }
                }

                List<Scalability> scalabilities = db.Scalabilities.ToList();
                Scalability scalability = null;
                foreach (int postedScalabilityID in postedScalabilityIDList)
                {
                    scalability = scalabilities.Where(s => s.ScalabilityID == postedScalabilityID).FirstOrDefault();

                    if (!investmentProfile.Scalabilities.Contains(scalability))
                    {
                        investmentProfile.Scalabilities.Add(scalability);
                        updated = true;
                    }
                }
            }
            else //if (postedScalabilityIDs == null)
            {
                investmentProfile.Scalabilities.Clear();
                updated = true;
            }

            return updated;
        }

        //[HttpGet]
        [Authorize(Roles = "Investor")]
        public ActionResult SubmitProfileForm(string id, bool cancel = false, string redirect = "", string redirectTab = "")
        {
            if (cancel)
            {
                //TempData["message"] = "Submission of form cancelled!";
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
                InvestmentID = investmentProfile.InvestmentID,
                InvestorUserID = investmentProfile.UserId,
                InvestorUserName = investmentProfile.User.UserName,
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
                Investment investmentProfile = db.Investments.Find(model.InvestmentID);
                if (investmentProfile == null)
                {
                    return HttpNotFound();
                }

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

        //---------------------------------------------------------------
       
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

            Investment investmentProfile = db.Investments.Find(id);

            if (investmentProfile == null)
            {
                return HttpNotFound();
            }
            //try
            investmentProfile.Locked = false;
            
            db.Entry(investmentProfile).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect) && redirect.ToUpper() != "INDEX")
            {
                if (redirect.ToUpper().Contains("DETAILS")) return RedirectToAction("ProfileDetails", new { id });
                else return RedirectToAction(redirect, new { id });
            }

            if (string.IsNullOrWhiteSpace(redirect) || redirect.ToUpper() == "INDEX")
                return RedirectToAction("Index", new { id });

            return RedirectToAction("ProfileDetails", new { id });
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