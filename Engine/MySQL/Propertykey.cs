using SqlSugar;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    /// <summary>
    /// 
    /// </summary>
    public class Propertykey
    {
        /// <summary>
        /// 
        /// </summary>
        public Propertykey()
        {
        }

        private System.Guid _GUID;
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public System.Guid GUID { get { return this._GUID; } set { this._GUID = value; } }

        private System.SByte _Type;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Type { get { return this._Type; } set { this._Type = value; } }

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

        private System.SByte _Deleted;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Deleted { get { return this._Deleted; } set { this._Deleted = value; } }

        private System.Guid _WebsiteGUID;
        /// <summary>
        /// 
        /// </summary>
        public System.Guid WebsiteGUID { get { return this._WebsiteGUID; } set { this._WebsiteGUID = value; } }

        private System.Guid _CategoryGUID;
        /// <summary>
        /// 
        /// </summary>
        public System.Guid CategoryGUID { get { return this._CategoryGUID; } set { this._CategoryGUID = value; } }

        private System.DateTime _Date;
        /// <summary>
        /// 
        /// </summary>
        public System.DateTime Date { get { return this._Date; } set { this._Date = value; } }
    }
}