using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

using backend.Models;
using backend.Dto;

namespace backend.Services;

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

    private async Task<int?> GetNextIntFromCellAsync(string spreadsheetId, string range)
    {
        var getCellRequest = _sheetService.Spreadsheets.Values.Get(spreadsheetId, range);
        var cellResponse = await getCellRequest.ExecuteAsync();

        if (cellResponse.Values == null || cellResponse.Values.Count != 1)
        {
            return null;
        }

        if (int.TryParse(cellResponse.Values[0][0]?.ToString(), out int nextRow))
        {
            return nextRow;
        }

        return null;
    }

    private async Task<IList<IList<object>>?> GetValuesFromRange(string spreadsheetId, string range)
    {
        var getCellRequest = _sheetService.Spreadsheets.Values.Get(spreadsheetId, range);
        var cellResponse = await getCellRequest.ExecuteAsync();

        return cellResponse.Values ?? null;
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
                Category = row[2].ToString() ?? string.Empty,
                Tag = row[3].ToString() ?? string.Empty,
                Amount = decimal.TryParse(row[4].ToString(), out decimal amount) ? amount : -1,
                Description = row.Count > 5 ? row[5].ToString() ?? string.Empty : string.Empty
            });
        }

        return templates;
    }

    private static (string LeftPart, string RightPart) SplitSheetRange(string range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            return (string.Empty, string.Empty);
        }

        int colonIndex = range.IndexOf(':');

        if (colonIndex == -1)
        {
            return (range, string.Empty);
        }

        string leftPart = range.Substring(0, colonIndex);
        string rightPart = range.Substring(colonIndex);

        return (leftPart, rightPart);
    }

    public async Task<bool> AppendExpenseAsync(string spreadsheetId, string sheet, string category, string tag, decimal amount, string? description = null)
    {
        int? nextRow = await GetNextIntFromCellAsync(spreadsheetId, $"{sheet}!J2");

        if (nextRow == null)
        {
            Console.WriteLine("Failed to append expense. The next row counter could not be fetched or parsed.");
            return false;
        }

        string range = $"{sheet}!A{nextRow}:G{nextRow}";
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var row = new List<object>
        {
            today.ToString("yyyy-MM-dd"),
            today.ToString("MMMM"),
            today.ToString("dddd"),
            category,
            tag,
            amount,
            description ?? string.Empty
        };

        var values = new ValueRange
        {
            Values = new List<IList<object>> { row }
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

    public async Task<bool> AppendTemplateAsync(string spreadsheetId, string sheet, string name, string category, string tag, decimal amount, string? description = null)
    {
        int? nextRow = await GetNextIntFromCellAsync(spreadsheetId, $"{sheet}!H2");

        if (nextRow == null)
        {
            Console.WriteLine("Failed to append template. The next row counter could not be fetched or parsed.");
            return false;
        }

        int? nextId = await GetNextIntFromCellAsync(spreadsheetId, $"{sheet}!I2");

        if (nextId == null)
        {
            Console.WriteLine("Failed to append template. The next id counter could not be fetched or parsed.");
            return false;
        }

        string range = $"{sheet}!A{nextRow}:F{nextRow}";

        var row = new List<object>
        {
            nextId,
            name,
            category,
            tag,
            amount,
            description ?? string.Empty
        };

        var values = new ValueRange
        {
            Values = new List<IList<object>> { row }
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

    public async Task<List<Template>?> GetTemplatesAsync(string spreadsheetId, string range)
    {
        var values = await GetValuesFromRange(spreadsheetId, range);

        if (values == null || values.Count <= 1)
        {
            return null;
        }

        var templates = MapValuesToTemplate(values);

        return templates;
    }

    public async Task<bool> UpdateTemplateAsync(int id, string spreadsheetId, string range, Template updatedTemplate)
    {
        var values = await GetTemplatesAsync(spreadsheetId, range);

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

        var (LeftPart, RightPart) = SplitSheetRange(range);
        var specificRowRange = $"{LeftPart}{targetRow}{RightPart}{targetRow}";

        var row = new List<object>
        {
            id,
            updatedTemplate.Name,
            updatedTemplate.Category,
            updatedTemplate.Tag,
            updatedTemplate.Amount,
            updatedTemplate.Description
        };

        var valueRange = new ValueRange
        {
            Values = [ row ]
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
}