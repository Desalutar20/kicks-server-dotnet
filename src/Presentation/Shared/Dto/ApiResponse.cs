namespace Presentation.Shared.Dto;

public sealed record ApiResponse<T>(T Data);