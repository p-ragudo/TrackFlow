using api.Services;
using Microsoft.AspNetCore.Mvc;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class IdentifiersController : ControllerBase
{
    private readonly GoogleSheetsService _googleSheetsService;

    public IdentifiersController(GoogleSheetsService googleSheetsService)
    {
        _googleSheetsService = googleSheetsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetIdentifiers(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet
    )
    {
        try
        {
            Identifiers? identifiers = await _googleSheetsService.GetIdentifiersAsync(spreadsheetId, sheet);

            if (identifiers == null)
            {
                return NotFound("Identifiers could not be fetched");
            }

            return Ok(new { identifiers });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint GET /api/v1/identifiers: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }
}