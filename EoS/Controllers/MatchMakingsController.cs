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
using EoS.Models.MMM;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EoS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MatchMakingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //---Index is renamed Result
        public ActionResult Index()
        {
            return RedirectToAction("Results");
        }

        // GET: MatchMakings
        public /*async Task<*/ActionResult/*>*/ Results(string dateTime, bool? sent/*, bool? sendReports*/)
        {
            List<MatchMaking> matchMakings = db.MatchMakings.OrderByDescending(mm => mm.MatchMakingDate).ToList();
            ViewBag.NoOfResults = matchMakings.Count();
            ViewBag.AllResults = true;
            //ViewBag.DateTime = null;
            //ViewBag.Sent = false;

            if (!string.IsNullOrEmpty(dateTime))
            {
                DateTime parsedDateTime = new DateTime();
                if (DateTime.TryParse(dateTime, out parsedDateTime))
                {
                    //matchMakings = matchMakings.Where(mm => DateTime.Compare(mm.MatchMakingDate, parsedDateTime) == 0).OrderBy(mm => mm.InvestmentId).ToList();
                    matchMakings = matchMakings.Where(mm => mm.MatchMakingDate.Date.Equals(parsedDateTime.Date) &&
                                    mm.MatchMakingDate.Hour.Equals(parsedDateTime.Hour) &&
                                    mm.MatchMakingDate.Minute.Equals(parsedDateTime.Minute) &&
                                    mm.MatchMakingDate.Second.Equals(parsedDateTime.Second)).OrderBy(mm => mm.InvestmentId).ToList();

                    ViewBag.AllResults = false;
                    ViewBag.DateTime = parsedDateTime;
                }
            }

            if (sent.HasValue)
            {
                ViewBag.Sent = sent.Value;
                matchMakings = matchMakings.Where(mm => mm.Sent == sent.Value).OrderByDescending(mm => mm.MatchMakingDate).OrderBy(mm => mm.NoOfMatches).ToList();
            }

            //if (sendReports.HasValue && reportSent.Value)
            //{
            //    //Send Emails

            //    List<ApplicationUser> investors = db.Users.Where(u => u.Roles.Where(r1 => r1.RoleId == db.Roles.Where(r2 => r2.Name == Role.Investor.ToString()).FirstOrDefault().Id).Any()).ToList();

            //    foreach (var investor in investors)
            //    {
            //        List<Models.Startup.Startup> matchedStartups = new List<Models.Startup.Startup>();

            //        foreach (var matchMaking in matchMakings)
            //        {
            //            var startupList = investor.Startups.Where(s => s.StartupID == matchMaking.StartupId);
            //            if (startupList.Any())
            //            {
            //                matchedStartups.Add(startupList.FirstOrDefault());              
            //            }
            //        }

            //        //Email code here
            //        if (EmailSent)
            //        foreach (var matchMaking in matchMakings)
            //        matchedMaking.Sent = true;
            //        string[] InvestorUserNames;
            //        int i = 0;
            //        foreach (investor in matchedInvestors)        
            //        InvestorUserNames[i++] = { matchedInvestor.UserName };
            //        return RedirectToAction("ReportSent", new { sent = true, investorUserNames = InvestorUserNames });
            //    }
            //}

            return View(matchMakings);
        }

        // GET: MatchMakings/Details/5
        public async Task<ActionResult> Details(int? id, bool? sent) //,string reportName)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MatchMaking matchMaking = db.MatchMakings.Find(id);
            if (matchMaking == null)
            {
                return HttpNotFound();
            }

            if (sent.HasValue && !sent.Value)
            {
                var smtpClients = db.SmtpClients.ToList();

                string investorId = db.Investments.Where(i => i.InvestmentID == matchMaking.InvestmentId).FirstOrDefault().UserId;
                ApplicationUser investor = db.Users.Find(investorId);

                foreach (Models.SMTP.SmtpClient smtpClient in smtpClients)
                {
                    if (smtpClient.Active)
                    {
                        try
                        {
                            //string bodyFormat = "\nDate: {0}\nStartup Project Id: {1}\n\nMatched {2} of {3} criterias in\nInvestment Profile {4}";
                            MailMessage message = new MailMessage
                            {
                                //Sender = new MailAddress(User.Identity.Name),
                                From = new MailAddress(User.Identity.Name),
                                Subject = "A Match Making Result from Enablers of Sweden",
                                Body = $"Date: {matchMaking.MatchMakingDate}\nStartup Project Id: {matchMaking.StartupId}\n\nMatched {matchMaking.NoOfMatches} of {matchMaking.MaxNoOfMatches} criterias in\nInvestment Profile {matchMaking.InvestmentId}",
                                //string.Format(bodyFormat, matchMaking.MatchMakingDate, matchMaking.StartupId, matchMaking.NoOfMatches, matchMaking.MaxNoOfMatches, matchMaking.InvestmentId),
                                IsBodyHtml = false
                            };
                            message.To.Add(new MailAddress(investor.Email));
                            message.Bcc.Add(new MailAddress(smtpClient.MailRecipient)); //Blind carbon copy
                            //var Admins = db.Users.Where(u => u.Roles.Where(r1 => r1.RoleId == db.Roles.Where(r2 => r2.Name == Role.Admin.ToString()).FirstOrDefault().Id).Any()).ToList();
                            var Admins = db.Users.Where(u => u.Roles.Where(r1 => r1.RoleId == db.Roles.Where(r2 => r2.Name == Role.Admin.ToString()).FirstOrDefault().Id).Any() && u.Email != smtpClient.MailRecipient).ToList();
                            foreach (var admin in Admins) //Blind carbon copy to all Admins except smtpClient.MailRecipient
                            {
                                message.Bcc.Add(admin.Email);
                            }
                            //message.Attachments.Add(new Attachment(HttpContext.Server.MapPath("~/Reports/" + reportName + ".pdf")));

                            using (var smtp = new SmtpClient())
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

                                //string[] InvestorUserNames = { investor.UserName };
                                TempData["investor_id"] = investor.Id;
                                TempData["investor_user_name"] = investor.UserName;
                                return RedirectToAction("ReportSent", new { sent = true, /*investorUserNames = InvestorUserNames,*/ matchMakingDate = matchMaking.MatchMakingDate });
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["error_message"] = ex.Message;
                            //return RedirectToAction("ReportSent", new { sent = false });
                        }
                    }
                }
                return RedirectToAction("ReportSent", new { sent = false, matchMakingDate = matchMaking.MatchMakingDate });
            }

            return View(matchMaking);
        }

        //Get
        public ActionResult ReportSent(bool? sent, /*string[] investorUserNames,*/ DateTime? matchMakingDate)
        {
            ViewBag.Sent = sent;
            if (matchMakingDate.HasValue) ViewBag.MatchMakingDate = matchMakingDate.Value;
            if (TempData.Any())
            {
                if (TempData.ContainsKey("investor_id")) ViewBag.InvestorId = TempData["investor_id"] as string;
                if (TempData.ContainsKey("investor_user_name")) ViewBag.InvestorUserName = TempData["investor_user_name"] as string;
                if (TempData.ContainsKey("error_message")) ViewBag.ErrorMessage = TempData["error_message"] as string;
                TempData.Clear();
            }

            return View();
        }

        // GET: MatchMakings/Run (Create)
        public ActionResult Motor(string Id) // Id==investmentId or InvestorId
        {
            RunMMMViewModel model = new RunMMMViewModel
            {
                Id = Id,
                //Project
                ProjectDomainSelected = true,
                //Funding
                FundingPhaseSelected = false,
                FundingAmountSelected = true,
                //Budget
                EstimatedExitPlanSelected = false,
                //Team
                TeamSkillsSelected = false,
                //Outcome
                OutcomesSelected = false,
                InnovationLevelSelected = false,
                ScalabilitySelected = false,

                MatchableInvestmentProfileList = new SelectList(GetMatchableInvestmentProfiles(Id), "InvestmentID", "InvestmentID"),
                MatchableStartupProjectList = new SelectList(GetMatchableStartupProjects(), "StartupID", "StartupID")
            };

            return View(model);
        }

        // POST: MatchMakings/Motor (Create)
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Motor(RunMMMViewModel model)
        {
            if (ModelState.IsValid)
            {
                int NoOfFailures = 0;
                int noOfMatches = 0;
                int MaxNoOfMatches = 0;
                if (model.ProjectDomainSelected) MaxNoOfMatches++;
                if (model.FundingPhaseSelected) MaxNoOfMatches++;
                if (model.FundingAmountSelected) MaxNoOfMatches++;
                if (model.EstimatedExitPlanSelected) MaxNoOfMatches++;
                if (model.TeamSkillsSelected) MaxNoOfMatches++;
                if (model.OutcomesSelected) MaxNoOfMatches++;
                if (model.InnovationLevelSelected) MaxNoOfMatches++;
                if (model.ScalabilitySelected) MaxNoOfMatches++;

                MatchMaking matchMaking = null;
                DateTime matchedDateTime = DateTime.Now;

                List<Investment> matchableInvestmentProfiles = GetMatchableInvestmentProfiles(model.MatchableInvestmentProfileId);
                List<Models.IdeaCarrier.Startup> matchableStartupProjects = GetMatchableStartupProjects(model.MatchableStartupProjectId);

                foreach (Investment matchableProfile in matchableInvestmentProfiles)
                {
                    foreach (Models.IdeaCarrier.Startup matchableProject in matchableStartupProjects)
                    {
                        matchMaking = new MatchMaking()
                        {
                            //MatchMakingId = matchableProfile.InvestmentID + "_" +matchableProject.StartupID,
                            InvestmentId = matchableProfile.InvestmentID,
                            StartupId = matchableProject.StartupID,
                            ProjectDomainMatched = null,
                            FundingPhaseMatched = null,
                            FundingAmountMatched = null,
                            EstimatedExitPlanMatched = null,
                            TeamSkillsMatched = null,
                            OutcomesMatched = null,
                            InnovationLevelMatched = null,
                            ScalabilityMatched = null,
                            NoOfMatches = 0,
                            MaxNoOfMatches = MaxNoOfMatches,
                            MatchMakingDate = matchedDateTime, 
                            Sent = false
                        };
                        matchMaking.NoOfMatches = 0;

                        if (model.ProjectDomainSelected)
                        {
                            if (matchableProfile.ProjectDomain.ProjectDomainName == matchableProject.ProjectDomain.ProjectDomainName)
                            {
                                matchMaking.ProjectDomainMatched = true;
                                matchMaking.NoOfMatches++;

                                if (model.FundingAmountSelected && matchableProfile.FundingAmounts.Contains(matchableProject.FundingAmount))
                                {
                                    matchMaking.FundingAmountMatched = true;
                                    matchMaking.NoOfMatches++;

                                    Match(matchableProfile, matchableProject, ref matchMaking, ref NoOfFailures, model); //Match the rest

                                    noOfMatches = matchMaking.NoOfMatches;

                                    List<MatchMaking> earlierMatchMakings = db.MatchMakings.Where(mm => mm.InvestmentId == matchMaking.InvestmentId && mm.StartupId == matchMaking.StartupId).ToList();
                                    
                                    //string matchMakingId = matchableProfile.InvestmentID + "_" +matchableProject.StartupID;
                                    //MatchMaking earlierMatchMaking = db.MatchMakings.Find(matchableProfile.InvestmentID + "_" + matchableProject.StartupID);

                                    if (earlierMatchMakings.Any()) //(earlierMatchMaking != null) 
                                    {
                                        MatchMaking earlierMatchMaking = earlierMatchMakings.FirstOrDefault();
                                        UpdateEarlierMatchMaking(earlierMatchMaking, matchMaking);

                                        db.Entry(earlierMatchMaking).State = EntityState.Modified;
                                    }
                                    else db.MatchMakings.Add(matchMaking);

                                    db.SaveChanges();

                                    //return RedirectToAction("Results", new { dateTime = matchedDateTime });
                                }
                                else
                                {
                                    NoOfFailures++;
                                    ModelState.AddModelError("", "Domain matches, but not Funding amount.");
                                }

                            }
                            else //ProjectDomain does not match
                            {
                                NoOfFailures++;
                                ModelState.AddModelError("", "Domain does not match.");
                                //return View(model);
                            }
                        }
                        else if (model.FundingAmountSelected)
                        {
                            if (matchableProfile.FundingAmounts.Contains(matchableProject.FundingAmount))
                            {
                                matchMaking.FundingAmountMatched = true;
                                matchMaking.NoOfMatches++;

                                //if (!investment.DueDate.HasValue) investment.DueDate = DateTime.Now;

                                Match(matchableProfile, matchableProject, ref matchMaking, ref NoOfFailures, model); //Match the rest

                                noOfMatches = matchMaking.NoOfMatches;

                                List<MatchMaking> earlierMatchMakings = db.MatchMakings.Where(mm => mm.InvestmentId == matchMaking.InvestmentId && mm.StartupId == matchMaking.StartupId).ToList();

                                //MatchMaking earlierMatchMaking = db.MatchMakings.Find(matchableProfile.InvestmentID + "_" +matchableProject.StartupID);

                                if (earlierMatchMakings.Any())  //if (earlierMatchMaking != null)
                                {
                                    MatchMaking earlierMatchMaking = earlierMatchMakings.FirstOrDefault();
                                    UpdateEarlierMatchMaking(earlierMatchMaking, matchMaking);

                                    db.Entry(earlierMatchMaking).State = EntityState.Modified;
                                }
                                else db.MatchMakings.Add(matchMaking);

                                db.SaveChanges();

                                //return RedirectToAction("Results", new { dateTime = matchedDateTime });
                            }
                            else
                            {
                                //ViewBag.Message += "Funding amounts do not match.<br />"; //<---where += "<br />..."
                                ModelState.AddModelError("", "Funding amount does not match.");
                            }
                        }
                        else
                        {
                            if (Match(matchableProfile, matchableProject, ref matchMaking, ref NoOfFailures, model))
                            {
                                noOfMatches = matchMaking.NoOfMatches;

                                List<MatchMaking> earlierMatchMakings = db.MatchMakings.Where(mm => mm.InvestmentId == matchMaking.InvestmentId && mm.StartupId == matchMaking.StartupId).ToList();

                                //MatchMaking earlierMatchMaking = db.MatchMakings.Find(matchableProfile.InvestmentID + "_" + matchableProject.StartupID);

                                if (earlierMatchMakings.Any()) //if (earlierMatchMaking != null)
                                {
                                    MatchMaking earlierMatchMaking = earlierMatchMakings.FirstOrDefault();
                                    UpdateEarlierMatchMaking(earlierMatchMaking, matchMaking);

                                    db.Entry(earlierMatchMaking).State = EntityState.Modified;
                                }
                                else db.MatchMakings.Add(matchMaking);

                                db.SaveChanges();

                                //return RedirectToAction("Results", new { dateTime = matchedDateTime });
                            }
                            else
                            {
                                //ViewBag.Message += "No matches found."; //for which investment?
                                //ModelState.AddModelError("", "No matches found.");
                                ModelState.AddModelError("", "The profiles and projects failed to be matched " + NoOfFailures.ToString() + " times.");
                            }
                        }
                    }
                }

                if (!matchableStartupProjects.Any())
                {
                    //ViewBag.Message += "No list of Startups exists. There is nothing to be matched.<br />";
                    ModelState.AddModelError("", "No list of projects exists. There is nothing to be matched.>");
                }
                //else if (startupProjects.Any() && NoOfFailures > 0)
                //{
                    //ViewBag.Message += " The profiles and projects failed to be matched " + NoOfFailures.ToString() + " times.<br />";
                //    ModelState.AddModelError("", "The profiles and projects failed to be matched " + NoOfFailures.ToString() + " times.");

                    //return RedirectToAction("Results", new { matchedDate = matchedDateTime }); //Matches done
                //}
                else return RedirectToAction("Results", new { dateTime = matchedDateTime });
            }
            else //Model state not valid
            {
                //ViewBag.Message += "No list of Startups exists. There is nothing to be matched.<br />";
                ModelState.AddModelError("", "Model state is not valid.");
            }

            model.ProjectDomainSelected = true;
            model.FundingAmountSelected = true;
            model.MatchableInvestmentProfileList = new SelectList(GetMatchableInvestmentProfiles(model.Id), "InvestmentID", "InvestmentID", model.MatchableInvestmentProfileId);
            //new SelectList(db.Investments.Where(i => i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID), "InvestmentID", "InvestmentID"/*, model.InvestmentProfileId*/);
            model.MatchableStartupProjectList = new SelectList(GetMatchableStartupProjects(), "StartupID", "StartupID", model.MatchableStartupProjectId);
            //new SelectList(db.Startups.Where(s => s.Locked && s.Approved && (DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).OrderBy(s => s.StartupID), "StartupID", "StartupID");
            //ViewBag.Message = "Model state is not valid.";
            return View(model);
        }

        private void UpdateEarlierMatchMaking(MatchMaking earlierMatchMaking, MatchMaking matchMaking)
        {
            if (earlierMatchMaking.ProjectDomainMatched != matchMaking.ProjectDomainMatched)
                earlierMatchMaking.ProjectDomainMatched = matchMaking.ProjectDomainMatched;

            if (earlierMatchMaking.FundingPhaseMatched != matchMaking.FundingPhaseMatched)
                earlierMatchMaking.FundingPhaseMatched = matchMaking.FundingPhaseMatched;

            if (earlierMatchMaking.FundingAmountMatched != matchMaking.FundingAmountMatched)
                earlierMatchMaking.FundingAmountMatched = matchMaking.FundingAmountMatched;

            if (earlierMatchMaking.EstimatedExitPlanMatched != matchMaking.EstimatedExitPlanMatched)
                earlierMatchMaking.EstimatedExitPlanMatched = matchMaking.EstimatedExitPlanMatched;

            if (earlierMatchMaking.TeamSkillsMatched != matchMaking.TeamSkillsMatched)
                earlierMatchMaking.TeamSkillsMatched = matchMaking.TeamSkillsMatched;

            if (earlierMatchMaking.OutcomesMatched != matchMaking.OutcomesMatched)
                earlierMatchMaking.OutcomesMatched = matchMaking.OutcomesMatched;

            if (earlierMatchMaking.InnovationLevelMatched != matchMaking.InnovationLevelMatched)
                earlierMatchMaking.InnovationLevelMatched = matchMaking.InnovationLevelMatched;

            if (earlierMatchMaking.ScalabilityMatched != matchMaking.ScalabilityMatched)
                earlierMatchMaking.ScalabilityMatched = matchMaking.ScalabilityMatched;

            earlierMatchMaking.NoOfMatches = matchMaking.NoOfMatches;
            earlierMatchMaking.MaxNoOfMatches = matchMaking.MaxNoOfMatches;
            earlierMatchMaking.MatchMakingDate = matchMaking.MatchMakingDate;
            earlierMatchMaking.Sent = matchMaking.Sent;
        }

        private bool Match(Investment investmentProfiles, Models.IdeaCarrier.Startup startupProject, ref MatchMaking matchMaking, ref int NoOfFailures, RunMMMViewModel model)
        {
            bool matchFound = false;

            if (model.FundingPhaseSelected)
            {
                if (investmentProfiles.FundingPhases.Contains(startupProject.FundingPhase))
                {
                    matchMaking.FundingPhaseMatched = true;
                    matchMaking.NoOfMatches++;
                    matchFound = true;
                }
                else
                {
                    matchMaking.FundingPhaseMatched = false;
                    NoOfFailures++;
                }
            }

            if (model.EstimatedExitPlanSelected)
            {
                if (investmentProfiles.EstimatedExitPlans.Contains(startupProject.EstimatedExitPlan))
                {
                    matchMaking.EstimatedExitPlanMatched = true;
                    matchMaking.NoOfMatches++;
                    matchFound = true;
                }
                else
                {
                    matchMaking.EstimatedExitPlanMatched = false;
                    NoOfFailures++;
                }
            }

            if (model.TeamSkillsSelected)
            {
                bool teamSkillsMatch = false;
                List<string> startupTeamWeaknesses = startupProject.TeamWeaknesses.Select(tw => tw.TeamWeaknessName).ToList();
                List<string> investmentTeamSkills = investmentProfiles.TeamSkills.Select(tw => tw.SkillName).ToList();

                if (investmentTeamSkills.Count() > startupTeamWeaknesses.Count())
                {
                    foreach (string startupTeamWeakness in startupTeamWeaknesses)
                    {
                        if (investmentTeamSkills.Contains(startupTeamWeakness))
                        {
                            teamSkillsMatch = true;
                            matchFound = true;
                        }
                    }
                }
                else
                {
                    foreach (string investmentTeamSkill in investmentTeamSkills)
                    {
                        if (startupTeamWeaknesses.Contains(investmentTeamSkill))
                        {
                            teamSkillsMatch = true;
                            matchFound = true;
                        }
                    }
                }
                matchMaking.TeamSkillsMatched = teamSkillsMatch;
                if (teamSkillsMatch) matchMaking.NoOfMatches++; else NoOfFailures++;
            }

            if (model.OutcomesSelected)
            {
                bool outcomesMatch = false;
                List<string> startupOutcomes = startupProject.Outcomes.Select(o => o.OutcomeName).ToList();
                List<string> investmentOutcomes = investmentProfiles.Outcomes.Select(o => o.OutcomeName).ToList();

                if (investmentOutcomes.Count() > startupOutcomes.Count())
                {
                    foreach (var startupOutcome in startupOutcomes)
                    {
                        if (investmentOutcomes.Contains(startupOutcome))
                        {
                            outcomesMatch = true;
                            matchFound = true;
                        }
                    }
                }
                else
                {
                    foreach (var investmentOutcome in investmentOutcomes)
                    {
                        if (startupOutcomes.Contains(investmentOutcome))
                        {
                            outcomesMatch = true;
                            matchFound = true;
                        }
                    }
                }
                matchMaking.OutcomesMatched = outcomesMatch;
                if (outcomesMatch) matchMaking.NoOfMatches++; else NoOfFailures++;
            }

            if (model.InnovationLevelSelected)
            {
                if (investmentProfiles.InnovationLevels.Contains(startupProject.InnovationLevel))
                {
                    matchMaking.InnovationLevelMatched = true;
                    matchMaking.NoOfMatches++;
                    matchFound = true;
                }
                else
                {
                    matchMaking.InnovationLevelMatched = false;
                    NoOfFailures++;
                }
            }
            if (model.ScalabilitySelected)
            {
                if (investmentProfiles.Scalabilities.Contains(startupProject.Scalability))
                {
                    matchMaking.ScalabilityMatched = true;
                    matchMaking.NoOfMatches++;
                    matchFound = true;
                }
                else
                {
                    matchMaking.ScalabilityMatched = false;
                    NoOfFailures++;
                }
            }
            return matchFound;
        }

        private List<Investment> GetMatchableInvestmentProfiles(string Id = "")
        {
            if (!string.IsNullOrEmpty(Id))
            {
                if (Id.ToUpper().StartsWith("IV")) //InvestmentId
                    return db.Investments.Where(i => i.InvestmentID == Id && i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID).ToList();
                else //InvestorId
                    return db.Investments.Where(i => i.UserId == Id && i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID).ToList();
            }
            return db.Investments.Where(i => i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID).ToList();    
        }

        private List<Models.IdeaCarrier.Startup> GetMatchableStartupProjects(string startupProjectId = "")
        {
            if (!string.IsNullOrEmpty(startupProjectId))
                return db.Startups.Where(s => s.StartupID == startupProjectId && s.Locked && s.Approved && (DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).ToList();

            return db.Startups.Where(s => s.Locked && s.Approved && (DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).OrderBy(s => s.StartupID).ToList();
        }

        // GET: MatchMakings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MatchMaking matchMaking = db.MatchMakings.Find(id);
            if (matchMaking == null)
            {
                return HttpNotFound();
            }
            return View(matchMaking);
        }

        // POST: MatchMakings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MatchMaking matchMaking = db.MatchMakings.Find(id);
            if (matchMaking != null)
            {
                db.MatchMakings.Remove(matchMaking);
                db.SaveChanges();
            }
            return RedirectToAction("Results");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: MatchMakings/DeleteAllResults
        //DeleteResults", new { 
        public ActionResult DeleteResults(string dateTime) //DateTime?
        {
            List<MatchMaking> matchMakings = db.MatchMakings.ToList();

            if (!string.IsNullOrEmpty(dateTime))
            {
                DateTime parsedDateTime = new DateTime();
                if (DateTime.TryParse(dateTime, out parsedDateTime))
                {
                    matchMakings = matchMakings.Where(mm => mm.MatchMakingDate.Date.Equals(parsedDateTime.Date) &&
                                    mm.MatchMakingDate.Hour.Equals(parsedDateTime.Hour) &&
                                    mm.MatchMakingDate.Minute.Equals(parsedDateTime.Minute) &&
                                    mm.MatchMakingDate.Second.Equals(parsedDateTime.Second)).ToList();
                }
            }

            if (matchMakings.Any())
            {
                foreach (MatchMaking matchMaking in matchMakings)
                    db.MatchMakings.Remove(matchMaking);

                db.SaveChanges();
            }
            return RedirectToAction("Results");
        }
    }
}