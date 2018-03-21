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
    public class OutcomesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: OutComes
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.Outcomes.ToList());
        }

        // GET: OutComes/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Outcome outcome = db.Outcomes.Find(id);
            if (outcome == null)
            {
                return HttpNotFound();
            }
            return View(outcome);
        }

        // GET: OutComes/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: OutComes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "OutComeID,OutComeName")] Outcome outcome)
        {
            if (ModelState.IsValid)
            {
                db.Outcomes.Add(outcome);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(outcome);
        }

        // GET: OutComes/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Outcome outcome = db.Outcomes.Find(id);
            if (outcome == null)
            {
                return HttpNotFound();
            }
            return View(outcome);
        }

        // POST: OutComes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "OutComeID,OutComeName")] Outcome outcome)
        {
            if (ModelState.IsValid)
            {
                db.Entry(outcome).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(outcome);
        }

        // GET: OutComes/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Outcome outcome = db.Outcomes.Find(id);
            if (outcome == null)
            {
                return HttpNotFound();
            }
            return View(outcome);
        }

        // POST: OutComes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            Outcome outcome = db.Outcomes.Find(id);
            db.Outcomes.Remove(outcome);
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
