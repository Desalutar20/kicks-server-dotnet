using Api.Extensions;
using Application;
using Infrastructure;
using Presentation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
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
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.AddEndpoints();

app.Run();
