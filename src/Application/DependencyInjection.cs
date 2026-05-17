using Application.Abstractions.OAuth;
using Application.Admin.Brands.UseCases.CreateBrand;
using Application.Admin.Brands.UseCases.DeleteBrand;
using Application.Admin.Brands.UseCases.GetBrands;
using Application.Admin.Brands.UseCases.UpdateBrand;
using Application.Admin.Categories.UseCases.CreateCategory;
using Application.Admin.Categories.UseCases.DeleteCategory;
using Application.Admin.Categories.UseCases.GetCategories;
using Application.Admin.Categories.UseCases.UpdateCategory;
using Application.Admin.Products.ProductSkus.UseCases.CreateProductSku;
using Application.Admin.Products.ProductSkus.UseCases.DeleteProductSku;
using Application.Admin.Products.ProductSkus.UseCases.DeleteProductSkuImage;
using Application.Admin.Products.ProductSkus.UseCases.GetProductSku;
using Application.Admin.Products.ProductSkus.UseCases.GetProductSkus;
using Application.Admin.Products.ProductSkus.UseCases.UpdateProductSku;
using Application.Admin.Products.UseCases.CreateProduct;
using Application.Admin.Products.UseCases.DeleteProduct;
using Application.Admin.Products.UseCases.GetProductFilters;
using Application.Admin.Products.UseCases.GetProducts;
using Application.Admin.Products.UseCases.UpdateProduct;
using Application.Admin.Users.Types;
using Application.Admin.Users.UseCases.DeleteUser;
using Application.Admin.Users.UseCases.GetAllAdminUsers;
using Application.Admin.Users.UseCases.ToggleBanUser;
using Application.Auth.Types;
using Application.Auth.UseCases.Authenticate;
using Application.Auth.UseCases.DeleteExpiredSessions;
using Application.Auth.UseCases.ForgotPassword;
using Application.Auth.UseCases.GenerateOAuthUrl;
using Application.Auth.UseCases.Logout;
using Application.Auth.UseCases.OAuthSignIn;
using Application.Auth.UseCases.ResetPassword;
using Application.Auth.UseCases.SignIn;
using Application.Auth.UseCases.SignUp;
using Application.Auth.UseCases.VerifyAccount;
using Domain.Product.ProductSku;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<SignUpCommand>, SignUpCommandHandler>();
        services.AddScoped<
            ICommandHandler<SignInCommand, UserWithSessionId>,
            SignInCommandHandler
        >();
        services.AddScoped<ICommandHandler<VerifyAccountCommand>, VerifyAccountCommandHandler>();

        services.AddScoped<ICommandHandler<ForgotPasswordCommand>, ForgotPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<ResetPasswordCommand>, ResetPasswordHandler>();

        services.AddScoped<
            ICommandHandler<AuthenticateCommand, SessionUser>,
            AuthenticateCommandHandler
        >();
        services.AddScoped<ICommandHandler<LogoutCommand>, LogoutCommandHandler>();
        services.AddScoped<
            ICommandHandler<DeleteExpiredSessionCommand>,
            DeleteExpiredSessionCommandHandler
        >();

        services.AddScoped<
            ICommandHandler<GenerateOAuthUrlCommand, (Uri, OAuthState)>,
            GenerateOAuthUrlCommandHandler
        >();
        services.AddScoped<ICommandHandler<OAuthSignInCommand, Guid>, OAuthSignInCommandHandler>();

        services.AddScoped<
            IQueryHandler<GetAllAdminUsersQuery, KeysetPaginated<AdminUser, UserId>>,
            GetAllAdminUsersQueryHandler
        >();
        services.AddScoped<ICommandHandler<ToggleBanUserCommand>, ToggleBanUserCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteUserCommand>, DeleteUserCommandHandler>();

        services.AddScoped<
            IQueryHandler<GetBrandsQuery, KeysetPaginated<Brand, BrandId>>,
            GetBrandsQueryHandler
        >();
        services.AddScoped<ICommandHandler<CreateBrandCommand, Brand>, CreateBrandCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateBrandCommand>, UpdateBrandCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteBrandCommand>, DeleteBrandCommandHandler>();

        services.AddScoped<
            IQueryHandler<GetCategoriesQuery, KeysetPaginated<Category, CategoryId>>,
            GetCategoriesQueryHandler
        >();
        services.AddScoped<
            ICommandHandler<CreateCategoryCommand, Category>,
            CreateCategoryCommandHandler
        >();
        services.AddScoped<ICommandHandler<UpdateCategoryCommand>, UpdateCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteCategoryCommand>, DeleteCategoryCommandHandler>();

        services.AddScoped<
            IQueryHandler<GetProductFiltersQuery, ProductFilterOptions>,
            GetProductFiltersQueryHandler
        >();
        services.AddScoped<
            IQueryHandler<GetProductsQuery, KeysetPaginated<Product, ProductId>>,
            GetProductsQueryHandler
        >();
        services.AddScoped<
            ICommandHandler<CreateProductCommand, Product>,
            CreateProductCommandHandler
        >();
        services.AddScoped<
            ICommandHandler<UpdateProductCommand, Product>,
            UpdateProductCommandHandler
        >();
        services.AddScoped<
            ICommandHandler<ToggleProductIsDeletedCommand>,
            ToggleProductIsDeletedCommandHandler
        >();

        services.AddScoped<
            IQueryHandler<GetProductSkusQuery, KeysetPaginated<ProductSku, ProductSkuId>>,
            GetProductSkusQueryHandler
        >();
        services.AddScoped<
            IQueryHandler<GetProductSkuQuery, ProductSku>,
            GetProductSkuQueryHandler
        >();
        services.AddScoped<
            ICommandHandler<CreateProductSkuCommand, ProductSkuId>,
            CreateProductSkuCommandHandler
        >();
        services.AddScoped<
            ICommandHandler<UpdateProductSkuCommand, ProductSku>,
            UpdateProductSkuCommandHandler
        >();

        services.AddScoped<
            ICommandHandler<DeleteProductSkuCommand>,
            DeleteProductSkuCommandHandler
        >();
        services.AddScoped<
            ICommandHandler<DeleteProductSkuImageCommand>,
            DeleteProductSkuImageCommandHandler
        >();

        return services;
    }
}
