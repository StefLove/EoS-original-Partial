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
    public class EstimatedExitPlansController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EstimatedExitPlans
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.EstimatedExitPlans.ToList());
        }

        // GET: EstimatedExitPlans/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstimatedExitPlan estimatedExitPlan = db.EstimatedExitPlans.Find(id);
            if (estimatedExitPlan == null)
            {
                return HttpNotFound();
            }
            return View(estimatedExitPlan);
        }

        // GET: EstimatedExitPlans/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: EstimatedExitPlans/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EstimatedExitPlanID,EstimatedExitPlanName")] EstimatedExitPlan estimatedExitPlan)
        {
            if (ModelState.IsValid)
            {
                db.EstimatedExitPlans.Add(estimatedExitPlan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(estimatedExitPlan);
        }

        // GET: EstimatedExitPlans/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstimatedExitPlan estimatedExitPlan = db.EstimatedExitPlans.Find(id);
            if (estimatedExitPlan == null)
            {
                return HttpNotFound();
            }
            return View(estimatedExitPlan);
        }

        // POST: EstimatedExitPlans/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "EstimatedExitPlanID,EstimatedExitPlanName")] EstimatedExitPlan estimatedExitPlan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(estimatedExitPlan).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(estimatedExitPlan);
        }

        // GET: EstimatedExitPlans/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstimatedExitPlan estimatedExitPlan = db.EstimatedExitPlans.Find(id);
            if (estimatedExitPlan == null)
            {
                return HttpNotFound();
            }
            return View(estimatedExitPlan);
        }

        // POST: EstimatedExitPlans/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EstimatedExitPlan estimatedExitPlan = db.EstimatedExitPlans.Find(id);
            db.EstimatedExitPlans.Remove(estimatedExitPlan);
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
