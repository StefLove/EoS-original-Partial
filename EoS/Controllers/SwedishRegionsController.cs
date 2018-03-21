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
    [Authorize(Roles = "Admin")]
    public class SwedishRegionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SwedishRegions
        public ActionResult Index()
        {
            return View(db.SwedishRegions.ToList());
        }

        // GET: SwedishRegions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SwedishRegion swedishRegions = db.SwedishRegions.Find(id);
            if (swedishRegions == null)
            {
                return HttpNotFound();
            }
            return View(swedishRegions);
        }

        // GET: SwedishRegions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SwedishRegions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RegionID,RegionName")] SwedishRegion swedishRegions)
        {
            if (ModelState.IsValid)
            {
                db.SwedishRegions.Add(swedishRegions);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(swedishRegions);
        }

        // GET: SwedishRegions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SwedishRegion swedishRegions = db.SwedishRegions.Find(id);
            if (swedishRegions == null)
            {
                return HttpNotFound();
            }
            return View(swedishRegions);
        }

        // POST: SwedishRegions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RegionID,RegionName")] SwedishRegion swedishRegions)
        {
            if (ModelState.IsValid)
            {
                db.Entry(swedishRegions).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(swedishRegions);
        }

        // GET: SwedishRegions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SwedishRegion swedishRegions = db.SwedishRegions.Find(id);
            if (swedishRegions == null)
            {
                return HttpNotFound();
            }
            return View(swedishRegions);
        }

        // POST: SwedishRegions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SwedishRegion swedishRegions = db.SwedishRegions.Find(id);
            db.SwedishRegions.Remove(swedishRegions);
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
