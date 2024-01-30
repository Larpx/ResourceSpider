using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Larpx.ResourceSpider.BaseLibrary.Helpers.Web
{
    public class JavaScriptHelper
    {
        private IJsEngineSwitcher oEngineSwitcher;

        public JavaScriptHelper()
        {
            if (oEngineSwitcher == null)
                oEngineSwitcher = JsEngineSwitcher.Current;
            oEngineSwitcher.EngineFactories.Add(new ChakraCoreJsEngineFactory());
            oEngineSwitcher.DefaultEngineName = ChakraCoreJsEngine.EngineName;
            oEngineSwitcher.EngineFactories.AddChakraCore(new ChakraCoreSettings
            {
                //否禁用对eval函数的调用
                DisableEval = true,
                //启用实验性功能
                EnableExperimentalFeatures = false
            });
        }

        /// <summary>
        /// 执行JS文件
        /// </summary>
        /// <param name="sExpression">要执行的方法名称</param>
        /// <param name="sPath">JS文件路径</param>
        /// <param name="oIList">参数名值对集合</param>
        /// <returns></returns>
        public string ExecuteScriptFile(string sExpression, string sPath, IEnumerable<KeyValuePair<string, string>> oIList = null)
        {
            try
            {
                if (!File.Exists(sPath))
                    throw new Exception("js文件不存在");

                string sResult = "";
                using (IJsEngine oEngine = JsEngineSwitcher.Current.CreateDefaultEngine())
                {
                    //指定执行文件
                    oEngine.ExecuteFile(sPath);

                    if (oIList != null)
                    {
                        //追加参数
                        foreach (var item in oIList)
                        {
                            oEngine.SetVariableValue(item.Key, item.Value);
                        }
                    }

                    //执行
                    sResult = oEngine.CallFunction(sExpression).ToString();
                }
                return sResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 执行JS
        /// </summary>
        /// <param name="sExpression">参数体</param>
        /// <param name="sCode">JavaScript代码、JS文件路径</param>
        /// <param name="oIList">参数名值对集合</param>
        /// <returns></returns>
        public string ExecuteScript(string sExpression, string sCode, IEnumerable<KeyValuePair<string, string>> oIList = null)
        {
            try
            {
                string sResult = "";
                using (IJsEngine oEngine = JsEngineSwitcher.Current.CreateDefaultEngine())
                {
                    //指定执行文件
                    oEngine.Execute(sCode);

                    if (oIList != null)
                    {
                        //追加参数
                        foreach (var item in oIList)
                        {
                            oEngine.SetVariableValue(item.Key, item.Value);
                        }
                    }

                    //执行
                    sResult = oEngine.CallFunction(sExpression).ToString();
                }
                return sResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
