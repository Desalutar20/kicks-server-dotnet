using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Admin.DeliveryOptions.Types;
using Domain.DeliveryOptions;
using Domain.Shared.ValueObjects;

namespace Application.Admin.DeliveryOptions.JsonConverters;

public class DeliveryOptionResponseConverter : JsonConverter<DeliveryOptionResponse>
{
    public override DeliveryOptionResponse Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var root = JsonDocument.ParseValue(ref reader).RootElement;

        var id = new DeliveryOptionId(root.GetProperty("id").GetGuid());
        var createdAt = root.GetProperty("createdAt").GetDateTimeOffset();
        var updatedAt = root.GetProperty("updatedAt").GetDateTimeOffset();
        var title = DeliveryOptionTitle.Create(root.GetProperty("title").GetString()!).Value;
        var description = DeliveryOptionDescription
            .Create(root.GetProperty("description").GetString()!)
            .Value;
        var price = Money.FromDollars(root.GetProperty("price").GetDecimal()).Value;

        return new DeliveryOptionResponse(id, createdAt, updatedAt, title, description, price);
    }

    public override void Write(
        Utf8JsonWriter writer,
        DeliveryOptionResponse value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id.Value);
        writer.WriteString("createdAt", value.CreatedAt);
        writer.WriteString("updatedAt", value.UpdatedAt);
        writer.WriteString("title", value.Title.Value);
        writer.WriteString("description", value.Description.Value);
        writer.WriteNumber("price", value.Price.Dollars);

        writer.WriteEndObject();
    }
}
