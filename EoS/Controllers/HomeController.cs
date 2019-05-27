using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EoS.Models;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.Owin;

namespace EoS.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.HomeInfo = "";

            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(Role.Admin.ToString()))
                {
                    ViewBag.UserRole = Role.Admin.ToString();
                    //return RedirectToAction("Index", "Manage"); 
                }
                else if (User.IsInRole(Role.IdeaCarrier.ToString()))
                {
                    //ViewBag.UserRole = Models.Role.IdeaCarrier.ToString();
                    return RedirectToAction("Index", "StartUps");
                }
                /*...*/
            }

            //var HomeInfos = db.HomeInfos.ToList();
            try //if db is empty or HomeInfos doesn't exist, it will crash
            {
                if (db.HomeInfos.Count() > 0) ViewBag.HomeInfo = db.HomeInfos.FirstOrDefault().Text;
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "";

            return View();
        }

        // GET: Home/Contact
        [Authorize]
        public ActionResult Contact(string subject)
        {
            Models.Home.ContactEmailFormViewModel model = new Models.Home.ContactEmailFormViewModel
            {
                Subject = subject
            };
            ViewBag.Message = "Your contact form.";

            return View(model);
        }

        // POST: Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateInput(false)] <----------------------
        [Authorize]
        public async Task<ActionResult> Contact(Models.Home.ContactEmailFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                string userRole = "";

                if (User.IsInRole(Role.Admin.ToString()))
                {
                    userRole = Role.Admin.ToString();
                }
                else if (User.IsInRole(Role.IdeaCarrier.ToString()))
                {
                    userRole = Role.IdeaCarrier.ToString();
                }
                /*...*/

                var smtpClients = db.SmtpClients.ToList();

                foreach (Models.SMTP.SmtpClient smtpClient in smtpClients)
                {
                    if (smtpClient.Active)
                    {
                        try
                        {
                            MailMessage message = new MailMessage
                            {
                                //Sender = new MailAddress(User.Identity.Name),
                                From = new MailAddress(User.Identity.Name),
                                Subject = model.Subject,
                                Body = $"\nAvsändare:\n{User.Identity.Name} ({userRole})\n{User.Identity.GetUserId()}\n----------------------------------------\n{model.Message}",
                                //string.Format(bodyFormat, User.Identity.Name, userRole, User.Identity.GetUserId(), model.Message),
                                IsBodyHtml = false
                            };
                            var Admins = db.Users.Where(u => u.Roles.Where(r1 => r1.RoleId == db.Roles.Where(r2 => r2.Name == Role.Admin.ToString()).FirstOrDefault().Id).Any()).ToList();
                            foreach (var admin in Admins)
                            {
                                message.To.Add(admin.Email);
                            }
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
                                return RedirectToAction("MessageSent", new { sent = true });
                            }
                        }
                        catch
                        {
                            //return RedirectToAction("MessageSent", new { sent = false });
                        }
                    }
                }
                return RedirectToAction("MessageSent", new { sent = false });
            }
            return View(model);
        }

        [Authorize]
        public ActionResult MessageSent(bool? sent/*, string message*/) //<--------
        {
            ViewBag.Sent = sent;

            return View();
        }
    }
}
