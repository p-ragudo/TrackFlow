using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

using api.Models;
using api.Dto;

namespace api.Services;

public class GoogleSheetsService
{
    private readonly GoogleCredential _credential;
    private readonly SheetsService _sheetService;

    public GoogleSheetsService()
    {
        _credential = CredentialFactory.FromFile<ServiceAccountCredential>("google-credentials.json").ToGoogleCredential();

        _sheetService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _credential
        });
    }

    private async Task<AppendValuesResponse> AppendAsync(string spreadsheetId, string range, ValueRange valueRange)
    {
        AppendRequest? appendRequest = _sheetService.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);

        appendRequest.ValueInputOption = AppendRequest.ValueInputOptionEnum.USERENTERED;
        appendRequest.InsertDataOption = AppendRequest.InsertDataOptionEnum.OVERWRITE;

        AppendValuesResponse response = await appendRequest.ExecuteAsync();
        return response;
    }

    private async Task<UpdateValuesResponse> UpdateAsync(string spreadsheetId, string range, ValueRange valueRange)
    {
        UpdateRequest? updateRequest = _sheetService.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);

        updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;

        UpdateValuesResponse response = await updateRequest.ExecuteAsync();
        return response;
    }

    private async Task<decimal?> GetNextIntFromCellAsync(string spreadsheetId, string range)
    {
        var getCellRequest = _sheetService.Spreadsheets.Values.Get(spreadsheetId, range);
        var cellResponse = await getCellRequest.ExecuteAsync();

        if (cellResponse.Values == null || cellResponse.Values.Count != 1)
            return null;

        if (int.TryParse(cellResponse.Values[0][0]?.ToString(), out int nextRow))
            return nextRow;

        return null;
    }

    private async Task<IList<IList<object>>?> GetValuesFromRange(string spreadsheetId, string range)
    {
        var getCellRequest = _sheetService.Spreadsheets.Values.Get(spreadsheetId, range);
        var cellResponse = await getCellRequest.ExecuteAsync();

        return cellResponse.Values ?? null;
    }

    private static List<TodayExpenseItemResponse> MapValuesToExpense(IList<IList<object>> values)
    {
        var expenses = new List<TodayExpenseItemResponse>();

        if (values == null || values.Count <= 1)
            return expenses;

        foreach (var row in values.Skip(1)) // skips header row
        {
            if (row.Count == 0) continue;

            expenses.Add(new TodayExpenseItemResponse
            (
                int.TryParse(row[0].ToString(), out int id) ? id : -1,
                row[1].ToString() ?? string.Empty,
                row[2].ToString() ?? string.Empty,
                row[3].ToString() ?? string.Empty,
                row[4].ToString() ?? string.Empty,
                row[5].ToString() ?? string.Empty,
                row[6].ToString() ?? string.Empty,
                decimal.TryParse(row[7].ToString(), out decimal amount) ? amount : -1,
                row.Count > 8 ? row[8].ToString() ?? string.Empty : string.Empty
            ));
        }

        return expenses;
    }

    private static List<Template> MapValuesToTemplate(IList<IList<object>> values)
    {
        var templates = new List<Template>();

        if (values == null || values.Count <= 1)
            return templates;

        foreach (var row in values.Skip(1)) // skips header row
        {
            if (row.Count == 0) continue;

            templates.Add(new Template
            {
                Id = int.TryParse(row[0].ToString(), out int id) ? id : -1,
                Name = row[1].ToString() ?? string.Empty,
                Group = row[2].ToString() ?? string.Empty,
                Category = row[3].ToString() ?? string.Empty,
                Tag = row[4].ToString() ?? string.Empty,
                Amount = decimal.TryParse(row[5].ToString(), out decimal amount) ? amount : -1,
                Description = row.Count > 6 ? row[6].ToString() ?? string.Empty : string.Empty
            });
        }

        return templates;
    }

    private static Identifiers MapValuesToIdentifiers(IList<IList<object>> values)
    {
        var identifiers = new Identifiers();

        if (values == null || values.Count <= 1)
            return identifiers;

        foreach (var row in values.Skip(1))
        {
            if (row.Count == 0) continue;

            identifiers.Groups.Add(row[0].ToString() ?? string.Empty);
            identifiers.Categories.Add(row[1].ToString() ?? string.Empty);
            identifiers.Tags.Add(row[2].ToString() ?? string.Empty);
        }

        return identifiers;
    }

    private static DateOnly GetDateLocal()
    {
        DateTime utcNow = DateTime.UtcNow;

        TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, targetTimeZone);

        return DateOnly.FromDateTime(localDateTime);
    }

    public async Task<bool> AppendExpenseAsync(string spreadsheetId, string sheet, string name, string group, string category, string tag, decimal amount, string? description = null)
    {
        DateOnly today = GetDateLocal();
        string completeSheet = $"{today:yyyy}_{sheet}";
        Console.WriteLine(completeSheet);

        int? nextRow = (int?)await GetNextIntFromCellAsync(spreadsheetId, $"{completeSheet}!M2");
        if (nextRow == null)
        {
            Console.WriteLine("Failed to append expense. The next row counter could not be fetched or parsed.");
            return false;
        }

        int? nextId = (int?)await GetNextIntFromCellAsync(spreadsheetId, $"{completeSheet}!N2");
        if (nextId == null)
        {
            Console.WriteLine("Failed to append expense. The next id could not be fetched or parsed.");
            return false;
        }
        Console.WriteLine($"\t\tID: {nextId}");

        string range = $"{completeSheet}!A{nextRow}:J{nextRow}";

        var newExpense = new Expense
        {
            Id = (int)nextId,
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
            AppendValuesResponse response = await AppendAsync(spreadsheetId, range, values);

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

    public async Task<bool> AppendTemplateAsync(string spreadsheetId, string sheet, string name, string group, string category, string tag, decimal amount, string? description = null)
    {
        int? nextRow = (int?) await GetNextIntFromCellAsync(spreadsheetId, $"{sheet}!I2");
        if (nextRow == null)
        {
            Console.WriteLine("Failed to append template. The next row counter could not be fetched or parsed.");
            return false;
        }

        int? nextId = (int?) await GetNextIntFromCellAsync(spreadsheetId, $"{sheet}!J2");
        if (nextId == null)
        {
            Console.WriteLine("Failed to append template. The next id counter could not be fetched or parsed.");
            return false;
        }

        string range = $"{sheet}!A{nextRow}:G{nextRow}";

        var newTemplate = new Template
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
            AppendValuesResponse response = await AppendAsync(spreadsheetId, range, values);

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

    public async Task<List<Template>?> GetTemplatesAsync(string spreadsheetId, string sheet)
    {
        var values = await GetValuesFromRange(spreadsheetId, $"{sheet}!A:G");

        if (values == null || values.Count <= 1)
            return null;

        var templates = MapValuesToTemplate(values);

        return templates;
    }

    public async Task<List<TodayExpenseItemResponse>?> GetTodayExpensesAsync(string spreadsheetId, string sheet)
    {
        var values = await GetValuesFromRange(spreadsheetId, $"{sheet}!A:I");

        if (values == null)
        {
            return null;
        }
        else if (values.Count < 1)
        {
            return [];
        }

        var expenses = MapValuesToExpense(values);

        return expenses;
    }

    public async Task<Identifiers?> GetIdentifiersAsync(string spreadsheetId, string sheet)
    {
        var values = await GetValuesFromRange(spreadsheetId, $"{sheet}!A:C");

        if (values == null)
        {
            return null;
        }
        else if (values.Count < 1)
        {
            return new Identifiers();
        }

        var identifiers = MapValuesToIdentifiers(values);

        return identifiers;
    }

    public async Task<bool> UpdateTemplateAsync(string spreadsheetId, string sheet, int id, Template updatedTemplate)
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

        var newTemplate = new Template
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
            var response = await UpdateAsync(spreadsheetId, specificRowRange, valueRange);

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
            .FirstOrDefault(s => s.Properties.Title.Equals("Templates", StringComparison.OrdinalIgnoreCase));
        if (targetSheet == null)
        {
            Console.WriteLine("Could not find sheet 'Templates'");
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
        DateOnly today = GetDateLocal();
        string completeSheet = $"{today:yyyy}_{sheet}";

        decimal? todayTotal = await GetNextIntFromCellAsync(spreadsheetId, $"{completeSheet}!O2");

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
        DateOnly today = GetDateLocal();

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
        var templates = MapValuesToTemplate(templateValues);

        // 2. Today's Total
        decimal todayTotal = 0;
        var totalValues = response.ValueRanges[1].Values;
        if (totalValues?.Count == 1 && totalValues[0].Count > 0)
        {
            _ = decimal.TryParse(totalValues[0][0]?.ToString(), out todayTotal);
        }

        // 3. Today's Expenses
        var expenseValues = response.ValueRanges[2].Values;
        var todayExpenses = expenseValues != null ? MapValuesToExpense(expenseValues) : new List<TodayExpenseItemResponse>();

        // 4. Identifiers
        var identifierValues = response.ValueRanges[3].Values;
        var identifiers = identifierValues != null ? MapValuesToIdentifiers(identifierValues) : new Identifiers();

        var nameValues = response.ValueRanges[4].Values;
        string name = (nameValues?.Count > 0 && nameValues[0].Count > 0)
            ? nameValues[0][0].ToString() ?? string.Empty
            : string.Empty;

        return new BootstrapResponse(templates, todayTotal, todayExpenses, identifiers, name);
    }

    public async Task<string?> GetNameAsync(string spreadsheetId, string sheet)
    {
        var getCellRequest = _sheetService.Spreadsheets.Values.Get(spreadsheetId, $"{sheet}!B1");
        var cellResponse = await getCellRequest.ExecuteAsync();

        if (cellResponse.Values == null || cellResponse.Values.Count != 1)
            return null;

        return cellResponse.Values[0][0]?.ToString();
    }
}