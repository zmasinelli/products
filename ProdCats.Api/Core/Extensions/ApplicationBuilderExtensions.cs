using ProdCats.Api.Middleware;

namespace ProdCats.Api.Core.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app, IWebHostEnvironment environment)
    {
        // Configure the HTTP request pipeline.
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        // CORS middleware (must be before UseAuthorization)
        app.UseCors("AngularApp");

        // Global exception handling middleware (must be early in pipeline)
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
