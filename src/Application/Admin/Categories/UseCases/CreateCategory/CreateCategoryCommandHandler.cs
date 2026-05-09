using Application.Admin.Categories.Errors;
using Domain.Product.Category.Exceptions;

namespace Application.Admin.Categories.UseCases.CreateCategory;

public sealed record CreateCategoryCommand(CategoryName Name) : ICommand<Category>;

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateCategoryCommand, Category>
{
    public async Task<Result<Category>> Handle(
        CreateCategoryCommand command,
        CancellationToken ct = default
    )
    {
        try
        {
            var newCategory = Category.Create(command.Name);
            categoryRepository.CreateCategory(newCategory);

            await unitOfWork.SaveChangesAsync(ct);

            return Result<Category>.Success(newCategory);
        }
        catch (CategoryAlreadyExistsException)
        {
            return Result<Category>.Failure(
                AdminCategoryErrors.CategoryAlreadyExists(command.Name)
            );
        }
    }
}
