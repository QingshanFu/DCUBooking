namespace MCCOMailHandler
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Threading;

    class MailHandler
    {
        // 存放待处理的日志信息
        public static Queue<string> pendingLogs;
        // 保存数据库连接句柄
        public static SqlConnection mySqlConnection;

        public MailHandler()
        {            
            pendingLogs = new Queue<string>();
        }

        /// <summary>
        /// 初始化程序
        /// </summary>
        public bool InitializeApp()
        {
            Console.WriteLine("MCCO mail handler is running...");

            // 读取配置
            AppConfig.LogFile = ConfigurationManager.AppSettings["LogFileName"];
            AppConfig.DBConnectionStr = ConfigurationManager.AppSettings["DataBaseConnectionString"];
            AppConfig.DBConnectTimes = ConfigurationManager.AppSettings["DataBaseReConnectTimes"];
            AppConfig.CCAlias = ConfigurationManager.AppSettings["CCAlias"];
            AppConfig.MailTemplate = ConfigurationManager.AppSettings["MailTemplate"];
            AppConfig.WebHomeUrl = ConfigurationManager.AppSettings["WebHomeUrl"];
            Console.WriteLine(System.AppDomain.CurrentDomain.BaseDirectory);

            // 在应用程序同级目录下创建日志文件
            if (string.IsNullOrEmpty(AppConfig.LogFile) || AppConfig.LogFile.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) // 检查文件名是否合法
            {
                AppConfig.LogFile = "MCCOMailHandler.log";
            }

            // 写日志
            pendingLogs.Enqueue("处理程序开始运行。");
            pendingLogs.Enqueue("数据库连接字符串: " + AppConfig.DBConnectionStr);
            pendingLogs.Enqueue("数据库重连次数: " + AppConfig.DBConnectTimes);
            pendingLogs.Enqueue("邮件抄送列表: " + AppConfig.CCAlias);
            pendingLogs.Enqueue("邮件模板路径: " + AppConfig.MailTemplate);
            pendingLogs.Enqueue("MCCO 主页地址: " + AppConfig.WebHomeUrl);

            // 创建日志处理线程
            Thread logThread = new Thread(WriteLogs);
            logThread.Start();

            // 测试数据库是否可以连接
            if (!ConnectDatabase())
            {
                Thread.Sleep(2000); // 等待日志写入完成
                logThread.Abort();
                return false;
            }

            // 从数据库中读取邮箱和邮件账号信息
            string sql = "select * from MailAccount";
            var mailAccount = MCCODatabase.QueryData(mySqlConnection, sql);
            if (mailAccount == null || mailAccount.Rows.Count == 0)
            {
                pendingLogs.Enqueue("没有查询到邮件设置信息，SQL:" + sql);

                Thread.Sleep(2000); // 等待日志写入完成
                logThread.Abort();
                return false;
            }
            else
            {
                MailAccount.Account = mailAccount.Rows[0][1].ToString(); //"bjmcco@microsoft.com",
                MailAccount.MailServer = mailAccount.Rows[0][2].ToString(); //"apj.cloudmail.microsoft.com",
                MailAccount.Password = mailAccount.Rows[0][3].ToString(); //"SEP062017!",

                pendingLogs.Enqueue("邮件服务器：" + MailAccount.MailServer);
                pendingLogs.Enqueue("邮箱账号：" + MailAccount.Account);
                pendingLogs.Enqueue("邮箱密码：" + MailAccount.Password);
            }

            // 检查是否能连接到邮件服务器

            return true;
        }

        /// <summary>
        /// 处理待发送的业务邮件
        /// </summary>
        public void HandlePenddingMail()
        {
            while (true)
            {
                // 取得待处理的邮件数据，状态等于0，并且预约发送时间已到达
                string sql = "select top 1 * from MailTask where Status=0 and SendTime<GETDATE() order by SendTime asc";
                var record = MCCODatabase.QueryData(mySqlConnection, sql);

                if (record.Rows.Count > 0) // 处理取得的数据
                {
                    var status = 1;
                    var mailTaskData = new Dictionary<string, string>();
                    mailTaskData.Add("recordID", record.Rows[0][0].ToString());
                    mailTaskData.Add("requestID", record.Rows[0][1].ToString());
                    mailTaskData.Add("requestType", record.Rows[0][2].ToString());
                    mailTaskData.Add("emailType", record.Rows[0][3].ToString());
                    mailTaskData.Add("sendTime", record.Rows[0][5].ToString());

                    pendingLogs.Enqueue(string.Format("开始处理邮件任务。ID={0};业务类型={1};RequestID={2}", mailTaskData["recordID"], mailTaskData["requestType"], mailTaskData["requestID"]));
                    status = HandleMailTask(mailTaskData);

                    // 处理完成后更新状态
                    sql = "update MailTask set Status=" + status + " where ID=" + mailTaskData["recordID"];
                    var count = MCCODatabase.ExecuteSql(mySqlConnection, sql);
                    pendingLogs.Enqueue(string.Format("邮件任务完成。ID={0}", mailTaskData["recordID"]));
                }
                else // 任务表中已经没有待处理数据
                {
                    Thread.Sleep(10000);
                }
            }            
        }

        /// <summary>
        /// 打开数据库连接并保留连接
        /// </summary>
        public static bool ConnectDatabase()
        {
            // 数据库连接已经打开
            if (mySqlConnection != null && mySqlConnection.State == ConnectionState.Open)
            {
                return true;
            }

            pendingLogs.Enqueue("开始执行 ConnectDatabase...");
            bool result = false;            
            //创建连接对象
            mySqlConnection = new SqlConnection();
            int count = 0;
            while (count < int.Parse(AppConfig.DBConnectTimes))
            {
                try
                {
                    mySqlConnection.ConnectionString = AppConfig.DBConnectionStr;
                    mySqlConnection.Open();
                    if (mySqlConnection.State == ConnectionState.Open)
                    {
                        result = true;
                        break;
                    }
                    else
                    {
                        pendingLogs.Enqueue("数据库连接失败，请检查连接字符串。");
                    }
                }
                catch (Exception e)
                {
                    pendingLogs.Enqueue(e.Message);
                }

                Thread.Sleep(5000);
                count++;
            }

            return result;
        }

        /// <summary>
        /// 日志线程处理函数
        /// </summary>
        public static void WriteLogs()
        {
            string logPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, AppConfig.LogFile);
            using (StreamWriter logHandle = new StreamWriter(logPath, false, System.Text.Encoding.UTF8))
            {
                while (true)
                {
                    if (pendingLogs.Count > 0)
                    {
                        var logItem = pendingLogs.Dequeue();
                        logHandle.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + logItem);
                        logHandle.Flush();
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }                    
                }                
            }
        }  
        
        /// <summary>
        /// 处理邮件任务，根据不同的业务取得数据，并发送邮件
        /// </summary>
        /// <param name="task">邮件任务数据</param>
        /// <returns>返回处理状态，正常结束返回 1</returns>
        private static int HandleMailTask(Dictionary<string, string> task)
        {
            int status = 1;

            if (task["requestType"] == "4") // 处理EventRequest的邮件信息
            {
                // 取得邮件中需要使用的业务数据
                string sql = "select RequestorAlias, RequestorName, RequestorPhone, EventName, AttendeeNumber, EventNote," +
                    "CONVERT(varchar(12), EventStartTime, 111) AS EventDate," +
                    "CONVERT(varchar(5), EventStartTime, 8) +'-' + CONVERT(varchar(5), EventEndTime, 8) AS EventTime," +
                    "((select Name from RequestGroup where ID = e.RequestorGroup) +'/' + (select Name from RequestGroup where ID = e.SubRequestorGroup)) as RequestorGroup," +
                    "(select Name from Dic_EventType where ID = e.EventType) as EventType," +
                    "(select Name from Dic_EventTheme where ID = e.EventTheme) as EventTheme, ID, Status " +
                    "from EventRequestRecords e where ID=" + task["requestID"];
                var eventRequest = MCCODatabase.QueryData(mySqlConnection, sql);
                if (eventRequest.Rows.Count == 1)
                {
                    var requestData = new Dictionary<string, string>();
                    requestData.Add("RequestorAlias", eventRequest.Rows[0][0].ToString());
                    requestData.Add("RequestorName", eventRequest.Rows[0][1].ToString());
                    requestData.Add("RequestorPhone", eventRequest.Rows[0][2].ToString());
                    requestData.Add("EventName", eventRequest.Rows[0][3].ToString());
                    requestData.Add("AttendeeNumber", eventRequest.Rows[0][4].ToString());
                    requestData.Add("EventNote", eventRequest.Rows[0][5].ToString());
                    requestData.Add("EventDate", eventRequest.Rows[0][6].ToString());
                    requestData.Add("EventTime", eventRequest.Rows[0][7].ToString());
                    requestData.Add("RequestorGroup", eventRequest.Rows[0][8].ToString());
                    requestData.Add("EventType", eventRequest.Rows[0][9].ToString());
                    requestData.Add("EventTheme", eventRequest.Rows[0][10].ToString());
                    requestData.Add("ID", eventRequest.Rows[0][11].ToString());
                    requestData.Add("Status", eventRequest.Rows[0][12].ToString());

                    var receives = GetMailReceivers(task);
                    string title = "Event Reservation for " + requestData["EventName"];
                    string template = string.Empty;
                    string content = string.Empty;
                    bool isNormalMail = true;

                    switch(task["emailType"]) // 根据邮件类型处理数据
                    {
                        case "1": // 用户提交申请成功
                            title = "Event request has been well received!";
                            receives["ToList"] = requestData["RequestorAlias"];
                            receives["CcList"] = "";
                            template = "NewRequestForEvent.html";
                            break;
                        case "2": // 提醒 Approver 新申请
                            title = "New Event Request from " + requestData["RequestorName"];
                            receives["CcList"] = "";
                            template = "ReviewReminderForEvent.html";
                            break;
                        case "3": // Approve Request，发送 S+
                            receives["ToList"] = requestData["RequestorAlias"];
                            template = "ApproveForEvent.html";
                            isNormalMail = false;
                            break;
                        case "4": // Update Request，发送 S+
                            receives["ToList"] = requestData["RequestorAlias"];
                            template = "ApproveForEvent.html";
                            isNormalMail = false;
                            break;
                        case "5": // Reject Request
                            title = "Your Event request was rejected";
                            receives["ToList"] = requestData["RequestorAlias"];
                            receives["CcList"] = "";
                            template = "RejectForEvent.html";
                            break;
                        case "6": // 用户取消 request
                            title = "Event request was cancelled by user";
                            receives["CcList"] = "";
                            template = "CancelForEvent.html";
                            break;
                        case "7": // 管理员取消 request，发送 S+
                            title = "Canceled: " + title;
                            receives["CcList"] = "";
                            template = "ApproveForEvent.html";
                            isNormalMail = false;
                            break;
                        case "8": // Post info reminder
                            if(requestData["Status"] != "1")
                            {
                                status = 4;
                                pendingLogs.Enqueue("RequestID=" + task["requestID"] + "的 Event 申请已经不是 Approved 状态。");
                                return status;
                            }

                            title = "We appreciated your highlight/feedback of your " + requestData["EventName"] + " event in MCCO";
                            receives["ToList"] = requestData["RequestorAlias"];
                            receives["CcList"] = "";
                            template = "PostReminderForEvent.html";
                            break;
                    }

                    // 检查邮件模板是否存在
                    var fullPath = Path.Combine(AppConfig.MailTemplate, template);
                    if (File.Exists(fullPath))
                    {
                        content = File.ReadAllText(fullPath);
                        content = content.Replace("#User#", requestData["RequestorAlias"]);
                        content = content.Replace("#EventName#", requestData["EventName"]);
                        content = content.Replace("#EventDate#", requestData["EventDate"]);
                        content = content.Replace("#EventTime#", requestData["EventTime"]);
                        content = content.Replace("#RequestorName#", requestData["RequestorName"]);
                        content = content.Replace("#RequestorPhone#", requestData["RequestorPhone"]);
                        content = content.Replace("#RequestorGroup#", requestData["RequestorGroup"]);
                        content = content.Replace("#EventType#", requestData["EventType"]);
                        content = content.Replace("#EventTheme#", requestData["EventTheme"]);
                        content = content.Replace("#AttendeeNumber#", requestData["AttendeeNumber"]);
                        content = content.Replace("#EventNote#", requestData["EventNote"]);
                        content = content.Replace("#Admin#", receives["ToList"]);
                        content = content.Replace("#RequestUrl#", AppConfig.WebHomeUrl + "/EventAdmin/Details/" + requestData["ID"]);

                        if (isNormalMail)
                        {
                            // 发送邮件
                            MailHelper.SendMail(receives, title, content);
                        }
                        else
                        {
                            // 发送 appointment
                            DateTime startTime = DateTime.Parse(requestData["EventDate"] + " " + requestData["EventTime"].Split('-')[0]);
                            DateTime endTime = DateTime.Parse(requestData["EventDate"] + " " + requestData["EventTime"].Split('-')[1]);
                            string appointmentID = null;

                            if (task["emailType"] == "3") // 新建 appointment
                            {
                                appointmentID = MailHelper.SendAppointment(receives, title, content, "BJW Microsoft Center One", startTime, endTime, task["emailType"]);

                                // 保存 Appointment UID
                                sql = "INSERT INTO CalendarRecord VALUES ('" + task["requestID"] + "','4','" + appointmentID + "')";
                                MCCODatabase.ExecuteSql(mySqlConnection, sql);
                            }
                            else // 更新或者删除 appointment
                            {
                                // 从数据库中取得已存在的 appointment uid，执行更新和删除操作
                                sql = "SELECT UID FROM CalendarRecord WHERE Type=4 And RecordID=" + task["requestID"];
                                var uid = MCCODatabase.QueryData(mySqlConnection, sql);
                                if (uid.Rows.Count > 0)
                                {
                                    appointmentID = uid.Rows[uid.Rows.Count - 1][0].ToString();
                                    appointmentID = MailHelper.SendAppointment(receives, title, content, "BJW Microsoft Center One", startTime, endTime, task["emailType"], appointmentID);
                                }
                                else
                                {
                                    status = 3;
                                    pendingLogs.Enqueue("RequestID=" + task["requestID"] + "的 Event 申请不存在对应的 appointment，无法执行更新/删除操作。");
                                }
                            }
                        }
                    }
                    else
                    {
                        status = 2;
                        pendingLogs.Enqueue("没有找到指定的邮件模板。ID=[" + task["recordID"] + "]");
                    }                                        
                }
                else
                {
                    status = 9;
                    pendingLogs.Enqueue("系统中不存在 ID=[" + task["requestID"] + "]的 Event 申请记录。");
                }
            }

            return status;
        }

        /// <summary>
        /// 从数据库中取得预设的收件人列表
        /// </summary>
        /// <param name="task">邮件任务数据</param>
        /// <returns>返回收件人列表</returns>
        private static Dictionary<string,string> GetMailReceivers(Dictionary<string, string> task)
        {
            string sql = string.Empty;
            var ret = new Dictionary<string, string>();

            if (task["requestType"] == "1") // 处理 Guided Tour 的邮件信息
            {
                // 读取数据库中预设的接收者信息
                sql = "select top 1 approver,Coordinator from GuidedTourSettings";
                var receivers = MCCODatabase.QueryData(mySqlConnection, sql);
                if(receivers.Rows.Count == 1)
                {
                    string approver = receivers.Rows[0][0].ToString();
                    string coordinator = receivers.Rows[0][1].ToString();
                    approver = approver.TrimEnd(';');
                    coordinator = coordinator.TrimEnd(';');

                    ret["ToList"] = approver;
                    ret["CcList"] = AppConfig.CCAlias.TrimEnd(';');
                    ret["Coordinator"] = coordinator;
                }                
            }

            if (task["requestType"] == "2") // 处理 Flexible Zone 的邮件信息
            {
                // 读取数据库中预设的接收者信息
                sql = "select top 1 approver from FlexibleZoneSettings";
                var receivers = MCCODatabase.QueryData(mySqlConnection, sql);
                if (receivers.Rows.Count == 1)
                {
                    string approver = receivers.Rows[0][0].ToString();
                    approver = approver.TrimEnd(';');

                    ret["ToList"] = approver;
                    ret["CcList"] = AppConfig.CCAlias.TrimEnd(';');
                    ret["Coordinator"] = "";
                }
            }

            if (task["requestType"] == "3") // 处理 Hosted Visit 的邮件信息
            {
                // 读取数据库中预设的接收者信息
                sql = "select top 1 approver from HostedVisitSetting";
                var receivers = MCCODatabase.QueryData(mySqlConnection, sql);
                if (receivers.Rows.Count == 1)
                {
                    string approver = receivers.Rows[0][0].ToString();
                    approver = approver.TrimEnd(';');

                    ret["ToList"] = approver;
                    ret["CcList"] = AppConfig.CCAlias.TrimEnd(';');
                    ret["Coordinator"] = "";
                }
            }

            if (task["requestType"] == "4") // 处理 EventRequest 的邮件信息
            {
                // 读取数据库中预设的接收者信息
                sql = "select top 1 approver from EventSettings";
                var receivers = MCCODatabase.QueryData(mySqlConnection, sql);
                if (receivers.Rows.Count == 1)
                {
                    string approver = receivers.Rows[0][0].ToString();
                    approver = approver.TrimEnd(';');

                    ret["ToList"] = approver;
                    ret["CcList"] = AppConfig.CCAlias.TrimEnd(';');
                    ret["Coordinator"] = "";
                }
            }

            return ret;
        }
    }

    // 保存邮件相关信息
    public class MailAccount
    {
        public static string Account { get; set; }
        public static string MailServer { get; set; }
        public static string Password { get; set; }
    }
}
