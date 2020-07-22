using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class resourcedata : BaseModel.ResourceData
    {
        public resourcedata()
        {

            this.Date = DateTime.Now;
            this.ResourceType = Convert.ToByte("0");
            this.Status = Convert.ToByte("0");
            this.Size = Convert.ToInt64("0");
            this.Processed = Convert.ToByte("0");
            this.Deleted = Convert.ToByte("0");

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
        /// Nullable:False
        /// </summary>           
        public new string ObjectGUID
        {
            get
            {
                return this.ObjectGUID;
            }

            set
            {
                this.ObjectGUID = base.ObjectGUID.ToString();
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

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string URL
        {
            get
            {
                return this.URL;
            }

            set
            {
                this.URL = base.URL;
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
                return this.Original;
            }

            set
            {
                this.Original = base.Original;
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
                return this.Name;
            }

            set
            {
                this.Name = base.Name;
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
                return this.File;
            }

            set
            {
                this.File = base.File;
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
                return this.URLType;
            }

            set
            {
                this.URLType = base.URLType;
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
                return this.ResourceType;
            }

            set
            {
                this.ResourceType = base.ResourceType;
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
                return this.Status;
            }

            set
            {
                this.Status = base.Status;
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
                return this.Password;
            }

            set
            {
                this.Password = base.Password;
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
                return this.Size;
            }

            set
            {
                this.Size = base.Size;
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
                return this.Md5;
            }

            set
            {
                this.Md5 = base.Md5;
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
                return this.Hash;
            }

            set
            {
                this.Hash = base.Hash;
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
                return this.Processed;
            }

            set
            {
                this.Processed = base.Processed;
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
                return this.Memo;
            }

            set
            {
                this.Memo = base.Memo;
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
                return this.Deleted;
            }

            set
            {
                this.Deleted = Convert.ToByte(base.Deleted);
            }
        }
    }
}
