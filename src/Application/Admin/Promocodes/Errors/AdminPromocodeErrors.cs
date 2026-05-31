using Domain.Promocodes;

namespace Application.Admin.Promocodes.Errors;

public static class AdminPromocodeErrors
{
    public static Error PromocodeAlreadyExists(PromocodeCode code) =>
        Error.Failure($"Promocode with code '{code.Value}' already exists");

    public static Error PromocodeNotFound(PromocodeId promocodeId) =>
        Error.Failure($"Promocode with id '{promocodeId}' doesn't exist");
}