using Domain.Abstractions;

namespace Domain.Shared.FileContent;

public sealed record FileName
{
    public const int MaxLength = 100;

    private FileName(string value)
    {
        Value = value;
    }

    public string Value { get; }

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

        return errors.Count == 0 ? new FileName(value) : Error.Validation("fileName", errors);
    }

    public override string ToString() => Value;

    public static implicit operator string(FileName FileName) => FileName.Value;

    public static implicit operator FileName(string value) => Create(value).Value;
};
