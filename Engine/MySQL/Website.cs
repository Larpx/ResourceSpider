using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class website : BaseModel.Website
    {
        public website()
        {

            this.Date = DateTime.Now;
            this.Status = Convert.ToByte("0");
            this.Priority = Convert.ToByte("0");
            this.Processed = Convert.ToByte("0");
            this.UpdateTime = DateTime.Now;
            this.UpdateTimes = Convert.ToInt32("0");
            this.Deleted = Convert.ToByte("0");
            this.IsCookies = Convert.ToByte("0");

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

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string ID
        {
            get
            {
                return this.ID;
            }

            set
            {
                this.ID = base.ID;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public new byte IsCookies
        {
            get
            {
                return this.IsCookies;
            }

            set
            {
                this.IsCookies = Convert.ToByte(base.IsCookies);
            }
        }

    }
}
