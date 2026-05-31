namespace Presentation.Admin.Promocodes.Dto;

public sealed record PromocodeValidityPeriodDto(DateTimeOffset ValidFrom, DateTimeOffset ValidTo);
