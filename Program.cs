using TrackFlow.Services;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.AddScoped<GoogleSheetsService>();

WebApplication? app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/addSheetTest", async (GoogleSheetsService googleSheetsService) => {
    try
    {
        await googleSheetsService.AppendExpenseAsync("Testing", 500.00m, "This is a test");
        return Results.Ok("Successfully appended!");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        return Results.InternalServerError();
    }
});


app.Run();

