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
        public /*async Task<*/ActionResult/*>*/ Results(DateTime? matchedDate, bool? sent/*, bool? sendReports*/)
        {
            List<MatchMaking> matchMakings = db.MatchMakings.ToList();

            if (matchedDate.HasValue)
            {
                matchMakings = matchMakings.Where(mm => DateTime.Compare(mm.MatchMakingDate, matchedDate.Value) == 0).OrderBy(mm => mm.InvestmentId).ToList();
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
            //
            //                
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

                                string[] InvestorUserNames = { investor.UserName };
                                return RedirectToAction("ReportSent", new { sent = true, investorUserNames = InvestorUserNames, matchMakingDate = matchMaking.MatchMakingDate }); //InvestorUserNames used while testing. ,reportName = reportName
                            }
                        }
                        catch
                        {
                            //return RedirectToAction("ReportSent", new { sent = false });
                        }
                    }
                }
                return RedirectToAction("ReportSent", new { sent = false, matchMakingDate = matchMaking.MatchMakingDate });
            }

            return View(matchMaking);
        }

        [Authorize]
        public ActionResult ReportSent(bool? sent, string[] investorUserNames, DateTime? matchMakingDate)
        {
            ViewBag.Sent = sent;
            ViewBag.InvestorUserNames = investorUserNames;
            if (matchMakingDate.HasValue) ViewBag.MatchMakingDate = matchMakingDate.Value;

            return View();
        }

        // GET: MatchMakings/Run (Create)
        public ActionResult Motor(string Id) // Id==investmentId or InvestorId
        {
            SelectList investments;

            if (!string.IsNullOrEmpty(Id))
            {
                if (Id.ToUpper().StartsWith("IV")) //InvestmentId
                {
                    investments = new SelectList(db.Investments.Where(i => i.InvestmentID == Id && i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID), "InvestmentID", "InvestmentID");
                    //if (investments.Count() == 1) investments.FirstOrDefault().Selected = true;
                }
                else //InvestorId
                {
                    investments = investments = new SelectList(db.Investments.Where(i => i.UserId == Id && i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID), "InvestmentID", "InvestmentID");
                }
            }
            else //All
            {
                investments = new SelectList(db.Investments.Where(i => i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID), "InvestmentID", "InvestmentID");
            }

            SelectList startups = new SelectList(db.Startups.Where(s => s.Locked && s.Approved && (DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).OrderBy(s => s.StartupID), "StartupID", "StartupID");
            //if (startups.Count() == 1) startups.FirstOrDefault().Selected = true;

            RunMMMViewModel model = new RunMMMViewModel
            {
                ProjectDomainSelected = true,
                FundingAmountSelected = true,
                Investments = investments,
                //InvestmentId = Id,
                Startups = startups
            };

            return View(model);
        }

        // POST: MatchMakings/Motor (Create)
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Motor([Bind(Include = "MatchMakingIdInvestmentId,StartupId,ProjectDomainMatched,FundingAmountMatched")] MatchMaking matchMaking)
        public ActionResult Motor(RunMMMViewModel model)
        {
            if (ModelState.IsValid) //(&& matched)
            {
                int NoOfFailures = 0; //<------------------------------------------

                MatchMaking matchMaking = new MatchMaking();

                /*
                 List<Investment> investmentss;
                 if (string.IsNullOrEmpty(model.InvestmentId))
                    {
                        investments = db.Investments.Where(i => i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID).ToList(); //All investments selected
                    }
                 */
                //else
                var investment = db.Investments.Where(i => i.InvestmentID == model.InvestmentId).FirstOrDefault(); //investment

                List<Models.IdeaCarrier.Startup> startups;

                if (string.IsNullOrEmpty(model.StartupId))
                {
                    startups = db.Startups.Where(s => s.Locked && s.Approved && (!s.DeadlineDate.HasValue || s.DeadlineDate.HasValue && DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).OrderBy(s => s.StartupID).ToList(); //All startups selected
                }
                else
                {
                    startups = db.Startups.Where(s => s.StartupID == model.StartupId).ToList();
                }

                DateTime matchedDateTime = DateTime.Now; //default(DateTime);

                //MMM

                //foreach (Investment investment in investments) matchMaking.InvestmentId = investment.InvestmentID;

                foreach (Models.IdeaCarrier.Startup startup in startups)
                {
                    //var existingMatchMaking s= investment.MatchMakings.Where(mm => mm.StartupId == startup.StartupID).ToList();

                    if (!investment.MatchMakings.Where(mm => mm.StartupId == startup.StartupID).Any()) //already matched
                    {
                        matchMaking.StartupId = startup.StartupID;
                        matchMaking.InvestmentId = investment.InvestmentID;

                        if (model.ProjectDomainSelected)
                        {
                            if (investment.ProjectDomain.ProjectDomainName == startup.ProjectDomain.ProjectDomainName) //investment.ExtraProjectDomains (of type string)
                            {
                                matchMaking.ProjectDomainMatched = true;
                                matchMaking.NoOfMatches++;

                                if (model.FundingAmountSelected && investment.FundingAmounts.Contains(startup.FundingAmount))
                                {
                                    matchMaking.FundingAmountMatched = true;
                                    matchMaking.NoOfMatches++;

                                    //if (!investment.DueDate.HasValue) investment.DueDate = DateTime.Now;

                                    Match(investment, startup, ref matchMaking, ref NoOfFailures, model); //Match the rest

                                    matchMaking.MatchMakingDate = matchedDateTime;

                                    db.MatchMakings.Add(matchMaking);
                                    db.SaveChanges();

                                    return RedirectToAction("Results", new { matchedDate = matchedDateTime });
                                }
                                else
                                {
                                    NoOfFailures++;
                                    ViewBag.Message += "Project Domains match, but not Funding Amounts.<br />"; //<---remove
                                    ModelState.AddModelError("", "Project Domains match, but not Funding Amounts.");
                                }

                            }
                            else //ProjectDomain does not matched
                            {
                                //model.ProjectDomainSelected = true;
                                //model.FundingAmountSelected = true;
                                //model.Investments = new SelectList(db.Investments.Where(i => i.Locked && i.Active), "InvestmentID", "InvestmentID");
                                //model.Startups = new SelectList(db.Startups.Where(s => s.Locked && s.Active && s.Approved), "StartupID", "StartupID");
                                NoOfFailures++;
                                ViewBag.Message += "Project Domains do not match.<br />"; //<----where
                                ModelState.AddModelError("", "Project Domains do not match.< br />");
                                //return View(model);
                            }
                        }
                        else if (model.FundingAmountSelected)
                        {
                            if (investment.FundingAmounts.Contains(startup.FundingAmount))
                            {
                                matchMaking.FundingAmountMatched = true;
                                matchMaking.NoOfMatches++;

                                //if (!investment.DueDate.HasValue) investment.DueDate = DateTime.Now;

                                Match(investment, startup, ref matchMaking, ref NoOfFailures, model); //Match the rest

                                matchMaking.MatchMakingDate = matchedDateTime;

                                db.MatchMakings.Add(matchMaking);
                                db.SaveChanges();

                                return RedirectToAction("Results", new { matchedDate = matchedDateTime });
                            }
                            else
                            {
                                ViewBag.Message += "Funding amounts do not match.<br />"; //<---where += "<br />..."
                                ModelState.AddModelError("", "Funding amounts do not match.");
                            }
                        }
                        else
                        {
                            if (Match(investment, startup, ref matchMaking, ref NoOfFailures, model)) //Match the rest <---------------model saknas
                            {
                                matchMaking.MatchMakingDate = matchedDateTime;

                                db.MatchMakings.Add(matchMaking);
                                db.SaveChanges();

                                return RedirectToAction("Results", new { matchedDate = matchedDateTime });
                            }
                            else
                            {
                                ViewBag.Message += "No matches found.<br />"; //for which investment?
                                ModelState.AddModelError("", "No matches found.");
                            }
                        }
                    }
                    else //already matched
                    {
                        //model.ProjectDomainSelected = true;
                        //model.FundingAmountSelected = true;
                        //model.Investments = new SelectList(db.Investments.Where(i => i.Locked && i.Active), "InvestmentID", "InvestmentID");
                        //model.Startups = new SelectList(db.Startups.Where(s => s.Locked && s.Active && s.Approved), "StartupID", "StartupID");
                        //ViewBag.Message = "Investment " + investment.InvestmentID + " and Startup " + startup.StartupID + " has already been matched, delete the post from MatchMakings if you want to redo it !";
                        int startupIndex = db.MatchMakings.ToList().FindIndex(mm => mm.StartupId == startup.StartupID);
                        ViewBag.Message += "<a href=\"~MatcMakings/Details\"" + startupIndex + "/>Investment " + investment.InvestmentID + " and Startup " + startup.StartupID + "</a> is already matched, delete the record from the MatchMakings if you want to redo it !<br />";
                        ModelState.AddModelError("", "No list of Startups exists. There is nothing to be matched.");
                        //return View(model);
                    }
                }

                if (startups.Any())
                {
                    ViewBag.Message += " The profiles and projects failed to be matched " + NoOfFailures.ToString() + " times.<br />";
                    ModelState.AddModelError("", "The profiles and projects failed to be matched " + NoOfFailures.ToString() + " times.");
                    //return RedirectToAction("Results", new { matchedDate = matchedDateTime }); //Matches done
                }
                else
                {
                    ViewBag.Message += "No list of Startups exists. There is nothing to be matched.<br />";
                    ModelState.AddModelError("", "No list of Startups exists. There is nothing to be matched.>");
                }
            }
            else //Model state not valid
            {
                ViewBag.Message += "No list of Startups exists. There is nothing to be matched.<br />";
                ModelState.AddModelError("", "Model state is not valid.");
            }

            model.ProjectDomainSelected = true;
            model.FundingAmountSelected = true;
            model.Investments = new SelectList(db.Investments.Where(i => i.Locked && i.Active && (!i.DueDate.HasValue || i.DueDate.HasValue && DateTime.Compare(i.DueDate.Value, DateTime.Now) > 0)).OrderBy(i => i.InvestmentID), "InvestmentID", "InvestmentID", model.InvestmentId); //<---remove model.InvestmentId later
            model.Startups = new SelectList(db.Startups.Where(s => s.Locked && s.Approved && (DateTime.Compare(s.DeadlineDate.Value, DateTime.Now) > 0)).OrderBy(s => s.StartupID), "StartupID", "StartupID");
            //ViewBag.Message = "Model state is not valid.";
            return View(model);
        }

        private bool Match(Investment investment, Models.IdeaCarrier.Startup startup, ref MatchMaking matchMaking, ref int NoOfFailures, RunMMMViewModel model)
        {
            bool matchFound = false;

            if (model.FundingPhaseSelected && investment.FundingPhases.Contains(startup.FundingPhase))
            {
                matchMaking.FundingPhaseMatched = true;
                matchMaking.NoOfMatches++;
                matchFound = true;
            }
            else NoOfFailures++;

            if (model.EstimatedExitPlanSelected && investment.EstimatedExitPlans.Contains(startup.EstimatedExitPlan))
            {
                matchMaking.EstimatedExitPlanMatched = true;
                matchMaking.NoOfMatches++;
                matchFound = true;
            }
            else NoOfFailures++;

            if (model.OutcomesSelected)
            {
                bool outcomesMatch = false;
                if (investment.Outcomes.Count() > startup.Outcomes.Count())
                {
                    foreach (var startupOutcome in startup.Outcomes)
                    {
                        if (investment.Outcomes.Contains(startupOutcome))
                        {
                            outcomesMatch = true;
                            matchFound = true;
                        }
                        //else outcomesMatch = false;
                    }
                }
                else
                {
                    foreach (var investmentOutcome in investment.Outcomes)
                    {
                        if (startup.Outcomes.Contains(investmentOutcome))
                        {
                            outcomesMatch = true;
                            matchFound = true;
                        }
                        //else outcomesMatch = false;
                    }
                }
                matchMaking.OutcomesMatched = outcomesMatch;
            }

            if (model.InnovationLevelSelected && investment.InnovationLevels.Contains(startup.InnovationLevel))
            {
                matchMaking.InnovationLevelMatched = true;
                matchMaking.NoOfMatches++;
                matchFound = true;
            }
            else NoOfFailures++;

            if (model.ScalabilitySelected && investment.Scalabilities.Contains(startup.Scalability))
            {
                matchMaking.ScalabilityMatched = true;
                matchMaking.NoOfMatches++;
                matchFound = true;
            }
            else NoOfFailures++;

            //investment.TeamSkills; startup.TeamWeaknesses

            return matchFound;
        }

        // GET: MatchMakings/Edit/5
        //No Editing Allowed !!
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MatchMaking matchMaking = db.Matchmakings.Find(id);
        //    if (matchMaking == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(matchMaking);
        //}

        //// POST: MatchMakings/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "MatchMakingId,MatchMakingDate,Sent,InvestmentId,StartupId,ProjectDomainMatched,FundingAmountMatched")] MatchMaking matchMaking)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(matchMaking).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(matchMaking);
        //}

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
            db.MatchMakings.Remove(matchMaking);
            db.SaveChanges();
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