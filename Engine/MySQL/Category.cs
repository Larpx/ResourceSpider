using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class category : BaseModel.Category
    {
        private DateTime _Date;
        private string _GUID;
        private string _WebsiteGUID;
        private string _Name;
        private string _NameChs;
        private string _URL;
        private byte _Status;
        private byte _Priority;
        private byte _Processed;
        private DateTime _UpdateTime;
        private int _UpdateTimes;
        private byte _Deleted;

        public category()
        {
            this._Date = DateTime.Now;
            this._Status = Convert.ToByte("0");
            this._Priority = Convert.ToByte("0");
            this._Processed = Convert.ToByte("0");
            this._UpdateTime = DateTime.Now;
            this._UpdateTimes = Convert.ToInt32("0");
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
        public new string NameChs
        {
            get
            {
                return this._NameChs;
            }

            set
            {
                this._NameChs = base.NameChs;
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
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public new byte Priority
        {
            get
            {
                return this._Priority;
            }

            set
            {
                this._Priority = base.Priority;
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
        /// Default:CURRENT_TIMESTAMP
        /// Nullable:False
        /// </summary>           
        public new DateTime UpdateTime
        {
            get
            {
                return this._UpdateTime;
            }

            set
            {
                this._UpdateTime = base.UpdateTime;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public new int UpdateTimes
        {
            get
            {
                return this._UpdateTimes;
            }

            set
            {
                this._UpdateTimes = base.UpdateTimes;
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
