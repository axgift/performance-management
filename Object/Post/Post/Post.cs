using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq; 
using System.Collections;
using Config;
using Tools;
using DateEntity;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using System.Globalization;

namespace Post
{
    public class Post
    {

        public static void Repost(HttpContext context)
        {
            string code = Tools.Secrecy.Escape("0");
            string returnMessage = Secrecy.Escape("Sorry, you have no access to our data. Please contact:admin");
            string Extdata = "{}";
            StringBuilder sb = new StringBuilder();
            string staffID = "-9999", datalist = string.Empty, outdatalist = string.Empty;

            if (Tools.Cookie.CookieCheck("BGstaffID"))
                staffID = Tools.Cookie.GetEncryptedCookieValue("BGstaffID");

            if (context.Request["r"] != null)
            {

                string r = Tools.Tools.getSafeCode(context.Request["r"]).ToLower();

                string outsqlstr = string.Empty;
                string outsqlvalue = string.Empty;
                string setsqlvalue = string.Empty;
                string dataname = string.Empty;

                string runsqlstr = string.Empty;

                if (r == "checkuser")
                {
                    string useremail = Tools.Tools.getSafeCode(context.Request["useremail"]);
                    string password = Tools.Tools.getSafeCode(context.Request["password"]);
                    string uty = Tools.Tools.getSafeCode(context.Request["uty"]);

                    SqlDataReader reader = BLL.ReturnValue("select LoginName,Email,LoginPassword,Status,ID,DepartmentID,LevelType,UserNmae from Web_staffes where LoginName='" + useremail + "'", false);
                    if (reader.Read())
                    {
                        if (reader.GetBoolean(3) == false)
                        {
                            code = Tools.Secrecy.Escape("0");
                            returnMessage = Tools.Secrecy.Escape("对不起,你帐号被限请联系网站管理员.");
                        }
                        else if (password == reader.GetString(2))
                        {
                            Tools.Cookie.SetEncryptedCookie("BGstaffID", reader.GetInt32(4).ToString(), DateTime.Now.AddDays(7));
                            Tools.Cookie.SetEncryptedCookie("BGstaffType", reader.GetInt32(6).ToString(), DateTime.Now.AddDays(7));
                            Tools.Cookie.SetEncryptedCookie("BGstaffmail", useremail, DateTime.Now.AddDays(7));
                            Tools.Cookie.SetEncryptedCookie("checkvalue", useremail + reader.GetString(2) + reader.GetBoolean(3).ToString() + reader.GetInt32(6).ToString(), DateTime.Now.AddDays(7));
                            Tools.Cookie.SetEncryptedCookie("departmentid", reader.GetInt32(5).ToString(), DateTime.Now.AddDays(7));
                            Tools.Cookie.SetEncryptedCookie("BGUsername", (reader.IsDBNull(7) ? useremail : reader.GetString(7)), DateTime.Now.AddDays(7));

                            if (reader.GetInt32(6) >0)
                            {
                                code = Tools.Secrecy.Escape("1");
                                returnMessage = Tools.Secrecy.Escape("usu");
                            }
                            else
                            {
                                code = Tools.Secrecy.Escape("1");
                                returnMessage = Tools.Secrecy.Escape("admin");
                            }
                        }
                        else
                        {
                            code = Tools.Secrecy.Escape("0");
                            returnMessage = Tools.Secrecy.Escape("密码错误");
                        }
                    }
                    else
                    {
                        code = Tools.Secrecy.Escape("2");
                        returnMessage = Tools.Secrecy.Escape("你还没有注册,请注册后使用");
                    }
                    reader.Close();
                }
                else if (r == "addnewstaff")
                {
                    string postdata = context.Request["postdata"];

                    JsonData jsonData = JsonMapper.ToObject(postdata);


                    SqlConnection sqlConn = DAL.localwcon();
                    SqlCommand cmdDB = new SqlCommand(@"", sqlConn);
                    cmdDB = new SqlCommand(@"Insert Web_staffes([LoginName]
      ,[LoginPassword]
      ,[UserNmae]
      ,[DepartmentID]
      ,[LevelType]
      ,[Job]
      ,[Email]
      ,[Status]) values(@LoginName
      ,@LoginPassword
      ,@UserNmae
      ,@DepartmentID
      ,@LevelType
      ,@Job
      ,@Email
      ,@Status)", sqlConn);
                    cmdDB.Parameters.Add("@LoginName", SqlDbType.VarChar).Value = jsonData["loginname"].ToString();
                    cmdDB.Parameters.Add("@LoginPassword", SqlDbType.VarChar).Value = jsonData["loginpassword"].ToString();
                    cmdDB.Parameters.Add("@UserNmae", SqlDbType.VarChar).Value = jsonData["username"].ToString();
                    cmdDB.Parameters.Add("@DepartmentID", SqlDbType.Int).Value = jsonData["department"].ToString();
                    cmdDB.Parameters.Add("@LevelType", SqlDbType.Int).Value = jsonData["leveltype"].ToString();
                    cmdDB.Parameters.Add("@Job", SqlDbType.VarChar).Value = jsonData["job"].ToString();
                    cmdDB.Parameters.Add("@Email", SqlDbType.VarChar).Value = jsonData["email"].ToString();
                    cmdDB.Parameters.Add("@Status", SqlDbType.Bit).Value = (jsonData["status"].ToString()=="1"?true:false);

                    cmdDB.Connection.Open();
                    try
                    {
                        cmdDB.ExecuteNonQuery();


                        code = Tools.Secrecy.Escape("1");
                        returnMessage = Tools.Secrecy.Escape("ok");
                    }
                    catch { }
                    cmdDB.Connection.Close();









                }
                else if (r == "updatestaff")
                {
                    string postdata = context.Request["postdata"];

                    JsonData jsonData = JsonMapper.ToObject(postdata);


                    SqlConnection sqlConn = DAL.localwcon();
                    SqlCommand cmdDB = new SqlCommand(@"", sqlConn);
                    cmdDB = new SqlCommand(@"UPDATE Web_staffes SET LoginName=@LoginName
      ,LoginPassword=@LoginPassword
      ,UserNmae=@UserNmae
      ,DepartmentID=@DepartmentID
      ,LevelType=@LevelType
      ,Job=@Job
      ,Email=@Email
      ,Status=@Status
      WHERE ID=@ID", sqlConn);
                    cmdDB.Parameters.Add("@LoginName", SqlDbType.VarChar).Value = jsonData["loginname"].ToString();
                    cmdDB.Parameters.Add("@LoginPassword", SqlDbType.VarChar).Value = jsonData["loginpassword"].ToString();
                    cmdDB.Parameters.Add("@UserNmae", SqlDbType.VarChar).Value = jsonData["username"].ToString();
                    cmdDB.Parameters.Add("@DepartmentID", SqlDbType.Int).Value = jsonData["department"].ToString();
                    cmdDB.Parameters.Add("@LevelType", SqlDbType.Int).Value = jsonData["leveltype"].ToString();
                    cmdDB.Parameters.Add("@Job", SqlDbType.VarChar).Value = jsonData["job"].ToString();
                    cmdDB.Parameters.Add("@Email", SqlDbType.VarChar).Value = jsonData["email"].ToString();
                    cmdDB.Parameters.Add("@Status", SqlDbType.Bit).Value = (jsonData["status"].ToString() == "1" ? true : false);
                    cmdDB.Parameters.Add("@ID", SqlDbType.Int).Value = Int32.Parse(jsonData["id"].ToString());

                    cmdDB.Connection.Open();
                    try
                    {
                        cmdDB.ExecuteNonQuery();


                        code = Tools.Secrecy.Escape("1");
                        returnMessage = Tools.Secrecy.Escape("ok");
                    }
                    catch { }
                    cmdDB.Connection.Close();









                }
                else if (r == "deleteuser")
                {
                    string id = Tools.Tools.getSafeCode(context.Request["id"]).ToLower();

                    bool updatestatus = false;
                    updatestatus = BLL.ExecuteSqlFun("delete Web_staffes where id=" + id, false);

                    if (updatestatus)
                    {
                        code = Tools.Secrecy.Escape("1");
                        returnMessage = Tools.Secrecy.Escape("删除成功");

                    }


                }
                else if (r == "saveconfig")
                {
                    string configfile = string.Empty;
                    string webconfigfile = string.Empty;
                    GetConfigInfo configinfo = new GetConfigInfo();
                    webconfigfile = configinfo.getConfigfile;

                    string webtitle = Tools.Tools.getSafeCode(context.Request["webtitle"]).ToLower();
                    string savedays = Tools.Tools.getSafeCode(context.Request["savedays"]).ToLower();
                    string endpostday = Tools.Tools.getSafeCode(context.Request["endpostday"]).ToLower();


                    GetConfigInfo.SaveAppSettings(webconfigfile, "webtitle", Tools.Secrecy.Encode(webtitle));
                    GetConfigInfo.SaveAppSettings(webconfigfile, "savedays", Tools.Secrecy.Encode(savedays));
                    GetConfigInfo.SaveAppSettings(webconfigfile, "endpostday", Tools.Secrecy.Encode(endpostday));



                    code = Tools.Secrecy.Escape("1");
                    returnMessage = Tools.Secrecy.Escape("保存成功");


                }
                else if (r == "saveuserinfo")
                {
                    string id = context.Request["id"].ToString().Trim();
                    string email = context.Request["email"].ToString().Trim();
                    string password = context.Request["password"].ToString().Trim();

                    bool updatestatus = false;

                    updatestatus = BLL.ExecuteSqlFun("update Web_staffes set Email='" + email + "',LoginPassword='" + password + "' where ID=" + id, false);

                    if (updatestatus)
                    {
                        code = Tools.Secrecy.Escape("1");
                        returnMessage = Tools.Secrecy.Escape("保存成功");
                    }

                }
                else if (r == "checkuserforweb")
                {

                    if (Tools.Cookie.CookieCheck("useremail"))
                    {

                        code = Tools.Secrecy.Escape("1");
                        returnMessage = Tools.Secrecy.Escape(Tools.Cookie.GetEncryptedCookieValue("useremail"));

                    }
                }
                else if (r == "getuserlist")
                {
                    DataSet ds = new DataSet();

                    string departmentid = "-9999";

                    if (Tools.Cookie.CookieCheck("departmentid"))
                        departmentid = Tools.Cookie.GetEncryptedCookieValue("departmentid");

                    ds = BLL.ReturnDataSet("select * from Web_staffes where DepartmentID=" + departmentid + " and LevelType>0", false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        code = Tools.Secrecy.Escape("1");
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                outdatalist = "{\"UserNmae\":\"" + ds.Tables[0].Rows[i]["UserNmae"] + "\",\"ID\":\"" + ds.Tables[0].Rows[i]["ID"] + "\"}";
                            else
                                outdatalist += ",{\"UserNmae\":\"" + ds.Tables[0].Rows[i]["UserNmae"] + "\",\"ID\":\"" + ds.Tables[0].Rows[i]["ID"] + "\"}";

                        }

                    }
                }
                else if (r == "getweekplanlist")
                {
                    DataSet ds = new DataSet();


                    ds = BLL.ReturnDataSet("select Plan_ID,status from Web_WeekPlan where Timestamp>'" + DateTime.Now.AddDays(-90) + "' and StaffID=" + staffID, false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        code = Tools.Secrecy.Escape("1");
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                outdatalist = "{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"])?"1":"0") + "\"}";
                            else
                                outdatalist += ",{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) ? "1" : "0") + "\"}";

                        }

                    }

                }
                else if (r == "getperformancelist")
                {
                    DataSet ds = new DataSet();


                    ds = BLL.ReturnDataSet("select top 10 Plan_ID,Rating from View_Web_WeekSummary_score where StaffID='" + staffID + "' order by Plan_ID desc", false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        code = Tools.Secrecy.Escape("1");
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                outdatalist = "{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Rating\":\"" + ds.Tables[0].Rows[i]["Rating"] + "\"}";
                            else
                                outdatalist += ",{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Rating\":\"" + ds.Tables[0].Rows[i]["Rating"] + "\"}";

                        }

                    }

                }
                else if (r == "getstaff")
                {
                    string page = context.Request["page"].ToString().Trim();
                    int limit = Int32.Parse(context.Request["limit"].ToString());
                    int offset = (Int32.Parse(page) - 1) * limit;
                    int totalcount = 0;


                    DataSet ds = new DataSet();


                    string query = string.Format(@"
                        SELECT *
                        FROM (
                            SELECT TOP {0} *
                            FROM (
                                SELECT TOP ({1} + {0}) *
                                FROM View_Web_Department
                                ORDER BY (SELECT 0)
                            ) AS SubQuery1
                            ORDER BY (SELECT 0) DESC
                        ) AS SubQuery2
                        ORDER BY (SELECT 0) ASC;
                    ", limit,offset);

                    ds = BLL.ReturnDataSet(query, false);



                    SqlDataReader reader = BLL.ReturnValue("SELECT COUNT(*) FROM Web_staffes", false);
                    if (reader.Read())
                    {
                        totalcount = reader.GetInt32(0);
                    }
                    reader.Close();








                    
                    string outJason = "{ \"code\": 0 ,\"msg\":\"没有数据\"}";

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        outJason = "{\r\n";
                        outJason += "   \"code\": 0,\r\n";
                        outJason += "   \"msg\": \"\",\r\n";
                        outJason += "   \"count\": " + totalcount + ",\r\n";
                        outJason += "   \"data\": [\r\n";

                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                            {
                                outJason += "   {\r\n";
                                outJason += "   \"LoginName\": \"" + ds.Tables[0].Rows[i]["LoginName"] + "\",\r\n";
                                outJason += "   \"UserNmae\": \"" + ds.Tables[0].Rows[i]["UserNmae"].ToString() + "\",\r\n";
                                outJason += "   \"loginPass\": \"" + ds.Tables[0].Rows[i]["LoginPassword"].ToString() + "\",\r\n";
                                outJason += "   \"leveltype\": \"" + ds.Tables[0].Rows[i]["LevelType"].ToString() + "\",\r\n";
                                outJason += "   \"DepartMentID\": \"" + ds.Tables[0].Rows[i]["DepartmentID"].ToString() + "\",\r\n";
                                outJason += "   \"DepartMentName\": \"" + ds.Tables[0].Rows[i]["DepartMentName"].ToString() + "\",\r\n";
                                outJason += "   \"id\": \"" + ds.Tables[0].Rows[i]["ID"].ToString() + "\",\r\n";
                                outJason += "   \"Status\": \"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\",\r\n";
                                outJason += "   \"Email\": \"" + ds.Tables[0].Rows[i]["Email"].ToString() + "\",\r\n";
                                outJason += "   \"Job\":\"" + ds.Tables[0].Rows[i]["Job"].ToString() + "\"\r\n";
                                outJason += "   }\r\n";
                            }
                            else
                            {
                                outJason += "   ,{\r\n";
                                outJason += "   \"LoginName\": \"" + ds.Tables[0].Rows[i]["LoginName"] + "\",\r\n";
                                outJason += "   \"UserNmae\": \"" + ds.Tables[0].Rows[i]["UserNmae"].ToString() + "\",\r\n";
                                outJason += "   \"loginPass\": \"" + ds.Tables[0].Rows[i]["LoginPassword"].ToString() + "\",\r\n";
                                outJason += "   \"leveltype\": \"" + ds.Tables[0].Rows[i]["LevelType"].ToString() + "\",\r\n";
                                outJason += "   \"DepartMentID\": \"" + ds.Tables[0].Rows[i]["DepartmentID"].ToString() + "\",\r\n";
                                outJason += "   \"DepartMentName\": \"" + ds.Tables[0].Rows[i]["DepartMentName"].ToString() + "\",\r\n";
                                outJason += "   \"id\": \"" + ds.Tables[0].Rows[i]["ID"].ToString() + "\",\r\n";
                                outJason += "   \"Status\": \"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\",\r\n";
                                outJason += "   \"Email\": \"" + ds.Tables[0].Rows[i]["Email"].ToString() + "\",\r\n";
                                outJason += "   \"Job\":\"" + ds.Tables[0].Rows[i]["Job"].ToString() + "\"\r\n";
                                outJason += "   }\r\n";

                            }
                        }

                        outJason += "";




                        outJason += "    ]\r\n";
                        outJason += "}";




                        context.Response.ContentType = "application/json";
                        context.Response.Write(outJason);
                        context.Response.End();
















                    }

                }
                else if (r == "getsummarylist")
                {
                    DataSet ds = new DataSet();
                    DataSet dsR = new DataSet();

//                    dsR = BLL.ReturnDataSet(@"SELECT 
//    PLAN_ID,
//    STUFF((
//        SELECT ',' + ADMIN_REPLY 
//        FROM Web_WeekSummary_list t2 
//        WHERE t2.PLAN_ID = t1.PLAN_ID  -- 按PLAN_ID分组拼接
//        FOR XML PATH(''), TYPE
//    ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS ADMIN_REPLIES
//FROM Web_WeekSummary_list t1
//GROUP BY PLAN_ID;",false);

                    dsR = BLL.ReturnDataSet(@"SELECT 
    PLAN_ID,
    staffid,
    STUFF((
        SELECT ',' + ADMIN_REPLY 
        FROM Web_WeekSummary_list t2 
        WHERE t2.PLAN_ID = t1.PLAN_ID 
          AND t2.staffid = t1.staffid
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS ADMIN_REPLIES
FROM Web_WeekSummary_list t1
GROUP BY PLAN_ID, staffid;", false);

                    ds = BLL.ReturnDataSet("select Plan_ID,SUM_Reason,Rating,Timestamp from View_Web_WeekSummary_score_List where Timestamp>'" + DateTime.Now.AddDays(-90) + "' and StaffID=" + staffID, false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        code = Tools.Secrecy.Escape("1");
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                            {
                                outdatalist = "{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Timestamp\":\"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Timestamp"]).ToShortDateString() + "\",\"Rating\":\"" + ds.Tables[0].Rows[i]["Rating"] + "\",\"sumreason\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["SUM_Reason"].ToString()) + "\",\"reply\":\"" + GetReply(ds.Tables[0].Rows[i]["Plan_ID"].ToString(), dsR) + "\"}";
                            }
                            else
                            {
                                outdatalist += ",{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Timestamp\":\"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Timestamp"]).ToShortDateString() + "\",\"Rating\":\"" + ds.Tables[0].Rows[i]["Rating"] + "\",\"sumreason\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["SUM_Reason"].ToString()) + "\",\"reply\":\"" + GetReply(ds.Tables[0].Rows[i]["Plan_ID"].ToString(), dsR) + "\"}";
                            }

                        }

                    }
                }
                else if (r == "getsummarylistsbyzj")
                {
                    DataSet ds = new DataSet();
                    string week = Tools.Tools.getSafeCode(context.Request["week"]);
                    string SUM_Reason = string.Empty;

                    SqlDataReader reader = BLL.ReturnValue("select SUM_Reason,Weeklysales,Weeklyadvertisingexpenditure,Numberoforders,Numberofitemssold from Web_WeekSummary where Plan_ID='" + week + "' and staffid=" + staffID, false);
                    if (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false)
                        {
                            SUM_Reason = reader.GetString(0);
                            returnMessage = Tools.Secrecy.Escape(SUM_Reason + "|" + reader.GetDecimal(1).ToString("F") + "|" + reader.GetDecimal(2).ToString("F") + "|" + reader.GetDecimal(3).ToString("F") + "|" + reader.GetDecimal(4).ToString("F"));
                        }
                    }
                    reader.Close();

                    ds = BLL.ReturnDataSet("select Plan_ID,Plan_Note,Status,Timestamp,Rating,Admin_Reply,Reply_timestamp,id from Web_WeekSummary_list where Plan_ID='" + week + "' and staffid=" + staffID, false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        code = Tools.Secrecy.Escape("1");


                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                outdatalist = "{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Plan_Note"] + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) ? " <span class=\"sur_btn\">已完成</span>" : ((Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) == false) ? "<em class=\"blfont\">已逾期</em>" : ""))) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\",\"Timestamp\":\"" + (Convert.ToDateTime(ds.Tables[0].Rows[i]["Timestamp"]).ToShortDateString()) + "\",\"Rating\":\"" + ds.Tables[0].Rows[i]["Rating"] + "\",\"Admin_Reply\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Admin_Reply"].ToString()) + "\",\"Reply_timestamp\":\"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Reply_timestamp"]).ToShortDateString() + "\"}";
                            else
                                outdatalist += ",{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Plan_Note"] + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) ? " <span class=\"sur_btn\">已完成</span>" : ((Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) == false) ? "<em class=\"blfont\">已逾期</em>" : ""))) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\",\"Timestamp\":\"" + (Convert.ToDateTime(ds.Tables[0].Rows[i]["Timestamp"]).ToShortDateString()) + "\",\"Rating\":\"" + ds.Tables[0].Rows[i]["Rating"] + "\",\"Admin_Reply\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Admin_Reply"].ToString()) + "\",\"Reply_timestamp\":\"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Reply_timestamp"]).ToShortDateString() + "\"}";

                        }

                    }

                }
                else if (r == "getweekplanlists")
                {
                    DataSet ds = new DataSet();
                    string week = Tools.Tools.getSafeCode(context.Request["week"]);

                    ds = BLL.ReturnDataSet("select Plan_ID,Plan_Note,status,Function_EndTime,id from Web_WeekPlan_list where Plan_ID='" + week + "' and staffid="+staffID, false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        code = Tools.Secrecy.Escape("1");
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                outdatalist = "{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape("<em>完成时限:" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Function_EndTime"]).ToShortDateString() + "</em>&nbsp;&nbsp;" + ds.Tables[0].Rows[i]["Plan_Note"] + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) ? " <span class=\"sur_btn\">已完成</span>" : " <span class=\"link_btn\"><a href=\"javascript:;\" class=\"sure_btn\" data-id=\"" + ds.Tables[0].Rows[i]["id"] + "\">确认完成</a></span>" + ((Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) == false && Convert.ToDateTime(ds.Tables[0].Rows[i]["Function_EndTime"]) < DateTime.Now) ? "<em class=\"blfont\">已逾期</em>" : ""))) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\"}";
                            else
                                outdatalist += ",{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape("<em>完成时限:" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Function_EndTime"]).ToShortDateString() + "</em>&nbsp;&nbsp;" + ds.Tables[0].Rows[i]["Plan_Note"] + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) ? " <span class=\"sur_btn\">已完成</span>" : " <span class=\"link_btn\"><a href=\"javascript:;\" class=\"sure_btn\" data-id=\"" + ds.Tables[0].Rows[i]["id"] + "\">确认完成</a></span>" + ((Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) == false && Convert.ToDateTime(ds.Tables[0].Rows[i]["Function_EndTime"]) < DateTime.Now) ? "<em class=\"blfont\">已逾期</em>" : ""))) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\"}";

                        }

                    }

                }
                else if (r == "getweekplanlistsbyedit")
                {
                    DataSet ds = new DataSet();
                    string week = Tools.Tools.getSafeCode(context.Request["week"]);

                    ds = BLL.ReturnDataSet("select Plan_ID,Plan_Note,status,Function_EndTime,id from Web_WeekPlan_list where Plan_ID='" + week + "' and staffid=" + staffID+" order by id desc", false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        code = Tools.Secrecy.Escape("1");
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                outdatalist = "{\"id\":\"" + ds.Tables[0].Rows[i]["id"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Plan_Note"].ToString()) + "\",\"Function_EndTime\":\"" + ds.Tables[0].Rows[i]["Function_EndTime"] + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\"}";
                            else
                                outdatalist += ",{\"id\":\"" + ds.Tables[0].Rows[i]["id"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Plan_Note"].ToString()) + "\",\"Function_EndTime\":\"" + ds.Tables[0].Rows[i]["Function_EndTime"] + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\"}";

                        }

                    }

                }
                else if (r == "getweekplanlistsbyzj")
                {
                    DataSet ds = new DataSet();
                    string week = Tools.Tools.getSafeCode(context.Request["week"]);

                    ds = BLL.ReturnDataSet("select Plan_ID,Plan_Note,status,Function_EndTime,id from Web_WeekPlan_list where Plan_ID='" + week + "' and staffid=" + staffID, false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        code = Tools.Secrecy.Escape("1");
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                outdatalist = "{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape("<em>完成时限:" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Function_EndTime"]).ToShortDateString() + "</em>&nbsp;&nbsp;" + ds.Tables[0].Rows[i]["Plan_Note"] + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) ? " <span class=\"sur_btn\">已完成</span>" : ((Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) == false && Convert.ToDateTime(ds.Tables[0].Rows[i]["Function_EndTime"]) < DateTime.Now) ? "<em class=\"blfont\">已逾期</em>" : ""))) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\",\"id\":\"" + ds.Tables[0].Rows[i]["id"] + "\"}";
                            else
                                outdatalist += ",{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape("<em>完成时限:" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Function_EndTime"]).ToShortDateString() + "</em>&nbsp;&nbsp;" + ds.Tables[0].Rows[i]["Plan_Note"] + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) ? " <span class=\"sur_btn\">已完成</span>" : ((Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) == false && Convert.ToDateTime(ds.Tables[0].Rows[i]["Function_EndTime"]) < DateTime.Now) ? "<em class=\"blfont\">已逾期</em>" : ""))) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\",\"id\":\"" + ds.Tables[0].Rows[i]["id"] + "\"}";

                        }

                    }

                }
                else if (r == "addweekplan")
                {
                    DataSet ds = new DataSet();
                    string week = Tools.Tools.getSafeCode(context.Request["week"]);
                    string lastday = context.Request["lastday"].ToString();
                    string planlist = context.Request["planlist"].ToString();

                    JsonData jsonData = JsonMapper.ToObject(planlist);


                    bool isNewstatus = true;

                    SqlDataReader reader = BLL.ReturnValue("select Plan_ID from Web_WeekPlan where Plan_ID='" + week + "' and staffid=" + staffID, false);
                    if (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false && reader.GetString(0).Length > 0)
                        {
                            isNewstatus = false;
                        }
                    }
                    reader.Close();




                    SqlConnection sqlConn = DAL.localwcon();
                    SqlCommand cmdDB;
                    sqlConn.Open();
                    if (isNewstatus)
                    {
                        cmdDB = new SqlCommand(@"INSERT [Web_WeekPlan] (
Timestamp,
Plan_ID,
StaffID
) VALUES (
@Timestamp,
@Plan_ID,
@StaffID
" + ")", sqlConn);

                        cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
                        cmdDB.Parameters.Add("@Plan_ID", SqlDbType.VarChar).Value = week;
                        cmdDB.Parameters.Add("@StaffID", SqlDbType.Int).Value = Int32.Parse(staffID);

                        //== END

                        cmdDB.ExecuteNonQuery();
                    }


                    if (jsonData.Count > 0)
                    {
                        for (int i = 0; i < jsonData.Count; i++)
                        {
                            cmdDB = new SqlCommand(@"INSERT [Web_WeekPlan_list] (
Timestamp,
Plan_ID,
staffID,
Plan_Note,
Function_EndTime,
EndTime,
Status
) VALUES (
@Timestamp,
@Plan_ID,
@staffID,
@Plan_Note,
@Function_EndTime,
@EndTime,
@Status
" + ")", sqlConn);

                            cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
                            cmdDB.Parameters.Add("@Plan_ID", SqlDbType.VarChar).Value = week;
                            cmdDB.Parameters.Add("@staffID", SqlDbType.Int).Value = staffID;
                            cmdDB.Parameters.Add("@Plan_Note", SqlDbType.VarChar).Value = Tools.Secrecy.unEscape(jsonData[i].ToString().Split('|')[0].ToString());
                            cmdDB.Parameters.Add("@Function_EndTime", SqlDbType.DateTime).Value = Convert.ToDateTime(Tools.Secrecy.unEscape(jsonData[i].ToString().Split('|')[1].ToString()));
                            cmdDB.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = Convert.ToDateTime(lastday);
                            cmdDB.Parameters.Add("@Status", SqlDbType.Bit).Value = false;
                            cmdDB.ExecuteNonQuery();
                        }

                    }












                    sqlConn.Close();




                    code = Tools.Secrecy.Escape("1");
                    returnMessage = Tools.Secrecy.Escape("保存成功");























                }
                else if (r == "updateweekplan")
                {
                    string week = context.Request["week"].ToString();
                    string planlist = context.Request["planlist"].ToString();
                    JsonData jsonData = JsonMapper.ToObject(planlist);

                    bool updateStatus = false;

                    updateStatus = BLL.ExecuteSqlFun("update Web_WeekPlan set status=1 where Plan_ID='" + week + "' and StaffID=" + staffID, false);


                    if (jsonData.Count > 0)
                    {
                        for (int i = 0; i < jsonData.Count; i++)
                        {
                            updateStatus = BLL.ExecuteSqlFun("update Web_WeekPlan_list set Plan_Note='" + jsonData[i]["plan_note"].ToString() + "',Function_EndTime='" + jsonData[i]["plan_date"].ToString() + "' where id=" + jsonData[i]["id"].ToString(), false);

                        }

                    }

                    if(updateStatus)
                        code = Tools.Secrecy.Escape("1");





                }
                else if (r == "addweeksummanr")
                {
                    string summaryData = context.Request["summaryData"].ToString();

                    string listjson = context.Request["listjson"].ToString();

                    JsonData jsonData = JsonMapper.ToObject(summaryData);

                    JsonData summaryArrData = JsonMapper.ToObject(listjson);

                    string week = jsonData["week"].ToString();
                    string issues = jsonData["issues"].ToString();

                    string Weeklysales = jsonData["Weeklysales"].ToString().Length > 0 ? jsonData["Weeklysales"].ToString() : "0";
                    string Weeklyadvertisingexpenditure = jsonData["Weeklyadvertisingexpenditure"].ToString().Length > 0 ? jsonData["Weeklyadvertisingexpenditure"].ToString() : "0";
                    string Numberoforders = jsonData["Numberoforders"].ToString().Length > 0 ? jsonData["Numberoforders"].ToString() : "0";
                    string Numberofitemssold = jsonData["Numberofitemssold"].ToString().Length > 0 ? jsonData["Numberofitemssold"].ToString() : "0";

                    bool updatestatus = false;


                    updatestatus = BLL.ExecuteSqlFun(string.Format(@"insert Web_WeekSummary([Plan_ID]
      ,[StaffID]
      ,[Status]
      ,[Timestamp]
      ,[SUM_Reason]
      ,[Weeklysales]
      ,[Weeklyadvertisingexpenditure]
      ,[Numberoforders]
      ,[Numberofitemssold]
) select [Plan_ID]
      ,[StaffID]
      ,0
      ,'{0}'
      ,'{2}'
      ,{3}
      ,{4}
      ,{5}
      ,{6}
from Web_WeekPlan where Plan_ID='{1}' and StaffID={7}", DateTime.Now.ToString(), week, issues, Weeklysales, Weeklyadvertisingexpenditure, Numberoforders, Numberofitemssold,staffID), false);



                    updatestatus = BLL.ExecuteSqlFun(string.Format(@"insert Web_WeekSummary_list([Plan_ID]
      ,[WeekPlanID]
      ,[Plan_Note]
      ,[StaffID]
      ,[Status]
      ,[Timestamp]
      ,[Rating]
      ,[Admin_Reply]
      ,[Reply_timestamp]) select [Plan_ID]
      ,[ID]
      ,[Plan_Note]
      ,[StaffID]
      ,[Status]
      ,'{0}'
      ,0
      ,''
      ,'' from Web_WeekPlan_list where Plan_ID='{1}' and StaffID={2}", DateTime.Now.ToString(), week,staffID), false);


                    if (summaryArrData.Count > 0)
                    {
                        for (int i = 0; i < summaryArrData.Count; i++)
                        {
                            updatestatus = BLL.ExecuteSqlFun("update Web_WeekSummary_list set Plan_Note=Plan_Note+'</br></br>'+'" + summaryArrData[i]["summarynote"] + "' where WeekPlanID=" + summaryArrData[i]["id"], false);
                        }

                    }



                    if (updatestatus)
                    {
                        code = Tools.Secrecy.Escape("1");
                        returnMessage = Tools.Secrecy.Escape("保存成功");
                    }



                }
                else if (r == "saveplanstatus")
                {

                    string id = Tools.Tools.getSafeCode(context.Request["id"]);
                    bool updatestatus = false;

                    updatestatus = BLL.ExecuteSqlFun("UPDATE Web_WeekPlan_list SET Status=1 WHERE ID=" + id, false);


                    if (updatestatus)
                    {
                        code = Tools.Secrecy.Escape("1");
                        returnMessage = Tools.Secrecy.Escape("保存成功");
                    }



                }
                else if (r == "getweekplanstatus")
                {

                    string week = Tools.Tools.getSafeCode(context.Request["week"]);

                    bool isHave = false;

                    SqlDataReader reader = BLL.ReturnValue("SELECT Plan_ID FROM Web_WeekPlan WHERE Plan_ID='" + week + "' and StaffID=" + staffID, false);

                    if (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false && reader.GetString(0).Length > 0)
                        {
                            isHave = true;
                        }
                    }
                    reader.Close();


                    if (isHave)
                    {
                        code = Tools.Secrecy.Escape("1");
                        returnMessage = Tools.Secrecy.Escape("已存在");
                    }
                    else
                    {
                        code = Tools.Secrecy.Escape("0");
                        returnMessage = Tools.Secrecy.Escape("不存在");
                    }



                }
                else if (r == "getweeksummarytatus")
                {

                    string week = Tools.Tools.getSafeCode(context.Request["week"]);

                    bool isHave = false;

                    SqlDataReader reader = BLL.ReturnValue("SELECT Plan_ID FROM Web_WeekSummary WHERE Plan_ID='" + week + "' and StaffID=" + staffID, false);

                    if (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false && reader.GetString(0).Length > 0)
                        {
                            isHave = true;
                        }
                    }
                    reader.Close();


                    if (isHave)
                    {
                        code = Tools.Secrecy.Escape("1");
                        returnMessage = Tools.Secrecy.Escape("已存在");
                    }
                    else
                    {
                        code = Tools.Secrecy.Escape("0");
                        returnMessage = Tools.Secrecy.Escape("不存在");
                    }



                }
                else if (r == "getuserinfobyid")
                {
                    string userid = Tools.Tools.getSafeCode(context.Request["id"]);

                    DataSet ds = new DataSet();

                    ds = BLL.ReturnDataSet("select * from View_Web_WeekSummary_score where staffid="+staffID+" order by plan_id desc", false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        code = Tools.Secrecy.Escape("1");

                       
                        
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                            {
                                returnMessage = Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Plan_ID"] + "|" + ds.Tables[0].Rows[i]["rating"]);
                            }
                            else if (i == 1)
                            {
                                outdatalist = "{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"rating\":\"" + ds.Tables[0].Rows[i]["rating"] + "\"}";
                            }
                            else
                            {
                                outdatalist += ",{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"rating\":\"" + ds.Tables[0].Rows[i]["rating"] + "\"}";
                            }
                        }

                    }

                }
                else if (r == "getweekplanbyuserid")
                {


                    string week = Tools.Tools.getSafeCode(context.Request["week"]);
                    string id = Tools.Tools.getSafeCode(context.Request["id"]);

                    string weekStatus = "1";


                    SqlDataReader reader = BLL.ReturnValue("select Status from Web_WeekPlan where Plan_ID='" + week + "' and StaffID=" + id, false);
                    if (reader.Read())
                    {
                        weekStatus = reader.GetBoolean(0) ? "1" : "0";
                    }
                    reader.Close();


                    DataSet ds = new DataSet();

                    ds = BLL.ReturnDataSet("select Plan_ID,Plan_Note,status,Function_EndTime,id from Web_WeekPlan_list where Plan_ID='" + week + "' and staffid=" + id+" order by id desc", false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        code = Tools.Secrecy.Escape("1");
                        returnMessage = Tools.Secrecy.Escape(weekStatus);

                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                outdatalist = "{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape("<em>完成时限:" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Function_EndTime"]).ToShortDateString() + "</em>&nbsp;&nbsp;" + ds.Tables[0].Rows[i]["Plan_Note"]) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\"}";
                            else
                                outdatalist += ",{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape("<em>完成时限:" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Function_EndTime"]).ToShortDateString() + "</em>&nbsp;&nbsp;" + ds.Tables[0].Rows[i]["Plan_Note"]) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\"}";

                        }

                    }



                }
                else if (r == "getweekbyuserid")
                {

                    string week = Tools.Tools.getSafeCode(context.Request["week"]);
                    string id = Tools.Tools.getSafeCode(context.Request["id"]);

                    decimal Weeklysales=0,Weeklyadvertisingexpenditure=0,Numberoforders=0,Numberofitemssold=0;

                    int score = 0, scorenum = 0;

                    string sum_reason = string.Empty;

                    SqlDataReader reader = BLL.ReturnValue("select SUM_Reason,Weeklysales,Weeklyadvertisingexpenditure,Numberoforders,Numberofitemssold from Web_WeekSummary where plan_id='" + week + "' and staffid=" + id, false);

                    if (reader.Read())
                    {
                        sum_reason = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        Weeklysales = reader.IsDBNull(0) ? 0 : reader.GetDecimal(1);
                        Weeklyadvertisingexpenditure = reader.IsDBNull(0) ? 0 : reader.GetDecimal(2);
                        Numberoforders = reader.IsDBNull(0) ? 0 : reader.GetDecimal(3);
                        Numberofitemssold = reader.IsDBNull(0) ? 0 : reader.GetDecimal(4);

                    }
                    reader.Close();


                   


                    DataSet ds = new DataSet();                 

                    ds = BLL.ReturnDataSet("select Plan_ID,Plan_Note,status,id,Rating,Admin_Reply,Reply_timestamp from Web_WeekSummary_list where Plan_ID='" + week + "' and staffid=" + id + " order by id desc", false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        code = Tools.Secrecy.Escape("1");

                        returnMessage = Tools.Secrecy.Escape(sum_reason);


                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                outdatalist = "{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Plan_Note"] + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) ? " <span class=\"sur_btn\">已完成</span>" : " <span class=\"link_btn\">未完成</span>" + ((Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) == false) ? "<em class=\"blfont\">已逾期</em>" : ""))) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\",\"id\":\"" + ds.Tables[0].Rows[i]["id"] + "\",\"Rating\":\"" + ds.Tables[0].Rows[i]["Rating"] + "\",\"Admin_Reply\":\"" + ds.Tables[0].Rows[i]["Admin_Reply"] + "\",\"Reply_timestamp\":\"" + ds.Tables[0].Rows[i]["Reply_timestamp"] + "\"}";
                            else
                                outdatalist += ",{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Plan_Note"] + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) ? " <span class=\"sur_btn\">已完成</span>" : " <span class=\"link_btn\">未完成</span>" + ((Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) == false) ? "<em class=\"blfont\">已逾期</em>" : ""))) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\",\"id\":\"" + ds.Tables[0].Rows[i]["id"] + "\",\"Rating\":\"" + ds.Tables[0].Rows[i]["Rating"] + "\",\"Admin_Reply\":\"" + ds.Tables[0].Rows[i]["Admin_Reply"] + "\",\"Reply_timestamp\":\"" + ds.Tables[0].Rows[i]["Reply_timestamp"] + "\"}";

                            score += Convert.ToInt32(ds.Tables[0].Rows[i]["Rating"]);
                            scorenum++;
                        }

                    }
                    else
                    {
                        ds = BLL.ReturnDataSet("select Plan_ID,Plan_Note,status,id,Rating,Admin_Reply,Reply_timestamp from Web_WeekSummary_list where staffid=" + id + " and rating=0 and Timestamp>'" + DateTime.Now.AddDays(-14) + "' order by id desc", false);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            code = Tools.Secrecy.Escape("1");
                            returnMessage = Tools.Secrecy.Escape(sum_reason);
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                if (i == 0)
                                {
                                    outdatalist = "{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Plan_Note"] + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) ? " <span class=\"sur_btn\">已完成</span>" : " <span class=\"link_btn\">未完成</span>" + ((Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) == false) ? "<em class=\"blfont\">已逾期</em>" : ""))) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\",\"id\":\"" + ds.Tables[0].Rows[i]["id"] + "\",\"Rating\":\"" + ds.Tables[0].Rows[i]["Rating"] + "\",\"Admin_Reply\":\"" + ds.Tables[0].Rows[i]["Admin_Reply"] + "\",\"Reply_timestamp\":\"" + ds.Tables[0].Rows[i]["Reply_timestamp"] + "\"}";
                                }
                                else
                                    outdatalist += ",{\"Plan_ID\":\"" + ds.Tables[0].Rows[i]["Plan_ID"] + "\",\"Plan_Note\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Plan_Note"] + (Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) ? " <span class=\"sur_btn\">已完成</span>" : " <span class=\"link_btn\">未完成</span>" + ((Convert.ToBoolean(ds.Tables[0].Rows[i]["status"]) == false) ? "<em class=\"blfont\">已逾期</em>" : ""))) + "\",\"status\":\"" + (Convert.ToBoolean(ds.Tables[0].Rows[i]["Status"]) ? "1" : "0") + "\",\"id\":\"" + ds.Tables[0].Rows[i]["id"] + "\",\"Rating\":\"" + ds.Tables[0].Rows[i]["Rating"] + "\",\"Admin_Reply\":\"" + ds.Tables[0].Rows[i]["Admin_Reply"] + "\",\"Reply_timestamp\":\"" + ds.Tables[0].Rows[i]["Reply_timestamp"] + "\"}";

                                score += Convert.ToInt32(ds.Tables[0].Rows[i]["Rating"]);
                                scorenum++;
                            }

                            reader = BLL.ReturnValue("select SUM_Reason,Weeklysales,Weeklyadvertisingexpenditure,Numberoforders,Numberofitemssold from Web_WeekSummary where plan_id='" + ds.Tables[0].Rows[0]["Plan_ID"] + "' and staffid=" + id, false);

                            if (reader.Read())
                            {
                                sum_reason = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                Weeklysales = reader.IsDBNull(0) ? 0 : reader.GetDecimal(1);
                                Weeklyadvertisingexpenditure = reader.IsDBNull(0) ? 0 : reader.GetDecimal(2);
                                Numberoforders = reader.IsDBNull(0) ? 0 : reader.GetDecimal(3);
                                Numberofitemssold = reader.IsDBNull(0) ? 0 : reader.GetDecimal(4);
                            }
                            reader.Close();
                            returnMessage = Tools.Secrecy.Escape(sum_reason.Replace("\r\n","</br>"));

                        }

                    }


                    Extdata = "{\"Weeklysales\":" + Weeklysales + ",\"Weeklyadvertisingexpenditure\":" + Weeklyadvertisingexpenditure + ",\"Numberoforders\":" + Numberoforders + ",\"Numberofitemssold\":" + Numberofitemssold + ",\"Score\":" + Math.Round(Convert.ToDecimal(score/(scorenum<1?1:scorenum)),2).ToString() + "}";

                }
                else if (r == "updaterating")
                {
                    string scorelist = context.Request["scorelist"].ToString();
                    JsonData jsonData = JsonMapper.ToObject(scorelist);

                    bool updateStaus = false;

                    if (jsonData.Count > 0)
                    {
                        for (int i = 0; i < jsonData.Count; i++)
                        {

                            updateStaus = BLL.ExecuteSqlFun("update Web_WeekSummary_list set Rating=" + jsonData[i]["score"].ToString() + ",Admin_Reply='" + jsonData[i]["bnote"].ToString() + "',Reply_timestamp='" + DateTime.Now + "' where ID=" + jsonData[i]["id"].ToString(), false);

                        }
                        code = Tools.Secrecy.Escape("1");
                    }



                }
                else if (r == "updateweekplanstatus")
                {
                    string week = context.Request["week"].ToString();
                    string userid = context.Request["userid"].ToString();

                    bool updateStaus = false;

                    updateStaus = BLL.ExecuteSqlFun("update Web_WeekPlan set Status=0 where Plan_ID='" + week + "' and StaffID=" + userid, false);

                     if (updateStaus)
                     {
                         code = Tools.Secrecy.Escape("1");
                     }



                }
                else if (r == "getdefault")
                {
                    string week = Tools.Tools.getSafeCode(context.Request["week"]);

                    int GobalTaskNumber = 0; //全部工作任务
                    int GobalRunTaskNumber = 0;//全部完成工作任务
                    int GobalNoRunTaskNumber = 0;//全部未完成工作会务

                    int StaffTaskNumber = 0; //个人工作任务
                    int StaffRunTaskNumber = 0;//个人完成工作任务
                    int StaffNoRunTaskNumber = 0;//个人未完成工作会务

                    DataSet NoRunds = new DataSet();//个人未完成工作任务条目
                    DataSet RunTaskStafflist = new DataSet(); //团队内已开始提交任务的人

                    DataSet TaskList = new DataSet();//团队人员进度计算数

                    string DepartmentID = Tools.Cookie.GetEncryptedCookieValue("departmentid"); //所属部门


                    //TaskList = BLL.ReturnDataSet("select StaffID,Status from Web_WeekPlan_list where StaffID in (select id from Web_staffes where DepartmentID=" + DepartmentID + ") and plan_id='" + week + "'", false);

                    TaskList = BLL.ReturnDataSet(string.Format(@"SELECT     dbo.Web_WeekPlan_list.StaffID, dbo.Web_WeekPlan_list.Status, dbo.Web_staffes.UserNmae
FROM         dbo.Web_WeekPlan_list INNER JOIN
                      dbo.Web_staffes ON dbo.Web_WeekPlan_list.StaffID = dbo.Web_staffes.ID
WHERE     (dbo.Web_WeekPlan_list.StaffID IN
                          (SELECT     ID
                            FROM          dbo.Web_staffes AS Web_staffes_1
                            WHERE      (DepartmentID = {0}))) AND (dbo.Web_WeekPlan_list.Plan_ID = '{1}')", DepartmentID, week), false);


                    if (TaskList.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < TaskList.Tables[0].Rows.Count; i++)
                        {
                            if (TaskList.Tables[0].Rows[i]["StaffID"].ToString() == staffID)
                            {
                                StaffTaskNumber++;//个人任务总量
                                if (Convert.ToBoolean(TaskList.Tables[0].Rows[i]["Status"]))
                                {
                                    StaffRunTaskNumber++;
                                }
                                else
                                {
                                    StaffNoRunTaskNumber++;
                                }
                            }

                            if (Convert.ToBoolean(TaskList.Tables[0].Rows[i]["Status"]))
                            {
                                GobalRunTaskNumber++;
                            }
                            else
                            {
                                GobalNoRunTaskNumber++;
                            }
                            GobalTaskNumber++;


                            ////////////////////////////











                        }
                    }



                    var statistics = new Dictionary<int, StaffStatistics>();

                    // 遍历DataTable进行统计
                    foreach (DataRow row in TaskList.Tables[0].Rows)
                    {
                        int staffId = Convert.ToInt32(row["StaffID"]);
                        int status = Convert.ToInt32(row["Status"]);
                        string username = row["UserNmae"].ToString();

                        // 如果字典中不存在该员工ID，添加新记录
                        if (!statistics.ContainsKey(staffId))
                        {
                            statistics[staffId] = new StaffStatistics { UserName = "", TotalTasks = 0, CompletedTasks = 0 };
                        }

                        // 更新统计数据
                        statistics[staffId].TotalTasks++;
                        statistics[staffId].UserName = username;
                        if (status == 1)
                        {
                            statistics[staffId].CompletedTasks++;
                        }
                    }

                    string teamUserPross = string.Empty;
                    // 输出统计结果
                    foreach (var staffId in statistics.Keys.OrderBy(id => id))
                    {
                        var stats = statistics[staffId];
                        //HttpContext.Current.Response.Write(string.Format("员工ID: {0}, 总任务数: {1}, 已完成任务数: {2}",
                        //    stats.UserName, stats.TotalTasks, stats.CompletedTasks));
                        teamUserPross += ",{\"staffid\":\"" + stats.UserName + "\",\"TotalTasks\":\"" + stats.TotalTasks + "\",\"CompletedTasks\":\"" + stats.CompletedTasks + "\"}";

                    }




                    NoRunds = BLL.ReturnDataSet("select ID,Plan_Note from Web_WeekPlan_list where StaffID=" + staffID + " and Status=0  and Plan_ID='" + week + "'", false); //个人待办事项
                    RunTaskStafflist = BLL.ReturnDataSet("select id,usernmae from Web_staffes where ID in (select StaffID from Web_WeekPlan_list where Status=1 and Plan_ID='" + week + "') and leveltype=1", false); //已提交人员

                    string waitdolist = string.Empty;
                    if (NoRunds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < NoRunds.Tables[0].Rows.Count; i++)
                        {

                            if (i == 0)
                                waitdolist = "{\"id\":\"" + NoRunds.Tables[0].Rows[i]["id"] + "\",\"Plan_Note\":\"" + NoRunds.Tables[0].Rows[i]["Plan_Note"] + "\"}";
                            else
                                waitdolist += ",{\"id\":\"" + NoRunds.Tables[0].Rows[i]["id"] + "\",\"Plan_Note\":\"" + NoRunds.Tables[0].Rows[i]["Plan_Note"] + "\"}";

                        }
                    }
                    string RunTaskStafflistStr = string.Empty;
                    if (RunTaskStafflist.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < RunTaskStafflist.Tables[0].Rows.Count; i++)
                        {

                            if (i == 0)
                                RunTaskStafflistStr = "{\"id\":\"" + RunTaskStafflist.Tables[0].Rows[i]["id"] + "\",\"usernmae\":\"" + RunTaskStafflist.Tables[0].Rows[i]["usernmae"] + "\"}";
                            else
                                RunTaskStafflistStr += ",{\"id\":\"" + RunTaskStafflist.Tables[0].Rows[i]["id"] + "\",\"usernmae\":\"" + RunTaskStafflist.Tables[0].Rows[i]["usernmae"] + "\"}";

                        }
                    }






                    code = Tools.Secrecy.Escape("1");


                    outdatalist = "{\"GobalTaskNumber\":\"" + GobalTaskNumber + "\",\"GobalRunTaskNumber\":\"" + GobalRunTaskNumber + "\",\"GobalNoRunTaskNumber\":\"" + GobalNoRunTaskNumber
                        + "\",\"StaffTaskNumber\":\"" + StaffTaskNumber + "\",\"StaffRunTaskNumber\":\"" + StaffRunTaskNumber + "\",\"StaffNoRunTaskNumber\":\"" + StaffNoRunTaskNumber
                        + "\",\"waitdolist\":[" + waitdolist + "],\"RunTaskStafflist\":[" + RunTaskStafflistStr + "],\"teamUserPross\":[" + teamUserPross.Substring(1) + "]}";











                }


























            }

            sb.Remove(0, sb.Length);
            sb.Append("{\"code\":\"" + code + "\",\"message\":\"" + returnMessage + "\",\"Datalist\":[" + outdatalist + "],\"Extdata\":" + Extdata + " }");
            context.Response.Write(sb.ToString());
        }




























        public class StaffStatistics
        {
            public string UserName { get; set; }
            public int TotalTasks { get; set; }
            public int CompletedTasks { get; set; }
        }





        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            DirectoryInfo[] dirs = dir.GetDirectories();


            // If the destination directory does not exist, create it.
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(targetDir, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to the new location.
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(targetDir, subdir.Name);
                CopyDirectory(subdir.FullName, tempPath);
            }
        }

        public static string GetReply(string weekid,DataSet ds)
        {
            string returnValue = string.Empty;

            string staffID = "-9999";

            if (Tools.Cookie.CookieCheck("BGstaffID"))
                staffID = Tools.Cookie.GetEncryptedCookieValue("BGstaffID");

            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (weekid.Trim().ToLower() == ds.Tables[0].Rows[i]["PLAN_ID"].ToString().Trim().ToLower() && ds.Tables[0].Rows[i]["staffID"].ToString().Trim().ToLower() == staffID)
                    {
                        returnValue=Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["ADMIN_REPLIES"].ToString());
                    }
                }
            }

            return returnValue;
        }

  
        //判断时间
        public static int getCtime(DateTime startTime, DateTime endTime)
        {
            DateTime newtime = DateTime.Now;
            TimeSpan Span = endTime.Subtract(startTime);
            if (DateTime.Compare(newtime, startTime) > 0)
            {
                Span = endTime.Subtract(newtime);
            }
            return Span.Days * 24 * 60 * 60 + Span.Hours * 60 * 60 + Span.Minutes * 60 + Span.Seconds;
        }

               

        
    }
}
