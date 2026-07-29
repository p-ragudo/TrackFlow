using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

using api.Models;
using SheetUtils = api.Utils.GoogleSheetUtils;

namespace api.Services;

public class GoogleSheetsPaymentService
{
    private readonly GoogleCredential _credential;
    private readonly SheetsService _sheetService;

    public GoogleSheetsPaymentService()
    {
        _credential = CredentialFactory.FromFile<ServiceAccountCredential>("google-credentials.json").ToGoogleCredential();

        _sheetService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _credential
        });
    }

    public async Task<bool> AppendPaymentAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        string rowCell,
        string idCell,
        string name,
        decimal amount,
        string? description = null
    )
    {
        DateOnly today = SheetUtils.GetDateLocal();
        string completeSheet = $"{today:yyyy}_{sheet}";
        Console.WriteLine(completeSheet);

        string? nextRowCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{completeSheet}!{rowCell}");
        if (nextRowCell == null)
        {
            Console.WriteLine("Failed to append payment. The next row counter could not be fetched.");
            return false;
        }
        if (int.TryParse(nextRowCell, out int nextRow))
        {
            Console.WriteLine("Failed to append payment. The next row counter could not be parsed.");
            return false;
        }

        string? nextIdCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{completeSheet}!{idCell}");
        if (nextRowCell == null)
        {
            Console.WriteLine("Failed to append payment. The next row id could not be fetched.");
            return false;
        }
        if (int.TryParse(nextIdCell, out int nextId))
        {
            Console.WriteLine("Failed to append payment. The next row id could not be parsed.");
            return false;
        }

        string range = $"{completeSheet}!{columnStart}{nextRow}:{columnEnd}{nextRow}";

        var newPayment = new Payment
        {
            Id = nextId,
            Date = today,
            Month = today.ToString("MMMM"),
            Day = today.ToString("dddd"),
            Name = name,
            Amount = amount,
            Description = description ?? string.Empty
        };

        var values = new ValueRange
        {
            Values = [newPayment.ToSpreadsheetRow()]
        };

        try
        {
            AppendValuesResponse response = await SheetUtils.AppendAsync(_sheetService, spreadsheetId, range, values);

            if (response?.Updates?.UpdatedRows == 1)
            {
                Console.WriteLine("Succesfully appended new payment!");
                return true;
            }

            Console.WriteLine("Failed to append payment.");
            return false;

        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Failed to append payment. Google API error: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Payment>?> GetPaymentsAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd
    )
    {
        var values = await SheetUtils.GetValuesFromRange(
            _sheetService, spreadsheetId, $"{sheet}!{columnStart}:{columnEnd}");

        if (values == null || values.Count <= 1)
            return null;

        var payments = SheetUtils.MapValuesToPayment(values);

        return payments;
    }

    public async Task<bool> UpdatePaymentAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        int id,
        Loan updatedLoan
    )
    {
        var payments = await GetPaymentsAsync(spreadsheetId, sheet, columnStart, columnEnd);

        if (payments == null)
            return false;

        int targetRow = -1;
        for (int i = 0; i < payments.Count; i++)
        {
            if (payments[i].Id == id)
                targetRow = payments[i].Id + 1;
        }

        if (targetRow == -1)
            return false;

        var specificRowRange = $"{sheet}!{columnStart}{targetRow}:{columnEnd}{targetRow}";

        var newPayment = new Payment
        {
            Id = id,
            Date = updatedLoan.Date,
            Month = updatedLoan.Month,
            Day = updatedLoan.Day,
            Name = updatedLoan.Name,
            Amount = updatedLoan.Amount,
            Description = updatedLoan.Description ?? string.Empty
        };

        var valueRange = new ValueRange
        {
            Values = [newPayment.ToSpreadsheetRow()]
        };

        try
        {
            var response = await SheetUtils.UpdateAsync(
                _sheetService, spreadsheetId, specificRowRange, valueRange);

            if (response?.UpdatedRows == 1)
            {
                Console.WriteLine($"Successfully edited payment with ID {id}");
                return true;
            }

            Console.WriteLine($"Failed to edit payment with ID: {id}");
            return false;
        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Failed to update payment with ID {id}. Google API error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeletePaymentByIdAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        int id
    )
    {
        var loans = await GetPaymentsAsync(spreadsheetId, sheet, columnStart, columnEnd);
        if (loans == null || loans.Count == 0)
            return false;

        int currentSheetRowPosition = -1;
        for (int i = 0; i < loans.Count; i++)
        {
            if (loans[i].Id == id)
            {
                currentSheetRowPosition = i + 2;
                break;
            }
        }

        if (currentSheetRowPosition == -1)
        {
            Console.WriteLine($"Payment with ID {id} could not be found in the sheet");
            return false;
        }

        int internalGoogleIndex = currentSheetRowPosition - 1;
        var spreadsheetInfo = await _sheetService.Spreadsheets.Get(spreadsheetId).ExecuteAsync();

        var targetSheet = spreadsheetInfo.Sheets
            .FirstOrDefault(s => s.Properties.Title.Equals(sheet, StringComparison.OrdinalIgnoreCase));
        if (targetSheet == null)
        {
            Console.WriteLine($"Could not find sheet '{sheet}'");
            return false;
        }

        int? sheetId = targetSheet.Properties.SheetId;

        var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = new List<Request>
            {
                new Request
                {
                    DeleteDimension = new DeleteDimensionRequest
                    {
                        Range = new DimensionRange
                        {
                            SheetId = sheetId,
                            Dimension = "ROWS",
                            StartIndex = internalGoogleIndex,
                            EndIndex = internalGoogleIndex + 1
                        }
                    }
                }
            }
        };

        try
        {
            await _sheetService.Spreadsheets.BatchUpdate(batchUpdateRequest, spreadsheetId).ExecuteAsync();
            Console.WriteLine($"Successfully deleted payment with ID {id} from row {currentSheetRowPosition} and shifted remaining rows up.");
            return true;
        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Google API error during payment row deletion: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AppendPaymentTemplateAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        string rowCell,
        string idCell,
        string name,
        decimal amount,
        string? description = null)
    {
        string? nextRowCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{sheet}!{rowCell}");
        if (nextRowCell == null)
        {
            Console.WriteLine("Failed to append payment template. The next row counter could not be fetched.");
            return false;
        }
        if (int.TryParse(nextRowCell, out int nextRow))
        {
            Console.WriteLine("Failed to append payment template. The next row counter could not be parsed.");
            return false;
        }

        string? nextIdCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{sheet}!{idCell}");
        if (nextIdCell == null)
        {
            Console.WriteLine("Failed to append payment template. The next id counter could not be fetched.");
            return false;
        }
        if (int.TryParse(nextIdCell, out int nextId))
        {
            Console.WriteLine("Failed to append payment template. The next id counter could not be parsed.");
            return false;
        }

        string range = $"{sheet}!{columnStart}{nextRow}:{columnEnd}{nextRow}";

        var newPaymentTemplate = new PaymentTemplate
        {
            Id = nextId,
            Name = name,
            Amount = amount,
            Description = description ?? string.Empty
        };

        var values = new ValueRange
        {
            Values = [newPaymentTemplate.ToSpreadsheetRow()]
        };

        try
        {
            AppendValuesResponse response = await SheetUtils.AppendAsync(_sheetService, spreadsheetId, range, values);

            if (response?.Updates?.UpdatedRows == 1)
            {
                Console.WriteLine("Succesfully appended new payment template!");
                return true;
            }

            Console.WriteLine("Failed to append payment template.");
            return false;

        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Failed to append payment template. Google API error: {ex.Message}");
            return false;
        }
    }

    public async Task<List<PaymentTemplate>?> GetPaymentTemplatesAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd
    )
    {
        var values = await SheetUtils.GetValuesFromRange(
            _sheetService, spreadsheetId, $"{sheet}!{columnStart}:{columnEnd}");

        if (values == null || values.Count <= 1)
            return null;

        var paymentTemplates = SheetUtils.MapValuesToPaymentTemplate(values);

        return paymentTemplates;
    }

    public async Task<bool> UpdatePaymentTemplateAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        int id,
        LoanTemplate updatedLoanTemplate
    )
    {
        var paymentTemplates = await GetPaymentTemplatesAsync(spreadsheetId, sheet, columnStart, columnEnd);

        if (paymentTemplates == null)
            return false;

        int targetRow = -1;
        for (int i = 0; i < paymentTemplates.Count; i++)
        {
            if (paymentTemplates[i].Id == id)
                targetRow = paymentTemplates[i].Id + 1;
        }

        if (targetRow == -1)
            return false;

        var specificRowRange = $"{sheet}!{columnStart}{targetRow}:{columnEnd}{targetRow}";

        var newPaymentTemplate = new PaymentTemplate
        {
            Id = id,
            Name = updatedLoanTemplate.Name,
            Amount = updatedLoanTemplate.Amount,
            Description = updatedLoanTemplate.Description ?? string.Empty
        };

        var valueRange = new ValueRange
        {
            Values = [newPaymentTemplate.ToSpreadsheetRow()]
        };

        try
        {
            var response = await SheetUtils.UpdateAsync(
                _sheetService, spreadsheetId, specificRowRange, valueRange);

            if (response?.UpdatedRows == 1)
            {
                Console.WriteLine($"Successfully edited payment template with ID {id}");
                return true;
            }

            Console.WriteLine($"Failed to edit payment template with ID: {id}");
            return false;
        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Failed to update payment template with ID {id}. Google API error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeletePaymentTemplateByIdAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        int id
    )
    {
        var loanTemplates = await GetPaymentTemplatesAsync(spreadsheetId, sheet, columnStart, columnEnd);
        if (loanTemplates == null || loanTemplates.Count == 0)
            return false;

        int currentSheetRowPosition = -1;
        for (int i = 0; i < loanTemplates.Count; i++)
        {
            if (loanTemplates[i].Id == id)
            {
                currentSheetRowPosition = i + 2;
                break;
            }
        }

        if (currentSheetRowPosition == -1)
        {
            Console.WriteLine($"Payment template with ID {id} could not be found in the sheet");
            return false;
        }

        int internalGoogleIndex = currentSheetRowPosition - 1;
        var spreadsheetInfo = await _sheetService.Spreadsheets.Get(spreadsheetId).ExecuteAsync();

        var targetSheet = spreadsheetInfo.Sheets
            .FirstOrDefault(s => s.Properties.Title.Equals(sheet, StringComparison.OrdinalIgnoreCase));
        if (targetSheet == null)
        {
            Console.WriteLine($"Could not find sheet '{sheet}'");
            return false;
        }

        int? sheetId = targetSheet.Properties.SheetId;

        var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = new List<Request>
            {
                new Request
                {
                    DeleteDimension = new DeleteDimensionRequest
                    {
                        Range = new DimensionRange
                        {
                            SheetId = sheetId,
                            Dimension = "ROWS",
                            StartIndex = internalGoogleIndex,
                            EndIndex = internalGoogleIndex + 1
                        }
                    }
                }
            }
        };

        try
        {
            await _sheetService.Spreadsheets.BatchUpdate(batchUpdateRequest, spreadsheetId).ExecuteAsync();
            Console.WriteLine($"Successfully deleted payment template with ID {id} from row {currentSheetRowPosition} and shifted remaining rows up.");
            return true;
        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Google API error during payment template row deletion: {ex.Message}");
            return false;
        }
    }
}