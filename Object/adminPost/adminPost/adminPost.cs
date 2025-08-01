using System;
using System.Web;
using System.Web.SessionState;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Config;
using Tools;
using DateEntity;
using System.IO;
using LitJson;

namespace adminPost
{
    public class AdminPost
    {
        public static void Repost(HttpContext context)
        {
            string dayimg = Secrecy.Escape("对不起,你的操作不合法，请联系: 管理员");
            StringBuilder sb = new StringBuilder();
            if (context.Request["r"] != null)
            {
               
                string r = Tools.Tools.getSafeCode(context.Request["r"]).ToLower();
                if (r == "checkadminuser")
                {
                   

                    GetConfigInfo configinfo = new GetConfigInfo();
                    bool iplock = Convert.ToBoolean(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "IPlock")));
                    bool wronglock = Convert.ToBoolean(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "wronglock")));
                    int wronglocknumber = Convert.ToInt32(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "wronglocknumber")));

                    bool templogin = true;
                    string useremail = Tools.Tools.getSafeCode(context.Request["name"]);
                    string password = Tools.Tools.getSafeCode(context.Request["pass"]);
                    SqlDataReader reader;

                  

                    if (iplock)
                    {
                        
                        reader = BLL.ReturnValue("select ip from Web_SafeIP where ip='" + context.Request.UserHostAddress.ToString() + "'", false);
                        if (!reader.Read())
                        {
                            dayimg = Tools.Secrecy.Escape("0|对不起,您不在安全IP.");
                            templogin = false;
                        }
                        else
                        {
                            templogin = true;
                        }
                        reader.Close();
                    }

                  

                    if (templogin)
                    {
                        reader = BLL.ReturnValue("select LoginName,LoginPassword,DepartmentID,WrongNum,DepartmenttypeName,TypeName,TypeLevel,Status,IsSafeIP,ID from Web_Staffes where LoginName='" + useremail + "'", false);
                        if (reader.Read())
                        {
                           

                            if (reader.GetString(1).ToLower() == password.ToLower())
                            {
                                if (reader.GetBoolean(7) == true)
                                {
                                    if (wronglock == true && reader.GetInt32(3) >= wronglocknumber)
                                        dayimg = Tools.Secrecy.Escape("0|对不起,你错误登陆次数过多,帐号已暂停,请联系主管.");
                                    else
                                    {
                                        dayimg = Tools.Secrecy.Escape("1|OK.");
                                        Tools.Cookie.SetEncryptedCookie("BGstaffID", reader.GetInt32(9).ToString(), DateTime.Now.AddDays(7));
                                        Tools.Cookie.SetEncryptedCookie("BGuserName", reader.GetString(0), DateTime.Now.AddDays(7));
                                        Tools.Cookie.SetEncryptedCookie("BGSerCyCookie", reader.GetString(0) + "@|@" + reader.GetInt32(2).ToString() + "@|@" + reader.GetInt32(6).ToString(), DateTime.Now.AddDays(7));
                                        Tools.Cookie.SetEncryptedCookie("BGDepartmenttypeName", reader.GetString(4), DateTime.Now.AddDays(7));
                                        Tools.Cookie.SetEncryptedCookie("BGTypeName", reader.GetString(5), DateTime.Now.AddDays(7));
                                        Tools.Cookie.SetEncryptedCookie("BGTypeLevel", reader.GetInt32(6).ToString(), DateTime.Now.AddDays(7));
                                    }
                                }
                                else
                                {
                                    dayimg = Tools.Secrecy.Escape("0|对不起你的帐号被锁定.");
                                }
                            }
                            else
                            {
                                dayimg = Tools.Secrecy.Escape("0|密码错误.");
                            }
                        }
                        else
                        {
                            dayimg = Tools.Secrecy.Escape("0|管理员不存在.");
                        }
                        reader.Close();

                    }


                }else if(r=="getstafflist")
                {
                    dayimg = Tools.Secrecy.Escape("0|err.");

                    string stafflist = Tools.Tools.getSafeCode(context.Request["staff"]);  // ;-1;1;

                    string outdatalist = string.Empty;

                    DataSet ds = new DataSet();


                    ds = BLL.ReturnDataSet("select ID,LoginName from Web_Staffes where loginname<>'systemadmin' and id not in(" + stafflist.Substring(1).Replace(";", ",") + "-1) order by id desc", false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        dayimg = "1";

                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                outdatalist += "" + ds.Tables[0].Rows[i]["id"].ToString() + "-" + ds.Tables[0].Rows[i]["LoginName"].ToString() + "";
                            else
                                outdatalist += "," + ds.Tables[0].Rows[i]["id"].ToString() + "-" + ds.Tables[0].Rows[i]["LoginName"].ToString() + "";

                        }

                    }
                    dayimg = Tools.Secrecy.Escape("1|" + outdatalist);
                    
                    //sb.Remove(0, sb.Length);
                    //sb.Append("{\"Results\":\"" + dayimg + "\",Datalist:[" + outdatalist + "]}");
                    //context.Response.Write(sb.ToString());
                    //context.Response.End();




                    
                }

















            }

            sb.Remove(0, sb.Length);
            sb.Append("{\"Results\":\"" + dayimg + "\"}");
            context.Response.Write(sb.ToString());
        }


        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="filePath"></param>
        public static void SaveBase64Image(string base64String, string filePath)
        {
             // 移除Base64字符串的前缀，如："data:image/png;base64,"
            base64String = base64String.Replace("data:image/png;base64,", "").Replace("data:image/jpeg;base64,", "");

           
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (FileStream imageFile = new FileStream(filePath, FileMode.Create))
            {
                imageFile.Write(imageBytes, 0, imageBytes.Length);
                imageFile.Flush();
            }

           

        }




        public static string GetAllFolder(string path)
        {
            string folder_Names = "";
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (DirectoryInfo subdir in dir.GetDirectories())
                folder_Names += subdir.FullName + ",";
            return folder_Names;
        }


    }
}
