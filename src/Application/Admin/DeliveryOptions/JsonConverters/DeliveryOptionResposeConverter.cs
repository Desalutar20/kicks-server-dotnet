using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Admin.DeliveryOptions.Types;
using Domain.DeliveryOptions;

namespace Application.Admin.DeliveryOptions.JsonConverters;

public class DeliveryOptionResponseConverter : JsonConverter<AdminDeliveryOptionResponse>
{
    public override AdminDeliveryOptionResponse Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var root = JsonDocument.ParseValue(ref reader).RootElement;

        var id = new DeliveryOptionId(root.GetProperty("id").GetGuid());
        var createdAt = root.GetProperty("createdAt").GetDateTimeOffset();
        var updatedAt = root.GetProperty("updatedAt").GetDateTimeOffset();
        var title = root.GetProperty("title").GetString()!;
        var description = root.GetProperty("description").GetString()!;
        var price = root.GetProperty("price").GetDecimal();

        return new AdminDeliveryOptionResponse(id, createdAt, updatedAt, title, description, price);
    }

    public override void Write(
        Utf8JsonWriter writer,
        AdminDeliveryOptionResponse value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id);
        writer.WriteString("createdAt", value.CreatedAt);
        writer.WriteString("updatedAt", value.UpdatedAt);
        writer.WriteString("title", value.Title);
        writer.WriteString("description", value.Description);
        writer.WriteNumber("price", value.Price);

        writer.WriteEndObject();
    }
}
