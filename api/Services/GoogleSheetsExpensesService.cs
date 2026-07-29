using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

using api.Models;
using api.Dto;
using SheetUtils = api.Utils.GoogleSheetUtils;

namespace api.Services;

public class GoogleSheetsExpensesService
{
    private readonly GoogleCredential _credential;
    private readonly SheetsService _sheetService;

    public GoogleSheetsExpensesService()
    {
        _credential = CredentialFactory.FromFile<ServiceAccountCredential>("google-credentials.json").ToGoogleCredential();

        _sheetService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _credential
        });
    }

    public async Task<bool> AppendExpenseAsync(
        string spreadsheetId,
        string sheet,
        string name,
        string rowCell,
        string idCell,
        string group,
        string category,
        string tag,
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
            Console.WriteLine("Failed to append expense. The next row counter could not be fetched.");
            return false;
        }
        if (int.TryParse(nextRowCell, out int nextRow))
        {
            Console.WriteLine("Failed to append expense. The next row counter could not be parsed.");
            return false;
        }

        string? nextIdCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{completeSheet}!{idCell}");
        if (nextRowCell == null)
        {
            Console.WriteLine("Failed to append expense. The next row id could not be fetched.");
            return false;
        }
        if (int.TryParse(nextIdCell, out int nextId))
        {
            Console.WriteLine("Failed to append expense. The next row id could not be parsed.");
            return false;
        }

        string range = $"{completeSheet}!A{nextRow}:J{nextRow}";

        var newExpense = new Expense
        {
            Id = nextId,
            Date = today,
            Month = today.ToString("MMMM"),
            Day = today.ToString("dddd"),
            Name = name,
            Group = group,
            Category = category,
            Tag = tag,
            Amount = amount,
            Description = description ?? string.Empty
        };

        var values = new ValueRange
        {
            Values = [newExpense.ToSpreadsheetRow()]
        };

        try
        {
            AppendValuesResponse response = await SheetUtils.AppendAsync(_sheetService, spreadsheetId, range, values);

            if (response?.Updates?.UpdatedRows == 1)
            {
                Console.WriteLine("Succesfully appended new row!");
                return true;
            }

            Console.WriteLine("Failed to append expense.");
            return false;

        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Failed to append expense. Google API error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AppendTemplateAsync(
        string spreadsheetId,
        string sheet,
        string name,
        string rowCell,
        string idCell,
        string group,
        string category,
        string tag,
        decimal amount,
        string? description = null)
    {
        string? nextRowCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{sheet}!{rowCell}");
        if (nextRowCell == null) {
            Console.WriteLine("Failed to append template. The next row counter could not be fetched.");
            return false;
        }
        if (int.TryParse(nextRowCell, out int nextRow))
        {
            Console.WriteLine("Failed to append template. The next row counter could not be parsed.");
            return false;
        }

        string? nextIdCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{sheet}!{idCell}");
        if (nextIdCell == null) {
            Console.WriteLine("Failed to append template. The next id counter could not be fetched.");
            return false;
        }
        if (int.TryParse(nextIdCell, out int nextId))
        {
            Console.WriteLine("Failed to append template. The next id counter could not be parsed.");
            return false;
        }

        string range = $"{sheet}!A{nextRow}:G{nextRow}";

        var newTemplate = new ExpenseTemplate
        {
            Id = (int)nextId,
            Name = name,
            Group = group,
            Category = category,
            Tag = tag,
            Amount = amount,
            Description = description ?? string.Empty
        };

        var values = new ValueRange
        {
            Values = [ newTemplate.ToSpreadsheetRow() ]
        };

        try
        {
            AppendValuesResponse response = await SheetUtils.AppendAsync(_sheetService, spreadsheetId, range, values);

            if (response?.Updates?.UpdatedRows == 1)
            {
                Console.WriteLine("Succesfully appended new template!");
                return true;
            }

            Console.WriteLine("Failed to append template.");
            return false;

        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Failed to append template. Google API error: {ex.Message}");
            return false;
        }
    }

    public async Task<List<ExpenseTemplate>?> GetTemplatesAsync(string spreadsheetId, string sheet)
    {
        var values = await SheetUtils.GetValuesFromRange(_sheetService, spreadsheetId, $"{sheet}!A:G");

        if (values == null || values.Count <= 1)
            return null;

        var templates = SheetUtils.MapValuesToTemplate(values);

        return templates;
    }

    public async Task<List<TodayExpenseItemResponse>?> GetTodayExpensesAsync(string spreadsheetId, string sheet)
    {
        var values = await SheetUtils.GetValuesFromRange(_sheetService, spreadsheetId, $"{sheet}!A:I");

        if (values == null)
        {
            return null;
        }
        else if (values.Count < 1)
        {
            return [];
        }

        var expenses = SheetUtils.MapValuesToExpense(values);

        return expenses;
    }

    public async Task<Identifiers?> GetIdentifiersAsync(string spreadsheetId, string sheet)
    {
        var values = await SheetUtils.GetValuesFromRange(_sheetService, spreadsheetId, $"{sheet}!A:C");

        if (values == null)
        {
            return null;
        }
        else if (values.Count < 1)
        {
            return new Identifiers();
        }

        var identifiers = SheetUtils.MapValuesToIdentifiers(values);

        return identifiers;
    }

    public async Task<bool> UpdateTemplateAsync(string spreadsheetId, string sheet, int id, ExpenseTemplate updatedTemplate)
    {
        var values = await GetTemplatesAsync(spreadsheetId, sheet);

        if (values == null)
            return false;

        int targetRow = -1;
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i].Id == id)
                targetRow = values[i].Id + 1;
        }

        if (targetRow == -1)
            return false;

        var specificRowRange = $"{sheet}!A{targetRow}:G{targetRow}";

        var newTemplate = new ExpenseTemplate
        {
            Id = id,
            Name = updatedTemplate.Name,
            Group = updatedTemplate.Group,
            Category = updatedTemplate.Category,
            Tag = updatedTemplate.Tag,
            Amount = updatedTemplate.Amount,
            Description = updatedTemplate.Description ?? string.Empty
        };

        var valueRange = new ValueRange
        {
            Values = [newTemplate.ToSpreadsheetRow()]
        };

        try
        {
            var response = await SheetUtils.UpdateAsync(_sheetService, spreadsheetId, specificRowRange, valueRange);

            if (response?.UpdatedRows == 1)
            {
                Console.WriteLine($"Successfully edited template with ID {id}");
                return true;
            }

            Console.WriteLine($"Failed to edit template with ID: {id}");
            return false;
        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Failed to update template with ID {id}. Google API error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteTemplateByIdAsync(string spreadsheetId, string sheet, int id)
    {
        var templates = await GetTemplatesAsync(spreadsheetId, sheet);
        if (templates == null || templates.Count == 0)
            return false;

        int currentSheetRowPosition = -1;

        for (int i = 0; i < templates.Count; i++)
        {
            if (templates[i].Id == id)
            {
                currentSheetRowPosition = i + 2;
                break;
            }
        }

        if (currentSheetRowPosition == -1)
        {
            Console.WriteLine($"Template with ID {id} could not be found in the sheet");
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
            Console.WriteLine($"Successfully deleted Template ID {id} from Row {currentSheetRowPosition} and shifted remaining rows up.");
            return true;
        }
        catch (Google.GoogleApiException ex)
        {
            Console.WriteLine($"Google API error during row deletion: {ex.Message}");
            return false;
        }
    }

    public async Task<decimal?> GetTodayTotal(string spreadsheetId, string sheet)
    {
        DateOnly today = SheetUtils.GetDateLocal();
        string completeSheet = $"{today:yyyy}_{sheet}";

        string? todayTotalCell = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{completeSheet}!O2");
        if (todayTotalCell == null)
        {
            Console.WriteLine("Failed to get today's total. The cell could not be fetched.");
            return null;
        }
        if (decimal.TryParse(todayTotalCell, out decimal todayTotal))
        {
            Console.WriteLine("Failed to get today's total. The cell could not be parsed.");
            return null;
        }

        return todayTotal;
    }

    public async Task<BootstrapResponse?> GetBootstrapDataAsync(
        string spreadsheetId,
        string templateSheet,
        string expensesSheet,
        string todayExpensesSheet,
        string identifiersSheet,
        string nameSheet
    )
    {
        DateOnly today = SheetUtils.GetDateLocal();

        // Define all ranges needed for startup
        var ranges = new List<string>
        {
            $"{templateSheet}!A:J",            // Range 0
            $"{today:yyyy}_{expensesSheet}!O2",      // Range 1 (Today's total cell)
            $"{todayExpensesSheet}!A:I",        // Range 2
            $"{identifiersSheet}!A:C",
            $"{nameSheet}!B1"         // Range 3
        };

        var batchGetRequest = _sheetService.Spreadsheets.Values.BatchGet(spreadsheetId);
        batchGetRequest.Ranges = ranges;

        var response = await batchGetRequest.ExecuteAsync();
        if (response?.ValueRanges == null || response.ValueRanges.Count < 4)
        {
            return null;
        }

        // 1. Templates
        var templateValues = response.ValueRanges[0].Values;
        var templates = SheetUtils.MapValuesToTemplate(templateValues);

        // 2. Today's Total
        decimal todayTotal = 0;
        var totalValues = response.ValueRanges[1].Values;
        if (totalValues?.Count == 1 && totalValues[0].Count > 0)
        {
            _ = decimal.TryParse(totalValues[0][0]?.ToString(), out todayTotal);
        }

        // 3. Today's Expenses
        var expenseValues = response.ValueRanges[2].Values;
        var todayExpenses = expenseValues != null ? SheetUtils.MapValuesToExpense(expenseValues) : new List<TodayExpenseItemResponse>();

        // 4. Identifiers
        var identifierValues = response.ValueRanges[3].Values;
        var identifiers = identifierValues != null ? SheetUtils.MapValuesToIdentifiers(identifierValues) : new Identifiers();

        var nameValues = response.ValueRanges[4].Values;
        string name = (nameValues?.Count > 0 && nameValues[0].Count > 0)
            ? nameValues[0][0].ToString() ?? string.Empty
            : string.Empty;

        return new BootstrapResponse(templates, todayTotal, todayExpenses, identifiers, name);
    }

    public async Task<string?> GetNameAsync(string spreadsheetId, string sheet)
    {
        string? name = await SheetUtils.GetCellStringAsync(_sheetService, spreadsheetId, $"{sheet}!B1");
        return name;
    }
}