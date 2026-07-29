using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

using api.Models;
using api.Dto;

namespace api.Utils;

public static class GoogleSheetUtils
{
    public static async Task<AppendValuesResponse> AppendAsync(SheetsService _sheetService, string spreadsheetId, string range, ValueRange valueRange)
    {
        AppendRequest? appendRequest = _sheetService.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);

        appendRequest.ValueInputOption = AppendRequest.ValueInputOptionEnum.USERENTERED;
        appendRequest.InsertDataOption = AppendRequest.InsertDataOptionEnum.OVERWRITE;

        AppendValuesResponse response = await appendRequest.ExecuteAsync();
        return response;
    }

    public static async Task<UpdateValuesResponse> UpdateAsync(SheetsService _sheetService, string spreadsheetId, string range, ValueRange valueRange)
    {
        UpdateRequest? updateRequest = _sheetService.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);

        updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;

        UpdateValuesResponse response = await updateRequest.ExecuteAsync();
        return response;
    }

    public static async Task<string?> GetCellStringAsync(SheetsService _sheetService, string spreadsheetId, string range)
    {
        var getCellRequest = _sheetService.Spreadsheets.Values.Get(spreadsheetId, range);
        var cellResponse = await getCellRequest.ExecuteAsync();

        if (cellResponse.Values == null || cellResponse.Values.Count != 1)
            return null;

        return cellResponse.Values[0][0].ToString();
    }

    public static async Task<IList<IList<object>>?> GetValuesFromRange(SheetsService _sheetService, string spreadsheetId, string range)
    {
        var getCellRequest = _sheetService.Spreadsheets.Values.Get(spreadsheetId, range);
        var cellResponse = await getCellRequest.ExecuteAsync();

        return cellResponse.Values ?? null;
    }

    public static List<TodayExpenseItemResponse> MapValuesToExpense(IList<IList<object>> values)
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

    public static List<Loan>? MapValuesToLoan(IList<IList<object>> values)
    {
        var loans = new List<Loan>();

        if (values == null || values.Count <= 1)
            return loans;

        foreach (var (index, row) in values.Skip(1).Index()) // skips header row
        {
            if (row.Count == 0) continue;

            string dateStr = row[1]?.ToString() ?? string.Empty;
            if (!DateOnly.TryParse(dateStr, out DateOnly date))
            {
                Console.WriteLine($"Error mapping values to loan. Failed to convert string to date at values[{index}]");
                return null;
            }

            loans.Add(new Loan
            {
                Id = int.TryParse(row[0].ToString(), out int id) ? id : -1,
                Date = date,
                Month = row[2].ToString() ?? string.Empty,
                Day = row[3].ToString() ?? string.Empty,
                Name = row[4].ToString() ?? string.Empty,
                Amount = decimal.TryParse(row[5].ToString(), out decimal amount) ? amount : -1,
                Description = row.Count > 6 ? row[6].ToString() ?? string.Empty : string.Empty
            });
        }

        return loans;
    }

    public static List<LoanTemplate>? MapValuesToLoanTemplate(IList<IList<object>> values)
    {
        var loans = new List<LoanTemplate>();

        if (values == null || values.Count <= 1)
            return loans;

        foreach (var (index, row) in values.Skip(1).Index()) // skips header row
        {
            if (row.Count == 0) continue;

            loans.Add(new LoanTemplate
            {
                Id = int.TryParse(row[0].ToString(), out int id) ? id : -1,
                Name = row[1].ToString() ?? string.Empty,
                Amount = decimal.TryParse(row[2].ToString(), out decimal amount) ? amount : -1,
                Description = row.Count > 3 ? row[3].ToString() ?? string.Empty : string.Empty
            });
        }

        return loans;
    }

    public static List<Payment>? MapValuesToPayment(IList<IList<object>> values)
    {
        var payments = new List<Payment>();

        if (values == null || values.Count <= 1)
            return payments;

        foreach (var (index, row) in values.Skip(1).Index()) // skips header row
        {
            if (row.Count == 0) continue;

            string dateStr = row[1]?.ToString() ?? string.Empty;
            if (!DateOnly.TryParse(dateStr, out DateOnly date))
            {
                Console.WriteLine($"Error mapping values to payment. Failed to convert string to date at values[{index}]");
                return null;
            }

            payments.Add(new Payment
            {
                Id = int.TryParse(row[0].ToString(), out int id) ? id : -1,
                Date = date,
                Month = row[2].ToString() ?? string.Empty,
                Day = row[3].ToString() ?? string.Empty,
                Name = row[4].ToString() ?? string.Empty,
                Amount = decimal.TryParse(row[5].ToString(), out decimal amount) ? amount : -1,
                Description = row.Count > 6 ? row[6].ToString() ?? string.Empty : string.Empty
            });
        }

        return payments;
    }

    public static List<PaymentTemplate>? MapValuesToPaymentTemplate(IList<IList<object>> values)
    {
        var payments = new List<PaymentTemplate>();

        if (values == null || values.Count <= 1)
            return payments;

        foreach (var (index, row) in values.Skip(1).Index()) // skips header row
        {
            if (row.Count == 0) continue;

            payments.Add(new PaymentTemplate
            {
                Id = int.TryParse(row[0].ToString(), out int id) ? id : -1,
                Name = row[1].ToString() ?? string.Empty,
                Amount = decimal.TryParse(row[2].ToString(), out decimal amount) ? amount : -1,
                Description = row.Count > 3 ? row[3].ToString() ?? string.Empty : string.Empty
            });
        }

        return payments;
    }


    public static List<ExpenseTemplate> MapValuesToTemplate(IList<IList<object>> values)
    {
        var templates = new List<ExpenseTemplate>();

        if (values == null || values.Count <= 1)
            return templates;

        foreach (var row in values.Skip(1)) // skips header row
        {
            if (row.Count == 0) continue;

            templates.Add(new ExpenseTemplate
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

    public static Identifiers MapValuesToIdentifiers(IList<IList<object>> values)
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

    public static DateOnly GetDateLocal()
    {
        DateTime utcNow = DateTime.UtcNow;

        TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, targetTimeZone);

        return DateOnly.FromDateTime(localDateTime);
    }
}