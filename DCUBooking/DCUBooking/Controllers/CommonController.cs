namespace DCUBooking.Controllers
{
    using DCUBooking.Models;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    public class CommonController : Controller
    {   
        // GET: Common
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 判断字典里是否存在相同 name 的 item，如果存在，就恢复该项
        /// </summary>
        /// <param name="db">数据库连接句柄</param>
        /// <param name="table">字典表名</param>
        /// <param name="name">新建项目名</param>
        public static bool RecoverNewItem(DCUModelContainer db, string table, string name)
        {
            string sql = "update " + table + " set deleted=0 where Name='" + name + "' and deleted=1";
            int ret = db.Database.ExecuteSqlCommand(sql);
            if (ret > 0)
            {
                return true;
            }

            return false;
        }
    }
}