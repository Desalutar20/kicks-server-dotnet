namespace Presentation.Shared.Dto;

public sealed record ApiResponse<T>(T Data)
    where T : notnull;

public sealed record ApiCursorResponse<T>(List<T> Data, string? PrevCursor, string? NextCursor)
    where T : notnull;
