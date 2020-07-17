using SqlSugar;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    /// <summary>
    /// 
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// 
        /// </summary>
        public Setting()
        {
        }

        private System.Guid _GUID;
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public System.Guid GUID { get { return this._GUID; } set { this._GUID = value; } }

        private System.String _Key;
        /// <summary>
        /// 
        /// </summary>
        public System.String Key { get { return this._Key; } set { this._Key = value?.Trim(); } }

        private System.String _Value;
        /// <summary>
        /// 
        /// </summary>
        public System.String Value { get { return this._Value; } set { this._Value = value?.Trim(); } }

        private System.SByte _Deleted;
        /// <summary>
        /// 
        /// </summary>
        public System.SByte Deleted { get { return this._Deleted; } set { this._Deleted = value; } }
    }
}