using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Config;
using Tools;
using System.Web;
using System.Data.SqlClient;


namespace DateEntity
{
    public class DateEntity
    {
    }

    public class BLL
    {
        /// <summary>
        /// 返回通用过程结果集
        /// </summary>
        /// <param name="storedStr">存储过程名</param>
        /// <param name="m">参数</param>      
        public static DataSet ReturnTongYong(string storedStr, Model m)
        {
            SqlParameter[] sqlParameter =
                 {
                     //将UI层传递过来的数据变量传给存储过程
                     new SqlParameter("DateTable",m.DateTable),
                     new SqlParameter("soryType",m.soryType),
                     new SqlParameter("KeyTitle",m.KeyTitle),
                     new SqlParameter("pagesize",m.pagesize),
                     new SqlParameter("pageindex",m.pageindex),
                     new SqlParameter("SearchByTitle",m.SearchByTitle),
                     new SqlParameter("SearchByType",m.SearchByType),
                     new SqlParameter("keyword",m.keyword),    
                     new SqlParameter("Wheresql",m.WhereSQL),   
                 };
            DAL d = new DAL();
            return d.ExecutesqlData(storedStr, null, null, sqlParameter);
        }

        /// <summary>
        /// 返回通用过程结果集
        /// </summary>
        /// <param name="storedStr">存储过程名</param>
        /// <param name="m">参数</param>      
        public static DataSet ReturnTongYong(string storedStr, Model m, SqlConnection con)
        {
            SqlParameter[] sqlParameter =
                 {
                     //将UI层传递过来的数据变量传给存储过程
                     new SqlParameter("DateTable",m.DateTable),
                     new SqlParameter("soryType",m.soryType),
                     new SqlParameter("KeyTitle",m.KeyTitle),
                     new SqlParameter("pagesize",m.pagesize),
                     new SqlParameter("pageindex",m.pageindex),
                     new SqlParameter("SearchByTitle",m.SearchByTitle),
                     new SqlParameter("SearchByType",m.SearchByType),
                     new SqlParameter("keyword",m.keyword),    
                     new SqlParameter("Wheresql",m.WhereSQL),   
                 };
            DAL d = new DAL();
            return d.ExecutesqlData(storedStr, null, con, sqlParameter);
        }

        /// <summary>
        /// 返回通用过程结果
        /// </summary>
        /// <param name="storedStr">存储过程名</param>
        /// <param name="m">参数</param>
        public static int getTongYongCount(string storedStr, Model m)
        {
            SqlParameter[] sqlParameter =
                 {
                     //将UI层传递过来的数据变量传给存储过程
                     new SqlParameter("DateTable",m.DateTable),
                     new SqlParameter("soryType",m.soryType),
                     new SqlParameter("KeyTitle",m.KeyTitle),
                     new SqlParameter("pagesize",m.pagesize),
                     new SqlParameter("pageindex",m.pageindex),
                     new SqlParameter("SearchByTitle",m.SearchByTitle),
                     new SqlParameter("SearchByType",m.SearchByType),
                     new SqlParameter("keyword",m.keyword),     
                     new SqlParameter("Wheresql",m.WhereSQL),   
                 };
            DAL d = new DAL();

            SqlDataReader reader = d.ExecuteSqlDataReader(storedStr, null, null, sqlParameter);
            if (reader.Read())
                return reader.GetInt32(0);
            else
                return 0;
        }

        /// <summary>
        /// 返回通用过程结果
        /// </summary>
        /// <param name="storedStr">存储过程名</param>
        /// <param name="m">参数</param>
        public static int getTongYongCount(string storedStr, Model m, SqlConnection con)
        {
            SqlParameter[] sqlParameter =
                 {
                     //将UI层传递过来的数据变量传给存储过程
                     new SqlParameter("DateTable",m.DateTable),
                     new SqlParameter("soryType",m.soryType),
                     new SqlParameter("KeyTitle",m.KeyTitle),
                     new SqlParameter("pagesize",m.pagesize),
                     new SqlParameter("pageindex",m.pageindex),
                     new SqlParameter("SearchByTitle",m.SearchByTitle),
                     new SqlParameter("SearchByType",m.SearchByType),
                     new SqlParameter("keyword",m.keyword),     
                     new SqlParameter("Wheresql",m.WhereSQL),   
                 };
            DAL d = new DAL();

            SqlDataReader reader = d.ExecuteSqlDataReader(storedStr, null, con, sqlParameter);
            if (reader.Read())
                return reader.GetInt32(0);
            else
                return 0;
        }

        /// <summary>
        /// 返回通用过程结果DataSet 类型
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// 
        public static DataSet ReturnDataSet(string tSql, bool isStore)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecutesqlData(tSql, null, null, null);
            else
                return d.ExecutesqlData(null, tSql, null, null);
        }

        /// <summary>
        /// 返回通用过程结果DataSet 类型
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// <param name="con">con 参数 新数据接接 SqlConnection 类型</param>
        /// 
        public static DataSet ReturnDataSet(string tSql, bool isStore, SqlConnection con)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecutesqlData(tSql, null, con, null);
            else
                return d.ExecutesqlData(null, tSql, con, null);
        }

        /// <summary>
        /// 返回通用过程结果DataSet 类型
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// <param name="sql">sql 参数</param>
        public static DataSet ReturnDataSet(string tSql, bool isStore, SqlParameter[] sql)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecutesqlData(tSql, null, null, sql);
            else
                return d.ExecutesqlData(null, tSql, null, sql);
        }

        /// <summary>
        /// 返回通用过程结果DataSet 类型
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// <param name="con">con 参数 新数据接接 SqlConnection 类型</param>
        /// <param name="sql">sql 参数</param>
        public static DataSet ReturnDataSet(string tSql, bool isStore, SqlConnection con, SqlParameter[] sql)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecutesqlData(tSql, null, con, sql);
            else
                return d.ExecutesqlData(null, tSql, con, sql);
        }

        /// <summary>
        /// 返回通用过程结果SqlDataReader 类型
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// 
        public static SqlDataReader ReturnValue(string tSql, bool isStore)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecuteSqlDataReader(tSql, null, null, null);
            else
                return d.ExecuteSqlDataReader(null, tSql, null, null);
        }

        /// <summary>
        /// 返回通用过程结果SqlDataReader 类型
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// <param name="con">con 参数 新数据接接 SqlConnection 类型</param>
        /// 
        public static SqlDataReader ReturnValue(string tSql, bool isStore, SqlConnection con)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecuteSqlDataReader(tSql, null, con, null);
            else
                return d.ExecuteSqlDataReader(null, tSql, con, null);
        }

        /// <summary>
        /// 返回通用过程结果SqlDataReader 类型
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// <param name="sql">sql 参数</param>
        public static SqlDataReader ReturnValue(string tSql, bool isStore, SqlParameter[] sql)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecuteSqlDataReader(tSql, null, null, sql);
            else
                return d.ExecuteSqlDataReader(null, tSql, null, sql);
        }

        /// <summary>
        /// 返回通用过程结果SqlDataReader 类型
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// <param name="con">con 参数 新数据接接 SqlConnection 类型</param>
        /// <param name="sql">sql 参数</param>
        public static SqlDataReader ReturnValue(string tSql, bool isStore, SqlConnection con, SqlParameter[] sql)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecuteSqlDataReader(tSql, null, con, sql);
            else
                return d.ExecuteSqlDataReader(null, tSql, con, sql);
        }

        /// <summary>
        /// 返回通用处理过程ExecuteSqlFun
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// 
        public static bool ExecuteSqlFun(string tSql, bool isStore)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecuteSqlFun(tSql, null, null, null);
            else
                return d.ExecuteSqlFun(null, tSql, null, null);
        }

        /// <summary>
        /// 返回通用处理过程ExecuteSqlFun
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// <param name="con">con 参数 新数据接接 SqlConnection 类型</param>
        /// 
        public static bool ExecuteSqlFun(string tSql, bool isStore, SqlConnection con)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecuteSqlFun(tSql, null, con, null);
            else
                return d.ExecuteSqlFun(null, tSql, con, null);
        }

        /// <summary>
        /// 返回通用处理过程ExecuteSqlFun
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// <param name="sql">sql 参数</param>
        public static bool ExecuteSqlFun(string tSql, bool isStore, SqlParameter[] sql)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecuteSqlFun(tSql, null, null, sql);
            else
                return d.ExecuteSqlFun(null, tSql, null, sql);
        }

        /// <summary>
        /// 返回通用处理过程ExecuteSqlFun
        /// </summary>
        /// <param name="tSql">SQL语句</param>
        /// <param name="isStore">isStore 是否为存储过程</param>
        /// <param name="con">con 参数 新数据接接 SqlConnection 类型</param>
        /// <param name="sql">sql 参数</param>
        public static bool ExecuteSqlFun(string tSql, bool isStore, SqlConnection con, SqlParameter[] sql)
        {
            DAL d = new DAL();
            if (isStore)
                return d.ExecuteSqlFun(tSql, null, con, sql);
            else
                return d.ExecuteSqlFun(null, tSql, con, sql);
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="mytype">mytype 日志类型</param>
        /// <param name="username">username 日志操作人</param>
        /// <param name="counternt">counternt 日志内容</param>
        /// 


        public static double getInfoFloat(string webstr)
        {
            double returnValue = 0;
            SqlConnection sqlConn = DAL.localrcon();
            sqlConn.Open();
            SqlCommand cmdDB = new SqlCommand(webstr, sqlConn);
            SqlDataReader reader = cmdDB.ExecuteReader();
            if (reader.Read())
            {
                returnValue = reader.GetDouble(0);
            }
            sqlConn.Close();
            return returnValue;
        }

        public static int getInfoint(string webstr)
        {
            int returnValue = 0;
            SqlConnection sqlConn = DAL.localrcon();
            sqlConn.Open();
            SqlCommand cmdDB = new SqlCommand(webstr, sqlConn);
            SqlDataReader reader = cmdDB.ExecuteReader();
            if (reader.Read())
            {
                returnValue = reader.GetInt32(0);
            }
            sqlConn.Close();
            return returnValue;
        }

    }

    public class DAL
    {
        /// <summary>
        /// 默认网站数据库链接 读
        /// </summary>
        public static SqlConnection localwcon()
        {
            bool islocal = false;
            GetConfigInfo configinfo = new GetConfigInfo();
            string getSqlxml = configinfo.getSqlconfigfile;

            islocal = Convert.ToBoolean(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(getSqlxml, "islocal")));
            return new SqlConnection(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(getSqlxml, "databasecon")));
        }

        /// <summary>
        /// 默认网站数据库链接
        /// </summary>
        public static SqlConnection localrcon()
        {
            bool islocal = false;
            GetConfigInfo configinfo = new GetConfigInfo();
            string getSqlxml = configinfo.getSqlconfigfile;
            islocal = Convert.ToBoolean(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(getSqlxml, "islocal")));
            return new SqlConnection(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(getSqlxml, "databasecon")));
        }


        /// <summary>
        /// 默认网站数据库链接 读
        /// </summary>
        public static SqlConnection webwcon()
        {

            GetConfigInfo configinfo = new GetConfigInfo();
            string getSqlxml = configinfo.getSqlconfigfile;
            return new SqlConnection(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(getSqlxml, "databasecon")));
        }

        /// <summary>
        /// 默认网站数据库链接
        /// </summary>
        public static SqlConnection webrcon()
        {

            GetConfigInfo configinfo = new GetConfigInfo();
            string getSqlxml = configinfo.getSqlconfigfile;
            return new SqlConnection(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(getSqlxml, "databasecon")));
        }


        /// <summary>
        /// 得到数据结果集 DataReader类型
        /// </summary>
        /// <param name="storedName">传入存储过程名 可为null</param>
        /// <param name="sqlStr">sql 语句 可以为拼接SQL语句</param> 
        /// <param name="con">SqlConnection 类型,为NULL则调用webconfig中数据库</param> 
        /// <param name="sql">SqlParameter 类型 为空为无参数</param> 
        ///
        public SqlDataReader ExecuteSqlDataReader(string storedName, string sqlStr, SqlConnection con, SqlParameter[] sql)
        {
            SqlConnection tempcon;

            if (con == null)
                tempcon = localrcon();
            else
                tempcon = con;

            try
            {
                tempcon.Open();
                SqlCommand com;
                if (storedName != null)
                {
                    com = new SqlCommand(storedName, tempcon);
                    com.CommandType = CommandType.StoredProcedure;
                }
                else
                {
                    com = new SqlCommand(sqlStr, tempcon);
                }

                if (sql != null)
                    com.Parameters.AddRange(sql);

                SqlDataReader reader = com.ExecuteReader();
                return reader;
            }
            catch (Exception Error)
            {
                throw Error;
            }
            finally
            {
                //tempcon.Close();
            }
        }

        /// <summary>
        /// 得到数据结果集 DataSet类型
        /// </summary>
        /// <param name="storedName">传入存储过程名 可为null</param>
        /// <param name="sqlStr">sql 语句 可以为拼接SQL语句</param> 
        /// <param name="con">SqlConnection 类型,为NULL则调用webconfig中数据库</param> 
        /// <param name="sql">SqlParameter 类型 为空为无参数</param> 
        ///
        public DataSet ExecutesqlData(string storedName, string sqlStr, SqlConnection con, SqlParameter[] sql)
        {
            SqlConnection tempcon;

            if (con == null)
                tempcon = localrcon();
            else
                tempcon = con;

            try
            {
                tempcon.Open();
                SqlCommand com;
                if (storedName != null)
                {
                    com = new SqlCommand(storedName, tempcon);
                    com.CommandType = CommandType.StoredProcedure;
                }
                else
                {
                    com = new SqlCommand(sqlStr, tempcon);
                }

                if (sql != null)
                    com.Parameters.AddRange(sql);

                SqlDataAdapter adapt = new SqlDataAdapter(com);
                DataSet ds = new DataSet("Data");
                adapt.Fill(ds, "getData");
                return ds;
            }
            catch (Exception Error)
            {
                throw Error;
            }
            finally
            {
                tempcon.Close();
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="storedName">传入存储过程名 可为null</param>
        /// <param name="sqlStr">sql 语句 可以为拼接SQL语句</param> 
        /// <param name="con">SqlConnection 类型,为NULL则调用webconfig中数据库</param> 
        /// <param name="sql">SqlParameter 类型 为空为无参数</param> 
        /// 
        public bool ExecuteSqlFun(string storedName, string sqlStr, SqlConnection con, SqlParameter[] sql)
        {
            bool returnValue = false;

            SqlConnection tempcon;

            if (con == null)
                tempcon = localrcon();
            else
                tempcon = con;

            try
            {
                tempcon.Open();
                SqlCommand com;
                if (storedName != null)
                {
                    com = new SqlCommand(storedName, tempcon);
                    com.CommandType = CommandType.StoredProcedure;
                }
                else
                {
                    com = new SqlCommand(sqlStr, tempcon);
                }

                if (sql != null)
                    com.Parameters.AddRange(sql);

                com.ExecuteNonQuery();
                returnValue = true;
            }
            catch (Exception Error)
            {
                throw Error;
            }
            finally
            {
                tempcon.Close();
            }
            return returnValue;
        }
    }

    //类模型.添加中.
    public class Model
    {
        //这里是通用存储过程
        private string _dateTable = string.Empty;
        public string DateTable { get { return this._dateTable; } set { this._dateTable = value; } }

        private string _soryType = string.Empty;
        public string soryType { get { return this._soryType; } set { this._soryType = value; } }

        private string _keyTitle = string.Empty;
        public string KeyTitle { get { return this._keyTitle; } set { this._keyTitle = value; } }

        private string _searchByTitle = string.Empty;
        public string SearchByTitle { get { return this._searchByTitle; } set { this._searchByTitle = value; } }

        private string _searchByType = string.Empty;
        public string SearchByType { get { return this._searchByType; } set { this._searchByType = value; } }

        private string _wheresql = string.Empty;
        public string WhereSQL { get { return this._wheresql; } set { this._wheresql = value; } }

        private int _pagesize = 1;
        public int pagesize { get { return this._pagesize; } set { this._pagesize = value; } }

        private string _pageindex = string.Empty;
        public string pageindex { get { return this._pageindex; } set { this._pageindex = value; } }

        private string _keyword = string.Empty;
        public string keyword { get { return this._keyword; } set { this._keyword = value; } }
    }
}
