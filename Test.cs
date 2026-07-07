(string Sheet, string LeftRange, string RightRange) SplitNoRowSheetRange(string range)
{
    if (string.IsNullOrWhiteSpace(range))
        return (string.Empty, string.Empty, string.Empty);

    int exclamationIndex = range.IndexOf('!');
    if (exclamationIndex == -1)
        return (string.Empty, string.Empty, string.Empty);

    int colonIndex = range.IndexOf(':');
    if (colonIndex == -1)
        return (string.Empty, string.Empty, string.Empty);


    string sheet = range.Substring(0, exclamationIndex);
    string leftPart = range.Substring(exclamationIndex + 1, colonIndex - exclamationIndex - 1);
    string rightPart = range.Substring(colonIndex + 1);

    return (sheet, leftPart, rightPart);
}

var result = SplitNoRowSheetRange("Templates!A23:H36");
Console.Write($"{result.Sheet} | {result.LeftRange} | {result.RightRange}");