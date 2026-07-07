using backend.Dto;
using backend.Services;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<GoogleSheetsService>();

WebApplication? app = builder.Build();


app.MapPost("/api/expenses", async (GoogleSheetsService service, CreateExpenseRequest request) =>
{
    try
    {
        bool success = await service.AppendExpenseAsync
        (
            request.SpreadsheetId,
            request.Sheet,
            request.Category,
            request.Tag,
            request.Amount,
            request.Description
        );

        if (success)
        {
            return Results.Ok("Successfully added new expense!");
        }
        else
        {
            return Results.Problem("Failed to add new expense.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error at endpoint POST /api/expenses: {ex.Message}");
        return Results.InternalServerError();
    }
});

app.MapPost("/api/templates", async (GoogleSheetsService service, CreateTemplateRequest request) =>
{
    try
    {
        bool success = await service.AppendTemplateAsync
        (
            request.SpreadsheetId,
            request.Sheet,
            request.Name,
            request.Category,
            request.Tag,
            request.Amount,
            request.Description
        );

        if (success)
        {
            return Results.Ok("Successfully added new template!");
        }
        else
        {
            return Results.Problem("Failed to add new template.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error at endpoint POST /api/templates: {ex.Message}");
        return Results.InternalServerError();
    }
});

app.MapGet("/api/templates", async (GoogleSheetsService service, string spreadsheetId, string range) =>
{
    try
    {
        var templates = await service.GetTemplatesAsync(spreadsheetId, range);

        if (templates == null)
        {
            return Results.NotFound("Templates could not be found.");
        }

        return Results.Ok(templates);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error at endpoint GET /api/templates: {ex.Message}");
        return Results.InternalServerError();
    }
});


app.Run();

