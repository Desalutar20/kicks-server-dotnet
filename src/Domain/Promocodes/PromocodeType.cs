using System.Text.Json.Serialization;

namespace Domain.Promocodes;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PromocodeType
{
    Fixed,
    Percent
}