namespace MCCOMailHandler
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    class MCCODatabase
    {
        /// <summary>
        /// 根据传入的sql语句读取数据，返回数据集
        /// </summary>
        /// <param name="connection">已经打开的数据库连接</param>
        /// <param name="sql">查询语句</param>
        public static DataTable QueryData(SqlConnection connection, string sql)
        {
            if (connection.State != ConnectionState.Open)
            {
                if(!MailHandler.ConnectDatabase())
                {
                    return null;
                }
            }

            SqlCommand cmd = null;
            SqlDataAdapter adapter = null;
            DataTable dt = new DataTable();
            try
            {
                cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                adapter = new SqlDataAdapter(cmd);                
                adapter.Fill(dt);
            }
            catch(Exception e)
            {
                MailHandler.pendingLogs.Enqueue(e.Message);
            }
            
            cmd.Dispose();
            adapter.Dispose();
            return dt;
        }

        /// <summary>
        /// 执行传入的sql语句
        /// </summary>
        /// <param name="connection">已经打开的数据库连接</param>
        /// <param name="sql">查询语句</param>
        public static int ExecuteSql(SqlConnection connection, string sql)
        {
            if (connection.State != ConnectionState.Open)
            {
                if (!MailHandler.ConnectDatabase())
                {
                    return -1;
                }
            }
            
            int ret = -1;
            SqlCommand cmd = null;
            try
            {
                cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                ret = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MailHandler.pendingLogs.Enqueue(e.Message);
            }

            cmd.Dispose();
            return ret;
        }
    }
}
