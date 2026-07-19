using api.Services;
using Microsoft.AspNetCore.Mvc;
using api.Dto;

namespace api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly GoogleSheetsService _googleSheetsService;

    public ExpensesController(GoogleSheetsService googleSheetsService)
    {
        _googleSheetsService = googleSheetsService;
    }

    [HttpGet("today/total")]
    public async Task<IActionResult> GetTodayTotal(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet
    )
    {
        try
        {
            decimal? todayTotal = await _googleSheetsService.GetTodayTotal(spreadsheetId, sheet);

            if (todayTotal == null)
            {
                return NotFound("Today's total could not be fetched");
            }

            return Ok(new { total = todayTotal});
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint GET /api/expenses/today/total: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
    }
    }

    [HttpPost]
    public async Task<IActionResult> CreateExpense(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromBody] CreateExpenseRequest request)
    {
        try
        {
            bool success = await _googleSheetsService.AppendExpenseAsync
            (
                spreadsheetId,
                sheet,
                request.Name,
                request.Group,
                request.Category,
                request.Tag,
                request.Amount,
                request.Description
            );

            if (success)
            {
                return Ok(new { message = "Successfully added new expense!" });
            }
            else
            {
                return Problem("Failed to add new expense.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint POST /api/expenses: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }
}