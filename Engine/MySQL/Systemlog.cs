using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class systemlog : BaseModel.SystemLog
    {
        public systemlog()
        {

            this.IP = Convert.ToInt64("0");
            this.Date = DateTime.Now;
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
        public new string ManagerGUID
        {
            get
            {
                return this.ManagerGUID;
            }

            set
            {
                this.ManagerGUID = base.ManagerGUID.ToString();
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
                return this.IP;
            }

            set
            {
                this.IP = base.IP;
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
        /// Nullable:False
        /// </summary>           
        public new string Message
        {
            get
            {
                return this.Message;
            }

            set
            {
                this.Message = base.Message;
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
                return this.Module;
            }

            set
            {
                this.Module = base.Module;
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
                return this.Action;
            }

            set
            {
                this.Action = base.Action;
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
                return this.Result;
            }

            set
            {
                this.Result = base.Result;
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
