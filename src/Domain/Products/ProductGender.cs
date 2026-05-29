using System.Text.Json.Serialization;

namespace Domain.Products;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductGender
{
    Men,
    Women,
    Unisex,
}
