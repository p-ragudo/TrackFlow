using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

using Backend.Models;

namespace Backend.Services;

public class GoogleSheetsService
{
    private readonly string _spreadsheetId;
    private readonly GoogleCredential _credential;
    private readonly SheetsService _sheetService;

    public GoogleSheetsService(IConfiguration configuration)
    {
        _spreadsheetId = configuration["GoogleSheets:SpreadsheetId"]
            ?? throw new InvalidOperationException("GoogleSheets:SpreadsheetId is missing");

        _credential = CredentialFactory.FromFile<ServiceAccountCredential>("google-credentials.json").ToGoogleCredential();

        _sheetService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _credential
        });
    }

    private async Task<AppendValuesResponse> AppendAsync(ValueRange values, string spreadsheetId, string range)
    {
        AppendRequest? appendRequest = _sheetService.Spreadsheets.Values.Append(values, spreadsheetId, range);

        appendRequest.ValueInputOption = AppendRequest.ValueInputOptionEnum.USERENTERED;
        appendRequest.InsertDataOption = AppendRequest.InsertDataOptionEnum.OVERWRITE;

        AppendValuesResponse response = await appendRequest.ExecuteAsync();
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

    public async Task<bool> AppendExpenseAsync(string sheet, string category, string tag, decimal amount, string? description = null)
    {
        int? nextRow = await GetNextIntFromCellAsync(_spreadsheetId, $"{sheet}!J2");

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
            AppendValuesResponse response = await AppendAsync(values, _spreadsheetId, range);

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

    public async Task<bool> AppendTemplateAsync(string sheet, string name, string category, string tag, decimal amount, string? description = null)
    {
        int? nextRow = await GetNextIntFromCellAsync(_spreadsheetId, $"{sheet}!H2");

        if (nextRow == null)
        {
            Console.WriteLine("Failed to append template. The next row counter could not be fetched or parsed.");
            return false;
        }

        int? nextId = await GetNextIntFromCellAsync(_spreadsheetId, $"{sheet}!I2");

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
            AppendValuesResponse response = await AppendAsync(values, _spreadsheetId, range);

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
}