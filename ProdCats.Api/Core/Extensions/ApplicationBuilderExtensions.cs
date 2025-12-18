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
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProdCats API v1");
                c.RoutePrefix = "swagger";
            });
        }
        else
        {
            app.UseHttpsRedirection();
        }

        // CORS middleware
        app.UseCors("AngularApp");

        // Global exception handling middleware (must be early in pipeline)
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.MapControllers();

        return app;
    }
}
