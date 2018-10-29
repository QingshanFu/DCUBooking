namespace MCCOMailHandler
{
    using Microsoft.Exchange.WebServices.Data;
    using System;
    using System.Collections.Generic;

    class MailHelper
    {
        private static ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2013_SP1);

        /// <summary>
        /// EWS service 初始化
        /// AutodiscoverUrl 首次执行需要比较长时间，初始化成功后保留信息，以后执行不需要再次执行 AutodiscoverUrl
        /// </summary>
        private static void InitializeEWS()
        {
            service.UseDefaultCredentials = false;
            //MailAccount.Account = "liaxu@outlook.com";
            //MailAccount.Password = "Xuliang821117";
            service.Credentials = new WebCredentials(MailAccount.Account, MailAccount.Password);

            if (service.Url == null)
            {
                service.AutodiscoverUrl(MailAccount.Account, RedirectionUrlValidationCallback);
                MailHandler.pendingLogs.Enqueue("开始初始化 EWS Services.");                
            }
        }

        /// <summary>
        /// AutodiscoverUrl 方法的回调函数
        /// </summary>
        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;

            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 发送 appointment
        /// </summary>
        /// /// <param name="receives">邮件接收列表</param>
        /// <param name="title">邮件标题</param>
        /// <param name="content">邮件内容</param>
        /// <param name="location">会议室名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="mailType">3：新建；4：更新；其它：删除</param>
        /// <param name="appointmentID">如果存在，则更新原有的 appointment，否则新增 appointment</param>
        /// <returns>返回 appointment uid</returns>
        public static string SendAppointment(Dictionary<string, string> receives, string title, string content, string location, DateTime startTime, DateTime endTime, string mailType, string appointmentID = null)
        {            
            MailHandler.pendingLogs.Enqueue("开始发送 Appointment。");

            InitializeEWS();
            string ret = null;
            Appointment app = null;

            try
            {
                if (string.IsNullOrEmpty(appointmentID))
                {
                    app = new Appointment(service);
                }
                else
                {
                    app = Appointment.Bind(service, new ItemId(appointmentID));
                }
            }
            catch (Exception e)
            {
                MailHandler.pendingLogs.Enqueue(e.Message);
                return ret;
            }

            foreach (var item in receives["ToList"].Split(';'))
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if (item.IndexOf("@") < 0)
                    {
                        app.RequiredAttendees.Add(new Attendee(item + "@microsoft.com"));
                    }
                    else
                    {
                        app.RequiredAttendees.Add(new Attendee(item));
                    }
                }
            }
            foreach (var item in receives["CcList"].Split(';'))
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if (item.IndexOf("@") < 0)
                    {
                        app.OptionalAttendees.Add(new Attendee(item + "@microsoft.com"));
                    }
                    else
                    {
                        app.OptionalAttendees.Add(new Attendee(item));
                    }
                }
            }

            app.Subject = title;
            app.Body = new MessageBody();
            app.Body.BodyType = BodyType.HTML;
            app.Body.Text = content;
            app.Importance = Importance.High;
            app.Location = location;
            app.Start = startTime;
            app.End = endTime;

            try
            {
                if (mailType == "3")
                {
                    app.Save(SendInvitationsMode.SendToAllAndSaveCopy);
                }
                else if (mailType == "4")
                {
                    app.Update(ConflictResolutionMode.AlwaysOverwrite, SendInvitationsOrCancellationsMode.SendToAllAndSaveCopy);
                }
                else
                {
                    app.Delete(DeleteMode.MoveToDeletedItems, SendCancellationsMode.SendOnlyToAll);
                }

                ret = app.Id.UniqueId;
            }
            catch(Exception e)
            {
                MailHandler.pendingLogs.Enqueue(e.Message);
            }

            return ret;
        }

        /// <summary>
        /// 发送普通邮件
        /// </summary>
        /// <param name="receives">邮件接收列表</param>
        /// <param name="title">邮件标题</param>
        /// <param name="content">邮件内容</param>
        public static void SendMail(Dictionary<string, string> receives, string title, string content)
        {
            MailHandler.pendingLogs.Enqueue("开始发送邮件。");

            InitializeEWS();
            EmailMessage email = new EmailMessage(service);

            email.Sender = new EmailAddress(MailAccount.Account); //发件人邮箱，名称
            foreach (var item in receives["ToList"].Split(';'))
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if(item.IndexOf("@") < 0)
                    {
                        email.ToRecipients.Add(new EmailAddress(item + "@microsoft.com"));
                    }
                    else
                    {
                        email.ToRecipients.Add(new EmailAddress(item));
                    }
                }
            }
            foreach (var item in receives["CcList"].Split(';'))
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if (item.IndexOf("@") < 0)
                    {
                        email.CcRecipients.Add(new EmailAddress(item + "@microsoft.com"));
                    }
                    else
                    {
                        email.CcRecipients.Add(new EmailAddress(item));
                    }
                }
            }

            email.Subject = title;
            email.Body = new MessageBody();
            email.Body.BodyType = BodyType.HTML;
            email.Body.Text = content;
            email.Importance = Importance.High;
            
            try
            {
                email.Send();
            }
            catch(Exception e)
            {
                MailHandler.pendingLogs.Enqueue(e.Message);
            }
        }
    }
}
