using System.Data;
using Dapper;

namespace Infrastructure.Data.TypeHandlers;

internal sealed class StringListTypeHandler<T> : SqlMapper.TypeHandler<List<string>>
{
    public override List<string> Parse(object value)
    {
        return ((string[])value).ToList();
    }

    public override void SetValue(IDbDataParameter parameter, List<string>? value)
    {
        parameter.Value = value?.ToArray();
    }
}
