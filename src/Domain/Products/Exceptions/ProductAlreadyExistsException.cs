using Domain.Abstractions;

namespace Domain.Products.Exceptions;

public class ProductAlreadyExistsException(Exception innerException)
    : AppException(
        "Product with same title, gender, category and brand already exists ",
        innerException
    );
