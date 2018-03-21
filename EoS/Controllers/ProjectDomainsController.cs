﻿using System;
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
    public class ProjectDomainsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ProjectDomains
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.ProjectDomains.ToList());
        }

        // GET: ProjectDomains/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectDomain projectDomain = db.ProjectDomains.Find(id);
            if (projectDomain == null)
            {
                return HttpNotFound();
            }
            return View(projectDomain);
        }

        // GET: ProjectDomains/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProjectDomains/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "ProjectDomainID,ProjectDomainName")] ProjectDomain projectDomain)
        {
            if (ModelState.IsValid)
            {
                db.ProjectDomains.Add(projectDomain);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(projectDomain);
        }

        // GET: ProjectDomains/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectDomain projectDomain = db.ProjectDomains.Find(id);
            if (projectDomain == null)
            {
                return HttpNotFound();
            }
            return View(projectDomain);
        }

        // POST: ProjectDomains/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "ProjectDomainID,ProjectDomainName")] ProjectDomain projectDomain)
        {
            if (ModelState.IsValid)
            {
                db.Entry(projectDomain).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(projectDomain);
        }

        // GET: ProjectDomains/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectDomain projectDomain = db.ProjectDomains.Find(id);
            if (projectDomain == null)
            {
                return HttpNotFound();
            }
            return View(projectDomain);
        }

        // POST: ProjectDomains/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            ProjectDomain projectDomain = db.ProjectDomains.Find(id);
            db.ProjectDomains.Remove(projectDomain);
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
