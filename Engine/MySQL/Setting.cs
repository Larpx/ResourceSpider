using System;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public class setting : BaseModel.Setting
    {
        public setting()
        {

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
        public new string Key
        {
            get
            {
                return this.Key;
            }

            set
            {
                this.Key = base.Key;
            }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public new string Value
        {
            get
            {
                return this.Value;
            }

            set
            {
                this.Value = base.Value;
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
