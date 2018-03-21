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
    public class IdeaCarrierMessagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: IdeaCarrierMessages
        public ActionResult Index()
        {
            return View(db.IdeaCarrierMessages.ToList());
        }

        // GET: IdeaCarrierMessages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IdeaCarrierMessage ideaCarrierMessage = db.IdeaCarrierMessages.Find(id);
            if (ideaCarrierMessage == null)
            {
                return HttpNotFound();
            }
            return View(ideaCarrierMessage);
        }

        // GET: IdeaCarrierMessages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: IdeaCarrierMessages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "Id,Text")] IdeaCarrierMessage ideaCarrierMessage)
        {
            if (ModelState.IsValid)
            {
                db.IdeaCarrierMessages.Add(ideaCarrierMessage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(ideaCarrierMessage);
        }

        // GET: IdeaCarrierMessages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IdeaCarrierMessage ideaCarrierMessage = db.IdeaCarrierMessages.Find(id);
            if (ideaCarrierMessage == null)
            {
                return HttpNotFound();
            }
            return View(ideaCarrierMessage);
        }

        // POST: ideaCarrierMessages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "Id,Text")] IdeaCarrierMessage ideaCarrierMessage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ideaCarrierMessage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ideaCarrierMessage);
        }

        // GET: StartupMessages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IdeaCarrierMessage ideaCarrierMessage = db.IdeaCarrierMessages.Find(id);
            if (ideaCarrierMessage == null)
            {
                return HttpNotFound();
            }
            return View(ideaCarrierMessage);
        }

        // POST: StartupMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            IdeaCarrierMessage ideaCarrierMessage = db.IdeaCarrierMessages.Find(id);
            db.IdeaCarrierMessages.Remove(ideaCarrierMessage);
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
