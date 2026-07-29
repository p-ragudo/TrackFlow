using api.Services;
using Microsoft.AspNetCore.Mvc;
using api.Dto;

namespace api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BootstrapController : ControllerBase
{
    private readonly GoogleSheetsExpensesService _googleSheetsService;

    public BootstrapController(GoogleSheetsExpensesService googleSheetsService)
    {
        _googleSheetsService = googleSheetsService;
    }

    [HttpPost]
    public async Task<IActionResult> GetBootstrap(
        [FromQuery] string spreadsheetId,
        [FromBody] BootstrapRequest request
    )
    {
        if (string.IsNullOrWhiteSpace(spreadsheetId))
            return BadRequest(new { message = "spreadsheetid query parameter is required." });

        var result = await _googleSheetsService.GetBootstrapDataAsync(
            spreadsheetId,
            request.TemplateSheet,
            request.ExpensesSheet,
            request.TodayExpensesSheet,
            request.IdentifiersSheet,
            request.NameSheet
        );

        if (result == null)
            return StatusCode(500, new { message = "Failed to load bootstrap data from Google Sheets." });

        return Ok(result);
    }
}