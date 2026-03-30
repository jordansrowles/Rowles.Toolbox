namespace Rowles.Toolbox.Core.Generators;

public static class CronBuilderCore
{
    public sealed record CronPreset(string Name, string Expression);

    public static string BuildField(string mode, string specific, string rangeStart, string rangeEnd, string step) => mode switch
    {
        "specific" => string.IsNullOrWhiteSpace(specific) ? "*" : specific.Trim(),
        "range" => $"{rangeStart}-{rangeEnd}",
        "step" => $"*/{step}",
        _ => "*"
    };

    public static void ParseField(string field, ref string mode, ref string specific,
        ref string rangeStart, ref string rangeEnd, ref string step)
    {
        if (field == "*")
        {
            mode = "every";
        }
        else if (field.StartsWith("*/"))
        {
            mode = "step";
            step = field[2..];
        }
        else if (field.Contains('-') && !field.Contains(','))
        {
            mode = "range";
            string[] rangeParts = field.Split('-');
            rangeStart = rangeParts[0];
            rangeEnd = rangeParts.Length > 1 ? rangeParts[1] : rangeStart;
        }
        else
        {
            mode = "specific";
            specific = field;
        }
    }

    public static bool FieldMatches(string mode, string specific, string rangeStart, string rangeEnd, string step, int value)
    {
        switch (mode)
        {
            case "every":
                return true;
            case "step":
                if (int.TryParse(step, out int stepVal) && stepVal > 0)
                    return value % stepVal == 0;
                return true;
            case "range":
                if (int.TryParse(rangeStart, out int rs) && int.TryParse(rangeEnd, out int re))
                    return value >= rs && value <= re;
                return true;
            case "specific":
                string[] parts = specific.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string part in parts)
                {
                    if (part.Contains('-'))
                    {
                        string[] rangeParts = part.Split('-');
                        if (int.TryParse(rangeParts[0], out int rStart) &&
                            int.TryParse(rangeParts[1], out int rEnd) &&
                            value >= rStart && value <= rEnd)
                            return true;
                    }
                    else if (int.TryParse(part, out int specific_val) && specific_val == value)
                    {
                        return true;
                    }
                }
                return false;
            default:
                return true;
        }
    }

    public static string DescribeExpression(string expr) => expr switch
    {
        "* * * * *" => "Every minute",
        "0 * * * *" => "At the start of every hour",
        "0 0 * * *" => "Daily at midnight",
        "0 12 * * *" => "Daily at noon",
        "0 0 1 * *" => "Monthly on the 1st at midnight",
        "0 0 1 1 *" => "Yearly on January 1st at midnight",
        _ => $"Custom: {expr}"
    };
}
