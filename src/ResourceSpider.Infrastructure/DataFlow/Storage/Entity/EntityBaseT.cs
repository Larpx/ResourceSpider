using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using ResourceSpider.Core.Exceptions;

namespace ResourceSpider.Infrastructure.DataFlow.Storage.Entity;

public abstract class EntityBase<T> : IEntity where T : class, new()
{
    private readonly Lazy<TableMetadata> _tableMetadata = new();

    public TableMetadata GetTableMetadata()
    {
        Configure();
        var type = GetType();
        var schema = type.GetCustomAttributes(typeof(SchemaAttribute), false).FirstOrDefault();
        if (schema != null)
        {
            _tableMetadata.Value.Schema = (SchemaAttribute)schema;
            if (string.IsNullOrWhiteSpace(_tableMetadata.Value.Schema.Table))
                _tableMetadata.Value.Schema = new SchemaAttribute(_tableMetadata.Value.Schema.Database, type.Name);
        }
        else
        {
            _tableMetadata.Value.Schema = new SchemaAttribute(null!, type.Name);
        }

        var properties = type.GetProperties().Where(x => x.CanRead && x.CanWrite).ToList();
        foreach (var property in properties)
        {
            var column = new Column
            {
                PropertyInfo = property,
                Name = property.Name,
                Type = property.PropertyType.Name,
                Required = property.GetCustomAttributes(typeof(RequiredAttribute), false).Any()
            };
            var stringLength = (StringLengthAttribute?)property.GetCustomAttributes(typeof(StringLengthAttribute), false).FirstOrDefault();
            if (stringLength != null) column.Length = stringLength.MaximumLength;
            _tableMetadata.Value.Columns[property.Name] = column;
        }

        if ((_tableMetadata.Value.Primary == null || _tableMetadata.Value.Primary.Count == 0))
        {
            var primary = properties.FirstOrDefault(x => x.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
            if (primary != null) _tableMetadata.Value.Primary = [primary.Name];
        }

        _tableMetadata.Value.TypeName = type.FullName ?? type.Name;

        if (_tableMetadata.Value.Primary != null && _tableMetadata.Value.Primary.Count > 0 && !_tableMetadata.Value.HasUpdateColumns)
        {
            var columns = _tableMetadata.Value.Columns.Select(x => x.Key).ToList();
            foreach (var primary in _tableMetadata.Value.Primary) columns.Remove(primary);
            _tableMetadata.Value.Updates = [..columns];
        }

        return _tableMetadata.Value;
    }

    protected virtual void Configure() { }

    protected T HasKey(Expression<Func<T, object>> expression)
    {
        var columns = GetColumns(expression);
        if (columns.Count == 0) throw new SpiderException("主键不能为空");
        _tableMetadata.Value.Primary = [..columns];
        return (T)(object)this;
    }

    protected T HasIndex(Expression<Func<T, object>> expression, bool isUnique = false)
    {
        var columns = GetColumns(expression);
        if (columns.Count == 0) throw new SpiderException("索引列不能为空");
        _tableMetadata.Value.Indexes.Add(new IndexMetadata(columns.ToArray(), isUnique));
        return (T)(object)this;
    }

    protected T ConfigureUpdateColumns(Expression<Func<T, object>> expression)
    {
        var columns = GetColumns(expression);
        _tableMetadata.Value.Updates = columns;
        return (T)(object)this;
    }

    private HashSet<string> GetColumns(Expression<Func<T, object>> expression)
    {
        var columns = new HashSet<string>();
        switch (expression.Body.NodeType)
        {
            case ExpressionType.New:
                var body = (NewExpression)expression.Body;
                foreach (var argument in body.Arguments) columns.Add(((MemberExpression)argument).Member.Name);
                break;
            case ExpressionType.MemberAccess: columns.Add(((MemberExpression)expression.Body).Member.Name); break;
            case ExpressionType.Convert: columns.Add(((MemberExpression)((UnaryExpression)expression.Body).Operand).Member.Name); break;
            default: throw new SpiderException("表达式不正确");
        }
        return columns;
    }
}
