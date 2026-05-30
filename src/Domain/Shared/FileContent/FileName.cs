using Domain.Abstractions;

namespace Domain.Shared.FileContent;

public sealed record FileName
{
    public const int MaxLength = 100;

    private FileName(string name, string extension)
    {
        Name = name;
        Extension = extension;
    }

    public string Name { get; }
    public string Extension { get; }

    public string FullName => $"{Name}{Extension}";

    public static Result<FileName> Create(string value)
    {
        var errors = new List<string>();

        var emptyResult = Guard.AgainstEmptyString(value, "File name");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = value.Trim();

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "File name");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        var name = Path.GetFileNameWithoutExtension(value);
        var extension = Path.GetExtension(value);

        var nameResult = Guard.AgainstEmptyString(extension, "File name");
        if (nameResult.IsFailure)
        {
            errors.Add(nameResult.Error.Description);
        }

        var extensionResult = Guard.AgainstEmptyString(extension, "File extension");
        if (extensionResult.IsFailure)
        {
            errors.Add(extensionResult.Error.Description);
        }

        return errors.Count == 0
            ? new FileName(name, extension)
            : Error.Validation("fileName", errors);
    }

    public override string ToString() => FullName;

    public static implicit operator string(FileName fileName) => fileName.FullName;

    public static implicit operator FileName(string value) => Create(value).Value;
};
