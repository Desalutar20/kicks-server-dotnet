namespace Application.Abstractions.Email;

public record struct Message(
    NonEmptyString Subject,
    Domain.Users.Email To,
    NonEmptyString PlainText,
    NonEmptyString? HtmlText
);
