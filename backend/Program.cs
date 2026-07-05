using Backend.Dto;
using Backend.Services;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<GoogleSheetsService>();

WebApplication? app = builder.Build();


app.MapPost("/api/expenses", async (GoogleSheetsService service, CreateExpenseRequest request) => {
    try
    {
        await service.AppendExpenseAsync
        (
            request.Sheet,
            request.Category,
            request.Tag,
            request.Amount,
            request.Description
        );

        return Results.Ok("Successfully appended!");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        return Results.InternalServerError();
    }
});


app.Run();

