using System.Data;
using System.Text.Json;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace Infrastructure.Data.TypeHandlers;

internal sealed class JsonBTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    public override T? Parse(object value)
    {
        if (value == null || value == DBNull.Value)
        {
            return default;
        }

        if (value is string s)
            return JsonSerializer.Deserialize<T>(s)!;

        if (value is NpgsqlRange<T>)
            return JsonSerializer.Deserialize<T>(value.ToString()!)!;

        return JsonSerializer.Deserialize<T>(value.ToString()!);
    }

    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.Value = JsonSerializer.Serialize(value);

        if (parameter is NpgsqlParameter npgsqlParameter)
        {
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
        }
    }
}
