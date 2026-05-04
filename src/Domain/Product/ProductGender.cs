using System.Text.Json.Serialization;

namespace Domain.Product;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductGender
{
    Men,
    Women,
    Unisex,
}
