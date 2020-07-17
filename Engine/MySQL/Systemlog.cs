using SqlSugar;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    /// <summary>
    /// 
    /// </summary>
    public class Systemlog
    {
        /// <summary>
        /// 
        /// </summary>
        public Systemlog()
        {
        }

        private System.Guid _GUID;
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public System.Guid GUID { get { return this._GUID; } set { this._GUID = value; } }

        private System.Guid _ManagerGUID;
        /// <summary>
        /// 
        /// </summary>
        public System.Guid ManagerGUID { get { return this._ManagerGUID; } set { this._ManagerGUID = value; } }

        private System.Int64 _IP;
        /// <summary>
        /// 
        /// </summary>
        public System.Int64 IP { get { return this._IP; } set { this._IP = value; } }

        private System.DateTime _Date;
        /// <summary>
        /// 
        /// </summary>
        public System.DateTime Date { get { return this._Date; } set { this._Date = value; } }

        private System.String _Message;
        /// <summary>
        /// 
        /// </summary>
        public System.String Message { get { return this._Message; } set { this._Message = value?.Trim(); } }

        private System.String _Module;
        /// <summary>
        /// 
        /// </summary>
        public System.String Module { get { return this._Module; } set { this._Module = value?.Trim(); } }

        private System.String _Action;
        /// <summary>
        /// 
        /// </summary>
        public System.String Action { get { return this._Action; } set { this._Action = value?.Trim(); } }

        private System.String _Result;
        /// <summary>
        /// 
        /// </summary>
        public System.String Result { get { return this._Result; } set { this._Result = value?.Trim(); } }

        private System.SByte _Deleted;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Deleted { get { return this._Deleted; } set { this._Deleted = value; } }
    }
}