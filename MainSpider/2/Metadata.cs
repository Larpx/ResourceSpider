using System;
using System.Linq;
using System.Text;

namespace Larpx.ResourceSpider.Engine.MySQL
{
    ///<summary>
    ///
    ///</summary>
    public partial class metadata
    {
           public metadata(){

            this.Date =Convert.ToDateTime("CURRENT_TIMESTAMP");
            this.Type =Convert.ToByte("0");
            this.Deleted =Convert.ToByte("0");

           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string GUID {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string WebsiteGUID {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Name {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Content {get;set;}

           /// <summary>
           /// Desc:
           /// Default:CURRENT_TIMESTAMP
           /// Nullable:False
           /// </summary>           
           public DateTime Date {get;set;}

           /// <summary>
           /// Desc:
           /// Default:0
           /// Nullable:False
           /// </summary>           
           public byte Type {get;set;}

           /// <summary>
           /// Desc:
           /// Default:0
           /// Nullable:False
           /// </summary>           
           public byte Deleted {get;set;}

    }
}
