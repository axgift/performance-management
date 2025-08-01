using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Config;

namespace PageSource
{
    public class PageSource
    {
        /// <summary>
        /// 读取模版文件HTML数据
        /// </summary>
        /// <param name="filename">文件名</param>
        public static string GetTemplateSocur(string FileName)
        {
            string tempStr = string.Empty;
            GetConfigInfo getConfiginfofun = new GetConfigInfo();
            StreamReader TemplateStream = new StreamReader(getConfiginfofun.getTemplatePath + FileName);
            string Template = TemplateStream.ReadToEnd();
            TemplateStream.Close();
            tempStr = Template;
            return tempStr;
        }

        /// <summary>
        /// 读取模版文件HTML数据
        /// </summary>
        /// <param name="filename">文件名</param>
        public static string GetTemplateSocur(string templatepath, string FileName)
        {
            string tempStr = string.Empty;
            GetConfigInfo getConfiginfofun = new GetConfigInfo();
            StreamReader TemplateStream = new StreamReader(templatepath + FileName);
            string Template = TemplateStream.ReadToEnd();
            TemplateStream.Close();
            tempStr = Template;
            return tempStr;
        }

        /// <summary>
        /// 读取模版文件HTML数据
        /// </summary>
        /// <param name="filename">文件名</param>
        public static string GetPublickSocur(string FileName)
        {
            string tempStr = string.Empty;
            GetConfigInfo getConfiginfofun = new GetConfigInfo();
            StreamReader TemplateStream;
            string Template = string.Empty;
            if (File.Exists(getConfiginfofun.getPublicdatePath + FileName) == true)
            {
                TemplateStream = new StreamReader(getConfiginfofun.getPublicdatePath + FileName);
                Template = TemplateStream.ReadToEnd();
                TemplateStream.Close();
            }


            tempStr = Template;
            return tempStr;
        }

        /// <summary>
        /// 读取模版文件HTML数据
        /// </summary>
        /// <param name="filename">文件名</param>
        public static string GetFileSocur(string filepath, string FileName)
        {
            string tempStr = string.Empty;
            GetConfigInfo getConfiginfofun = new GetConfigInfo();
            StreamReader TemplateStream = new StreamReader(filepath + FileName);
            string Template = TemplateStream.ReadToEnd();
            TemplateStream.Close();
            tempStr = Template;
            return tempStr;
        }
    }
}
