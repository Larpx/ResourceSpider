using System;

namespace Larpx.ResourceSpider.Engine
{
    ///<summary>
    ///
    ///</summary>
    public partial class Website
    {
        public Website()
        {
            this.GUID = Guid.NewGuid();
            this.Date = DateTime.Now;
            this.Status = Convert.ToByte("0");
            this.Priority = Convert.ToByte("0");
            this.Processed = Convert.ToByte("0");
            this.UpdateTime = DateTime.Now;
            this.UpdateTimes = Convert.ToInt32("0");
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
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        public DateTime Date { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string URL { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Name { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string NameChs { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte Status { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte Priority { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte Processed { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int UpdateTimes { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Memo { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool Deleted { get; set; }

    }
}
