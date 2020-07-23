using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class propertydetail : BaseModel.PropertyDetail
    {
        private string _GUID;
        private string _LinkGUID;
        private string _PropertyKeyGUID;
        private string _KeyText;
        private string _PropertyValueGUID;
        private string _ValueText;
        private byte _Deleted;
        private string _WebsiteGUID;
        private string _CategoryGUID;
        private DateTime _Date;

        public propertydetail()
        {

            this._Deleted = Convert.ToByte("0");
            this._Date = DateTime.Now;

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
                return this._GUID;
            }

            set
            {
                this._GUID = base.GUID.ToString();
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public new string LinkGUID
        {
            get
            {
                return this._LinkGUID;
            }

            set
            {
                this._LinkGUID = base.LinkGUID.ToString();
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public new string PropertyKeyGUID
        {
            get
            {
                return this._PropertyKeyGUID;
            }

            set
            {
                this._PropertyKeyGUID = base.PropertyKeyGUID.ToString();
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string KeyText
        {
            get
            {
                return this._KeyText;
            }

            set
            {
                this._KeyText = base.KeyText;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string PropertyValueGUID
        {
            get
            {
                return this._PropertyValueGUID;
            }

            set
            {
                this._PropertyValueGUID = base.PropertyValueGUID.ToString();
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string ValueText
        {
            get
            {
                return this._ValueText;
            }

            set
            {
                this._ValueText = base.ValueText;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public new byte Deleted
        {
            get
            {
                return this._Deleted;
            }

            set
            {
                this._Deleted = Convert.ToByte(base.Deleted);
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
                return this._WebsiteGUID;
            }

            set
            {
                this._WebsiteGUID = base.WebsiteGUID.ToString();
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
                return this._CategoryGUID;
            }

            set
            {
                this._CategoryGUID = base.CategoryGUID.ToString();
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
                return this._Date;
            }

            set
            {
                this._Date = base.Date;
            }
        }
    }
}
