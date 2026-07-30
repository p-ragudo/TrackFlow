using api.Services;
using Microsoft.AspNetCore.Mvc;
using api.Dto;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LoansController : ControllerBase
{
    private readonly GoogleSheetsLoanService _googleSheetsService;

    public LoansController(GoogleSheetsLoanService googleSheetsService)
    {
        _googleSheetsService = googleSheetsService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLoan(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromBody] CreateLoanRequest request
    )
    {
        try
        {
            bool success = await _googleSheetsService.AppendLoanAsync(
                spreadsheetId,
                sheet,
                request.ColumnStart,
                request.ColumnEnd,
                request.CounterSheet,
                request.RowCell,
                request.IdCell,
                request.Name,
                request.Amount,
                request.Description
            );

            if (success)
            {
                return Ok(new { message = "Successfully added new loan!" });
            }
            else
            {
                return Problem("Failed to add new loan.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint POST /api/v1/loans: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetLoans(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromQuery] string columnStart,
        [FromQuery] string columnEnd
    )
    {
        try
        {
            List<Loan>? loans = await _googleSheetsService.GetLoansAsync(
                spreadsheetId, sheet, columnStart, columnEnd
            );

            if (loans == null)
                return NotFound("Loans could not be found.");

            return Ok(new { loans });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint GET /api/v1/loans: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateLoanById(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromBody] PutLoan request
    )
    {
        try
        {
            var success = await _googleSheetsService.UpdateLoanAsync(
                spreadsheetId,
                sheet,
                request.ColumnStart,
                request.ColumnEnd,
                request.Id,
                new Loan
                {
                    Id = request.Id,
                    Date = request.Date,
                    Month = request.Month,
                    Day = request.Day,
                    Name = request.Name,
                    Amount = request.Amount,
                    Description = request.Description
                }
            );

            if (success)
            {
                return Ok(new { message = "Successfully updated loan!" });
            }
            else
            {
                return Problem("Failed to update loan.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint PUT /api/v1/loans: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteLoanById(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromQuery] string columnStart,
        [FromQuery] string columnEnd,
        [FromQuery] int id
    )
    {
        try
        {
            var success = await _googleSheetsService.DeleteLoanByIdAsync(
                spreadsheetId,
                sheet,
                columnStart,
                columnEnd,
                id
            );

            if (success)
            {
                return Ok(new { message = "Successfully deleted loan!" });
            }
            else
            {
                return Problem("Failed to delete loan.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint DELETE /api/v1/loans: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }
}