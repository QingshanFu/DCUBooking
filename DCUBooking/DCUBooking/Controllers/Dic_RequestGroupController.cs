namespace DCUBooking.Controllers
{
    using DCUBooking.Models;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The controller for the Dic_RequestGroup dictionary.
    /// </summary>
    //[AuthenAdmin]
    public class Dic_RequestGroupController : Controller
    {
        private DCUModelContainer db = new DCUModelContainer();

        /// <summary>
        /// GET: Dic_RequestGroup, return the home page to configure the Dic_RequestGroup information
        /// </summary>
        public ActionResult Index()
        {
            var groupList = db.Dic_RequestGroup.SqlQuery("select * from Dic_RequestGroup where Deleted=0").ToList();

            foreach (var item in groupList)
            {
                Dic_RequestGroup rg = db.Dic_RequestGroup.Find(item.ParentID);
                if (rg != null)
                {
                    item.SetParentName(rg);
                }
            }

            return View(groupList);
        }

        /// <summary>
        /// GET: Dic_RequestGroup, return detail page for specified Dic_RequestGroup item
        /// </summary>
        public ActionResult Details(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dic_RequestGroup requestGroup = db.Dic_RequestGroup.Find(id);
            if (requestGroup == null)
            {
                return HttpNotFound();
            }

            Dic_RequestGroup rg = db.Dic_RequestGroup.Find(requestGroup.ParentID);
            if (rg != null)
            {
                requestGroup.SetParentName(rg);
            }

            return View(requestGroup);
        }

        /// <summary>
        /// GET: Dic_RequestGroup, return new item page to create Dic_RequestGroup item
        /// </summary>
        public ActionResult Create()
        {
            List<SelectListItem> groupItems = new List<SelectListItem>();
            var groupList = db.Dic_RequestGroup.SqlQuery("select * from Dic_RequestGroup where Deleted=0").ToList();

            foreach (var item in groupList)
            {
                if (item.ParentID == null)
                {
                    var selectItem = new SelectListItem();
                    selectItem.Text = item.Name;
                    selectItem.Value = item.ID.ToString();
                    groupItems.Add(selectItem);
                }
            }
            ViewData["GroupItems"] = groupItems;

            return View();
        }

        /// <summary>
        /// POST: Dic_RequestGroup, save new item to database
        /// </summary>
        /// <param name="requestGroup">New item for Dic_RequestGroup</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,ParentID")] Dic_RequestGroup requestGroup)
        {
            if (ModelState.IsValid)
            {
                db.Dic_RequestGroup.Add(requestGroup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(requestGroup);
        }

        /// <summary>
        /// GET: Dic_RequestGroup, return edit page for Dic_RequestGroup item
        /// </summary>
        /// <param name="id">Specify the item to be updated</param>
        public ActionResult Edit(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dic_RequestGroup requestGroup = db.Dic_RequestGroup.Find(id);
            if (requestGroup == null)
            {
                return HttpNotFound();
            }

            List<SelectListItem> groupItems = new List<SelectListItem>();
            var groupList = db.Dic_RequestGroup.SqlQuery("select * from Dic_RequestGroup where Deleted=0").ToList();

            foreach (var item in groupList)
            {
                if (item.ParentID == null)
                {
                    if (item.Name == requestGroup.Name)
                    {
                        continue;
                    }

                    var selectItem = new SelectListItem();
                    selectItem.Text = item.Name;
                    selectItem.Value = item.ID.ToString();
                    if(item.ID == requestGroup.ParentID)
                    {
                        selectItem.Selected = true;
                    }
                    groupItems.Add(selectItem);
                }
            }
            ViewData["GroupItems"] = groupItems;

            return View(requestGroup);
        }

        /// <summary>
        /// POST: Dic_RequestGroup, update the changes to database
        /// </summary>
        /// <param name="requestGroup">Dic_RequestGroup item to be updated</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,ParentID")] Dic_RequestGroup requestGroup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(requestGroup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(requestGroup);
        }

        /// <summary>
        /// GET: Dic_RequestGroup, return the page to delete the item
        /// </summary>
        /// <param name="id">Specify the item to be deleted</param>
        public ActionResult Delete(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dic_RequestGroup requestGroup = db.Dic_RequestGroup.Find(id);
            if (requestGroup == null)
            {
                return HttpNotFound();
            }
            return View(requestGroup);
        }

        /// <summary>
        /// POST: Dic_RequestGroup, delete the Dic_RequestGroup item
        /// </summary>
        /// <param name="id">Specify the item to be deleted</param>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(byte id)
        {
            db.Database.ExecuteSqlCommand("Update Dic_RequestGroup set Deleted=1 where ID=" + id);
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
