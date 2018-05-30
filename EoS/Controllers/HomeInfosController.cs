using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EoS.Models;
using EoS.Models.Home;

namespace EoS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeInfosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: HomeInfos
        public ActionResult Index()
        {
            return View(db.HomeInfos.ToList());
        }

        // GET: HomeInfos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HomeInfo homeInfo = db.HomeInfos.Find(id);
            if (homeInfo == null)
            {
                return HttpNotFound();
            }
            return View(homeInfo);
        }

        // GET: HomeInfos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HomeInfos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateInput(false)] <-------------------------------------------------------------
        public ActionResult Create([Bind(Include = "Id,Text")] HomeInfo homeInfo)
        {
            if (ModelState.IsValid)
            {
                db.HomeInfos.Add(homeInfo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(homeInfo);
        }

        // GET: HomeInfos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HomeInfo homeInfo = db.HomeInfos.Find(id);
            if (homeInfo == null)
            {
                return HttpNotFound();
            }
            return View(homeInfo);
        }

        // POST: HomeInfos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "Id,Text")] HomeInfo homeInfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(homeInfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(homeInfo);
        }

        // GET: HomeInfos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HomeInfo homeInfo = db.HomeInfos.Find(id);
            if (homeInfo == null)
            {
                return HttpNotFound();
            }
            return View(homeInfo);
        }

        // POST: HomeInfos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            HomeInfo homeInfo = db.HomeInfos.Find(id);
            db.HomeInfos.Remove(homeInfo);
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
