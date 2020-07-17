using SqlSugar;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    /// <summary>
    /// 
    /// </summary>
    public class Resourcedata
    {
        /// <summary>
        /// 
        /// </summary>
        public Resourcedata()
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

        private System.Guid _ObjectGUID;
        /// <summary>
        /// 
        /// </summary>
        public System.Guid ObjectGUID { get { return this._ObjectGUID; } set { this._ObjectGUID = value; } }

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

        private System.String _Original;
        /// <summary>
        /// 
        /// </summary>
        public System.String Original { get { return this._Original; } set { this._Original = value?.Trim(); } }

        private System.String _Name;
        /// <summary>
        /// 
        /// </summary>
        public System.String Name { get { return this._Name; } set { this._Name = value?.Trim(); } }

        private System.String _File;
        /// <summary>
        /// 
        /// </summary>
        public System.String File { get { return this._File; } set { this._File = value?.Trim(); } }

        private System.SByte _URLType;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte URLType { get { return this._URLType; } set { this._URLType = value; } }

        private System.SByte _ResourceType;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte ResourceType { get { return this._ResourceType; } set { this._ResourceType = value; } }

        private System.SByte _Status;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Status { get { return this._Status; } set { this._Status = value; } }

        private System.String _Password;
        /// <summary>
        /// 
        /// </summary>
        public System.String Password { get { return this._Password; } set { this._Password = value?.Trim(); } }

        private System.Int64? _Size;
        /// <summary>
        /// 
        /// </summary>
        public System.Int64? Size { get { return this._Size; } set { this._Size = value ?? default(System.Int64); } }

        private System.String _Md5;
        /// <summary>
        /// 
        /// </summary>
        public System.String Md5 { get { return this._Md5; } set { this._Md5 = value?.Trim(); } }

        private System.String _Hash;
        /// <summary>
        /// 
        /// </summary>
        public System.String Hash { get { return this._Hash; } set { this._Hash = value?.Trim(); } }

        private System.SByte _Processed;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Processed { get { return this._Processed; } set { this._Processed = value; } }

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
    }
}