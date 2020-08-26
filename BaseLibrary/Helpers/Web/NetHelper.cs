using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace Larpx.ResourceSpider.BaseLibrary.Helpers.Web
{
    /// <summary>
    /// 网络工具类
    /// </summary>
    public class NetHelper
    {
        /// <summary>
        /// Ping一个地址，测试是否可通
        /// </summary>
        /// <param name="strIP"></param>
        /// <returns></returns>
        public static bool PingIp(string strIP)
        {
            bool bRet = false;
            try
            {
                Ping pingSend = new Ping();
                strIP = strIP.Replace("https://", "").Replace("http://", "").Replace("/", "").Trim();
                PingReply reply = pingSend.Send(strIP, 1000);
                if (reply.Status == IPStatus.Success)
                    bRet = true;
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }

    }
}
