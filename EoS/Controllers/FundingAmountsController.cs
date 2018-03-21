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
    public class FundingAmountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FundingAmounts
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.FundingAmounts.ToList());
        }

        // GET: FundingAmounts/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FundingAmount fundingAmount = db.FundingAmounts.Find(id);
            if (fundingAmount == null)
            {
                return HttpNotFound();
            }
            return View(fundingAmount);
        }

        // GET: FundingAmounts/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: FundingAmounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "FundingAmountID,FundingAmountValue")] FundingAmount fundingAmount)
        {
            if (ModelState.IsValid)
            {
                db.FundingAmounts.Add(fundingAmount);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fundingAmount);
        }

        // GET: FundingAmounts/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FundingAmount fundingAmount = db.FundingAmounts.Find(id);
            if (fundingAmount == null)
            {
                return HttpNotFound();
            }
            return View(fundingAmount);
        }

        // POST: FundingAmounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "FundingAmountID,FundingAmountValue")] FundingAmount fundingAmount)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fundingAmount).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fundingAmount);
        }

        // GET: FundingAmounts/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FundingAmount fundingAmount = db.FundingAmounts.Find(id);
            if (fundingAmount == null)
            {
                return HttpNotFound();
            }
            return View(fundingAmount);
        }

        // POST: FundingAmounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            FundingAmount fundingAmount = db.FundingAmounts.Find(id);
            db.FundingAmounts.Remove(fundingAmount);
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
