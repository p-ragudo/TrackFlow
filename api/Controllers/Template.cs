using api.Services;
using Microsoft.AspNetCore.Mvc;
using api.Dto;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly GoogleSheetsService _googleSheetsService;

    public TemplatesController(GoogleSheetsService googleSheetsService)
    {
        _googleSheetsService = googleSheetsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet
    )
    {
        try
        {
            var templates = await _googleSheetsService.GetTemplatesAsync(spreadsheetId, sheet);

            if (templates == null)
            {
                return NotFound("Templates could not be found.");
            }

            return Ok(new { templates });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint GET /api/templates: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTemplate(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromBody] CreateTemplateRequest request
    )
    {
        try
        {
            bool success = await _googleSheetsService.AppendTemplateAsync
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
                return Ok(new { message = "Successfully added new template!" });
            }
            else
            {
                return Problem("Failed to add new template.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint POST /api/templates: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTemplateById(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromQuery] int id,
        [FromBody] PutTemplate request
    )
    {
        try
        {
            var success = await _googleSheetsService.UpdateTemplateAsync(
                spreadsheetId,
                sheet,
                id,
                new Template
                {
                    Id = id,
                    Name = request.Name,
                    Group = request.Group,
                    Category = request.Category,
                    Tag = request.Tag,
                    Amount = request.Amount,
                    Description = request.Description
                }
            );

            if (success)
            {
                return Ok(new { message = $"Successfully updated template with ID {id}" });
            }
            else
            {
                return Problem($"Failed to update template with ID {id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint PUT /api/templates: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteTemplateById(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromQuery] int id
    )
    {
        try
        {
            var success = await _googleSheetsService.DeleteTemplateByIdAsync(spreadsheetId, sheet, id);

            if (success)
            {
                return Ok(new { message = $"Successfully deleted template with ID {id}" });
            }
            else
            {
                return Problem($"Failed to delete template with ID {id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint DELETE /api/templates: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }
}