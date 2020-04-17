using System.Runtime.InteropServices;
using System.Text;

namespace Larpx.ResourceSpider.CommonHelper
{
    public class YDMWrapper
    {
        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern void YDM_SetAppInfo(int nAppId, string lpAppKey);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_Login(string lpUserName, string lpPassWord);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_DecodeByPath(string lpFilePath, int nCodeType, StringBuilder pCodeResult);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_UploadByPath(string lpFilePath, int nCodeType);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_EasyDecodeByPath(string lpUserName, string lpPassWord, int nAppId, string lpAppKey, string lpFilePath, int nCodeType, int nTimeOut, StringBuilder pCodeResult);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_DecodeByBytes(byte[] lpBuffer, int nNumberOfBytesToRead, int nCodeType, StringBuilder pCodeResult);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_UploadByBytes(byte[] lpBuffer, int nNumberOfBytesToRead, int nCodeType);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_EasyDecodeByBytes(string lpUserName, string lpPassWord, int nAppId, string lpAppKey, byte[] lpBuffer, int nNumberOfBytesToRead, int nCodeType, int nTimeOut, StringBuilder pCodeResult);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_GetResult(int nCaptchaId, StringBuilder pCodeResult);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_Report(int nCaptchaId, bool bCorrect);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_EasyReport(string lpUserName, string lpPassWord, int nAppId, string lpAppKey, int nCaptchaId, bool bCorrect);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_GetBalance(string lpUserName, string lpPassWord);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_EasyGetBalance(string lpUserName, string lpPassWord, int nAppId, string lpAppKey);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_SetTimeOut(int nTimeOut);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_Reg(string lpUserName, string lpPassWord, string lpEmail, string lpMobile, string lpQQUin);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_EasyReg(int nAppId, string lpAppKey, string lpUserName, string lpPassWord, string lpEmail, string lpMobile, string lpQQUin);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_Pay(string lpUserName, string lpPassWord, string lpCard);

        [DllImport("../Lib/yundamaAPI.dll")]
        public static extern int YDM_EasyPay(string lpUserName, string lpPassWord, long nAppId, string lpAppKey, string lpCard);

    }
}
