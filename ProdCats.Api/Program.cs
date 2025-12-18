using Microsoft.OpenApi.Models;
using ProdCats.Api.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProdCats API",
        Version = "v1",
        Description = "API for managing products and categories"
    });
});

// Configure application services, database, and CORS
builder.Services.AddApplicationServices();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration, builder.Environment);

var app = builder.Build();

// Seed database
await app.SeedDatabaseAsync();

// Configure the HTTP request pipeline
app.ConfigurePipeline(builder.Environment);

app.Run();
