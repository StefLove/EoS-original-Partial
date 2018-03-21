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
using EoS.Models.IdeaCarrier;

namespace EoS.Controllers
{
    public class FundingDivisionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FundingDivisions
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.FundingDivisions.ToList());
        }

        // GET: FundingDivisions/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FundingDivision fundingDivision = db.FundingDivisions.Find(id);
            if (fundingDivision == null)
            {
                return HttpNotFound();
            }
            return View(fundingDivision);
        }

        // GET: FundingDivisions/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: FundingDivisions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "FundingDivisionID,FundingDivisionName")] FundingDivision fundingDivision)
        {
            if (ModelState.IsValid)
            {
                db.FundingDivisions.Add(fundingDivision);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fundingDivision);
        }

        // GET: FundingDivisions/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FundingDivision fundingDivision = db.FundingDivisions.Find(id);
            if (fundingDivision == null)
            {
                return HttpNotFound();
            }
            return View(fundingDivision);
        }

        // POST: FundingDivisions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "FundingDivisionID,FundingDivisionName")] FundingDivision fundingDivision)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fundingDivision).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fundingDivision);
        }

        // GET: FundingDivisions/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FundingDivision fundingDivision = db.FundingDivisions.Find(id);
            if (fundingDivision == null)
            {
                return HttpNotFound();
            }
            return View(fundingDivision);
        }

        // POST: FundingDivisions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            FundingDivision fundingDivision = db.FundingDivisions.Find(id);
            db.FundingDivisions.Remove(fundingDivision);
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
