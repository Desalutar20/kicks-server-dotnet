namespace Infrastructure.Services;

internal sealed class EmailService(
    Config config) : IEmailSender, IAsyncDisposable
{
    private static readonly TimeSpan SmtpClientTimeout = TimeSpan.FromSeconds(10);

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly SmtpClient _smtpClient = new();
    private DateTimeOffset _disconnectAfter = DateTimeOffset.MinValue;

    public async ValueTask DisposeAsync()
    {
        if (_smtpClient.IsConnected) await _smtpClient.DisconnectAsync(true);

        _semaphore.Dispose();
        _smtpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<Result> SendAsync(Message message, CancellationToken ct = default)
    {
        var mimeMessageResult = BuildMimeMessage(message);
        if (mimeMessageResult.IsFailure) return Result.Failure(mimeMessageResult.Error);

        await _semaphore.WaitAsync(ct);

        try
        {
            await ConnectAsync(ct);
            await _smtpClient.SendAsync(mimeMessageResult.Value, ct);

            return Result.Success();
        }
        finally
        {
            _semaphore.Release();
        }
    }


    private Result<MimeMessage> BuildMimeMessage(Message message)
    {
        if (!MailboxAddress.TryParse(message.To.Value, out var to))
            return Result<MimeMessage>.Failure(Error.Failure($"invalid address {message.To.Value}"));

        var msg = new MimeMessage();

        msg.Subject = message.Subject.Value;
        msg.From.Add(MailboxAddress.Parse(config.Smtp.From));
        msg.To.Add(to);

        var builder = new BodyBuilder
        {
            TextBody = message.PlainText.Value
        };

        if (message.HtmlText is not null) builder.HtmlBody = message.HtmlText.Value.Value;

        msg.Body = builder.ToMessageBody();

        return Result<MimeMessage>.Success(msg);
    }

    private async Task ConnectAsync(CancellationToken ct)
    {
        if (_smtpClient.IsConnected)
        {
            await _smtpClient.NoOpAsync(ct);
        }
        else
        {
            await _smtpClient.ConnectAsync(config.Smtp.Host, config.Smtp.Port, cancellationToken: ct);
            await _smtpClient.AuthenticateAsync(new NetworkCredential(config.Smtp.User, config.Smtp.Password), ct);
        }

        ScheduleDisconnect(ct);
    }

    private void ScheduleDisconnect(CancellationToken ct)
    {
        _disconnectAfter = DateTimeOffset.UtcNow.Add(SmtpClientTimeout);
        Task.Run(async () =>
        {
            await Task.Delay(SmtpClientTimeout.Add(TimeSpan.FromSeconds(1)), ct);
            if (DateTimeOffset.UtcNow > _disconnectAfter) await DisconnectAsync(ct);
        });
    }

    private async Task DisconnectAsync(CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);

        try
        {
            if (!_smtpClient.IsConnected) return;

            await _smtpClient.DisconnectAsync(
                true, ct);
            _disconnectAfter = DateTimeOffset.MinValue;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}