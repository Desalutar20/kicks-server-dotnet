using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Product;
using Domain.Product.Brand;

namespace Infrastructure.Data.Product.JsonConverters;

public class BrandFilterItemConverter : JsonConverter<BrandFilterItem>
{
    public override BrandFilterItem Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var root = JsonDocument.ParseValue(ref reader).RootElement;

        var id = new BrandId(root.GetProperty("id").GetGuid());
        var name = BrandName.Create(root.GetProperty("name").GetString()!).Value;

        return new BrandFilterItem(id, name);
    }

    public override void Write(
        Utf8JsonWriter writer,
        BrandFilterItem value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id.Value);
        writer.WriteString("name", value.Name.Value);

        writer.WriteEndObject();
    }
}
