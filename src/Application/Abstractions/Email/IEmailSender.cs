namespace Application.Abstractions.Email;

public interface IEmailSender
{
    Task<Result> SendAsync(Message message, CancellationToken ct = default);
}
