using FastEndpoints;
using FastEndpoints.OpenApi;
using Omne_Crud_Demo.IoC;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.RegisterServices();
builder.Services.RegisterRepositories();
builder.Services.AddPersistence(builder.Configuration);
builder.Services
    .AddFastEndpoints()
    .OpenApiDocument(options =>
    {
        options.DocumentName = "v1";
        options.Title = "Omne CRUD API";
        options.Version = "v1";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Omne CRUD API";
    });
}

app.MapDefaultEndpoints();

app.UseFileServer();
app.Run();

public partial class Program { }
