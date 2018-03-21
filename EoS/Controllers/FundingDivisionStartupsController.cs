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

namespace EoS.Controllers
{
    public class FundingDivisionStartupsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FundingDivisionStartups
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var fundingDivisionStartups = db.FundingDivisionStartups.Include(f => f.FundingDivision).Include(f => f.Startup);
            return View(fundingDivisionStartups.ToList());
        }

        // GET: FundingDivisionStartups/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FundingDivisionStartup fundingDivisionStartup = db.FundingDivisionStartups.Find(id);
            if (fundingDivisionStartup == null)
            {
                return HttpNotFound();
            }
            return View(fundingDivisionStartup);
        }

        // GET: FundingDivisionStartups/Create
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult Create()
        {
            ViewBag.FundingDivisionID = new SelectList(db.FundingDivisions, "FundingDivisionID", "FundingDivisionName");
            ViewBag.StartupID = new SelectList(db.Startups, "StartupID", "UserID");
            return View();
        }

        // POST: FundingDivisionStartups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "IdeaCarrier")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FundingDivisionID,StartupID,Percentage")] FundingDivisionStartup fundingDivisionStartup)
        {
            if (ModelState.IsValid)
            {
                db.FundingDivisionStartups.Add(fundingDivisionStartup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //ViewBag.FundingDivisionID = new SelectList(db.FundingDivisions, "FundingDivisionID", "FundingDivisionName", fundingDivisionStartup.FundingDivisionID);
            //ViewBag.StartupID = new SelectList(db.Startups, "StartupID", "UserID", fundingDivisionStartup.StartupID);
            return View(fundingDivisionStartup);
        }

        // GET: FundingDivisionStartups/Edit/5
        [Authorize(Roles = "IdeaCarrier")]
        public ActionResult Edit(int? id, string fundingDivisionName)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.MaxValue = 0;
            ViewBag.FundingDivisionName = fundingDivisionName;

            FundingDivisionStartup fundingDivisionStartup = db.FundingDivisionStartups.Find(id);
            if (fundingDivisionStartup == null)
            {
                return HttpNotFound();
            }
            //ViewBag.FundingDivisionID = new SelectList(db.FundingDivisions, "FundingDivisionID", "FundingDivisionName", fundingDivisionStartup.FundingDivisionID);
            //ViewBag.StartupID = new SelectList(db.Startups, "StartupID", "UserID", fundingDivisionStartup.StartupID);

            var fundingDivisionStartupList = db.FundingDivisionStartups.Where(f => f.StartupID == fundingDivisionStartup.StartupID).ToList();
            int totalPercentage = 0;
            foreach (var fundingDivisionStartupItem in fundingDivisionStartupList)
            {
                totalPercentage += fundingDivisionStartupItem.Percentage;
            }
            //if (totalPercentage < 100)
            ViewBag.MaxValue = 100 - totalPercentage + fundingDivisionStartup.Percentage;

            return View(fundingDivisionStartup);
        }

        // POST: FundingDivisionStartups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "IdeaCarrier")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FundingDivisionID,StartupID,Percentage")] FundingDivisionStartup fundingDivisionStartup, string fundingDivisionName, string maxValue)
        {
            if (ModelState.IsValid)
            {
                if (fundingDivisionStartup.Percentage > int.Parse(maxValue))
                {
                    ViewBag.FundingDivisionName = fundingDivisionName;
                    ViewBag.MaxValue = maxValue;
                    ViewBag.Message = "Value to high!";
                    return View(fundingDivisionStartup);
                }

                db.Entry(fundingDivisionStartup).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect("~/Startups/ProjectForm/" + fundingDivisionStartup.StartupID + "#Budget"); //RedirectToAction("Index");
            }
            //ViewBag.FundingDivisionID = new SelectList(db.FundingDivisions, "FundingDivisionID", "FundingDivisionName", fundingDivisionStartup.FundingDivisionID);
            //ViewBag.StartupID = new SelectList(db.Startups, "StartupID", "UserID", fundingDivisionStartup.StartupID);
            return View(fundingDivisionStartup);
        }

        // GET: FundingDivisionStartups/Delete/5
        [Authorize(Roles = "Admin,IdeaCarrier")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FundingDivisionStartup fundingDivisionStartup = db.FundingDivisionStartups.Find(id);
            if (fundingDivisionStartup == null)
            {
                return HttpNotFound();
            }
            return View(fundingDivisionStartup);
        }

        // POST: FundingDivisionStartups/Delete/5
        [Authorize(Roles = "Admin,IdeaCarrier")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FundingDivisionStartup fundingDivisionStartup = db.FundingDivisionStartups.Find(id);
            db.FundingDivisionStartups.Remove(fundingDivisionStartup);
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
