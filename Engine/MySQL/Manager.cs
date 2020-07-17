using SqlSugar;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    /// <summary>
    /// 
    /// </summary>
    public class Manager
    {
        /// <summary>
        /// 
        /// </summary>
        public Manager()
        {
        }

        private System.Guid _GUID;
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public System.Guid GUID { get { return this._GUID; } set { this._GUID = value; } }

        private System.String _UserName;
        /// <summary>
        /// 
        /// </summary>
        public System.String UserName { get { return this._UserName; } set { this._UserName = value?.Trim(); } }

        private System.String _Password;
        /// <summary>
        /// 
        /// </summary>
        public System.String Password { get { return this._Password; } set { this._Password = value?.Trim(); } }

        private System.SByte _Type;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Type { get { return this._Type; } set { this._Type = value; } }

        private System.SByte _Status;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Status { get { return this._Status; } set { this._Status = value; } }

        private System.String _Name;
        /// <summary>
        /// 
        /// </summary>
        public System.String Name { get { return this._Name; } set { this._Name = value?.Trim(); } }

        private System.String _Mail;
        /// <summary>
        /// 
        /// </summary>
        public System.String Mail { get { return this._Mail; } set { this._Mail = value?.Trim(); } }

        private System.String _Mobile;
        /// <summary>
        /// 
        /// </summary>
        public System.String Mobile { get { return this._Mobile; } set { this._Mobile = value?.Trim(); } }

        private System.SByte _Deleted;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Deleted { get { return this._Deleted; } set { this._Deleted = value; } }
    }
}