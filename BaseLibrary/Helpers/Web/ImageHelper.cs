using System;
using System.IO;

namespace Larpx.ResourceSpider.BaseLibrary.Helpers.Web
{
    /// <summary>
    /// 图片操作类
    /// </summary>
    public class ImageHelper
    {
        /// <summary>
        /// Base64字符串转图片
        /// </summary>
        /// <param name="base64Str">源字符串</param>
        /// <param name="savePath">保存路径</param>
        public static void Base642Img(string base64Str, string savePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(base64Str))
                    throw new ArgumentNullException();

                if (File.Exists(savePath))
                    File.Delete(savePath);

                //string sType = "png";
                //if (base64Str.Contains("data:image/png;base64,"))
                //    sType =  "png";
                //else if (base64Str.Contains("data:image/jgp;base64,") || base64Str.Contains("data:image/jpg;base64,") || base64Str.Contains("data:image/jpeg;base64,"))
                //    sType =  "jpg";
                //else if (base64Str.Contains("data:image/gif;base64,"))
                //    sType = "gif";
                //else
                //    sType = "png";

                //base64Str = base64Str.Replace("data:image/png;base64,", "").Replace("data:image/jgp;base64,", "")
                //.Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");

                byte[] arr = Convert.FromBase64String(base64Str);

                using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
                {
                    fileStream.Write(arr, 0, arr.Length);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
