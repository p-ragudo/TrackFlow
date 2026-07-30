using api.Services;
using Microsoft.AspNetCore.Mvc;
using api.Dto;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly GoogleSheetsPaymentService _googleSheetsService;

    public PaymentsController(GoogleSheetsPaymentService googleSheetsService)
    {
        _googleSheetsService = googleSheetsService;
    }


    [HttpPost]
    public async Task<IActionResult> CreatePayment(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromBody] CreatePaymentRequest request
    )
    {
        try
        {
            bool success = await _googleSheetsService.AppendPaymentAsync(
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
                return Ok(new { message = "Successfully added new payment!" });
            }
            else
            {
                return Problem("Failed to add new payment.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint POST /api/v1/payments: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPayments(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromQuery] string columnStart,
        [FromQuery] string columnEnd
    )
    {
        try
        {
            List<Payment>? payments = await _googleSheetsService.GetPaymentsAsync(
                spreadsheetId, sheet, columnStart, columnEnd
            );

            if (payments == null)
                return NotFound("Payments could not be found.");

            return Ok(new { payments });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint GET /api/v1/payments: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePaymentById(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromBody] PutPayment request
    )
    {
        try
        {
            var success = await _googleSheetsService.UpdatePaymentAsync(
                spreadsheetId,
                sheet,
                request.ColumnStart,
                request.ColumnEnd,
                request.Id,
                new Payment
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
                return Ok(new { message = "Successfully updated payment!" });
            }
            else
            {
                return Problem("Failed to update payment.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint PUT /api/v1/payment: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeletePaymentById(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromQuery] string columnStart,
        [FromQuery] string columnEnd,
        [FromQuery] int id
    )
    {
        try
        {
            var success = await _googleSheetsService.DeletePaymentByIdAsync(
                spreadsheetId,
                sheet,
                columnStart,
                columnEnd,
                id
            );

            if (success)
            {
                return Ok(new { message = "Successfully deleted payment!" });
            }
            else
            {
                return Problem("Failed to delete payment.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint DELETE /api/v1/payment: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpPost("templates")]
    public async Task<IActionResult> CreatePaymentTemplate(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromBody] CreatePaymentTemplateRequest request
    )
    {
        try
        {
            bool success = await _googleSheetsService.AppendPaymentTemplateAsync(
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
                return Ok(new { message = "Successfully added new payment template!" });
            }
            else
            {
                return Problem("Failed to add new payment template.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint POST /api/v1/payments/templates: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetPaymentTemplates(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromQuery] string columnStart,
        [FromQuery] string columnEnd
    )
    {
        try
        {
            List<PaymentTemplate>? paymentTemplates = await _googleSheetsService.GetPaymentTemplatesAsync(
                spreadsheetId, sheet, columnStart, columnEnd
            );

            if (paymentTemplates == null)
                return NotFound("Payment templates could not be found.");

            return Ok(new { paymentTemplates });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint GET /api/v1/payments/templates: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpPut("templates")]
    public async Task<IActionResult> UpdatePaymentTemplateById(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromBody] PutPaymentTemplate request
    )
    {
        try
        {
            var success = await _googleSheetsService.UpdatePaymentTemplateAsync(
                spreadsheetId,
                sheet,
                request.ColumnStart,
                request.ColumnEnd,
                request.Id,
                new PaymentTemplate
                {
                    Id = request.Id,
                    Name = request.Name,
                    Amount = request.Amount,
                    Description = request.Description
                }
            );

            if (success)
            {
                return Ok(new { message = "Successfully updated payment template!" });
            }
            else
            {
                return Problem("Failed to update payment template.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint PUT /api/v1/payment/templates: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpDelete("templates")]
    public async Task<IActionResult> DeletePaymentTemplateById(
        [FromQuery] string spreadsheetId,
        [FromQuery] string sheet,
        [FromQuery] string columnStart,
        [FromQuery] string columnEnd,
        [FromQuery] int id
    )
    {
        try
        {
            var success = await _googleSheetsService.DeletePaymentTemplateByIdAsync(
                spreadsheetId,
                sheet,
                columnStart,
                columnEnd,
                id
            );

            if (success)
            {
                return Ok(new { message = "Successfully deleted payment template!" });
            }
            else
            {
                return Problem("Failed to delete payment template.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at endpoint DELETE /api/v1/payment/templates: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }
}