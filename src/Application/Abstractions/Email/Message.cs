namespace Application.Abstractions.Email;

public record struct Message(
    NonEmptyString Subject,
    Domain.User.Email To,
    NonEmptyString PlainText,
    NonEmptyString? HtmlText);