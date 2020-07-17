using SqlSugar;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    /// <summary>
    /// 
    /// </summary>
    public class Link
    {
        /// <summary>
        /// 
        /// </summary>
        public Link()
        {
        }

        private System.Guid _GUID;
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public System.Guid GUID { get { return this._GUID; } set { this._GUID = value; } }

        private System.Guid _WebsiteGUID;
        /// <summary>
        /// 
        /// </summary>
        public System.Guid WebsiteGUID { get { return this._WebsiteGUID; } set { this._WebsiteGUID = value; } }

        private System.Guid? _CategoryGUID;
        /// <summary>
        /// 
        /// </summary>
        public System.Guid? CategoryGUID { get { return this._CategoryGUID; } set { this._CategoryGUID = value ?? default(System.Guid); } }

        private System.String _SN;
        /// <summary>
        /// 
        /// </summary>
        public System.String SN { get { return this._SN; } set { this._SN = value?.Trim(); } }

        private System.String _ID;
        /// <summary>
        /// 
        /// </summary>
        public System.String ID { get { return this._ID; } set { this._ID = value?.Trim(); } }

        private System.String _URL;
        /// <summary>
        /// 
        /// </summary>
        public System.String URL { get { return this._URL; } set { this._URL = value?.Trim(); } }

        private System.String _Name;
        /// <summary>
        /// 
        /// </summary>
        public System.String Name { get { return this._Name; } set { this._Name = value?.Trim(); } }

        private System.String _NameChs;
        /// <summary>
        /// 
        /// </summary>
        public System.String NameChs { get { return this._NameChs; } set { this._NameChs = value?.Trim(); } }

        private System.String _Title;
        /// <summary>
        /// 
        /// </summary>
        public System.String Title { get { return this._Title; } set { this._Title = value?.Trim(); } }

        private System.String _TitleChs;
        /// <summary>
        /// 
        /// </summary>
        public System.String TitleChs { get { return this._TitleChs; } set { this._TitleChs = value?.Trim(); } }

        private System.String _Brief;
        /// <summary>
        /// 
        /// </summary>
        public System.String Brief { get { return this._Brief; } set { this._Brief = value?.Trim(); } }

        private System.String _BriefChs;
        /// <summary>
        /// 
        /// </summary>
        public System.String BriefChs { get { return this._BriefChs; } set { this._BriefChs = value?.Trim(); } }

        private System.String _Detail;
        /// <summary>
        /// 
        /// </summary>
        public System.String Detail { get { return this._Detail; } set { this._Detail = value?.Trim(); } }

        private System.String _DetailChs;
        /// <summary>
        /// 
        /// </summary>
        public System.String DetailChs { get { return this._DetailChs; } set { this._DetailChs = value?.Trim(); } }

        private System.DateTime _Date;
        /// <summary>
        /// 
        /// </summary>
        public System.DateTime Date { get { return this._Date; } set { this._Date = value; } }

        private System.SByte _Type;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Type { get { return this._Type; } set { this._Type = value; } }

        private System.SByte _Processed;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Processed { get { return this._Processed; } set { this._Processed = value; } }

        private System.DateTime _UpdateTime;
        /// <summary>
        /// 
        /// </summary>
        public System.DateTime UpdateTime { get { return this._UpdateTime; } set { this._UpdateTime = value; } }

        private System.Int32 _UpdateTimes;
        /// <summary>
        /// 
        /// </summary>
        public System.Int32 UpdateTimes { get { return this._UpdateTimes; } set { this._UpdateTimes = value; } }

        private System.String _Remark;
        /// <summary>
        /// 
        /// </summary>
        public System.String Remark { get { return this._Remark; } set { this._Remark = value?.Trim(); } }

        private System.SByte _Deleted;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Deleted { get { return this._Deleted; } set { this._Deleted = value; } }
    }
}