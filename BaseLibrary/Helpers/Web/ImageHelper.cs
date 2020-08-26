using System;
using System.Drawing;
using System.Drawing.Imaging;
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

                ImageFormat sType = ImageFormat.Jpeg;
                if (base64Str.Contains("data:image/png;base64,"))
                    sType = ImageFormat.Png;
                else if (base64Str.Contains("data:image/jgp;base64,") || base64Str.Contains("data:image/jpg;base64,") || base64Str.Contains("data:image/jpeg;base64,"))
                    sType = ImageFormat.Jpeg;
                else if (base64Str.Contains("data:image/gif;base64,"))
                    sType = ImageFormat.Gif;
                else
                    sType = ImageFormat.Png;

                base64Str = base64Str.Replace("data:image/png;base64,", "").Replace("data:image/jgp;base64,", "")
                .Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");

                byte[] arr = Convert.FromBase64String(base64Str);
                using (MemoryStream ms = new MemoryStream(arr))
                {
                    using (Bitmap bmp = new Bitmap(ms))
                    {
                        bmp.Save(savePath, sType);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
