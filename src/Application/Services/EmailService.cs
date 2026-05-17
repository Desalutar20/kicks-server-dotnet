using System.Text.Json;
using Application.Abstractions.Email;
using Application.Abstractions.Email.JsonConverters;
using Application.Config;

namespace Application.Services;

internal static class EmailService
{
    private static readonly NonEmptyString AccountVerificationSubject = NonEmptyString
        .Create("Account verification")
        .Value;

    private static readonly NonEmptyString PasswordChangedSubject = NonEmptyString
        .Create("Your password has been changed")
        .Value;

    private static readonly NonEmptyString ResetPasswordSubject = NonEmptyString
        .Create("Reset your password")
        .Value;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new MessageConverter() },
    };

    public static Message BuildAccountVerificationEmail(
        ApplicationConfig config,
        Email to,
        NonEmptyString token
    )
    {
        var url = new UriBuilder(config.ClientUrl)
        {
            Path = config.AccountVerificationPath,
            Query =
                $"token={Uri.EscapeDataString(token.Value)}&email={Uri.EscapeDataString(to.Value)}",
        }.Uri;

        var text = NonEmptyString
            .Create(
                $"""
                Hello,

                Please verify your account by clicking the link below:

                {url}

                Thank you
                """
            )
            .Value;

        var html = NonEmptyString
            .Create(
                $"""
                <html>
                <body style="font-family: Arial, sans-serif; color: #333;">
                    <h2 style="color: #2E86C1;">Hello!</h2>
                    <p>Please verify your account by clicking the button below:</p>
                    <a href="{url}"
                        target="_blank"
                       style="
                         display: inline-block;
                         padding: 10px 20px;
                         background-color: #2E86C1;
                         color: white;
                         text-decoration: none;
                         border-radius: 5px;
                         font-weight: bold;
                       ">
                       Verify Account
                    </a>
                    <p style="margin-top: 20px;">If the button doesn't work, copy and paste this link into your browser:</p>
                    <p>{url}</p>
                    <p>Thank you</p>
                </body>
                </html>
                """
            )
            .Value;

        return new Message(AccountVerificationSubject, to, text, html);
    }

    public static Message BuildResetPasswordEmail(
        ApplicationConfig config,
        Email to,
        NonEmptyString token
    )
    {
        var resetUrl = new UriBuilder(config.ClientUrl)
        {
            Path = config.ResetPasswordPath,
            Query =
                $"token={Uri.EscapeDataString(token.Value)}&email={Uri.EscapeDataString(to.Value)}",
        }.Uri;

        var text = NonEmptyString
            .Create(
                $"""
                Hello,

                You requested a password reset.

                Open the link below to set a new password:

                {resetUrl}

                If you did not request this, you can safely ignore this email.

                Thanks
                """
            )
            .Value;

        var html = NonEmptyString
            .Create(
                $"""
                <html>
                <body style="font-family: Arial, sans-serif; color: #333;">
                    <h2>Password reset</h2>

                    <p>You requested a password reset.</p>

                    <a href="{resetUrl}"
                       target="_blank"
                       style="
                         display:inline-block;
                         padding:12px 24px;
                         background-color:#2E86C1;
                         color:#ffffff;
                         text-decoration:none;
                         border-radius:6px;
                         font-weight:bold;
                       ">
                        Reset password
                    </a>

                    <p style="margin-top:20px;">
                        If the button doesn’t work, copy and paste this link into your browser:
                    </p>

                    <p>{resetUrl}</p>

                    <p>
                        If you did not request a password reset, just ignore this email.
                    </p>

                    <p>
                        Thanks
                    </p>
                </body>
                </html>
                """
            )
            .Value;

        return new Message(ResetPasswordSubject, to, text, html);
    }

    public static Message BuildPasswordChangedEmail(Email to)
    {
        var text = NonEmptyString
            .Create(
                """
                Hello,

                Your password has been successfully changed.

                If you did not perform this action, please contact support immediately.

                Thanks
                """
            )
            .Value;

        var html = NonEmptyString
            .Create(
                """
                <html>
                <body style="font-family: Arial, sans-serif; color: #333;">
                    <h2>Password changed</h2>
                    <p>Your password has been successfully changed.</p>
                    <p>If you did not perform this action, contact support immediately.</p>
                    <p>Thanks</p>
                </body>
                </html>
                """
            )
            .Value;

        return new Message(PasswordChangedSubject, to, text, html);
    }

    public static NonEmptyString SerializeMessage(Message message) =>
        NonEmptyString.Create(JsonSerializer.Serialize(message, Options)).Value;
}
