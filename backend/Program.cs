using Backend.Dto;
using Backend.Services;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<GoogleSheetsService>();

WebApplication? app = builder.Build();


app.MapPost("/api/expenses", async (GoogleSheetsService service, CreateExpenseRequest request) =>
{
    try
    {
        bool success = await service.AppendExpenseAsync
        (
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
        Console.WriteLine(ex);
        return Results.InternalServerError();
    }
});

app.MapPost("/api/templates", async (GoogleSheetsService service, CreateTemplateRequest request) => {
    try
    {
        bool success = await service.AppendTemplateAsync
        (
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
        Console.WriteLine(ex);
        return Results.InternalServerError();
    }
});


app.Run();

