using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Larpx.ResourceSpider.BaseLibrary.Helpers
{
    public class CommonHelper
    {
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
            StringBuilder newRandom = new StringBuilder(62);
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
            byte[] byStr = Encoding.UTF8.GetBytes(str);
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
        //public static T Clone<T>(T RealObject)
        //{
        //    using (Stream objectStream = new MemoryStream())
        //    {
        //        IFormatter formatter = new BinaryFormatter();
        //        formatter.Serialize(objectStream, RealObject);
        //        objectStream.Seek(0, SeekOrigin.Begin);
        //        return (T)formatter.Deserialize(objectStream);
        //    }
        //}

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
