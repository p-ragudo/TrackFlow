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

    public async Task AppendExpenseAsync(string sheet, string category, string tag, decimal amount, string? description = null)
    {
        int nextId = await GetNextIdAsync(sheet + "!K2");

        int targetRow = nextId + 1;
        string specificRange = $"{sheet}!A{targetRow}:H{targetRow}";
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var rowValues = new List<object>
        {
            nextId,
            today.ToString("yyyy-MM-dd"),
            today.ToString("MMMM"),
            today.ToString("dddd"),
            category,
            tag,
            amount,
            description ?? string.Empty
        };

        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { rowValues }
        };

        AppendRequest? appendRequest = _sheetService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, specificRange);

        appendRequest.ValueInputOption = AppendRequest.ValueInputOptionEnum.USERENTERED;
        appendRequest.InsertDataOption = AppendRequest.InsertDataOptionEnum.OVERWRITE;

        AppendValuesResponse response = await appendRequest.ExecuteAsync();
        Console.WriteLine($"Successfully appended row. Updated range: {response.Updates.UpdatedRange}");
    }

    private async Task<int> GetNextIdAsync(string range)
    {
        var cellRequest = _sheetService.Spreadsheets.Values.Get(_spreadsheetId, range);
        ValueRange cellResponse = await cellRequest.ExecuteAsync();

        int nextId = 1;

        if (cellResponse.Values?.Count > 0)
        {
            string? cellValue = cellResponse.Values[0][0]?.ToString();
            if (int.TryParse(cellValue, out int parsedId))
            {
                nextId = parsedId;
            }
        }

        return nextId;
    }
}