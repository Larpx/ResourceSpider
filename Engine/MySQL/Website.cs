using SqlSugar;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    /// <summary>
    /// 
    /// </summary>
    public class Website
    {
        /// <summary>
        /// 
        /// </summary>
        public Website()
        {
        }

        private System.Guid _GUID;
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public System.Guid GUID { get { return this._GUID; } set { this._GUID = value; } }

        private System.DateTime _Date;
        /// <summary>
        /// 
        /// </summary>
        public System.DateTime Date { get { return this._Date; } set { this._Date = value; } }

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

        private System.SByte _Status;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Status { get { return this._Status; } set { this._Status = value; } }

        private System.SByte _Priority;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Priority { get { return this._Priority; } set { this._Priority = value; } }

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

        private System.String _Memo;
        /// <summary>
        /// 
        /// </summary>
        public System.String Memo { get { return this._Memo; } set { this._Memo = value?.Trim(); } }

        private System.SByte _Deleted;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Deleted { get { return this._Deleted; } set { this._Deleted = value; } }

        private System.String _ID;
        /// <summary>
        /// 
        /// </summary>
        public System.String ID { get { return this._ID; } set { this._ID = value?.Trim(); } }

        private System.SByte _IsCookies;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte IsCookies { get { return this._IsCookies; } set { this._IsCookies = value; } }
    }
}