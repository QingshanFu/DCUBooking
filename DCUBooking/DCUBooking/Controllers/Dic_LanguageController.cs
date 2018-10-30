namespace DCUBooking.Controllers
{    
    using DCUBooking.Models;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    public class Dic_LanguageController : Controller
    {
        private DCUModelContainer db = new DCUModelContainer();

        // GET: Dic_Language
        public ActionResult Index()
        {
            var languageList = db.Dic_Language.SqlQuery("select * from Dic_Language where Deleted=0").ToList();
            return View(languageList);
        }        

        // GET: Dic_Language/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Dic_Language/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name")] Dic_Language dic_Language)
        {
            if (ModelState.IsValid)
            {
                if (!CommonController.RecoverNewItem(db, "Dic_Language", dic_Language.Name))
                {
                    db.Dic_Language.Add(dic_Language);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            return View(dic_Language);
        }

        // GET: Dic_Language/Edit/5
        public ActionResult Edit(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dic_Language dic_Language = db.Dic_Language.Find(id);
            if (dic_Language == null)
            {
                return HttpNotFound();
            }
            return View(dic_Language);
        }

        // POST: Dic_Language/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name")] Dic_Language dic_Language)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dic_Language).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(dic_Language);
        }

        // GET: Dic_Language/Delete/5
        public ActionResult Delete(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dic_Language dic_Language = db.Dic_Language.Find(id);
            if (dic_Language == null)
            {
                return HttpNotFound();
            }
            return View(dic_Language);
        }

        // POST: Dic_Language/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(byte id)
        {
            db.Database.ExecuteSqlCommand("Update Dic_Language set Deleted=1 where ID=" + id);
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
