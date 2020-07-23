using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class systemlog : BaseModel.SystemLog
    {
        private string _GUID;
        private string _ManagerGUID;
        private long _IP;
        private DateTime _Date;
        private string _Message;
        private string _Module;
        private string _Action;
        private string _Result;
        private byte _Deleted;

        public systemlog()
        {

            this._IP = Convert.ToInt64("0");
            this._Date = DateTime.Now;
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
        public new string ManagerGUID
        {
            get
            {
                return this._ManagerGUID;
            }

            set
            {
                this._ManagerGUID = base.ManagerGUID.ToString();
            }
        }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public new long IP
        {
            get
            {
                return this._IP;
            }

            set
            {
                this._IP = base.IP;
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
        /// Nullable:False
        /// </summary>           
        public new string Message
        {
            get
            {
                return this._Message;
            }

            set
            {
                this._Message = base.Message;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Module
        {
            get
            {
                return this._Module;
            }

            set
            {
                this._Module = base.Module;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Action
        {
            get
            {
                return this._Action;
            }

            set
            {
                this._Action = base.Action;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Result
        {
            get
            {
                return this._Result;
            }

            set
            {
                this._Result = base.Result;
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
