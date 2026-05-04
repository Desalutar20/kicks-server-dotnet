MIGRATION_NAME ?= Initial

ef-migration:
	dotnet ef migrations add $(MIGRATION_NAME) \
	--project src/Infrastructure/Infrastructure.csproj \
	--startup-project src/Api/Api.csproj \
	--context Infrastructure.Data.AppDbContext \
	--configuration Debug \
	--output-dir Data/Migrations

ef-update:
	dotnet ef database update \
	--project src/Infrastructure/Infrastructure.csproj \
	--startup-project src/Api/Api.csproj \
	--context Infrastructure.Data.AppDbContext \
	--configuration Debug
