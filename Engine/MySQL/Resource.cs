using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class resource : BaseModel.Resource
    {
        private string _GUID;
        private string _WebsiteGUID;
        private string _PageGUID;
        private string _URL;
        private string _Original;
        private DateTime _Date;
        private byte _Type;
        private string _Path;
        private string _FileName;
        private string _Hash;
        private byte _Deleted;

        public resource()
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
        public new string PageGUID
        {
            get
            {
                return this._PageGUID;
            }

            set
            {
                this._PageGUID = base.PageGUID.ToString();
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string URL
        {
            get
            {
                return this._URL;
            }

            set
            {
                this._URL = base.URL;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public new string Original
        {
            get
            {
                return this._Original;
            }

            set
            {
                this._Original = base.Original;
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
        /// Default:
        /// Nullable:False
        /// </summary>           
        public new string Path
        {
            get
            {
                return this._Path;
            }

            set
            {
                this._Path = base.Path;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public new string FileName
        {
            get
            {
                return this._FileName;
            }

            set
            {
                this._FileName = base.FileName;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Hash
        {
            get
            {
                return this._Hash;
            }

            set
            {
                this._Hash = base.Hash;
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
