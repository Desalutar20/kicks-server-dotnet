namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssemblies(typeof(DependencyInjection).Assembly)
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        // services.AddScoped<ICommandHandler<SignUpCommand>, SignUpCommandHandler>();
        // services.AddScoped<
        //     ICommandHandler<SignInCommand, UserWithSessionId>,
        //     SignInCommandHandler
        // >();
        // services.AddScoped<ICommandHandler<VerifyAccountCommand>, VerifyAccountCommandHandler>();

        // services.AddScoped<ICommandHandler<ForgotPasswordCommand>, ForgotPasswordCommandHandler>();
        // services.AddScoped<ICommandHandler<ResetPasswordCommand>, ResetPasswordHandler>();

        // services.AddScoped<
        //     ICommandHandler<AuthenticateCommand, SessionUser>,
        //     AuthenticateCommandHandler
        // >();
        // services.AddScoped<ICommandHandler<LogoutCommand>, LogoutCommandHandler>();
        // services.AddScoped<
        //     ICommandHandler<DeleteExpiredSessionCommand>,
        //     DeleteExpiredSessionCommandHandler
        // >();

        // services.AddScoped<
        //     ICommandHandler<GenerateOAuthUrlCommand, (Uri, OAuthState)>,
        //     GenerateOAuthUrlCommandHandler
        // >();
        // services.AddScoped<ICommandHandler<OAuthSignInCommand, Guid>, OAuthSignInCommandHandler>();

        // services.AddScoped<
        //     IQueryHandler<GetAllAdminUsersQuery, KeysetPaginated<AdminUser, UserId>>,
        //     GetAllAdminUsersQueryHandler
        // >();
        // services.AddScoped<ICommandHandler<ToggleBanUserCommand>, ToggleBanUserCommandHandler>();
        // services.AddScoped<ICommandHandler<DeleteUserCommand>, DeleteUserCommandHandler>();

        // services.AddScoped<
        //     IQueryHandler<GetBrandsQuery, KeysetPaginated<Brand, BrandId>>,
        //     GetBrandsQueryHandler
        // >();
        // services.AddScoped<ICommandHandler<CreateBrandCommand, Brand>, CreateBrandCommandHandler>();
        // services.AddScoped<ICommandHandler<UpdateBrandCommand>, UpdateBrandCommandHandler>();
        // services.AddScoped<ICommandHandler<DeleteBrandCommand>, DeleteBrandCommandHandler>();

        // services.AddScoped<
        //     IQueryHandler<GetCategoriesQuery, KeysetPaginated<Category, CategoryId>>,
        //     GetCategoriesQueryHandler
        // >();
        // services.AddScoped<
        //     ICommandHandler<CreateCategoryCommand, Category>,
        //     CreateCategoryCommandHandler
        // >();
        // services.AddScoped<ICommandHandler<UpdateCategoryCommand>, UpdateCategoryCommandHandler>();
        // services.AddScoped<ICommandHandler<DeleteCategoryCommand>, DeleteCategoryCommandHandler>();

        // services.AddScoped<
        //     IQueryHandler<GetProductFiltersQuery, ProductFilterOptions>,
        //     GetProductFiltersQueryHandler
        // >();
        // services.AddScoped<
        //     IQueryHandler<GetProductsQuery, KeysetPaginated<Product, ProductId>>,
        //     GetProductsQueryHandler
        // >();
        // services.AddScoped<
        //     ICommandHandler<CreateProductCommand, Product>,
        //     CreateProductCommandHandler
        // >();
        // services.AddScoped<
        //     ICommandHandler<UpdateProductCommand, Product>,
        //     UpdateProductCommandHandler
        // >();
        // services.AddScoped<
        //     ICommandHandler<ToggleProductIsDeletedCommand>,
        //     ToggleProductIsDeletedCommandHandler
        // >();

        // services.AddScoped<
        //     IQueryHandler<GetAdminProductSkusQuery, KeysetPaginated<ProductSku, ProductSkuId>>,
        //     GetAdminProductSkusQueryHandler
        // >();
        // services.AddScoped<
        //     IQueryHandler<GetAdminProductSkuQuery, ProductSku>,
        //     GetAdminProductSkuQueryHandler
        // >();
        // services.AddScoped<
        //     ICommandHandler<CreateProductSkuCommand, ProductSkuId>,
        //     CreateProductSkuCommandHandler
        // >();
        // services.AddScoped<
        //     ICommandHandler<UpdateProductSkuCommand, ProductSku>,
        //     UpdateProductSkuCommandHandler
        // >();

        // services.AddScoped<
        //     ICommandHandler<DeleteProductSkuCommand>,
        //     DeleteProductSkuCommandHandler
        // >();
        // services.AddScoped<
        //     ICommandHandler<DeleteProductSkuImageCommand>,
        //     DeleteProductSkuImageCommandHandler
        // >();

        // services.AddScoped<
        //     IQueryHandler<GetProductSkusFiltersQuery, ProductSkusFilterOptions>,
        //     GetProductSkusFiltersQueryHandler
        // >();

        // services.AddScoped<
        //     IQueryHandler<GetProductSkusQuery, KeysetPaginated<ProductSku, ProductSkuId>>,
        //     GetProductSkusQueryHandler
        // >();
        // services.AddScoped<
        //     IQueryHandler<GetProductSkuQuery, ProductSkuWithVariants>,
        //     GetProductSkuQueryHandler
        // >();

        return services;
    }
}
