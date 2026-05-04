using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Abstractions.Email.JsonConverters;

public sealed class MessageConverter : JsonConverter<Message>
{
    public override Message Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var root = JsonDocument.ParseValue(ref reader).RootElement;

        var subject = NonEmptyString.Create(root.GetProperty("subject").GetString()!).Value;
        var email = Domain.User.Email.Create(root.GetProperty("to").GetString()!).Value;
        var plainText = NonEmptyString.Create(root.GetProperty("plainText").GetString()!).Value;

        NonEmptyString? htmlText = root.TryGetProperty("htmlText", out var fn)
            ? NonEmptyString.Create(fn.GetString()!).Value
            : null;

        return new Message(subject, email, plainText, htmlText);
    }

    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("subject", value.Subject.Value);
        writer.WriteString("to", value.To.Value);
        writer.WriteString("plainText", value.PlainText.ToString());

        if (value.HtmlText is not null)
            writer.WriteString("htmlText", value.HtmlText.Value.Value);

        writer.WriteEndObject();
    }
}
