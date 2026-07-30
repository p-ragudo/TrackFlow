using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

using api.Models;
using SheetUtils = api.Utils.GoogleSheetUtils;

namespace api.Services;

public class GoogleSheetsLoanService
{
    private readonly GoogleCredential _credential;
    private readonly SheetsService _sheetService;

    public GoogleSheetsLoanService()
    {
        _credential = CredentialFactory.FromFile<ServiceAccountCredential>("google-credentials.json").ToGoogleCredential();

        _sheetService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _credential
        });
    }

    public async Task<bool> AppendLoanAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        string counterSheet,
        string rowCell,
        string idCell,
        string name,
        decimal amount,
        string? description = null
    )
    {
        DateOnly today = SheetUtils.GetDateLocal();

        string? nextRowCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{counterSheet}!{rowCell}");
        if (nextRowCell == null)
        {
            Console.WriteLine("Failed to append loan. The next row counter could not be fetched.");
            return false;
        }
        if (!int.TryParse(nextRowCell, out int nextRow))
        {
            Console.WriteLine("Failed to append loan. The next row counter could not be parsed.");
            return false;
        }

        string? nextIdCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{counterSheet}!{idCell}");
        if (nextIdCell == null)
        {
            Console.WriteLine("Failed to append loan. The next row id could not be fetched.");
            return false;
        }
        if (!int.TryParse(nextIdCell, out int nextId))
        {
            Console.WriteLine("Failed to append loan. The next row id could not be parsed.");
            return false;
        }

        string range = $"{sheet}!{columnStart}{nextRow}:{columnEnd}{nextRow}";

        var newLoan = new Loan
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
            Values = [newLoan.ToSpreadsheetRow()]
        };

        try
        {
            AppendValuesResponse response = await SheetUtils.AppendAsync(_sheetService, spreadsheetId, range, values);

            if (response?.Updates?.UpdatedRows == 1)
            {
                Console.WriteLine("Succesfully appended new loan!");
                return true;
            }

            Console.WriteLine("Failed to append loan.");
            return false;

        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Failed to append loan. Google API error: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Loan>?> GetLoansAsync(
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

        var loans = SheetUtils.MapValuesToLoan(values);

        return loans;
    }

    public async Task<bool> UpdateLoanAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        int id,
        Loan updatedLoan
    )
    {
        var loans = await GetLoansAsync(spreadsheetId, sheet, columnStart, columnEnd);

        if (loans == null)
            return false;

        int targetRow = -1;
        for (int i = 0; i < loans.Count; i++)
        {
            if (loans[i].Id == id)
                targetRow = loans[i].Id + 1;
        }

        if (targetRow == -1)
            return false;

        var specificRowRange = $"{sheet}!{columnStart}{targetRow}:{columnEnd}{targetRow}";

        var newLoan = new Loan
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
            Values = [newLoan.ToSpreadsheetRow()]
        };

        try
        {
            var response = await SheetUtils.UpdateAsync(
                _sheetService, spreadsheetId, specificRowRange, valueRange);

            if (response?.UpdatedRows == 1)
            {
                Console.WriteLine($"Successfully edited loan with ID {id}");
                return true;
            }

            Console.WriteLine($"Failed to edit loan with ID: {id}");
            return false;
        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Failed to update loan with ID {id}. Google API error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteLoanByIdAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        int id
    )
    {
        var loans = await GetLoansAsync(spreadsheetId, sheet, columnStart, columnEnd);
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
            Console.WriteLine($"Loan with ID {id} could not be found in the sheet");
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
            Console.WriteLine($"Successfully deleted loan with ID {id} from row {currentSheetRowPosition} and shifted remaining rows up.");
            return true;
        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Google API error during loan row deletion: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AppendLoanTemplateAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        string counterSheet,
        string rowCell,
        string idCell,
        string name,
        decimal amount,
        string? description = null)
    {
        string? nextRowCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{counterSheet}!{rowCell}");
        if (nextRowCell == null)
        {
            Console.WriteLine("Failed to append loan template. The next row counter could not be fetched.");
            return false;
        }
        if (int.TryParse(nextRowCell, out int nextRow))
        {
            Console.WriteLine("Failed to append loan template. The next row counter could not be parsed.");
            return false;
        }

        string? nextIdCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{counterSheet}!{idCell}");
        if (nextIdCell == null)
        {
            Console.WriteLine("Failed to append loan template. The next id counter could not be fetched.");
            return false;
        }
        if (int.TryParse(nextIdCell, out int nextId))
        {
            Console.WriteLine("Failed to append loan template. The next id counter could not be parsed.");
            return false;
        }

        string range = $"{sheet}!{columnStart}{nextRow}:{columnEnd}{nextRow}";

        var newLoanTemplate = new LoanTemplate
        {
            Id = nextId,
            Name = name,
            Amount = amount,
            Description = description ?? string.Empty
        };

        var values = new ValueRange
        {
            Values = [newLoanTemplate.ToSpreadsheetRow()]
        };

        try
        {
            AppendValuesResponse response = await SheetUtils.AppendAsync(_sheetService, spreadsheetId, range, values);

            if (response?.Updates?.UpdatedRows == 1)
            {
                Console.WriteLine("Succesfully appended new loan template!");
                return true;
            }

            Console.WriteLine("Failed to append loan template.");
            return false;

        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Failed to append loan template. Google API error: {ex.Message}");
            return false;
        }
    }

    public async Task<List<LoanTemplate>?> GetLoanTemplatesAsync(
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

        var loanTemplates = SheetUtils.MapValuesToLoanTemplate(values);

        return loanTemplates;
    }

    public async Task<bool> UpdateLoanTemplateAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        int id,
        LoanTemplate updatedLoanTemplate
    )
    {
        var loanTemplates = await GetLoanTemplatesAsync(spreadsheetId, sheet, columnStart, columnEnd);

        if (loanTemplates == null)
            return false;

        int targetRow = -1;
        for (int i = 0; i < loanTemplates.Count; i++)
        {
            if (loanTemplates[i].Id == id)
                targetRow = loanTemplates[i].Id + 1;
        }

        if (targetRow == -1)
            return false;

        var specificRowRange = $"{sheet}!{columnStart}{targetRow}:{columnEnd}{targetRow}";

        var newLoanTemplate = new LoanTemplate
        {
            Id = id,
            Name = updatedLoanTemplate.Name,
            Amount = updatedLoanTemplate.Amount,
            Description = updatedLoanTemplate.Description ?? string.Empty
        };

        var valueRange = new ValueRange
        {
            Values = [newLoanTemplate.ToSpreadsheetRow()]
        };

        try
        {
            var response = await SheetUtils.UpdateAsync(
                _sheetService, spreadsheetId, specificRowRange, valueRange);

            if (response?.UpdatedRows == 1)
            {
                Console.WriteLine($"Successfully edited loan template with ID {id}");
                return true;
            }

            Console.WriteLine($"Failed to edit loan template with ID: {id}");
            return false;
        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Failed to update loan template with ID {id}. Google API error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteLoanTemplateByIdAsync(
        string spreadsheetId,
        string sheet,
        string columnStart,
        string columnEnd,
        int id
    )
    {
        var loanTemplates = await GetLoanTemplatesAsync(spreadsheetId, sheet, columnStart, columnEnd);
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
            Console.WriteLine($"Loan template with ID {id} could not be found in the sheet");
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
            Console.WriteLine($"Successfully deleted loan template with ID {id} from row {currentSheetRowPosition} and shifted remaining rows up.");
            return true;
        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Google API error during loan template row deletion: {ex.Message}");
            return false;
        }
    }
}