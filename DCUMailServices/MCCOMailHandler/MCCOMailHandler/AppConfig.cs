namespace MCCOMailHandler
{
    class AppConfig
    {
        // 日志文件名
        public static string LogFile { get; set; }

        // 数据库连接字符串
        public static string DBConnectionStr { get; set; }

        // 数据库重连次数
        public static string DBConnectTimes { get; set; }

        // 邮件抄送列表
        public static string CCAlias { get; set; }

        // 邮件模板路径
        public static string MailTemplate { get; set; }

        // MCCO 系统主页地址
        public static string WebHomeUrl { get; set; }
    }
}
