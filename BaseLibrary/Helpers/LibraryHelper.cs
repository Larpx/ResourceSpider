using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Larpx.ResourceSpider.Helpers
{
    public class LibraryHelper
    {
        /// <summary>
        /// 挂在模块功能
        /// </summary>
        private static void DoExce9(string path)
        {
            // ClassLibrary.dll

            //Byte[] byte1 = System.IO.File.ReadAllBytes(path);//也是可以的
            //Assembly assem = Assembly.Load(byte1);

            Assembly assem = Assembly.LoadFile(path);

            //string t_class = "mydll.Class1";//理论上已经加载了dll文件，可以通过命名空间加上类名获取类的类型，这里应该修改为如下：

            //string t_class = "mydll.Class1,mydll";//如果你想要得到的是被本工程内部的类，可以“命名空间.父类……类名”;如果是外部的，需要在后面加上“,链接库名”;

            //再次感谢thy38的帮助。

            //Type ty = Type.GetType(t_class);//这儿在调试的时候ty=null，一直不理解，望有高人可以解惑

            Type[] tys = assem.GetTypes();//只好得到所有的类型名，然后遍历，通过类型名字来区别了
            foreach (Type ty in tys)//huoquleiming
            {
                if (ty.Name == "MainClass")
                {
                    ConstructorInfo magicConstructor = ty.GetConstructor(Type.EmptyTypes);//获取不带参数的构造函数
                    object magicClassObject = magicConstructor.Invoke(new object[] { });//这里是获取一个类似于类的实例的东东

                    //object magicClassObject = Activator.CreateInstance(t);//获取无参数的构造实例还可以通过这样
                    MethodInfo mi = ty.GetMethod("Exce");
                    object aa = mi.Invoke(magicClassObject, new string[] { "666" });
                    //MessageBox.Show( aa.ToString() );//这儿是执行类class1的sayhello方法
                }
            }

            //AppDomain pluginDomain = (pluginInstanceContainer[key] as PluginEntity).PluginDomain;
            //if (pluginDomain != null)
            //{
            //  AppDomain.Unload(pluginDomain);
            // } 

        }
    }
}
