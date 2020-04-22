using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Larpx.ResourceSpider.CommonHelper
{
    public class ParameterBuilder : DataDictionary
    {
        public string GetEncodedParameters()
        {
            string text = string.Empty;
            foreach (Item current in this.m_oItems)
            {
                object obj = text;
                text = string.Concat(new object[]
                {
                    obj,
                    current.Key,
                    '=',
                    HttpUtility.UrlEncode(current.Value, Encoding.UTF8),
                    '&'
                });
            }
            text = text.TrimEnd(new char[]
            {
                '&'
            });
            return text;
        }

        public string GetParameters()
        {
            string text = string.Empty;
            foreach (Item current in this.m_oItems)
            {
                object obj = text;
                text = string.Concat(new object[]
                {
                    obj,
                    current.Key,
                    '=',
                    current.Value,
                    '&'
                });
            }
            text = text.TrimEnd(new char[]
            {
                '&'
            });
            return text;
        }

        public void SetEncodedParameters(string sParameters)
        {
            if (string.IsNullOrEmpty(sParameters))
            {
                return;
            }
            string[] array = sParameters.Split(new char[]
            {
                '&'
            });
            base.Clear();
            string[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                string text = array2[i];
                string[] array3 = text.Split(new char[]
                {
                    '='
                });
                if (array3.Length == 2)
                {
                    string[] array4 = array3[1].Split(new char[]
                    {
                        ','
                    });
                    string[] array5 = array4;
                    for (int j = 0; j < array5.Length; j++)
                    {
                        string str = array5[j];
                        base.Add(array3[0], HttpUtility.UrlDecode(str, Encoding.UTF8));
                    }
                }
            }
        }

        public void SetParameters(string sParameters)
        {
            if (string.IsNullOrEmpty(sParameters))
            {
                return;
            }
            string[] array = sParameters.Split(new char[]
            {
                '&'
            });
            base.Clear();
            string[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                string text = array2[i];
                string[] array3 = text.Split(new char[]
                {
                    '='
                });
                if (array3.Length == 2)
                {
                    string[] array4 = array3[1].Split(new char[]
                    {
                        ','
                    });
                    string[] array5 = array4;
                    for (int j = 0; j < array5.Length; j++)
                    {
                        string sValue = array5[j];
                        base.Add(array3[0], sValue);
                    }
                }
            }
        }

        protected const char KEY_SPLITER = '=';

        protected const char PARAMETER_SPLITER = '&';
    }

    public class DataDictionary
    {
        public DataDictionary()
        {
            this.m_oItems = new List<Item>();
        }

        public void Add(string sKey, string sValue)
        {
            DataDictionary.Item item = default(Item);
            item.Key = sKey;
            item.Value = sValue;
            this.m_oItems.Add(item);
        }

        public void Clear()
        {
            this.m_oItems.Clear();
        }

        public string GetValue(string sKey)
        {
            string result = string.Empty;
            foreach (Item current in this.m_oItems)
            {
                if (current.Key == sKey)
                {
                    result = current.Value;
                    break;
                }
            }
            return result;
        }

        public string GetValues(string sKey)
        {
            string text = string.Empty;
            foreach (Item current in this.m_oItems)
            {
                if (current.Key == sKey)
                {
                    text = text + current.Value + ',';
                }
            }
            text = text.TrimEnd(new char[]
            {
                ','
            });
            return text;
        }

        public void Remove(string sKey)
        {
            for (int i = 0; i < this.m_oItems.Count; i++)
            {
                if (this.m_oItems[i].Key == sKey)
                {
                    this.m_oItems.Remove(this.m_oItems[i]);
                    i--;
                }
            }
        }

        protected List<Item> m_oItems;

        protected const char VALUE_SPLITER = ',';

        protected struct Item
        {
            public string Key;

            public string Value;
        }
    }
}
