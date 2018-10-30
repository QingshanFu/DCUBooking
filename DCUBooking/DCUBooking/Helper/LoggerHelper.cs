namespace DCUBooking.Helper
{
    /// <summary>
    /// Helper class to handle operation logs
    /// </summary>
    public class LoggerHelper
    {
        /// <summary>
        /// Get log handle defined in log4net.config
        /// </summary>
        static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("loginfo");

        /// <summary>
        /// Write log information
        /// </summary>
        /// <param name="Msg">Log information</param>
        public static void Info(string Msg)
        {
            string userName = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                userName = System.Web.HttpContext.Current.User.Identity.Name;
            }

            loginfo.Info(userName + " - " + Msg);
        }
    }
}