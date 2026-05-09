using Domain.Abstractions;

namespace Domain.Product.Exceptions;

public class ProductAlreadyExistsException(Exception innerException)
    : AppException(
        "Product with same title, gender, category and brand already exists ",
        innerException
    );
