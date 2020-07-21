using System;
using System.Linq;
using System.Text;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class link : BaseModel.Link
    {
        public link()
        {

            this.Date = Convert.ToDateTime("CURRENT_TIMESTAMP");
            this.Type = Convert.ToByte("0");
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
        /// Nullable:True
        /// </summary>           
        public new string CategoryGUID
        {
            get
            {
                return this.CategoryGUID;
            }

            set
            {
                this.CategoryGUID = base.CategoryGUID.ToString();
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public new string SN
        {
            get
            {
                return this.SN;
            }

            set
            {
                this.SN = base.SN;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
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
        public new string Title
        {
            get
            {
                return this.Title;
            }

            set
            {
                this.Title = base.Title;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string TitleChs
        {
            get
            {
                return this.TitleChs;
            }

            set
            {
                this.TitleChs = base.TitleChs;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Brief
        {
            get
            {
                return this.Brief;
            }

            set
            {
                this.Brief = base.Brief;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string BriefChs
        {
            get
            {
                return this.BriefChs;
            }

            set
            {
                this.BriefChs = base.BriefChs;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Detail
        {
            get
            {
                return this.Detail;
            }

            set
            {
                this.Detail = base.Detail;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string DetailChs
        {
            get
            {
                return this.DetailChs;
            }

            set
            {
                this.DetailChs = base.DetailChs;
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
        public new byte Type
        {
            get
            {
                return this.Type;
            }

            set
            {
                this.Type = base.Type;
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
        public new string Remark
        {
            get
            {
                return this.Remark;
            }

            set
            {
                this.Remark = base.Remark;
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
