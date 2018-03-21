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
    public class ScalabilitiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Scalabilities
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.Scalabilities.ToList());
        }

        // GET: Scalabilities/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Scalability scalability = db.Scalabilities.Find(id);
            if (scalability == null)
            {
                return HttpNotFound();
            }
            return View(scalability);
        }

        // GET: Scalabilities/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Scalabilities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "ScalabilityID,ScalabilityName")] Scalability scalability)
        {
            if (ModelState.IsValid)
            {
                db.Scalabilities.Add(scalability);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(scalability);
        }

        // GET: Scalabilities/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Scalability scalability = db.Scalabilities.Find(id);
            if (scalability == null)
            {
                return HttpNotFound();
            }
            return View(scalability);
        }

        // POST: Scalabilities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "ScalabilityID,ScalabilityName")] Scalability scalability)
        {
            if (ModelState.IsValid)
            {
                db.Entry(scalability).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(scalability);
        }

        // GET: Scalabilities/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Scalability scalability = db.Scalabilities.Find(id);
            if (scalability == null)
            {
                return HttpNotFound();
            }
            return View(scalability);
        }

        // POST: Scalabilities/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Scalability scalability = db.Scalabilities.Find(id);
            db.Scalabilities.Remove(scalability);
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
