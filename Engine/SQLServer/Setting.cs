using System;
using SqlSugar;

namespace Larpx.ResourceSpider.Engine.SQLServer
{
    ///<summary>
    ///
    ///</summary>
    public partial class Setting
    {
        public Setting()
        {

            this.GUID = Guid.NewGuid();
            this.Deleted = false;

        }


        /// <summary>
        /// Desc:
        /// Default:newid()
        /// Nullable:False
        /// </summary>       
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public Guid GUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Key { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Value { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool Deleted { get; set; }

    }
}
