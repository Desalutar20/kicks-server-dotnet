using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Auth.Types;

namespace Application.Auth.JsonConverters;

public class SessionUserConverter : JsonConverter<SessionUser>
{
    public override SessionUser? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var root = JsonDocument.ParseValue(ref reader).RootElement;

        var id = new UserId(root.GetProperty("id").GetGuid());
        var email = Email.Create(root.GetProperty("email").GetString()!).Value;

        FirstName? firstName = root.TryGetProperty("firstName", out var fn)
            ? FirstName.Create(fn.GetString()!).Value
            : null;

        LastName? lastName = root.TryGetProperty("lastName", out var ln)
            ? LastName.Create(ln.GetString()!).Value
            : null;

        var role = Enum.Parse<Role>(root.GetProperty("role").GetString()!);

        Gender? gender = root.TryGetProperty("gender", out var g)
            ? Enum.Parse<Gender>(g.GetString()!)
            : null;

        return new SessionUser(id, email, firstName, lastName, role, gender);
    }

    public override void Write(Utf8JsonWriter writer, SessionUser value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id.Value);
        writer.WriteString("email", value.Email.Value);
        writer.WriteString("role", value.Role.ToString());


        if (value.FirstName is not null) writer.WriteString("firstName", value.FirstName.Value.Value);
        if (value.LastName is not null) writer.WriteString("lastName", value.LastName.Value.Value);
        if (value.Gender is not null) writer.WriteString("gender", value.Gender.ToString());


        writer.WriteEndObject();
    }
}