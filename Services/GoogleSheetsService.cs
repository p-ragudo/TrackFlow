using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace TrackFlow.Services;

public class GoogleSheetsService
{
    private const string ApplicationName = "GoogleSheets Backend API";
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
            HttpClientInitializer = _credential,
            ApplicationName = ApplicationName,
        });
    }

    public async Task AppendExpenseAsync(string category, decimal amount, string description)
    {
        // 3. Define the sheet name and columns to target
        // "Sheet1!A:D" means target Sheet1 and dynamically find the next empty row from columns A to D
        const string range = "Sheet1!A:D";

        // 4. Prepare the row data
        var rowValues = new List<object>
        {
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // Column A: Timestamp
            category,                                    // Column B: Category (e.g., Food)
            amount,                                      // Column C: Amount
            description                                  // Column D: Notes
        };

        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { rowValues }
        };

        // 5. Configure the request to append data to the bottom of the table
        AppendRequest? appendRequest = _sheetService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);

        // USER_ENTERED parses strings into native types (e.g., formats numbers as currency, dates as text)
        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

        // Insert data into new rows rather than overwriting existing data cells
        appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

        // No formatting, so it won't be bold
        appendRequest.ResponseValueRenderOption = SpreadsheetsResource.ValuesResource.AppendRequest.ResponseValueRenderOptionEnum.UNFORMATTEDVALUE;

        // 6. Execute the write operation asynchronously
        AppendValuesResponse response = await appendRequest.ExecuteAsync();
        Console.WriteLine($"Successfully appended row. Updated range: {response.Updates.UpdatedRange}");
    }
}