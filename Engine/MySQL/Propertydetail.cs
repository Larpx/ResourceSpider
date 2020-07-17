using SqlSugar;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    /// <summary>
    /// 
    /// </summary>
    public class Propertydetail
    {
        /// <summary>
        /// 
        /// </summary>
        public Propertydetail()
        {
        }

        private System.Guid _GUID;
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public System.Guid GUID { get { return this._GUID; } set { this._GUID = value; } }

        private System.Guid _LinkGUID;
        /// <summary>
        /// 
        /// </summary>
        public System.Guid LinkGUID { get { return this._LinkGUID; } set { this._LinkGUID = value; } }

        private System.Guid _PropertyKeyGUID;
        /// <summary>
        /// 
        /// </summary>
        public System.Guid PropertyKeyGUID { get { return this._PropertyKeyGUID; } set { this._PropertyKeyGUID = value; } }

        private System.String _KeyText;
        /// <summary>
        /// 
        /// </summary>
        public System.String KeyText { get { return this._KeyText; } set { this._KeyText = value?.Trim(); } }

        private System.Guid? _PropertyValueGUID;
        /// <summary>
        /// 
        /// </summary>
        public System.Guid? PropertyValueGUID { get { return this._PropertyValueGUID; } set { this._PropertyValueGUID = value ?? default(System.Guid); } }

        private System.String _ValueText;
        /// <summary>
        /// 
        /// </summary>
        public System.String ValueText { get { return this._ValueText; } set { this._ValueText = value?.Trim(); } }

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