using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class category : BaseModel.Category
    {
        public category()
        {
            this.Date = Convert.ToDateTime("CURRENT_TIMESTAMP");
            this.Status = Convert.ToByte("0");
            this.Priority = Convert.ToByte("0");
            this.Processed = Convert.ToByte("0");
            this.UpdateTime = Convert.ToDateTime("CURRENT_TIMESTAMP");
            this.UpdateTimes = Convert.ToInt32("0");
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
        /// Nullable:True
        /// </summary>           
        public new string NameChs
        {
            get
            {
                return this.NameChs;
            }

            set
            {
                this.NameChs = base.NameChs;
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
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public new byte Priority
        {
            get
            {
                return this.Priority;
            }

            set
            {
                this.Priority = base.Priority;
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
        /// Default:CURRENT_TIMESTAMP
        /// Nullable:False
        /// </summary>           
        public new DateTime UpdateTime
        {
            get
            {
                return this.UpdateTime;
            }

            set
            {
                this.UpdateTime = base.UpdateTime;
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
                return this.UpdateTimes;
            }

            set
            {
                this.UpdateTimes = base.UpdateTimes;
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
