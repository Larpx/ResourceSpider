using Larpx.ResourceSpider.CommonHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace Larpx.ResourceSpider.CommonHelper
{
    public class CommonHelper
    {
        /// <summary>
        /// meta类型
        /// </summary>
        public enum MetaType
        {
            Charset,
            Viewport,
            Keywords,
            Description,
            Renderer,
            X_UA_Compatible,
            Cache_Control,
            Other
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public enum DatabaseType
        {
            MySql = 0,
            SqlServer = 1,
            Sqlite = 2,
            Oracle = 3,
            PostgreSQL = 4
        }

        /// <summary>
        /// 生成随机序列
        /// </summary>
        /// <param name="Length"></param>
        /// <returns></returns>
        public static string GenerateRandomNumber(int Length = 16)
        {
            char[] constant =
            {
                '0','1','2','3','4','5','6','7','8','9',
                'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
            };
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }

        /// <summary>
        /// 根据当前系统时间加随机序列来生成订单号
        /// 17位长度
        /// </summary>
        /// <returns>订单号</returns>
        public static string GenerateOutTradeNo()
        {
            Random oRan = new Random();
            return string.Format("{0}{1}", DateTime.Now.ToString("yyyyMMddHHmmss"), oRan.Next(100, 999));
        }

        /// <summary>
        /// 生成随机串，随机串包含字母或数字
        /// </summary>
        /// <returns>随机串</returns>
        public static string GenerateNonceStr()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        /// <summary>
        /// 生成时间戳，标准北京时间，时区为东八区，自1970年1月1日 0点0分0秒以来的秒数
        /// </summary>
        /// <returns>时间戳</returns>
        public static string GenerateTimeStamp()
        {
            TimeSpan oTimeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(oTimeSpan.TotalSeconds).ToString();
        }

        /// <summary>
        /// 将字符串进行url编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlEncode(string str)
        {
            StringBuilder oSb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str);
            for (int i = 0; i < byStr.Length; i++)
            {
                oSb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return oSb.ToString();
        }

        /// <summary>
        /// 利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="RealObject"></param>
        /// <returns></returns>
        public static T Clone<T>(T RealObject)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, RealObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(objectStream);
            }
        }

        /// <summary>
        /// 将List随机排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputList"></param>
        /// <returns></returns>
        public static List<T> GetRandomList<T>(List<T> inputList)
        {
            //Copy to a array
            T[] copyArray = new T[inputList.Count];
            inputList.CopyTo(copyArray);

            //Add range
            List<T> copyList = new List<T>();
            copyList.AddRange(copyArray);

            //Set outputList and random
            List<T> outputList = new List<T>();
            Random rd = new Random(DateTime.Now.Millisecond);

            while (copyList.Count > 0)
            {
                //Select an index and item
                int rdIndex = rd.Next(0, copyList.Count - 1);
                T remove = copyList[rdIndex];

                //remove it from copyList and add it to output
                copyList.Remove(remove);
                outputList.Add(remove);
            }
            return outputList;
        }

        /// <summary>
        /// 云打码
        /// </summary>
        /// <param name="sURL">验证码图片</param>
        /// <param name="sResult">结果</param>
        /// <returns>大于零表示打码成功</returns>
        public static int EasyDecodeByBytes(string sURL, out string sResult)
        {
            sResult = "";
            string username, password, lpAppKey;
            int nCodeType, nCaptchaId, nAppId, nTimeOut;
            StringBuilder pCodeResult = new StringBuilder(new string(' ', 30)); // 分配30个字节存放识别结果

            // 一键版本无需调用 YDM_SetAppInfo 和 YDM_Login，但需传入软件ID密钥等4个参数
            //username = "DLarpx";
            //password = "50zx31cvb";
            //nAppId = 6472;
            //lpAppKey = "3f0e944ec851d6984bf572d821edf0b2";

            username = ConfigurationManager.AppSettings["UserName"];
            password = ConfigurationManager.AppSettings["Pdw"];
            nAppId = Convert.ToInt32(ConfigurationManager.AppSettings["AppID"]);
            lpAppKey = ConfigurationManager.AppSettings["AppKey"];

            // 例：1004表示4位字母数字，不同类型收费不同。请准确填写，否则影响识别率。在此查询所有类型 http://www.yundama.com/price.html
            nCodeType = 3006;

            // 超时时间，单位：秒
            nTimeOut = 60;

            int nBalance = YDMWrapper.YDM_EasyGetBalance(username, password, nAppId, lpAppKey);

            if (nBalance > 0)
            {
                using (var oResponsePic = EasyHttpHelper.ReadData(sURL, 0))
                {
                    using (Stream oPic = oResponsePic.GetResponseStream())
                    {

                        byte[] data = new byte[1024];
                        int length = 0;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            while ((length = oPic.Read(data, 0, data.Length)) > 0)
                            {
                                ms.Write(data, 0, length);
                            }
                            ms.Seek(0, SeekOrigin.Begin);
                            byte[] buffer = ms.ToArray();

                            // 返回验证码ID，大于零为识别成功，返回其他错误代码请查询 http://www.yundama.com/apidoc/YDM_ErrorCode.html
                            nCaptchaId = YDMWrapper.YDM_EasyDecodeByBytes(username, password, nAppId, lpAppKey, buffer, (int)buffer.Length, nCodeType, nTimeOut, pCodeResult);
                        }
                    }
                }

                sResult = pCodeResult.ToString();
                return nCaptchaId;
            }
            else
            {
                Console.WriteLine("账户已欠费");
                return -1;
            }
        }

        /// <summary>
        /// 正则规则匹配与转换
        /// </summary>
        /// <param name="sSource"></param>
        /// <param name="bOut"></param>
        /// <returns></returns>
        public static bool RegexMathAndConvert(string sSource, out bool bOut)
        {
            if (string.IsNullOrEmpty(sSource))
            {
                bOut = false;
                return false;
            }

            try
            {
                bOut = Convert.ToBoolean(sSource);
                return true;
            }
            catch (Exception)
            {
                bOut = false;
                return false;
            }
        }

        /// <summary>
        /// 检测字符串是否能通过正则校验
        /// </summary>
        /// <param name="sSource">源数据</param>
        /// <param name="sRegex">正则表达式</param>
        /// <returns>是否通过</returns>
        public static bool RegexMathAndConvert(string sSource, string sRegex)
        {
            if (string.IsNullOrEmpty(sSource) || string.IsNullOrEmpty(sRegex))
            {
                return false;
            }

            try
            {
                Regex oRegex = new Regex(sRegex);
                return oRegex.IsMatch(sSource);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 判断源数据是否为正整数，如果是则转换
        /// </summary>
        /// <param name="sSource">源数据</param>
        /// <param name="sOut">转换结果</param>
        /// <returns>是否通过校验</returns>
        public static bool RegexMathAndConvert(string sSource, out int sOut)
        {
            if (string.IsNullOrEmpty(sSource))
            {
                sOut = 0;
                return false;
            }

            try
            {
                Match m = Regex.Match(sSource, @"[1-9]\d*");
                if (m.Success)
                {
                    sOut = Convert.ToInt32(m.Value);
                    return true;
                }
                else
                {
                    sOut = 0;
                    return false;
                }
            }
            catch (Exception)
            {
                sOut = 0;
                return false;
            }
        }

        /// <summary>
        /// 判断源数据是否为小数，如果是则转换
        /// </summary>
        /// <param name="sSource">源数据</param>
        /// <param name="sOut">转换结果</param>
        /// <returns>是否通过校验</returns>
        public static bool RegexMathAndConvert(string sSource, out double sOut)
        {
            if (string.IsNullOrEmpty(sSource))
            {
                sOut = 0;
                return false;
            }

            try
            {
                Regex oRegex = new Regex(@"");
                if (oRegex.IsMatch(sSource))
                {
                    sOut = Convert.ToInt32(sSource);
                    return true;
                }
                else
                {
                    sOut = 0;
                    return false;
                }
            }
            catch (Exception)
            {
                sOut = 0;
                return false;
            }
        }

        /// <summary>
        /// 判断源数据是否为小数，如果是则转换
        /// </summary>
        /// <param name="sSource">源数据</param>
        /// <param name="sOut">转换结果</param>
        /// <returns>是否通过校验</returns>
        public static bool RegexMathAndConvert(string sSource, string sRegex, out double sOut)
        {
            if (string.IsNullOrEmpty(sSource) || string.IsNullOrEmpty(sRegex))
            {
                sOut = 0;
                return false;
            }

            try
            {
                Regex oRegex = new Regex(sRegex);
                if (oRegex.IsMatch(sSource))
                {
                    sOut = Convert.ToDouble(sSource);
                    return true;
                }
                else
                {
                    sOut = 0;
                    return false;
                }
            }
            catch (Exception)
            {
                sOut = 0;
                return false;
            }
        }
    }
}
