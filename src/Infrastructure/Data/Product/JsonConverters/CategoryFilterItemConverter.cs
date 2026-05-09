using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Product;
using Domain.Product.Category;

namespace Infrastructure.Data.Product.JsonConverters;

public class CategoryFilterItemConverter : JsonConverter<CategoryFilterItem>
{
    public override CategoryFilterItem Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var root = JsonDocument.ParseValue(ref reader).RootElement;

        var id = new CategoryId(root.GetProperty("id").GetGuid());
        var name = CategoryName.Create(root.GetProperty("name").GetString()!).Value;

        return new CategoryFilterItem(id, name);
    }

    public override void Write(
        Utf8JsonWriter writer,
        CategoryFilterItem value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id.Value);
        writer.WriteString("name", value.Name.Value);

        writer.WriteEndObject();
    }
}
