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
app.MapControllers();

app.Run();