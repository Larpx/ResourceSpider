using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Larpx.ResourceSpider.DataFlow.Storage
{
    /// <summary>
    /// 表元数据
    /// </summary>
    public class TableMetadata
    {
        /// <summary>
        /// 实体类型名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Schema
        /// </summary>
        public Schema Schema { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public HashSet<string> Primary { get; set; }

        /// <summary>
        /// 索引
        /// </summary>
        public HashSet<IndexMetadata> Indexes { get; }

        /// <summary>
        /// 更新列
        /// </summary>
        public HashSet<string> Updates { get; set; }

        /// <summary>
        /// 属性名，属性数据类型的字典
        /// </summary>
        public Dictionary<string, Column> Columns { get; }

        /// <summary>
        /// 是否是自增主键
        /// </summary>
        public bool IsAutoIncrementPrimary => Primary != null && Primary.Count == 1 &&
                                              (Columns[Primary.First()].Type == "Int32" ||
                                               Columns[Primary.First()].Type == "Int64");

        /// <summary>
        /// 判断某一列是否在主键中
        /// </summary>
        /// <param name="column">列</param>
        /// <returns></returns>
        public bool IsPrimary(string column)
        {
            return Primary != null && Primary.Contains(column);
        }

        /// <summary>
        /// 判断是否有主键
        /// </summary>
        public bool HasPrimary => Primary != null && Primary.Count > 0;

        /// <summary>
        /// 判断是否有更新列
        /// </summary>
        public bool HasUpdateColumns => Updates != null && Updates.Count > 0;

        /// <summary>
        /// 构造方法
        /// </summary>
        public TableMetadata()
        {
            Indexes = new HashSet<IndexMetadata>();
            Columns = new Dictionary<string, Column>();
            Primary = new HashSet<string>();
            Updates = new HashSet<string>();
        }
    }

    /// <summary>
    /// Schema 信息
    /// </summary>
    public class Schema : Attribute
    {
        /// <summary>
        /// 数据库名
        /// </summary>
        public string Database { get; }

        /// <summary>
        /// 表名
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// 表名后缀
        /// </summary>
        public TablePostfix TablePostfix { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="database">数据库名</param>
        /// <param name="table">表名</param>
        /// <param name="tablePostfix">表名后缀</param>
        public Schema(string database, string table, TablePostfix tablePostfix = TablePostfix.None)
        {
            Database = database;
            Table = table;
            TablePostfix = tablePostfix;
        }
    }

    /// <summary>
    /// 索引元数据
    /// </summary>
    public class IndexMetadata
    {
        private readonly bool _isUnique;
        private readonly string _name;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="columns">列</param>
        /// <param name="isUnique">是否唯一索引</param>
        public IndexMetadata(string[] columns, bool isUnique = false)
        {
            Columns = columns;
            _isUnique = isUnique;
            _name = $"{(_isUnique ? "UNIQUE_" : "INDEX_")}{string.Join("_", columns.Select(x => x.ToUpper()))}";
        }

        /// <summary>
        /// 索引名称
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// 是否唯一索引
        /// </summary>
        public bool IsUnique => _isUnique;

        /// <summary>
        /// 索引的列
        /// </summary>
        public string[] Columns { get; }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }
    }

    /// <summary>
    /// 列信息
    /// </summary>
    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Length { get; set; } = 255;
        public bool Required { get; set; }

        /// <summary>
        /// 属性反射，用于设置解析值到实体对象
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
    }


    /// <summary>
    /// 表名后缀
    /// </summary>
    public enum TablePostfix
    {
        /// <summary>
        /// 无
        /// </summary>
        None,

        /// <summary>
        /// 表名的后缀为星期一的时间
        /// </summary>
        Monday,

        /// <summary>
        /// 表名的后缀为今天的时间 {name}_20171212
        /// </summary>
        Today,

        /// <summary>
        /// 表名的后缀为当月 {name}_201712
        /// </summary>
        Month
    }
}
