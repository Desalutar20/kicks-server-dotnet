using Domain.Shared.ValueObjects;

namespace Application.Abstractions.Email;

public record struct Message(
    NonEmptyString Subject,
    Domain.Shared.ValueObjects.Email To,
    NonEmptyString PlainText,
    NonEmptyString? HtmlText
);
