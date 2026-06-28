using Application.Abstractions.Database;
using Application.Admin.Categories.Errors;
using Application.Admin.Categories.Types;
using Domain.Categories.Exceptions;

namespace Application.Admin.Categories.UseCases.CreateCategory;

public sealed record CreateCategoryCommand(CategoryName Name) : ICommand<AdminCategoryResponse>;

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateCategoryCommand, AdminCategoryResponse>
{
    public async Task<Result<AdminCategoryResponse>> Handle(
        CreateCategoryCommand command,
        CancellationToken ct = default
    )
    {
        try
        {
            var newCategory = new Category(command.Name);
            categoryRepository.CreateCategory(newCategory);

            await unitOfWork.SaveChangesAsync(ct);

            return newCategory.ToResponse();
        }
        catch (CategoryAlreadyExistsException)
        {
            return AdminCategoryErrors.CategoryAlreadyExists(command.Name);
        }
    }
}
