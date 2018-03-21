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

namespace EoS.Controllers
{
    public class FundingPhasesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FundingPhases
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.FundingPhases.ToList());
        }

        // GET: FundingPhases/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FundingPhase fundingPhase = db.FundingPhases.Find(id);
            if (fundingPhase == null)
            {
                return HttpNotFound();
            }
            return View(fundingPhase);
        }

        // GET: FundingPhases/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: FundingPhases/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "FundingPhaseID,FundingPhaseName")] FundingPhase fundingPhase)
        {
            if (ModelState.IsValid)
            {
                db.FundingPhases.Add(fundingPhase);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fundingPhase);
        }

        // GET: FundingPhases/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FundingPhase fundingPhase = db.FundingPhases.Find(id);
            if (fundingPhase == null)
            {
                return HttpNotFound();
            }
            return View(fundingPhase);
        }

        // POST: FundingPhases/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "FundingPhaseID,FundingPhaseName")] FundingPhase fundingPhase)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fundingPhase).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fundingPhase);
        }

        // GET: FundingPhases/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FundingPhase fundingPhase = db.FundingPhases.Find(id);
            if (fundingPhase == null)
            {
                return HttpNotFound();
            }
            return View(fundingPhase);
        }

        // POST: FundingPhases/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FundingPhase fundingPhase = db.FundingPhases.Find(id);
            db.FundingPhases.Remove(fundingPhase);
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
