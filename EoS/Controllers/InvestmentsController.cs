using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EoS.Models;
using EoS.Models.Investor;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Net.Mail;
//using EoS.Models.Shared;
//using Eos.Models.Investor;

namespace EoS.Controllers
{
    [Authorize]
    public class InvestmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Investments
        [Authorize(Roles = "Admin, Investor")]
        public ActionResult Index(string id, bool? matchable, string orderBy) //id==investorId
        {
            List<Investment> investments = null;

            ViewBag.Matchable = null;
            ViewBag.UserRole = "";
            ViewBag.InvestorId = "";
            ViewBag.InvestorUserName = "";

            if (User.IsInRole(Role.Admin.ToString()))
            {
                ViewBag.Matchable = null;

                if (!string.IsNullOrEmpty(id)) //The Admin looks at a special Investor's investment profiles
                {
                    if (!matchable.HasValue)
                    {
                        investments = db.Investments.Where(i => i.UserId == id).ToList();
                    }
                    else if (matchable.Value)
                    {
                        ViewBag.Matchable = true;
                        investments = db.Investments.Where(i => i.UserId == id && (i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0))).OrderBy(i => i.InvestmentID).ToList();
                    }
                    else
                    {
                        ViewBag.Matchable = false;
                        investments = db.Investments.Where(i => i.UserId == id && (!i.Locked || !i.Active || (i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) <= 0))).OrderBy(i => i.InvestmentID).ToList();
                    }
                    ViewBag.UserRole = Role.Admin.ToString();
                    ViewBag.InvestorId = id; //==investments.FirstOrDefault().User.Id
                    ViewBag.InvestorUserName = "";
                    ViewBag.InvestorExternalId = "";
                    ApplicationUser investor = db.Users.Find(id);
                    if (investor != null)
                    {
                        ViewBag.InvestorUserName = investor.UserName;
                        ViewBag.InvestorExternalId = investor.ExternalId;
                    }
                    //return View(investments);
                }
                else
                {
                    if (!matchable.HasValue)
                    {
                        investments = db.Investments.ToList();
                    }
                    else if (matchable.Value)
                    {
                        ViewBag.Matchable = true;
                        investments = db.Investments.Where(i => i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID).ToList();
                    }
                    else
                    {
                        ViewBag.Matchable = false;
                        investments = db.Investments.Where(i => !i.Locked || !i.Active || (i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) <= 0)).OrderBy(i => i.InvestmentID).ToList();
                    }
                    ViewBag.UserRole = Role.Admin.ToString();
                    //return View(investments);
                }
            }
            else //if (User.IsInRole("Investor"))
            {
                string currentUserId = User.Identity.GetUserId();
                ApplicationUser currentUser = db.Users.Find(currentUserId);
                investments = db.Investments.Where(u => u.UserId == currentUser.Id).ToList();
                ViewBag.UserRole = Role.Investor.ToString();
                ViewBag.InvestorExternalId = currentUser.ExternalId;
                //return View(investments);
            }

            if (investments == null) investments = db.Investments.ToList();

            if (!string.IsNullOrEmpty(orderBy))
                switch (orderBy.ToUpper())
                {
                    case "USERNAME": return View(investments.OrderBy(iv => iv.User.UserName)); //break;
                    case "INVESTMENTID": return View(investments.OrderBy(iv => iv.InvestmentID));
                    case "PROFILENAME": return View(investments.OrderBy(iv => iv.ProfileName));
                    case "COUNTRY": return View(investments.OrderBy(iv => iv.Country.CountryName));
                    case "SWEDISHREGION": return View(investments.OrderBy(iv => iv.SwedishRegion?.RegionName));
                    case "PROJECTDOMAINNAME": return View(investments.OrderBy(iv => iv.ProjectDomain?.ProjectDomainName));
                    case "MATCHMAKINGCOUNT": return View(investments.OrderBy(iv => iv.MatchMakings?.Count()));
                    case "LASTSAVEDDATE": return View(investments.OrderBy(iv => iv.LastSavedDate));
                    case "DUEDATE": return View(investments.OrderByDescending(iv => iv.DueDate));
                    case "CREATEDDATE": return View(investments.OrderByDescending(iv => iv.CreatedDate));
                    case "ACTIVE": return View(investments.OrderByDescending(iv => iv.Active));
                }

            return View(investments);
        }

        // GET: Investments/ProfileDetails/5
        [Authorize(Roles = "Admin, Investor")]
        public ActionResult ProfileDetails(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment investment = db.Investments.Find(id);
            if (investment == null)
            {
                return HttpNotFound();
            }

            ViewBag.UserRole = Role.Investor.ToString();

            if (User.IsInRole(Role.Admin.ToString()))
            {
                ViewBag.UserRole = Role.Admin.ToString();
                ViewBag.InvestorUserName = investment.User.UserName;
                return View(investment);
            }
            ViewBag.SwedishRegionName = "";
            if (investment.SwedishRegionID.HasValue) ViewBag.SwedishRegionName = db.SwedishRegions.Where(sr => sr.RegionID == investment.SwedishRegionID).FirstOrDefault().RegionName;

            return View(investment);
        }

        // GET: Investments/AddNewProfile
        [Authorize(Roles = "Investor")]
        public ActionResult AddNewProfile()
        {
            //these ViewBags for dropdownlist for create view that handle the selection options
            ViewBag.CountryId = new SelectList(db.Countries, "CountryID", "CountryName");
            ViewBag.RegionsId = new SelectList(db.SwedishRegions, "RegionID", "RegionName");
            ViewBag.InvestorMessage = db.InvestorMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();
            ViewBag.SwedishCountryId = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();

            return View();
        }

        // POST: Investments/AddNewProfile
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Investor")] //InvestmentID,UserId,TeamMemberSize,ExtraProjectDomains,TeamExperience,CreatedDate,Locked
        public async Task<ActionResult> AddNewProfile([Bind(Include = "ProfileName,CountryID,SwedishRegionID")] Investment investment, string submitCommand)
        {
            //if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Submit"))
            //{   //if user submit the form, lock it for editable
                //this must be before ModelState.IsValid
            //    investment.Locked = true;
            //    TryValidateModel(investment);
            //}

            if (ModelState.IsValid)
            {
                string countryAbbreviation = db.Countries.Find(investment.CountryID).CountryAbbreviation;
                string investmentRandomCode = "IV" + countryAbbreviation + HelpFunctions.GetShortCode();
                //check if the code is already exist
                while (db.Investments.Any(u => u.InvestmentID == investmentRandomCode))
                {
                    investmentRandomCode = "IV" + countryAbbreviation + HelpFunctions.GetShortCode();
                }
                investment.InvestmentID = investmentRandomCode;
                investment.UserId = User.Identity.GetUserId();
                investment.CreatedDate = DateTime.Now;
                investment.LastSavedDate = investment.CreatedDate; //DateTime.Now;  //<----------Changed

                string UserId = User.Identity.GetUserId();
                var userInvestments = db.Investments.Where(i => i.UserId == UserId);
                if (userInvestments.Any()) investment.DueDate = DateTime.Now.Date.AddDays(-1);

                //if (investment.Locked) //??
                investment.Active = true; //<------------ Active Added

                db.Investments.Add(investment);
                db.SaveChanges();

                if (investment.Active) //onödigt Locked??
                {
                    //if (investment.Locked)
                    investment.DueDate = DateTime.Now;

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
                                    From = new MailAddress(User.Identity.Name),
                                    Subject = "Ny profil påbörjad av " + User.Identity.GetUserName(),
                                    Body = $"Ny investeringsprofil påbörjad.\n\nInvesterarens användarnamn: {User.Identity.GetUserName()}\nInvesterarens id: {User.Identity.GetUserId()}\n\nProfilens id: {investment.InvestmentID}",
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
                                }
                            }
                            catch { }
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Proceed")) //<-----------"Proceed to the Profile form"
                {
                    return RedirectToAction("ProfileForm", "Investments", new { id = investment.InvestmentID });
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            //these ViewBags for dropdownlist for create view that handle the selection options
            ViewBag.CountryId = new SelectList(db.Countries, "CountryID", "CountryName");
            ViewBag.RegionsId = new SelectList(db.SwedishRegions, "RegionID", "RegionName");
            ViewBag.SwedishCountryId = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();
            ViewBag.InvestorMessage = db.InvestorMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();

            return View(investment);
        }

        // GET: Investments/Create
        [Authorize(Roles = "Investor")]
        public ActionResult AddProfile2()
        {
            //these ViewBags for dropdownlist for create view that handle the selection options
            ViewBag.CountryId = new SelectList(db.Countries, "CountryID", "CountryName"); //model = new InvestmentAddProfileViewModel
            ViewBag.RegionsId = new SelectList(db.SwedishRegions, "RegionID", "RegionName");
            ViewBag.InvestorMessage = db.InvestorMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();
            ViewBag.SwedishCountryId = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();

            return View();
        }

        // POST: Investments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Investor")]           //[Bind(Include = "ProfileName,CountryID,RegionID,TeamMemberSize,ExtraProjectDomains,TeamExperience")] InvestmentID,UserId,CreatedDate,Locked                                             
        public async Task<ActionResult> AddProfile2(InvestmentAddProfileViewModel model, string submitCommand)
        {
            Investment investment = new Investment(); //<----------newInvestment

            //if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Submit"))
            //{   //if user submit the form, lock it for editable
            //this must be before ModelState.IsValid
            //    investment.Locked = true;
            //    TryValidateModel(addedProfile);
            //}

            if (ModelState.IsValid)
            {
                string countryAbbreviation = db.Countries.Find(model.CountryID).CountryAbbreviation;
                string investmentRandomCode = ""; //"IV" + countryAbbreviation + HelpFunctions.GetShortCode();
                //check if the code is already exist
                //while (db.Investments.Any(u => u.InvestmentID == investmentRandomCode)) //<------Do Until
                //{
                //    investmentRandomCode = "IV" + countryAbbreviation + HelpFunctions.GetShortCode();
                //}
                do
                {
                    investmentRandomCode = "IV" + countryAbbreviation + HelpFunctions.GetShortCode();
                } while (db.Investments.Any(u => u.InvestmentID == investmentRandomCode));
                investment.InvestmentID = investmentRandomCode;

                investment.UserId = User.Identity.GetUserId();
                investment.CreatedDate = DateTime.Now;
                investment.LastSavedDate = investment.CreatedDate; //DateTime.Now; //<--------------Changed

                string UserId = User.Identity.GetUserId();
                var userInvestments = db.Investments.Where(i => i.UserId == UserId);
                if (userInvestments.Any()) investment.DueDate = DateTime.Now.Date.AddDays(-1);

                investment.ProfileName = model.ProfileName;
                investment.CountryID = model.CountryID;
                //investment.Country = db.Countries.Find(model.CountryID).CountryAbbreviation; <------------------------------------
                investment.SwedishRegionID = model.SwedishRegionID;
                //investment.SwedishRegion = db.SwedishRegions.Find(model.CountryID).RegionName; <----------------------------------
                //investment.ExtraProjectDomains = addedProfile.ExtraProjectDomains;
                //investment.TeamExperience = addedProfile.TeamExperience;
                //investment.TeamMemberSize = addedProfile.TeamMemberSize;

                //if (investment.Locked) 
                investment.Active = true; //??<------------Added

                db.Investments.Add(investment);
                db.SaveChanges();

                if (investment.Active) //<----------onödigt
                {
                    //if (investment.Locked)
                    investment.DueDate = DateTime.Now;

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
                                    From = new MailAddress(User.Identity.Name),
                                    Subject = "Ny profil påbörjad av " + User.Identity.GetUserName(),
                                    Body = $"Ny investeringsprofil påbörjad.\n\nInvesterarens användarnamn: {User.Identity.GetUserName()}\nInvesterarens id: {User.Identity.GetUserId()}\n\nProfilens id: {investment.InvestmentID}",
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
                                }
                            }
                            catch { }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Start")) //<-------------"Proceed to the Form"
                {
                    return RedirectToAction("Edit", "Investments", new { id = investment.InvestmentID });
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            //these ViewBags for dropdownlist for create view that handle the selection options
            ViewBag.CountryId = new SelectList(db.Countries, "CountryID", "CountryName");
            ViewBag.RegionsId = new SelectList(db.SwedishRegions, "RegionID", "RegionName");
            ViewBag.SwedishCountryId = db.Countries.Where(c => c.CountryName == "Sweden").Select(u => u.CountryID).Single();
            ViewBag.InvestorMessage = db.InvestorMessages.Where(m => m.Id == 1).Select(m => m.Text).Single().ToString();

            return View(investment);
        }

        // GET: Investments/ProfileForm/5
        [Authorize(Roles = "Admin, Investor")]
        public ActionResult ProfileForm(string id)
        {
            if (User.IsInRole(Role.Admin.ToString())) RedirectToAction("EditAdmin", new { id });

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment investment = db.Investments.Find(id);
            if (investment == null)
            {
                return HttpNotFound();
            }

            //These viewBags to handel the current values of the investment properties if exist and provide by other selections
            ViewBag.projectDomainIdEdit = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", investment.ProjectDomainID != null ? investment.ProjectDomainID : null);

            //Get the Project Domain Value Other to display the extra box
            //ViewBag.otherDomainId = db.ProjectDomains.Where(d => d.ProjectDomainName == "Other").Select(u => u.ProjectDomainID).Single();

            //Get the already checked value if exist
            PopulateAssignedCheckBoxsData(investment);

            return View(investment);
        }

        // POST: Investments/ProfileForm/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Investor")] //ExtraProjectDomains,TeamSkills  ,LastSavedDate ,Locked
        public ActionResult ProfileForm([Bind(Include = "UserId,InvestmentID,CountryID,SwedishRegionID,ProfileName,ProjectDomainID,FutureFundingNeeded,EstimatedBreakEven,TeamMemberSizeMoreThanOne,TeamHasExperience,ActiveInvestor,PossibleIncomeStreams,CreatedDate,DueDate,Active")] Investment investment, string[] selectedFundingPhases, string[] selectedFundingAmount, string[] selectedEstimatedExitPlans, string[] selectedTeamSkills, string[] selectedOutcomes, string[] selectedInnovationLevels, string[] selectedScalabilities, string activeTab, string submitCommand)
        {
            //Investment currentInvestment = db.Investments.Find(investment.InvestmentID);
            //if (db.Investments.Find(investment.InvestmentID).Locked)
            //if (IsLocked(investment.InvestmentID)) return RedirectToAction("ProfileDetails", new { id = investment.InvestmentID }); //<----------

            if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Submit"))
            {   //if user submit the form, lock it for editable
                //this must be before ModelState.IsValid

                investment.Locked = true;

                //
                if (selectedFundingAmount == null) //!selectedFundingAmount.Any()
                {
                    ModelState.AddModelError("FundingAmounts", "Select at least one Funding amount");
                }
                if (selectedFundingPhases == null)
                {
                    ModelState.AddModelError("FundingPhases", "Select at least one Funding phase");
                }
                if (selectedOutcomes == null)
                {
                    ModelState.AddModelError("Outcomes", "Select at least one Outcome");
                }
                if (selectedInnovationLevels == null)
                {
                    ModelState.AddModelError("InnovationLevels", "Select at least one Level of innovation");
                }
                if (selectedScalabilities == null)
                {
                    ModelState.AddModelError("Scalabilities", "Select at least one Scalability");
                }
                if (selectedEstimatedExitPlans == null)
                {
                    ModelState.AddModelError("EstimatedExitPlans", "Select at least one Estimated exit plan");
                }
                TryValidateModel(investment);
            }

            if (ModelState.IsValid)
            {
                investment.LastSavedDate = DateTime.Now;
                db.Entry(investment).State = EntityState.Modified;
                UpdateInvestmentCheckBoxesData(selectedTeamSkills, selectedFundingAmount, selectedFundingPhases, selectedOutcomes, selectedInnovationLevels, selectedScalabilities, selectedEstimatedExitPlans, investment);
                db.SaveChanges();

                //if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Submit"))
                //currentInvestment = db.Investments.Find(investment.InvestmentID);

                //if (db.Investments.Find(investment.InvestmentID).Locked) return RedirectToAction("ProfileDetails", new { id = investment.InvestmentID });

            }
            else ModelState.AddModelError("", "Validation Error !!");

            //if (IsLocked(investment.InvestmentID)) return RedirectToAction("ProfileDetails", new { id = investment.InvestmentID });

            //These viewBags to handel the current values of the investment properties if exist and provide by other selections
            ViewBag.projectDomainIdEdit = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", investment.ProjectDomainID != null ? investment.ProjectDomainID : null);

            //Get the Project Domain Value Other to display the extra box
            //ViewBag.otherDomainId = db.ProjectDomains.Where(d => d.ProjectDomainName == "Other").Select(u => u.ProjectDomainID).Single(); //<---------Changed

            //Get the already checked value again if it exists to pass the ViewBags
            PopulateAssignedCheckBoxsData(investment, selectedTeamSkills, selectedFundingAmount, selectedFundingPhases, selectedOutcomes, selectedInnovationLevels, selectedScalabilities, selectedEstimatedExitPlans);

            return View(investment);
        }

        // GET: Investments/ProfileForm/5
        [Authorize(Roles = "Admin, Investor")]
        public ActionResult ProfileForm2(string id)
        {
            if (User.IsInRole(Role.Admin.ToString())) RedirectToAction("EditAdmin", new { id });

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Investment investment = db.Investments.Find(id);
            if (investment == null)
            {
                return HttpNotFound();
            }

            //These viewBags to handel the current values of the investment properties if exist and provide by other selections
            ViewBag.projectDomainIdEdit = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", investment.ProjectDomainID != null ? investment.ProjectDomainID : null);

            //Get the Project Domain Value Other to display the extra box
            //ViewBag.otherDomainId = db.ProjectDomains.Where(d => d.ProjectDomainName == "Other").Select(u => u.ProjectDomainID).Single(); //<-------Changed

            //Get the already checked value if exist
            PopulateAssignedCheckBoxsData(investment);

            return View(investment);
        }

        // POST: Investments/ProfileForm/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Investor")] //UserId,CountryID,SwedishRegionID,CreatedDate,LastSavedDate,Locked, [Bind(Include = "InvestmentID,ProfileName,ProjectDomainID,TeamMemberSize,ExtraProjectDomains,TeamExperience,AlreadySpentTime,AlreadySpentMoney,TeamVision,FixedRole,EstimatedBreakEven,IncomeStream,PayingCustomers,Active")]
        public ActionResult ProfileForm2(InvestmentProfileFormViewModel profileForm, string[] selectedFundingPhases, string[] selectedFundingAmount, string[] selectedEstimatedExitPlans, string[] selectedTeamSkills, string[] selectedOutcomes, string[] selectedInnovationLevels, string[] selectedScalabilities, string activeTab, string submitCommand)
        {
            Investment investment = db.Investments.Find(profileForm.InvestmentID);

            if (!string.IsNullOrEmpty(submitCommand) && submitCommand.StartsWith("Submit"))
            {   //if user submit the form, lock it for editable
                //this must be before ModelState.IsValid
                investment.Locked = true;

                if (selectedFundingAmount == null)
                {
                    ModelState.AddModelError("FundingAmounts", "Select at least one Funding amount");
                }
                if (selectedFundingPhases == null)
                {
                    ModelState.AddModelError("FundingPhases", "Select at least one Funding phase");
                }
                if (selectedOutcomes == null)
                {
                    ModelState.AddModelError("Outcomes", "Select at least one Outcome");
                }
                if (selectedInnovationLevels == null)
                {
                    ModelState.AddModelError("InnovationLevels", "Select at least one Level of innovation");
                }
                if (selectedScalabilities == null)
                {
                    ModelState.AddModelError("Scalabilities", "Select at least one Scalability");
                }
                if (selectedEstimatedExitPlans == null)
                {
                    ModelState.AddModelError("EstimatedExitPlans", "Select at least one Estimated exit plan");
                }
                TryValidateModel(profileForm);
            }

            if (ModelState.IsValid)
            {
                investment.LastSavedDate = DateTime.Now;  
                db.Entry(investment).State = EntityState.Modified;
                UpdateInvestmentCheckBoxesData(selectedTeamSkills, selectedFundingAmount, selectedFundingPhases, selectedOutcomes, selectedInnovationLevels, selectedScalabilities, selectedEstimatedExitPlans, investment);
                db.SaveChanges();
                return RedirectToAction("ProfileDetails", new { id = investment.InvestmentID });
            }
            //These viewBags to handel the current values of the investment properties if exist and provide by other selections
            ViewBag.projectDomainIdEdit = new SelectList(db.ProjectDomains, "ProjectDomainID", "ProjectDomainName", investment.ProjectDomainID != null ? investment.ProjectDomainID : null);

            //Get the Project Domain Value Other to display the extra box
            ViewBag.otherDomainId = db.ProjectDomains.Where(d => d.ProjectDomainName == "Other").Select(u => u.ProjectDomainID).Single();

            //Get the already checked value again if exist to pass the viewBags
            PopulateAssignedCheckBoxsData(investment, selectedTeamSkills, selectedFundingAmount, selectedFundingPhases, selectedOutcomes, selectedInnovationLevels, selectedScalabilities, selectedEstimatedExitPlans);

            return View(profileForm);
        }

        // GET: Investments/EditAdmin/5
        [Authorize(Roles = "Admin")]
        public ActionResult EditAdmin(string id) //ProfileFormAdmin
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

            if (!investment.DueDate.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);  //RedirectToAction("ProfileDetails", new { id = id });  //<---Doesn't work
            }

            InvestmentEditAdminViewModel model = new InvestmentEditAdminViewModel
            {
                InvestmentId = investment.InvestmentID,
                DueDate = investment.DueDate,
                InvestorName = investment.User.UserName
            };

            return View(model);
        }

        // POST: Investments/EditAdmin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] /*[Bind(Include = "StartupID,ProjectSummary,Locked,Approved")]*/
        public ActionResult EditAdmin(InvestmentEditAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                Investment investment = db.Investments.Find(model.InvestmentId);
                if (investment == null)
                {
                    return HttpNotFound();
                }

                if (model.DueDate.HasValue) investment.DueDate = model.DueDate.Value.Date;
                else if (investment.DueDate.HasValue) investment.DueDate = null;

                db.Entry(investment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ProfileDetails", new { id = investment.InvestmentID });
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
            Investment investment = db.Investments.Find(id);
            if (investment == null)
            {
                return HttpNotFound();
            }
            return View(investment);
        }

        // POST: Investments/RemoveProfile/5
        [Authorize(Roles = "Admin, Investor")]
        [HttpPost, ActionName("RemoveProfile")]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveConfirmed(string id)
        {
            Investment investment = db.Investments.Find(id);
            db.Investments.Remove(investment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void UpdateInvestmentCheckBoxesData(string[] selectedSkills, string[] selectedFundingAmount, string[] selectedFundingPhases,  string[] selectedOutcomes, string[] selectedInnovationLevels, string[] selectedScalabilities, string[] selectedEstimatedExitPlans, Investment investment)
        {
            //********************************************************************************//
            if (selectedSkills != null) //<---------------------------Added
            {
                //this is the new selection list
                var selectedSkillsHS = new HashSet<String>(selectedSkills);
                var currentSkillsInvestment = db.Investments.Include(w => w.TeamSkills).Where(s => s.InvestmentID == investment.InvestmentID).Single();
                var investmentCurrentSkills = new HashSet<int>(currentSkillsInvestment.TeamSkills.Select(w => w.SkillID));

                foreach (var skill in db.TeamSkills)
                {
                    if (selectedSkillsHS.Contains(skill.SkillID.ToString()))
                    {
                        //if the selection not in previous selections, add it
                        if (!investmentCurrentSkills.Contains(skill.SkillID))
                        {
                            investment.TeamSkills.Add(skill);
                        }
                    }
                    else
                    {
                        //if the selection in previous selections but not in the new selected list, remove it 
                        if (investmentCurrentSkills.Contains(skill.SkillID))
                        {
                            investment.TeamSkills.Remove(skill);
                        }
                    }
                }
            }
            else
            {
                //Delete Any previous selection if any
                //this is the current or previous saved selections
                var currentSkillsInvestment = db.Investments.Include(w => w.TeamSkills).Where(s => s.InvestmentID == investment.InvestmentID).Single();
                var startupCurrentWeaknesses = new HashSet<int>(currentSkillsInvestment.TeamSkills.Select(w => w.SkillID));

                foreach (var skill in db.TeamSkills)
                {
                    investment.TeamSkills.Remove(skill);
                }
            }


            if (selectedFundingAmount != null)
            {
                //this is the new selection list
                var selectedFundingAmountHS = new HashSet<String>(selectedFundingAmount);
                //this is the current or previous saved selections
                var currentFundingAmount = db.Investments.Include(f => f.FundingAmounts).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investomentCurrentFundingAmount = new HashSet<int>(currentFundingAmount.FundingAmounts.Select(f => f.FundingAmountID));

                foreach (var amount in db.FundingAmounts)
                {
                    if (selectedFundingAmountHS.Contains(amount.FundingAmountID.ToString()))
                    {
                        //if the selection not in previous selections, add it
                        if (!investomentCurrentFundingAmount.Contains(amount.FundingAmountID))
                        {
                            investment.FundingAmounts.Add(amount);
                        }
                    }
                    else
                    {
                        //if the selection in previous selections but not in the new selected list, remove it 
                        if (investomentCurrentFundingAmount.Contains(amount.FundingAmountID))
                        {
                            investment.FundingAmounts.Remove(amount);
                        }
                    }
                }
            }
            else
            {
                //Delete Any previous selection if any
                //this is the current or previous saved selections
                var currentFundingAmount = db.Investments.Include(f => f.FundingAmounts).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investmentCurrentFundingAmount = new HashSet<int>(currentFundingAmount.FundingAmounts.Select(f => f.FundingAmountID));

                foreach (var amount in db.FundingAmounts)
                {
                    
                      investment.FundingAmounts.Remove(amount);
                }
            }

            if (selectedFundingPhases != null)
            {
                //this is the new selection list
                var selectedFundingPhasestHS = new HashSet<String>(selectedFundingPhases);
                //this is the current or previous saved selections
                var currentFundingPhases = db.Investments.Include(f => f.FundingPhases).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investomentCurrentFundingPhases = new HashSet<int>(currentFundingPhases.FundingPhases.Select(f => f.FundingPhaseID));

                foreach (var phase in db.FundingPhases)
                {
                    if (selectedFundingPhasestHS.Contains(phase.FundingPhaseID.ToString()))
                    {
                        //if the selection not in previous selections, add it
                        if (!investomentCurrentFundingPhases.Contains(phase.FundingPhaseID))
                        {
                            investment.FundingPhases.Add(phase);
                        }
                    }
                    else
                    {
                        //if the selection in previous selections but not in the new selected list, remove it 
                        if (investomentCurrentFundingPhases.Contains(phase.FundingPhaseID))
                        {
                            investment.FundingPhases.Remove(phase);
                        }
                    }
                }
            }
            else
            {
                //Delete Any previous selection if any
                //this is the current or previous saved selections
                var currentFundingPhases = db.Investments.Include(f => f.FundingPhases).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investomentCurrentFundingPhases = new HashSet<int>(currentFundingPhases.FundingPhases.Select(f => f.FundingPhaseID));

                foreach (var phase in db.FundingPhases)
                {

                    investment.FundingPhases.Remove(phase);
                }
            }

            //********************************************************************************//

            if (selectedOutcomes != null)
            {
                //this is the new selection list
                var selectedOutcomesHS = new HashSet<String>(selectedOutcomes);
                //this is the current or previous saved selections
                var currentInvestmentOutcomes = db.Investments.Include(o => o.Outcomes).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investmentCurrentOutcomes = new HashSet<int>(currentInvestmentOutcomes.Outcomes.Select(o => o.OutcomeID));

                foreach (var outcome in db.Outcomes)
                {
                    if (selectedOutcomesHS.Contains(outcome.OutcomeID.ToString()))
                    {
                        //if the selection not in previous selections, add it
                        if (!investmentCurrentOutcomes.Contains(outcome.OutcomeID))
                        {
                            investment.Outcomes.Add(outcome);
                        }
                    }
                    else
                    {
                        //if the selection in previous selections but not in the new selected list, remove it 
                        if (investmentCurrentOutcomes.Contains(outcome.OutcomeID))
                        {
                            investment.Outcomes.Remove(outcome);
                        }
                    }
                }
            }
            else
            {
                //Delete Any previous selection if any
                //this is the current or previous saved selections
                var currentInvestmentOutcomes = db.Investments.Include(o => o.Outcomes).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investmentCurrentOutcomes = new HashSet<int>(currentInvestmentOutcomes.Outcomes.Select(o => o.OutcomeID));

                foreach (var outcome in db.Outcomes)
                {

                    investment.Outcomes.Remove(outcome);
                }
            }

            //********************************************************************************//

            if (selectedInnovationLevels != null)
            {
                //this is the new selection list
                var selectedInnovationLevelsHS = new HashSet<String>(selectedInnovationLevels);
                //this is the current or previous saved selections
                var currentInnovationLevels = db.Investments.Include(nl => nl.InnovationLevels).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investomentCurrentInnovationLevels = new HashSet<int>(currentInnovationLevels.InnovationLevels.Select(nl => nl.InnovationLevelID));

                foreach (var level in db.InnovationLevels)
                {
                    if (selectedInnovationLevelsHS.Contains(level.InnovationLevelID.ToString()))
                    {
                        //if the selection not in previous selections, add it
                        if (!investomentCurrentInnovationLevels.Contains(level.InnovationLevelID))
                        {
                            investment.InnovationLevels.Add(level);
                        }
                    }
                    else
                    {
                        //if the selection in previous selections but not in the new selected list, remove it 
                        if (investomentCurrentInnovationLevels.Contains(level.InnovationLevelID))
                        {
                            investment.InnovationLevels.Remove(level);
                        }
                    }
                }
            }
            else
            {
                //Delete Any previous selection if any
                //this is the current or previous saved selections
                var currentInnovationLevels = db.Investments.Include(nl => nl.InnovationLevels).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investomentCurrentInnovationLevels = new HashSet<int>(currentInnovationLevels.InnovationLevels.Select(nl => nl.InnovationLevelID));

                foreach (var level in db.InnovationLevels)
                {

                    investment.InnovationLevels.Remove(level);
                }
            }

            //********************************************************************************//

            if (selectedScalabilities != null)
            {
                //this is the new selection list
                var selectedScalabilitiesHS = new HashSet<String>(selectedScalabilities);
                //this is the current or previous saved selections
                var currentScalabilities = db.Investments.Include(s => s.Scalabilities).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investomentCurrentScalabilities = new HashSet<int>(currentScalabilities.Scalabilities.Select(s => s.ScalabilityID));

                foreach (var scalability in db.Scalabilities)
                {
                    if (selectedScalabilitiesHS.Contains(scalability.ScalabilityID.ToString()))
                    {
                        //if the selection not in previous selections, add it
                        if (!investomentCurrentScalabilities.Contains(scalability.ScalabilityID))
                        {
                            investment.Scalabilities.Add(scalability);
                        }
                    }
                    else
                    {
                        //if the selection in previous selections but not in the new selected list, remove it 
                        if (investomentCurrentScalabilities.Contains(scalability.ScalabilityID))
                        {
                            investment.Scalabilities.Remove(scalability);
                        }
                    }
                }
            }
            else
            {
                //Delete Any previous selection if any
                //this is the current or previous saved selections
                var currentScalabilities = db.Investments.Include(s => s.Scalabilities).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investomentCurrentScalabilities = new HashSet<int>(currentScalabilities.Scalabilities.Select(s => s.ScalabilityID));

                foreach (var scalability in db.Scalabilities)
                {

                    investment.Scalabilities.Remove(scalability);
                }
            }

            //********************************************************************************//

            if (selectedEstimatedExitPlans != null)
            {
                //this is the new selection list
                var selectedEstimatedExitPlansHS = new HashSet<String>(selectedEstimatedExitPlans);
                //this is the current or previous saved selections
                var currentEstimatedExitPlans = db.Investments.Include(e => e.EstimatedExitPlans).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investomentCurrentEstimatedExitPlans = new HashSet<int>(currentEstimatedExitPlans.EstimatedExitPlans.Select(e => e.EstimatedExitPlanID));

                foreach (var plan in db.EstimatedExitPlans)
                {
                    if (selectedEstimatedExitPlansHS.Contains(plan.EstimatedExitPlanID.ToString()))
                    {
                        //if the selection not in previous selections, add it
                        if (!investomentCurrentEstimatedExitPlans.Contains(plan.EstimatedExitPlanID))
                        {
                            investment.EstimatedExitPlans.Add(plan);
                        }
                    }
                    else
                    {
                        //if the selection in previous selections but not in the new selected list, remove it 
                        if (investomentCurrentEstimatedExitPlans.Contains(plan.EstimatedExitPlanID))
                        {
                            investment.EstimatedExitPlans.Remove(plan);
                        }
                    }
                }
            }
            else
            {
                //Delete Any previous selection if any
                //this is the current or previous saved selections
                var currentEstimatedExitPlans = db.Investments.Include(e => e.EstimatedExitPlans).Where(i => i.InvestmentID == investment.InvestmentID).Single();
                var investomentCurrentEstimatedExitPlans = new HashSet<int>(currentEstimatedExitPlans.EstimatedExitPlans.Select(e => e.EstimatedExitPlanID));

                foreach (var plan in db.EstimatedExitPlans)
                {
                    investment.EstimatedExitPlans.Remove(plan);
                }
            }
            //********************************************************************************//
        }

        private void PopulateAssignedCheckBoxsData(Investment investment)
        {
            var allSkills = db.TeamSkills;
            var currentSkillsInvestment = db.Investments.Include(s => s.TeamSkills).Where(i => i.InvestmentID == investment.InvestmentID).Single(); //<----------Added
            var investmentSkills = new HashSet<int>(currentSkillsInvestment.TeamSkills.Select(w => w.SkillID));
            var skillsViewModel = new List<TeamSkillViewModel>();

            var allFundingAmounts = db.FundingAmounts;
            var currentInvestmentFundingAmount = db.Investments.Include(f => f.FundingAmounts).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var InvestmentFundingAmount = new HashSet<int>(currentInvestmentFundingAmount.FundingAmounts.Select(F => F.FundingAmountID));
            var FundingAmountViewModel = new List<InvestorFundingAmountViewModel>();

            var allFundingPhases = db.FundingPhases;
            var currentInvestmentFundingPhase = db.Investments.Include(f => f.FundingPhases).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var InvestmentFundingPhase = new HashSet<int>(currentInvestmentFundingPhase.FundingPhases.Select(F => F.FundingPhaseID));
            var FundingPhaseViewModel = new List<InvestorFundingPhaseViewModel>();

            var allOutcomes = db.Outcomes;
            var currentInvestmentOutcome = db.Investments.Include(o => o.Outcomes).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var InvestmentOutcome = new HashSet<int>(currentInvestmentOutcome.Outcomes.Select(o => o.OutcomeID));
            var OutcomeViewModel = new List<InvestorOutcomeViewModel>();

            var allInnovationLevels = db.InnovationLevels;
            var currentInnovationLevels = db.Investments.Include(inn => inn.InnovationLevels).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var InvestmentInnovationLevels = new HashSet<int>(currentInnovationLevels.InnovationLevels.Select(inn => inn.InnovationLevelID));
            var InnovationLevelsViewModel = new List<InvestorInnovationLevelViewModel>();

            var allScalabilities = db.Scalabilities;
            var currentInvestorScalabilities = db.Investments.Include(s => s.Scalabilities).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var InvestmentScalabilities = new HashSet<int>(currentInvestorScalabilities.Scalabilities.Select(s => s.ScalabilityID));
            var ScalabilitiesViewModel = new List<InvestorScalabilityViewModel>();

            var allEstimatedExitPlans = db.EstimatedExitPlans;
            var currentInvestorEstimatedExitPlans = db.Investments.Include(e => e.EstimatedExitPlans).Where(i => i.InvestmentID == investment.InvestmentID).Single();
            var InvestmentEstimatedExitPlans = new HashSet<int>(currentInvestorEstimatedExitPlans.EstimatedExitPlans.Select(e => e.EstimatedExitPlanID));
            var EstimatedExitPlanViewModel = new List<InvestorEstimatedExitPlanViewModel>();

            foreach (var skill in allSkills) //<-----------------------------Added
            {
                skillsViewModel.Add(new TeamSkillViewModel
                {
                    SkillID = skill.SkillID,
                    SkillName = skill.SkillName,
                    Assigned = investmentSkills.Contains(skill.SkillID)
                });
            }
            ViewBag.skillsViewModel = skillsViewModel;

            foreach (var amount in allFundingAmounts)
            {
                FundingAmountViewModel.Add(new InvestorFundingAmountViewModel { FundingAmountID = amount.FundingAmountID, FundingAmountValue = amount.FundingAmountValue, Assigned = InvestmentFundingAmount.Contains(amount.FundingAmountID) });
            }
            ViewBag.fundingAmountViewModel = FundingAmountViewModel;

            foreach (var phase in allFundingPhases)
            {
                FundingPhaseViewModel.Add(new InvestorFundingPhaseViewModel { FundingPhaseID = phase.FundingPhaseID, FundingPhaseName = phase.FundingPhaseName, Assigned = InvestmentFundingPhase.Contains(phase.FundingPhaseID) });
            }
            ViewBag.fundingPhaseViewModel = FundingPhaseViewModel;

            foreach (var outcome in allOutcomes)
            {
                OutcomeViewModel.Add(new InvestorOutcomeViewModel { OutcomeID = outcome.OutcomeID, OutcomeName = outcome.OutcomeName, Assigned = InvestmentOutcome.Contains(outcome.OutcomeID) });
            }
            ViewBag.outcomeViewModel = OutcomeViewModel;

            foreach (var level in allInnovationLevels)
            {
                InnovationLevelsViewModel.Add(new InvestorInnovationLevelViewModel { InnovationLevelID = level.InnovationLevelID, InnovationLevelName = level.InnovationLevelName, Assigned = InvestmentInnovationLevels.Contains(level.InnovationLevelID) });
            }
            ViewBag.innovationLevelsViewModel = InnovationLevelsViewModel;

            foreach (var scalability in allScalabilities)
            {
                ScalabilitiesViewModel.Add(new InvestorScalabilityViewModel { ScalabilityID = scalability.ScalabilityID, ScalabilityName = scalability.ScalabilityName, Assigned = InvestmentScalabilities.Contains(scalability.ScalabilityID) });
            }
            ViewBag.scalabilitiesViewModel = ScalabilitiesViewModel;

            foreach (var exitPlan in allEstimatedExitPlans)
            {                                                    
                EstimatedExitPlanViewModel.Add(new InvestorEstimatedExitPlanViewModel { EstimatedExitPlanID = exitPlan.EstimatedExitPlanID, EstimatedExitPlanName = exitPlan.EstimatedExitPlanName, Assigned = InvestmentEstimatedExitPlans.Contains(exitPlan.EstimatedExitPlanID) });
            }
            ViewBag.estimatedExitPlanViewModel = EstimatedExitPlanViewModel;

        }
                                         //<-------remove investment, not used
        private void PopulateAssignedCheckBoxsData(Investment investment, string[] selectedSkills, string[] selectedFundingAmount, string[] selectedFundingPhases, string[] selectedOutcomes, string[] selectedInnovationLevels, string[] selectedScalabilities, string[] selectedEstimatedExitPlans)
        {
            //populate the selected checkbox from selectedstring after validation errors

            var allSkills = db.TeamSkills; //<-------------------------Added
            var selectedSkillsHS = new HashSet<String>();
            var skillsViewModel = new List<TeamSkillViewModel>();

            if (selectedSkills != null)
            {
                selectedSkillsHS = new HashSet<String>(selectedSkills);
            }

            var allFundingAmounts = db.FundingAmounts;
            var selectedInvestmentFundingAmountHS = new HashSet<String>();
            var FundingAmountViewModel = new List<InvestorFundingAmountViewModel>();

            if (selectedFundingAmount != null)
            {
                selectedInvestmentFundingAmountHS = new HashSet<String>(selectedFundingAmount);
            }

            var allFundingPhases = db.FundingPhases;
            var selectedInvestmentFundingPhaseHS = new HashSet<String>();
            var FundingPhaseViewModel = new List<InvestorFundingPhaseViewModel>();
            if (selectedFundingPhases != null)
            {
                selectedInvestmentFundingPhaseHS = new HashSet<String>(selectedFundingPhases);
            }

            var allOutcomes = db.Outcomes;
            var selectedInvestmentOutcomeHS = new HashSet<String>();
            var OutcomeViewModel = new List<InvestorOutcomeViewModel>();
            if (selectedOutcomes != null)
            {
                selectedInvestmentOutcomeHS = new HashSet<String>(selectedOutcomes);
            }

            var allInnovationLevels = db.InnovationLevels;
            var selectedInvestmentInnovationLevelsHS = new HashSet<String>();
            var InnovationLevelsViewModel = new List<InvestorInnovationLevelViewModel>();
            if (selectedInnovationLevels != null)
            {
                selectedInvestmentInnovationLevelsHS = new HashSet<String>(selectedInnovationLevels);
            }

            var allScalabilities = db.Scalabilities;
            var selectedInvestmentScalabilitiesHS = new HashSet<String>();
            var ScalabilitiesViewModel = new List<InvestorScalabilityViewModel>();
            if (selectedScalabilities != null)
            {
                selectedInvestmentScalabilitiesHS = new HashSet<String>(selectedScalabilities);
            }

            var allEstimatedExitPlans = db.EstimatedExitPlans;
            var selectedInvestmentEstimatedExitPlansHS = new HashSet<String>();
            var EstimatedExitPlanViewModel = new List<InvestorEstimatedExitPlanViewModel>();
            if (selectedEstimatedExitPlans != null)
            {
                selectedInvestmentEstimatedExitPlansHS = new HashSet<String>(selectedEstimatedExitPlans);
            }

            foreach (var skill in allSkills) //<----------------------------Added
            {
                skillsViewModel.Add(new TeamSkillViewModel
                {
                    SkillID = skill.SkillID,
                    SkillName = skill.SkillName,
                    Assigned = selectedSkillsHS.Contains(skill.SkillID.ToString())
                });
            }
            ViewBag.skillsViewModel = skillsViewModel;

            foreach (var amount in allFundingAmounts)
            {
                FundingAmountViewModel.Add(new InvestorFundingAmountViewModel { FundingAmountID = amount.FundingAmountID, FundingAmountValue = amount.FundingAmountValue, Assigned = selectedInvestmentFundingAmountHS.Contains(amount.FundingAmountID.ToString()) });
            }
            ViewBag.fundingAmountViewModel = FundingAmountViewModel;

            foreach (var phase in allFundingPhases)
            {
                FundingPhaseViewModel.Add(new InvestorFundingPhaseViewModel { FundingPhaseID = phase.FundingPhaseID, FundingPhaseName = phase.FundingPhaseName, Assigned = selectedInvestmentFundingPhaseHS.Contains(phase.FundingPhaseID.ToString()) });
            }
            ViewBag.fundingPhaseViewModel = FundingPhaseViewModel;

            foreach (var outcome in allOutcomes)
            {
                OutcomeViewModel.Add(new InvestorOutcomeViewModel { OutcomeID = outcome.OutcomeID, OutcomeName = outcome.OutcomeName, Assigned = selectedInvestmentOutcomeHS.Contains(outcome.OutcomeID.ToString()) });
            }
            ViewBag.outcomeViewModel = OutcomeViewModel;

            foreach (var level in allInnovationLevels)
            {
                InnovationLevelsViewModel.Add(new InvestorInnovationLevelViewModel { InnovationLevelID = level.InnovationLevelID, InnovationLevelName = level.InnovationLevelName, Assigned = selectedInvestmentInnovationLevelsHS.Contains(level.InnovationLevelID.ToString()) });
            }
            ViewBag.innovationLevelsViewModel = InnovationLevelsViewModel;

            foreach (var scalability in allScalabilities)
            {
                ScalabilitiesViewModel.Add(new InvestorScalabilityViewModel { ScalabilityID = scalability.ScalabilityID, ScalabilityName = scalability.ScalabilityName, Assigned = selectedInvestmentScalabilitiesHS.Contains(scalability.ScalabilityID.ToString()) });
            }
            ViewBag.scalabilitiesViewModel = ScalabilitiesViewModel;

            foreach (var exitPlan in allEstimatedExitPlans)
            {
                EstimatedExitPlanViewModel.Add(new InvestorEstimatedExitPlanViewModel { EstimatedExitPlanID = exitPlan.EstimatedExitPlanID, EstimatedExitPlanName = exitPlan.EstimatedExitPlanName, Assigned = selectedInvestmentEstimatedExitPlansHS.Contains(exitPlan.EstimatedExitPlanID.ToString()) });
            }
            ViewBag.estimatedExitPlanViewModel = EstimatedExitPlanViewModel;
        }

        [Authorize(Roles = "Investor")]
        public ActionResult Activate(string id, string redirect) //Activate or Unactivate
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment investment = db.Investments.Find(id);

            if (investment == null)
            {
                return HttpNotFound();
            }
            //try
            investment.Active = !investment.Active;

            db.Entry(investment).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(redirect))
            {
                /*if (redirect == "ProjectDetails") */
                return RedirectToAction(redirect, new { id }); //<-------------------------Changed
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

        [Authorize(Roles = "Admin")]
        public ActionResult Unlock(string id, string redirect)
        {
            if (id == null)
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
                /*if (redirect == "ProfileDetails")*/
                return RedirectToAction("ProjectDetails", new { id }); //<-----------------Changed
            }

            return RedirectToAction("Index");
        }

        // GET: Investments/Reminder
        [Authorize(Roles = ("Admin"))]
        public ActionResult Reminder(string id, string subject, string message, string redirect) //id==investmentId
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Investment Investment = db.Investments.Find(id);

            if (Investment == null)
            {
                return HttpNotFound();
            }
            else
            {
                ReminderInvestmentViewModel model = new ReminderInvestmentViewModel
                {
                    InvestmentId = id, //==InvestmentId
                    InvestorEmail = Investment.User.Email,
                    InvestorId = Investment.UserId,
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