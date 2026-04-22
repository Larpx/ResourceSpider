using System.Reflection;

namespace ResourceSpider.Infrastructure.DataFlow.Storage.Entity;

/// <summary>
/// 实体接口，定义获取表元数据的方法
/// </summary>
public interface IEntity
{
    /// <summary>
    /// 获取实体的表元数据信息
    /// </summary>
    /// <returns>表元数据</returns>
    TableMetadata GetTableMetadata();
}

/// <summary>
/// 列元数据，描述数据库列的名称、类型、长度和约束
/// </summary>
public class Column
{
    /// <summary>
    /// 列名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 列数据类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 列最大长度
    /// </summary>
    public int Length { get; set; } = 255;

    /// <summary>
    /// 是否为必填列
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 关联的属性信息
    /// </summary>
    public PropertyInfo PropertyInfo { get; set; } = null!;
}

/// <summary>
/// 数据库架构特性，标注实体对应的数据库和表名
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SchemaAttribute : Attribute
{
    /// <summary>
    /// 数据库名称
    /// </summary>
    public string Database { get; }

    /// <summary>
    /// 表名称
    /// </summary>
    public string Table { get; }

    /// <summary>
    /// 表名后缀策略
    /// </summary>
    public TablePostfix TablePostfix { get; set; }

    /// <summary>
    /// 初始化数据库架构特性
    /// </summary>
    /// <param name="database">数据库名称</param>
    /// <param name="table">表名称</param>
    /// <param name="tablePostfix">表名后缀策略</param>
    public SchemaAttribute(string database, string table, TablePostfix tablePostfix = TablePostfix.None)
    {
        Database = database;
        Table = table;
        TablePostfix = tablePostfix;
    }
}

/// <summary>
/// 表名后缀策略枚举
/// </summary>
public enum TablePostfix { None, Monday, Month, Today }

/// <summary>
/// 存储模式枚举
/// </summary>
public enum StorageMode { Insert, InsertIgnoreDuplicate, InsertAndUpdate, Update }

/// <summary>
/// 表元数据，描述实体对应的数据库表结构信息
/// </summary>
public class TableMetadata
{
    /// <summary>
    /// 实体类型全名
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// 数据库架构信息
    /// </summary>
    public SchemaAttribute Schema { get; set; } = null!;

    /// <summary>
    /// 主键列名集合
    /// </summary>
    public HashSet<string> Primary { get; set; } = [];

    /// <summary>
    /// 索引元数据集合
    /// </summary>
    public HashSet<IndexMetadata> Indexes { get; } = [];

    /// <summary>
    /// 更新列名集合
    /// </summary>
    public HashSet<string> Updates { get; set; } = [];

    /// <summary>
    /// 列元数据字典，键为列名
    /// </summary>
    public Dictionary<string, Column> Columns { get; } = new();

    /// <summary>
    /// 获取是否为自增主键
    /// </summary>
    public bool IsAutoIncrementPrimary => Primary != null && Primary.Count == 1 && (Columns[Primary.First()].Type is "Int32" or "Int64");

    /// <summary>
    /// 判断指定列是否为主键
    /// </summary>
    /// <param name="column">列名</param>
    /// <returns>是主键返回 true，否则返回 false</returns>
    public bool IsPrimary(string column) => Primary != null && Primary.Contains(column);

    /// <summary>
    /// 获取是否存在主键
    /// </summary>
    public bool HasPrimary => Primary != null && Primary.Count > 0;

    /// <summary>
    /// 获取是否存在更新列
    /// </summary>
    public bool HasUpdateColumns => Updates != null && Updates.Count > 0;
}

/// <summary>
/// 索引元数据，描述数据库索引的列和唯一性
/// </summary>
public class IndexMetadata
{
    private readonly bool _isUnique;
    private readonly string _name;

    /// <summary>
    /// 初始化索引元数据
    /// </summary>
    /// <param name="columns">索引包含的列名数组</param>
    /// <param name="isUnique">是否为唯一索引</param>
    public IndexMetadata(string[] columns, bool isUnique = false) { Columns = columns; _isUnique = isUnique; _name = $"{(_isUnique ? "UNIQUE_" : "INDEX_")}{string.Join("_", columns.Select(x => x.ToUpper()))}"; }

    /// <summary>
    /// 索引名称
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// 是否为唯一索引
    /// </summary>
    public bool IsUnique => _isUnique;

    /// <summary>
    /// 索引包含的列名数组
    /// </summary>
    public string[] Columns { get; }

    /// <summary>
    /// 获取哈希码
    /// </summary>
    /// <returns>索引名称的哈希码</returns>
    public override int GetHashCode() => _name.GetHashCode();
}

/// <summary>
/// SQL 语句集合，包含建库、建表、插入、更新等操作的 SQL 模板
/// </summary>
public class SqlStatements
{
    /// <summary>
    /// 创建数据库的 SQL 语句
    /// </summary>
    public string CreateDatabaseSql { get; set; } = string.Empty;

    /// <summary>
    /// 创建表的 SQL 语句
    /// </summary>
    public string CreateTableSql { get; set; } = string.Empty;

    /// <summary>
    /// 插入数据的 SQL 语句
    /// </summary>
    public string InsertSql { get; set; } = string.Empty;

    /// <summary>
    /// 插入并忽略重复数据的 SQL 语句
    /// </summary>
    public string InsertIgnoreDuplicateSql { get; set; } = string.Empty;

    /// <summary>
    /// 更新数据的 SQL 语句
    /// </summary>
    public string UpdateSql { get; set; } = string.Empty;

    /// <summary>
    /// 插入或更新数据的 SQL 语句
    /// </summary>
    public string InsertAndUpdateSql { get; set; } = string.Empty;
}
