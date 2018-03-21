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
    public class InnovationLevelsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: InnovationLevels
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.InnovationLevels.ToList());
        }

        // GET: InnovationLevels/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InnovationLevel innovationLevel = db.InnovationLevels.Find(id);
            if (innovationLevel == null)
            {
                return HttpNotFound();
            }
            return View(innovationLevel);
        }

        // GET: InnovationLevels/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: InnovationLevels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "InnovationLevelID,InnovationLevelName")] InnovationLevel innovationLevel)
        {
            if (ModelState.IsValid)
            {
                db.InnovationLevels.Add(innovationLevel);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(innovationLevel);
        }

        // GET: InnovationLevels/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InnovationLevel innovationLevel = db.InnovationLevels.Find(id);
            if (innovationLevel == null)
            {
                return HttpNotFound();
            }
            return View(innovationLevel);
        }

        // POST: InnovationLevels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "InnovationLevelID,InnovationLevelName")] InnovationLevel innovationLevel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(innovationLevel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(innovationLevel);
        }

        // GET: InnovationLevels/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InnovationLevel innovationLevel = db.InnovationLevels.Find(id);
            if (innovationLevel == null)
            {
                return HttpNotFound();
            }
            return View(innovationLevel);
        }

        // POST: InnovationLevels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            InnovationLevel innovationLevel = db.InnovationLevels.Find(id);
            db.InnovationLevels.Remove(innovationLevel);
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
