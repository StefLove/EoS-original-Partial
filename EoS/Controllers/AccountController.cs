using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using EoS.Models;
//using EoS.Models.SMTP;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using System.Data.Entity;

namespace EoS.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //---------Stefan
        [Authorize] //(Roles = "Admin")
        public ActionResult Index(string accountType = "", string orderBy = "") //<--------- descend ascend
        {
            ViewBag.AccountType = accountType;

;           if (!User.IsInRole(Models.Role.Admin.ToString())) return RedirectToAction("Index", "Manage");

            var DisplayUsers = new List<DisplayUserViewModel>();

            foreach (var user in db.Users)
            {
                if (string.IsNullOrEmpty(accountType) ||
                    accountType == Role.IdeaCarrier.ToString() && UserManager.IsInRole(user.Id, Role.IdeaCarrier.ToString()) ||
                    accountType == Role.Investor.ToString() && UserManager.IsInRole(user.Id, Role.Investor.ToString()))
                {
                    DisplayUsers.Add(new DisplayUserViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        StartDate = user.UserStartDate,
                        LastLoginDate = user.LastLoginDate,
                        ExpiryDate  = user.ExpiryDate,
                        CountryName = user.CountryName,
                        Role = UserManager.GetRoles(user.Id).FirstOrDefault()
                        //BlogComments = user.BlogComments
                    });
                }
                else if (accountType == Role.Admin.ToString())
                {
                    if ((User.Identity.Name != user.UserName) && UserManager.IsInRole(user.Id, Role.Admin.ToString()))
                    {
                        DisplayUsers.Add(new DisplayUserViewModel
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            UserFullName = user.UserFullName,
                            StartDate = user.UserStartDate,
                            LastLoginDate = user.LastLoginDate,
                            Role = Role.Admin.ToString()
                            //Blogs = user.Blogs*/
                        });
                    }
                }
            }

            if (!string.IsNullOrEmpty(orderBy))
                switch (orderBy.ToUpper())
                {
                    case "ROLE": return View(DisplayUsers.OrderBy(du => du.Role)); //break;
                    case "STARTDATE":     return View(DisplayUsers.OrderByDescending(du => du.StartDate)); //break;
                    case "LASTLOGINDATE": return View(DisplayUsers.OrderByDescending(du => du.LastLoginDate)); //break;
                    case "COUNTRYNAME": return View(DisplayUsers.OrderBy(du => du.CountryName)); //break;
                    case "USERNAME": return View(DisplayUsers.OrderBy(du => du.UserName)); //break;
                    case "USERFULLNAME": return View(DisplayUsers.OrderBy(du => du.UserFullName));
                    case "EXPIRYDATE": return View(DisplayUsers.OrderByDescending(du => du.ExpiryDate));
                }

            if (string.IsNullOrEmpty(accountType)) return View(DisplayUsers.OrderByDescending(du => du.StartDate).OrderBy(du => du.Role));
            return View(DisplayUsers.OrderByDescending(du => du.StartDate).OrderBy(du => du.UserName));
        }

        // GET: Account/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id, string accountType = "")
        {
            if (string.IsNullOrEmpty(id) || id == User.Identity.GetUserId()) //Null or trying to delete your own Admin account!
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return RedirectToAction("Index", "Manage"); 
            }

            ApplicationUser user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
                //return RedirectToAction("Index", "Manage");
            }

            ViewBag.AccountType = accountType;
            ViewBag.UserRole = UserManager.GetRoles(user.Id).FirstOrDefault(); //<--------ViewBag.UserRole + ??

            return View(user);
        }

        // POST: Account/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            if (user == null) return RedirectToAction("Details", "Account", new { accountType = Role.Admin.ToString() });
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index", "Manage");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Details(string id)
        {
            string userId = id;

            bool ownAccount = false;

            if (string.IsNullOrEmpty(id))
            {
                userId = User.Identity.GetUserId(); //View your own account
                ownAccount = true;
            }
            else if (userId == User.Identity.GetUserId()) ownAccount = true;

            //if (string.IsNullOrEmpty(userId))
            //{
            //    userId = User.Identity.GetUserId();
            //}

            ApplicationUser user = db.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            string countryName = "";
            
            if (!string.IsNullOrEmpty(user.CountryName))
            {
                Models.Shared.Country country = db.Countries.Where(c => c.CountryAbbreviation == user.CountryName).FirstOrDefault();
                countryName = country.CountryName + " (" + country.CountryAbbreviation + ")";
            }
            DisplayUserViewModel model = new DisplayUserViewModel
            {
                Id = /*user.Id == User.Identity.GetUserId() ? "" : */user.Id, //no need to pass id when viewing own account
                Role = UserManager.GetRoles(user.Id).FirstOrDefault(),
                StartDate = user.UserStartDate,
                LastLoginDate = user.LastLoginDate,
                UserFullName = user.UserFullName,
                Email = user.Email,
                UserName = user.UserName,
                CountryName = countryName,
                EmailConfirmed = user.EmailConfirmed,
                ExpiryDate = user.ExpiryDate,
                LockoutEndDate = user.LockoutEndDateUtc,
                LockoutEnabled = user.LockoutEnabled,
                NumberOfBlogs = user.Blogs.ToList().Count(),
                NumberOfBlogComments = user.BlogComments.Count()
            };

            ViewBag.OwnAccount = ownAccount;

            return View(model);
        }

        // GET: Account/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string id)
        {
            string userId = id;

            bool ownAccount = false;

            if (string.IsNullOrEmpty(userId))
            {
                userId = User.Identity.GetUserId();
                ownAccount = true;
            }
            else if (userId == User.Identity.GetUserId()) ownAccount = true;

            ApplicationUser user = db.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            int countryId = 1; //Sweden (SE)

            if (!string.IsNullOrEmpty(user.CountryName)) countryId = db.Countries.Where(c => c.CountryAbbreviation == user.CountryName).FirstOrDefault().CountryID;

            EditUserViewModel model = new EditUserViewModel
            {
                Id = user.Id,
                UserFirstName = user.UserFirstName,
                UserLastName = user.UserLastName,
                //UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                CountryId = countryId,
                ExpiryDate = user.ExpiryDate,
                LockoutEndDate = user.LockoutEndDateUtc,
                LockoutEnabled = user.LockoutEnabled,
                TwoFactorEnabled = user.TwoFactorEnabled,
                Role = UserManager.GetRoles(user.Id).FirstOrDefault()
            };

            ViewBag.OwnAccount = ownAccount;

            model.Countries = new SelectList(db.Countries, "CountryId", "CountryName");

            return View(model);
        }

        // POST: Account/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ActionName("Edit")]
        [ValidateAntiForgeryToken] /*Organisation,*/
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "Id,Role,UserFirstName,UserLastName,CountryId,Email,EmailConfirmed,ExpireDate,LockoutEndDateUtc,LockoutEnabled,TwoFactorEnabled")] EditUserViewModel model)
        //public ActionResult Edit(EditUserViewModel model)
        {
            bool ownAccount = false;

            if (ModelState.IsValid)
            {
                string userId = model.Id;

                if (string.IsNullOrEmpty(userId))
                {
                    userId = User.Identity.GetUserId();
                    ownAccount = true;
                }
                else if (userId == User.Identity.GetUserId()) ownAccount = true;

                ApplicationUser user = db.Users.Find(userId);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.Email = model.Email;
                user.EmailConfirmed = model.EmailConfirmed;
                user.UserName = model.Email; //model.UserName;

                user.CountryName = db.Countries.Where(c => c.CountryID == model.CountryId).FirstOrDefault().CountryAbbreviation;
                user.ExpiryDate = model.ExpiryDate;
                user.LockoutEndDateUtc = model.LockoutEndDate;
                user.LockoutEnabled = model.LockoutEnabled;
                user.TwoFactorEnabled = model.TwoFactorEnabled;

                if (model.Role == Role.Admin.ToString())
                {
                    user.UserFirstName = model.UserFirstName;
                    user.UserLastName = model.UserLastName;
                    user.CountryName = null;
                }

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                //IdentityResult identityResult = await UserManager.UpdateAsync(user);

                return RedirectToAction("Details", new { id = (ownAccount) ? "" : user.Id });
            }

            ViewBag.OwnAccount = ownAccount;

            model.Countries = new SelectList(db.Countries, "CountryId", "CountryName");

            return View(model);
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) //Already logged in!
            {
                if (User.IsInRole(Role.Admin.ToString()))
                {
                    return RedirectToAction("Index", "Manage");
                }
                else if (User.IsInRole(Role.IdeaCarrier.ToString()))
                {
                    return RedirectToAction("Index", "StartUps");
                }
                else if (User.IsInRole(Role.Investor.ToString()))
                {
                    return RedirectToAction("Index", "Investments");
                }
            }
            
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApplicationUser user = db.Users.Where(u => u.Email == model.Email).FirstOrDefault(); //Code moved out of SignInManager

            if (user != null && user.ExpiryDate.HasValue && user.ExpiryDate.Value.Date.Subtract(DateTime.Now.Date).Days < 0) //&& UserManager.IsInRole(user.Id, Role.IdeaCarrier.ToString())
                return RedirectToAction("AccountExpired", new { userName = user.UserName, userRole = UserManager.GetRoles(user.Id).FirstOrDefault() });

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            try
            {
                var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
                switch (result)
                {
                    case SignInStatus.Success:

                        //ApplicationUser user = db.Users.Where(u => u.Email == model.Email).FirstOrDefault();

                        //if (user.LastLoginDate == null && !User.IsInRole(Role.IdeaCarrier.ToString())) user.LockoutEnabled = false;
                        user.LastLoginDate = DateTime.Now;
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();

                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            string userRole = UserManager.GetRoles(user.Id).FirstOrDefault();
                            if (userRole == Role.Admin.ToString()) return RedirectToAction("Index","Manage");
                            else if (userRole == Role.IdeaCarrier.ToString()) return RedirectToAction("Index", "Startups");
                            else if (userRole == Role.Investor.ToString()) return RedirectToAction("Index", "Investments");
                        }
                        return RedirectToLocal(returnUrl);

                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe });
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                }
            }
            catch(Exception e)
            {
                ModelState.AddModelError("", e.Message);
                return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        // GET: /Account/Register
        //[Authorize(Roles = "Admin")] //
        [AllowAnonymous] //<------------------!!
        public ActionResult Register()  //string accountType = ""
        {
            ViewBag.AccountType = ""; //<-----

            if (User.Identity.IsAuthenticated) //Already logged in!
            {
                if (User.IsInRole(Role.Admin.ToString()))
                {
                    ViewBag.AccountType = Role.Admin.ToString(); //Only new Admin accounts by Admins (or a special Admin account)
                    //ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName"); //<---------??
                    return View();
                }
                else
                if (User.IsInRole(Role.IdeaCarrier.ToString()))
                {
                    return RedirectToAction("Index", "StartUps");
                }
                else if (User.IsInRole(Role.Investor.ToString()))
                {
                    return RedirectToAction("Index", "Investments");
                }
            }

            //ViewBag.AccountType = accountType;
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName"); //<---------
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        //[Authorize(Roles = "Admin")] //
        [AllowAnonymous] //<-----------------!!
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            bool newAdminAccount = model.Role == null; //&& User.IsInRole(Role.Admin.ToString()); //<----------!!
            string countryName = "";

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser();

                if (newAdminAccount) //AccountType == Role.Admin.ToString()
                {
                    if (model.UserFirstName != "anonymous") user.UserFirstName = model.UserFirstName;
                    if (model.UserLastName != "anonymous") user.UserLastName = model.UserLastName;
                    //if (model.Organisation != "anonymous") user.Organisation = model.Organisation;
                    user.LockoutEnabled = false;
                }
                else if (model.Role == RegisterRole.IdeaCarrier)
                {
                    if (model.UserFirstName == "anonymous" || model.UserLastName == "anonymous" || model.Organisation == "anonymous")
                    {
                        ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName");
                        return View(model);
                    }
                    Models.Shared.Country country = db.Countries.Where(c => c.CountryID == model.CountryId).FirstOrDefault();
                    user.CountryName = country.CountryAbbreviation;

                    countryName = country.CountryName + " (" + country.CountryAbbreviation + ")"; //for email

                    if (model.ExpiryDate.HasValue) user.ExpiryDate = model.ExpiryDate;
                    user.LockoutEndDateUtc = model.LockoutEndDate; //Only for IdeaCarriers
                }
                else if (model.Role == RegisterRole.Investor)
                {
                    Models.Shared.Country country = db.Countries.Where(c => c.CountryID == model.CountryId).FirstOrDefault();
                    user.CountryName = country.CountryAbbreviation;

                    string investorRandomId = ""; 
                    do
                    {
                        investorRandomId = "INV" + user.CountryName + HelpFunctions.GetShortCode();
                    } while (db.Users.Any(u => u.ExternalId == investorRandomId));
                    user.ExternalId = investorRandomId;

                    //countryName = country.CountryName + " (" + country.CountryAbbreviation + ")"; //for email
                    //user.ActiveInvestor = true; //<-------to be implemented
                    user.LockoutEnabled = false;
                }

                user.UserName = model.Email;
                user.Email = model.Email;
                user.UserStartDate = DateTime.Now; //<---move to ApplicationUser

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (newAdminAccount)
                    {
                        UserManager.AddToRole(user.Id, Role.Admin.ToString()); // AccountType == Role.Admin.ToString() Register another Admin account
                        return RedirectToAction("Index", "Account", new { accountType = Role.Admin.ToString() });
                    }
                    else
                    {
                        UserManager.AddToRole(user.Id, model.Role.ToString());

                        await SendEmail(model, user);

                        if (!User.Identity.IsAuthenticated && !User.IsInRole(Role.Admin.ToString())) //<----------------!!
                        {
                            user.LastLoginDate = DateTime.Now;
                            //if (model.Role == RegisterRole.IdeaCarrier) user.LockoutEnabled = false;
                            db.Entry(user).State = EntityState.Modified;
                            db.SaveChanges();

                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                            //await SendEmail(model, user.Id);
                        }

                        if (model.Role == RegisterRole.IdeaCarrier)
                        {
                            //await SendEmail(model, user.Id, user.CountryName);
                            return RedirectToAction("AddNewProject", "Startups");
                        }
                        else if (model.Role == RegisterRole.Investor)
                        {
                            //await SendEmail(model, user.Id, user.CountryName);
                            return RedirectToAction("AddNewProfile", "Investments");
                        }
                    }

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    // await sendEmail(model); <-------------------
                    //if (User.Identity.IsAuthenticated) {

                    //if (newAdminAccount) //AccountType == Role.Admin.ToString() User.IsInRole(Role.Admin.ToString()) <-----------------!!
                    //{
                    //    //if (AccountType == Role.Admin.ToString())
                    //    return RedirectToAction("Index", "Account", new { newAdminAccount = true }); //Another Admin account registered
                    //    //else return RedirectToAction("Index", "Manage"); //<-- maybe never
                    //}
                    //else
                    //if (model.Role == RegisterRole.IdeaCarrier)
                    //{
                    //    await SendEmail(model, user.Id);
                    //    return RedirectToAction("Index", "Account"); //<--------------------------------!!
                    //    //return RedirectToAction("Create", "StartUps");
                    //}
                    //else if (model.Role == RegisterRole.Investor)
                    //{
                    //    await SendEmail(model, user.Id);
                    //    return RedirectToAction("Index", "Account"); //<--------------------------------!!
                    //    //return RedirectToAction("Create", "Investments");
                    //}

                    //else if (/*User.Identity.IsAuthenticated && */model.Role == RegisterRole.IdeaCarrier) //User.IsInRole(Role.IdeaCarrier.ToString())
                    //{
                    //    //await SendEmail(model, user.Id);
                    //    return RedirectToAction("Index", "StartUps");
                    //}
                    //else if (/*User.Identity.IsAuthenticated && */model.Role == RegisterRole.IdeaCarrier) //User.IsInRole(Role.Investor.ToString())
                    //{
                    //    //await SendEmail(model, user.Id);
                    //    return RedirectToAction("Index", "Investments");
                    //}

                    return RedirectToAction("Index", "Home");
                }

                AddErrors(result); //<-----add error page if account already registered

                ViewBag.Error = "Error: <br />" + result.Succeeded + "<br />" + result.ToString();
                ModelState.AddModelError("","Account could not be created: < br />" + result.ToString());

                ViewBag.AccountType = (newAdminAccount) ? Role.Admin.ToString() : "";
                if (!User.IsInRole(Role.Admin.ToString())) ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName");
                return View(model);
            }

            // If we got this far, something failed, redisplay form
            //ViewBag.AccountType = AccountType;
            ViewBag.AccountType = (newAdminAccount) ? Role.Admin.ToString() : "";
            if (!User.IsInRole(Role.Admin.ToString())) ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName");
            return View(model);
        }
        
        private async Task SendEmail(RegisterViewModel model, ApplicationUser user) //RegisterViewModel model, string userId, string country
        {
            var smtpClients = db.SmtpClients.ToList();

            foreach (Models.SMTP.SmtpClient smtpClient in smtpClients)
            {
                if (smtpClient.Active)
                {
                    try
                    {
                        //string bodyFormat = "\nAnvändarnamn: {0} ({1})\nId: {2}\n\nNamn: {3}\nTelefonnummer: {4}\nOrganisation: {5}\nLand: {6}";
                        string role = "";
                        string body = "";

                        //if (model.Role.HasValue) role = model.Role.Value == RegisterRole.Investor ? "Investerare" : "Idébärare";

                        //if (UserManager.IsInRole(user.Id, RegisterRole.Investor.ToString())) role = "Investerare";

                        if (UserManager.IsInRole(user.Id, RegisterRole.IdeaCarrier.ToString())) //(role == RegisterRole.IdeaCarrier.ToString())
                        {
                            role = "Idébärare";
                            body = $"Ny {role}\nAnvändarnamn: {user.Email}\nId: {user.Id}\n\nNamn: {model.UserFirstName + " " + model.UserLastName}\nLand: {user.CountryName}\nOrganisation: {model.Organisation}\nTelefonnummer: {model.PhoneNumber}";
                        }
                        else if (UserManager.IsInRole(user.Id, RegisterRole.Investor.ToString())) //(role == RegisterRole.Investor.ToString())
                        {
                            role = "Investerare";
                            body = $"Ny {role}\nAnvändarnamn: {user.Email}\nId: {user.Id}\nExtern id: {user.ExternalId}\nLand: {user.CountryName}";
                            if (!string.IsNullOrEmpty(model.UserFirstName) || !string.IsNullOrEmpty(model.UserLastName)) body += $"\n\nNamn: {user.UserFirstName + " " + user.UserLastName}";
                            if (!string.IsNullOrEmpty(model.Organisation) && model.Organisation != "anonymous") body += $"\nOrganisation: {model.Organisation}";
                            if (!string.IsNullOrEmpty(model.PhoneNumber) && model.PhoneNumber.Trim() != "0") body += $"\nTelefonnummer: {model.PhoneNumber}";                        
                        }
                        else body = "Fel på rollen."; //Does not happen?

                        MailMessage message = new MailMessage
                        {
                            Sender = new MailAddress(model.Email),
                            From = new MailAddress(model.Email),
                            Subject = "Ny " + role,
                            //Body = string.Format(bodyFormat, model.Email, model.Role.ToString(), userId, model.UserFirstName + " " + model.UserLastName, model.PhoneNumber, model.Organisation, country),
                            Body = body,
                            IsBodyHtml = false
                        };
                        message.To.Add(new MailAddress(smtpClient.MailRecipient));
                        var Admins = db.Users.Where(u => u.Roles.Where(r1 => r1.RoleId == db.Roles.Where(r2 => r2.Name == Role.Admin.ToString()).FirstOrDefault().Id).Any() && u.Email != smtpClient.MailRecipient).ToList();
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
                        }
                    }
                    catch { }
                }
            }
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, model.ReturnUrl, model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult AccountExpired(string userName, string userRole)
        {
            ViewBag.AccountUserName = userName;
            ViewBag.AccountUserRole = userRole;

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}