using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Larpx.ResourceSpider.BaseLibrary.Data
{
    /// <summary>
    /// Json通信模型，返回任务结果集合
    /// </summary>
    [Serializable]
    public class ResultList
    {
        private Int32 _Code;
        private String _Message;
        private int _Count;
        private List<ProcessTask> _Data;

        public ResultList()
        {
            Count = 0;
            _Data = new List<ProcessTask>();
        }

        public ResultList(Int32 _C, String _M, List<ProcessTask> _D)
        {
            Code = _C;
            Message = _M;
            Count = _D.Count;
            Data = _D;
        }

        /// <summary>
        /// 程序结果
        /// 0.执行失败
        /// 1.执行成功
        /// </summary>
        public int Code { get => _Code; set => _Code = value; }

        /// <summary>
        /// 请求结果信息
        /// </summary>
        public string Message { get => _Message; set => _Message = value; }

        /// <summary>
        /// 结果集合数量
        /// </summary>
        public int Count { get => _Count; set => _Count = value; }

        /// <summary>
        /// 结果结合
        /// </summary>
        public List<ProcessTask> Data { get => _Data; set => _Data = value; }
    }

    /// <summary>
    /// Json通信模型
    /// </summary>
    [Serializable]
    public class Result
    {
        private Int32 _Code;
        private String _Message;
        private Object _Data;

        public int Code { get => _Code; set => _Code = value; }
        public string Message { get => _Message; set => _Message = value; }
        public Object Data { get => _Data; set => _Data = value; }

        public Result()
        {
        }

        public Result(Int32 _C, String _M, Object _D)
        {
            Code = _C;
            Message = _M;
            Data = _D;
        }
    }

    /// <summary>
    /// 任务模型
    /// </summary>
    [Serializable]
    public class ProcessTask
    {
        private Guid _GUID;
        private string _ID;
        private string _ANSI;
        private string _Link;
        private byte _Type;

        public ProcessTask(Guid guid, string sID, string sANSI, string sLink)
        {
            _GUID = guid;
            _ID = sID;
            ANSI = sANSI;
            _Link = sLink;
            _Type = 0;
        }

        public ProcessTask(Guid guid, string sID, string sANSI, string sLink, byte bStatus)
        {
            _GUID = guid;
            _ID = sID;
            ANSI = sANSI;
            _Link = sLink;
            _Type = bStatus;
        }

        /// <summary>
        /// GUID
        /// </summary>
        public Guid GUID { get => _GUID; set => _GUID = value; }

        /// <summary>
        /// 校验值
        /// </summary>
        public string ID { get => _ID; set => _ID = value; }

        /// <summary>
        /// 商品链接
        /// </summary>
        public string Link { get => _Link; set => _Link = value; }

        /// <summary>
        /// 商品ANSI
        /// </summary>
        public string ANSI { get => _ANSI; set => _ANSI = value; }

        /// <summary>
        /// 操作类型
        /// 0.补充详情
        /// 1.更新信息
        /// 2.更新价格
        /// 3.翻译
        /// </summary>
        public byte Type { get => _Type; set => _Type = value; }
    }

    /// <summary>
    /// 附加内容
    /// </summary>
    [Serializable]
    public class AttachmentsItem
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 详情
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public string color { get; set; }
    }

    /// <summary>
    /// 倍洽机器人提示信息
    /// </summary>
    [Serializable]
    public class BearyChat
    {
        public BearyChat()
        {
            attachments = new List<AttachmentsItem>();
        }

        /// <summary>
        /// 愿原力与你同在
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// 附加内容
        /// </summary>
        public List<AttachmentsItem> attachments { get; set; }
    }
}
