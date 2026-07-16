using backend.Dto;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<GoogleSheetsService>();

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

app.MapPost("/api/expenses", async (GoogleSheetsService service, string spreadsheetId, string sheet, [FromBody] CreateExpenseRequest request) =>
{
    try
    {
        bool success = await service.AppendExpenseAsync
        (
            spreadsheetId,
            sheet,
            request.Name,
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

app.MapGet("/api/expenses/today", async (GoogleSheetsService service, string spreadsheetId, string sheet) =>
{
    try
    {
        decimal? todayTotal = await service.GetTodayTotal(spreadsheetId, sheet);

        if (todayTotal == null)
        {
            return Results.NotFound("Today's total could not be fetched");
        }

        return Results.Ok(todayTotal);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error at endpoint GET /api/expenses/today: {ex.Message}");
        return Results.InternalServerError();
    }
});

app.MapPost("/api/templates", async (GoogleSheetsService service, string spreadsheetId, string sheet, CreateTemplateRequest request) =>
{
    try
    {
        bool success = await service.AppendTemplateAsync
        (
            spreadsheetId,
            sheet,
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

app.MapGet("/api/templates", async (GoogleSheetsService service, string spreadsheetId, string sheet) =>
{
    try
    {
        var templates = await service.GetTemplatesAsync(spreadsheetId, sheet);

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

app.MapPut("/api/templates", async (GoogleSheetsService service, string spreadsheetId, string sheet, int id, [FromBody] PutTemplate request) =>
{
    try
    {
        var success = await service.UpdateTemplateAsync(
            spreadsheetId,
            sheet,
            id,
            new Template
            {
                Id = id,
                Name = request.Name,
                Category = request.Category,
                Tag = request.Tag,
                Amount = request.Amount,
                Description = request.Description
            }
        );

        if (success)
        {
            return Results.Ok($"Successfully updated template with ID {id}");
        }
        else
        {
            return Results.Problem($"Failed to update template with ID {id}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error at endpoint PUT /api/templates: {ex.Message}");
        return Results.InternalServerError();
    }
});

app.MapDelete("/api/templates", async (GoogleSheetsService service, string spreadsheetId, string sheet, int id) =>
{
    try
    {
        var success = await service.DeleteTemplateByIdAsync(spreadsheetId, sheet, id);

        if (success)
        {
            return Results.Ok($"Successfully deleted template with ID {id}");
        }
        else
        {
            return Results.Problem($"Failed to delete template with ID {id}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error at endpoint DELETE /api/templates: {ex.Message}");
        return Results.InternalServerError();
    }
});


app.Run();

