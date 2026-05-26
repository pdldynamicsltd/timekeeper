using System.Collections.Generic;
using System.Text;

namespace CadentManagement.TimeTracking.Importing;

internal static class CsvImportParser
{
    public static List<Dictionary<string, string>> Parse(string csvContent)
    {
        var result = new List<Dictionary<string, string>>();
        var rows = ParseRows(csvContent);
        if (rows.Count <= 1)
        {
            return result;
        }

        var headers = rows[0];
        for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            if (row.Count == 0)
            {
                continue;
            }

            var dictionary = new Dictionary<string, string>();
            for (var i = 0; i < headers.Count; i++)
            {
                var key = headers[i]?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                dictionary[key] = i < row.Count ? row[i]?.Trim() ?? string.Empty : string.Empty;
            }

            result.Add(dictionary);
        }

        return result;
    }

    private static List<List<string>> ParseRows(string content)
    {
        var rows = new List<List<string>>();
        if (string.IsNullOrWhiteSpace(content))
        {
            return rows;
        }

        var currentRow = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < content.Length; i++)
        {
            var c = content[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < content.Length && content[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (!inQuotes && c == ',')
            {
                currentRow.Add(currentField.ToString());
                currentField.Clear();
                continue;
            }

            if (!inQuotes && (c == '\n' || c == '\r'))
            {
                if (c == '\r' && i + 1 < content.Length && content[i + 1] == '\n')
                {
                    i++;
                }

                currentRow.Add(currentField.ToString());
                currentField.Clear();

                if (currentRow.Count > 1 || !string.IsNullOrWhiteSpace(currentRow[0]))
                {
                    rows.Add(currentRow);
                }

                currentRow = new List<string>();
                continue;
            }

            currentField.Append(c);
        }

        currentRow.Add(currentField.ToString());
        if (currentRow.Count > 1 || !string.IsNullOrWhiteSpace(currentRow[0]))
        {
            rows.Add(currentRow);
        }

        if (rows.Count > 0 && rows[0].Count > 0)
        {
            rows[0][0] = rows[0][0].TrimStart('\uFEFF');
        }

        return rows;
    }
}
