namespace DCUBooking.Controllers
{
    using DCUBooking.Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    public class Set_HolidayController : Controller
    {
        private DCUModelContainer db = new DCUModelContainer();        

        // GET: Set_Holiday
        public ActionResult Index()
        {
            // 只显示当前日期以后的设置
            string today = DateTime.Now.ToString("yyyy/MM/dd");
            string sql = "select * from Set_Holiday where date>='" + today + "' order by Date";

            return View(db.Set_Holiday.SqlQuery(sql).ToList());
        }

        // GET: Set_Holiday/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Set_Holiday set_Holiday = db.Set_Holiday.Find(id);
            if (set_Holiday == null)
            {
                return HttpNotFound();
            }
            return View(set_Holiday);
        }

        // GET: Set_Holiday/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Set_Holiday/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Date,HolidayType,IsWorkDay,Remarks")] Set_Holiday set_Holiday)
        {
            if (ModelState.IsValid)
            {
                db.Set_Holiday.Add(set_Holiday);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(set_Holiday);
        }              

        // GET: Set_Holiday/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Set_Holiday set_Holiday = db.Set_Holiday.Find(id);
            if (set_Holiday == null)
            {
                return HttpNotFound();
            }

            // 删除工作日并且当前日期内有request，需要提示信息
            if (set_Holiday.IsWorkDay && CheckDate(set_Holiday.Date.ToString("MM/dd/yyyy")))
            {
                ViewBag.MessageInfo = "The date you selected has booking information.";
            }

            return View(set_Holiday);
        }

        // POST: Set_Holiday/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Set_Holiday set_Holiday = db.Set_Holiday.Find(id);
            db.Set_Holiday.Remove(set_Holiday);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Checks if the current date exist
        /// </summary>
        /// <param name="Date">The date to be checked</param>
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)] // Clear the cache
        public JsonResult CheckDateExist(DateTime Date)
        {
            bool isExist = true;
            string sql = "select * from Set_Holiday where date='" + Date.ToString("yyyy/MM/dd") + "'";
            var result = db.Set_Holiday.SqlQuery(sql).ToList();
            isExist = result.Count > 0;

            return Json(!isExist, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Checks if the current date has booking request
        /// </summary>
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)] // Clear the cache
        public JsonResult CheckDateOccupied()
        {
            string date = Request.Form["SelectedDate"];
            date = DateTime.Parse(date).ToString("MM/dd/yyyy");
            if (CheckDate(date))
            {
                return Json("True");
            }

            return Json("False");
        }

        /// <summary>
        /// 判断当前日期是否有request
        /// </summary>
        /// <param name="date">The date to be checked, the format must be MM/dd/yyyy</param>
        private bool CheckDate(string date)
        {
            // 判断Hosted Visit是否存在记录
            //sql = "SELECT ID from HostedVisitRequestRecord where (Status=0 or Status=1) and VisitDate='" + date + "'";
            //records = db.Database.SqlQuery<long>(sql).ToList();
            //if (records != null && records.Count > 0)
            //{
            //    return true;
            //}

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 查询指定日期是否是工作日
        /// </summary>
        /// <param name="db">Database handle</param>
        /// <param name="date">The date to be determined</param>
        public static bool IsWorkDay(DCUModelContainer db, DateTime date)
        {
            int dayOfWeek = (int)date.DayOfWeek;
            if (dayOfWeek == 0 || dayOfWeek == 6) // 周六和周日
            {
                // 需要查询当日是否是调整工作日
                string sql = "select * from Set_Holiday where isworkday=1 and Date='" + date.ToString("yyyy/MM/dd") + "'";
                if (db.Database.SqlQuery<Set_Holiday>(sql).ToList().Count() > 0)
                {
                    return true;
                }
            }
            else // 周一到周五
            {
                // 需要查询当日是否是法定假日
                string sql = "select * from Set_Holiday where isworkday=0 and Date='" + date.ToString("yyyy/MM/dd") + "'";
                if (db.Database.SqlQuery<Set_Holiday>(sql).ToList().Count() == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
