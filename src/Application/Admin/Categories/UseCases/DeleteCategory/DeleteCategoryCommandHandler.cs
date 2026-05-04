using Application.Admin.Categories.Errors;
using Domain.Product.Category;

namespace Application.Admin.Categories.UseCases.DeleteCategory;

public sealed record DeleteCategoryCommand(CategoryId CategoryId) : ICommand;

internal sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken ct = default)
    {
        var category = await categoryRepository.GetCategoryByIdAsync(command.CategoryId, true, ct);
        if (category is null)
        {
            return AdminCategoriesErrors.CategoryNotFound(command.CategoryId);
        }

        categoryRepository.DeleteCategory(category);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
