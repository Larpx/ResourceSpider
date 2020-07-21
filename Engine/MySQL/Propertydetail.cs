using System;
using System.Linq;
using System.Text;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class propertydetail : BaseModel.PropertyDetail
    {
        public propertydetail()
        {

            this.Deleted = Convert.ToByte("0");
            this.Date = Convert.ToDateTime("CURRENT_TIMESTAMP");

        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public new string GUID
        {
            get
            {
                return this.GUID;
            }

            set
            {
                this.GUID = base.GUID.ToString();
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string LinkGUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string PropertyKeyGUID { get; set; }

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
        public string PropertyValueGUID { get; set; }

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
        public new byte Deleted
        {
            get
            {
                return this.Deleted;
            }

            set
            {
                this.Deleted = Convert.ToByte(base.Deleted);
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public new string WebsiteGUID
        {
            get
            {
                return this.WebsiteGUID;
            }

            set
            {
                this.WebsiteGUID = base.WebsiteGUID.ToString();
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string CategoryGUID
        {
            get
            {
                return this.CategoryGUID;
            }

            set
            {
                this.CategoryGUID = base.CategoryGUID.ToString();
            }
        }

        /// <summary>
        /// Desc:
        /// Default:CURRENT_TIMESTAMP
        /// Nullable:False
        /// </summary>           
        public new DateTime Date
        {
            get
            {
                return this.Date;
            }

            set
            {
                this.Date = base.Date;
            }
        }
    }
}
