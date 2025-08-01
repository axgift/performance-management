<%@ WebHandler Language="C#" Class="post" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.Text;
using Post;

public class post : IHttpHandler, IRequiresSessionState
{
    
    public void ProcessRequest (HttpContext context) {
        //if (Config.getConfig.IsTrueUrl(context.Request.Url.Host, context.Request.UrlReferrer.AbsoluteUri, context.Request.ServerVariables.Get("Local_Addr").ToString()))
        //{
            Post.Post.Repost(context);
        //}
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}