namespace Presentation.Shared.Dto;

public sealed record ApiResponse<T>(T Data);

public sealed record ApiCursorResponse<T>(List<T> Data, string? PrevCursor, string? NextCursor);
