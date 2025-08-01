using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Collections;
using com.paypal.sdk.util;
using Config;
using Tools;
using DateEntity;
using System.Net;
using System.Collections.Generic;
using ShoppingCart;
using System.Linq;
using PaypalExpress;


namespace Post
{
    public class Post
    {
        public static string currency = "USD";
        public static string curformat = "$";
        public static string currencyrate = "1";

        public static void Repost(HttpContext context)
        {
            string dayimg = Secrecy.Escape("Sorry, you have no access to our data. Please contact: paul@liuhaimin.com");
            StringBuilder sb = new StringBuilder();

            if (Tools.Cookie.CookieCheck("currency"))
            {
                currency = Tools.Cookie.GetEncryptedCookieValue("currency");
                curformat = Tools.Cookie.GetEncryptedCookieValue("curformat");
                currencyrate = (Convert.ToDecimal(Tools.Cookie.GetEncryptedCookieValue("currencyrate")) / 100).ToString();
            }

            if (context.Request["r"] != null)
            {
                dayimg = Tools.Secrecy.Escape("0");

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

                    SqlDataReader reader = BLL.ReturnValue("select LoginName,PhoneNumber,LoginPassword,Status,ID,UserType from Web_User where LoginName='" + useremail + "'", false);
                    if (reader.Read())
                    {
                        if (reader.GetBoolean(3) == false)
                        {
                            dayimg = Tools.Secrecy.Escape("0|对不起,你帐号被限请联系网站管理员.");
                        }
                        else if (password == reader.GetString(2))
                        {
                            Tools.Cookie.SetEncryptedCookie("userid", reader.GetInt32(4).ToString(), DateTime.Now.AddDays(7));
                            Tools.Cookie.SetEncryptedCookie("useremail", useremail, DateTime.Now.AddDays(7));
                            Tools.Cookie.SetEncryptedCookie("checkvalue", useremail + reader.GetString(2) + reader.GetBoolean(3).ToString(), DateTime.Now.AddDays(7));
                            Tools.Cookie.SetEncryptedCookie("usertype", reader.GetInt32(5).ToString(), DateTime.Now.AddDays(7));
                            if (reader.GetInt32(5) == 1)
                                dayimg = Tools.Secrecy.Escape("1|buyer");
                            else
                                dayimg = Tools.Secrecy.Escape("1|store");
                        }
                        else
                        {
                            dayimg = Tools.Secrecy.Escape("0|密码错误.");
                        }
                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("2|你还没有注册,请注册后使用.");
                    }
                    reader.Close();
                }
                else if (r == "qregester")
                {
                    string user = context.Request["username"].Replace(" ", "jiahao").Replace(".aspx", ".do").Replace("jiahao", "+");
                    string newpassword = Tools.Tools.getSafeCode(context.Request["newpassword"].Length < 1 ? "123456" : context.Request["newpassword"]);
                    string usernick = Tools.Tools.getSafeCode(context.Request["usernick"]);



                    bool updatestatus = false;

                    if (user.Length > 0 && user.IndexOf("focalecig") < 0 && user.IndexOf("wallbuys") < 0 && user.IndexOf("ecigbuy") < 0 && user.IndexOf("gearbast") < 0)
                    {
                        string newCode = Tools.Random.getCode();

                        SqlDataReader reader = BLL.ReturnValue("select email from ACT_Accounts where email='" + user + "'", false);
                        if (reader.Read())
                        {
                            dayimg = Tools.Secrecy.Escape("0|Account already exists, please change.");
                        }
                        else
                        {
                            string tempPcode = "";
                            tempPcode = Tools.Tools.egetcode(Tools.Tools.Str_char(10, true));
                            GetConfigInfo configinfo = new GetConfigInfo();
                            string website = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getEmailconfigfile, "website"));
                            updatestatus = BLL.ExecuteSqlFun("insert ACT_Accounts(EMail,Nick,Password,FirstName,LastName,PaypalEmail,VerificationCode,Affcode) values('" + user + "','" + usernick + "','" + newpassword + "','','','" + user + "','" + newCode + "','" + tempPcode + "');insert Web_Point(email,Point,CostPoint,TotalPoint) values('" + user + "',0,0,0);", false);

                            decimal codevalue = 3;
                            tempPcode = Tools.Tools.egetcode(Tools.Tools.Str_char(13, true));

                            SqlConnection sqlConn = DAL.localwcon();
                            SqlCommand cmdDB = new SqlCommand(@"INSERT Web_Coupon(CouponNumber,Coupontype,LostPrice,CouponValue,lostPrice2,CouponValue2,CouponEmail,Ispaid,Creattimeamp,Startimeamp,Endtimeamp,IsUSD,SupportCategory,SupportSku,CouponDesign,StoreName) values(@CouponNumber,@Coupontype,@LostPrice,@CouponValue,@lostPrice2,@CouponValue2,@CouponEmail,@Ispaid,@Creattimeamp,@Startimeamp,@Endtimeamp,@IsUSD,@SupportType,@SupportSku,@CouponDesign,@StoreName)", sqlConn);
                            cmdDB.Parameters.Add("@CouponNumber", SqlDbType.VarChar).Value = tempPcode;
                            cmdDB.Parameters.Add("@Coupontype", SqlDbType.Int).Value = 5;
                            cmdDB.Parameters.Add("@LostPrice", SqlDbType.Decimal).Value = 10;
                            cmdDB.Parameters.Add("@CouponValue", SqlDbType.Decimal).Value = codevalue;
                            cmdDB.Parameters.Add("@lostPrice2", SqlDbType.Decimal).Value = 100000;
                            cmdDB.Parameters.Add("@CouponValue2", SqlDbType.Decimal).Value = 0;

                            cmdDB.Parameters.Add("@CouponEmail", SqlDbType.VarChar).Value = user;
                            cmdDB.Parameters.Add("@Ispaid", SqlDbType.Bit).Value = false;
                            cmdDB.Parameters.Add("@Creattimeamp", SqlDbType.DateTime).Value = DateTime.Now;
                            cmdDB.Parameters.Add("@Startimeamp", SqlDbType.DateTime).Value = DateTime.Now;
                            cmdDB.Parameters.Add("@Endtimeamp", SqlDbType.DateTime).Value = DateTime.Now.AddYears(2);
                            cmdDB.Parameters.Add("@IsUSD", SqlDbType.Bit).Value = true;

                            cmdDB.Parameters.Add("@SupportType", SqlDbType.VarChar).Value = "";
                            cmdDB.Parameters.Add("@SupportSku", SqlDbType.VarChar).Value = "";
                            cmdDB.Parameters.Add("@CouponDesign", SqlDbType.VarChar).Value = "for new account";
                            cmdDB.Parameters.Add("@StoreName", SqlDbType.VarChar).Value = website;

                            cmdDB.Connection.Open();
                            cmdDB.ExecuteNonQuery();
                            cmdDB.Connection.Close();



                            if (updatestatus)
                            {
                                Tools.Cookie.SetEncryptedCookie("useremail", user);
                                Tools.Cookie.SetEncryptedCookie("nick", usernick);
                                Tools.Cookie.SetEncryptedCookie("isaff", "0");
                                Tools.Cookie.SetEncryptedCookie("isdropshipp", "False");

                                Tools.Cookie.SetEncryptedCookie("payemail", user);
                                Tools.Cookie.SetEncryptedCookie("qpassword", newpassword);
                                Tools.Cookie.SetEncryptedCookie("meberlevel", "usu");
                                Tools.Cookie.SetEncryptedCookie("point", "0");
                                Tools.Cookie.SetEncryptedCookie("discount", "No Discount");
                                Tools.Cookie.SetEncryptedCookie("logindate", DateTime.Now.ToString());
                                Tools.Cookie.SetEncryptedCookie("lastlogintime", DateTime.Now.ToString());

                                //try
                                //{
                                //    Email.EmailSend.newaccount(user, null, user.Substring(0, user.Length - user.IndexOf("@")), newpassword, "New account from " + website);
                                //}
                                //catch { }
                                dayimg = Tools.Secrecy.Escape("1|ok");
                            }
                            else
                            {
                                dayimg = Tools.Secrecy.Escape("0|err");
                            }
                        }
                        reader.Close();
                    }

                }
                else if (r == "setshippingstyle")
                {
                    string shippingstyle = context.Request["shippingstyle"];
                    string cid = context.Request["cid"] != null ? context.Request["cid"] : "0";
                    string styleflag = context.Request["styleflag"];
                    string warestock = "0";
                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    string isp = "1";

                    string shippingstylestr = PageSource.PageSource.GetPublickSocur("shippingstyle.ptshop").Trim();
                    string[] shippingstylelist = shippingstylestr.Split('\r');
                    for (int i = 1; i < shippingstylelist.Length; i++)
                    {
                        if (shippingstyle == shippingstylelist[i].ToString().Trim().Split('|')[12].ToString().Trim().ToLower())
                        {
                            warestock = shippingstylelist[i].ToString().Trim().Split('|')[15].ToString().Trim();
                            cart.shipping.ShippingMethod = shippingstylelist[i].ToString().Trim().Split('|')[1].ToString();
                            isp = shippingstylelist[i].ToString().Trim().Split('|')[19].ToString();
                        }
                    }

                    dayimg = Tools.Secrecy.Escape("0|err");
                    DataSet ds = new DataSet();
                    GetConfigInfo configinfo = new GetConfigInfo();

                    decimal totalWeight = cart.GetWeighttotal(warestock) + 10;
                    decimal EmsDiscount = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "emsdiscount")));
                    decimal DhlDiscount = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "dhldiscount")));

                    decimal limitFreeshipping = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "limitetrackfee")));
                    decimal outshippingfee;
                    decimal HokongPost = 0;
                    decimal weightLever = Math.Ceiling(totalWeight / 500M) - 1;
                    decimal emsshippingFee = 0M, dhlshippingFee = 0M;
                    decimal rateprice = 6.6M;
                    cart.shipping.CountryID = cid;

                    string ExpressFee = cart.shipping.getExpressFee(totalWeight);

                    if (styleflag == "k")
                    {
                        if (Convert.ToInt32(cid) > 0)
                        {
                            ds = BLL.ReturnDataSet("select S.EMSFirstPrice emsFirstPrice,S.EMSNextPrice emsNextPrice,S.DHLFirstPrice dhlFirstPrice,S.DHLNextPrice dhlNextPrice,R.price from Web_ShippingCountry S,Web_Main_Rate R where S.ID=" + cid + " and R.Verified=1", false);
                            emsshippingFee = (((Convert.ToDecimal(ds.Tables[0].Rows[0]["emsFirstPrice"]) + Convert.ToDecimal(ds.Tables[0].Rows[0]["emsNextPrice"]) * weightLever) * EmsDiscount - HokongPost) * 1.06M) / Convert.ToDecimal(ds.Tables[0].Rows[0]["price"]);
                            dhlshippingFee = (((Convert.ToDecimal(ds.Tables[0].Rows[0]["dhlFirstPrice"]) + Convert.ToDecimal(ds.Tables[0].Rows[0]["dhlNextPrice"]) * weightLever) * DhlDiscount - HokongPost) * 1.06M) / Convert.ToDecimal(ds.Tables[0].Rows[0]["price"]);
                        }
                        else
                        {
                            emsshippingFee = Convert.ToDecimal(ExpressFee.Split('|')[0]);
                            dhlshippingFee = Convert.ToDecimal(ExpressFee.Split('|')[1]);
                        }

                        if (emsshippingFee < 0)
                            emsshippingFee = 0;
                        if (dhlshippingFee < 0)
                            dhlshippingFee = 0;

                        cart.shipping.ShippingFee = (emsshippingFee == 0 ? dhlshippingFee : emsshippingFee);
                    }

                    cart.shipping.shippingstyle = styleflag;

                    configinfo = new Config.GetConfigInfo();

                    for (int i = 1; i < shippingstylelist.Length; i++)
                    {
                        if (shippingstyle == shippingstylelist[i].ToString().Trim().Split('|')[12].ToString().Trim().ToLower())
                        {

                            //当为平邮时   

                            if (shippingstylelist[i].ToString().Trim().Split('|')[5] == "1")//支 持 免 运 费  
                            {
                                if (shippingstylelist[i].ToString().Trim().Split('|')[21] == "0")//运 费 越 重 越 贵 
                                {
                                    //在 免 费 情 况 下 判 断 免 费 起 始 重 量 ， 在 不 能 免 费 时 要 收 费 
                                    if (cart.GetSubtotal(warestock) >= Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[22]))
                                    {
                                        if (totalWeight >= Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[8])) //当 免 运 费 时 重 量 超 了 非 免 运 费 时 要 收 费 
                                        {
                                            outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : ((totalWeight / 1000) * Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[2]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[3]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[4])) / rateprice;

                                        }
                                        else//当 免 运 费 时 重 量 没 有 超  非 免 运 费 重 量 时 为 免 费 
                                        {
                                            outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : 0;
                                        }
                                    }
                                    else //在 低 于 免 运 费 金 额 时 要 收 费 的 
                                    {
                                        outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : ((totalWeight / 1000) * Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[2]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[3]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[4])) / rateprice;
                                    }
                                }
                                else//运 费 越 重 越 省 钱 
                                {
                                    //在 免 费 情 况 下 判 断 免 费 起 始 重 量 ， 在 不 能 免 费 时 要 收 费 
                                    if (cart.GetSubtotal(warestock) >= Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[22]))
                                    {
                                        if (totalWeight <= Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[8])) //当 免 运 费 时 重 量 小 于 非 免 运 费 时 要 收 费 
                                        {
                                            outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : ((totalWeight / 1000) * Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[2]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[3]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[4])) / rateprice;
                                        }
                                        else//当 免 运 费 时 重 量 超 了 非 免 运 费 重 量 时 为 免 费 
                                        {
                                            outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : 0;
                                        }
                                    }
                                    else //在 低 于 免 运 费 金 额 时 要 收 费 的 
                                    {
                                        outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : ((totalWeight / 1000) * Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[2]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[3]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[4])) / rateprice;
                                    }
                                }

                            }
                            else//不 支 持 免 运 费 
                            {
                                outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : ((totalWeight / 1000) * Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[2]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[3]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[4])) / rateprice;
                            }

                            cart.shipping.ShippingFee = outshippingfee;

                            if (shippingstylelist[i].ToString().Trim().Split('|')[11].ToString() == "0")
                            {
                                cart.Remove("-8888");
                            }

                        }


                    }

                    dayimg = Tools.Secrecy.Escape("1|" + cart.GetSubtotal(warestock).ToString("C") + "|" + cart.shipping.ShippingFee.ToString("C") + "|" + cart.GetSubDiscount(warestock).ToString("C") + "|" + cart.GetTotal(warestock).ToString("C") + "|" + shippingstyle + "|" + cart.GetShippingTotal().ToString("F") + "|" + cart.shipping.ShippingMethod + "|" + Math.Round((cart.GetTotal(warestock) / 10), 0).ToString() + "|" + isp + "|" + cart.shipping.getShippingStyle(warestock));

                }
                else if (r == "setgroupbuyshippingfee")
                {
                    string shippingstyle = context.Request["shippingstyle"];
                    string styleflag = context.Request["styleflag"];
                    string warestock = "0";
                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    string isp = "1";

                    string shippingstylestr = PageSource.PageSource.GetPublickSocur("shippingstyle.ptshop").Trim();
                    string[] shippingstylelist = shippingstylestr.Split('\r');
                    for (int i = 1; i < shippingstylelist.Length; i++)
                    {
                        if (shippingstyle == shippingstylelist[i].ToString().Trim().Split('|')[12].ToString().Trim().ToLower())
                        {
                            warestock = shippingstylelist[i].ToString().Trim().Split('|')[15].ToString().Trim();
                            cart.shipping.ShippingMethod = shippingstylelist[i].ToString().Trim().Split('|')[1].ToString();
                            isp = shippingstylelist[i].ToString().Trim().Split('|')[19].ToString();
                        }
                    }

                    dayimg = Tools.Secrecy.Escape("0|err");
                    DataSet ds = new DataSet();
                    GetConfigInfo configinfo = new GetConfigInfo();

                    decimal totalWeight = cart.GetWeighttotal(warestock) + 10;
                    decimal EmsDiscount = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "emsdiscount")));
                    decimal DhlDiscount = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "dhldiscount")));

                    decimal limitFreeshipping = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "limitetrackfee")));
                    decimal outshippingfee;
                    decimal HokongPost = 0;
                    decimal weightLever = Math.Ceiling(totalWeight / 500M) - 1;
                    decimal emsshippingFee = 0M, dhlshippingFee = 0M;
                    decimal rateprice = 6.6M;
                    cart.shipping.CountryID = "0";

                    string ExpressFee = cart.shipping.getExpressFee(totalWeight);

                    if (styleflag == "k")
                    {
                        if (Convert.ToInt32(cart.shipping.CountryID) > 0)
                        {
                            ds = BLL.ReturnDataSet("select S.EMSFirstPrice emsFirstPrice,S.EMSNextPrice emsNextPrice,S.DHLFirstPrice dhlFirstPrice,S.DHLNextPrice dhlNextPrice,R.price from Web_ShippingCountry S,Web_Main_Rate R where S.ID=0 and R.Verified=1", false);
                            emsshippingFee = (((Convert.ToDecimal(ds.Tables[0].Rows[0]["emsFirstPrice"]) + Convert.ToDecimal(ds.Tables[0].Rows[0]["emsNextPrice"]) * weightLever) * EmsDiscount - HokongPost) * 1.06M) / Convert.ToDecimal(ds.Tables[0].Rows[0]["price"]);
                            dhlshippingFee = (((Convert.ToDecimal(ds.Tables[0].Rows[0]["dhlFirstPrice"]) + Convert.ToDecimal(ds.Tables[0].Rows[0]["dhlNextPrice"]) * weightLever) * DhlDiscount - HokongPost) * 1.06M) / Convert.ToDecimal(ds.Tables[0].Rows[0]["price"]);
                        }
                        else
                        {
                            emsshippingFee = Convert.ToDecimal(ExpressFee.Split('|')[0]);
                            dhlshippingFee = Convert.ToDecimal(ExpressFee.Split('|')[1]);
                        }

                        if (emsshippingFee < 0)
                            emsshippingFee = 0;
                        if (dhlshippingFee < 0)
                            dhlshippingFee = 0;

                        cart.shipping.ShippingFee = (emsshippingFee == 0 ? dhlshippingFee : emsshippingFee);
                    }

                    cart.shipping.shippingstyle = styleflag;

                    configinfo = new Config.GetConfigInfo();

                    for (int i = 1; i < shippingstylelist.Length; i++)
                    {
                        if (shippingstyle == shippingstylelist[i].ToString().Trim().Split('|')[12].ToString().Trim().ToLower())
                        {

                            //当为平邮时   

                            if (shippingstylelist[i].ToString().Trim().Split('|')[5] == "1")//支 持 免 运 费  
                            {
                                if (shippingstylelist[i].ToString().Trim().Split('|')[21] == "0")//运 费 越 重 越 贵 
                                {
                                    //在 免 费 情 况 下 判 断 免 费 起 始 重 量 ， 在 不 能 免 费 时 要 收 费 
                                    if (cart.GetSubtotal(warestock) >= Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[22]))
                                    {
                                        if (totalWeight >= Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[8])) //当 免 运 费 时 重 量 超 了 非 免 运 费 时 要 收 费 
                                        {
                                            outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : ((totalWeight / 1000) * Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[2]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[3]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[4])) / rateprice;

                                        }
                                        else//当 免 运 费 时 重 量 没 有 超  非 免 运 费 重 量 时 为 免 费 
                                        {
                                            outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : 0;
                                        }
                                    }
                                    else //在 低 于 免 运 费 金 额 时 要 收 费 的 
                                    {
                                        outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : ((totalWeight / 1000) * Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[2]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[3]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[4])) / rateprice;
                                    }
                                }
                                else//运 费 越 重 越 省 钱 
                                {
                                    //在 免 费 情 况 下 判 断 免 费 起 始 重 量 ， 在 不 能 免 费 时 要 收 费 
                                    if (cart.GetSubtotal(warestock) >= Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[22]))
                                    {
                                        if (totalWeight <= Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[8])) //当 免 运 费 时 重 量 小 于 非 免 运 费 时 要 收 费 
                                        {
                                            outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : ((totalWeight / 1000) * Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[2]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[3]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[4])) / rateprice;
                                        }
                                        else//当 免 运 费 时 重 量 超 了 非 免 运 费 重 量 时 为 免 费 
                                        {
                                            outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : 0;
                                        }
                                    }
                                    else //在 低 于 免 运 费 金 额 时 要 收 费 的 
                                    {
                                        outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : ((totalWeight / 1000) * Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[2]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[3]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[4])) / rateprice;
                                    }
                                }

                            }
                            else//不 支 持 免 运 费 
                            {
                                outshippingfee = shippingstylelist[i].ToString().Trim().Split('|')[11] == "0" ? (shippingstylelist[i].ToString().Trim().Split('|')[10].IndexOf("ems") > -1 ? emsshippingFee : dhlshippingFee) : ((totalWeight / 1000) * Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[2]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[3]) + Convert.ToDecimal(shippingstylelist[i].ToString().Trim().Split('|')[4])) / rateprice;
                            }

                            outshippingfee = outshippingfee * 0.96M;

                            cart.shipping.ShippingFee = outshippingfee;

                            if (shippingstylelist[i].ToString().Trim().Split('|')[11].ToString() == "0")
                            {
                                cart.Remove("-8888");
                            }

                        }


                    }

                    dayimg = Tools.Secrecy.Escape("1|" + cart.GetSubtotal(warestock).ToString("C") + "|" + cart.shipping.ShippingFee.ToString("C") + "|" + cart.GetSubDiscount(warestock).ToString("C") + "|" + cart.GetTotal(warestock).ToString("C") + "|" + shippingstyle + "|" + cart.GetShippingTotal().ToString("F") + "|" + cart.shipping.ShippingMethod + "|" + Math.Round((cart.GetTotal(warestock) / 10), 0).ToString() + "|" + isp + "|" + cart.shipping.getShippingStyle(warestock));

                }
                else if (r == "getshippingstatus")
                {
                    string cid = context.Request["cid"].Trim();
                    string cidname = context.Request["cidname"].Trim();
                    string warestock = context.Request["warestock"].Trim();
                    dayimg = Tools.Secrecy.Escape("0|err");
                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    cart.shipping.CountryID = cid;
                    cart.shipping.CountryName = cidname;

                    dayimg = Tools.Secrecy.Escape("1|" + cart.shipping.getShippingStylebyCountry(warestock, cid, cidname) + "|" + (cart.GetTotal(warestock)).ToString("C")) + "|" + cart.shipping.ShippingFee.ToString("C");
                }
                else if (r == "checkaffstatus")
                {
                    dayimg = Tools.Secrecy.Escape("0|err");

                    string useremail = "SGIEWRLTKERGITER843";
                    if (Tools.Cookie.CookieCheck("useremail"))
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    string userid = "0";

                    SqlDataReader reader = BLL.ReturnValue("select id,AffCode from ACT_Accounts where email='" + useremail + "'", false);
                    if (reader.Read())
                    {
                        userid = reader.GetInt32(0).ToString();
                        dayimg = Tools.Secrecy.Escape("1|" + userid + "|" + reader.GetString(1));
                    }
                    reader.Close();


                }
                else if (r == "checkbarining")
                {
                    dayimg = Tools.Secrecy.Escape("0|err");

                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]),shortname=string.Empty;
                    bool isbraining = false;
                    bool isupdate = true;
                    bool isinsertdeals = true;
                    DateTime endtime = DateTime.Now;

                    int bid = 0,ustock=0;
                    decimal bprice = 0,oldprice=0;


                    string useremail = "SGIEWRLTKERGITER843";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");

                        SqlDataReader reader = BLL.ReturnValue("select Stock,BarginingPrice,sku,id,OldPrice,EndTime,shortName from Bargining_Products where sku=" + sku, false);
                        if (reader.Read())
                        {
                            if (reader.GetInt32(0) > 0)
                            {
                                isbraining = true;
                                bprice = reader.GetDecimal(1);
                                oldprice = reader.GetDecimal(4);
                                bid = reader.GetInt32(3);
                                ustock = reader.GetInt32(0);
                                endtime = reader.GetDateTime(5);
                                shortname = Tools.PostStr.potstr(reader.GetString(6));
                            }
                            else
                            {
                                dayimg = Tools.Secrecy.Escape("0|Sold out");
                            }
                        }
                        reader.Close();

                      
                        if (isbraining)
                        {
                            reader = BLL.ReturnValue("select max(id) from Deals_Bargining_Products where Email='" + useremail + "' and sku=" + sku, false);
                            if (reader.Read())
                            {
                                if (reader.IsDBNull(0) == false)
                                    isinsertdeals = false;
                            }
                            reader.Close();


                            if (isinsertdeals)
                            {
                                isupdate = BLL.ExecuteSqlFun("insert Deals_Bargining_Products(SKU,Email,OldPrice,EndPrice,NowPrice,EndTime) values(" + sku + ",'" + useremail + "'," + oldprice + "," + bprice + "," + oldprice + ",'" + endtime + "')", false);

                                isupdate = BLL.ExecuteSqlFun("update Bargining_Products set Stock=Stock-1 where sku=" + sku, false);

                                if (isupdate)
                                {
                                    reader = BLL.ReturnValue("select max(id) from Deals_Bargining_Products where Email='" + useremail + "' and sku=" + sku, false);
                                    if (reader.Read())
                                    {
                                        dayimg = Tools.Secrecy.Escape("1|" + reader.GetInt32(0) + "|" + Tools.Secrecy.Escape(shortname) + "|" + sku);
                                    }
                                    reader.Close();

                                }
                            }
                            else
                            {
                                reader = BLL.ReturnValue("select max(id) from Deals_Bargining_Products where Email='" + useremail + "' and sku=" + sku, false);
                                if (reader.Read())
                                {
                                    dayimg = Tools.Secrecy.Escape("1|" + reader.GetInt32(0) + "|" + Tools.Secrecy.Escape(shortname) + "|" + sku);
                                }
                                reader.Close();
                            }
                        }

                    }
                    else
                        dayimg = Tools.Secrecy.Escape("-1|err");





                }
                else if (r == "bariningnow")
                {
                    dayimg = Tools.Secrecy.Escape("0|err");

                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]), shortname = string.Empty,includeemail=string.Empty;
                    string bid = Tools.Tools.getSafeCode(context.Request["bid"]);
                    string IP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
                    decimal oldprice = 0, endprice = 0, nowprice = 0,chajia=0,KanJia=0;
                    bool iscanBarining = true,iscanbariningbyemail=true;


                    string useremail = "SGIEWRLTKERGITER843",nickname=string.Empty;
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }

                    SqlDataReader reader = BLL.ReturnValue("select OldPrice,EndPrice,NowPrice,email from Deals_Bargining_Products where id=" + bid, false);

                    if (reader.Read())
                    {
                        oldprice = reader.GetDecimal(0);
                        endprice = reader.GetDecimal(1);
                        nowprice = reader.GetDecimal(2);
                        chajia = nowprice - endprice;
                        includeemail = reader.GetString(3);
                    }
                    reader.Close();


                    reader = BLL.ReturnValue("select COUNT(*) from Deals_Bargining_list where ip='" + IP + "' and IncludeID=" + bid, false);

                    if (reader.Read())
                    {
                        if (reader.GetInt32(0) > 0)
                            iscanBarining = false;
                    }
                    reader.Close();

                    reader = BLL.ReturnValue("select COUNT(*) from Deals_Bargining_list where Email='" + useremail + "' and IncludeID=" + bid, false);

                    if (reader.Read())
                    {
                        if (reader.GetInt32(0) > 0)
                            iscanbariningbyemail = false;
                    }
                    reader.Close();



                    if (iscanBarining)
                    {
                        if (iscanbariningbyemail)
                        {
                            if (includeemail.ToLower() == useremail.ToLower())
                            {
                                dayimg = Tools.Secrecy.Escape("-1|Sorry, you can't cut the price for yourself.");
                            }
                            else
                            {
                                reader = BLL.ReturnValue("select Email,Nick from ACT_Accounts where email='" + useremail + "'", false);

                                if (reader.Read())
                                {
                                    nickname = reader.IsDBNull(1) ? "" : reader.GetString(1);
                                }
                                reader.Close();

                                if (nickname.Length < 1)
                                    nickname = useremail.Substring(0, 5) + "...";

                                string[] arrStr = new string[24] { "0.15", "0.75", "1.25", "1.65", "0.36", "0.74", "0", "0.29", "1.37", "1.42", "1.07", "3.47", "2.93", "2.71", "5.62", "4.41", "0.98", "0.72", "3.33", "4.87", "5.84", "2.47", "3.95", "1.88" };

                                System.Random rnd = new System.Random();
                                int radi = rnd.Next(0, 23);
                                KanJia = Convert.ToDecimal(arrStr[radi]);

                                if (KanJia > chajia)
                                    KanJia = chajia;




                                string kanjiamessage = "I'm sorry, I didn't help you.";

                                if (KanJia > 0)
                                    kanjiamessage = " really cool, he helped you cut off " + KanJia.ToString("C") + ".";

                                bool updatestatus = false;

                                updatestatus = BLL.ExecuteSqlFun("update Deals_Bargining_Products set NowPrice=NowPrice-" + KanJia + " where id=" + bid, false);

                                updatestatus = BLL.ExecuteSqlFun("insert Deals_Bargining_list(Email,Message,Timesamp,IncludeID,IP) values('" + useremail + "','" + kanjiamessage + "','" + DateTime.Now.ToString() + "'," + bid + ",'" + IP + "')", false);

                                if (updatestatus)
                                    dayimg = Tools.Secrecy.Escape("1|OK");
                            }
                        }
                        else
                        {
                            dayimg = Tools.Secrecy.Escape("0|The same account can't be cut down many times.");
                        }
                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("0|The same IP can't be cut down many times.");
                    }




                }
                else if (r == "getorderstatus")
                {
                    string remark = Tools.Tools.getSafeCode(context.Request["remark"]);
                    string email = remark.Split('|')[0];
                    string ordernumber = remark.Split('|')[1];
                    string contentdatelist = string.Empty;

                    DataSet ds = BLL.ReturnDataSet("select OS.OrderNumber,OS.StatusCode,OS.Timestamp,OS.Notes,O.ShippingTrackingNumber from OrderStatus OS,Orders O where OS.ordernumber='" + ordernumber + "' and OS.OrderNumber=(select OrderNumber from Orders where OrderNumber='" + ordernumber + "' and EMail='" + email + "') and OS.ordernumber=O.ordernumber", false);

                    dayimg = "1";
                    sb.Remove(0, sb.Length);
                    sb.Append("{\"Results\":\"" + dayimg + "\",\"rank\":[");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {

                            sb.Append("{\"OrderNumber\":\"" + ds.Tables[0].Rows[i]["OrderNumber"].ToString() + "\",\"status\":\"" + Tools.Secrecy.Escape(Tools.Order.OrderStatusLookup(Convert.ToInt32(ds.Tables[0].Rows[i]["StatusCode"]))) + "\",\"times\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Timestamp"].ToString()) + "\",\"tracknumber\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["ShippingTrackingNumber"].ToString()) + "\",\"remark\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["Notes"].ToString()) + "\"}");
                            if (i <= (ds.Tables[0].Rows.Count - 2))
                            {
                                sb.Append(",");
                            }
                        }
                    }
                    sb.Append("]}");


                    context.Response.Write(sb.ToString());
                    context.Response.End();

                }
                else if (r == "qcheckuser")
                {
                    string useremail = context.Request["username"].Replace(" ", "jiahao").Replace(".aspx", ".do").Replace("jiahao", "+");
                    string pass = context.Request["password"];
                    SqlDataReader reader = BLL.ReturnValue("select E.CustemType,E.AffDiscount,E.Status,E.Verified,E.DropStatus,E.Password,E.PaypalEmail,E.Nick,E.Discount,P.Point,E.LoginTimestamp,E.LastName from ACT_Accounts E,Web_Point P where E.EMail=P.email and E.email='" + useremail + "'", false);

                    if (reader.Read())
                    {

                        if (reader.GetBoolean(3) == false)
                        {
                            dayimg = Tools.Secrecy.Escape("0|You have not activated.");
                        }
                        else if (reader.GetBoolean(2) == false)
                        {
                            dayimg = Tools.Secrecy.Escape("0|Your account has been frozen.");
                        }
                        else if (pass == reader.GetString(5))
                        {
                            Tools.Cookie.SetEncryptedCookie("useremail", useremail);
                            Tools.Cookie.SetEncryptedCookie("nick", (!reader.IsDBNull(7) && reader.GetString(7).Length > 0) ? reader.GetString(7) : (!reader.IsDBNull(11) && reader.GetString(11).Length > 0 ? reader.GetString(11) : useremail.Substring(0, useremail.Length - useremail.IndexOf("@") - 1).Replace("@", "")));
                            Tools.Cookie.SetEncryptedCookie("isaff", reader.GetDecimal(1) == 0 ? "0" : "1");
                            Tools.Cookie.SetEncryptedCookie("isdropshipp", reader.GetBoolean(4).ToString());

                            Tools.Cookie.SetEncryptedCookie("payemail", reader.IsDBNull(6) ? useremail : reader.GetString(6));
                            Tools.Cookie.SetEncryptedCookie("qpassword", pass);
                            Tools.Cookie.SetEncryptedCookie("meberlevel", "usu");

                            Tools.Cookie.SetEncryptedCookie("point", reader.IsDBNull(9) ? "0" : reader.GetInt32(9).ToString());
                            Tools.Cookie.SetEncryptedCookie("discount", reader.IsDBNull(8) ? "No Discount" : Math.Round(((1 - reader.GetDecimal(8)) * 100), 0).ToString() + "%");
                            Tools.Cookie.SetEncryptedCookie("logindate", DateTime.Now.ToString());
                            Tools.Cookie.SetEncryptedCookie("lastlogintime", reader.IsDBNull(10) ? DateTime.Now.ToString() : reader.GetDateTime(10).ToString());

                            dayimg = "1|ok";
                        }
                        else
                        {
                            dayimg = "0|Password wrong,try again.";
                        }
                    }
                    else
                    {
                        dayimg = "0|You have not registered.";
                    }
                    reader.Close();
                }
                else if (r == "pagecheckuserinfo")
                {
                    if (Tools.Cookie.CookieCheck("logindate"))
                    {

                        if (DateTime.Compare(Convert.ToDateTime(Tools.Cookie.GetEncryptedCookieValue("logindate")), DateTime.Now.AddHours(6)) < 0)
                        {
                            dayimg = Tools.Secrecy.Escape("1|" + Tools.Cookie.GetEncryptedCookieValue("nick") + "|" + Tools.Cookie.GetEncryptedCookieValue("point") + "|" + Tools.Cookie.GetEncryptedCookieValue("discount") + "|" + Tools.Cookie.GetEncryptedCookieValue("meberlevel") + "|" + Tools.PostStr.lenstr(Tools.Cookie.GetEncryptedCookieValue("nick"), 9));
                        }
                        else
                        {
                            string useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");
                            string pass = Tools.Cookie.GetEncryptedCookieValue("qpassword");

                            SqlDataReader reader = BLL.ReturnValue("select E.CustemType,E.AffDiscount,E.Status,E.Verified,E.DropStatus,E.Password,E.PaypalEmail,E.Nick,E.Discount,P.Point,E.LoginTimestamp from ACT_Accounts E,Web_Point P where E.EMail=P.email and E.email='" + useremail + "'", false);

                            if (reader.Read())
                            {
                                if (reader.GetBoolean(3) == false)
                                {
                                    dayimg = Tools.Secrecy.Escape("0|You have not activated.");
                                }
                                else if (reader.GetBoolean(2) == false)
                                {
                                    dayimg = Tools.Secrecy.Escape("0|Your account has been frozen.");
                                }
                                else if (pass == reader.GetString(5))
                                {
                                    Tools.Cookie.SetEncryptedCookie("useremail", useremail);
                                    Tools.Cookie.SetEncryptedCookie("nick", reader.IsDBNull(7) ? useremail.Substring(0, useremail.Length - useremail.IndexOf("@") - 1).Replace("@", "") : reader.GetString(7));
                                    Tools.Cookie.SetEncryptedCookie("isaff", reader.GetDecimal(1) == 0 ? "0" : "1");
                                    Tools.Cookie.SetEncryptedCookie("isdropshipp", reader.GetBoolean(4).ToString());

                                    Tools.Cookie.SetEncryptedCookie("payemail", reader.IsDBNull(6) ? useremail : reader.GetString(6));
                                    Tools.Cookie.SetEncryptedCookie("qpassword", pass);
                                    Tools.Cookie.SetEncryptedCookie("meberlevel", "usu");
                                    Tools.Cookie.SetEncryptedCookie("point", reader.IsDBNull(9) ? "0" : reader.GetInt32(9).ToString());
                                    Tools.Cookie.SetEncryptedCookie("discount", reader.IsDBNull(8) ? "No Discount" : Math.Round(((1 - reader.GetDecimal(8)) * 100), 0).ToString() + "%");
                                    Tools.Cookie.SetEncryptedCookie("logindate", DateTime.Now.ToString());
                                    Tools.Cookie.SetEncryptedCookie("lastlogintime", reader.IsDBNull(10) ? DateTime.Now.ToString() : reader.GetDateTime(10).ToString());


                                    dayimg = Tools.Secrecy.Escape("1|" + (reader.IsDBNull(7) ? useremail.Substring(0, useremail.Length - useremail.IndexOf("@") - 1).Replace("@", "") : reader.GetString(7)) + "|" + (reader.IsDBNull(9) ? "0" : reader.GetInt32(9).ToString()) + "|" + (reader.IsDBNull(8) ? "No Discount" : ((1 - reader.GetDecimal(8)) * 100).ToString() + "%") + "|" + Tools.Mebers.TypeLookup(reader.GetInt32(0)) + "|" + (reader.IsDBNull(7) ? Tools.PostStr.lenstr(useremail.Substring(0, useremail.Length - useremail.IndexOf("@")), 9) : Tools.PostStr.lenstr(reader.GetString(7), 9)));
                                }
                                else
                                {
                                    dayimg = Tools.Secrecy.Escape("0|Unknown error.");
                                }
                            }
                            else
                            {
                                dayimg = Tools.Secrecy.Escape("0|You have not registered.");
                            }
                            reader.Close();
                        }
                    }
                    else
                    {
                        if (Tools.Cookie.CookieCheck("qpassword"))
                        {

                            string useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");
                            string pass = Tools.Cookie.GetEncryptedCookieValue("qpassword");

                            SqlDataReader reader = BLL.ReturnValue("select E.CustemType,E.AffDiscount,E.Status,E.Verified,E.DropStatus,E.Password,E.PaypalEmail,E.Nick,E.Discount,P.Point,E.LoginTimestamp from ACT_Accounts E,Web_Point P where E.EMail=P.email and E.email='" + useremail + "'", false);

                            if (reader.Read())
                            {
                                if (reader.GetBoolean(3) == false)
                                {
                                    dayimg = Tools.Secrecy.Escape("0|You have not activated.");
                                }
                                else if (reader.GetBoolean(2) == false)
                                {
                                    dayimg = Tools.Secrecy.Escape("0|Your account has been frozen.");
                                }
                                else if (pass == reader.GetString(5))
                                {
                                    Tools.Cookie.SetEncryptedCookie("useremail", useremail);
                                    Tools.Cookie.SetEncryptedCookie("nick", reader.IsDBNull(7) ? useremail.Substring(0, useremail.Length - useremail.IndexOf("@") - 1).Replace("@", "") : reader.GetString(7));
                                    Tools.Cookie.SetEncryptedCookie("isaff", reader.GetDecimal(1) == 0 ? "0" : "1");

                                    Tools.Cookie.SetEncryptedCookie("isdropshipp", reader.GetBoolean(4).ToString());

                                    Tools.Cookie.SetEncryptedCookie("payemail", reader.IsDBNull(6) ? useremail : reader.GetString(6));
                                    Tools.Cookie.SetEncryptedCookie("qpassword", pass);
                                    Tools.Cookie.SetEncryptedCookie("meberlevel", "usu");
                                    Tools.Cookie.SetEncryptedCookie("point", reader.IsDBNull(9) ? "0" : reader.GetInt32(9).ToString());
                                    Tools.Cookie.SetEncryptedCookie("discount", reader.IsDBNull(8) ? "No Discount" : Math.Round(((1 - reader.GetDecimal(8)) * 100), 0).ToString() + "%");
                                    Tools.Cookie.SetEncryptedCookie("logindate", DateTime.Now.ToString());
                                    Tools.Cookie.SetEncryptedCookie("lastlogintime", reader.IsDBNull(10) ? DateTime.Now.ToString() : reader.GetDateTime(10).ToString());

                                    dayimg = Tools.Secrecy.Escape("1|" + useremail + "|" + (reader.IsDBNull(9) ? "0" : reader.GetInt32(9).ToString()) + "|" + (reader.IsDBNull(9) ? "No Discount" : ((1 - reader.GetDecimal(8)) * 100).ToString() + "%") + "|" + Tools.Mebers.TypeLookup(reader.GetInt32(0)) + "|" + (reader.IsDBNull(7) ? Tools.PostStr.lenstr(useremail.Substring(0, useremail.Length - useremail.IndexOf("@")), 9) : Tools.PostStr.lenstr(reader.GetString(7), 9)));
                                }
                                else
                                {
                                    dayimg = Tools.Secrecy.Escape("0|Unknown error.");
                                }
                            }
                            else
                            {
                                dayimg = Tools.Secrecy.Escape("0|You have not registered.");
                            }
                            reader.Close();
                        }
                        else
                        {
                            string useremail = Tools.Tools.getSafeCode(context.Request["useremail"]);
                            string pass = Tools.Tools.getSafeCode(context.Request["qpassword"]);

                            SqlDataReader reader = BLL.ReturnValue("select E.CustemType,E.AffDiscount,E.Status,E.Verified,E.DropStatus,E.Password,E.PaypalEmail,E.Nick,E.Discount,P.Point,E.LoginTimestamp from ACT_Accounts E,Web_Point P where E.EMail=P.email and E.email='" + useremail + "'", false);

                            if (reader.Read())
                            {
                                if (reader.GetBoolean(3) == false)
                                {
                                    dayimg = Tools.Secrecy.Escape("0|You have not activated.");
                                }
                                else if (reader.GetBoolean(2) == false)
                                {
                                    dayimg = Tools.Secrecy.Escape("0|Your account has been frozen.");
                                }
                                else if (pass == reader.GetString(5))
                                {
                                    Tools.Cookie.SetEncryptedCookie("useremail", useremail);
                                    Tools.Cookie.SetEncryptedCookie("nick", reader.IsDBNull(7) ? useremail.Substring(0, useremail.Length - useremail.IndexOf("@") - 1).Replace("@", "") : reader.GetString(7));
                                    Tools.Cookie.SetEncryptedCookie("isaff", reader.GetDecimal(1) == 0 ? "0" : "1");

                                    Tools.Cookie.SetEncryptedCookie("isdropshipp", reader.GetBoolean(4).ToString());

                                    Tools.Cookie.SetEncryptedCookie("payemail", reader.IsDBNull(6) ? useremail : reader.GetString(6));
                                    Tools.Cookie.SetEncryptedCookie("qpassword", pass);
                                    Tools.Cookie.SetEncryptedCookie("meberlevel", "usu");
                                    Tools.Cookie.SetEncryptedCookie("point", reader.IsDBNull(9) ? "0" : reader.GetInt32(9).ToString());
                                    Tools.Cookie.SetEncryptedCookie("discount", reader.IsDBNull(8) ? "No Discount" : Math.Round(((1 - reader.GetDecimal(8)) * 100), 0).ToString() + "%");
                                    Tools.Cookie.SetEncryptedCookie("logindate", DateTime.Now.ToString());
                                    Tools.Cookie.SetEncryptedCookie("lastlogintime", reader.IsDBNull(10) ? DateTime.Now.ToString() : reader.GetDateTime(10).ToString());

                                    dayimg = Tools.Secrecy.Escape("1|" + useremail + "|" + (reader.IsDBNull(9) ? "0" : reader.GetInt32(9).ToString()) + "|" + (reader.IsDBNull(9) ? "No Discount" : ((1 - reader.GetDecimal(8)) * 100).ToString() + "%") + "|" + Tools.Mebers.TypeLookup(reader.GetInt32(0)) + "|" + (reader.IsDBNull(7) ? Tools.PostStr.lenstr(useremail.Substring(0, useremail.Length - useremail.IndexOf("@")), 9) : Tools.PostStr.lenstr(reader.GetString(7), 9)));
                                }
                                else
                                {
                                    dayimg = Tools.Secrecy.Escape("0|Unknown error.");
                                }
                            }
                            else
                            {
                                dayimg = Tools.Secrecy.Escape("0|<font color=\"red\">Error with your account,try again.</font>");
                            }
                            reader.Close();

                        }
                    }

                }
                else if (r == "addnewsletter")
                {
                    string email = Tools.Tools.getSafeCode(context.Request["email"]).ToLower();
                    bool addstatus = true;
                    SqlDataReader reader = BLL.ReturnValue("select email from Web_Newsletter where email='" + email + "'", false);
                    if (reader.Read())
                    {
                        addstatus = false;
                    }
                    reader.Close();

                    if (addstatus)
                    {
                        bool updatestatus = false;

                        updatestatus = BLL.ExecuteSqlFun("insert Web_Newsletter(Email,IsNew,IsPromotion,IsClearance,isOther) values('" + email + "',1,1,1,1)", false);

                        if (updatestatus)
                            dayimg = Tools.Secrecy.Escape("1|1");
                        else
                            dayimg = Tools.Secrecy.Escape("0|0");
                    }
                    else
                        dayimg = Tools.Secrecy.Escape("0|This email already exists.");
                }
                else if (r == "addtocart")
                {
                    dayimg = Tools.Secrecy.Escape("Sorry, you have no access to our data. Please contact: paul@liuhaimin.com");

                    GetConfigInfo configinfo = new GetConfigInfo();

                    string shoppingurl = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "shoppingcarturl"));

                    if (shoppingurl.IndexOf("https:") < 0)
                    {
                        if (shoppingurl.IndexOf("http://") < 0)
                            shoppingurl = "http://" + shoppingurl;
                    }
                    else
                    {
                        shoppingurl = shoppingurl.Replace("https//", "https://");
                    }

                    string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
                    string sku = Tools.Tools.IsNumeric(context.Request["sku"]) ? Tools.Tools.getSafeCode(context.Request["sku"]) : "0";
                    string warestock = Tools.Tools.IsNumeric(context.Request["wt"]) ? Tools.Tools.getSafeCode(context.Request["wt"]) : "0";
                    string qty = Tools.Tools.IsNumeric(context.Request["qty"]) ? Tools.Tools.getSafeCode(context.Request["qty"]) : "1";
                    string gl = (context.Request["gl"] != null && context.Request["gl"].Length > 0) ? context.Request["gl"] : "";

                    int quantity = Int32.Parse(qty);
                    decimal proPrice = 0, limitetrackfee = 20, trackfee = 1.5M, OldPrice = 0;
                    limitetrackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "limitetrackfee")));
                    trackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "trackfee")));

                    string shortname = string.Empty, color = string.Empty, mode = string.Empty, smallpictureurl = string.Empty, leadtime = string.Empty, glsku = string.Empty, glprice = string.Empty, shopname = string.Empty, producturl = string.Empty, warehouse = "0", tempFullname = string.Empty;
                    decimal sampleprice = 0, pricemid = 0, pricemin = 0, promotionprice = 0, lostcartotal = 0, unitweight = 0, carronweight = 0, discount = 0;
                    bool instock = false, isinstock = false, isairmail = true, battery = false, issoldoutpro = false, supportcoupon = true, liquid = false, errSku = false, updatestatus = false, IsPomotion = false;
                    int promotiontype = 0, bigcategory = 0, midcategory = 0, mincategory = 0, stock = 0, Limtid_soldQty = 1;
                    DateTime starttime = DateTime.Now, endtime = DateTime.Now;
                    bool ispromotionproduct = false;

                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    DataSet ds = new DataSet();

                    //////////////////////////////////Get Product Info/////////////////////////////////////////
                    SqlConnection sqlConn = DAL.localrcon();
                    sqlConn.Open();
                    SqlCommand cmdDB = new SqlCommand(@"GetShoppingSKUInfo", sqlConn);
                    cmdDB.CommandType = CommandType.StoredProcedure;
                    cmdDB.Parameters.Add("@sku", SqlDbType.Int).Value = Int32.Parse(sku);
                    SqlDataAdapter adapt = new SqlDataAdapter(cmdDB);
                    adapt.Fill(ds, "ProductInfo");
                    ///////////////////////////////////Get Product Info///////////////////////////////////////                   

                    DataRow row = ds.Tables[0].Rows[0];
                    if (ds.Tables["ProductInfo"].Rows.Count == 1)
                    {
                        shortname = (string)row["Shortname"];
                        smallpictureurl = (string)row["smallpictureurl"];
                        leadtime = (string)row["leadtime"];
                        glsku = row["glsku"].ToString().Length > 0 ? (string)row["glsku"] : "";
                        glprice = row["glprice"].ToString().Length > 0 ? (string)row["glprice"] : "";
                        shopname = row["shopname"].ToString().Length > 0 ? (string)row["shopname"] : "";
                        producturl = (string)row["producturl"];
                        warehouse = (string)row["warehouse"];
                        sampleprice = (decimal)row["sampleprice"];
                        pricemid = (decimal)row["pricemid"];
                        pricemin = (decimal)row["pricemin"];
                        promotionprice = (decimal)row["promotionprice"];
                        lostcartotal = row["LostCartTotal"].ToString().Length > 0 ? (decimal)row["LostCartTotal"] : 0;
                        unitweight = (decimal)row["unitweight"];
                        carronweight = row["CartonWeight"].ToString().Length > 0 ? (decimal)row["CartonWeight"] : 10;
                        instock = (bool)row["instock"];
                        isairmail = (bool)row["isairmail"];
                        battery = (bool)row["battery"];
                        issoldoutpro = (bool)row["issoldoutpro"];
                        supportcoupon = (bool)row["supportcoupon"];
                        liquid = (bool)row["liquid"];
                        promotiontype = (int)row["promotiontype"];
                        bigcategory = (int)row["bigcategory"];
                        midcategory = (int)row["midcategory"];
                        mincategory = (int)row["mincategory"];
                        stock = (int)row["stock"];
                        if (stock > 0)
                            isinstock = true;
                        starttime = row["starttime"].ToString().Length > 0 ? Convert.ToDateTime(row["starttime"]) : DateTime.Now;
                        endtime = row["endtime"].ToString().Length > 0 ? Convert.ToDateTime(row["endtime"]) : DateTime.Now;
                        Limtid_soldQty = row["Limtid_soldQty"].ToString().Length > 0 ? (int)row["Limtid_soldQty"] : 1;
                        color = row["color"].ToString();
                        mode = row["mode"].ToString();
                        OldPrice = Convert.ToDecimal(row["msrp"]);

                        if (quantity < Limtid_soldQty)
                            quantity = Limtid_soldQty;

                        if (promotiontype == 0)
                        {
                            //正常商品
                            if (quantity >= 4)
                                proPrice = pricemin;
                            else if (quantity > 1)
                                proPrice = pricemid;
                            else
                                proPrice = sampleprice;

                            //vip price
                            if (HttpContext.Current.Session["vipdiscount"] != null && Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]) < 1)
                            {
                                discount = proPrice - proPrice * Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]);
                                proPrice = proPrice * Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]);

                            }
                            else
                                discount = OldPrice - sampleprice;


                            if (issoldoutpro == true)
                            {
                                if (stock < quantity)
                                {
                                    quantity = stock;
                                }
                                if (quantity > 0)
                                {
                                    //减库存
                                    updatestatus = BLL.ExecuteSqlFun("update products set stock=stock-" + quantity + " where sku=" + sku + ";insert TempStockList(SKU,SoldNumber,IP,WareHouse) values(" + sku + "," + quantity + ",'" + ip.ToString() + "','dt')", false);
                                }
                                else
                                {
                                    dayimg = Tools.Secrecy.Escape("0|The goods have been sold out."); //时间不对
                                    errSku = true;
                                }

                            }
                        }
                        else
                        {
                            // promotion item
                            //时间在促销期
                            if (DateTime.Compare(DateTime.Now, starttime) > 0 && DateTime.Compare(endtime, DateTime.Now) > 0)
                            {
                                if (lostcartotal > cart.GetTotal("0"))
                                {
                                    quantity = 0;
                                    dayimg = Tools.Secrecy.Escape("0|I'm sorry,  you can buy this goods only after  purchase of goods more than <strong>" + (curformat + " " + (Convert.ToDecimal(row["LostCartTotal"]) * Convert.ToDecimal(currencyrate)).ToString("F")) + "</strong> , please choose other goods first, click <a href=\"/category/Random/\">here</a> to purchase!");
                                    errSku = true;
                                }
                                else
                                {
                                    //促销商品类别分类 0 正常商品；1无限制促销商品；2一人购买一个促销；3无限制减库存促销；4一人购买一个减库存促销

                                    if (promotiontype == 3 || promotiontype == 4)
                                    {
                                        if (stock < 1)
                                        {
                                            quantity = 0;
                                            dayimg = Tools.Secrecy.Escape("0|Sorry，SKU:<FONT COLOR=\"RED\"><STRONG>" + sku.ToString() + "</STRONG></FONT> Sold out.Change SKU！<a href=\"/product/sku-" + sku.ToString() + "\">Return</a>");
                                            errSku = true;
                                        }
                                        else
                                        {
                                            if (stock < quantity)
                                                quantity = stock;
                                        }

                                        //减库存
                                        updatestatus = BLL.ExecuteSqlFun("update products set stock=stock-" + quantity + " where sku=" + sku + ";insert TempStockList(SKU,SoldNumber,IP,WareHouse) values(" + sku + "," + quantity + ",'" + ip.ToString() + "','dt')", false);
                                        ispromotionproduct = true;
                                    }

                                    if (promotiontype == 2 || promotiontype == 4)
                                    {
                                        quantity = 1;
                                        string pomotionsku = string.Empty;

                                        if (Tools.Cookie.CookieCheck("pomotionsku"))
                                        {
                                            if (Tools.Cookie.GetEncryptedCookieValue("pomotionsku").IndexOf("|" + (string)sku) < 0)
                                                pomotionsku = Tools.Cookie.GetEncryptedCookieValue("pomotionsku");
                                            else
                                            {
                                                dayimg = Tools.Secrecy.Escape("0|Sorry，SKU:<FONT COLOR=\"RED\"><STRONG>" + sku.ToString() + "</STRONG></FONT> you can buy this product only once!");
                                                errSku = true;
                                            }
                                        }
                                        else
                                            pomotionsku = "0";

                                        Tools.Cookie.SetEncryptedCookie("pomotionsku", pomotionsku + "|" + (string)sku);
                                    }

                                    proPrice = promotionprice;
                                    IsPomotion = true;
                                    tempFullname = "Pm" + promotiontype.ToString();
                                    OldPrice = sampleprice;
                                    discount = sampleprice - proPrice;
                                }
                            }
                            else
                            {
                                //促销商品没有停售且不在促销期购买则按正常商品一样
                                //正常商品
                                if (quantity >= 4)
                                    proPrice = pricemin;
                                else if (quantity > 1)
                                    proPrice = pricemid;
                                else
                                    proPrice = sampleprice;

                                //vip price
                                if (HttpContext.Current.Session["vipdiscount"] != null && Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]) < 1)
                                {
                                    discount = proPrice - proPrice * Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]);
                                    proPrice = proPrice * Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]);
                                }
                                else
                                    discount = OldPrice - sampleprice;

                                if (issoldoutpro == true)
                                {
                                    if (stock < quantity)
                                    {
                                        quantity = stock;
                                    }
                                    if (quantity > 0)
                                    {
                                        //减库存
                                        updatestatus = BLL.ExecuteSqlFun("update products set stock=stock-" + quantity + " where sku=" + sku + ";insert TempStockList(SKU,SoldNumber,IP,WareHouse) values(" + sku + "," + quantity + ",'" + ip.ToString() + "','dt')", false);
                                    }
                                    else
                                    {
                                        dayimg = Tools.Secrecy.Escape("0|The goods have been sold out."); //时间不对
                                        errSku = true;
                                    }

                                }

                            }
                        }



                        if (!errSku && quantity > 0)
                        {

                            cart.Remove("0" + row["SKU"].ToString());
                            cart.Remove(row["SKU"].ToString());

                            if (ispromotionproduct)
                                cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), color, mode, isinstock, (isinstock ? "" : "Delivery time may take 3-7 days"), (isinstock ? "Required 2-7 business days for processing" : ""), bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, OldPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true,false, false, isairmail, true, false, supportcoupon, IsPomotion, battery, liquid, warehouse, 3); //这 里 是 减 库 存 商 品 
                            else
                                cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), color, mode, isinstock, (isinstock ? "" : "Delivery time may take 3-7 days"), (isinstock ? "Required 2-7 business days for processing" : ""), bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, OldPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true,false, false, isairmail, true, false, supportcoupon, IsPomotion, battery, liquid, warehouse);


                            if (gl.Length > 0)
                            {
                                string tempglfullname = string.Empty;

                                string glskustr = (string)gl.ToString().Replace("0,", "").ToString();
                                ds = getGlskuinfo(glskustr);

                                string[] proglskulist = glsku.Split(',');
                                string[] proglskulistprice = glprice.Split(',');
                                decimal glproductprice = 0;

                                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {
                                    for (int j = 0; j < proglskulist.Length; j++)
                                    {
                                        if (ds.Tables[0].Rows[i]["sku"].ToString() == proglskulist[j].ToString())
                                        {
                                            cart.Remove(proglskulist[j]);
                                            tempglfullname = ds.Tables[0].Rows[i]["Shortname"].ToString();
                                            glproductprice = Convert.ToDecimal(proglskulistprice[j]);
                                            cart.Add(Int32.Parse(proglskulist[j]), proglskulist[j].ToString(), Int32.Parse(proglskulist[j].ToString()), "Default", "Default", true, "", "", Convert.ToInt32(ds.Tables[0].Rows[i]["BigCategory"]), Convert.ToInt32(ds.Tables[0].Rows[i]["MidCategory"]), Convert.ToInt32(ds.Tables[0].Rows[i]["MinCategory"]), Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]) - glproductprice, Int32.Parse(sku), ds.Tables[0].Rows[i]["SmallPictureURL"].ToString(), "", ds.Tables[0].Rows[i]["Shortname"].ToString(), 1, glproductprice, glproductprice, Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitWeight"]) + Convert.ToDecimal(ds.Tables[0].Rows[i]["CartonWeight"]), ds.Tables[0].Rows[i]["LeadTime"].ToString(), ds.Tables[0].Rows[i]["ProductURL"].ToString(), ds.Tables[0].Rows[i]["ShopName"].ToString(), true, false,false, Convert.ToBoolean(ds.Tables[0].Rows[i]["Isairmail"]), true, true, Convert.ToBoolean(ds.Tables[0].Rows[i]["Supportcoupon"]), false, Convert.ToBoolean(ds.Tables[0].Rows[i]["Battery"]), Convert.ToBoolean(ds.Tables[0].Rows[i]["Liquid"]), ds.Tables[0].Rows[i]["warehouse"].ToString());
                                        }
                                    }
                                }
                            }
                            cart.RefreshCoupon(1779, cart.GetSubtotal(warestock), warestock);
                            string tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warestock));

                            if (warehouse == "1")
                            {
                                dayimg = Tools.Secrecy.Escape("1|germany|" + Tools.Secrecy.Escape(getTuiitemlist(sku)) + "|" + (curformat + " " + (cart.GetSubtotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (cart.GetSubtotal(warestock) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(warestock).ToString("F") + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((proPrice * quantity) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (ispromotionproduct ? "1" : "0") + "|" + quantity.ToString() + "|" + Math.Round((cart.GetTotal(warestock) / 10), 0).ToString() + "|" + (cart.IsinShippingcart(-8888) ? "1" : "0") + "|" + Tools.Secrecy.Escape(shoppingurl));

                            }
                            else if (warehouse == "2")
                            {
                                dayimg = Tools.Secrecy.Escape("1|usa|" + Tools.Secrecy.Escape(getTuiitemlist(sku)) + "|" + (curformat + " " + (cart.GetSubtotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (cart.GetSubtotal(warestock) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(warestock).ToString("F") + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((proPrice * quantity) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (ispromotionproduct ? "1" : "0") + "|" + quantity.ToString() + "|" + Math.Round((cart.GetTotal(warestock) / 10), 0).ToString() + "|" + (cart.IsinShippingcart(-8888) ? "1" : "0") + "|" + Tools.Secrecy.Escape(shoppingurl));

                            }
                            else
                                dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + Tools.Secrecy.Escape(getTuiitemlist(sku)) + "|" + (curformat + " " + (cart.GetSubtotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (cart.GetSubtotal(warestock) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(warestock).ToString("F") + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((proPrice * quantity) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (ispromotionproduct ? "1" : "0") + "|" + quantity.ToString() + "|" + Math.Round((cart.GetTotal(warestock) / 10), 0).ToString() + "|" + (cart.IsinShippingcart(-8888) ? "1" : "0") + "|" + Tools.Secrecy.Escape(shoppingurl));

                        }

                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("0|Without this commodity.");
                    }

                    if (cart.IsinShippingcart(1779))
                        cart.RefreshCoupon(1779, cart.GetSupportCouponTotal("0"), "0");

                    string os = HttpContext.Current.Request.UserAgent.ToString();

                    string vemail = "guest";
                    if (Tools.Cookie.CookieCheck("useremail"))
                        vemail = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    if (os != null && ip != null)
                    {
                        try
                        {
                            insertbuyskulist(Tools.Tools.GetOSNameByUserAgent(os), ip, sku.ToString(), vemail);
                        }
                        catch { }
                    }





                }
                else if (r == "buybariningsku")
                {
                    dayimg = Tools.Secrecy.Escape("Sorry, you have no access to our data. Please contact: paul@liuhaimin.com");

                    GetConfigInfo configinfo = new GetConfigInfo();

                    string shoppingurl = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "shoppingcarturl"));
                    string website = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getEmailconfigfile, "website"));

                    if (shoppingurl.IndexOf("https:") < 0)
                    {
                        if (shoppingurl.IndexOf("http://") < 0)
                            shoppingurl = "http://" + shoppingurl;
                    }
                    else
                    {
                        shoppingurl = shoppingurl.Replace("https//", "https://");
                    }

                    string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
                    string id = Tools.Tools.IsNumeric(context.Request["id"]) ? Tools.Tools.getSafeCode(context.Request["id"]) : "0";
                    string warestock = Tools.Tools.IsNumeric(context.Request["wt"]) ? Tools.Tools.getSafeCode(context.Request["wt"]) : "0";
                    string qty = Tools.Tools.IsNumeric(context.Request["qty"]) ? Tools.Tools.getSafeCode(context.Request["qty"]) : "1";
                    string gl = (context.Request["gl"] != null && context.Request["gl"].Length > 0) ? context.Request["gl"] : "";
                    string sku = "";

                    int quantity = Int32.Parse(qty);
                    decimal proPrice = 0, limitetrackfee = 20, trackfee = 1.5M, OldPrice = 0;
                    limitetrackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "limitetrackfee")));
                    trackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "trackfee")));

                    string shortname = string.Empty, color = string.Empty, mode = string.Empty, smallpictureurl = string.Empty, leadtime = string.Empty, glsku = string.Empty, glprice = string.Empty, shopname = string.Empty, producturl = string.Empty, warehouse = "0", tempFullname = string.Empty;
                    decimal sampleprice = 0, pricemid = 0, pricemin = 0, promotionprice = 0, lostcartotal = 0, unitweight = 0, carronweight = 0, discount = 0;
                    bool instock = false, isinstock = false, isairmail = true, battery = false, issoldoutpro = false, supportcoupon = true, liquid = false, errSku = false, updatestatus = false, IsPomotion = false;
                    int promotiontype = 0, bigcategory = 0, midcategory = 0, mincategory = 0, stock = 0, Limtid_soldQty = 1;
                    DateTime starttime = DateTime.Now, endtime = DateTime.Now;
                    bool ispromotionproduct = false;

                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    DataSet ds = new DataSet();

                    //////////////////////////////////Get Product Info/////////////////////////////////////////
                    ds = BLL.ReturnDataSet("select * from View_Deals_Bargining_Products where id=" + id, false);
                    ///////////////////////////////////Get Product Info///////////////////////////////////////                   

                    DataRow row = ds.Tables[0].Rows[0];
                    if (ds.Tables[0].Rows.Count == 1)
                    {
                        sku = Convert.ToString(row["sku"]);
                        shortname = (string)row["FullName"];
                        smallpictureurl = (string)row["SmallPictureURL"];
                        leadtime = (string)row["LeadTime"];
                        glsku = "";
                        glprice ="";
                        shopname = (string)row["ShopName"];
                        producturl = "/bargining/" + Convert.ToString(row["id"]) + "/" + Tools.PostStr.potstr((string)row["FullName"]) + "-" + Convert.ToString(row["sku"]);
                        warehouse = (string)row["WareHouse"];
                        sampleprice = (decimal)row["OldPrice"];
                        pricemid = (decimal)row["OldPrice"];
                        pricemin = (decimal)row["OldPrice"];
                        promotionprice = (decimal)row["NowPrice"];
                        lostcartotal = 0;
                        unitweight = (decimal)row["UnitWeight"];
                        carronweight = row["CartonWeight"].ToString().Length > 0 ? (decimal)row["CartonWeight"] : 10;
                        instock = (bool)row["instock"];
                        isairmail = (bool)row["isairmail"];
                        battery = (bool)row["battery"];
                        issoldoutpro = (bool)row["issoldoutpro"];
                        supportcoupon = (bool)row["supportcoupon"];
                        liquid = (bool)row["liquid"];
                        promotiontype = 3;
                        bigcategory = (int)row["PrimaryBigCategoryID"];
                        midcategory = (int)row["PrimaryMidCategoryID"];
                        mincategory = (int)row["PrimaryCategoryID"];
                        stock = (int)row["Stock"];
                        if (stock > 0)
                            isinstock = true;
                        starttime = DateTime.Now.AddHours(-1);
                        endtime = row["EndTime"].ToString().Length > 0 ? Convert.ToDateTime(row["EndTime"]) : DateTime.Now;
                        Limtid_soldQty = 1;
                        color = row["color"].ToString();
                        mode = row["mode"].ToString();
                        OldPrice = Convert.ToDecimal(row["msrp"]);

                        if (quantity < Limtid_soldQty)
                            quantity = Limtid_soldQty;

                        // promotion item
                        //时间在促销期
                        if (DateTime.Compare(DateTime.Now, starttime) > 0 && DateTime.Compare(endtime, DateTime.Now) > 0)
                        {
                            if (lostcartotal > cart.GetTotal("0"))
                            {
                                quantity = 0;
                                dayimg = Tools.Secrecy.Escape("0|I'm sorry,  you can buy this goods only after  purchase of goods more than <strong>" + (curformat + " " + (Convert.ToDecimal(row["LostCartTotal"]) * Convert.ToDecimal(currencyrate)).ToString("F")) + "</strong> , please choose other goods first, click <a href=\"/category/Random/\">here</a> to purchase!");
                                errSku = true;
                            }
                            else
                            {
                                //促销商品类别分类 0 正常商品；1无限制促销商品；2一人购买一个促销；3无限制减库存促销；4一人购买一个减库存促销

                                if (promotiontype == 3 || promotiontype == 4)
                                {
                                    if (stock < 1)
                                    {
                                        quantity = 0;
                                        dayimg = Tools.Secrecy.Escape("0|Sorry，SKU:<FONT COLOR=\"RED\"><STRONG>" + sku.ToString() + "</STRONG></FONT> Sold out.Change SKU！<a href=\"/product/sku-" + sku.ToString() + "\">Return</a>");
                                        errSku = true;
                                    }
                                    else
                                    {
                                        if (stock < quantity)
                                            quantity = stock;
                                    }

                                    //减库存
                                    //updatestatus = BLL.ExecuteSqlFun("update products set stock=stock-" + quantity + " where sku=" + sku + ";insert TempStockList(SKU,SoldNumber,IP,WareHouse) values(" + sku + "," + quantity + ",'" + ip.ToString() + "','dt')", false);
                                    ispromotionproduct = true;
                                }

                                if (promotiontype == 2 || promotiontype == 4)
                                {
                                    quantity = 1;
                                    string pomotionsku = string.Empty;

                                    if (Tools.Cookie.CookieCheck("pomotionsku"))
                                    {
                                        if (Tools.Cookie.GetEncryptedCookieValue("pomotionsku").IndexOf("|" + (string)sku) < 0)
                                            pomotionsku = Tools.Cookie.GetEncryptedCookieValue("pomotionsku");
                                        else
                                        {
                                            dayimg = Tools.Secrecy.Escape("0|Sorry，SKU:<FONT COLOR=\"RED\"><STRONG>" + sku.ToString() + "</STRONG></FONT> you can buy this product only once!");
                                            errSku = true;
                                        }
                                    }
                                    else
                                        pomotionsku = "0";

                                    Tools.Cookie.SetEncryptedCookie("pomotionsku", pomotionsku + "|" + (string)sku);
                                }

                                proPrice = promotionprice;
                                IsPomotion = true;
                                tempFullname = "Pm" + promotiontype.ToString();
                                OldPrice = sampleprice;
                                discount = sampleprice - proPrice;
                            }
                        }



                        if (!errSku && quantity > 0)
                        {
                            string tempcontent = "<div class=\"detail_phone\"><strong>Phone Number:</strong> <input class=\"text phonenumber\" id=\"phonenumbers\" value=\"\" autocomplete=\"off\" type=\"text\"></div>";

                            tempcontent += "<p><strong>About color in shortage</strong></p>";

                            tempcontent += "<p><select id=\"chk_shortagecolor\">";
                            tempcontent += "    <option value=\"0\">Please Choose </option>";
                            tempcontent += "    <option value=\"1\">Contact me at once</option>";
                            tempcontent += "    <option value=\"2\">Replace it by any other colors available and inform me</option>";
                            tempcontent += "    <option value=\"3\">Refund(for items in shortage) and inform me</option>";
                            tempcontent += "    <option value=\"4\">See remarks</option>";
                            tempcontent += "</select></p>";

                            tempcontent += "<p><strong>About size in shortage</strong></p>";

                            tempcontent += "<p><select id=\"chk_shortagesize\">";
                            tempcontent += "    <option value=\"0\">Please Choose </option>";
                            tempcontent += "    <option value=\"1\">Contact me at once</option>";
                            tempcontent += "    <option value=\"2\">Replace it by any other colors available and inform me</option>";
                            tempcontent += "    <option value=\"3\">Refund(for items in shortage) and inform me</option>";
                            tempcontent += "    <option value=\"4\">See remarks</option>";
                            tempcontent += "</select></p>";

                            tempcontent += "<p><strong>About goods in shortage</strong></p>";

                            tempcontent += "<p><select id=\"chk_shortagegoods\">";
                            tempcontent += "    <option value=\"0\">Please Choose </option>";
                            tempcontent += "    <option value=\"1\">Contact me at once</option>";
                            tempcontent += "    <option value=\"2\">Replace it by any other colors available and inform me</option>";
                            tempcontent += "    <option value=\"3\">Refund(for items in shortage) and inform me</option>";
                            tempcontent += "    <option value=\"4\">See remarks</option>";
                            tempcontent += "</select></p>";

                            tempcontent += "<p><strong>Self-service order amount</strong></p>";

                            tempcontent += "<p>";
                            tempcontent += "    <input id=\"zzsb\" placeholder=\"Shop customization\" type=\"text\"></p>";

                            tempcontent += "<div class=\"quick_ordernote\">";
                            tempcontent += "    <h5>Remark to the whole order: you could input extra requirement for the whole order in below column.</h5>";
                            tempcontent += "     <textarea id=\"extrarequest\" class=\"extrarequest\" cols=\"20\" rows=\"2\"></textarea>";
                            tempcontent += "</div>";



                            if (Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "webstatus")) == "open")
                            {
                                //    string paystylestr = PageSource.PageSource.GetPublickSocur("Activipaystyle.ptshop").Trim();
                                //    string[] category1list = paystylestr.Split('\r');
                                //    foreach (string key in category1list)
                                //    {
                                //        if (key.Length > 2)
                                //        {
                                //            tempcontent += "<img src=\"" + key.Trim().Split('|')[1] + "\" class=\"checkout\" onclick=\"checkoutpaystyle('buyrule','" + key.Trim().Split('|')[2] + "','" + warestock + "')\" /><br/>";
                                //        }
                                //    }
                                tempcontent += "<img src=\"/images/paystyle/btn_paywith_primary_l.png\" class=\"checkout jumpnow\" onclick=\"checkoutpaystylebynow('buyrule','paypal','" + warehouse + "')\">";
                                tempcontent += "<div class=\"buy-now-info detail_buy_rule\"><label for=\"policy\"><input id=\"buyrule\" checked=\"\" type=\"checkbox\"><label for=\"buyrule\" class=\"f10\">I agree to the " + website + " <a href=\"/help\" target=\"_blank\">Terms and Policy</a>.</label></label></div>";
                            }



                            cart.Remove("0" + row["SKU"].ToString());
                            cart.Remove(row["SKU"].ToString());

                            if (ispromotionproduct)
                                cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), color, mode, isinstock, (isinstock ? "" : "Delivery time may take 3-7 days"), (isinstock ? "Required 2-7 business days for processing" : ""), bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, OldPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, true, false, supportcoupon, IsPomotion, battery, liquid, warehouse, 3); //这 里 是 减 库 存 商 品 
                            else
                                cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), color, mode, isinstock, (isinstock ? "" : "Delivery time may take 3-7 days"), (isinstock ? "Required 2-7 business days for processing" : ""), bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, OldPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, true, false, supportcoupon, IsPomotion, battery, liquid, warehouse);


                            cart.RefreshCoupon(1779, cart.GetSubtotal(warestock), warestock);
                            string tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warestock).Replace("selectshippingstyle", "pop_selectshippingstyle"));

                            if (warehouse == "1")
                            {
                                dayimg = Tools.Secrecy.Escape("1|germany|" + Tools.Secrecy.Escape(getShoppingcartlist(sku)) + "|" + (curformat + " " + (cart.GetSubtotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (cart.GetSubtotal(warestock) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(warestock).ToString("F") + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((proPrice * quantity) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (ispromotionproduct ? "1" : "0") + "|" + quantity.ToString() + "|" + Math.Round((cart.GetTotal(warestock) / 10), 0).ToString() + "|" + (cart.IsinShippingcart(-8888) ? "1" : "0") + "|" + Tools.Secrecy.Escape(shoppingurl) + "|" + Tools.Secrecy.Escape(tempcontent));

                            }
                            else if (warehouse == "2")
                            {
                                dayimg = Tools.Secrecy.Escape("1|usa|" + Tools.Secrecy.Escape(getShoppingcartlist(sku)) + "|" + (curformat + " " + (cart.GetSubtotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (cart.GetSubtotal(warestock) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(warestock).ToString("F") + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((proPrice * quantity) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (ispromotionproduct ? "1" : "0") + "|" + quantity.ToString() + "|" + Math.Round((cart.GetTotal(warestock) / 10), 0).ToString() + "|" + (cart.IsinShippingcart(-8888) ? "1" : "0") + "|" + Tools.Secrecy.Escape(shoppingurl) + "|" + Tools.Secrecy.Escape(tempcontent));

                            }
                            else
                                dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + Tools.Secrecy.Escape(getShoppingcartlist(sku)) + "|" + (curformat + " " + (cart.GetSubtotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (cart.GetSubtotal(warestock) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(warestock).ToString("F") + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((proPrice * quantity) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (ispromotionproduct ? "1" : "0") + "|" + quantity.ToString() + "|" + Math.Round((cart.GetTotal(warestock) / 10), 0).ToString() + "|" + (cart.IsinShippingcart(-8888) ? "1" : "0") + "|" + Tools.Secrecy.Escape(shoppingurl) + "|" + Tools.Secrecy.Escape(tempcontent));

                        }

                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("0|Without this commodity.");
                    }

                    if (cart.IsinShippingcart(1779))
                        cart.RefreshCoupon(1779, cart.GetSupportCouponTotal("0"), "0");


                }
                else if (r == "buynow")
                {
                    dayimg = Tools.Secrecy.Escape("Sorry, you have no access to our data. Please contact: paul@liuhaimin.com");

                    GetConfigInfo configinfo = new GetConfigInfo();

                    string shoppingurl = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "shoppingcarturl"));
                    string website = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getEmailconfigfile, "website"));

                    if (shoppingurl.IndexOf("https:") < 0)
                    {
                        if (shoppingurl.IndexOf("http://") < 0)
                            shoppingurl = "http://" + shoppingurl;
                    }
                    else
                    {
                        shoppingurl = shoppingurl.Replace("https//", "https://");
                    }

                    string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
                    string sku = Tools.Tools.IsNumeric(context.Request["sku"]) ? Tools.Tools.getSafeCode(context.Request["sku"]) : "0";
                    string warestock = Tools.Tools.IsNumeric(context.Request["wt"]) ? Tools.Tools.getSafeCode(context.Request["wt"]) : "0";
                    string qty = Tools.Tools.IsNumeric(context.Request["qty"]) ? Tools.Tools.getSafeCode(context.Request["qty"]) : "1";
                    string gl = (context.Request["gl"] != null && context.Request["gl"].Length > 0) ? context.Request["gl"] : "";

                    int quantity = Int32.Parse(qty);
                    decimal proPrice = 0, limitetrackfee = 20, trackfee = 1.5M, OldPrice = 0;
                    limitetrackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "limitetrackfee")));
                    trackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "trackfee")));

                    string shortname = string.Empty, color = string.Empty, mode = string.Empty, smallpictureurl = string.Empty, leadtime = string.Empty, glsku = string.Empty, glprice = string.Empty, shopname = string.Empty, producturl = string.Empty, warehouse = "0", tempFullname = string.Empty;
                    decimal sampleprice = 0, pricemid = 0, pricemin = 0, promotionprice = 0, lostcartotal = 0, unitweight = 0, carronweight = 0, discount = 0;
                    bool instock = false, isinstock = false, isairmail = true, battery = false, issoldoutpro = false, supportcoupon = true, liquid = false, errSku = false, updatestatus = false, IsPomotion = false;
                    int promotiontype = 0, bigcategory = 0, midcategory = 0, mincategory = 0, stock = 0, Limtid_soldQty = 1;
                    DateTime starttime = DateTime.Now, endtime = DateTime.Now;
                    bool ispromotionproduct = false;

                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    DataSet ds = new DataSet();

                    //////////////////////////////////Get Product Info/////////////////////////////////////////
                    SqlConnection sqlConn = DAL.localrcon();
                    sqlConn.Open();
                    SqlCommand cmdDB = new SqlCommand(@"GetShoppingSKUInfo", sqlConn);
                    cmdDB.CommandType = CommandType.StoredProcedure;
                    cmdDB.Parameters.Add("@sku", SqlDbType.Int).Value = Int32.Parse(sku);
                    SqlDataAdapter adapt = new SqlDataAdapter(cmdDB);
                    adapt.Fill(ds, "ProductInfo");
                    ///////////////////////////////////Get Product Info///////////////////////////////////////                   

                    DataRow row = ds.Tables[0].Rows[0];
                    if (ds.Tables["ProductInfo"].Rows.Count == 1)
                    {
                        shortname = (string)row["Shortname"];
                        smallpictureurl = (string)row["smallpictureurl"];
                        leadtime = (string)row["leadtime"];
                        glsku = row["glsku"].ToString().Length > 0 ? (string)row["glsku"] : "";
                        glprice = row["glprice"].ToString().Length > 0 ? (string)row["glprice"] : "";
                        shopname = (string)row["shopname"];
                        producturl = (string)row["producturl"];
                        warehouse = (string)row["warehouse"];
                        sampleprice = (decimal)row["sampleprice"];
                        pricemid = (decimal)row["pricemid"];
                        pricemin = (decimal)row["pricemin"];
                        promotionprice = (decimal)row["promotionprice"];
                        lostcartotal = row["LostCartTotal"].ToString().Length > 0 ? (decimal)row["LostCartTotal"] : 0;
                        unitweight = (decimal)row["unitweight"];
                        carronweight = row["CartonWeight"].ToString().Length > 0 ? (decimal)row["CartonWeight"] : 10;
                        instock = (bool)row["instock"];
                        isairmail = (bool)row["isairmail"];
                        battery = (bool)row["battery"];
                        issoldoutpro = (bool)row["issoldoutpro"];
                        supportcoupon = (bool)row["supportcoupon"];
                        liquid = (bool)row["liquid"];
                        promotiontype = (int)row["promotiontype"];
                        bigcategory = (int)row["bigcategory"];
                        midcategory = (int)row["midcategory"];
                        mincategory = (int)row["mincategory"];
                        stock = (int)row["stock"];
                        if (stock > 0)
                            isinstock = true;
                        starttime = row["starttime"].ToString().Length > 0 ? Convert.ToDateTime(row["starttime"]) : DateTime.Now;
                        endtime = row["endtime"].ToString().Length > 0 ? Convert.ToDateTime(row["endtime"]) : DateTime.Now;
                        Limtid_soldQty = row["Limtid_soldQty"].ToString().Length > 0 ? (int)row["Limtid_soldQty"] : 1;
                        color = row["color"].ToString();
                        mode = row["mode"].ToString();
                        OldPrice = Convert.ToDecimal(row["msrp"]);

                        if (quantity < Limtid_soldQty)
                            quantity = Limtid_soldQty;

                        if (promotiontype == 0)
                        {
                            //正常商品
                            if (quantity >= 4)
                                proPrice = pricemin;
                            else if (quantity > 1)
                                proPrice = pricemid;
                            else
                                proPrice = sampleprice;

                            //vip price
                            if (HttpContext.Current.Session["vipdiscount"] != null && Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]) < 1)
                            {
                                discount = proPrice - proPrice * Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]);
                                proPrice = proPrice * Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]);

                            }
                            else
                                discount = OldPrice - sampleprice;


                            if (issoldoutpro == true)
                            {
                                if (stock < quantity)
                                {
                                    quantity = stock;
                                }
                                if (quantity > 0)
                                {
                                    //减库存
                                    updatestatus = BLL.ExecuteSqlFun("update products set stock=stock-" + quantity + " where sku=" + sku + ";insert TempStockList(SKU,SoldNumber,IP,WareHouse) values(" + sku + "," + quantity + ",'" + ip.ToString() + "','dt')", false);
                                }
                                else
                                {
                                    dayimg = Tools.Secrecy.Escape("0|The goods have been sold out."); //时间不对
                                    errSku = true;
                                }

                            }
                        }
                        else
                        {
                            // promotion item
                            //时间在促销期
                            if (DateTime.Compare(DateTime.Now, starttime) > 0 && DateTime.Compare(endtime, DateTime.Now) > 0)
                            {
                                if (lostcartotal > cart.GetTotal("0"))
                                {
                                    quantity = 0;
                                    dayimg = Tools.Secrecy.Escape("0|I'm sorry,  you can buy this goods only after  purchase of goods more than <strong>" + (curformat + " " + (Convert.ToDecimal(row["LostCartTotal"]) * Convert.ToDecimal(currencyrate)).ToString("F")) + "</strong> , please choose other goods first, click <a href=\"/category/Random/\">here</a> to purchase!");
                                    errSku = true;
                                }
                                else
                                {
                                    //促销商品类别分类 0 正常商品；1无限制促销商品；2一人购买一个促销；3无限制减库存促销；4一人购买一个减库存促销

                                    if (promotiontype == 3 || promotiontype == 4)
                                    {
                                        if (stock < 1)
                                        {
                                            quantity = 0;
                                            dayimg = Tools.Secrecy.Escape("0|Sorry，SKU:<FONT COLOR=\"RED\"><STRONG>" + sku.ToString() + "</STRONG></FONT> Sold out.Change SKU！<a href=\"/product/sku-" + sku.ToString() + "\">Return</a>");
                                            errSku = true;
                                        }
                                        else
                                        {
                                            if (stock < quantity)
                                                quantity = stock;
                                        }

                                        //减库存
                                        updatestatus = BLL.ExecuteSqlFun("update products set stock=stock-" + quantity + " where sku=" + sku + ";insert TempStockList(SKU,SoldNumber,IP,WareHouse) values(" + sku + "," + quantity + ",'" + ip.ToString() + "','dt')", false);
                                        ispromotionproduct = true;
                                    }

                                    if (promotiontype == 2 || promotiontype == 4)
                                    {
                                        quantity = 1;
                                        string pomotionsku = string.Empty;

                                        if (Tools.Cookie.CookieCheck("pomotionsku"))
                                        {
                                            if (Tools.Cookie.GetEncryptedCookieValue("pomotionsku").IndexOf("|" + (string)sku) < 0)
                                                pomotionsku = Tools.Cookie.GetEncryptedCookieValue("pomotionsku");
                                            else
                                            {
                                                dayimg = Tools.Secrecy.Escape("0|Sorry，SKU:<FONT COLOR=\"RED\"><STRONG>" + sku.ToString() + "</STRONG></FONT> you can buy this product only once!");
                                                errSku = true;
                                            }
                                        }
                                        else
                                            pomotionsku = "0";

                                        Tools.Cookie.SetEncryptedCookie("pomotionsku", pomotionsku + "|" + (string)sku);
                                    }

                                    proPrice = promotionprice;
                                    IsPomotion = true;
                                    tempFullname = "Pm" + promotiontype.ToString();
                                    OldPrice = sampleprice;
                                    discount = sampleprice - proPrice;
                                }
                            }
                            else
                            {
                                //促销商品没有停售且不在促销期购买则按正常商品一样
                                //正常商品
                                if (quantity >= 4)
                                    proPrice = pricemin;
                                else if (quantity > 1)
                                    proPrice = pricemid;
                                else
                                    proPrice = sampleprice;

                                //vip price
                                if (HttpContext.Current.Session["vipdiscount"] != null && Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]) < 1)
                                {
                                    discount = proPrice - proPrice * Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]);
                                    proPrice = proPrice * Convert.ToDecimal(HttpContext.Current.Session["vipdiscount"]);
                                }
                                else
                                    discount = OldPrice - sampleprice;

                                if (issoldoutpro == true)
                                {
                                    if (stock < quantity)
                                    {
                                        quantity = stock;
                                    }
                                    if (quantity > 0)
                                    {
                                        //减库存
                                        updatestatus = BLL.ExecuteSqlFun("update products set stock=stock-" + quantity + " where sku=" + sku + ";insert TempStockList(SKU,SoldNumber,IP,WareHouse) values(" + sku + "," + quantity + ",'" + ip.ToString() + "','dt')", false);
                                    }
                                    else
                                    {
                                        dayimg = Tools.Secrecy.Escape("0|The goods have been sold out."); //时间不对
                                        errSku = true;
                                    }

                                }

                            }
                        }



                        if (!errSku && quantity > 0)
                        {
                            string tempcontent = "<div class=\"detail_phone\"><strong>Phone Number:</strong> <input class=\"text phonenumber\" id=\"phonenumbers\" value=\"\" autocomplete=\"off\" type=\"text\"></div>";

                            tempcontent += "<p><strong>About color in shortage</strong></p>";

                            tempcontent += "<p><select id=\"chk_shortagecolor\">";
                            tempcontent += "    <option value=\"0\">Please Choose </option>";
                            tempcontent += "    <option value=\"1\">Contact me at once</option>";
                            tempcontent += "    <option value=\"2\">Replace it by any other colors available and inform me</option>";
                            tempcontent += "    <option value=\"3\">Refund(for items in shortage) and inform me</option>";
                            tempcontent += "    <option value=\"4\">See remarks</option>";
                            tempcontent += "</select></p>";

                            tempcontent += "<p><strong>About size in shortage</strong></p>";

                            tempcontent += "<p><select id=\"chk_shortagesize\">";
                            tempcontent += "    <option value=\"0\">Please Choose </option>";
                            tempcontent += "    <option value=\"1\">Contact me at once</option>";
                            tempcontent += "    <option value=\"2\">Replace it by any other colors available and inform me</option>";
                            tempcontent += "    <option value=\"3\">Refund(for items in shortage) and inform me</option>";
                            tempcontent += "    <option value=\"4\">See remarks</option>";
                            tempcontent += "</select></p>";

                            tempcontent += "<p><strong>About goods in shortage</strong></p>";

                            tempcontent += "<p><select id=\"chk_shortagegoods\">";
                            tempcontent += "    <option value=\"0\">Please Choose </option>";
                            tempcontent += "    <option value=\"1\">Contact me at once</option>";
                            tempcontent += "    <option value=\"2\">Replace it by any other colors available and inform me</option>";
                            tempcontent += "    <option value=\"3\">Refund(for items in shortage) and inform me</option>";
                            tempcontent += "    <option value=\"4\">See remarks</option>";
                            tempcontent += "</select></p>";

                            tempcontent += "<p><strong>Self-service order amount</strong></p>";

                            tempcontent += "<p>";
                            tempcontent += "    <input id=\"zzsb\" placeholder=\"Shop customization\" type=\"text\"></p>";

                            tempcontent += "<div class=\"quick_ordernote\">";
                            tempcontent += "    <h5>Remark to the whole order: you could input extra requirement for the whole order in below column.</h5>";
                            tempcontent += "     <textarea id=\"extrarequest\" class=\"extrarequest\" cols=\"20\" rows=\"2\"></textarea>";
                            tempcontent += "</div>";



                            if (Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "webstatus")) == "open")
                            {
                                //    string paystylestr = PageSource.PageSource.GetPublickSocur("Activipaystyle.ptshop").Trim();
                                //    string[] category1list = paystylestr.Split('\r');
                                //    foreach (string key in category1list)
                                //    {
                                //        if (key.Length > 2)
                                //        {
                                //            tempcontent += "<img src=\"" + key.Trim().Split('|')[1] + "\" class=\"checkout\" onclick=\"checkoutpaystyle('buyrule','" + key.Trim().Split('|')[2] + "','" + warestock + "')\" /><br/>";
                                //        }
                                //    }
                                tempcontent += "<img src=\"/images/paystyle/btn_paywith_primary_l.png\" class=\"checkout jumpnow\" onclick=\"checkoutpaystylebynow('buyrule','paypal','" + warehouse + "')\">";
                                tempcontent += "<div class=\"buy-now-info detail_buy_rule\"><label for=\"policy\"><input id=\"buyrule\" checked=\"\" type=\"checkbox\"><label for=\"buyrule\" class=\"f10\">I agree to the " + website + " <a href=\"/help\" target=\"_blank\">Terms and Policy</a>.</label></label></div>";
                            }
                            


                            cart.Remove("0" + row["SKU"].ToString());
                            cart.Remove(row["SKU"].ToString());

                            if (ispromotionproduct)
                                cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), color, mode, isinstock, (isinstock ? "" : "Delivery time may take 3-7 days"), (isinstock ? "Required 2-7 business days for processing" : ""), bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, OldPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false,false, isairmail, true, false, supportcoupon, IsPomotion, battery, liquid, warehouse, 3); //这 里 是 减 库 存 商 品 
                            else
                                cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), color, mode, isinstock, (isinstock ? "" : "Delivery time may take 3-7 days"), (isinstock ? "Required 2-7 business days for processing" : ""), bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, OldPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false,false, isairmail, true, false, supportcoupon, IsPomotion, battery, liquid, warehouse);


                            if (gl.Length > 0)
                            {
                                string tempglfullname = string.Empty;

                                string glskustr = (string)gl.ToString().Replace("0,", "").ToString();
                                ds = getGlskuinfo(glskustr);

                                string[] proglskulist = glsku.Split(',');
                                string[] proglskulistprice = glprice.Split(',');
                                decimal glproductprice = 0;

                                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {
                                    for (int j = 0; j < proglskulist.Length; j++)
                                    {
                                        if (ds.Tables[0].Rows[i]["sku"].ToString() == proglskulist[j].ToString())
                                        {
                                            cart.Remove(proglskulist[j]);
                                            tempglfullname = ds.Tables[0].Rows[i]["Shortname"].ToString();
                                            glproductprice = Convert.ToDecimal(proglskulistprice[j]);
                                            cart.Add(Int32.Parse(proglskulist[j]), proglskulist[j].ToString(), Int32.Parse(proglskulist[j].ToString()), "Default", "Default", true, "", "", Convert.ToInt32(ds.Tables[0].Rows[i]["BigCategory"]), Convert.ToInt32(ds.Tables[0].Rows[i]["MidCategory"]), Convert.ToInt32(ds.Tables[0].Rows[i]["MinCategory"]), Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]) - glproductprice, Int32.Parse(sku), ds.Tables[0].Rows[i]["SmallPictureURL"].ToString(), "", ds.Tables[0].Rows[i]["Shortname"].ToString(), 1, glproductprice, glproductprice, Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitWeight"]) + Convert.ToDecimal(ds.Tables[0].Rows[i]["CartonWeight"]), ds.Tables[0].Rows[i]["LeadTime"].ToString(), ds.Tables[0].Rows[i]["ProductURL"].ToString(), ds.Tables[0].Rows[i]["ShopName"].ToString(), true, false,false, Convert.ToBoolean(ds.Tables[0].Rows[i]["Isairmail"]), true, true, Convert.ToBoolean(ds.Tables[0].Rows[i]["Supportcoupon"]), false, Convert.ToBoolean(ds.Tables[0].Rows[i]["Battery"]), Convert.ToBoolean(ds.Tables[0].Rows[i]["Liquid"]), ds.Tables[0].Rows[i]["warehouse"].ToString());
                                        }
                                    }
                                }
                            }
                            cart.RefreshCoupon(1779, cart.GetSubtotal(warestock), warestock);
                            string tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warestock).Replace("selectshippingstyle","pop_selectshippingstyle"));

                            if (warehouse == "1")
                            {
                                dayimg = Tools.Secrecy.Escape("1|germany|" + Tools.Secrecy.Escape(getShoppingcartlist(sku)) + "|" + (curformat + " " + (cart.GetSubtotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (cart.GetSubtotal(warestock) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(warestock).ToString("F") + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((proPrice * quantity) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (ispromotionproduct ? "1" : "0") + "|" + quantity.ToString() + "|" + Math.Round((cart.GetTotal(warestock) / 10), 0).ToString() + "|" + (cart.IsinShippingcart(-8888) ? "1" : "0") + "|" + Tools.Secrecy.Escape(shoppingurl) + "|" + Tools.Secrecy.Escape(tempcontent));

                            }
                            else if (warehouse == "2")
                            {
                                dayimg = Tools.Secrecy.Escape("1|usa|" + Tools.Secrecy.Escape(getShoppingcartlist(sku)) + "|" + (curformat + " " + (cart.GetSubtotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (cart.GetSubtotal(warestock) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(warestock).ToString("F") + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((proPrice * quantity) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (ispromotionproduct ? "1" : "0") + "|" + quantity.ToString() + "|" + Math.Round((cart.GetTotal(warestock) / 10), 0).ToString() + "|" + (cart.IsinShippingcart(-8888) ? "1" : "0") + "|" + Tools.Secrecy.Escape(shoppingurl) + "|" + Tools.Secrecy.Escape(tempcontent));

                            }
                            else
                                dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + Tools.Secrecy.Escape(getShoppingcartlist(sku)) + "|" + (curformat + " " + (cart.GetSubtotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warestock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (cart.GetSubtotal(warestock) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warestock, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(warestock).ToString("F") + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((proPrice * quantity) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (ispromotionproduct ? "1" : "0") + "|" + quantity.ToString() + "|" + Math.Round((cart.GetTotal(warestock) / 10), 0).ToString() + "|" + (cart.IsinShippingcart(-8888) ? "1" : "0") + "|" + Tools.Secrecy.Escape(shoppingurl) + "|" + Tools.Secrecy.Escape(tempcontent));

                        }

                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("0|Without this commodity.");
                    }

                    if (cart.IsinShippingcart(1779))
                        cart.RefreshCoupon(1779, cart.GetSupportCouponTotal("0"), "0");

                   
                }
                else if (r == "addnewtag")
                {
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string tag = Tools.Tools.StripHTMLTags(context.Request["tag"]);
                    bool updatestatus = false;

                    if (tag.Length > 50)
                        tag = tag.Substring(0,50);

                    updatestatus = BLL.ExecuteSqlFun("INSERT Web_Tags(SKU,Tag) VALUES(" + sku + ",'" + tag + "')", false);
                    if (updatestatus)
                        dayimg = Secrecy.Escape("1|ok");
                    else
                        dayimg = Secrecy.Escape("0|err with you.");

                }
                else if (r == "setcookie")
                {
                    string cookiename = Tools.Tools.getSafeCode(context.Request["ckn"]);
                    string cookievalue = Tools.Tools.getSafeCode(context.Request["ckv"]);

                    try
                    {
                        Tools.Cookie.SetEncryptedCookie(cookiename, cookievalue);
                    }
                    catch { }
                    dayimg = Secrecy.Escape("1|ok");
                }
                else if (r == "addnewreview")
                {
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string review_price = Tools.Tools.getSafeCode(context.Request["review_price"]);
                    string review_Value = Tools.Tools.getSafeCode(context.Request["review_Value"]);
                    string review_Quality = Tools.Tools.getSafeCode(context.Request["review_Quality"]);
                    string review_nickname = Tools.Tools.getSafeCode(context.Request["review_nickname"]);
                    string review_content = context.Request["review_content"];
                    string useremail = "guest@ecigbuy.de";
                    if (Tools.Cookie.CookieCheck("useremail"))
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    SqlConnection sqlConn = DAL.localwcon();

                    SqlCommand cmdDB = new SqlCommand(@"", sqlConn);

                    cmdDB = new SqlCommand(@"INSERT Web_Reviews([SKU]
      ,[Email]
      ,[Nick]
      ,[Price]
      ,[value]
      ,[quality]
      ,[Timestamp]
      ,[Good]
      ,[BAD]
      ,[Contents]
      ,[ShowStatus]
      ,[GetPoint])
      values(@SKU
      ,@Email
      ,@Nick
      ,@Price
      ,@value
      ,@quality
      ,@Timestamp
      ,@Good
      ,@BAD
      ,@Contents
      ,@ShowStatus
      ,@GetPoint)", sqlConn);

                    cmdDB.Parameters.Add("@SKU", SqlDbType.Int).Value = Int32.Parse(sku);
                    cmdDB.Parameters.Add("@Email", SqlDbType.NVarChar).Value = useremail;
                    cmdDB.Parameters.Add("@Nick", SqlDbType.NVarChar).Value = review_nickname;
                    cmdDB.Parameters.Add("@Price", SqlDbType.Int).Value = Int32.Parse(review_price);
                    cmdDB.Parameters.Add("@value", SqlDbType.Int).Value = Int32.Parse(review_Value);
                    cmdDB.Parameters.Add("@quality", SqlDbType.Int).Value = Int32.Parse(review_Quality);
                    cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
                    cmdDB.Parameters.Add("@Good", SqlDbType.Int).Value = 0;
                    cmdDB.Parameters.Add("@BAD", SqlDbType.Int).Value = 0;
                    cmdDB.Parameters.Add("@Contents", SqlDbType.VarChar).Value = review_content;
                    cmdDB.Parameters.Add("@ShowStatus", SqlDbType.Bit).Value = false;
                    cmdDB.Parameters.Add("@GetPoint", SqlDbType.Int).Value = 2;

                    cmdDB.Connection.Open();
                    cmdDB.ExecuteNonQuery();
                    cmdDB.Connection.Close();
                    dayimg = Tools.Secrecy.Escape("1|ok");

                }
                else if (r == "deletesku")
                {
                    GetConfigInfo configinfo = new GetConfigInfo();

                    dayimg = Tools.Secrecy.Escape("0|err");
                    string sku = Tools.Tools.IsNumeric(context.Request["sku"]) ? context.Request["sku"] : "-9999999999";
                    string areastock = Tools.Tools.IsNumeric(context.Request["areastock"]) ? context.Request["areastock"] : "0";
                    bool deletestatuserr = false;
                    bool isglsku = false;
                    int promotionsku = 0;
                    int glsku = -10000;
                    int mainglsku = -1000;
                    decimal limitetrackfee = 20, trackfee = 1.5M;

                    limitetrackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "limitetrackfee")));
                    trackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "trackfee")));

                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    if (Int32.Parse(sku) == 1778 || Int32.Parse(sku) == 1780 || Int32.Parse(sku) == 1779 || Int32.Parse(sku) == 1666) //1666 luckmoney code
                    {
                        cart.Remove(sku + "coupon");
                    }
                    else if (Int32.Parse(sku) == -8888)
                    {
                        cart.Remove("-8888");
                    }
                    else
                    {
                        bool ispromotion = false;
                        decimal myskuprice = 0;
                        string temppomotionSku = "0";
                        for (int i = 0; i < cart.Items.Count; i++)
                        {
                            if (cart.Items[i].IsPromotion)
                            {
                                ispromotion = true;
                                temppomotionSku += "," + cart.Items[i].SKU.ToString();
                            }

                            if (cart.Items[i].SKU == Int32.Parse(sku))
                            {
                                myskuprice = cart.Items[i].Price;
                                isglsku = cart.Items[i].IsGLsku;
                                glsku = cart.Items[i].GLsku;
                            }
                            else
                            {
                                if (cart.Items[i].GLsku > 2999)
                                    mainglsku = cart.Items[i].GLsku;
                            }

                        }

                        decimal lostprice = 0M;
                        SqlDataReader reader = BLL.ReturnValue("select top 1 LostPoPrice,sku from Products where SKU in(" + temppomotionSku + ") order by LostPoPrice desc", false);
                        if (reader.Read())
                        {
                            lostprice = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            promotionsku = reader.GetInt32(1);
                        }
                        reader.Close();

                        if (isglsku)
                        {
                            if (cart.IsinShippingcart(glsku))
                            {
                                deletestatuserr = true;
                                promotionsku = glsku;
                            }
                        }
                        else
                        {
                            if (ispromotion && mainglsku.ToString() != sku)
                            {
                                if (cart.GetItemCount(areastock) > 1)
                                {
                                    if (lostprice > cart.GetSubtotalNoPm(areastock) - myskuprice)
                                        deletestatuserr = true;
                                    else
                                        cart.Remove(sku);
                                }
                                else
                                    cart.Remove(sku);
                            }
                            else
                                cart.Remove(sku);
                        }

                        cart.RemoveGlsku(Int32.Parse(sku), areastock);
                        cart.RemoveGlsku(Int32.Parse(sku), areastock);
                        cart.RemoveGlsku(Int32.Parse(sku), areastock);
                        cart.RemoveGlsku(Int32.Parse(sku), areastock);
                    }

                    if (cart.IsinShippingcart(1779))
                    {
                        cart.RefreshCoupon(1779, cart.GetSubtotalNoPm(areastock), areastock);
                    }

                    if (cart.IsinShippingcart(1778))
                    {
                        cart.RefreshCouponbysku(1778, cart.GetSubtotalNoPm(areastock), areastock);
                    }

                    string tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle(areastock));


                    deleteCartlogSKU(sku);


                    if (deletestatuserr)
                        dayimg = Tools.Secrecy.Escape("0|Sorry, can not delete this product. Please delete SKU:" + promotionsku + " first");
                    else
                        dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + (curformat + " " + (cart.GetSubtotal(areastock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((-cart.GetSubDiscount(areastock)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(areastock) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (cart.GetTotal(areastock) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(areastock, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(areastock).ToString("F") + "|" + Math.Round((cart.GetTotal(areastock) / 10), 0).ToString());


                }
                else if (r == "clearcart")
                {
                    dayimg = Tools.Secrecy.Escape("0|err");

                    HttpContext.Current.Session.Clear();

                    dayimg = Tools.Secrecy.Escape("1|ok");
                }
                else if (r == "getiptocountry")
                {
                    dayimg = Tools.Secrecy.Escape("1|OR-OTHERS");

                    string IP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
                    try
                    {
                        IPSearch finder = new IPSearch(HttpContext.Current.Server.MapPath("/ipdata/qqzeng-ip-utf8.dat"));
                        string result = finder.Query(IP);
                        dayimg = Tools.Secrecy.Escape("1|" + result.Split('|')[8].Trim() + "-" + result.Split('|')[7].Trim().ToUpper());
                    }
                    catch { }
                }
                else if (r == "uploadwishlist")
                {
                    string skulist = Tools.Tools.getSafeCode(context.Request["skulist"]).Replace(",,", ",").Replace(",,", ",").Replace(",,", ",").Replace(",,", ",").Replace(",,", ",").Replace(",,", ",").Replace(",,", ",").Replace(",,", ",").Replace(",,", ",").Replace(",,", ",").Replace(",,", ",");
                    string email = string.Empty;
                    if (skulist != "null")
                    {
                        ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                        if (Tools.Cookie.CookieCheck("useremail"))
                            email = Tools.Cookie.GetEncryptedCookieValue("useremail");

                        string cartskulist = "0", warehouser = "0";

                        if (cart.GetItemCount(warehouser) > 0)
                        {
                            for (int i = 0; i < cart.GetItemCount(warehouser); i++)
                            {
                                cartskulist += "," + cart.Items[i].SKU.ToString();
                            }
                        }

                        if (email.Length > 5)
                        {
                            bool updateStatus = false;
                            if (cartskulist.Length > -1)
                                updateStatus = BLL.ExecuteSqlFun("update ACT_Accounts set CartSKUlist='" + cartskulist + "' where Email='" + email + "'", false);

                            if (cartskulist.Length > -1)
                                updateStatus = BLL.ExecuteSqlFun("update ACT_Accounts set WishSKUlist='" + skulist + "' where Email='" + email + "'", false);
                            dayimg = Tools.Secrecy.Escape("1|ok");
                        }
                        else
                            dayimg = Tools.Secrecy.Escape("0|err");
                    }
                    else
                        dayimg = Tools.Secrecy.Escape("0|err");
                }
                else if (r == "seteightstatus")
                {
                    dayimg = Tools.Secrecy.Escape("0|err");
                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);
                    cart.originalReferer = "1";
                    dayimg = Tools.Secrecy.Escape("1|ok");
                }
                else if (r == "addspecialsku")
                {
                    GetConfigInfo configinfo = new GetConfigInfo();
                    dayimg = Tools.Secrecy.Escape("Sorry, you have no access to our data. Please contact: paul@liuhaimin.com");
                    string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
                    string sku = Tools.Tools.IsNumeric(context.Request["sku"]) ? Tools.Tools.getSafeCode(context.Request["sku"]) : "0";
                    string qty = Tools.Tools.IsNumeric(context.Request["qty"]) ? Tools.Tools.getSafeCode(context.Request["qty"]) : "1";

                    int quantity = Int32.Parse(qty);
                    decimal proPrice = 0, limitetrackfee = 20, trackfee = 1.5M;

                    limitetrackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "limitetrackfee")));
                    trackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "trackfee")));

                    string shortname = string.Empty, smallpictureurl = string.Empty, leadtime = string.Empty, glsku = string.Empty, glprice = string.Empty, shopname = string.Empty, producturl = string.Empty, warehouse = "0", tempFullname = string.Empty;
                    decimal sampleprice = 0, pricemid = 0, pricemin = 0, promotionprice = 0, lostcartotal = 0, unitweight = 0, carronweight = 0, discount = 0;
                    bool instock = false, isairmail = true, battery = false, issoldoutpro = false, IsusuPro = true, supportcoupon = true, liquid = false, errSku = false, updatestatus = false, IsPomotion = false;
                    int promotiontype = 0, bigcategory = 0, midcategory = 0, mincategory = 0, stock = 0;
                    DateTime starttime = DateTime.Now, endtime = DateTime.Now;

                    warehouse = Tools.Tools.IsNumeric(context.Request["wt"]) ? Tools.Tools.getSafeCode(context.Request["wt"]) : "0";

                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    cart.Remove(sku);

                    switch (sku)
                    {
                        case "-8888":
                            smallpictureurl = "/images/-8888.jpg";
                            shortname = "Paid Package Track Number";
                            if (cart.GetTotal(warehouse) >= limitetrackfee)
                                proPrice = 0;
                            else
                                proPrice = 1.5M;

                            if (warehouse == "3")
                            {
                                trackfee = 2;
                                proPrice = 2;
                            }

                            producturl = "javascript(0);";
                            IsusuPro = false;
                            break;
                        case "1888":
                            smallpictureurl = "/images/1888.jpg";
                            shortname = "Dropshipping";
                            proPrice = 0;
                            producturl = "javascript(0);";
                            IsusuPro = false;
                            break;

                    }

                    cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false,false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);

                    string tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warehouse));
                    dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + Tools.Secrecy.Escape(getTuiitemlist(sku)) + "|" + (curformat + " " + (cart.GetSubtotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warehouse)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (cart.GetTotal(warehouse) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(warehouse).ToString("F") + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + ((proPrice * quantity) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + smallpictureurl + "|" + shortname + "|" + producturl);

                }
                else if (r == "addcoupon")
                {
                    GetConfigInfo configinfo = new GetConfigInfo();
                    dayimg = Tools.Secrecy.Escape("Sorry, you have no access to our data. Please contact: paul@liuhaimin.com");
                    string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
                    string couponcode = Tools.Tools.getSafeCode(context.Request["couponcode"].Trim());

                    int quantity = 1;
                    decimal proPrice = 0, limitetrackfee = 20, trackfee = 1.5M;

                    limitetrackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "limitetrackfee")));
                    trackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "trackfee")));

                    string sku = "-999999", shortname = string.Empty, smallpictureurl = string.Empty, leadtime = string.Empty, glsku = string.Empty, glprice = string.Empty, shopname = string.Empty, producturl = string.Empty, warehouse = "0", tempFullname = string.Empty;
                    decimal sampleprice = 0, pricemid = 0, pricemin = 0, promotionprice = 0, lostcartotal = 0, unitweight = 0, carronweight = 0, discount = 0;
                    bool instock = false, isairmail = true, battery = false, issoldoutpro = false, IsusuPro = true, supportcoupon = true, liquid = false, errSku = false, updatestatus = false, IsPomotion = false;
                    int promotiontype = 0, bigcategory = 0, midcategory = 0, mincategory = 0, stock = 0;
                    DateTime starttime = DateTime.Now, endtime = DateTime.Now;
                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    warehouse = Tools.Tools.getSafeCode(context.Request["wt"]);


                    DataSet ds = new DataSet();
                    ds = BLL.ReturnDataSet("select top 1 CouponNumber,Coupontype,LostPrice,CouponValue,lostPrice2,CouponValue2,CouponEmail,Ispaid,Startimeamp,Endtimeamp,IsUSD,SupportCategory,SupportSku,StoreName from Web_Coupon where CouponNumber='" + couponcode + "' and CouponValue>0", false);

                    if (ds.Tables[0].Rows.Count == 1)
                    {
                        int coupontype = Convert.ToInt32(ds.Tables[0].Rows[0]["Coupontype"]);                   //优惠卷类别
                        decimal LostPrice = Convert.ToDecimal(ds.Tables[0].Rows[0]["LostPrice"]);               //一级最少消费要求
                        decimal CouponValue = Convert.ToDecimal(ds.Tables[0].Rows[0]["CouponValue"]);
                        decimal lostPrice2 = Convert.ToDecimal(ds.Tables[0].Rows[0]["lostPrice2"]);
                        decimal CouponValue2 = Convert.ToDecimal(ds.Tables[0].Rows[0]["CouponValue2"]);
                        string CouponEmail = Convert.ToString(ds.Tables[0].Rows[0]["CouponEmail"]);
                        bool Ispaid = Convert.ToBoolean(ds.Tables[0].Rows[0]["Ispaid"]);
                        DateTime Startimeamp = Convert.ToDateTime(ds.Tables[0].Rows[0]["Startimeamp"]);
                        DateTime Endtimeamp = Convert.ToDateTime(ds.Tables[0].Rows[0]["Endtimeamp"]);
                        bool IsUSD = Convert.ToBoolean(ds.Tables[0].Rows[0]["IsUSD"]);
                        string SupportType = Convert.ToString(ds.Tables[0].Rows[0]["SupportCategory"]);
                        string SupportSku = Convert.ToString(ds.Tables[0].Rows[0]["SupportSku"]);
                        string StoreName = Convert.ToString(ds.Tables[0].Rows[0]["StoreName"]);
                        shortname = "Coupon Code " + couponcode;
                        IsusuPro = false;
                        shopname = StoreName;
                        decimal ocouponvalue = CouponValue;
                        decimal resultValue = (cart.GetSupportCouponTotal(warehouse) >= lostPrice2 ? CouponValue2 : CouponValue);
                        discount = resultValue;
                        string tempgetshippingstyleinfo = string.Empty;


                        if (resultValue < 1 && resultValue > 0)
                        {
                            resultValue = (1M - resultValue) * cart.GetSupportCouponTotal(warehouse);
                        }

                        DateTime NowTime = DateTime.Now;
                        //1,抵金优惠卷,无最低消费限制,可由客人购买或客服送给客人做store credit(客人消费完毕为止,可以抵金和paypal共用)
                        //2,多人共用coupon,可设置比率或是消费金额给相应折扣
                        //3,单品折扣coupon ,可以对单个商品进行折扣coupon设置使用此coupon可以得到百分比或是消费多少给相应折扣金额优惠
                        //4,类别折扣coupon,可以设置类别下属SKU得到百分比或是消费多少给相应折扣优惠
                        //5,一次性优惠卷
                        smallpictureurl = "/images/" + sku + ".jpg";
                        proPrice = 0;
                        producturl = "javascript:void(0);";
                        IsusuPro = false;

                        if (cart.GetSupportCouponTotal(warehouse) >= LostPrice)
                        {
                            switch (coupontype)
                            {
                                case 1://抵金优惠卷,无最低消费限制,可由客人购买或客服送给客人做store credit(客人消费完毕为止,可以抵金和paypal共用)
                                    if (resultValue <= 0)
                                    {
                                        dayimg = Tools.Secrecy.Escape("0|Sorry, preferential volumes have been used.");
                                    }
                                    else
                                    {
                                        if (resultValue >= cart.GetSupportCouponTotal(warehouse))
                                            resultValue = cart.GetSupportCouponTotal(warehouse);

                                        proPrice = -resultValue;
                                        sku = "1780";
                                        smallpictureurl = "/images/" + sku + ".jpg";
                                        if (cart.IsinShippingcart(1780))
                                        {
                                            dayimg = Tools.Secrecy.Escape("0|Discount code already exists");
                                        }
                                        else
                                        {
                                            if (resultValue > 0)
                                            {
                                                cart.Remove(sku.ToString() + "coupon");
                                                cart.Add(Int32.Parse(sku), sku + "coupon", Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, couponcode, tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                cart.UpdateCouponValue(couponcode, resultValue);
                                            }

                                            if (cart.GetTotal(warehouse) >= limitetrackfee)
                                            {
                                                cart.Remove("-8888");
                                                sku = "-8888";
                                                smallpictureurl = "/images/-8888.jpg";
                                                shortname = "Paid Package Track Number";
                                                proPrice = 0;
                                                producturl = "javascript:void(0);";
                                                IsusuPro = false;

                                                if (cart.GetSupportCouponTotal(warehouse) >= limitetrackfee)
                                                {
                                                    cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                }
                                                else
                                                {
                                                    proPrice = trackfee;
                                                    cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                }
                                            }
                                            tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warehouse));
                                            dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + getConfig.imgPath() + "/images/" + sku + ".jpg" + "|" + couponcode + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubtotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubDiscount(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + cart.GetSubtotal(warehouse).ToString("F") + "|" + (cart.GetTotal(warehouse) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warehouse, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + Math.Round((cart.GetTotal(warehouse) / 10), 0).ToString());
                                        }
                                    }
                                    break;
                                case 2://多人共用coupon,可设置比率或是消费金额给相应折扣
                                    if (IsUSD)
                                        sku = "1778";
                                    else
                                        sku = "1779";
                                    smallpictureurl = "/images/" + sku + ".jpg";
                                    if (cart.IsinShippingcart(Int32.Parse(sku)))
                                    {
                                        dayimg = Tools.Secrecy.Escape("0|Discount code already exists");
                                    }
                                    else
                                    {
                                        if (DateTime.Compare(NowTime, Startimeamp) > 0 || DateTime.Compare(Endtimeamp, NowTime) < 0)
                                        {
                                            if (cart.GetSubtotal(warehouse) >= resultValue)
                                            {
                                                //优惠卷可用
                                                cart.Remove(sku.ToString() + "coupon");

                                                proPrice = -resultValue;

                                                if (resultValue > 0)
                                                {
                                                    cart.Add(Int32.Parse(sku), sku + "coupon", Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, couponcode, tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                }

                                                if (cart.IsinShippingcart(-8888))
                                                {
                                                    cart.Remove("-8888");
                                                    sku = "-8888";
                                                    smallpictureurl = "/images/-8888.jpg";
                                                    shortname = "Paid Package Track Number";
                                                    proPrice = 0;
                                                    producturl = "javascript:void(0);";
                                                    IsusuPro = false;

                                                    if (cart.GetSupportCouponTotal(warehouse) >= limitetrackfee)
                                                    {
                                                        cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, 0, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                    }
                                                    else
                                                    {
                                                        proPrice = trackfee;
                                                        cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, 0, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                    }
                                                }
                                                tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warehouse));
                                                dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + getConfig.imgPath() + "/images/" + sku + ".jpg" + "|Coupon Code " + couponcode + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubtotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubDiscount(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + cart.GetSubtotal(warehouse).ToString("F") + "|" + (cart.GetTotal(warehouse) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warehouse, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + Math.Round((cart.GetTotal(warehouse) / 10), 0).ToString());
                                            }
                                            else
                                            {
                                                dayimg = Tools.Secrecy.Escape("0|Your discount amount does not meet the requirements.");
                                            }
                                        }
                                        else
                                        {
                                            dayimg = Tools.Secrecy.Escape("0|Coupon is not valid.");
                                        }
                                    }
                                    break;
                                case 3://单品折扣coupon ,可以对单个商品进行折扣coupon设置使用此coupon可以得到百分比或是消费多少给相应折扣金额优惠
                                    if (cart.GetSubtotalBySkuList(SupportSku, warehouse) < 0.01M)
                                    {
                                        dayimg = Tools.Secrecy.Escape("0|Without this coupon support commodity.");
                                    }
                                    else
                                    {
                                        if (IsUSD)
                                            sku = "1778";
                                        else
                                            sku = "1779";
                                        if (cart.IsinShippingcart(Int32.Parse(sku)))
                                        {
                                            dayimg = Tools.Secrecy.Escape("0|Discount code already exists");
                                        }
                                        else
                                        {
                                            if (DateTime.Compare(NowTime, Startimeamp) > 0 || DateTime.Compare(Endtimeamp, NowTime) < 0)
                                            {
                                                //优惠卷可用
                                                if (sku == "1779")
                                                {
                                                    resultValue = (cart.GetSupportCouponTotal(warehouse) >= lostPrice2 ? CouponValue2 : CouponValue);
                                                    resultValue = cart.GetSubtotalBySkuList(SupportSku, warehouse) * (1 - resultValue);
                                                }
                                                if (resultValue > 0)
                                                {
                                                    cart.Remove(sku.ToString() + "coupon");
                                                    shortname = "SkuCoupon|" + SupportSku;
                                                    proPrice = -resultValue;
                                                    cart.Add(Int32.Parse(sku), sku + "coupon", Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, couponcode, tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                }

                                                if (cart.IsinShippingcart(-8888))
                                                {
                                                    cart.Remove("-8888");
                                                    sku = "-8888";
                                                    smallpictureurl = "/images/-8888.jpg";
                                                    shortname = "Paid Package Track Number";
                                                    proPrice = 0;
                                                    producturl = "javascript:void(0);";
                                                    IsusuPro = false;

                                                    if (cart.GetSupportCouponTotal(warehouse) >= limitetrackfee)
                                                    {
                                                        cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, 0, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                    }
                                                    else
                                                    {
                                                        proPrice = trackfee;
                                                        cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, 0, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                    }
                                                }
                                                tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warehouse));
                                                dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + getConfig.imgPath() + "/images/" + sku + ".jpg" + "|Coupon Code " + couponcode + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubtotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubDiscount(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + cart.GetSubtotal(warehouse).ToString("F") + "|" + (cart.GetTotal(warehouse) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warehouse, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + Math.Round((cart.GetTotal(warehouse) / 10), 0).ToString());
                                            }
                                            else
                                            {
                                                dayimg = Tools.Secrecy.Escape("0|Coupon expires.");
                                            }
                                        }
                                    }
                                    break;
                                case 4://类别折扣coupon,可以设置类别下属SKU得到百分比或是消费多少给相应折扣优惠
                                    if (cart.GetSubtotalByCategory(SupportType, warehouse) < 0.01M)
                                    {
                                        dayimg = Tools.Secrecy.Escape("0|Coupon is not valid.");
                                    }
                                    else
                                    {
                                        if (IsUSD)
                                            sku = "1778";
                                        else
                                            sku = "1779";

                                        NowTime = DateTime.Now; //err 这里可能有问题
                                        if (cart.IsinShippingcart(Int32.Parse(sku)))
                                        {
                                            dayimg = Tools.Secrecy.Escape("0|Discount code already exists");
                                        }
                                        else
                                        {
                                            if (DateTime.Compare(NowTime, Startimeamp) > 0 || DateTime.Compare(Endtimeamp, NowTime) < 0)
                                            {
                                                //优惠卷可用

                                                if (sku == "1779")
                                                {
                                                    resultValue = (cart.GetSupportCouponTotal(warehouse) >= lostPrice2 ? CouponValue2 : CouponValue);
                                                    resultValue = cart.GetSubtotalByCategory(SupportType, warehouse) * (1 - resultValue);
                                                }

                                                if (resultValue > 0)
                                                {
                                                    cart.Remove(sku.ToString() + "coupon");
                                                    shortname = "TypeCoupon|" + SupportType;
                                                    proPrice = -resultValue;
                                                    cart.Add(Int32.Parse(sku), sku + "coupon", Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, couponcode, tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                }

                                                if (cart.IsinShippingcart(-8888))
                                                {
                                                    cart.Remove("-8888");
                                                    sku = "-8888";
                                                    smallpictureurl = "/images/-8888.jpg";
                                                    shortname = "Paid Package Track Number";
                                                    proPrice = 0;
                                                    producturl = "javascript:void(0);";
                                                    IsusuPro = false;

                                                    if (cart.GetSupportCouponTotal(warehouse) >= limitetrackfee)
                                                    {
                                                        cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, 0, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                    }
                                                    else
                                                    {
                                                        proPrice = trackfee;
                                                        cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, 0, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                    }
                                                }
                                                tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warehouse));
                                                dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + getConfig.imgPath() + "/images/" + sku + ".jpg" + "|Coupon Code " + couponcode + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubtotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubDiscount(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + cart.GetSubtotal(warehouse).ToString("F") + "|" + (cart.GetTotal(warehouse) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warehouse, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + Math.Round((cart.GetTotal(warehouse) / 10), 0).ToString());
                                            }
                                            else
                                            {
                                                dayimg = Tools.Secrecy.Escape("0|Coupon expires.");
                                            }
                                        }
                                    }
                                    break;
                                case 5://一次性优惠卷
                                    if (IsUSD)
                                        sku = "1778";
                                    else
                                        sku = "1779";
                                    if (cart.IsinShippingcart(Int32.Parse(sku)))
                                    {
                                        dayimg = Tools.Secrecy.Escape("0|Discount code already exists");
                                    }
                                    else
                                    {
                                        if (DateTime.Compare(NowTime, Startimeamp) > 0 || DateTime.Compare(Endtimeamp, NowTime) < 0)
                                        {
                                            if (Ispaid)
                                            {
                                                dayimg = Tools.Secrecy.Escape("0|The coupon is invalid.");
                                            }
                                            else
                                            {
                                                if (CouponEmail.Length > 3)
                                                {
                                                    string email = string.Empty;
                                                    if (Tools.Cookie.CookieCheck("useremail"))
                                                        email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                                                    else
                                                        email = "guest";

                                                    if (email.ToLower() == CouponEmail.ToLower())
                                                    {
                                                        if (resultValue > 0)
                                                        {
                                                            cart.Remove(sku.ToString() + "coupon");
                                                            proPrice = -resultValue;
                                                            cart.Add(Int32.Parse(sku), sku + "coupon", Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, couponcode, tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false,false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);

                                                            if (cart.IsinShippingcart(-8888))
                                                            {
                                                                cart.Remove("-8888");
                                                                sku = "-8888";
                                                                smallpictureurl = "/images/-8888.jpg";
                                                                shortname = "Paid Package Track Number";
                                                                proPrice = 0;
                                                                producturl = "javascript:void(0);";
                                                                IsusuPro = false;
                                                                if (cart.GetSupportCouponTotal(warehouse) >= limitetrackfee)
                                                                {
                                                                    cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, 0, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                                }
                                                                else
                                                                {
                                                                    proPrice = trackfee;
                                                                    cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, 0, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                                }
                                                            }
                                                            tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warehouse));
                                                            dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + getConfig.imgPath() + "/images/" + sku + ".jpg" + "|Coupon Code " + couponcode + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubtotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubDiscount(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + cart.GetSubtotal(warehouse).ToString("F") + "|" + (cart.GetTotal(warehouse) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warehouse, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + Math.Round((cart.GetTotal(warehouse) / 10), 0).ToString());
                                                            updatestatus = BLL.ExecuteSqlFun("update Web_Coupon set Ispaid=1 where CouponNumber='" + couponcode + "'", false);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        dayimg = Tools.Secrecy.Escape("0|You have no right to use this coupon.");
                                                    }
                                                }
                                                else
                                                {
                                                    if (resultValue > 0)
                                                    {
                                                        //优惠卷可用
                                                        cart.Remove(sku.ToString() + "coupon");
                                                        proPrice = -resultValue;
                                                        cart.Add(Int32.Parse(sku), sku + "coupon", Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, couponcode, tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false,false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);

                                                        if (cart.IsinShippingcart(-8888))
                                                        {
                                                            cart.Remove("-8888");
                                                            sku = "-8888";
                                                            smallpictureurl = "/images/-8888.jpg";
                                                            shortname = "Paid Package Track Number";
                                                            proPrice = 0;
                                                            producturl = "javascript:void(0);";
                                                            IsusuPro = false;

                                                            if (cart.GetSupportCouponTotal(warehouse) >= limitetrackfee)
                                                            {
                                                                cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, 0, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                            }
                                                            else
                                                            {
                                                                proPrice = trackfee;
                                                                cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, 0, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, quantity > 1 ? false : true, false, false, isairmail, IsusuPro, false, supportcoupon, IsPomotion, battery, liquid, warehouse);
                                                            }
                                                        }

                                                        tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warehouse));
                                                        dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + getConfig.imgPath() + "/images/" + sku + ".jpg" + "|Coupon Code " + couponcode + "|" + (curformat + " " + (proPrice * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubtotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetShippingTotal() * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetSubDiscount(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + (curformat + " " + (cart.GetTotal(warehouse) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + cart.GetSubtotal(warehouse).ToString("F") + "|" + (cart.GetTotal(warehouse) > limitetrackfee ? "Free" : (curformat + " " + (trackfee * Convert.ToDecimal(currencyrate)).ToString("F"))) + "|" + (curformat + " " + ((-cart.GetSubDiscount(warehouse, 1779)) * Convert.ToDecimal(currencyrate)).ToString("F")) + "|" + tempgetshippingstyleinfo + "|" + Math.Round((cart.GetTotal(warehouse) / 10), 0).ToString());
                                                        updatestatus = BLL.ExecuteSqlFun("update Web_Coupon set Ispaid=1 where CouponNumber='" + couponcode + "'", false);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            dayimg = Tools.Secrecy.Escape("0|Coupon expires.");
                                        }
                                    }
                                    break;
                            }

                        }
                        else
                        {
                            dayimg = Tools.Secrecy.Escape("0|Sorry, you buy the goods do not support this coupon, you should at least be spending " + (curformat + " " + (LostPrice * Convert.ToDecimal(currencyrate)).ToString("F")));
                        }

                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("0|Without this coupon.");
                    }

                }
                else if (r == "getagestatusbysession")
                {
                    dayimg = Tools.Secrecy.Escape("0|err");
                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);
                    string returnvalue = "";
                    try { returnvalue = cart.originalReferer; }
                    catch { }
                    if (returnvalue == "1")
                        dayimg = Tools.Secrecy.Escape("1|ok");

                }
                else if (r == "setcurrency")
                {
                    dayimg = Tools.Secrecy.Escape("0|err");

                    string curreneystr = PageSource.PageSource.GetPublickSocur("curreney.ptshop").Trim();
                    string[] curreneylist = curreneystr.Split('\r');

                    string curreneyrate = "610";

                    string cur = Tools.Tools.getSafeCode(context.Request["cur"]).Trim();
                    string fstr = Tools.Tools.getSafeCode(context.Request["fstr"]).Trim();

                    for (int i = 0; i < curreneylist.Length; i++)
                    {
                        if (curreneylist[i].Split('|')[0].Trim().ToLower() == cur.Trim().ToLower())
                        {
                            curreneyrate = curreneylist[i].Split('|')[2];
                            break;
                        }
                    }

                    Tools.Cookie.SetEncryptedCookie("currency", cur); //usd
                    Tools.Cookie.SetEncryptedCookie("curformat", fstr); //$
                    Tools.Cookie.SetEncryptedCookie("currencyrate", curreneyrate); //rate
                    dayimg = Tools.Secrecy.Escape("1|ok");
                }
                else if (r == "getmypassword")
                {
                    GetConfigInfo configinfo = new GetConfigInfo();
                    string website = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getEmailconfigfile, "website"));
                    string email = Tools.Tools.getSafeCode(context.Request["email"]);
                    string returnpassword = string.Empty;
                    string nick = "Friend";
                    SqlDataReader reader = BLL.ReturnValue("select email,Password,Nick from ACT_Accounts where EMail='" + email + "'", false);
                    bool istrueemail = false;
                    if (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false)
                            istrueemail = true;
                        returnpassword = reader.GetString(1);

                        if (reader.IsDBNull(2) == false && reader.GetString(2).Length > 0)
                            nick = reader.GetString(2);
                    }
                    reader.Close();

                    if (istrueemail)
                    {
                        try
                        {
                            //Email.EmailSend.getPassword(email, null, nick, returnpassword, "Get your password from " + website);
                            dayimg = Tools.Secrecy.Escape("1|Password has been sent successfully, please according to the requirements of the operation.");
                        }
                        catch
                        {
                            dayimg = Tools.Secrecy.Escape("0|Mail server, please try again later.");
                        }
                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("0|Sorry, you have not registered as a member,<a href=\"/login\">Registration</a> now");
                    }

                }
                else if (r == "getactorderlist")
                {
                    string pagenow = Tools.Tools.getSafeCode(context.Request["pagenow"]);
                    string orderstatus = Tools.Tools.getSafeCode(context.Request["orderstatus"]);
                    string actOrder_startime = Tools.Tools.getSafeCode(context.Request["actOrder_startime"]);
                    string actOrder_endtime = Tools.Tools.getSafeCode(context.Request["actOrder_endtime"]);
                    string wherewsql = string.Empty;


                    string useremail = "IDSFGKWET475IRFIRE8I";

                    if (Tools.Cookie.CookieCheck("useremail"))
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    string outdatalist = string.Empty;

                    wherewsql = " and EMail='" + useremail + "'";

                    if (orderstatus != "-9999")
                        wherewsql += " and OrderStatus=" + orderstatus + "";

                    if (actOrder_startime.Length > 0 || actOrder_endtime.Length > 0)
                    {
                        if (actOrder_startime.Length > 0)
                            wherewsql += " and Timestamp >= '" + actOrder_startime + "'";

                        if (actOrder_endtime.Length > 0)
                            wherewsql += " and Timestamp <= '" + actOrder_endtime + "'";
                    }


                    int totalCounts = 0;

                    SqlDataReader reader = BLL.ReturnValue("select count(*) from View_OrderWithStatus where email!=''" + wherewsql, false);
                    if (reader.Read())
                    {
                        totalCounts = reader.GetInt32(0);
                    }
                    reader.Close();




                    DataSet ds = new DataSet();

                    Model m = new Model();
                    m.DateTable = "View_OrderWithStatus";
                    m.soryType = "ID DESC";
                    m.KeyTitle = "id";
                    m.pagesize = 10;
                    m.pageindex = pagenow;
                    m.SearchByTitle = "email";
                    m.SearchByType = "!=";
                    m.keyword = "";
                    m.WhereSQL = wherewsql;

                    ds = new DataSet();
                    ds = BLL.ReturnTongYong("TongYongFenYe", m);

                    if (totalCounts == 0)
                        totalCounts = BLL.getTongYongCount("TongYongFenYeCount", m);

                    string functionstr = string.Empty;
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        dayimg = "1";
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            functionstr = string.Empty;
                            switch (Convert.ToInt32(ds.Tables[0].Rows[i]["OrderStatus"]))
                            {
                                case -1:
                                    functionstr = "<a href=\"/submitpayment/" + ds.Tables[0].Rows[i]["ordernumber"] + "\" target=\"_blank\">Pay for this order</a>";
                                    break;
                                case 20:
                                    if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(ds.Tables[0].Rows[i]["timestamp"]).AddDays(5)) > 0)
                                        functionstr = "<strong><a href=\"javascript:sosorder('" + ds.Tables[0].Rows[i]["ordernumber"] + "')\">Order Prioritized</a></strong>";
                                    break;
                                case -6:
                                    functionstr = "<a href=\"/submitpayment/" + ds.Tables[0].Rows[i]["ordernumber"] + "\" target=\"_blank\">Pay for this order</a>";
                                    break;
                                case 15:
                                    functionstr = "<strong><a href=\"javascript:sosorder('" + ds.Tables[0].Rows[i]["ordernumber"] + "')\">Order Prioritized</a></strong>";
                                    break;
                                case 50:
                                    functionstr = "<strong><a href=\"javascript:trackorder('" + ds.Tables[0].Rows[i]["ordernumber"] + "')\">Order track</a></strong>";
                                    break;
                                case 60:
                                    functionstr = "<strong><a href=\"javascript:opencaseorder('" + ds.Tables[0].Rows[i]["ordernumber"] + "')\">Cast the order</a></strong><strong><a href=\"javascript:void();\" onclick=\"Completeorder('" + ds.Tables[0].Rows[i]["ordernumber"] + "')\">Complete this order</a></strong><strong><a href=\"javascript:trackorder('" + ds.Tables[0].Rows[i]["ordernumber"] + "')\">Order track</a></strong>";
                                    break;
                                case 80:
                                    functionstr = "<strong><a href=\"javascript:opencaseorder('" + ds.Tables[0].Rows[i]["ordernumber"] + "')\">Cast the order</a></strong><strong><a href=\"javascript:trackorder('" + ds.Tables[0].Rows[i]["ordernumber"] + "')\">Order track</a></strong>";
                                    break;
                            }


                            if (i == 0)
                                outdatalist += "{OrderNumber:\"" + ds.Tables[0].Rows[i]["OrderNumber"] + "\",id:\"" + ds.Tables[0].Rows[i]["id"] + "\",username:\"" + ds.Tables[0].Rows[i]["BillingLastName"] + " " + ds.Tables[0].Rows[i]["BillingFirstName"] + "\",TotalPayment:\"" + (curformat + " " + (Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalPayment"]) * Convert.ToDecimal(currencyrate)).ToString("F")) + "\",Timestamp:\"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Timestamp"]).ToShortDateString() + "\",LastTimestamp:\"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["LastTimestamp"]).ToShortDateString() + "\",ShippingMethod:\"" + ds.Tables[0].Rows[i]["ShippingMethod"].ToString() + "\",ShippingTrackingNumber:\"" + ds.Tables[0].Rows[i]["ShippingTrackingNumber"].ToString() + "\",OrderStatus:\"" + Tools.Order.OrderStatusLookup(Convert.ToInt32(ds.Tables[0].Rows[i]["OrderStatus"])) + "\",orderlist:\"" + Secrecy.Escape(getOrderliststr(ds.Tables[0].Rows[i]["OrderNumber"].ToString()).Replace("\"", "shuangyin")) + "\",functions:\"" + Secrecy.Escape(functionstr.Replace("\"", "shuangyin")) + "\",totalCount:\"" + totalCounts + "\"}";
                            else
                                outdatalist += ",{OrderNumber:\"" + ds.Tables[0].Rows[i]["OrderNumber"] + "\",id:\"" + ds.Tables[0].Rows[i]["id"] + "\",username:\"" + ds.Tables[0].Rows[i]["BillingLastName"] + " " + ds.Tables[0].Rows[i]["BillingFirstName"] + "\",TotalPayment:\"" + (curformat + " " + (Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalPayment"]) * Convert.ToDecimal(currencyrate)).ToString("F")) + "\",Timestamp:\"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Timestamp"]).ToShortDateString() + "\",LastTimestamp:\"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["LastTimestamp"]).ToShortDateString() + "\",ShippingMethod:\"" + ds.Tables[0].Rows[i]["ShippingMethod"].ToString() + "\",ShippingTrackingNumber:\"" + ds.Tables[0].Rows[i]["ShippingTrackingNumber"].ToString() + "\",OrderStatus:\"" + Tools.Order.OrderStatusLookup(Convert.ToInt32(ds.Tables[0].Rows[i]["OrderStatus"])) + "\",orderlist:\"" + Secrecy.Escape(getOrderliststr(ds.Tables[0].Rows[i]["OrderNumber"].ToString()).Replace("\"", "shuangyin")) + "\",functions:\"" + Secrecy.Escape(functionstr.Replace("\"", "shuangyin")) + "\",totalCount:\"" + totalCounts + "\"}";

                        }
                    }

                    sb.Remove(0, sb.Length);
                    sb.Append("{\"Results\":\"" + dayimg + "\",Datalist:[" + outdatalist + "]}");
                    context.Response.Write(sb.ToString());
                    context.Response.End();

                }
                else if (r == "getactsupportlist")
                {
                    string pagenow = Tools.Tools.getSafeCode(context.Request["pagenow"]);
                    //string orderstatus = Tools.Tools.getSafeCode(context.Request["orderstatus"]);
                    //string actOrder_startime = Tools.Tools.getSafeCode(context.Request["actOrder_startime"]);
                    //string actOrder_endtime = Tools.Tools.getSafeCode(context.Request["actOrder_endtime"]);
                    string wherewsql = string.Empty;


                    string useremail = "0415487";

                    if (Tools.Cookie.CookieCheck("useremail"))
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    string outdatalist = string.Empty;

                    wherewsql = " and userEmail='" + useremail + "'";


                    int totalCounts = 0;

                    SqlDataReader reader = BLL.ReturnValue("select count(*) from View_TicketList where id>0 " + wherewsql, false);
                    if (reader.Read())
                    {
                        totalCounts = reader.GetInt32(0);
                    }
                    reader.Close();


                    DataSet ds = new DataSet();

                    Model m = new Model();
                    m.DateTable = "View_TicketList";
                    m.soryType = "ID DESC";
                    m.KeyTitle = "id";
                    m.pagesize = 30;
                    m.pageindex = pagenow;
                    m.SearchByTitle = "id";
                    m.SearchByType = "!=";
                    m.keyword = "";
                    m.WhereSQL = wherewsql;

                    ds = new DataSet();
                    ds = BLL.ReturnTongYong("TongYongFenYe", m);

                    if (totalCounts == 0)
                        totalCounts = BLL.getTongYongCount("TongYongFenYeCount", m);

                    string functionstr = string.Empty;

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        dayimg = "1";
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {

                            if (i == 0)
                                outdatalist += "{Title:\"" + ds.Tables[0].Rows[i]["Title"] + "\",id:\"" + ds.Tables[0].Rows[i]["id"] + "\",Question:\"" + Secrecy.Escape(ds.Tables[0].Rows[i]["Question"].ToString().Replace("\r", "</br>").Replace("\n", "</br>")) + "\",Timestamp:\"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Timesamp"]).ToShortDateString() + "\",anTimesamp:\"" + (ds.Tables[0].Rows[i]["anTimesamp"].ToString().Length > 0 ? Convert.ToDateTime(ds.Tables[0].Rows[i]["anTimesamp"]).ToShortDateString() : "") + "\",Anser:\"" + Secrecy.Escape(ds.Tables[0].Rows[i]["Anser"].ToString()) + "\",aneruser:\"" + (ds.Tables[0].Rows[i]["Nick"].ToString().Length > 0 ? ds.Tables[0].Rows[i]["Nick"].ToString() : ds.Tables[0].Rows[i]["Email"].ToString().Substring(0, 2) + "...") + "\",LoginName:\"" + ds.Tables[0].Rows[i]["LoginName"] + "\",totalCount:\"" + totalCounts + "\"}";
                            else
                                outdatalist += ",{Title:\"" + ds.Tables[0].Rows[i]["Title"] + "\",id:\"" + ds.Tables[0].Rows[i]["id"] + "\",Question:\"" + Secrecy.Escape(ds.Tables[0].Rows[i]["Question"].ToString().Replace("\r", "</br>").Replace("\n", "</br>")) + "\",Timestamp:\"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Timesamp"]).ToShortDateString() + "\",anTimesamp:\"" + (ds.Tables[0].Rows[i]["anTimesamp"].ToString().Length > 0 ? Convert.ToDateTime(ds.Tables[0].Rows[i]["anTimesamp"]).ToShortDateString() : "") + "\",Anser:\"" + Secrecy.Escape(ds.Tables[0].Rows[i]["Anser"].ToString()) + "\",aneruser:\"" + (ds.Tables[0].Rows[i]["Nick"].ToString().Length > 0 ? ds.Tables[0].Rows[i]["Nick"].ToString() : ds.Tables[0].Rows[i]["Email"].ToString().Substring(0, 2) + "...") + "\",LoginName:\"" + ds.Tables[0].Rows[i]["LoginName"] + "\",totalCount:\"" + totalCounts + "\"}";

                        }
                    }

                    sb.Remove(0, sb.Length);
                    sb.Append("{\"Results\":\"" + dayimg + "\",Datalist:[" + outdatalist + "]}");
                    context.Response.Write(sb.ToString());
                    context.Response.End();

                }
                else if (r == "addnewproblem")
                {
                    string sup_title = Tools.Tools.getSafeCode(context.Request["sup_title"]);
                    string pro_question = Tools.Tools.getSafeCode(context.Request["pro_question"]);
                    string wherewsql = string.Empty;
                    string useremail = "0";
                    if (Tools.Cookie.CookieCheck("useremail"))
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    SqlConnection sqlConn = DAL.localwcon();
                    SqlCommand cmdDB = new SqlCommand(@"", sqlConn);
                    cmdDB = new SqlCommand(@"  INSERT INTO Web_Ticket([Title]
      ,[Question]
      ,[userEmail]
      ,[Timesamp]) values(
       @Title
      ,@Question
      ,@userEmail
      ,@Timesamp)", sqlConn);

                    cmdDB.Parameters.Add("@Title", SqlDbType.NVarChar).Value = sup_title;
                    cmdDB.Parameters.Add("@Question", SqlDbType.NVarChar).Value = pro_question;
                    cmdDB.Parameters.Add("@userEmail", SqlDbType.VarChar).Value = useremail;
                    cmdDB.Parameters.Add("@Timesamp", SqlDbType.DateTime).Value = DateTime.Now;

                    cmdDB.Connection.Open();
                    cmdDB.ExecuteNonQuery();
                    cmdDB.Connection.Close();
                    dayimg = Tools.Secrecy.Escape("1|ok");



                }
                else if (r == "postnewdiscussion")
                {
                    string blogid = Tools.Tools.getSafeCode(context.Request["blogid"]);
                    string nick = Tools.Tools.getSafeCode(context.Request["nick"]);
                    string discussion = context.Request["discussion"].ToString();

                    string wherewsql = string.Empty;
                    string useremail = "0";
                    if (Tools.Cookie.CookieCheck("useremail"))
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    SqlConnection sqlConn = DAL.localwcon();
                    SqlCommand cmdDB = new SqlCommand(@"", sqlConn);
                    cmdDB = new SqlCommand(@"  INSERT INTO Web_Discussion([Nick]
      ,[Discussion]
      ,[Timestamp]
      ,[ParentID]
      ,[Email]) values(
       @Nick
      ,@Discussion
      ,@Timestamp
      ,@ParentID
      ,@Email)", sqlConn);

                    cmdDB.Parameters.Add("@Nick", SqlDbType.NVarChar).Value = nick;
                    cmdDB.Parameters.Add("@Discussion", SqlDbType.NVarChar).Value = discussion;
                    cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
                    cmdDB.Parameters.Add("@ParentID", SqlDbType.Int).Value = Int32.Parse(blogid);
                    cmdDB.Parameters.Add("@Email", SqlDbType.VarChar).Value = useremail;

                    cmdDB.Connection.Open();
                    cmdDB.ExecuteNonQuery();
                    cmdDB.Connection.Close();
                    dayimg = Tools.Secrecy.Escape("1");

                }
                else if (r == "postnewreply")
                {
                    string blogid = Tools.Tools.getSafeCode(context.Request["blogid"]);
                    string nick = Tools.Tools.getSafeCode(context.Request["nick"]);
                    string Replay = context.Request["replay"].ToString();

                    string wherewsql = string.Empty;
                    string useremail = "0";
                    if (Tools.Cookie.CookieCheck("useremail"))
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    SqlConnection sqlConn = DAL.localwcon();
                    SqlCommand cmdDB = new SqlCommand(@"", sqlConn);
                    cmdDB = new SqlCommand(@"  INSERT INTO Web_Discussion_Reply([Nick]
      ,[Replay]
      ,[Timestamp]
      ,[parentID]
      ,[Email]) values(
       @Nick
      ,@Replay
      ,@Timestamp
      ,@parentID
      ,@Email)", sqlConn);

                    cmdDB.Parameters.Add("@Nick", SqlDbType.NVarChar).Value = nick;
                    cmdDB.Parameters.Add("@Replay", SqlDbType.NVarChar).Value = Replay;
                    cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
                    cmdDB.Parameters.Add("@parentID", SqlDbType.Int).Value = Int32.Parse(blogid);
                    cmdDB.Parameters.Add("@Email", SqlDbType.VarChar).Value = useremail;

                    cmdDB.Connection.Open();
                    cmdDB.ExecuteNonQuery();
                    cmdDB.Connection.Close();
                    dayimg = Tools.Secrecy.Escape("1");

                }
                else if (r == "updateactinfo")
                {
                    string sex = Tools.Tools.getSafeCode(context.Request["sex"]);
                    string firstname = Tools.Tools.getSafeCode(context.Request["firstname"]);
                    string lastname = Tools.Tools.getSafeCode(context.Request["lastname"]);
                    string email = Tools.Tools.getSafeCode(context.Request["email"]);
                    string Nick = Tools.Tools.getSafeCode(context.Request["Nick"]);
                    string currentpsw = Tools.Tools.getSafeCode(context.Request["currentpsw"]);
                    string newpsw = Tools.Tools.getSafeCode(context.Request["newpsw"]);
                    string confirmpsw = Tools.Tools.getSafeCode(context.Request["newpsw"]);

                    string useremail = email;

                    if (Tools.Cookie.CookieCheck("useremail"))
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    bool isright = true, updatestatus = false;

                    if (newpsw.Length > 0)
                    {
                        SqlDataReader reader = BLL.ReturnValue("select Password from ACT_Accounts where email='" + email + "'", false);
                        if (reader.Read())
                        {
                            if (reader.GetString(0).ToLower() != currentpsw.ToLower())
                                isright = false;
                        }
                        reader.Close();
                    }

                    if (isright)
                    {
                        if (newpsw.Length > 0)
                        {
                            if (newpsw == confirmpsw)
                            {
                                updatestatus = BLL.ExecuteSqlFun("update ACT_Accounts set Password='" + newpsw + "',FirstName='" + firstname + "',LastName='" + lastname + "', Sex='" + sex + "',Nick='" + Nick + "' where Email='" + useremail + "'", false);
                                dayimg = Tools.Secrecy.Escape("1|ok");
                            }
                            else
                            {
                                dayimg = Tools.Secrecy.Escape("0|Two passwords are not consistent.");
                            }
                        }
                        else
                        {
                            updatestatus = BLL.ExecuteSqlFun("update ACT_Accounts set FirstName='" + firstname + "',LastName='" + lastname + "', Sex='" + sex + "',Nick='" + Nick + "' where Email='" + useremail + "'", false);
                            dayimg = Tools.Secrecy.Escape("1|ok");
                        }

                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("0|Password error.");
                    }


                }
                else if (r == "checkgroupby")
                {
                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);
                    dayimg = Tools.Secrecy.Escape("0|err");
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string qty = Tools.Tools.getSafeCode(context.Request["qty"]);
                    SqlDataReader reader = BLL.ReturnValue("select V.SKU,V.Stock,V.Grouptype,P.PrimaryBigCategoryID,P.PrimaryMidCategoryID,P.PrimaryCategoryID,V.SmallPictureURL,P.FullName,V.GroupByPrice,P.UnitWeight,P.LeadTime,V.ProductURL,P.ShopName,P.Battery,P.Liquid,P.WareHouse,P.SamplePrice,P.IsAirmail,P.color,P.mode from View_GroupbyList V,Products P where V.SKU=P.SKU and V.SKU=" + sku, false);
                    int Stock = 0;
                    int grouptype = 0, bigcategory = 0, midcategory = 0, mincategory = 0; // 0订购型 1减库存型
                    decimal groupprice = 0, sampprice = 0, unitweight = 0, discount = 0;
                    string SmallPictureURL = string.Empty, color = string.Empty, mode = string.Empty, shortname = string.Empty, producturl = string.Empty, shopname = string.Empty, warehouse = "0";
                    bool IsAirmail = false, Battery = false, Liquid = false, isinstock = false;
                    cart.Remove(sku);

                    if (reader.Read())
                    {
                        Stock = reader.GetInt32(1);
                        if (Stock > 0)
                            isinstock = true;
                        groupprice = reader.GetDecimal(8);
                        grouptype = reader.GetInt32(2);
                        shortname = reader.GetString(7);
                        producturl = reader.GetString(11);
                        SmallPictureURL = reader.GetString(6);
                        sampprice = reader.GetDecimal(16);
                        unitweight = reader.GetDecimal(9);
                        IsAirmail = reader.GetBoolean(17);
                        Battery = reader.GetBoolean(13);
                        bigcategory = reader.GetInt32(3);
                        midcategory = reader.GetInt32(4);
                        mincategory = reader.GetInt32(5);
                        discount = (sampprice - groupprice) / sampprice;
                        shopname = reader.GetString(12);
                        Liquid = reader.GetBoolean(14);
                        warehouse = "3";
                        color = reader.IsDBNull(18) ? "" : reader.GetString(18);
                        mode = reader.IsDBNull(19) ? "" : reader.GetString(19);

                        cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), color, mode, isinstock, "", "", bigcategory, midcategory, mincategory, discount, 0, SmallPictureURL, "", shortname, Int32.Parse(qty), groupprice, groupprice, unitweight + 10, "Group purchase of goods. To meet the requirements of delivery.", producturl, shopname, (Int32.Parse(qty) > 1 ? false : true), false,false, IsAirmail, false, false, false, false, Battery, Liquid, warehouse);

                    }
                    reader.Close();

                    //DataSet ds = new DataSet();
                    //ds = BLL.ReturnDataSet("Select * from Web_ShippingCountry order by CountryName asc", false);
                    //string selectcountrys = "<div><select id=\"selectcountry\" class=\"showshippingcountry\"><option value=\"-1\">Please Select your country</option>";

                    //if (ds.Tables[0].Rows.Count > 0)
                    //{

                    //    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    //    {
                    //        selectcountrys += "<option value=\"" + ds.Tables[0].Rows[i]["id"].ToString() + "\" " + (cart.shipping.CountryID == ds.Tables[0].Rows[i]["id"].ToString() ? "selected" : "") + ">" + "    " + ds.Tables[0].Rows[i]["ShortName"].ToString() + "-" + ds.Tables[0].Rows[i]["CountryName"].ToString() + "  " + "</option>";
                    //    }
                    //}

                    //selectcountrys += "</select> <p><strong>Tel.</strong><input id=\"telphone\" type=\"text\" class=\"telphone\" /><input id=\"ismust\" type=\"hidden\" value=\"" + (cart.shipping.getShippingStyle(warehouse).Split('|')[2]) + "\" class=\"telphone\" /><span id=\"arrphone\"></span></p></div>";


                    if (grouptype == 0)
                    {


                        //if (groupprice >= 15)
                        //    dayimg = Secrecy.Secrecy.Escape("1|" + cart.getshippingstyleinfo() + selectcountrys + "<div class=\"poptrack\"><input id=\"tracknubmer\" type=\"checkbox\" /> <label for=\"tracknubmer\">Free tracknumber</label></div>|" + Stock + "|" + cart.GetSubtotal().ToString("C"));
                        //else
                        dayimg = Tools.Secrecy.Escape("1|" + Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warehouse).Replace("selectshippingstyle", "ajaxgroupbuyselectshippingstyle")) + "|" + Stock + "|" + cart.GetSubtotal(warehouse).ToString("C") + "|" + warehouse);

                    }
                    else
                    {
                        if (grouptype == 2)
                        {
                            if (Stock < 1)
                            {
                                dayimg = Tools.Secrecy.Escape("0|This product has been sold out!|" + +Stock + "|" + cart.GetSubtotal("0").ToString("C"));
                            }
                            else
                            {
                                dayimg = Tools.Secrecy.Escape("1|" + Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warehouse).Replace("selectshippingstyle", "ajaxgroupbuyselectshippingstyle")) + "|" + Stock + "|" + cart.GetSubtotal(warehouse).ToString("C") + "|" + warehouse);
                            }
                        }
                        else
                        {
                            if (Stock < 1)
                            {
                                dayimg = Tools.Secrecy.Escape("0|This product has been sold out!|" + +Stock + "|" + cart.GetSubtotal("0").ToString("C"));
                            }
                            else
                            {
                                dayimg = Tools.Secrecy.Escape("1|" + Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warehouse).Replace("selectshippingstyle", "ajaxgroupbuyselectshippingstyle")) + "|" + Stock + "|" + cart.GetSubtotal(warehouse).ToString("C") + "|" + warehouse);

                            }
                        }
                    }
                }
                else if (r == "checkfreegift")
                {
                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);
                    dayimg = Tools.Secrecy.Escape("0|err");
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string qty = Tools.Tools.getSafeCode(context.Request["qty"]);

                    string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();

                    SqlDataReader reader = BLL.ReturnValue("select V.SKU,V.Stock,V.PromotionType,P.PrimaryBigCategoryID,P.PrimaryMidCategoryID,P.PrimaryCategoryID,V.SmallPictureURL,P.FullName,V.SamplePrice,P.UnitWeight,P.LeadTime,V.ProductURL,P.ShopName,P.Battery,P.Liquid,P.WareHouse,P.SamplePrice,P.IsAirmail,P.color,P.mode from View_ProductList V,Products P where V.SKU=P.SKU and V.SKU=" + sku, false);
                    int Stock = 0;
                    int grouptype = 0, bigcategory = 0, midcategory = 0, mincategory = 0; // 0订购型 1减库存型
                    decimal groupprice = 0, sampprice = 0, unitweight = 0, discount = 0;
                    string SmallPictureURL = string.Empty, color = string.Empty, mode = string.Empty, shortname = string.Empty, producturl = string.Empty, shopname = string.Empty, warehouse = "4";
                    bool IsAirmail = false, Battery = false, Liquid = false, isinstock = false, updatestatus = false, addgiftstatus = true, issoldout = false;
                    cart.Remove(sku);

                    if (reader.Read())
                    {
                        Stock = reader.GetInt32(1);
                        if (Stock > 0)
                            isinstock = true;
                        groupprice = reader.GetDecimal(8);
                        grouptype = reader.GetInt32(2);
                        shortname = reader.GetString(7);
                        producturl = reader.GetString(11);
                        SmallPictureURL = reader.GetString(6);
                        sampprice = reader.GetDecimal(16);
                        unitweight = reader.GetDecimal(9);
                        IsAirmail = reader.GetBoolean(17);
                        Battery = reader.GetBoolean(13);
                        bigcategory = reader.GetInt32(3);
                        midcategory = reader.GetInt32(4);
                        mincategory = reader.GetInt32(5);
                        discount = 0;
                        shopname = reader.GetString(12);
                        Liquid = reader.GetBoolean(14);
                        warehouse = "4";
                        color = reader.IsDBNull(18) ? "" : reader.GetString(18);
                        mode = reader.IsDBNull(19) ? "" : reader.GetString(19);



                        if (Stock > 0)
                        {
                            if (cart.IsHaveFreeGift(warehouse) < 1)
                            {
                               
                                cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), color, mode, isinstock, "", "", bigcategory, midcategory, mincategory, discount, 0, SmallPictureURL, "", shortname, Int32.Parse(qty), groupprice, groupprice, unitweight + 10, "Free gift", producturl, shopname, (Int32.Parse(qty) > 1 ? false : true), false, true, IsAirmail, false, false, false, false, Battery, Liquid, warehouse);
                                //减库存
                                updatestatus = BLL.ExecuteSqlFun("update products set stock=stock-" + qty + " where sku=" + sku + ";insert TempStockList(SKU,SoldNumber,IP,WareHouse) values(" + sku + "," + qty + ",'" + ip.ToString() + "','dt')", false);

                            }
                            else
                            {
                                addgiftstatus = false;
                                issoldout = false;
                            }


                        }
                        else
                        {
                            addgiftstatus = false;
                            issoldout = true;
                        }
                    }
                    reader.Close();


                    if (addgiftstatus==false)
                    {
                        if (issoldout)
                            dayimg = Tools.Secrecy.Escape("0|This product has been sold out!|" + +Stock + "|" + cart.GetSubtotal(warehouse).ToString("C"));
                        else
                            dayimg = Tools.Secrecy.Escape("0|A maximum of two free gifts!|" + +Stock + "|" + cart.GetSubtotal(warehouse).ToString("C"));
                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("1|" + Tools.Secrecy.Escape(cart.shipping.getShippingStyle(warehouse).Replace("selectshippingstyle", "ajaxgroupbuyselectshippingstyle")) + "|" + Stock + "|" + cart.GetSubtotal(warehouse).ToString("C") + "|" + warehouse);

                    }
                }
                else if (r == "checkcartcheckout")
                {
                    string warestock = context.Request["wt"] != null ? Tools.Tools.getSafeCode(context.Request["wt"]) : "0";
                    string phone = context.Request["phone"] != null ? Tools.Tools.getSafeCode(context.Request["phone"]) : "";
                    string paypalstyle = context.Request["style"] != null ? Tools.Tools.getSafeCode(context.Request["style"]) : "paypal";

                    string shortagecolor = context.Request["shortagecolor"] != null ? Tools.Tools.getSafeCode(context.Request["shortagecolor"]) : "0";
                    string shortsize = context.Request["shortsize"] != null ? Tools.Tools.getSafeCode(context.Request["shortsize"]) : "0";
                    string shortgoods = context.Request["shortgoods"] != null ? Tools.Tools.getSafeCode(context.Request["shortgoods"]) : "0";
                    string ordermark = context.Request["ordermark"] != null ? Tools.Tools.getSafeCode(context.Request["ordermark"]) : "";
                    string haiguanfee = context.Request["haiguanfee"] != null ? Tools.Tools.getSafeCode(context.Request["haiguanfee"]) : "customization";  //customization 网站自定

                    GetConfigInfo configinfo = new GetConfigInfo();
                    string website = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "mapsite"));

                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    foreach (Item item in cart.Items)
                    {
                        if (item.PromotionType > 0)
                        {
                            if (getCtime(DateTime.Now, item.DateTimeNow.AddMinutes(15)) > 0)
                            { }
                            else
                            {
                                cart.RemovebuySKU(item.SKU);
                            }
                        }

                    }

                    decimal getlostprice = 0; //得 到 最 少 要 求 购 买 订 单 金 额 

                    if (cart.cartIshavepromotion(warestock))
                        getlostprice = cart.getLostpricebyPromotion(warestock);

                    if (cart.IsinShippingcart(1779, warestock))
                        cart.RefreshCoupon(1779, cart.GetSubtotalNoPm(warestock), warestock);

                    if (cart.IsinShippingcart(1780, warestock))
                        cart.RefreshCouponbyDIS(1780, cart.GetSubtotalNoPm(warestock), warestock);

                    if (cart.IsinShippingcart(1778, warestock))
                        cart.RefreshCouponbyDIS(1778, cart.GetSubtotalNoPm(warestock), warestock);

                    if (cart.cartIshavepromotion(warestock) && getlostprice > cart.GetSubtotalNoPm(warestock))
                    {
                        dayimg = Tools.Secrecy.Escape("66|Sorry, your purchase request error, please select the purchase of goods again.");
                    }
                    else
                    {
                        if (cart.Items.Count < 1 || cart.GetTotal(warestock) <= 0.01M)
                        {
                            if (cart.IsHaveLuckymoney(warestock) || cart.IsHaveCoupon(warestock) || cart.IsinShippingcart(1780))
                                dayimg = Tools.Secrecy.Escape("99|ok"); //有 COUPON或 者 红 包 生 成 的 订 单 
                            else
                                dayimg = Tools.Secrecy.Escape("0|No items");
                        }
                        else
                        {

                            if (phone.Length > 3)
                                cart.shipping.ShippingPhone = phone;


                            cart.shipping.shortagecolor = shortagecolor;
                            cart.shipping.shortagesize = shortsize;
                            cart.shipping.shortagegoods = shortgoods;
                            cart.shipping.ordermark = ordermark;
                            cart.shipping.haiguanfee = haiguanfee;

                            //这里要加入二次审核运费
                            cart.shipping.ResetShippingFee(false, warestock, cart.shipping.CountryName);

                            //这里要加入二次审核-8888 运单号
                            if (cart.IsinShippingcart(-8888))
                            {
                                cart.Remove("-8888");
                                if (warestock == "3")
                                {
                                    cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 2M, 2M, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false, false, true, false, false, false, false, false, false, warestock);
                                }
                                else
                                {
                                    if (cart.GetTotal(warestock) >= 20)
                                        cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 0, 0, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false, false, true, false, false, false, false, false, false, warestock);
                                    else
                                        cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 1.5M, 1.5M, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false, false, true, false, false, false, false, false, false, warestock);
                                }
                            }

                            if (cart.IsHaveLuckymoney(warestock))
                            {
                                //paul
                                decimal couponlostprice = 0;
                                string luckmodecode = cart.GetLuckmoneyCode(warestock);
                                SqlDataReader reader = BLL.ReturnValue("select LostPrice from Web_luckymoney where CodeNumber='" + luckmodecode + "'", false);
                                if (reader.Read())
                                {
                                    couponlostprice = reader.GetDecimal(0);
                                }
                                reader.Close();



                                if (cart.GetSubtotalForLuckmoney(warestock) >= couponlostprice)
                                    dayimg = Tools.Secrecy.Escape("99|ok");
                                else
                                    dayimg = Tools.Secrecy.Escape("66|Sorry, there is mistake occur when you use the lucky money. Please use your lucky money as the requirement.(Purchase amount should meet to the require amount).");

                            }
                            else
                                dayimg = Tools.Secrecy.Escape("1|" + warestock);

                            cart.paystyle = paypalstyle;
                        }
                    }
                }
                else if (r == "checkgroupbuyshippingcart")
                {
                    string warestock = "3", paypalstyle = "paypal";
                    GetConfigInfo configinfo = new GetConfigInfo();
                    string website = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "mapsite"));

                    string phone = context.Request["phone"] != null ? Tools.Tools.getSafeCode(context.Request["phone"]) : "";
                    string track = context.Request["track"] != null ? Tools.Tools.getSafeCode(context.Request["track"]) : "0";

                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);
                    //这里要加入二次审核运费
                    if (cart.Items.Count < 1 || cart.GetTotal(warestock) <= 0.01M)
                    {
                        if (cart.IsHaveLuckymoney(warestock) || cart.IsHaveCoupon(warestock) || cart.IsinShippingcart(1780))
                            dayimg = Tools.Secrecy.Escape("99|ok"); //有 COUPON或 者 红 包 生 成 的 订 单 
                        else
                            dayimg = Tools.Secrecy.Escape("0|No items");
                    }
                    else
                    {
                        if (cart.IsinShippingcart(1779, warestock))
                            cart.RefreshCoupon(1779, cart.GetSubtotalNoPm(warestock), warestock);

                        cart.shipping.ResetShippingFee(false, warestock, cart.shipping.CountryName);

                        if (phone.Length > 3)
                            cart.shipping.ShippingPhone = phone;

                        //这里要加入二次审核-8888 运单号
                        if (cart.IsinShippingcart(-8888) || decimal.Parse(track) > 0)
                        {
                            cart.Remove("-8888");
                            if (warestock == "3")
                            {
                                cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 2M, 2M, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false,false, true, false, false, false, false, false, false, warestock);
                            }
                            else
                            {
                                if (cart.GetTotal(warestock) >= 20)
                                    cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 0, 0, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false,false, true, false, false, false, false, false, false, warestock);
                                else
                                    cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 1.5M, 1.5M, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false, false, true, false, false, false, false, false, false, warestock);
                            }
                        }

                        if (cart.IsHaveLuckymoney(warestock))
                        {
                            //paul
                            decimal couponlostprice = 0;
                            string luckmodecode = cart.GetLuckmoneyCode(warestock);
                            SqlDataReader reader = BLL.ReturnValue("select LostPrice from Web_luckymoney where CodeNumber='" + luckmodecode + "'", false);
                            if (reader.Read())
                            {
                                couponlostprice = reader.GetDecimal(0);
                            }
                            reader.Close();



                            if (cart.GetSubtotal(warestock) >= couponlostprice)
                                dayimg = Tools.Secrecy.Escape("99|ok");
                            else
                                dayimg = Tools.Secrecy.Escape("66|Sorry, there is mistake occur when you use the lucky money. Please use your lucky money as the requirement.(Purchase amount should meet to the require amount).");

                        }
                        else
                            dayimg = Tools.Secrecy.Escape("1|" + warestock);

                        cart.paystyle = paypalstyle;
                    }
                }
                else if (r == "checkfreegiftcheckout")
                {
                    string warestock = "4", paypalstyle = "paypal";
                    GetConfigInfo configinfo = new GetConfigInfo();
                    string website = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "mapsite"));

                    string phone = context.Request["phone"] != null ? Tools.Tools.getSafeCode(context.Request["phone"]) : "";
                    string track = context.Request["track"] != null ? Tools.Tools.getSafeCode(context.Request["track"]) : "0";

                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);
                    //这里要加入二次审核运费
                    if (cart.Items.Count < 1 || cart.GetTotal(warestock) <= 0.01M)
                    {
                        if (cart.IsHaveLuckymoney(warestock) || cart.IsHaveCoupon(warestock) || cart.IsinShippingcart(1780))
                            dayimg = Tools.Secrecy.Escape("99|ok"); //有 COUPON或 者 红 包 生 成 的 订 单 
                        else
                            dayimg = Tools.Secrecy.Escape("0|No items");
                    }
                    else
                    {
                        if (cart.IsinShippingcart(1779, warestock))
                            cart.RefreshCoupon(1779, cart.GetSubtotalNoPm(warestock), warestock);

                        cart.shipping.ResetShippingFee(false, warestock, cart.shipping.CountryName);

                        if (phone.Length > 3)
                            cart.shipping.ShippingPhone = phone;

                        //这里要加入二次审核-8888 运单号
                        if (cart.IsinShippingcart(-8888) || decimal.Parse(track) > 0)
                        {
                            cart.Remove("-8888");
                            if (warestock == "4")
                            {
                                cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 2M, 2M, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false,false, true, false, false, false, false, false, false, warestock);
                            }
                            else
                            {
                                if (cart.GetTotal(warestock) >= 20)
                                    cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 0, 0, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false, false, true, false, false, false, false, false, false, warestock);
                                else
                                    cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 1.5M, 1.5M, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false, false, true, false, false, false, false, false, false, warestock);
                            }
                        }

                        if (cart.IsHaveLuckymoney(warestock))
                        {
                            //paul
                            decimal couponlostprice = 0;
                            string luckmodecode = cart.GetLuckmoneyCode(warestock);
                            SqlDataReader reader = BLL.ReturnValue("select LostPrice from Web_luckymoney where CodeNumber='" + luckmodecode + "'", false);
                            if (reader.Read())
                            {
                                couponlostprice = reader.GetDecimal(0);
                            }
                            reader.Close();



                            if (cart.GetSubtotal(warestock) >= couponlostprice)
                                dayimg = Tools.Secrecy.Escape("99|ok");
                            else
                                dayimg = Tools.Secrecy.Escape("66|Sorry, there is mistake occur when you use the lucky money. Please use your lucky money as the requirement.(Purchase amount should meet to the require amount).");

                        }
                        else
                            dayimg = Tools.Secrecy.Escape("1|" + warestock);

                        cart.paystyle = paypalstyle;
                    }
                }
                else if (r == "addnewwishlishtheme")
                {
                    string theme = Tools.Tools.getSafeCode(context.Request["theme"].Replace("'", "''"));
                    string email = "SAFIGKFDSGIERTEWWER";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }
                    bool updatestatus = true;

                    SqlDataReader reader = BLL.ReturnValue("SELECT Name FROM Wishlist WHERE EMAIL='" + email + "' AND NAME='" + theme + "'", false);
                    if (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false)
                            updatestatus = false;
                    }
                    reader.Close();


                    if (updatestatus)
                    {
                        updatestatus = BLL.ExecuteSqlFun("INSERT Wishlist(Email,SkuList,Name) VALUES('" + email + "','0,','" + theme + "');", false);
                        if (updatestatus)
                            dayimg = Tools.Secrecy.Escape("1|" + theme.Replace("''", "'") + "|" + theme.Replace("''", "'").Replace("'", "yinhs"));
                    }
                    else
                        dayimg = Tools.Secrecy.Escape("0|Has existed, please replace the new.");

                }
                else if (r == "deletewishlist")
                {
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string styleflag = Tools.Tools.getSafeCode(context.Request["styleflag"]).Replace("yinhs", "''");
                    bool updatestatus = false;

                    string email = "SAFIGKFDSGIERTEWWER";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }

                    updatestatus = BLL.ExecuteSqlFun("UPDATE Wishlist SET SKULIST=REPLACE(SKULIST,'," + sku + ",',',') WHERE NAME='" + styleflag + "' AND EMAIL='" + email + "'", false);

                    if (updatestatus)
                    {
                        dayimg = Tools.Secrecy.Escape("1|ok");
                    }
                    else
                        dayimg = Tools.Secrecy.Escape("0|err");

                }
                else if (r == "updatewishlist")
                {
                    string sku = Tools.Tools.getSafeCode(context.Request["seelctskuliststr"]);
                    string styleflag = Tools.Tools.getSafeCode(context.Request["id"]);
                    bool updatestatus = false;

                    string email = "SAFIGKFDSGIERTEWWER";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }

                    updatestatus = BLL.ExecuteSqlFun("UPDATE Wishlist SET SKULIST=SKULIST+'" + sku + "' WHERE id='" + styleflag + "' AND EMAIL='" + email + "'", false);

                    if (updatestatus)
                    {
                        dayimg = Tools.Secrecy.Escape("1|ok");
                    }
                    else
                        dayimg = Tools.Secrecy.Escape("0|err");

                }
                else if (r == "checkwishlistinfo")
                {
                    string email = "SAFIGKFDSGIERTEWWER";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }

                    string flag = Tools.Tools.getSafeCode(context.Request["flag"]);

                    DataSet ds = new DataSet();
                    string outNamelist = string.Empty;

                    ds = BLL.ReturnDataSet("SELECT Name FROM Wishlist WHERE EMAIL='" + email + "' order by name asc", false);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            outNamelist += "<li class=\"fbt\"><a href=\"javascript:" + (flag == "add" ? "addtowishlist" : "viewwishlist") + "('" + ds.Tables[0].Rows[i]["Name"].ToString().Replace("'", "yinhs") + "')\">" + ds.Tables[0].Rows[i]["Name"].ToString() + "</a></li>";
                        }
                        dayimg = Tools.Secrecy.Escape("1|" + outNamelist);
                    }

                }
                else if (r == "checkpagewishlist")
                {
                    string email = "SAFIGKFDSGIERTEWWER";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }
                                     

                    DataSet ds = new DataSet();
                    string outNamelist = string.Empty;

                    ds = BLL.ReturnDataSet("SELECT top 3 SkuList,Name FROM Wishlist WHERE EMAIL='" + email + "' order by name asc", false);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            outNamelist += "<h3>" + ds.Tables[0].Rows[i]["Name"].ToString() + "</h3><div class=\"info\"> " + getWishlistinfo(ds.Tables[0].Rows[i]["SkuList"].ToString()) + " </div>";
                        }
                        dayimg = Tools.Secrecy.Escape("1|" + outNamelist);
                    }




                }
                else if (r == "getmywishandmange")
                {
                    string email = "SAFIGKFDSGIERTEWWER";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }

                    string flag = Tools.Tools.getSafeCode(context.Request["flag"]);

                    DataSet ds = new DataSet();
                    string outNamelist = string.Empty;

                    ds = BLL.ReturnDataSet("SELECT Name,id FROM Wishlist WHERE EMAIL='" + email + "' order by name asc", false);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            outNamelist += "<tr><td class=\"name\"><input type=\"hidden\" value=\"" + ds.Tables[0].Rows[i]["id"].ToString() + "\"> <strong>" + ds.Tables[0].Rows[i]["Name"].ToString().Replace("'", "yinhs") + "</strong></td><td><a href=\"javascript:viod(0);\" onclick=\"editwishlisttheme(this);\">Edit</a></td><td><a href=\"javascript:viod(0);\" onclick=\"deletewishlisttheme(this);\">Delete</a></td><td></td></tr>";
                        }
                        dayimg = Tools.Secrecy.Escape("1|" + outNamelist);
                    }
                    else
                        dayimg = Tools.Secrecy.Escape("1|");

                }
                else if (r == "addskutowishlist")
                {
                    dayimg = Tools.Secrecy.Escape("0|err");
                    string email = "SAFIGKFDSGIERTEWWER";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }

                    string flag = Tools.Tools.getSafeCode(context.Request["flag"]).Replace("yinhs", "'");
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);

                    DataSet ds = new DataSet();
                    string outNamelist = string.Empty;
                    bool updatestatus = false;

                    ds = BLL.ReturnDataSet("SELECT Name FROM Wishlist WHERE EMAIL='" + email + "' and SkuList like '%," + sku + ",%'", false);

                    if (ds.Tables[0].Rows.Count < 1)
                    {
                        updatestatus = BLL.ExecuteSqlFun("update Wishlist set SkuList=SkuList+'" + sku + ",' where EMAIL='" + email + "' and Name='" + flag.Replace("'", "''") + "'", false);

                        if (updatestatus)
                            dayimg = Tools.Secrecy.Escape("1|ok");
                    }

                }
                else if (r == "editwishlisttheme")
                {
                    dayimg = Tools.Secrecy.Escape("0|err");
                    string email = "SAFIGKFDSGIERTEWWER";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }

                    bool updatestatus = false;
                    string flag = Tools.Tools.getSafeCode(context.Request["flag"]);
                    string id = Tools.Tools.getSafeCode(context.Request["id"]);
                    string name = Tools.Tools.getSafeCode(context.Request["name"]);

                    if (flag == "0")
                    {
                        updatestatus = BLL.ExecuteSqlFun("delete Wishlist where id=" + id, false);
                    }
                    else
                    {
                        updatestatus = BLL.ExecuteSqlFun("update Wishlist set name='" + name.Replace("yinhs", "''") + "' where id=" + id, false);
                    }


                    if (updatestatus)
                        dayimg = Tools.Secrecy.Escape("1|ok");

                }
                else if (r == "getyhitemlist")
                {
                    dayimg = Tools.Secrecy.Escape("0|err");
                    string outhtmlstr = string.Empty;
                    decimal yuhuic = Convert.ToDecimal(Tools.Tools.getSafeCode(context.Request["yuhuic"])) * 6.6M * 1.6M;
                    string sku = context.Request["sku"].ToString();
                    string outsku = string.Empty;
                    DataSet ds = new DataSet();
                    int outnum = 0;
                    ds = BLL.ReturnDataSet("select P.SKU,P.FullName,P.SmallPictureURL,P.SamplePrice-P.Purchaseprice-((P.UnitWeight+20)/1000*89) as lirun,P.SamplePrice/R.Price SamplePrice from Products P,Web_Main_Rate R where R.Verified=1 and P.PromotionType=0 and P.SaleQty>5 and P.InStock=1 and P.Hidden=0 and P.WareHouse='0' and P.SamplePrice-P.Purchaseprice-((P.UnitWeight+20)/1000*89)>1 and sku !=" + sku + " order by NEWID()", false);
                    decimal outprice = 0;
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (outprice < yuhuic)
                            {
                                outnum++;
                                outsku += ds.Tables[0].Rows[i]["sku"] + "-";
                                outhtmlstr += "<li><img src=\"" + Config.getConfig.imgPath() + ds.Tables[0].Rows[i]["SmallPictureURL"] + "\" /> <h5 class=\"p_name\">" + ds.Tables[0].Rows[i]["FullName"] + "</h5><h5>" + Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]).ToString("C") + "</h5></li>";
                                outprice += Convert.ToDecimal(ds.Tables[0].Rows[i]["lirun"]);
                            }
                            else
                                break;
                        }

                        dayimg = Tools.Secrecy.Escape("1|" + outhtmlstr + "|" + outnum.ToString() + "|" + Tools.Secrecy.Encode(outsku).Replace("+", "jiah").Replace("/", "xieg"));
                    }


                }
                else if (r == "addnewdecop")
                {
                    dayimg = Tools.Secrecy.Escape("0|err");
                    string outhtmlstr = string.Empty;

                    string sku = context.Request["sku"].ToString();
                    string defullname = context.Request["defullname"].ToString();
                    string dedescription = context.Request["dedescription"].ToString();
                    string isoked = context.Request["isoked"].ToString();

                    SqlConnection sqlConn = DAL.localwcon();
                    SqlCommand cmdDB = new SqlCommand(@"UPDATE Web_DeProducts set DeFullname=@defullname,DeDescription=@dedescription,TranslateStatus=@TranslateStatus where SKU=@sku", sqlConn);
                    cmdDB.Parameters.Add("@defullname", SqlDbType.NVarChar).Value = defullname;
                    cmdDB.Parameters.Add("@dedescription", SqlDbType.NText).Value = dedescription;
                    if (isoked == "1")
                        cmdDB.Parameters.Add("@TranslateStatus", SqlDbType.Bit).Value = true;
                    else
                        cmdDB.Parameters.Add("@TranslateStatus", SqlDbType.Bit).Value = false;

                    cmdDB.Parameters.Add("@sku", SqlDbType.Int).Value = sku;

                    cmdDB.Connection.Open();
                    cmdDB.ExecuteNonQuery();
                    cmdDB.Connection.Close();

                    dayimg = "1";

                }
                else if (r == "gethelptitle")
                {
                    string id = Tools.Tools.getSafeCode(context.Request["id"]);
                    string contentdate = PageSource.PageSource.GetPublickSocur("help/allhelptitle.ptshop").Trim();
                    string[] contentdatelist = contentdate.Trim().Split('\r');
                    dayimg = "1";
                    sb.Remove(0, sb.Length);
                    sb.Append("{\"Results\":\"" + dayimg + "\",\"rank\":[");

                    if (contentdatelist.Length > 0)
                    {
                        for (int i = 0; i < contentdatelist.Length; i++)
                        {
                            sb.Append("{\"id\":\"" + contentdatelist[i].Split('|')[0].Trim() + "\",\"title\":\"" + Tools.Secrecy.Escape(contentdatelist[i].Split('|')[2].Trim()) + "\",\"includeid\":\"" + Tools.Secrecy.Escape(contentdatelist[i].Split('|')[1].Trim()) + "\"}");
                            if (i <= (contentdatelist.Length - 2))
                            {
                                sb.Append(",");
                            }
                        }
                    }
                    sb.Append("]}");


                    context.Response.Write(sb.ToString());
                    context.Response.End();
                }
                else if (r == "setreviewpj")
                {
                    string id = Tools.Tools.getSafeCode(context.Request["id"]);
                    string reviewstyle = Tools.Tools.getSafeCode(context.Request["reviewstyle"]);

                    bool updateStatus = false;

                    if(reviewstyle.ToLower()=="no")
                        updateStatus = BLL.ExecuteSqlFun("Update Web_Reviews set BAD=BAD+1 where id=" + id, false);
                    else
                        updateStatus = BLL.ExecuteSqlFun("Update Web_Reviews set Good=Good+1 where id=" + id, false);


                    if (updateStatus)
                        dayimg = Tools.Secrecy.Escape("1|ok");
                    else
                        dayimg = Tools.Secrecy.Escape("0|err");



                }
                else if (r == "setdigg")
                {
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string num = Tools.Tools.getSafeCode(context.Request["num"]);

                    bool updateStatus = false;

                    updateStatus = BLL.ExecuteSqlFun("Update Products set Digg=Digg+" + num + " where sku=" + sku, false);
                    if (updateStatus)
                        dayimg = Tools.Secrecy.Escape("1|ok");
                    else
                        dayimg = Tools.Secrecy.Escape("0|err");



                }
                else if (r == "newpricematch")
                {
                   
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string p_name = Tools.Tools.getSafeCode(context.Request["p_name"]);
                    string p_email = Tools.Tools.getSafeCode(context.Request["p_email"]);
                    string p_link = context.Request["p_link"].ToString();
                    string p_note = context.Request["p_note"].ToString();

                    bool updateStatus = false;


                    SqlConnection sqlConn = DAL.localwcon();
                    SqlCommand cmdDB = new SqlCommand(@"INSERT Web_Pricematch(Email,UserName,ItemLike,ItemNote,SKU,Timestamp) values(@Email,@UserName,@ItemLike,@ItemNote,@SKU,@Timestamp)", sqlConn);
                    cmdDB.Parameters.Add("@Email", SqlDbType.VarChar).Value = p_email;
                    cmdDB.Parameters.Add("@UserName", SqlDbType.VarChar).Value = p_name;
                    cmdDB.Parameters.Add("@ItemLike", SqlDbType.VarChar).Value = p_link;
                    cmdDB.Parameters.Add("@ItemNote", SqlDbType.NVarChar).Value = p_note;
                    cmdDB.Parameters.Add("@SKU", SqlDbType.Int).Value = Int32.Parse(sku);
                    cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;

                    cmdDB.Connection.Open();

                    try
                    {
                        cmdDB.ExecuteNonQuery();
                        updateStatus = true;
                    }
                    catch { }


                    if (updateStatus)
                        dayimg = Tools.Secrecy.Escape("1|ok");
                    else
                        dayimg = Tools.Secrecy.Escape("0|err");
                }
                else if (r == "postreview")
                {
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string buildquality = Tools.Tools.getSafeCode(context.Request["buildquality"]);
                    string servicequality = Tools.Tools.getSafeCode(context.Request["servicequality"]);
                    string review_title = Tools.Tools.getSafeCode(context.Request["review_title"]);
                    string review_body = context.Request["review_body"].ToString();


                    string imglist = context.Request["imglist"];
                    string videocode = context.Request["videocode"];


                    string useremail = "Guest";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }

                    string mynick = "Guest";
                    if (Tools.Cookie.CookieCheck("nick"))
                        mynick = Tools.Cookie.GetEncryptedCookieValue("nick");

                    SqlConnection con = DAL.localwcon();
                    SqlCommand com = new SqlCommand("INSERT Web_Reviews(SKU,email,Nick,Rate,Buildquality,Servicequality,Timestamp,Good,BAD,Summary,Contents,Imageslist,Video) values(@SKU,@email,@Nick,@Rate,@Buildquality,@Servicequality,@Timestamp,@Good,@BAD,@Summary,@Contents,@Imageslist,@Video)", con);

                    com.Parameters.Add("@SKU", SqlDbType.Int).Value = Int32.Parse(sku);
                    com.Parameters.Add("@email", SqlDbType.VarChar).Value = useremail;
                    com.Parameters.Add("@Nick", SqlDbType.VarChar).Value = mynick;
                    com.Parameters.Add("@Rate", SqlDbType.Int).Value = Convert.ToInt32((Int32.Parse(buildquality) + Int32.Parse(servicequality)) / 2);
                    com.Parameters.Add("@Buildquality", SqlDbType.Int).Value = Int32.Parse(buildquality);
                    com.Parameters.Add("@Servicequality", SqlDbType.Int).Value = Int32.Parse(servicequality);
                    com.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
                    com.Parameters.Add("@Good", SqlDbType.Int).Value = 0;
                    com.Parameters.Add("@BAD", SqlDbType.Int).Value = 0;
                    com.Parameters.Add("@Summary", SqlDbType.VarChar).Value = review_title;
                    com.Parameters.Add("@Contents", SqlDbType.NVarChar).Value = review_body.Replace("\r", "<br />");
                    com.Parameters.Add("@Imageslist", SqlDbType.VarChar).Value = imglist;
                    com.Parameters.Add("@Video", SqlDbType.VarChar).Value = videocode;


                    dayimg = Tools.Secrecy.Escape("0|Unknown error.");
                    con.Open();
                    try
                    {
                        com.ExecuteNonQuery();
                        dayimg = Tools.Secrecy.Escape("1");
                    }
                    catch { }
                    con.Close();

                }
                else if (r == "getactilist")
                {
                    string contentdate = string.Empty;
                    string contentdate3 = string.Empty;//得到相应数据内容
                    contentdate3 = PageSource.PageSource.GetPublickSocur("activities.ptshop").Trim();
                    dayimg = Tools.Secrecy.Escape("0|err");

                    if (contentdate3.Length > 0)
                    {
                        string[] contentdatelist = contentdate3.Trim().Split('\r');
                        if (contentdatelist.Length > 0)
                        {
                            foreach (string key in contentdatelist)
                            {
                                contentdate += "<a href=\"" + key.Split('|')[2] + "\">" + key.Split('|')[0] + "</a>";
                            }

                        }
                    }

                    dayimg = Tools.Secrecy.Escape("1|" + contentdate);

                }
                else if (r == "getactilistandacountstatus")
                {
                    string contentdate = string.Empty;
                    string contentdate3 = string.Empty;//得到相应数据内容
                    contentdate3 = PageSource.PageSource.GetPublickSocur("activities.ptshop").Trim();
                    dayimg = Tools.Secrecy.Escape("0|err");

                    if (contentdate3.Length > 0)
                    {
                        string[] contentdatelist = contentdate3.Trim().Split('\r');
                        if (contentdatelist.Length > 0)
                        {
                            foreach (string key in contentdatelist)
                            {
                                contentdate += "<a href=\"" + key.Split('|')[2] + "\">" + key.Split('|')[0] + "</a>";
                            }

                        }
                    }

                    string useremail = "guest";
                    if (Tools.Cookie.CookieCheck("nick"))
                        useremail = Tools.Cookie.GetEncryptedCookieValue("nick");

                    dayimg = Tools.Secrecy.Escape("1|" + useremail + "|" + contentdate);

                }
                else if (r == "sendshortmessage")
                {
                    string message = Tools.Tools.getSafeCode(context.Request["message"]);
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);

                    string usernick = "guest";
                    if (Tools.Cookie.CookieCheck("nick"))
                        usernick = Tools.Cookie.GetEncryptedCookieValue("nick");

                    string useremail = "Guest@hotecig.com";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }

                    SqlConnection con = DAL.localwcon();
                    SqlCommand com = new SqlCommand("insert Web_ShortMessage([SKU],[Email],[Message],[Nick]) values(@sku,@email,@message,@nick)", con);

                    com.Parameters.Add("@sku", SqlDbType.Int).Value = Int32.Parse(sku);
                    com.Parameters.Add("@email", SqlDbType.VarChar).Value = useremail;
                    com.Parameters.Add("@message", SqlDbType.NVarChar).Value = message.Replace("\r", "<br />");
                    com.Parameters.Add("@nick", SqlDbType.NVarChar).Value = usernick;

                    dayimg = Tools.Secrecy.Escape("0|Unknown error.");
                    con.Open();
                    try
                    {
                        com.ExecuteNonQuery();
                        dayimg = Tools.Secrecy.Escape("1|ok");
                    }
                    catch { }
                    con.Close();

                }
                else if (r == "getshortmessage")
                {
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string returnvalues = string.Empty;

                    string useremail = "Guest@hotecig.com";
                    if (Tools.Cookie.CookieCheck("useremail"))
                    {
                        useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    }

                    DataSet ds = new DataSet();

                    ds = BLL.ReturnDataSet("select * from Web_ShortMessage where sku=" + sku + " order by ID desc", false);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        returnvalues += "[";
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                returnvalues += "'" + ds.Tables[0].Rows[i]["Nick"].ToString().Replace("'", "\'").Replace("-", "") + "-" + ds.Tables[0].Rows[i]["Message"].ToString().Replace("'", "\'").Replace("-", "") + "-" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Timesamp"]).ToShortDateString() + "'";
                            else
                                returnvalues += ", '" + ds.Tables[0].Rows[i]["Nick"].ToString().Replace("'", "\'").Replace("-", "") + "-" + ds.Tables[0].Rows[i]["Message"].ToString().Replace("'", "\'").Replace("-", "") + "-" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Timesamp"]).ToShortDateString() + "'"; ;

                        }
                        returnvalues += "]";

                        dayimg = Tools.Secrecy.Escape("1|" + Tools.Secrecy.Escape(returnvalues));
                    }



                }
                else if (r == "gettuiitemlist")
                {
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);

                    DataSet ds = new DataSet();



                    ds = BLL.ReturnDataSet(string.Format(@"SELECT TOP {0} V.SKU,V.battery,V.score,V.liquid,V.SaleQty,V.IsSoldoutPro,v.MSRP,V.digg,V.InStock,V.Glsku,V.Glprice, V.SamplePrice,V.ShopName,V.CategoryShortname,V.CategoryID, V.SmallPictureURL,V.ProductURL,V.ShortName,V.PromotionPrice,v.PromotionType, V.StartTime,V.EndTime,V.stock,V.ScoreNumber,V.reviewnum,V.WareHouse,V.color,V.LeadTime,V.SlideShowPictureThumbURLs from View_ProductList V where SKU in(
select sku from Web_ViewSkuList where Email in (select top 100 Email from Web_ViewSkuList where SKU={1} group by Email)
) order by UpadteTimesamp desc ", 15, sku), false);



                    string producttype = string.Empty;
                    decimal promotionprice = 0, price = 0;
                    dayimg = "1";
                    sb.Remove(0, sb.Length);
                    sb.Append("{\"Results\":\"" + dayimg + "\",\"Datalist\":[");

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            promotionprice = 0;
                            price = 0;
                            producttype = string.Empty;

                            if (Convert.ToInt32(ds.Tables[0].Rows[i]["PromotionType"]) > 0)
                            {
                                if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(ds.Tables[0].Rows[i]["StartTime"])) > 0 && DateTime.Compare(Convert.ToDateTime(ds.Tables[0].Rows[i]["EndTime"]), DateTime.Now) > 0)
                                {
                                    if (Convert.ToInt32(ds.Tables[0].Rows[i]["PromotionType"]) == 3 || Convert.ToInt32(ds.Tables[0].Rows[i]["PromotionType"]) == 4)
                                    {
                                        if (Convert.ToInt32(ds.Tables[0].Rows[i]["stock"]) > 0)
                                        {
                                            promotionprice = Convert.ToDecimal(ds.Tables[0].Rows[i]["PromotionPrice"]);
                                            price = Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]);

                                            producttype = "<span class=\"off_icon\">" + (Math.Round(((Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]) - Convert.ToDecimal(ds.Tables[0].Rows[i]["PromotionPrice"])) / Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"])), 2) * 100).ToString().Replace(".00", "").Replace(".0", "") + "<a href=\"/Product/" + Tools.PostStr.potstr(ds.Tables[0].Rows[i]["shortname"].ToString()) + "-" + ds.Tables[0].Rows[i]["sku"].ToString() + "\"></a></span>";
                                        }
                                        else
                                        {
                                            promotionprice = Convert.ToDecimal(ds.Tables[0].Rows[i]["PromotionPrice"]);
                                            price = Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]);
                                            producttype = "<span class=\"soldOut_icon\"><a href=\"/Product/" + Tools.PostStr.potstr(ds.Tables[0].Rows[i]["shortname"].ToString()) + "-" + ds.Tables[0].Rows[i]["sku"].ToString() + "\"></a></span>";
                                        }
                                    }
                                    else
                                    {
                                        promotionprice = Convert.ToDecimal(ds.Tables[0].Rows[i]["PromotionPrice"]);
                                        price = Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]);

                                        producttype = "<span class=\"off_icon\">" + (Math.Round(((Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]) - Convert.ToDecimal(ds.Tables[0].Rows[i]["PromotionPrice"])) / Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"])), 2) * 100).ToString().Replace(".00", "").Replace(".0", "") + "<a href=\"/Product/" + Tools.PostStr.potstr(ds.Tables[0].Rows[i]["shortname"].ToString()) + "-" + ds.Tables[0].Rows[i]["sku"].ToString() + "\"></a></span>";
                                    }
                                }
                                else
                                {
                                    promotionprice = Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]);
                                    price = Convert.ToDecimal(ds.Tables[0].Rows[i]["MSRP"]);
                                }
                            }
                            else
                            {
                                promotionprice = Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]);
                                price = Convert.ToDecimal(ds.Tables[0].Rows[i]["MSRP"]);
                            }

                            if (Convert.ToBoolean(ds.Tables[0].Rows[i]["InStock"]) == false)
                            {
                                producttype = "<span class=\"soldOut_icon\"><a href=\"/Product/" + Tools.PostStr.potstr(ds.Tables[0].Rows[i]["shortname"].ToString()) + "-" + ds.Tables[0].Rows[i]["sku"].ToString() + "\"></a></span>";
                            }

                            if (Convert.ToBoolean(ds.Tables[0].Rows[i]["IsSoldoutPro"]) == true)
                            {
                                if (Convert.ToInt32(ds.Tables[0].Rows[i]["Stock"]) < 1)
                                {
                                    producttype = "<span class=\"soldOut_icon\"><a href=\"/Product/" + Tools.PostStr.potstr(ds.Tables[0].Rows[i]["shortname"].ToString()) + "-" + ds.Tables[0].Rows[i]["sku"].ToString() + "\"></a></span>";
                                }
                            }


                            sb.Append("{\"sku\":\"" + ds.Tables[0].Rows[i]["sku"].ToString().Trim() + "\",\"producturl\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["ProductURL"].ToString().Trim()) + "\",\"smallpicture\":\"" + Tools.Secrecy.Escape(Config.getConfig.imgPath() + ds.Tables[0].Rows[i]["SmallPictureURL"].ToString().Trim()) + "\",\"shortname\":\"" + Tools.Secrecy.Escape(ds.Tables[0].Rows[i]["ShortName"].ToString().Trim()) + "\",\"price\":\"" + promotionprice.ToString("C") + "\"}");
                            if (i <= (ds.Tables[0].Rows.Count - 2))
                            {
                                sb.Append(",");
                            }
                        }
                    }
                    sb.Append("]}");


                    context.Response.Write(sb.ToString());
                    context.Response.End();

                }
                else if (r == "checkpagepomotionbuynow")
                {
                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    GetConfigInfo configinfo = new GetConfigInfo();
                    string website = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getEmailconfigfile, "website"));

                    string phone = context.Request["phone"].ToString();
                    string track = context.Request["track"] != null ? context.Request["track"] : "0";

                    if (track == "1")
                    {
                        if (cart.GetSubtotal("0") >= 20)
                            cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 0, 0, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false, false, true, false, false, false, false, false, false, "0");
                        else
                            cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 1.5M, 1.5M, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false, false, true, false, false, false, false, false, false, "0");
                    }


                    if (phone.Length > 1)
                        cart.shipping.ShippingPhone = phone;

                    dayimg = Tools.Secrecy.Escape("1");
                }
                else if (r == "checksecondkill")
                {

                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);
                    dayimg = Tools.Secrecy.Escape("0|err");
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    SqlDataReader reader = BLL.ReturnValue("select SKU,SecondkillPrice,StartTime,EndTime,BoughtNum,IsOn,Stocks,SamplePrice,Shortname,SmallPictureURL,ProductURL,Stocks from View_Secondkill where sku=" + sku, false);
                    int Stock = 0;

                    if (cart.GetItemCount("0") > 4)
                        context.Session.Remove("Cart");
                    else
                        cart.Remove(sku);


                    DateTime StartTimePM = DateTime.Now;
                    DateTime EndTimePM = DateTime.Now;
                    DateTime NowTimePM = DateTime.Now;


                    decimal Secondkillprice = 0, sampprice = 0;
                    string SmallPictureURL = string.Empty, shortname = string.Empty, producturl = string.Empty;
                    if (reader.Read())
                    {
                        Stock = reader.GetInt32(6);
                        Secondkillprice = reader.GetDecimal(1);
                        shortname = reader.GetString(8);
                        producturl = reader.GetString(10);
                        SmallPictureURL = reader.GetString(9);
                        sampprice = reader.GetDecimal(7);

                        StartTimePM = reader.GetDateTime(2);
                        EndTimePM = reader.GetDateTime(3);

                        if (DateTime.Compare(NowTimePM, StartTimePM) < 0)
                        {
                            dayimg = Tools.Secrecy.Escape("0|the event is not start|" + +Stock);
                        }
                        else if (DateTime.Compare(NowTimePM, StartTimePM) > 0 && DateTime.Compare(EndTimePM, NowTimePM) > 0)
                        {
                            if (Stock < 1)
                            {
                                dayimg = Tools.Secrecy.Escape("0|This product has been sold out!|" + +Stock);
                            }
                            else
                            {
                                bool updatestatus = false;
                                updatestatus = BLL.ExecuteSqlFun("update Web_Secondkill set Stocks=Stocks-1 where sku=" + sku, false);
                                if (updatestatus)
                                {

                                    cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", false, "Instant kill", "Instant kill", 0, 0, 0, 0, 0, SmallPictureURL, "", shortname, 1, Secondkillprice, sampprice, 150, "group buy", producturl, "hotecig", true, false, false, true, true, false, false, true, false, false, "0");

                                    DataSet ds = new DataSet();
                                    ds = BLL.ReturnDataSet("Select * from Web_ShippingCountry order by CountryName asc", false);
                                    string selectcountrys = "<select id=\"selectcountry\" class=\"showshippingcountry\" onchange=\"GetEmaFee(this)\"><option value=\"-1\">Please Select your country</option>";

                                    if (ds.Tables[0].Rows.Count > 0)
                                    {

                                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                        {
                                            selectcountrys += "<option value=\"" + ds.Tables[0].Rows[i]["id"].ToString() + "\" " + (cart.shipping.CountryID == ds.Tables[0].Rows[i]["id"].ToString() ? "selected" : "") + ">" + "    " + ds.Tables[0].Rows[i]["ShortName"].ToString() + "-" + ds.Tables[0].Rows[i]["CountryName"].ToString() + "  " + "</option>";
                                        }
                                    }
                                    selectcountrys += "</select> <p><strong>Tel.</strong><input id=\"telphone\" type=\"text\" class=\"telphone\" /><span id=\"arrphone\"></span></p>";



                                    if (Secondkillprice >= 20)
                                        dayimg = Tools.Secrecy.Escape("2|<div>You have INSTANT KILL a product success, please pay for it in 15 minute, or you will lost  this chance.</div>" + cart.shipping.getShippingStyle("0").Split('|')[1] + selectcountrys + "<div class=\"poptrack\"><input id=\"tracknubmer\" type=\"checkbox\" /> <label for=\"tracknubmer\">Free tracknumber</label></div>|" + Stock);
                                    else
                                        dayimg = Tools.Secrecy.Escape("1|<div>You have INSTANT KILL a product success, please pay for it in 15 minute, or you will lost  this chance.</div>" + cart.shipping.getShippingStyle("0").Split('|')[1] + selectcountrys + "<div class=\"poptrack\"><input id=\"tracknubmer\" type=\"checkbox\" /> <label for=\"tracknubmer\">$1.50</label></div>|" + Stock);
                                }
                                else
                                {
                                    dayimg = Tools.Secrecy.Escape("0|your operation is wrong, pls try again|" + +Stock);
                                }
                            }


                        }
                        else
                        {
                            dayimg = Tools.Secrecy.Escape("0|this event is expire|" + +Stock);
                        }



                    }
                    reader.Close();

                }
                else if (r == "getbrandinfo")
                {
                    string brand = Tools.Tools.getSafeCode(context.Request["brand"]);
                    string outstr = string.Empty;
                    SqlDataReader reader = BLL.ReturnValue("select BrandNote from Web_Brand where BrandName='" + brand + "'", false);

                    if (reader.Read())
                    {
                        outstr = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                    reader.Close();

                    dayimg = Tools.Secrecy.Escape("1|" + Tools.Secrecy.Escape(outstr));

                }
                else if (r == "postxinyong")
                {
                    string isaddaddress="0", billingfirstname = string.Empty, billinglastname = string.Empty, billingname=string.Empty, billingemail = string.Empty, billingcompany = string.Empty, billingZipCode = string.Empty, billingAddress = string.Empty, billingpostNumber = string.Empty, billingCity = string.Empty, billingState = string.Empty, billingphoneNumber = string.Empty, billingCountry;
                    string shippingfirstname = string.Empty,shippingname=string.Empty, shippinglastname = string.Empty, shippingcompany = string.Empty, shippingZipCode = string.Empty, shippingAddress = string.Empty, shippingpostnumber = string.Empty, shippingCity = string.Empty, shippingState = string.Empty, shippingphoneNumber = string.Empty, shippingCountry;

                    string warecode = string.Empty;

                    string expYear = context.Request["expYear"].ToString();
                    string cardNO = context.Request["cardNO"].ToString();
                    string expMonth = context.Request["expMonth"].ToString();
                    string cvv = context.Request["cvv"].ToString();

                    isaddaddress = context.Request["isaddaddress"].ToString();

                    shippingfirstname = context.Request["shippingfirstname"].ToString();
                    shippinglastname = context.Request["shippinglastname"].ToString();
                    shippingname = shippingfirstname + "." + shippinglastname;
                    shippingcompany = context.Request["shippingcompany"].ToString();
                    shippingZipCode = context.Request["shippingZipCode"].ToString();
                    shippingAddress = context.Request["shippingAddress"].ToString();
                    shippingCity = context.Request["shippingCity"].ToString();
                    shippingState = context.Request["shippingState"].ToString();
                    shippingpostnumber = context.Request["shippingpostnumber"].ToString();
                    shippingCountry = context.Request["shippingCountry"].ToString();
                    shippingphoneNumber = context.Request["shippingCountry"].ToString();
                    warecode = context.Request["warecode"].ToString();

                    billingfirstname = context.Request["billingfirstname"].ToString();
                    billinglastname = context.Request["billinglastname"].ToString();
                    billingname = billingfirstname + "." + billinglastname;
                    billingemail = context.Request["billingemail"].ToString();
                    billingcompany = context.Request["billingcompany"].ToString();
                    billingZipCode = context.Request["billingZipCode"].ToString();
                    billingAddress = context.Request["billingAddress"].ToString();
                    billingpostNumber = context.Request["billingpostNumber"].ToString();
                    billingCity = context.Request["billingCity"].ToString();
                    billingState = context.Request["billingState"].ToString();

                    billingphoneNumber = context.Request["billingphoneNumber"].ToString();
                    billingCountry = context.Request["billingCountry"].ToString();

                    if (isaddaddress == "1")
                        add_address(billingemail, shippingfirstname, shippinglastname, shippingcompany, shippingZipCode, shippingAddress, shippingCity, shippingState, shippingpostnumber, shippingCountry, shippingphoneNumber);

                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);


                    string ordernumber = string.Empty;
                    ordernumber = Order.Order.CreateOrders(warecode, billingemail, billingfirstname, billinglastname, billingcompany, billingAddress, billingCity, billingState, billingZipCode, billingCountry, billingphoneNumber, shippingfirstname, shippinglastname, shippingcompany, shippingAddress, shippingCity, shippingState, shippingZipCode, shippingCountry, shippingphoneNumber,cart.shipping.shortagecolor,cart.shipping.shortagesize,cart.shipping.shortagegoods,cart.shipping.haiguanfee,cart.shipping.ordermark, cart.GetTotal(warecode));
                    string outordernumber = string.Empty, transactionnumber = string.Empty, outamount = string.Empty, respMsg = string.Empty;

                    GetConfigInfo configinfo = new GetConfigInfo();
                    string merno = string.Empty, hashcode = string.Empty, terno = string.Empty, safecode = string.Empty, transtype = string.Empty, apitype = string.Empty, transmodel = string.Empty, currencycode = string.Empty, xyreturnurl = string.Empty, mermgrurl = string.Empty, language = string.Empty, hash = string.Empty, payIP = string.Empty;
                    string outpht = string.Empty;
                    string paypalconfigfile = configinfo.getPaypalconfigfile;
                    string goodsstring = "{\"goodsInfo\":[";

                    if (cart.Items.Count > 0)
                    {
                        for (int i = 0; i < cart.Items.Count; i++)
                        {
                            if (i == 0)
                                goodsstring += "{\"goodsName\":\"" + cart.Items[i].FullName.ToString().Replace("'", "").Replace("\"", "") + "\",\"quantity\":\"" + cart.Items[i].Quantity.ToString() + "\",\"goodsPrice\":\"" + cart.Items[i].Price.ToString("F") + "\"}";
                            else
                                goodsstring += ",{\"goodsName\":\"" + cart.Items[i].FullName.ToString().Replace("'", "").Replace("\"", "") + "\",\"quantity\":\"" + cart.Items[i].Quantity.ToString() + "\",\"goodsPrice\":\"" + cart.Items[i].Price.ToString("F") + "\"}";
                        }
                    }
                    goodsstring += "]}";






                    merno = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(paypalconfigfile, "merno"));
                    terno = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(paypalconfigfile, "terno"));
                    transtype = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(paypalconfigfile, "transtype"));
                    apitype = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(paypalconfigfile, "apitype"));
                    transmodel = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(paypalconfigfile, "transmodel"));
                    currencycode = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(paypalconfigfile, "currencycode"));
                    xyreturnurl = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(paypalconfigfile, "xyreturnurl"));
                    mermgrurl = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(paypalconfigfile, "mermgrurl"));
                    language = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(paypalconfigfile, "language"));
                    hash = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(paypalconfigfile, "hash"));
                    safecode = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(paypalconfigfile, "safecode"));
                    payIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();



                    outpht = "EncryptionMode=SHA256&CharacterSet=UTF8&merNo=" + merno + "&terNo=" + terno + "&orderNo=" + ordernumber + "&currencyCode=" + currencycode + "&amount=" + cart.GetTotal(warecode).ToString("F") + "&payIP=" + payIP + "&transType=" + transtype + "&transModel=" + transmodel + "&" + safecode;

                    hashcode = Tools.Tools.GetSHA256(outpht);



                    string URL = "https://payment.fhtpay.com/FHTPayment/api/payment"; //配置网关地址

                    Hashtable headers = new Hashtable();
                    headers.Add("merNo", merno);
                    headers.Add("terNo", terno);
                    headers.Add("transType", transtype);
                    headers.Add("apiType", apitype);
                    headers.Add("transModel", transmodel);
                    headers.Add("amount", cart.GetTotal(warecode).ToString("F"));
                    headers.Add("currencyCode", currencycode);
                    headers.Add("orderNo", ordernumber);
                    headers.Add("merremark", "");
                    headers.Add("returnURL", xyreturnurl);
                    headers.Add("merMgrURL", mermgrurl);
                    headers.Add("webInfo", HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString());
                    headers.Add("language", language);
                    headers.Add("cardCountry", billingCountry);
                    headers.Add("cardState", billingState);
                    headers.Add("cardCity", billingCity);
                    headers.Add("cardAddress", billingAddress);
                    headers.Add("cardZipCode", billingZipCode);
                    headers.Add("payIP", payIP);
                    headers.Add("cardFullName", billingname);
                    headers.Add("cardFullPhone", billingphoneNumber);
                    headers.Add("grCountry", shippingCountry);
                    headers.Add("grState", shippingState);
                    headers.Add("grCity", shippingCity);
                    headers.Add("grAddress", shippingAddress);
                    headers.Add("grZipCode", shippingZipCode);
                    headers.Add("grEmail", billingemail);
                    headers.Add("grphoneNumber", billingphoneNumber);
                    headers.Add("grPerName", shippingname);
                    headers.Add("goodsString", goodsstring);
                    headers.Add("hash", hash);
                    headers.Add("hashcode", hashcode);
                    headers.Add("expYear", expYear);
                    headers.Add("cardNO", cardNO);
                    headers.Add("expMonth", expMonth);
                    headers.Add("cvv", cvv);


                    string szdata = "";
                    if (URL != "")
                    {
                        szdata = Xinyongka.Xinyongka.SendMessageByPost(headers, URL);
                    }

                    try
                    {
                        string datafilename = HttpContext.Current.Server.MapPath("\\aff\\" + DateTime.Now.ToString().Replace("/", "-").Replace("\\", "-").Replace(":", "-").Replace(" ", "-") + ".txt");
                        bool createStatussss = CreatFile.CreatFiles(datafilename, szdata, "utf-8");
                    }
                    catch { }

                    if (szdata.Length > 0)
                    {
                        string[] returnarr = szdata.Replace("{", "").Replace("}", "").Replace("\"", "").Split(',');

                        if (returnarr.Length > 2)
                        {
                            if (returnarr[8].Split(':')[1] == "00" || returnarr[8].Split(':')[1] == "0")
                            {

                                for (int i = 0; i < returnarr.Length; i++)
                                {
                                    if (returnarr[i].Split(':')[0].ToLower() == "orderno")
                                    {
                                        outordernumber = returnarr[i].Split(':')[1].ToUpper();
                                    }
                                    if (returnarr[i].Split(':')[0].ToLower() == "tradeno")
                                    {
                                        transactionnumber = returnarr[i].Split(':')[1].ToUpper();
                                    }
                                    if (returnarr[i].Split(':')[0].ToLower() == "amount")
                                    {
                                        outamount = returnarr[i].Split(':')[1].ToUpper();
                                    }
                                    if (returnarr[i].Split(':')[0].ToLower() == "respmsg")
                                    {
                                        respMsg = returnarr[i].Split(':')[1].ToUpper();
                                    }


                                }



                                SqlConnection sqlConn = DAL.localwcon();

                                SqlCommand cmdDB = new SqlCommand(@"INSERT [Transactions] (
Timestamp,
PaymentDate,
Cleared,
Voided,
GatewayResponse,
OrderNumber,
Staff,
TransactionNumber,
ParentTransactionNumber,
Amount,
Notes,
TransactionLog
) VALUES (
@Timestamp,
@PaymentDate,
@Cleared,
@Voided,
@GatewayResponse,
@OrderNumber,
@Staff,
@TransactionNumber,
@ParentTransactionNumber,
@Amount,
@Notes,
@TransactionLog
" + ")", sqlConn);

                                cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
                                cmdDB.Parameters.Add("@PaymentDate", SqlDbType.DateTime).Value = DateTime.Now;
                                cmdDB.Parameters.Add("@Cleared", SqlDbType.Bit).Value = true;
                                cmdDB.Parameters.Add("@Voided", SqlDbType.Bit).Value = false;
                                cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = outordernumber; // set to null for now

                                cmdDB.Parameters.Add("@GatewayResponse", SqlDbType.VarChar).Value = "";
                                cmdDB.Parameters.Add("@Staff", SqlDbType.VarChar).Value = "FUHUIDONG";
                                cmdDB.Parameters.Add("@TransactionNumber", SqlDbType.VarChar).Value = transactionnumber + "";
                                cmdDB.Parameters.Add("@ParentTransactionNumber", SqlDbType.VarChar).Value = "";
                                cmdDB.Parameters.Add("@Amount", SqlDbType.Money).Value = outamount;
                                cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "Via xinyongka";
                                cmdDB.Parameters.Add("@TransactionLog", SqlDbType.Text).Value = respMsg;

                                //== END

                                sqlConn.Open();
                                cmdDB.ExecuteNonQuery();


                                cmdDB = new SqlCommand(@"INSERT OrderStatus (OrderNumber, Timestamp, Username, StatusCode, Notes) VALUES (@OrderNumber, @Timestamp, @Username, @StatusCode, @Notes)", sqlConn);
                                cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = outordernumber;
                                cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
                                cmdDB.Parameters.Add("@Username", SqlDbType.VarChar).Value = "FUHUIDONG";
                                cmdDB.Parameters.Add("@StatusCode", SqlDbType.Int).Value = 1;
                                cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "Instant Order Received";
                                cmdDB.ExecuteNonQuery();


                                sqlConn.Close();



                                dayimg = Tools.Secrecy.Escape("1|" + outordernumber);
                            }
                            else
                            {
                                dayimg = Tools.Secrecy.Escape("0|" + returnarr[1].Split(':')[1]);
                            }

                        }
                        else
                        {
                            dayimg = Tools.Secrecy.Escape("0|" + returnarr[1].Split(':')[1]);
                        }



                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("0|err with your payment");
                    }





                    //{"respCode":"1132","respMsg":"Merchant payment limit URL"}

                    //{"Results":"EncryptionMode=SHA256&CharacterSet=UTF8&merNo=108888&terNo=88826&orderNo=Q1027184577&currencyCode=USD&amount=1.06&payIP=192.168.1.77&transType=sales&transModel=M&d641f79b91394611b702fe4b2bb4edd2|D17AFF73375E20B52BD1ACAD1ECAFFB2D3D00D34DF15AB3138BFD1D4A84F8A8B|{"respCode":"1132","respMsg":"Merchant payment limit URL"}"}









                }
                else if (r == "addfreegiftsku")
                {
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    Config.GetConfigInfo configinfo = new Config.GetConfigInfo();

                    int quantity = 1;
                    decimal proPrice = 0, limitetrackfee = 20, trackfee = 1.5M;
                    limitetrackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "limitetrackfee")));
                    trackfee = Convert.ToDecimal(Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getConfigfile, "trackfee")));

                    string shortname = string.Empty, smallpictureurl = string.Empty, leadtime = string.Empty, glsku = string.Empty, glprice = string.Empty, shopname = string.Empty, producturl = string.Empty, warehouse = "0", tempFullname = string.Empty;
                    decimal sampleprice = 0, pricemid = 0, pricemin = 0, promotionprice = 0, lostcartotal = 0, unitweight = 0, carronweight = 0, discount = 0;
                    bool instock = false, isairmail = true, battery = false, issoldoutpro = false, supportcoupon = true, liquid = false, errSku = false, updatestatus = false, IsPomotion = true;
                    int promotiontype = 0, bigcategory = 0, midcategory = 0, mincategory = 0, stock = 0;
                    DateTime starttime = DateTime.Now, endtime = DateTime.Now;
                    bool ispromotionproduct = false;

                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    DataSet ds = new DataSet();

                    //////////////////////////////////Get Product Info/////////////////////////////////////////
                    SqlConnection sqlConn = DAL.localrcon();
                    sqlConn.Open();
                    SqlCommand cmdDB = new SqlCommand(@"GetShoppingSKUInfo", sqlConn);
                    cmdDB.CommandType = CommandType.StoredProcedure;
                    cmdDB.Parameters.Add("@sku", SqlDbType.Int).Value = Int32.Parse(sku);
                    SqlDataAdapter adapt = new SqlDataAdapter(cmdDB);
                    adapt.Fill(ds, "ProductInfo");
                    ///////////////////////////////////Get Product Info///////////////////////////////////////                   

                    DataRow row = ds.Tables[0].Rows[0];
                    if (ds.Tables["ProductInfo"].Rows.Count == 1)
                    {
                        shortname = (string)row["Shortname"];
                        smallpictureurl = (string)row["smallpictureurl"];
                        leadtime = (string)row["leadtime"];
                        glsku = row["glsku"].ToString().Length > 0 ? (string)row["glsku"] : "";
                        glprice = row["glprice"].ToString().Length > 0 ? (string)row["glprice"] : "";
                        shopname = (string)row["shopname"];
                        producturl = (string)row["producturl"];
                        warehouse = (string)row["warehouse"];
                        sampleprice = (decimal)row["sampleprice"];
                        pricemid = (decimal)row["pricemid"];
                        pricemin = (decimal)row["pricemin"];
                        promotionprice = (decimal)row["promotionprice"];
                        lostcartotal = row["LostCartTotal"].ToString().Length > 0 ? (decimal)row["LostCartTotal"] : 0;
                        unitweight = (decimal)row["unitweight"];
                        carronweight = row["CartonWeight"].ToString().Length > 0 ? (decimal)row["CartonWeight"] : 10;
                        instock = (bool)row["instock"];
                        isairmail = (bool)row["isairmail"];
                        battery = (bool)row["battery"];
                        issoldoutpro = (bool)row["issoldoutpro"];
                        supportcoupon = (bool)row["supportcoupon"];
                        liquid = (bool)row["liquid"];
                        promotiontype = (int)row["promotiontype"];
                        bigcategory = (int)row["bigcategory"];
                        midcategory = (int)row["midcategory"];
                        mincategory = (int)row["mincategory"];
                        stock = (int)row["stock"];
                        starttime = row["starttime"].ToString().Length > 0 ? Convert.ToDateTime(row["starttime"]) : DateTime.Now;
                        endtime = row["endtime"].ToString().Length > 0 ? Convert.ToDateTime(row["endtime"]) : DateTime.Now;
                    }

                    string warestock = "0";

                    ds = BLL.ReturnDataSet("select * from Web_FreeGift where sku=" + sku + " and Stock>0", false);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        updatestatus = BLL.ExecuteSqlFun("update Web_FreeGift set stock=stock-1 where sku=" + sku, false);

                        cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), "", "", true, "", "", bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, proPrice, (unitweight + carronweight), leadtime, producturl, shopname, (quantity > 1 ? false : true), true,true, isairmail, true, false, supportcoupon, IsPomotion, battery, liquid, warehouse);

                        string tempgetshippingstyleinfo = Tools.Secrecy.Escape(cart.shipping.getShippingStyle("0"));

                        dayimg = Tools.Secrecy.Escape("1|" + sku + "|" + Tools.Secrecy.Escape(getTuiitemlist(sku)) + "|" + cart.GetSubtotal(warehouse).ToString("C") + "|" + cart.GetShippingTotal().ToString("C") + "|" + (-cart.GetSubDiscount(warehouse)).ToString("C") + "|" + cart.GetTotal(warehouse).ToString("C") + "|" + (cart.GetTotal(warehouse) > limitetrackfee ? "Free" : trackfee.ToString("C")) + "|" + "|" + tempgetshippingstyleinfo + "|" + cart.GetSubtotalNoPm(warehouse).ToString("F") + "|" + proPrice.ToString("C") + "|" + (proPrice * quantity).ToString("C") + "|" + (Config.getConfig.imgPath() + smallpictureurl) + "|" + shortname + "|" + producturl);

                    }
                    else
                        dayimg = Tools.Secrecy.Escape("0|0");
                }
                else if (r == "checkdeals")
                {
                    dayimg = Tools.Secrecy.Escape("0|Sorry, this commercial activity period is invalid.");
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string qty = Tools.Tools.getSafeCode(context.Request["qty"]);
                    SqlDataReader reader = BLL.ReturnValue("select WD.SKU,WD.Price,WD.boughtnumber,WD.dealsad,WD.dealsimglist,WD.dealsdescription,WD.Startimes,WD.EndTimes,VP.FullName,VP.Description,VP.ProductURL,VP.SmallPictureURL,VP.SamplePrice,WD.LimitSalesNum,WD.boughtnumber from Web_Deals WD,Products VP where WD.SKU=VP.SKU and WD.SKU=" + sku, false);
                    DateTime starttime = DateTime.Now, endtime = DateTime.Now, nowtime = DateTime.Now;
                    decimal dealsprice = 0, sampprice = 0;
                    string SmallPictureURL = string.Empty, shortname = string.Empty, producturl = string.Empty;
                    int LimitSalesNum = 0, boughtnumber = 0;
                    if (reader.Read())
                    {

                        dealsprice = reader.GetDecimal(1);
                        shortname = reader.GetString(8);
                        producturl = reader.GetString(10);
                        SmallPictureURL = reader.GetString(11);
                        sampprice = reader.GetDecimal(12);
                        starttime = reader.IsDBNull(6) ? DateTime.Now : reader.GetDateTime(6);
                        endtime = reader.IsDBNull(7) ? DateTime.Now : reader.GetDateTime(7);
                        LimitSalesNum = reader.IsDBNull(13) ? 50 : reader.GetInt32(13);
                        boughtnumber = reader.IsDBNull(14) ? 0 : reader.GetInt32(14);
                    }
                    reader.Close();

                    if (DateTime.Compare(nowtime, starttime) > 0 && DateTime.Compare(endtime, nowtime) > 0)
                    {
                        if (LimitSalesNum > boughtnumber)
                            dayimg = Tools.Secrecy.Escape("1|<p>This is a presale item. The payment you made is for a “Ticket” of the product. We will send you payment link for the rest amount to the email address you provided below   (24 hours before we receive the products)</p><p>Customers paid for this “Ticket” will get it reserved. The shipping date depends on the date provided by the supplier and the date you pay the full amount of the product.</p><p><strong>Email:</strong><input id=\"Email\" class=\"dealemail\" type=\"text\" /></p>");
                        else
                            dayimg = Tools.Secrecy.Escape("0|<p class=\"note\">Sold out</p>");
                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("0|<p class=\"note\">Expired.</p>");
                    }
                }
                else if (r == "dealsbuynow")
                {
                    dayimg = Tools.Secrecy.Escape("0|Sorry, this commercial activity period is invalid.");
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string email = Tools.Tools.getSafeCode(context.Request["email"]);
                    string qty = Tools.Tools.getSafeCode(context.Request["qty"]);
                    SqlDataReader reader = BLL.ReturnValue("select WD.SKU,WD.Price,WD.boughtnumber,WD.dealsad,WD.dealsimglist,WD.dealsdescription,WD.Startimes,WD.EndTimes,VP.FullName,VP.Description,VP.ProductURL,VP.SmallPictureURL,VP.SamplePrice,WD.LimitSalesNum,WD.boughtnumber from Web_Deals WD,Products VP where WD.SKU=VP.SKU and WD.SKU=" + sku, false);
                    DateTime starttime = DateTime.Now, endtime = DateTime.Now, nowtime = DateTime.Now;
                    decimal dealsprice = 0, sampprice = 0;
                    string SmallPictureURL = string.Empty, shortname = string.Empty, producturl = string.Empty;
                    int LimitSalesNum = 0, boughtnumber = 0;
                    if (reader.Read())
                    {
                        dealsprice = reader.GetDecimal(1);
                        shortname = reader.GetString(8);
                        producturl = reader.GetString(10);
                        SmallPictureURL = reader.GetString(11);
                        sampprice = reader.GetDecimal(12);
                        starttime = reader.IsDBNull(6) ? DateTime.Now : reader.GetDateTime(6);
                        endtime = reader.IsDBNull(7) ? DateTime.Now : reader.GetDateTime(7);
                        LimitSalesNum = reader.IsDBNull(13) ? 50 : reader.GetInt32(13);
                        boughtnumber = reader.IsDBNull(14) ? 0 : reader.GetInt32(14);
                    }
                    reader.Close();

                    if (DateTime.Compare(nowtime, starttime) > 0 && DateTime.Compare(endtime, nowtime) > 0)
                    {
                        if (LimitSalesNum >= (boughtnumber + Int32.Parse(qty)))
                        {
                            bool updatestatus = false;
                            updatestatus = BLL.ExecuteSqlFun("update Web_Deals set boughtnumber=boughtnumber+" + qty + " where sku=" + sku, false);
                            string ordernumber = CreateYudingOrders(email, sku, Int32.Parse(qty), dealsprice);

                            dayimg = Tools.Secrecy.Escape("1|" + ordernumber);

                        }
                        else
                            dayimg = Tools.Secrecy.Escape("0|<p class=\"note\">Sold out</p>");
                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("0|<p class=\"note\">Expired.</p>");
                    }

                }
                else if (r == "addnewpointgift")
                {
                    string sku = Tools.Tools.getSafeCode(context.Request["sku"]);
                    string Point = Tools.Tools.getSafeCode(context.Request["Point"]);
                    string Stock = Tools.Tools.getSafeCode(context.Request["Stock"]);


                    bool updatestatus = false;
                    updatestatus = BLL.ExecuteSqlFun("insert Point_exchangeproducts(SKU,Stock,Point,ExchangeNumber) values(" + sku + "," + Stock + "," + Point + ",0)", false);
                    if (updatestatus)
                        dayimg = Tools.Secrecy.Escape("1|Add successfully");
                    else
                        dayimg = Tools.Secrecy.Escape("0|err");

                }
                else if (r == "getmodelshow")
                {

                    int nowpage = context.Request["page"] == null ? 1 : Int32.Parse(context.Request["page"]);
                    int pagecount = 30;
                    string searchby = "id", keyword = "0", whereStr=string.Empty;

                  

                    DataSet ds = new DataSet();
                    int totalCounts = 0;

                    Model m = new Model();
                    m.DateTable = "Web_ShareImage";
                    m.soryType = " id desc";
                    m.KeyTitle = "id";
                    m.pagesize = pagecount;
                    m.pageindex = nowpage.ToString();
                    m.SearchByTitle = "id";
                    m.SearchByType = "no";
                    m.keyword = "0";
                    m.WhereSQL = "";
                                                           
                    ds = BLL.ReturnTongYong("TongYongFenYe", m);
                    totalCounts = BLL.getTongYongCount("TongYongFenYeCount", m);


                   // ds = BLL.ReturnDataSet("select * from Web_ShareImage", false);

                  

                    string outhtmles = "[";
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                                outhtmles += "{\"picture\": \"" + ds.Tables[0].Rows[i]["Images"].ToString() + "\", \"title\": \"" + ds.Tables[0].Rows[i]["Title"].ToString() + "\", \"sku\": " + ds.Tables[0].Rows[i]["sku"].ToString() + ", \"time\": \"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Timesamp"]).ToShortDateString() + "\", \"author\": \" " + ds.Tables[0].Rows[i]["author"].ToString() + "\", \"target\": \"javascript:;\"}";
                            else
                                outhtmles += ",{\"picture\": \"" + ds.Tables[0].Rows[i]["Images"].ToString() + "\", \"title\": \"" + ds.Tables[0].Rows[i]["Title"].ToString() + "\", \"sku\": " + ds.Tables[0].Rows[i]["sku"].ToString() + ", \"time\": \"" + Convert.ToDateTime(ds.Tables[0].Rows[i]["Timesamp"]).ToShortDateString() + "\", \"author\": \" " + ds.Tables[0].Rows[i]["author"].ToString() + "\", \"target\": \"javascript:;\"}";
                        }
                    }
                    
                    outhtmles += "]";
                    HttpContext.Current.Response.Write(outhtmles);
                   HttpContext.Current.Response.End();

                }
                else if (r == "addvipcodefee")
                {
                    ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

                    cart.clearCart("0");

                    string fee = Tools.Tools.getSafeCode(context.Request["fee"]);
                    string sku = "1002";
                    string codeimg = "", color = string.Empty, mode = string.Empty, smallpictureurl = string.Empty, tempFullname = string.Empty, shortname = string.Empty;
                    int bigcategory = 0, midcategory = 0, mincategory = 0, discount = 0, quantity = 1;

                    decimal proPrice = 5, OldPrice = 10;
                    shortname = "vip code";


                    bool isinstock = true;

                    codeimg = "/images/hour40.jpg";

                    cart.Add(Int32.Parse(sku), sku, Int32.Parse(sku), color, mode, isinstock, (isinstock ? "" : "Delivery time may take 3-7 days"), (isinstock ? "Required 2-7 business days for processing" : ""), bigcategory, midcategory, mincategory, discount, 0, smallpictureurl, "", tempFullname + shortname, quantity, proPrice, OldPrice, 0, "", "javascript:;", "hotecig", (quantity > 1 ? false : true), false,false, true, true, false, false, true, false, false, "0", 0); //这 里 是 减 库 存 商 品 

                    //cart.Add(-8888, "-8888", -8888, "", "", true, "", "", 0, 0, 0, 0, 0, "javascript:void(0)", "", "TrackNumber ", 1, 0, 0, 0, "TrackNumber ", "javascript:void(0)", website.Replace(".com", "").Replace(".net", "").Replace(".cn", ""), true, false, true, false, false, false, false, false, false, "0");

                    //cart.Add(sku, sku.ToString(), sku, "wallbuy.com", 0, codeimg, "vip code", 1, Convert.ToDecimal(fee), Convert.ToDecimal(fee), 50, "30 hours", "javascript:void(0)", true, false, true, false, false, false, true, false, false, "dt");

                    cart.shipping.ShippingFee = 0;
                    cart.shipping.CountryID = "-1";
                    cart.shipping.ShippingMethod = "default";

                    dayimg = Tools.Secrecy.Escape("1|ok");

                }
                else if (r == "checkpoorder")
                {
                    string username = string.Empty;
                    string paypalemail = string.Empty;

                    dayimg = Tools.Secrecy.Escape("0|error with you,Try again.");
                    string d = context.Request["pdate"];
                    string ordernumber = string.Empty;
                    if (d.IndexOf("|") > -1)
                    {
                        ordernumber = d.Split('|')[0].Trim();
                        paypalemail = d.Split('|')[1].Trim();
                        username = paypalemail;
                    }
                    else
                    {
                        ordernumber = d;
                        username = Tools.Cookie.GetEncryptedCookieValue("useremail");
                        paypalemail = Tools.Cookie.GetEncryptedCookieValue("payemail");
                    }

                    DataSet ds = new DataSet();
                    ds = BLL.ReturnDataSet("select ordernumber,Typegift,TypeCommer,TypeDOC,TypeOther,Pricevalue,PriceTotalvalue from OrderPOlist where ordernumber='" + ordernumber + "' and (email='" + username + "' or email='" + paypalemail + "')", false);

                    bool ishaveorder = false;
                    if (ds.Tables[0].Rows.Count > 0)
                        ishaveorder = true;

                    string potypestr = "giftselect";

                    if (ishaveorder)
                    {
                        if (Convert.ToBoolean(ds.Tables[0].Rows[0]["Typegift"]))
                            potypestr = "giftselect";
                        if (Convert.ToBoolean(ds.Tables[0].Rows[0]["TypeCommer"]))
                            potypestr = "comselect";
                        if (Convert.ToBoolean(ds.Tables[0].Rows[0]["TypeDOC"]))
                            potypestr = "docselect";
                        if (Convert.ToBoolean(ds.Tables[0].Rows[0]["TypeOther"]))
                            potypestr = "otherselect";
                        dayimg = Tools.Secrecy.Escape("1|" + potypestr + "|" + Convert.ToDecimal(ds.Tables[0].Rows[0]["Pricevalue"]).ToString("C") + "|" + Convert.ToDecimal(ds.Tables[0].Rows[0]["PriceTotalvalue"]).ToString("C"));
                    }
                    else
                    {
                        ds = BLL.ReturnDataSet("select OrderNumber,ShippingCountry,TotalPayment from View_OrderWithStatus where OrderNumber='" + ordernumber + "' and (email='" + username + "' or email='" + paypalemail + "') and OrderStatus in (1,20)", false);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            if (ds.Tables[0].Rows[0]["ShippingCountry"].ToString().ToUpper() == "GERMANY")
                                dayimg = Tools.Secrecy.Escape("1|otherselect|" + Convert.ToDecimal(ds.Tables[0].Rows[0]["TotalPayment"]).ToString("C") + "|" + Convert.ToDecimal(ds.Tables[0].Rows[0]["TotalPayment"]).ToString("C"));
                            else
                                dayimg = Tools.Secrecy.Escape("1|giftselect|" + Convert.ToDecimal(ds.Tables[0].Rows[0]["TotalPayment"]).ToString("C") + "|" + Convert.ToDecimal(ds.Tables[0].Rows[0]["TotalPayment"]).ToString("C"));
                        }
                        else
                            dayimg = Tools.Secrecy.Escape("0|error with you,Try again or your order can not change your po list.");

                    }

                }
                else if (r == "complageorder")
                {
                    string ordernumber = Tools.Tools.getSafeCode(context.Request["ordernumber"]);
                    string remark = Tools.Tools.getSafeCode(context.Request["remark"]);
                    string email = "guest";
                    bool isok = false;
                    SqlDataReader reader = BLL.ReturnValue("select ordernumber from View_OrderWithStatus where ordernumber='" + ordernumber + "' and OrderStatus in (65,60)", false);

                    if (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false)
                            isok = true;
                    }
                    reader.Close();

                    if (isok)
                    {
                        SqlConnection con = DAL.localwcon();
                        SqlCommand com = new SqlCommand("insert Web_OrderComplate(OrderNumber,Remark,Timestamp) values(@OrderNumber,@Remark,@Timestamp)", con);

                        com.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = ordernumber;
                        com.Parameters.Add("@Remark", SqlDbType.VarChar).Value = remark;
                        com.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;

                        dayimg = Tools.Secrecy.Escape("0|Unknown error.");
                        con.Open();
                        try
                        {
                            com.ExecuteNonQuery();

                            if (Tools.Cookie.CookieCheck("useremail"))
                                email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                            else
                                email = "guest";

                            com = new SqlCommand(@"INSERT OrderStatus (OrderNumber, Timestamp, Username, StatusCode, Notes) VALUES (@ordernumber,@Timestamp,@Username,@StatusCode,@Notes)", con);
                            com.Parameters.Add("@ordernumber", SqlDbType.VarChar).Value = ordernumber;
                            com.Parameters.Add("@timestamp", SqlDbType.DateTime).Value = DateTime.Now;
                            com.Parameters.Add("@username", SqlDbType.VarChar).Value = email;
                            com.Parameters.Add("@statuscode", SqlDbType.VarChar).Value = 80;
                            com.Parameters.Add("@notes", SqlDbType.VarChar).Value = "Completed the orders By " + email;
                            com.ExecuteNonQuery();



                        }
                        catch { }


                        decimal OrderPayment = getOrderPayment(ordernumber);
                        int fenpoint = (int)Math.Round(OrderPayment, 0);
                        string leftStr = fenpoint.ToString().Substring(fenpoint.ToString().Length - 1, 1).ToString();


                        if (Int32.Parse(leftStr) > 7)
                        {
                            fenpoint = fenpoint / 10 + 1;
                        }
                        else
                        {
                            fenpoint = fenpoint / 10;
                        }

                        com = new SqlCommand("update Web_Point set point=point+@point,TotalPoint=TotalPoint+@point,Timestamp=@Timestamp where email=@email", con);
                        com.Parameters.Add("@email", SqlDbType.VarChar).Value = email;
                        com.Parameters.Add("@point", SqlDbType.BigInt).Value = fenpoint;
                        com.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
                        com.ExecuteNonQuery();


                        //给推广者积分
                        string apemail = getemail(email);

                        if (apemail.Length > 5)
                        {
                            com = new SqlCommand("update Web_Point set point=point+@point,TotalPoint=TotalPoint+@point,Timestamp=@Timestamp where email=@email", con);
                            com.Parameters.Add("@email", SqlDbType.VarChar).Value = apemail.Split('|')[0].ToString();
                            com.Parameters.Add("@point", SqlDbType.BigInt).Value = Convert.ToInt64(apemail.Split('|')[1].ToString());
                            com.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
                            com.ExecuteNonQuery();

                            com = new SqlCommand("update Point_Log set status=2 where email=@email and ordernumber=@ordernumber", con);
                            com.Parameters.Add("@email", SqlDbType.VarChar).Value = apemail.Split('|')[0].ToString();
                            com.Parameters.Add("@ordernumber", SqlDbType.VarChar).Value = ordernumber;
                            com.ExecuteNonQuery();
                        }

                        com = new SqlCommand("insert Web_Point_Log(TransactionNumber,Amount,Points,Status,Notes,Email,OrderNumber,Timeamp,Pointype) values(@TransactionNumber,@Amount,@Points,2,@Notes,@Email,@OrderNumber,@Timeamp,1)", con);
                        com.Parameters.Add("@TransactionNumber", SqlDbType.VarChar).Value = "-";
                        com.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = ordernumber;

                        com.Parameters.Add("@Amount", SqlDbType.Money).Value = OrderPayment;
                        com.Parameters.Add("@Points", SqlDbType.Int).Value = fenpoint;
                        com.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = "W order";
                        com.Parameters.Add("@Email", SqlDbType.VarChar).Value = email;
                        com.Parameters.Add("@Timeamp", SqlDbType.DateTime).Value = DateTime.Now;
                        com.ExecuteNonQuery();




                        dayimg = Tools.Secrecy.Escape("1|ok");


                        con.Close();

                    }
                    else
                    {
                        dayimg = Tools.Secrecy.Escape("0|Order can not Completed.");
                    }
                }
                else if (r == "getdrawlist")
                {
                    dayimg = Tools.Secrecy.Escape("0|Sorry, your have submitted.");

                    DataSet ds = new DataSet();
                    ds = BLL.ReturnDataSet("select top 13 D.ID,D.Email,D.Wintype,A.nick from Web_Drawlist D,ACT_Accounts A where A.EMail=D.Email order by D.Timeamps desc", false);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        dayimg = "1";
                        sb.Remove(0, sb.Length);
                        sb.Append("{\"Results\":\"" + dayimg + "\",\"rank\":[");
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            sb.Append("{\"id\":\"" + ds.Tables[0].Rows[i]["id"].ToString() + "\",\"wintype\":\"" + ds.Tables[0].Rows[i]["Wintype"].ToString() + "\",\"nick\":\"" + ds.Tables[0].Rows[i]["nick"].ToString() + "\"}");
                            sb.Append(",");
                        }

                        sb.Append("{\"id\":\"0\",\"wintype\":\"0\",\"nick\":\"0\"}]}");
                        context.Response.Write(sb.ToString());
                        context.Response.End();
                    }

                }
                else if (r == "checkuserpointstatus")
                {
                    dayimg = Tools.Secrecy.Escape("0|Sorry, your have submitted.");

                    string usrename = "guest";
                    if (Tools.Cookie.CookieCheck("useremail") == true)
                        usrename = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    int nowpoint = 0;

                    SqlDataReader reader = BLL.ReturnValue("select point from Web_Point where email='" + usrename + "'", false);
                    if (reader.Read())
                    {
                        nowpoint = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                    reader.Close();

                    if (usrename != "guest")
                    {
                        if (nowpoint >= 10)
                        {
                            bool updatestatus = false;
                            updatestatus = BLL.ExecuteSqlFun("update Web_Point set point=point-10,CostPoint=CostPoint+10 where email='" + usrename + "'", false);

                            string shippingstylestr = PageSource.PageSource.GetPublickSocur("drawlist.ptshop").Trim();
                            string[] shippingstylelist = shippingstylestr.Split('\r');
                            string outtemptxt = "";
                            for (int i = 0; i < shippingstylelist.Length; i++)
                            {
                                if (i == 0)
                                    outtemptxt += "{\"id\":" + (i + 1) + ",\"types\":\"" + shippingstylelist[i].Split('|')[0].Trim() + "\",\"prize\":\"" + shippingstylelist[i].Split('|')[1].Trim() + "\",\"v\":" + shippingstylelist[i].Split('|')[3].Trim() + "}";
                                else
                                    outtemptxt += ",{\"id\":" + (i + 1) + ",\"types\":\"" + shippingstylelist[i].Split('|')[0].Trim() + "\",\"prize\":\"" + shippingstylelist[i].Split('|')[1].Trim() + "\",\"v\":" + shippingstylelist[i].Split('|')[3].Trim() + "}";
                            }
                            outtemptxt += "";



                            dayimg = Tools.Secrecy.Escape("1|" + (nowpoint - 10).ToString() + "|" + usrename + "|" + getContrylistByname() + "|" + outtemptxt);
                        }
                        else
                            dayimg = Tools.Secrecy.Escape("2|err");
                    }

                    sb.Remove(0, sb.Length);
                    sb.Append("{\"Results\":\"" + dayimg + "\"}");
                    context.Response.Write(sb.ToString());
                    context.Response.End();

                }
                else if (r == "checkexchangeforproduct")
                {
                    dayimg = Tools.Secrecy.Escape("0|Sorry, your have submitted.");
                    string sku = context.Request["sku"];

                    string usrename = "guest";
                    if (Tools.Cookie.CookieCheck("useremail") == true)
                        usrename = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    int nowpoint = 0, skupoint = 0, skustock = 0;
                    SqlDataReader reader = BLL.ReturnValue("select point from Web_Point where email='" + usrename + "'", false);
                    if (reader.Read())
                    {
                        nowpoint = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                    reader.Close();


                    reader = BLL.ReturnValue("select Stock,Point from Point_exchangeproducts where sku=" + sku + "", false);
                    if (reader.Read())
                    {
                        skupoint = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        skustock = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                    reader.Close();

                    if (usrename != "guest")
                    {
                        if (skustock > 0)
                        {
                            if (nowpoint >= skupoint)
                                dayimg = Tools.Secrecy.Escape("1|ok|" + getContrylistByname());
                            else
                                dayimg = Tools.Secrecy.Escape("2|Sorry, your points is not enough.|err");
                        }
                        else
                        {
                            dayimg = Tools.Secrecy.Escape("2|sold out|err");
                        }
                    }

                }
                else if (r == "getexchangestockbysku")
                {
                    dayimg = Tools.Secrecy.Escape("0|Sorry, your have submitted.");
                    string sku = context.Request["sku"];

                    string usrename = "guest";
                    if (Tools.Cookie.CookieCheck("useremail") == true)
                        usrename = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    int nowpoint = 0, skupoint = 0, skustock = 0;
                    SqlDataReader reader = BLL.ReturnValue("select Stock,Point from Point_exchangeproducts where sku=" + sku + "", false);
                    if (reader.Read())
                    {
                        skupoint = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        skustock = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                    reader.Close();
                    if (skustock > 0)
                        dayimg = Tools.Secrecy.Escape("1|" + skustock);

                }
                else if (r == "createnewgiftorder")
                {
                    dayimg = Tools.Secrecy.Escape("0|Sorry, your have submitted.");
                    string email = "guest@wallbuys.com";
                    if (Tools.Cookie.CookieCheck("useremail") == true)
                        email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    string txtNewShippingFirstName = context.Request["txtNewShippingFirstName"];
                    string txtNewShippingLastName = context.Request["txtNewShippingLastName"];
                    string txtNewShippingCompany = context.Request["txtNewShippingCompany"];
                    string txtNewShippingStreetAddress = context.Request["txtNewShippingStreetAddress"];
                    string txtNewShippingCity = context.Request["txtNewShippingCity"];
                    string txtNewShippingState = context.Request["txtNewShippingState"];
                    string txtNewShippingZip = context.Request["txtNewShippingZip"];
                    string txtNewShippingCountry = context.Request["txtNewShippingCountry"];
                    string txtNewShippingTelephone = context.Request["txtNewShippingTelephone"];
                    string sku = context.Request["sku"];


                    int nowpoint = 0, skupoint = 0, skustock = 0;
                    SqlDataReader reader = BLL.ReturnValue("select Stock,Point from Point_exchangeproducts where sku=" + sku + ";", false);
                    if (reader.Read())
                    {
                        skupoint = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        skustock = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                    reader.Close();

                    bool updatestatus = false;
                    updatestatus = BLL.ExecuteSqlFun("update Point_exchangeproducts set stock=stock-1 where sku=" + sku + ";", false);
                    updatestatus = BLL.ExecuteSqlFun("update Web_Point set Point=Point-" + skupoint + ",CostPoint=CostPoint+" + skupoint + " where Email='" + email + "';", false);

                    reader = BLL.ReturnValue("select Point from Web_Point where email='" + email + "';", false);
                    if (reader.Read())
                    {
                        nowpoint = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                    reader.Close();


                    string ordernumber = CreateGiftOrders(email, txtNewShippingFirstName, txtNewShippingLastName, txtNewShippingStreetAddress, txtNewShippingCity, txtNewShippingState, txtNewShippingCountry, txtNewShippingZip, txtNewShippingTelephone, txtNewShippingCompany, sku);

                    dayimg = Tools.Secrecy.Escape("1|" + nowpoint + "|" + skustock + "|" + ordernumber);

                }
                else if (r == "getpointstatus")
                {
                    dayimg = Tools.Secrecy.Escape("0|Sorry, your have submitted.");

                    string usrename = "guest";
                    if (Tools.Cookie.CookieCheck("useremail") == true)
                        usrename = Tools.Cookie.GetEncryptedCookieValue("useremail");

                    int nowpoint = 0;

                    SqlDataReader reader = BLL.ReturnValue("select point from Web_Point where email='" + usrename + "'", false);
                    if (reader.Read())
                    {
                        nowpoint = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                    reader.Close();

                    string temptxt = "";

                    if (nowpoint >= 500)
                        temptxt += "        <option value=\"500\">$50.00</option>";
                    if (nowpoint >= 300)
                        temptxt += "        <option value=\"300\">$30.00</option>";
                    if (nowpoint >= 200)
                        temptxt += "        <option value=\"200\">$20.00</option>";
                    if (nowpoint >= 100)
                        temptxt += "        <option value=\"100\">$10.00</option>";
                    if (nowpoint >= 50)
                        temptxt += "        <option value=\"50\">$5.00</option>";
                    if (nowpoint < 50)
                        temptxt += "        <option value=\"0\">No Item</option>";

                    dayimg = Tools.Secrecy.Escape("1|" + temptxt + "|" + usrename + "|" + nowpoint);

                }
                else if (r == "exchangenewcode")
                {
                    GetConfigInfo configinfo = new GetConfigInfo();
                    string website = Tools.Secrecy.Decodes(GetConfigInfo.GetAppSettings(configinfo.getEmailconfigfile, "website"));

                    dayimg = Tools.Secrecy.Escape("0|Sorry, your have submitted.");
                    string codevalue = context.Request["codevalue"];
                    string email = context.Request["email"];

                    string tempPcode = "";
                    tempPcode = Tools.Tools.egetcode(Tools.Tools.Str_char(10, true));

                    if (Int32.Parse(codevalue) > 0)
                    {
                        SqlConnection sqlConn = DAL.localwcon();
                        SqlCommand cmdDB = new SqlCommand(@"INSERT Web_Coupon(CouponNumber,Coupontype,LostPrice,CouponValue,lostPrice2,CouponValue2,CouponEmail,Ispaid,Creattimeamp,Startimeamp,Endtimeamp,IsUSD,SupportCategory,SupportSku,CouponDesign,StoreName) values(@CouponNumber,@Coupontype,@LostPrice,@CouponValue,@lostPrice2,@CouponValue2,@CouponEmail,@Ispaid,@Creattimeamp,@Startimeamp,@Endtimeamp,@IsUSD,@SupportType,@SupportSku,@CouponDesign,@StoreName)", sqlConn);
                        cmdDB.Parameters.Add("@CouponNumber", SqlDbType.VarChar).Value = tempPcode;
                        cmdDB.Parameters.Add("@Coupontype", SqlDbType.Int).Value = 1;
                        cmdDB.Parameters.Add("@LostPrice", SqlDbType.Decimal).Value = (decimal.Parse(codevalue) / 10) * 2;
                        cmdDB.Parameters.Add("@CouponValue", SqlDbType.Decimal).Value = decimal.Parse(codevalue) / 10;
                        cmdDB.Parameters.Add("@lostPrice2", SqlDbType.Decimal).Value = 100000;
                        cmdDB.Parameters.Add("@CouponValue2", SqlDbType.Decimal).Value = 0;

                        cmdDB.Parameters.Add("@CouponEmail", SqlDbType.VarChar).Value = email;
                        cmdDB.Parameters.Add("@Ispaid", SqlDbType.Bit).Value = false;
                        cmdDB.Parameters.Add("@Creattimeamp", SqlDbType.DateTime).Value = DateTime.Now;
                        cmdDB.Parameters.Add("@Startimeamp", SqlDbType.DateTime).Value = DateTime.Now;
                        cmdDB.Parameters.Add("@Endtimeamp", SqlDbType.DateTime).Value = DateTime.Now.AddYears(2);
                        cmdDB.Parameters.Add("@IsUSD", SqlDbType.Bit).Value = true;

                        cmdDB.Parameters.Add("@SupportType", SqlDbType.VarChar).Value = "";
                        cmdDB.Parameters.Add("@SupportSku", SqlDbType.VarChar).Value = "";
                        cmdDB.Parameters.Add("@CouponDesign", SqlDbType.VarChar).Value = "from points";
                        cmdDB.Parameters.Add("@StoreName", SqlDbType.VarChar).Value = website;

                        cmdDB.Connection.Open();
                        cmdDB.ExecuteNonQuery();

                        cmdDB = new SqlCommand(@"update Web_Point set point=point-@point,CostPoint=CostPoint+@point where email=@email", sqlConn);
                        cmdDB.Parameters.Add("@email", SqlDbType.VarChar).Value = email;
                        cmdDB.Parameters.Add("@point", SqlDbType.Int).Value = Int32.Parse(codevalue);
                        cmdDB.ExecuteNonQuery();
                        cmdDB.Connection.Close();

                        dayimg = Tools.Secrecy.Escape("1|" + tempPcode + "|" + DateTime.Now.AddYears(2).ToShortDateString());
                    }
                    else
                        dayimg = Tools.Secrecy.Escape("0|Sorry, your points is not enough.");
                }
                else if (r == "exchangenewcodebuydraw")
                {
                    dayimg = Tools.Secrecy.Escape("0|Sorry, your have submitted.");
                    string codevalue = context.Request["codevalue"];
                    string email = context.Request["email"];

                    string tempPcode = "";
                    tempPcode = Tools.Tools.egetcode(Tools.Tools.Str_char(10, true));

                    if (Int32.Parse(codevalue) > 0)
                    {
                        SqlConnection sqlConn = DAL.localwcon();
                        SqlCommand cmdDB = new SqlCommand(@"INSERT Web_Coupon(CouponNumber,Coupontype,LostPrice,CouponValue,lostPrice2,CouponValue2,CouponEmail,Ispaid,Creattimeamp,Startimeamp,Endtimeamp,IsUSD,SupportCategory,SupportSku,CouponDesign,StoreName) values(@CouponNumber,@Coupontype,@LostPrice,@CouponValue,@lostPrice2,@CouponValue2,@CouponEmail,@Ispaid,@Creattimeamp,@Startimeamp,@Endtimeamp,@IsUSD,@SupportType,@SupportSku,@CouponDesign,@StoreName)", sqlConn);
                        cmdDB.Parameters.Add("@CouponNumber", SqlDbType.VarChar).Value = tempPcode;
                        cmdDB.Parameters.Add("@Coupontype", SqlDbType.Int).Value = 1;
                        cmdDB.Parameters.Add("@LostPrice", SqlDbType.Decimal).Value = (decimal.Parse(codevalue)) * 4;
                        cmdDB.Parameters.Add("@CouponValue", SqlDbType.Decimal).Value = decimal.Parse(codevalue);
                        cmdDB.Parameters.Add("@lostPrice2", SqlDbType.Decimal).Value = 100000;
                        cmdDB.Parameters.Add("@CouponValue2", SqlDbType.Decimal).Value = 0;

                        cmdDB.Parameters.Add("@CouponEmail", SqlDbType.VarChar).Value = email;
                        cmdDB.Parameters.Add("@Ispaid", SqlDbType.Bit).Value = false;
                        cmdDB.Parameters.Add("@Creattimeamp", SqlDbType.DateTime).Value = DateTime.Now;
                        cmdDB.Parameters.Add("@Startimeamp", SqlDbType.DateTime).Value = DateTime.Now;
                        cmdDB.Parameters.Add("@Endtimeamp", SqlDbType.DateTime).Value = DateTime.Now.AddYears(2);
                        cmdDB.Parameters.Add("@IsUSD", SqlDbType.Bit).Value = true;

                        cmdDB.Parameters.Add("@SupportType", SqlDbType.VarChar).Value = "";
                        cmdDB.Parameters.Add("@SupportSku", SqlDbType.VarChar).Value = "";
                        cmdDB.Parameters.Add("@CouponDesign", SqlDbType.VarChar).Value = "from points";
                        cmdDB.Parameters.Add("@CouponTitle", SqlDbType.VarChar).Value = "";
                        cmdDB.Parameters.Add("@StoreName", SqlDbType.VarChar).Value = "Wallbuys.com";

                        cmdDB.Connection.Open();
                        cmdDB.ExecuteNonQuery();

                        cmdDB = new SqlCommand(@"insert Web_Drawlist(Email,Wintype) values(@Email,@Wintype)", sqlConn);
                        cmdDB.Parameters.Add("@Email", SqlDbType.VarChar).Value = email;
                        cmdDB.Parameters.Add("@Wintype", SqlDbType.VarChar).Value = "Coupon: " + Convert.ToDecimal(codevalue).ToString("C");
                        cmdDB.ExecuteNonQuery();
                        cmdDB.Connection.Close();

                        dayimg = Tools.Secrecy.Escape("1|" + tempPcode + "|" + DateTime.Now.AddYears(2).ToShortDateString());
                    }
                    else
                        dayimg = Tools.Secrecy.Escape("0|Sorry, your points is not enough.");
                }
                else if (r == "createneworderbydraw")
                {
                    dayimg = Tools.Secrecy.Escape("0|Sorry, your have submitted.");
                    string email = "guest@wallbuys.com";
                    if (Tools.Cookie.CookieCheck("useremail") == true)
                        email = Tools.Cookie.GetEncryptedCookieValue("useremail");
                    string txtNewShippingFirstName = context.Request["txtNewShippingFirstName"];
                    string txtNewShippingLastName = context.Request["txtNewShippingLastName"];
                    string txtNewShippingCompany = context.Request["txtNewShippingCompany"];
                    string txtNewShippingStreetAddress = context.Request["txtNewShippingStreetAddress"];
                    string txtNewShippingCity = context.Request["txtNewShippingCity"];
                    string txtNewShippingState = context.Request["txtNewShippingState"];
                    string txtNewShippingZip = context.Request["txtNewShippingZip"];
                    string txtNewShippingCountry = context.Request["txtNewShippingCountry"];
                    string txtNewShippingTelephone = context.Request["txtNewShippingTelephone"];

                    string sku = context.Request["sku"];
                    SqlConnection sqlConn = DAL.localwcon();
                    SqlCommand cmdDB = new SqlCommand(@"insert Web_Drawlist(Email,Wintype) values(@Email,@Wintype)", sqlConn);
                    cmdDB.Parameters.Add("@Email", SqlDbType.VarChar).Value = email;
                    cmdDB.Parameters.Add("@Wintype", SqlDbType.VarChar).Value = "Win sku: " + sku;

                    cmdDB.Connection.Open();
                    cmdDB.ExecuteNonQuery();
                    cmdDB.Connection.Close();



                    string ordernumber = CreateOrdersbydraw(email, txtNewShippingFirstName, txtNewShippingLastName, txtNewShippingStreetAddress, txtNewShippingCity, txtNewShippingState, txtNewShippingCountry, txtNewShippingZip, txtNewShippingTelephone, txtNewShippingCompany, sku);

                    dayimg = Tools.Secrecy.Escape("1|" + ordernumber);

                }
                else if (r == "paymentexpress")
                {
                    //string billingfirstname = string.Empty, billinglastname = string.Empty, billingname = string.Empty, billingemail = string.Empty, billingcompany = string.Empty, billingZipCode = string.Empty, billingAddress = string.Empty, billingpostNumber = string.Empty, billingCity = string.Empty, billingState = string.Empty, billingphoneNumber = string.Empty, billingCountry;
                    //string shippingfirstname = string.Empty, shippingname = string.Empty, shippinglastname = string.Empty, shippingcompany = string.Empty, shippingZipCode = string.Empty, shippingAddress = string.Empty, shippingpostnumber = string.Empty, shippingCity = string.Empty, shippingState = string.Empty, shippingphoneNumber = string.Empty, shippingCountry;

                    //string warecode = string.Empty;


                    //shippingfirstname = context.Request["shippingfirstname"].ToString();
                    //shippinglastname = context.Request["shippinglastname"].ToString();
                    //shippingname = shippingfirstname + "." + shippinglastname;
                    //shippingcompany = context.Request["shippingcompany"].ToString();
                    //shippingZipCode = context.Request["shippingZipCode"].ToString();
                    //shippingAddress = context.Request["shippingAddress"].ToString();
                    //shippingCity = context.Request["shippingCity"].ToString();
                    //shippingState = context.Request["shippingState"].ToString();
                    //shippingpostnumber = context.Request["shippingpostnumber"].ToString();
                    //shippingCountry = context.Request["shippingCountry"].ToString();
                    //shippingphoneNumber = context.Request["shippingCountry"].ToString();
                    //warecode = context.Request["warecode"].ToString();

                    //billingfirstname = context.Request["billingfirstname"].ToString();
                    //billinglastname = context.Request["billinglastname"].ToString();
                    //billingname = billingfirstname + "." + billinglastname;
                    //billingemail = context.Request["billingemail"].ToString();
                    //billingcompany = context.Request["billingcompany"].ToString();
                    //billingZipCode = context.Request["billingZipCode"].ToString();
                    //billingAddress = context.Request["billingAddress"].ToString();
                    //billingpostNumber = context.Request["billingpostNumber"].ToString();
                    //billingCity = context.Request["billingCity"].ToString();
                    //billingState = context.Request["billingState"].ToString();

                    //billingphoneNumber = context.Request["billingphoneNumber"].ToString();
                    //billingCountry = context.Request["billingCountry"].ToString();

                    //ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);


                    //string currency = "USD";
                    //string name = shippingfirstname + " " + shippinglastname;
                    //string SHIPTOSTREET = shippingcompany + " " + shippingAddress + " " + shippingpostnumber;
                    //string SHIPTOCITY = shippingCity;
                    //string SHIPTOSTATE = shippingState;
                    //string SHIPTOCOUNTRYCODE = shippingCountry;
                    //string SHIPTOZIP = shippingZipCode;


                    //string L_NAME1 = this.L_NAME1.Text;
                    //string L_AMT1 = this.L_AMT1.Text;
                    //string L_QTY1 = this.L_QTY1.Text;

                    //string L_NAME0 = this.L_NAME0.Text;
                    //string L_AMT0 = this.L_AMT0.Text;
                    //string L_QTY0 = this.L_QTY0.Text;


                    //string OrderId = PaypalProvider.IDzdy("PayPal");//订单号
                    //string OrderName = L_NAME0 + "(" + L_AMT0 + "*" + L_QTY0 + ");" + L_NAME1 + "(" + L_AMT1 + "*" + L_QTY1 + ")";


                    //string hots = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

                    //NVPCodec encoder = new NVPCodec();
                    //encoder.Add("PAYMENTACTION", "Sale");

                    ////不允许客户改地址
                    //encoder.Add("ADDROVERRIDE", "1");
                    //encoder.Add("CANCELURL", hots + "/Pay/Index");
                    //encoder.Add("CURRENCYCODE", currency);
                    //encoder.Add("SHIPTONAME", name);
                    //encoder.Add("SHIPTOSTREET", SHIPTOSTREET);
                    //encoder.Add("SHIPTOCITY", SHIPTOCITY);
                    //encoder.Add("SHIPTOSTATE", SHIPTOSTATE);
                    //encoder.Add("SHIPTOCOUNTRYCODE", SHIPTOCOUNTRYCODE);
                    //encoder.Add("SHIPTOZIP", SHIPTOZIP);



                    //encoder.Add("L_NAME0", L_NAME0); //fullname
                    //encoder.Add("L_NUMBER0", "1000"); //sku
                    //encoder.Add("L_DESC0", "Size: 8.8-oz");
                    //encoder.Add("L_AMT0", L_AMT0); // 价格
                    //encoder.Add("L_QTY0", L_QTY0);

                    //encoder.Add("L_NAME1", L_NAME1); //fullname
                    //encoder.Add("L_NUMBER1", "10001"); //sku
                    //encoder.Add("L_DESC1", "Size: Two 24-piece boxes");
                    //encoder.Add("L_AMT1", L_AMT1); // 价格
                    //encoder.Add("L_QTY1", L_QTY1);



                    //encoder.Add("L_ITEMWEIGHTVALUE1", "0.5");
                    //encoder.Add("L_ITEMWEIGHTUNIT1", "lbs");

                    //double ft = double.Parse(L_QTY0) * double.Parse(L_AMT0) + double.Parse(L_QTY1) * double.Parse(L_AMT1);
                    //encoder.Add("ITEMAMT", ft.ToString());
                    //encoder.Add("TAXAMT", "0.01");

                    //double expressFee = Convert.ToDouble(this.txtExpressFee.Text);//物流费用
                    //double amt = System.Math.Round(ft + expressFee, 2); //总付费


                    //double maxamt = System.Math.Round(amt + 25.00f, 2);

                    //encoder.Add("SHIPDISCAMT", "-3.00");
                    //encoder.Add("AMT", amt.ToString());

                    ////这里填写返回地址，在IIS中时，去掉Test
                    //string returnURL = hots + "Test/PayReturn.aspx?amount=" + amt.ToString() + "&OrderId=" + OrderId;


                    //encoder.Add("RETURNURL", returnURL);
                    //encoder.Add("SHIPPINGAMT", "8.00");
                    //encoder.Add("MAXAMT", maxamt.ToString());

                    //encoder.Add("INSURANCEOPTIONOFFERED", "true");
                    //encoder.Add("INSURANCEAMT", "1.00");
                    //encoder.Add("LOCALECODE", "US");
                    //encoder.Add("NOSHIPPING", "1");

                    //encoder.Add("L_SHIPPINGOPTIONISDEFAULT0", "false");
                    //encoder.Add("L_SHIPPINGOPTIONNAME0", "Ground");
                    //encoder.Add("L_SHIPPINGOPTIONLABEL0", "UPS Ground 7 Days");
                    //encoder.Add("L_SHIPPINGOPTIONAMOUNT0", "3.00");

                    //encoder.Add("L_SHIPPINGOPTIONISDEFAULT1", "true");
                    //encoder.Add("L_SHIPPINGOPTIONNAME1", "UPS Air");
                    //encoder.Add("L_SHIPPINGOPTIONlABEL1", "UPS Next Day Air");
                    //encoder.Add("L_SHIPPINGOPTIONAMOUNT1", "8.00");

                    //encoder.Add("CALLBACKTIMEOUT", "4");



                    //DateTime Addtime = DateTime.Now;
                    ////写入数据库订单

                    ////下面是用代码加载连接access数据库对象 
                    ///*
                    //string connectionStr = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Server.MapPath("~/App_Data/mu.mdb");
                    //OleDbConnection connection = new OleDbConnection(connectionStr);
                    //connection.Open();
   
       
                    //String sql = "insert into Qianbo_Order(OrderId,OrderName,price,Linkman,Address,ZipCode,AddTime,state,expressprice) values('" + OrderId + "','" + OrderName + "'," + amt + ",'" + name + "','" + SHIPTOSTREET + "','" + SHIPTOZIP + "','" + Addtime + "','未付款'," + expressFee + ")";
                    //OleDbCommand oleComm = new OleDbCommand(sql, connection);
                    //oleComm.ExecuteNonQuery();

                    //if (connection != null)
                    //    connection.Close();

                    // * 
                    // */

                    ////Response.Write("insert into Qianbo_Order(OrderId,OrderName,price,Linkman,Address,ZipCode,AddTime,state,expressprice) values('" + OrderId + "','" + OrderName + "'," + amt + ",'" + name + "','" + SHIPTOSTREET + "','" + SHIPTOZIP + "','" + Addtime + "','未付款'," + expressFee + ")");

                    //NVPCodec decoder = PaypalExpress.PaypalExpress.SetExpressCheckout(encoder);

                    //string ack = decoder["ACK"];
                    //string L_ERRORCODE0 = decoder["L_ERRORCODE0"];
                    //string L_SHORTMESSAGE0 = decoder["L_SHORTMESSAGE0"];
                    //string L_LONGMESSAGE0 = decoder["L_LONGMESSAGE0"];
                    //string L_SEVERITYCODE0 = decoder["L_SEVERITYCODE0"];

                    //if (!string.IsNullOrEmpty(ack) &&
                    //    (ack.Equals("Success", System.StringComparison.OrdinalIgnoreCase) || ack.Equals("SuccessWithWarning", System.StringComparison.OrdinalIgnoreCase))
                    //    )
                    //{
                    //    // 发送电子邮件
                    //    //new MailManager(email, "订单", "订单信息").Send();
                    //    Session["TOKEN"] = decoder["token"];
                    //    Response.Redirect(ConfigurationManager.AppSettings["RedirectURL"] + decoder["token"]);

                    //}
                    //else
                    //{
                    //    Response.Redirect("buy.aspx");
                    //}



                    

                }



            }

            sb.Remove(0, sb.Length);
            sb.Append("{\"Results\":\"" + dayimg + "\"}");
            context.Response.Write(sb.ToString());
        }


        private static void add_address(string email, string shippingfirstname,string shippinglastname,string shippingcompany,string shippingZipCode,string shippingAddress,string shippingCity,string shippingState,string shippingpostnumber,string shippingCountry,string shippingphoneNumber)
        {
            SqlConnection sqlConn = DAL.localwcon();
            SqlCommand cmdDB = new SqlCommand(@"insert ACT_Addresses([Email]
      ,[FirstName]
      ,[LastName]
      ,[Company]
      ,[PostNumber]
      ,[StreetAddress]
      ,[City]
      ,[State]
      ,[Zip]
      ,[Country]
      ,[Telephone]
      ,[Timestamp]
      ,[isDefault]
      ,[Apname]) values(@Email
      ,@FirstName
      ,@LastName
      ,@Company
      ,@PostNumber
      ,@StreetAddress
      ,@City
      ,@State
      ,@Zip
      ,@Country
      ,@Telephone
      ,@Timestamp
      ,@isDefault
      ,@Apname)", sqlConn);
            cmdDB.Parameters.Add("@Email", SqlDbType.VarChar).Value = email;
            cmdDB.Parameters.Add("@FirstName", SqlDbType.VarChar).Value = shippingfirstname;
            cmdDB.Parameters.Add("@LastName", SqlDbType.VarChar).Value = shippinglastname;
            cmdDB.Parameters.Add("@Company", SqlDbType.VarChar).Value = shippingcompany;
            cmdDB.Parameters.Add("@PostNumber", SqlDbType.VarChar).Value = shippingpostnumber;
            cmdDB.Parameters.Add("@StreetAddress", SqlDbType.VarChar).Value = shippingAddress;
            cmdDB.Parameters.Add("@City", SqlDbType.VarChar).Value = shippingCity;
            cmdDB.Parameters.Add("@State", SqlDbType.VarChar).Value = shippingState;
            cmdDB.Parameters.Add("@Zip", SqlDbType.VarChar).Value = shippingZipCode;
            cmdDB.Parameters.Add("@Country", SqlDbType.VarChar).Value =shippingCountry;
            cmdDB.Parameters.Add("@Telephone", SqlDbType.VarChar).Value = shippingphoneNumber;
            cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
            cmdDB.Parameters.Add("@isDefault", SqlDbType.Bit).Value =false;
            cmdDB.Parameters.Add("@Apname", SqlDbType.NVarChar).Value = "";


            cmdDB.Connection.Open();
            cmdDB.ExecuteNonQuery();
            cmdDB.Connection.Close();
        }

        private static string getWishlistinfo(string skulist)
        {
            string returnvalue = string.Empty;

            DataSet ds = new DataSet();
            ds = BLL.ReturnDataSet("select top 3 SKU,ShortName,SmallPictureURL,SamplePrice,ProductURL from View_ProductList where SKU in (" + skulist + "0)", false);
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    returnvalue += "<dl><dt><a href=\"" + ds.Tables[0].Rows[i]["ProductURL"].ToString() + "\" title=\"" + ds.Tables[0].Rows[i]["ShortName"].ToString() + "\"><img src=\"" + Config.getConfig.imgPath() + ds.Tables[0].Rows[i]["SmallPictureURL"].ToString() + "\" alt=\"" + ds.Tables[0].Rows[i]["ShortName"].ToString() + "\" /></a></dt><dd>" + Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]).ToString("C") + "</dd></dl>";

                }

            }

            return returnvalue;
        }

        private static string getOrderliststr(string ordernumber)
        {
            string returnvalue = string.Empty;
            DataSet ds = new DataSet();
            ds = BLL.ReturnDataSet("select * from OrderLines where OrderNumber='" + ordernumber + "' order by ID asc", false);

            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    returnvalue += "<tr><td class=\"images\"><img src=\"" + (ds.Tables[0].Rows[i]["SmallPicture"].ToString().Length > 0 ? Config.getConfig.imgPath() + ds.Tables[0].Rows[i]["SmallPicture"].ToString() : "/images/" + ds.Tables[0].Rows[i]["sku"].ToString() + ".jpg") + "\" onerror=\"javascript:this.src='/images/404.jpg';\" /></td><td class=\"fullname\"><a href=\"javascript:viewskuinfo(" + ds.Tables[0].Rows[i]["sku"].ToString() + ")\">" + (ds.Tables[0].Rows[i]["FullName"].ToString().Length > 0 ? ds.Tables[0].Rows[i]["FullName"].ToString() : ds.Tables[0].Rows[i]["linenotes"].ToString()) + "</a></td><td class=\"w80\">" + (curformat + " " + (Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitPrice"]) * Convert.ToDecimal(currencyrate)).ToString("F")) + "</td><td class=\"w120\">" + ds.Tables[0].Rows[i]["sku"] + " X " + ds.Tables[0].Rows[i]["Quantity"] + "</td></tr>";
                }
            }

            return returnvalue;
        }

        private static DataSet getGlskuinfo(string skulist)
        {
            DataSet ds = new DataSet();
            ds = BLL.ReturnDataSet(@"select P.SKU,
                    P.Shortname,
                    P.SamplePrice/R.price SamplePrice,
                    P.PriceMid/R.price PriceMid,
                    P.PriceMin/R.price PriceMin,
                    P.PromotionType,
                    P.PromotionPrice,
                    P.PrimaryBigCategoryID BigCategory,
                    P.PrimaryMidCategoryID MidCategory,
                    P.PrimaryCategoryID MinCategory,
                    P.instock,
                    P.stock,
                    P.SmallPictureURL,
                    P.Isairmail,
                    P.LeadTime,
                    P.GLsku,
                    P.GLprice,
                    P.StartTime,
                    P.EndTime,
                    P.LostPoPrice LostCartTotal,
                    P.Battery,
                    P.IsSoldoutPro,
                    p.Supportcoupon,
                    P.ShopName,
                    P.ProductURL,
                    P.UnitWeight,                    
                    P.CartonWeight,
                    P.Liquid,
                    P.warehouse
					FROM [Products] P,Web_Main_Rate R WHERE P.[SKU] in (" + skulist + ") AND R.Verified=1", false);
            return ds;
        }

        /// <summary>
        /// 购买成功推荐商品
        /// </summary>
        /// <param name="sku"></param>
        /// <returns></returns>
        private static string getTuiitemlist(string sku)
        {
            string returnvalue = string.Empty;
            int outtotal = 0;
            DataSet ds = new DataSet();
            ds = BLL.ReturnDataSet("select top 5 SKU,SmallPictureURL,ShortName,SamplePrice,PromotionPrice,PromotionType,ProductURL from View_ProductList where SKU in (select top 1000 sku from Web_BuySkuList where ip in (select top 1000 ip from Web_ViewSkuList where sku=" + sku + ")) order by NEWID();", false);
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    returnvalue += "<dl><dt class=\"img\"><a href=\"" + ds.Tables[0].Rows[i]["ProductURL"].ToString() + "\"><img src=\"" + (getConfig.imgPath() + ds.Tables[0].Rows[i]["SmallPictureURL"].ToString()) + "\" width=\"100\" height=\"100\" /></a></dt><dd class=\"fullname\"><a href=\"" + ds.Tables[0].Rows[i]["ProductURL"].ToString() + "\">" + ds.Tables[0].Rows[i]["ShortName"].ToString() + "</a></dd><dd class=\"Oinfo\">" + (Convert.ToInt32(ds.Tables[0].Rows[i]["PromotionType"]) > 0 ? (curformat + " " + (Convert.ToDecimal(ds.Tables[0].Rows[i]["PromotionPrice"]) * Convert.ToDecimal(currencyrate)).ToString("F")) : (curformat + " " + (Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]) * Convert.ToDecimal(currencyrate)).ToString("F"))) + "</dd></dl>";
                    outtotal++;
                }
            }

            if (outtotal < 5)
            {
                ds = BLL.ReturnDataSet("select top " + (5 - outtotal) + " SKU,SmallPictureURL,ShortName,SamplePrice,PromotionPrice,PromotionType,ProductURL from View_ProductList where SKU in (select top 1000 sku from Web_BuySkuList where ip in (select top 1000 ip from Web_ViewSkuList where sku=" + sku + ")) order by NEWID();", false);
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    returnvalue += "<dl><dt class=\"img\"><a href=\"" + ds.Tables[0].Rows[i]["ProductURL"].ToString() + "\"><img src=\"" + (getConfig.imgPath() + ds.Tables[0].Rows[i]["SmallPictureURL"].ToString()) + "\" width=\"100\" height=\"100\" /></a></dt><dd class=\"fullname\"><a href=\"" + ds.Tables[0].Rows[i]["ProductURL"].ToString() + "\">" + ds.Tables[0].Rows[i]["ShortName"].ToString() + "</a></dd><dd class=\"Oinfo\">" + (Convert.ToInt32(ds.Tables[0].Rows[i]["PromotionType"]) > 0 ? (curformat + " " + (Convert.ToDecimal(ds.Tables[0].Rows[i]["PromotionPrice"]) * Convert.ToDecimal(currencyrate)).ToString("F")) : (curformat + " " + (Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]) * Convert.ToDecimal(currencyrate)).ToString("F"))) + "</dd></dl>";
                    outtotal++;
                }
            }

            if (outtotal < 5)
            {
                ds = BLL.ReturnDataSet("select top " + (5 - outtotal) + " SKU,SmallPictureURL,ShortName,SamplePrice,PromotionPrice,PromotionType,ProductURL from View_ProductList where SKU in (select SKU from Products where PrimaryCategoryID = (select PrimaryCategoryID from Products where SKU=" + sku + ")) order by NEWID();", false);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        returnvalue += "<dl><dt class=\"img\"><a href=\"" + ds.Tables[0].Rows[i]["ProductURL"].ToString() + "\"><img src=\"" + (getConfig.imgPath() + ds.Tables[0].Rows[i]["SmallPictureURL"].ToString()) + "\" width=\"100\" height=\"100\" /></a></dt><dd class=\"fullname\"><a href=\"" + ds.Tables[0].Rows[i]["ProductURL"].ToString() + "\">" + ds.Tables[0].Rows[i]["ShortName"].ToString() + "</a></dd><dd class=\"Oinfo\">" + (Convert.ToInt32(ds.Tables[0].Rows[i]["PromotionType"]) > 0 ? (curformat + " " + (Convert.ToDecimal(ds.Tables[0].Rows[i]["PromotionPrice"]) * Convert.ToDecimal(currencyrate)).ToString("F")) : (curformat + " " + (Convert.ToDecimal(ds.Tables[0].Rows[i]["SamplePrice"]) * Convert.ToDecimal(currencyrate)).ToString("F"))) + "</dd></dl>";

                    }
                }
            }
            return returnvalue;
        }


        private static string getShoppingcartlist(string sku)
        {
            string returnvalue = string.Empty;

            ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

            if (cart.Items.Count > 0)
            {
                for (int i = 0; i < cart.Items.Count; i++)
                {
                    returnvalue += "<div class=\"p_cartlist\">";
                    if (sku == cart.Items[i].SKU.ToString())
                        returnvalue += "<div class=\"selectpan\"><input id=\"Checkbox1\" type=\"checkbox\" checked /></div>";
                    else
                        returnvalue += "<div class=\"selectpan\"><input id=\"Checkbox1\" type=\"checkbox\" /></div>";

                    returnvalue += "<div class=\"imagepan\"><img width=\"50\" height=\"50\" src=\"" + Config.getConfig.imgPath() + cart.Items[i].SmallPictureURL + "\" align=\"left\" /> </div>";
                    returnvalue += "<div class=\"infopan\"><h5>" + cart.Items[i].FullName + "</h5>";
                    returnvalue += "<p>Quantity:" + cart.Items[i].Quantity + " Price:" + Convert.ToDecimal(cart.Items[i].Price).ToString("C") + " SKU:" + cart.Items[i].SKU + "</p>";
                    returnvalue += "</div>";

                }
            }



            return returnvalue;
        }




        public static void insertbuyskulist(string os, string ip, string sku, string email)
        {
            // sku21241|2012-02-05 00:00:00-

            string lookskulist = string.Empty;
            if (Tools.Cookie.CookieCheck("buyskulist"))
                lookskulist = Tools.Cookie.GetEncryptedCookieValue("buyskulist");

            if (lookskulist.IndexOf("sku" + sku + "|") < 0)
            {//不存在时就添加
                if (lookskulist.Length > 6)
                    Tools.Cookie.SetEncryptedCookie("buyskulist", lookskulist + "&sku" + sku + "|" + DateTime.Now.ToString());
                else
                    Tools.Cookie.SetEncryptedCookie("buyskulist", "&sku" + sku + "|" + DateTime.Now.ToString());

                bool insertstatus = false;
                insertstatus = BLL.ExecuteSqlFun("insert Web_buyskulist(SKU,IP,Email,OS) values(" + sku + ",'" + ip + "','" + email + "','" + os + "')", false);

            }
            else
            {
                //存在时,判断时间
                bool insertstatus = true;

                if (lookskulist.IndexOf("-") > 0 || lookskulist.IndexOf("/") > 0)
                {
                    string[] timelist = lookskulist.Split('&');

                    for (int i = 0; i < timelist.Length; i++)
                    {
                        if (timelist[i].IndexOf("sku" + sku + "|") > -1)
                        {
                            DateTime NowTime = DateTime.Now;

                            if (DateTime.Compare(NowTime, Convert.ToDateTime(timelist[i].Trim().Split('|')[1]).AddHours(6)) < 0)
                                insertstatus = false;

                        }
                    }
                }

                if (insertstatus)
                {
                    insertstatus = BLL.ExecuteSqlFun("insert Web_buyskulist(SKU,IP,Email,OS) values(" + sku + ",'" + ip + "','" + email + "','" + os + "')", false);
                }
            }


        }

        private static string getemail(string ordernumber)
        {
            string returnValue = "|";

            SqlConnection sqlConn = DAL.localrcon();
            sqlConn.Open();

            SqlCommand cmdDB = new SqlCommand(@"SELECT [email],[points] from Web_Point_Log where ordernumber=@ordernumber and status=1", sqlConn);
            cmdDB.Parameters.Add("@ordernumber", SqlDbType.VarChar).Value = ordernumber;

            SqlDataReader reader = cmdDB.ExecuteReader();

            if (reader.Read())
            {
                if (reader.IsDBNull(0) == false)
                    returnValue = reader.GetString(0) + "|" + reader.GetInt32(1).ToString();
            }
            sqlConn.Close();

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

        private static void deleteCartlogSKU(string sku)
        {
            string useremail = string.Empty;

            if (Tools.Cookie.CookieCheck("useremail"))
                useremail = Tools.Cookie.GetEncryptedCookieValue("useremail");


            bool updatstatus = false;
            updatstatus = BLL.ExecuteSqlFun("update ACT_Accounts set CartSKUlist=REPLACE(CartSKUlist,'," + sku + "',',') where Email='" + useremail + "'", false);
        }

        private static decimal getOrderPayment(string ordernumber)
        {
            decimal returnValue = 0M;

            SqlConnection sqlConn = DAL.localrcon();
            sqlConn.Open();

            SqlCommand cmdDB = new SqlCommand(@"SELECT [totalpayment] from View_OrderWithStatus where ordernumber=@ordernumber", sqlConn);
            cmdDB.Parameters.Add("@ordernumber", SqlDbType.VarChar).Value = ordernumber;

            SqlDataReader reader = cmdDB.ExecuteReader();
            if (reader.Read())
            {
                if (reader.IsDBNull(0) == false)
                    returnValue = reader.GetDecimal(0);
            }
            sqlConn.Close();
            return returnValue;
        }

        private static string getContrylistByname()
        {
            ShoppingCart.ShoppingCart cart = ShoppingCart.ShoppingCart.GetShoppingCartFromSession(HttpContext.Current.Session);

            DataSet ds = new DataSet();
            ds = BLL.ReturnDataSet("Select * from Web_ShippingCountry order by CountryName asc", false);
            string selectcountrys = string.Empty;

            if (ds.Tables[0].Rows.Count > 0)
            {
                selectcountrys = string.Empty;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    selectcountrys += "<option value=\"" + ds.Tables[0].Rows[i]["CountryName"].ToString() + "\" " + (cart.shipping.CountryID == ds.Tables[0].Rows[i]["id"].ToString() ? "selected" : "") + ">" + "    " + ds.Tables[0].Rows[i]["ShortName"].ToString() + "-" + ds.Tables[0].Rows[i]["CountryName"].ToString() + "  " + "</option>";
                }
            }
            return selectcountrys;
        }


        private static string CreateOrdersbydraw(string useremail, string firstname, string lastname, string address, string city, string state, string selectcountry, string zip, string phone, string Company, string sku)
        {

            string emailAddress = string.Empty;

            if (useremail != null && useremail.Length > 2)
            {
                emailAddress = useremail;
            }
            else
            {
                if (Tools.Cookie.GetCookie("useremail") != null)
                    emailAddress = Tools.Cookie.GetEncryptedCookieValue("useremail");
            }
            int nowpoint = 0, skupoint = 0, skustock = 0;
            decimal unitprice = 0;
            SqlDataReader reader = BLL.ReturnValue("select SamplePrice unitprice from View_ProductList where sku=" + sku + ";", false);
            if (reader.Read())
            {
                unitprice = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
            }
            reader.Close();


            SqlConnection sqlConn = DAL.localwcon();



            SqlCommand cmdDB = new SqlCommand(@"CreateOrder", sqlConn);
            cmdDB.CommandType = CommandType.StoredProcedure;
            cmdDB.Parameters.Add("@EMail", SqlDbType.VarChar).Value = emailAddress;
            cmdDB.Parameters.Add("@BillingFirstName", SqlDbType.NVarChar).Value = firstname;
            cmdDB.Parameters.Add("@ShippingFirstName", SqlDbType.NVarChar).Value = firstname;
            cmdDB.Parameters.Add("@BillingLastName", SqlDbType.NVarChar).Value = lastname;
            cmdDB.Parameters.Add("@ShippingLastName", SqlDbType.NVarChar).Value = lastname;
            cmdDB.Parameters.Add("@BillingCompany", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingCompany", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@BillingAddress", SqlDbType.NVarChar).Value = address;
            cmdDB.Parameters.Add("@BillingCity", SqlDbType.NVarChar).Value = city;
            cmdDB.Parameters.Add("@BillingState", SqlDbType.NVarChar).Value = state;
            cmdDB.Parameters.Add("@BillingZip", SqlDbType.NVarChar).Value = zip;
            cmdDB.Parameters.Add("@BillingCountry", SqlDbType.NVarChar).Value = selectcountry;
            cmdDB.Parameters.Add("@BillingPhone", SqlDbType.NVarChar).Value = phone;
            cmdDB.Parameters.Add("@ShippingAddress", SqlDbType.NVarChar).Value = address;
            cmdDB.Parameters.Add("@ShippingCity", SqlDbType.NVarChar).Value = city;
            cmdDB.Parameters.Add("@ShippingState", SqlDbType.NVarChar).Value = state;
            cmdDB.Parameters.Add("@ShippingZip", SqlDbType.NVarChar).Value = zip;
            cmdDB.Parameters.Add("@ShippingCountry", SqlDbType.NVarChar).Value = selectcountry;
            cmdDB.Parameters.Add("@ShippingPhone", SqlDbType.NVarChar).Value = phone;
            cmdDB.Parameters.Add("@ShippingMethod", SqlDbType.VarChar).Value = "Registered Air Mail, Package Tracking Service.";
            cmdDB.Parameters.Add("@ShippingTrackingNumber", SqlDbType.VarChar).Value = "";
            cmdDB.Parameters.Add("@SalesPerson", SqlDbType.VarChar).Value = "";
            cmdDB.Parameters.Add("@CartLog", SqlDbType.VarChar).Value = "";

            cmdDB.Parameters.Add("@MissColorNote", SqlDbType.VarChar).Value = "2";
            cmdDB.Parameters.Add("@MissItemNote", SqlDbType.VarChar).Value = "2";
            cmdDB.Parameters.Add("@ShortageSize", SqlDbType.VarChar).Value = "2";
            cmdDB.Parameters.Add("@HaiguanFee", SqlDbType.VarChar).Value = "15";
            cmdDB.Parameters.Add("@OrderMark", SqlDbType.VarChar).Value = "";

            cmdDB.Parameters.Add("@OrderStatus", SqlDbType.Int).Value = -1;
            cmdDB.Parameters.Add("@TotalPayment", SqlDbType.Money).Value = 0;
            cmdDB.Parameters.Add("@TotalRefunds", SqlDbType.Money).Value = 0;
            cmdDB.Parameters.Add("@IsFullyPaid", SqlDbType.Bit).Value = 0;
            cmdDB.Parameters.Add("@IsFullyRefunded", SqlDbType.Bit).Value = 0;

            SqlDataAdapter adapt = new SqlDataAdapter(cmdDB);
            DataSet ds = new DataSet("Results");

            string OrderNumber = "";

            cmdDB.Connection.Open();
            adapt.Fill(ds, "Results");

            // check order create results

            OrderNumber = ds.Tables[0].Rows[0]["OrderNumber"].ToString();

            // create each product              

            bool Freetracknumber = false;

            cmdDB = new SqlCommand(@"CreateOrderLine", sqlConn);
            cmdDB.CommandType = CommandType.StoredProcedure;
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber;
            cmdDB.Parameters.Add("@Quantity", SqlDbType.Int).Value = 1;
            cmdDB.Parameters.Add("@IsPaid", SqlDbType.Bit).Value = false;
            cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "Draw gift order from points";
            cmdDB.Parameters.Add("@ProcessingInstructions", SqlDbType.VarChar).Value = "";
            cmdDB.Parameters.Add("@SKU", SqlDbType.Int).Value = Int32.Parse(sku);
            cmdDB.Parameters.Add("@LineNotes", SqlDbType.VarChar).Value = "Gift from points with sku:" + sku;
            cmdDB.Parameters.Add("@UnitPrice", SqlDbType.Money).Value = unitprice;
            cmdDB.Parameters.Add("@StockArea", SqlDbType.VarChar).Value = "0";
            cmdDB.ExecuteNonQuery();

            cmdDB = new SqlCommand(@"CreateOrderLine", sqlConn);
            cmdDB.CommandType = CommandType.StoredProcedure;
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber;
            cmdDB.Parameters.Add("@Quantity", SqlDbType.Int).Value = 1;
            cmdDB.Parameters.Add("@IsPaid", SqlDbType.Bit).Value = false;
            cmdDB.Parameters.Add("@UnitPrice", SqlDbType.Money).Value = 0;
            cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "Air Mail";
            cmdDB.Parameters.Add("@SKU", SqlDbType.Int).Value = 0;
            cmdDB.Parameters.Add("@LineNotes", SqlDbType.VarChar).Value = "Air Mail";
            cmdDB.Parameters.Add("@ProcessingInstructions", SqlDbType.VarChar).Value = "SAMPLE";
            cmdDB.Parameters.Add("@StockArea", SqlDbType.VarChar).Value = "0";
            cmdDB.ExecuteNonQuery();

            cmdDB = new SqlCommand(@"INSERT [Transactions] (
Timestamp,
PaymentDate,
Cleared,
Voided,
GatewayResponse,
OrderNumber,
Staff,
TransactionNumber,
ParentTransactionNumber,
Amount,
Notes,
TransactionLog
) VALUES (
@Timestamp,
@PaymentDate,
@Cleared,
@Voided,
@GatewayResponse,
@OrderNumber,
@Staff,
@TransactionNumber,
@ParentTransactionNumber,
@Amount,
@Notes,
@TransactionLog
" +
                    ")", sqlConn);

            cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
            cmdDB.Parameters.Add("@PaymentDate", SqlDbType.DateTime).Value = DateTime.Now;
            cmdDB.Parameters.Add("@Cleared", SqlDbType.Bit).Value = true;
            cmdDB.Parameters.Add("@Voided", SqlDbType.Bit).Value = false;
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber; // set to null for now
            cmdDB.Parameters.Add("@GatewayResponse", SqlDbType.VarChar).Value = "Draw from point";
            cmdDB.Parameters.Add("@Staff", SqlDbType.VarChar).Value = "Draw";
            cmdDB.Parameters.Add("@TransactionNumber", SqlDbType.VarChar).Value = "Drawpayfor" + OrderNumber;
            cmdDB.Parameters.Add("@ParentTransactionNumber", SqlDbType.VarChar).Value = "";
            cmdDB.Parameters.Add("@Amount", SqlDbType.Money).Value = unitprice;
            cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "draw";
            cmdDB.Parameters.Add("@TransactionLog", SqlDbType.Text).Value = "Pay from draw from point center";
            cmdDB.ExecuteNonQuery();

            cmdDB = new SqlCommand(@"INSERT OrderStatus (OrderNumber, Timestamp, Username, StatusCode, Notes) VALUES (@OrderNumber, @Timestamp, @Username, @StatusCode, @Notes)", sqlConn);
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber;
            cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
            cmdDB.Parameters.Add("@Username", SqlDbType.VarChar).Value = "Luck Draw";
            cmdDB.Parameters.Add("@StatusCode", SqlDbType.Int).Value = 1;
            cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "Luck Draw";
            cmdDB.ExecuteNonQuery();

            cmdDB = new SqlCommand(@"update Orders set TotalPayment=@TotalPayment,IsFullyPaid=1 where ordernumber=@OrderNumber", sqlConn);
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber;
            cmdDB.Parameters.Add("@TotalPayment", SqlDbType.Money).Value = unitprice;
            cmdDB.ExecuteNonQuery();

            cmdDB = new SqlCommand(@"update Point_exchangeproducts set ExchangeNumber=ExchangeNumber+1 where sku=@sku", sqlConn);
            cmdDB.Parameters.Add("@sku", SqlDbType.Int).Value = Int32.Parse(sku);
            cmdDB.ExecuteNonQuery();

            cmdDB.Connection.Close();
            //清空购物车

            return OrderNumber;
        }

        //gift order
        private static string CreateGiftOrders(string useremail, string firstname, string lastname, string address, string city, string state, string selectcountry, string zip, string phone, string Company, string sku)
        {

            string emailAddress = string.Empty;

            if (useremail != null && useremail.Length > 2)
            {
                emailAddress = useremail;
            }
            else
            {
                if (Tools.Cookie.GetCookie("useremail") != null)
                    emailAddress = Tools.Cookie.GetEncryptedCookieValue("useremail");
            }
            int nowpoint = 0, skustock = 0;
            decimal skupoint = 0;
            SqlDataReader reader = BLL.ReturnValue("select Stock,Point from Point_exchangeproducts where sku=" + sku + ";", false);
            if (reader.Read())
            {
                skupoint = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetInt32(1).ToString());
                skustock = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
            }
            reader.Close();


            SqlConnection sqlConn = DAL.localwcon();



            SqlCommand cmdDB = new SqlCommand(@"CreateOrder", sqlConn);
            cmdDB.CommandType = CommandType.StoredProcedure;
            cmdDB.Parameters.Add("@EMail", SqlDbType.VarChar).Value = emailAddress;
            cmdDB.Parameters.Add("@BillingFirstName", SqlDbType.NVarChar).Value = firstname;
            cmdDB.Parameters.Add("@ShippingFirstName", SqlDbType.NVarChar).Value = firstname;
            cmdDB.Parameters.Add("@BillingLastName", SqlDbType.NVarChar).Value = lastname;
            cmdDB.Parameters.Add("@ShippingLastName", SqlDbType.NVarChar).Value = lastname;
            cmdDB.Parameters.Add("@BillingCompany", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingCompany", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@BillingAddress", SqlDbType.NVarChar).Value = address;
            cmdDB.Parameters.Add("@BillingCity", SqlDbType.NVarChar).Value = city;
            cmdDB.Parameters.Add("@BillingState", SqlDbType.NVarChar).Value = state;
            cmdDB.Parameters.Add("@BillingZip", SqlDbType.NVarChar).Value = zip;
            cmdDB.Parameters.Add("@BillingCountry", SqlDbType.NVarChar).Value = selectcountry;
            cmdDB.Parameters.Add("@BillingPhone", SqlDbType.NVarChar).Value = phone;
            cmdDB.Parameters.Add("@ShippingAddress", SqlDbType.NVarChar).Value = address;
            cmdDB.Parameters.Add("@ShippingCity", SqlDbType.NVarChar).Value = city;
            cmdDB.Parameters.Add("@ShippingState", SqlDbType.NVarChar).Value = state;
            cmdDB.Parameters.Add("@ShippingZip", SqlDbType.NVarChar).Value = zip;
            cmdDB.Parameters.Add("@ShippingCountry", SqlDbType.NVarChar).Value = selectcountry;
            cmdDB.Parameters.Add("@ShippingPhone", SqlDbType.NVarChar).Value = phone;
            cmdDB.Parameters.Add("@ShippingMethod", SqlDbType.VarChar).Value = "Air Mail";
            cmdDB.Parameters.Add("@ShippingTrackingNumber", SqlDbType.VarChar).Value = "";
            cmdDB.Parameters.Add("@SalesPerson", SqlDbType.VarChar).Value = "";
            cmdDB.Parameters.Add("@CartLog", SqlDbType.VarChar).Value = "";

            cmdDB.Parameters.Add("@MissColorNote", SqlDbType.VarChar).Value = "2";
            cmdDB.Parameters.Add("@MissItemNote", SqlDbType.VarChar).Value = "2";
            cmdDB.Parameters.Add("@ShortageSize", SqlDbType.VarChar).Value = "2";
            cmdDB.Parameters.Add("@HaiguanFee", SqlDbType.VarChar).Value = "15";
            cmdDB.Parameters.Add("@OrderMark", SqlDbType.VarChar).Value = "";

            cmdDB.Parameters.Add("@OrderStatus", SqlDbType.Int).Value = -1;
            cmdDB.Parameters.Add("@TotalPayment", SqlDbType.Money).Value = 0;
            cmdDB.Parameters.Add("@TotalRefunds", SqlDbType.Money).Value = 0;
            cmdDB.Parameters.Add("@IsFullyPaid", SqlDbType.Bit).Value = 0;
            cmdDB.Parameters.Add("@IsFullyRefunded", SqlDbType.Bit).Value = 0;

            SqlDataAdapter adapt = new SqlDataAdapter(cmdDB);
            DataSet ds = new DataSet("Results");

            string OrderNumber = "";

            cmdDB.Connection.Open();
            adapt.Fill(ds, "Results");

            // check order create results

            OrderNumber = ds.Tables[0].Rows[0]["OrderNumber"].ToString();

            // create each product              

            bool Freetracknumber = false;

            cmdDB = new SqlCommand(@"CreateOrderLine", sqlConn);
            cmdDB.CommandType = CommandType.StoredProcedure;
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber;
            cmdDB.Parameters.Add("@Quantity", SqlDbType.Int).Value = 1;
            cmdDB.Parameters.Add("@IsPaid", SqlDbType.Bit).Value = false;
            cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "Draw gift order from points";
            cmdDB.Parameters.Add("@ProcessingInstructions", SqlDbType.VarChar).Value = "";
            cmdDB.Parameters.Add("@SKU", SqlDbType.Int).Value = Int32.Parse(sku);
            cmdDB.Parameters.Add("@LineNotes", SqlDbType.VarChar).Value = "Gift from points with sku:" + sku;
            cmdDB.Parameters.Add("@UnitPrice", SqlDbType.Money).Value = skupoint / 10;
            cmdDB.Parameters.Add("@StockArea", SqlDbType.VarChar).Value = "0";
            cmdDB.ExecuteNonQuery();

            cmdDB = new SqlCommand(@"CreateOrderLine", sqlConn);
            cmdDB.CommandType = CommandType.StoredProcedure;
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber;
            cmdDB.Parameters.Add("@Quantity", SqlDbType.Int).Value = 1;
            cmdDB.Parameters.Add("@IsPaid", SqlDbType.Bit).Value = false;
            cmdDB.Parameters.Add("@UnitPrice", SqlDbType.Money).Value = 0;
            cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "Air Mail";
            cmdDB.Parameters.Add("@SKU", SqlDbType.Int).Value = 0;
            cmdDB.Parameters.Add("@LineNotes", SqlDbType.VarChar).Value = "Air Mail";
            cmdDB.Parameters.Add("@ProcessingInstructions", SqlDbType.VarChar).Value = "SAMPLE";
            cmdDB.Parameters.Add("@StockArea", SqlDbType.VarChar).Value = "0";
            cmdDB.ExecuteNonQuery();

            cmdDB = new SqlCommand(@"INSERT [Transactions] (
Timestamp,
PaymentDate,
Cleared,
Voided,
GatewayResponse,
OrderNumber,
Staff,
TransactionNumber,
ParentTransactionNumber,
Amount,
Notes,
TransactionLog
) VALUES (
@Timestamp,
@PaymentDate,
@Cleared,
@Voided,
@GatewayResponse,
@OrderNumber,
@Staff,
@TransactionNumber,
@ParentTransactionNumber,
@Amount,
@Notes,
@TransactionLog
" +
                    ")", sqlConn);

            cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
            cmdDB.Parameters.Add("@PaymentDate", SqlDbType.DateTime).Value = DateTime.Now;
            cmdDB.Parameters.Add("@Cleared", SqlDbType.Bit).Value = true;
            cmdDB.Parameters.Add("@Voided", SqlDbType.Bit).Value = false;
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber; // set to null for now
            cmdDB.Parameters.Add("@GatewayResponse", SqlDbType.VarChar).Value = "pay from point";
            cmdDB.Parameters.Add("@Staff", SqlDbType.VarChar).Value = "Point";
            cmdDB.Parameters.Add("@TransactionNumber", SqlDbType.VarChar).Value = "Giftpayfor" + OrderNumber;
            cmdDB.Parameters.Add("@ParentTransactionNumber", SqlDbType.VarChar).Value = "";
            cmdDB.Parameters.Add("@Amount", SqlDbType.Money).Value = skupoint / 10;
            cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "Point";
            cmdDB.Parameters.Add("@TransactionLog", SqlDbType.Text).Value = "Pay from point from point center";
            cmdDB.ExecuteNonQuery();

            cmdDB = new SqlCommand(@"INSERT OrderStatus (OrderNumber, Timestamp, Username, StatusCode, Notes) VALUES (@OrderNumber, @Timestamp, @Username, @StatusCode, @Notes)", sqlConn);
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber;
            cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
            cmdDB.Parameters.Add("@Username", SqlDbType.VarChar).Value = "Points";
            cmdDB.Parameters.Add("@StatusCode", SqlDbType.Int).Value = 1;
            cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "Draw with point";
            cmdDB.ExecuteNonQuery();

            cmdDB = new SqlCommand(@"update Orders set TotalPayment=@TotalPayment,IsFullyPaid=1 where ordernumber=@OrderNumber", sqlConn);
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber;
            cmdDB.Parameters.Add("@TotalPayment", SqlDbType.Money).Value = skupoint / 10;
            cmdDB.ExecuteNonQuery();

            cmdDB = new SqlCommand(@"update Point_exchangeproducts set ExchangeNumber=ExchangeNumber+1 where sku=@sku", sqlConn);
            cmdDB.Parameters.Add("@sku", SqlDbType.Int).Value = Int32.Parse(sku);
            cmdDB.ExecuteNonQuery();

            cmdDB.Connection.Close();
            //清空购物车

            return OrderNumber;
        }


        //生成预定订单
        public static string CreateYudingOrders(string useremail, string sku, int qty, decimal price)
        {

            string emailAddress = string.Empty;

            if (useremail != null && useremail.Length > 2)
            {
                emailAddress = useremail;
            }
            else
            {
                if (Tools.Cookie.GetCookie("useremail") != null)
                    emailAddress = Tools.Cookie.GetEncryptedCookieValue("useremail");
            }
            int nowpoint = 0, skupoint = 0, skustock = 0;
            SqlDataReader reader = BLL.ReturnValue("select Stock,Point from Point_exchangeproducts where sku=" + sku + ";", false);
            if (reader.Read())
            {
                skupoint = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                skustock = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
            }
            reader.Close();


            SqlConnection sqlConn = DAL.localwcon();



            SqlCommand cmdDB = new SqlCommand(@"CreateOrder", sqlConn);
            cmdDB.CommandType = CommandType.StoredProcedure;
            cmdDB.Parameters.Add("@EMail", SqlDbType.VarChar).Value = emailAddress;
            cmdDB.Parameters.Add("@BillingFirstName", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingFirstName", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@BillingLastName", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingLastName", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@BillingCompany", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingCompany", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@BillingAddress", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@BillingCity", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@BillingState", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@BillingZip", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@BillingCountry", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@BillingPhone", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingAddress", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingCity", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingState", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingZip", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingCountry", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingPhone", SqlDbType.NVarChar).Value = "";
            cmdDB.Parameters.Add("@ShippingMethod", SqlDbType.VarChar).Value = "Email send.";
            cmdDB.Parameters.Add("@ShippingTrackingNumber", SqlDbType.VarChar).Value = "";
            cmdDB.Parameters.Add("@SalesPerson", SqlDbType.VarChar).Value = "yuding";
            cmdDB.Parameters.Add("@CartLog", SqlDbType.VarChar).Value = "";

            cmdDB.Parameters.Add("@MissColorNote", SqlDbType.VarChar).Value = "2";
            cmdDB.Parameters.Add("@MissItemNote", SqlDbType.VarChar).Value = "2";
            cmdDB.Parameters.Add("@ShortageSize", SqlDbType.VarChar).Value = "2";
            cmdDB.Parameters.Add("@HaiguanFee", SqlDbType.VarChar).Value = "15";
            cmdDB.Parameters.Add("@OrderMark", SqlDbType.VarChar).Value = "";

            cmdDB.Parameters.Add("@OrderStatus", SqlDbType.Int).Value = (int)Tools.Order.Status.QuotationProvided;
            cmdDB.Parameters.Add("@TotalPayment", SqlDbType.Money).Value = 0;
            cmdDB.Parameters.Add("@TotalRefunds", SqlDbType.Money).Value = 0;
            cmdDB.Parameters.Add("@IsFullyPaid", SqlDbType.Bit).Value = 0;
            cmdDB.Parameters.Add("@IsFullyRefunded", SqlDbType.Bit).Value = 0;

            SqlDataAdapter adapt = new SqlDataAdapter(cmdDB);
            DataSet ds = new DataSet("Results");

            string OrderNumber = "";

            cmdDB.Connection.Open();
            adapt.Fill(ds, "Results");

            // check order create results

            OrderNumber = ds.Tables[0].Rows[0]["OrderNumber"].ToString();

            // create each product              

            bool Freetracknumber = false;

            cmdDB = new SqlCommand(@"CreateOrderLine", sqlConn);
            cmdDB.CommandType = CommandType.StoredProcedure;
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber;
            cmdDB.Parameters.Add("@Quantity", SqlDbType.Int).Value = qty;
            cmdDB.Parameters.Add("@IsPaid", SqlDbType.Bit).Value = false;
            cmdDB.Parameters.Add("@UnitPrice", SqlDbType.Money).Value = price;
            cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "Commodity reservation service";
            cmdDB.Parameters.Add("@SKU", SqlDbType.Int).Value = Int32.Parse(sku);
            cmdDB.Parameters.Add("@LineNotes", SqlDbType.VarChar).Value = "Commodity reservation service";
            cmdDB.Parameters.Add("@ProcessingInstructions", SqlDbType.VarChar).Value = "system";
            cmdDB.Parameters.Add("@StockArea", SqlDbType.VarChar).Value = "0";
            cmdDB.ExecuteNonQuery();

            cmdDB = new SqlCommand(@"INSERT OrderStatus (OrderNumber, Timestamp, Username, StatusCode, Notes) VALUES (@OrderNumber, @Timestamp, @Username, @StatusCode, @Notes)", sqlConn);
            cmdDB.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = OrderNumber;
            cmdDB.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;
            cmdDB.Parameters.Add("@Username", SqlDbType.VarChar).Value = "system";
            cmdDB.Parameters.Add("@StatusCode", SqlDbType.Int).Value = -1;
            cmdDB.Parameters.Add("@Notes", SqlDbType.VarChar).Value = "Order has been received. Order not paid for yet. Please contact customer.";
            cmdDB.ExecuteNonQuery();

            cmdDB.Connection.Close();
            //清空购物车

            return OrderNumber;
        }
    }
}
