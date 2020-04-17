using System;

namespace Larpx.ResourceSpider.Engine
{
    ///<summary>
    ///
    ///</summary>
    public partial class SystemLog
    {
        public SystemLog()
        {

            this.GUID = Guid.NewGuid();
            this.IP = Convert.ToInt64("0");
            this.Date = DateTime.Now;
            this.Deleted = false;

        }

        /// <summary>
        /// Desc:
        /// Default:newid()
        /// Nullable:False
        /// </summary>           
        public Guid GUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid ManagerGUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long IP { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        public DateTime Date { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Message { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Module { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Action { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Result { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool Deleted { get; set; }

    }
}
