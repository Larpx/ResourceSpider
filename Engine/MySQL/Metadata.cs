using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class metadata : BaseModel.MetaData
    {
        private string _GUID;
        private string _WebsiteGUID;
        private string _Name;
        private string _Content;
        private DateTime _Date;
        private byte _Type;
        private byte _Deleted;

        public metadata()
        {
            
            this._Date = DateTime.Now;
            this._Type = Convert.ToByte("0");
            this._Deleted = Convert.ToByte("0");

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
        /// Nullable:False
        /// </summary>           
        public new string Name
        {
            get
            {
                return this._Name;
            }

            set
            {
                this._Name = base.Name;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Content
        {
            get
            {
                return this._Content;
            }

            set
            {
                this._Content = base.Content;
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

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public new byte Type
        {
            get
            {
                return this._Type;
            }

            set
            {
                this._Type = base.Type;
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
    }
}
