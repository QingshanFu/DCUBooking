namespace DCUBooking.Controllers
{
    using DCUBooking.Models;
    using Helper;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The controller for Dic_Industry.
    /// </summary>
    //[AuthenAdmin]
    public class Dic_IndustryController : Controller
    {
        private DCUModelContainer db = new DCUModelContainer();

        /// <summary>
        /// GET: Dic_Industry, return the home page to configure the Dic_Industry information
        /// </summary>
        public ActionResult Index()
        {
            var industryList = db.Dic_Industry.SqlQuery("select * from Dic_Industry where Deleted=0").ToList();
            return View(industryList);
        }

        /// <summary>
        /// GET: Dic_Industry, return new item page to create Industry item
        /// </summary>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: Dic_Industry, save new item to database
        /// </summary>
        /// <param name="industry">New item for Industry</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name")] Dic_Industry industry)
        {
            if (ModelState.IsValid)
            {
                if (!CommonController.RecoverNewItem(db, "Dic_Industry", industry.Name))
                {
                    db.Dic_Industry.Add(industry);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            return View(industry);
        }

        /// <summary>
        /// GET: Dic_Industry, return edit page for Industry item
        /// </summary>
        /// <param name="id">Specify the item to be updated</param>
        public ActionResult Edit(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dic_Industry industry = db.Dic_Industry.Find(id);
            if (industry == null)
            {
                return HttpNotFound();
            }
            return View(industry);
        }

        /// <summary>
        /// POST: Dic_Industry, update the changes to database
        /// </summary>
        /// <param name="industry">Industry item to be updated</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name")] Dic_Industry industry)
        {
            if (ModelState.IsValid)
            {
                db.Entry(industry).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(industry);
        }

        /// <summary>
        /// GET: Dic_Industry, return the page to delete the item
        /// </summary>
        /// <param name="id">Specify the item to be deleted</param>
        public ActionResult Delete(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dic_Industry industry = db.Dic_Industry.Find(id);
            if (industry == null)
            {
                return HttpNotFound();
            }
            return View(industry);
        }

        /// <summary>
        /// POST: Dic_Industry, delete the Industry item
        /// </summary>
        /// <param name="id">Specify the item to be deleted</param>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(byte id)
        {
            db.Database.ExecuteSqlCommand("Update Dic_Industry set Deleted=1 where ID=" + id);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Override method to release the database resource
        /// </summary>
        /// <param name="disposing">To release database resource or not</param>
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
