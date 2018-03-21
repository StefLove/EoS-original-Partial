using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EoS.Models;
using EoS.Models.Admin;

namespace EoS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InvestorMessagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: InvestorMessages
        public ActionResult Index()
        {
            return View(db.InvestorMessages.ToList());
        }

        // GET: InvestorMessages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InvestorMessage investorMessage = db.InvestorMessages.Find(id);
            if (investorMessage == null)
            {
                return HttpNotFound();
            }
            return View(investorMessage);
        }

        // GET: InvestorMessages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: InvestorMessages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "Id,Text")] InvestorMessage investorMessage)
        {
            if (ModelState.IsValid)
            {
                db.InvestorMessages.Add(investorMessage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(investorMessage);
        }

        // GET: InvestorMessages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InvestorMessage investorMessage = db.InvestorMessages.Find(id);
            if (investorMessage == null)
            {
                return HttpNotFound();
            }
            return View(investorMessage);
        }

        // POST: InvestorMessages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "Id,Text")] InvestorMessage investorMessage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(investorMessage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(investorMessage);
        }

        // GET: InvestorMessages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InvestorMessage investorMessage = db.InvestorMessages.Find(id);
            if (investorMessage == null)
            {
                return HttpNotFound();
            }
            return View(investorMessage);
        }

        // POST: InvestorMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InvestorMessage investorMessage = db.InvestorMessages.Find(id);
            db.InvestorMessages.Remove(investorMessage);
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