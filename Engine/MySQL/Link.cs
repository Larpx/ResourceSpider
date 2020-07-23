using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class link : BaseModel.Link
    {
        private string _GUID;
        private string _WebsiteGUID;
        private string _CategoryGUID;
        private string _SN;
        private string _ID;
        private string _URL;
        private string _Name;
        private string _NameChs;
        private string _Title;
        private string _TitleChs;
        private string _Brief;
        private string _BriefChs;
        private string _Detail;
        private string _DetailChs;
        private DateTime _Date;
        private byte _Type;
        private byte _Processed;
        private DateTime _UpdateTime;
        private int _UpdateTimes;
        private string _Remark;
        private byte _Deleted;

        public link()
        {

            this._Date = DateTime.Now;
            this._Type = Convert.ToByte("0");
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
        /// Nullable:True
        /// </summary>           
        public new string CategoryGUID
        {
            get
            {
                return this._CategoryGUID;
            }

            set
            {
                this._CategoryGUID = base.CategoryGUID.ToString();
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
                return this._SN;
            }

            set
            {
                this._SN = base.SN;
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
                return this._ID;
            }

            set
            {
                this._ID = base.ID;
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
        public new string Title
        {
            get
            {
                return this._Title;
            }

            set
            {
                this._Title = base.Title;
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
                return this._TitleChs;
            }

            set
            {
                this._TitleChs = base.TitleChs;
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
                return this._Brief;
            }

            set
            {
                this._Brief = base.Brief;
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
                return this._BriefChs;
            }

            set
            {
                this._BriefChs = base.BriefChs;
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
                return this._Detail;
            }

            set
            {
                this._Detail = base.Detail;
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
                return this._DetailChs;
            }

            set
            {
                this._DetailChs = base.DetailChs;
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
        public new byte Type
        {
            get
            {
                return this._Type;
            }

            set
            {
                this._Type = base.Type;
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
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Remark
        {
            get
            {
                return this._Remark;
            }

            set
            {
                this._Remark = base.Remark;
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
