using Api.Extensions;
using Application;
using Infrastructure;
using Presentation;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

builder.Host.UseDefaultServiceProvider(config =>
{
    config.ValidateOnBuild = true;
    config.ValidateScopes = true;
});

builder.WebHost.UseKestrel(options => options.AddServerHeader = false);

builder.Services.ConfigureServices(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration).AddApplication().AddPresentation();

var app = builder.Build();

app.UseHttpLogging();
app.UseExceptionHandler();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    await app.ApplyMigrations();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRateLimiter();
app.AddEndpoints();

app.Run();
