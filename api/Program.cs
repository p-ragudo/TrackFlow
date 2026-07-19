using api.Services;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<GoogleSheetsService>();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

WebApplication? app = builder.Build();

app.UseCors("AllowAnyOrigin");

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/health"))
    {
        await next();
        return;
    }

    if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedKey) ||
        extractedKey != builder.Configuration["ApiKey"])
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next();
});

app.MapControllers();

app.Run();