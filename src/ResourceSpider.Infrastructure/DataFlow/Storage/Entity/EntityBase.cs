using System.Reflection;

namespace ResourceSpider.Infrastructure.DataFlow.Storage.Entity;

public interface IEntity
{
    TableMetadata GetTableMetadata();
}

public class Column
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Length { get; set; } = 255;
    public bool Required { get; set; }
    public PropertyInfo PropertyInfo { get; set; } = null!;
}

[AttributeUsage(AttributeTargets.Class)]
public class SchemaAttribute : Attribute
{
    public string Database { get; }
    public string Table { get; }
    public TablePostfix TablePostfix { get; set; }

    public SchemaAttribute(string database, string table, TablePostfix tablePostfix = TablePostfix.None)
    {
        Database = database;
        Table = table;
        TablePostfix = tablePostfix;
    }
}

public enum TablePostfix { None, Monday, Month, Today }

public enum StorageMode { Insert, InsertIgnoreDuplicate, InsertAndUpdate, Update }

public class TableMetadata
{
    public string TypeName { get; set; } = string.Empty;
    public SchemaAttribute Schema { get; set; } = null!;
    public HashSet<string> Primary { get; set; } = [];
    public HashSet<IndexMetadata> Indexes { get; } = [];
    public HashSet<string> Updates { get; set; } = [];
    public Dictionary<string, Column> Columns { get; } = new();
    public bool IsAutoIncrementPrimary => Primary != null && Primary.Count == 1 && (Columns[Primary.First()].Type is "Int32" or "Int64");
    public bool IsPrimary(string column) => Primary != null && Primary.Contains(column);
    public bool HasPrimary => Primary != null && Primary.Count > 0;
    public bool HasUpdateColumns => Updates != null && Updates.Count > 0;
}

public class IndexMetadata
{
    private readonly bool _isUnique;
    private readonly string _name;
    public IndexMetadata(string[] columns, bool isUnique = false) { Columns = columns; _isUnique = isUnique; _name = $"{(_isUnique ? "UNIQUE_" : "INDEX_")}{string.Join("_", columns.Select(x => x.ToUpper()))}"; }
    public string Name => _name;
    public bool IsUnique => _isUnique;
    public string[] Columns { get; }
    public override int GetHashCode() => _name.GetHashCode();
}

public class SqlStatements
{
    public string CreateDatabaseSql { get; set; } = string.Empty;
    public string CreateTableSql { get; set; } = string.Empty;
    public string InsertSql { get; set; } = string.Empty;
    public string InsertIgnoreDuplicateSql { get; set; } = string.Empty;
    public string UpdateSql { get; set; } = string.Empty;
    public string InsertAndUpdateSql { get; set; } = string.Empty;
}
