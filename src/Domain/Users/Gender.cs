using System.Text.Json.Serialization;

namespace Domain.Users;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    Male,
    Female,
    Other,
}
