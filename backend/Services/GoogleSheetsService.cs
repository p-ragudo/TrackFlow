using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace TrackFlow.backend.Services;

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

    public async Task AppendExpenseAsync(string category, decimal amount, string description)
    {
        const string range = "2026!A:D";

        var rowValues = new List<object>
        {
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            category,
            amount,
            description
        };

        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { rowValues }
        };

        AppendRequest? appendRequest = _sheetService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);

        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

        AppendValuesResponse response = await appendRequest.ExecuteAsync();
        Console.WriteLine($"Successfully appended row. Updated range: {response.Updates.UpdatedRange}");
    }
}