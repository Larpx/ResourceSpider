using System;
using SqlSugar;

namespace Larpx.ResourceSpider.Engine
{
    ///<summary>
    ///
    ///</summary>
    public partial class PropertyDetail
    {
        public PropertyDetail()
        {

            this.GUID = Guid.NewGuid();
            this.Deleted = false;
            this.Date = DateTime.Now;

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
        public Guid WebsiteGUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid? CategoryGUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        public DateTime Date { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid LinkGUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid PropertyKeyGUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string KeyText { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid? PropertyValueGUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ValueText { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool Deleted { get; set; }

    }
}
