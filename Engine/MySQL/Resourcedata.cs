using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class resourcedata : BaseModel.ResourceData
    {
        private byte _Deleted;
        private string _Memo;
        private byte _Processed;
        private string _Hash;
        private string _Md5;
        private long? _Size;
        private string _Password;
        private byte _Status;
        private byte _ResourceType;
        private byte _URLType;
        private string _File;
        private string _Name;
        private string _Original;
        private string _URL;
        private DateTime _Date;
        private string _ObjectGUID;
        private string _WebsiteGUID;
        private string _GUID;

        public resourcedata()
        {

            this._Date = DateTime.Now;
            this._ResourceType = Convert.ToByte("0");
            this._Status = Convert.ToByte("0");
            this._Size = Convert.ToInt64("0");
            this._Processed = Convert.ToByte("0");
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
        public new string ObjectGUID
        {
            get
            {
                return this._ObjectGUID;
            }

            set
            {
                this._ObjectGUID = base.ObjectGUID.ToString();
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
        /// Nullable:False
        /// </summary>           
        public new string File
        {
            get
            {
                return this._File;
            }

            set
            {
                this._File = base.File;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public new byte URLType
        {
            get
            {
                return this._URLType;
            }

            set
            {
                this._URLType = base.URLType;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public new byte ResourceType
        {
            get
            {
                return this._ResourceType;
            }

            set
            {
                this._ResourceType = base.ResourceType;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public new byte Status
        {
            get
            {
                return this._Status;
            }

            set
            {
                this._Status = base.Status;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Password
        {
            get
            {
                return this._Password;
            }

            set
            {
                this._Password = base.Password;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        public new long? Size
        {
            get
            {
                return this._Size;
            }

            set
            {
                this._Size = base.Size;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Md5
        {
            get
            {
                return this._Md5;
            }

            set
            {
                this._Md5 = base.Md5;
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
        public new byte Processed
        {
            get
            {
                return this._Processed;
            }

            set
            {
                this._Processed = base.Processed;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Memo
        {
            get
            {
                return this._Memo;
            }

            set
            {
                this._Memo = base.Memo;
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
