using System.Text.Json.Serialization;

namespace Domain.User;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    Male,
    Female,
    Other,
}
