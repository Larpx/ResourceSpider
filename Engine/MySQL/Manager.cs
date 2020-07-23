using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class manager : BaseModel.Manager
    {
        private string _GUID;
        private string _UserName;
        private string _Password;
        private byte _Type;
        private byte _Status;
        private string _Name;
        private string _Mail;
        private string _Mobile;
        private byte _Deleted;

        public manager()
        {

            this._Type = Convert.ToByte("0");
            this._Status = Convert.ToByte("0");
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
        public new string UserName
        {
            get
            {
                return this._UserName;
            }

            set
            {
                this._UserName = base.UserName;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public new string Password
        {
            get
            {
                return this._Password;
            }

            set
            {
                this._Password = base.Password;
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
        /// Default:
        /// Nullable:True
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
        public new string Mail
        {
            get
            {
                return this._Mail;
            }

            set
            {
                this._Mail = base.Mail;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public new string Mobile
        {
            get
            {
                return this._Mobile;
            }

            set
            {
                this._Mobile = base.Mobile;
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
