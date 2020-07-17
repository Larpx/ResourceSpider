using SqlSugar;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    /// <summary>
    /// 
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// 
        /// </summary>
        public Resource()
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

        private System.Guid _PageGUID;
        /// <summary>
        /// 
        /// </summary>
        public System.Guid PageGUID { get { return this._PageGUID; } set { this._PageGUID = value; } }

        private System.String _URL;
        /// <summary>
        /// 
        /// </summary>
        public System.String URL { get { return this._URL; } set { this._URL = value?.Trim(); } }

        private System.String _Original;
        /// <summary>
        /// 
        /// </summary>
        public System.String Original { get { return this._Original; } set { this._Original = value?.Trim(); } }

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

        private System.String _Path;
        /// <summary>
        /// 
        /// </summary>
        public System.String Path { get { return this._Path; } set { this._Path = value?.Trim(); } }

        private System.String _FileName;
        /// <summary>
        /// 
        /// </summary>
        public System.String FileName { get { return this._FileName; } set { this._FileName = value?.Trim(); } }

        private System.String _Hash;
        /// <summary>
        /// 
        /// </summary>
        public System.String Hash { get { return this._Hash; } set { this._Hash = value?.Trim(); } }

        private System.SByte _Deleted;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Deleted { get { return this._Deleted; } set { this._Deleted = value; } }
    }
}