using SqlSugar;
using System;

namespace Larpx.ResourceSpider.Engine
{
    ///<summary>
    ///
    ///</summary>
    public partial class MetaData
    {
        public MetaData()
        {
            this.GUID = Guid.NewGuid();
            this.Date = DateTime.Now;
            this.Type = Convert.ToByte("0");
            this.Deleted = false;
        }

        /// <summary>
        /// Desc:
        /// Default:newid()
        /// Nullable:False
        /// </summary>       
        [SugarColumn(IsPrimaryKey = true)]
        public Guid GUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        public Guid WebsiteGUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Name { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Content { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        public DateTime Date { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte Type { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool Deleted { get; set; }

    }
}
