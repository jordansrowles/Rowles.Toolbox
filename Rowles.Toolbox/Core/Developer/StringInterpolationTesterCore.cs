using System.Text.RegularExpressions;

namespace Rowles.Toolbox.Core.Developer;

public static class StringInterpolationTesterCore
{
    // ── Types ──────────────────────────────────────────────

    public sealed record PresetDefinition(
        string Label,
        string Icon,
        string FormatString,
        (string TypeName, string Value, string ArgName)[] Args);

    // ── Static Data ───────────────────────────────────────

    public static readonly Regex PlaceholderPattern = new(
        @"\{(\d+)(?:,-?\d+)?(?::[^}]*)?\}", RegexOptions.Compiled);

    public static readonly string[] TypeOptions =
        ["String", "Int32", "Double", "DateTime", "Decimal", "Boolean", "TimeSpan", "Guid"];

    public static readonly PresetDefinition[] Presets =
    [
        new("Currency", "ti ti-currency-dollar", "Total: {0:C2}",
            [("Decimal", "4299.50", "total")]),
        new("Date Format", "ti ti-calendar", "Today is {0:dddd, MMMM d, yyyy}",
            [("DateTime", "2024-06-15", "today")]),
        new("Alignment", "ti ti-align-justified", "|{0,-20}|{1,10:N2}|",
            [("String", "Product Name", "product"), ("Double", "49.99", "price")]),
        new("Number Formats", "ti ti-hash", "Hex: {0:X8}, Scientific: {0:E2}, Percent: {0:P1}",
            [("Int32", "255", "value")])
    ];

    public static readonly (string Spec, string Desc, string Example)[] NumericRefData =
    [
        ("C", "Currency", "$1,234.56"),
        ("C2", "Currency (2dp)", "$1,234.56"),
        ("D", "Decimal (int only)", "001234"),
        ("E", "Scientific", "1.234560E+003"),
        ("E2", "Scientific (2dp)", "1.23E+003"),
        ("F", "Fixed-point", "1234.57"),
        ("F4", "Fixed (4dp)", "1234.5670"),
        ("G", "General", "1234.567"),
        ("N", "Number", "1,234.57"),
        ("N0", "Number (0dp)", "1,235"),
        ("P", "Percent", "42.00 %"),
        ("P1", "Percent (1dp)", "42.0 %"),
        ("X", "Hex (int only)", "4D2"),
        ("X8", "Hex (8 digits)", "000004D2")
    ];

    public static readonly (string Spec, string Desc, string Example)[] DateTimeRefData =
    [
        ("d", "Short date", "6/15/2024"),
        ("D", "Long date", "Saturday, June 15, 2024"),
        ("t", "Short time", "2:30 PM"),
        ("T", "Long time", "2:30:00 PM"),
        ("f", "Full (short time)", "Saturday, June 15, 2024 2:30 PM"),
        ("F", "Full (long time)", "Saturday, June 15, 2024 2:30:00 PM"),
        ("g", "General (short)", "6/15/2024 2:30 PM"),
        ("G", "General (long)", "6/15/2024 2:30:00 PM"),
        ("M", "Month/day", "June 15"),
        ("Y", "Year/month", "June 2024"),
        ("o", "Round-trip", "2024-06-15T14:30:00.0000000"),
        ("s", "Sortable", "2024-06-15T14:30:00"),
        ("u", "Universal sortable", "2024-06-15 14:30:00Z"),
        ("yyyy-MM-dd", "Custom date", "2024-06-15"),
        ("HH:mm:ss", "Custom time", "14:30:00"),
        ("ddd MMM d", "Custom short", "Sat Jun 15")
    ];

    public static readonly (string Spec, string Desc, string Example)[] AlignmentRefData =
    [
        ("{0,10}", "Right-align (10 chars)", "     Hello"),
        ("{0,-10}", "Left-align (10 chars)", "Hello     "),
        ("{0,10:N2}", "Right-align + format", "  1,234.56"),
        ("{0,-15:C}", "Left-align + currency", "$1,234.56      ")
    ];

    public static readonly (string Spec, string Desc, string Example)[] CompositeRefData =
    [
        ("{0:N2}", "Number (2dp)", "1,234.56"),
        ("{0,10:C}", "Align + currency", " $1,234.56"),
        ("{0:yyyy-MM-dd}", "Custom date", "2024-06-15"),
        ("{0:000.00}", "Zero-padded", "042.50"),
        ("{0:##.##}", "Digit placeholder", "42.5"),
        ("{0:0.00%}", "Custom percent", "42.50%"),
        ("{0:(###) ###-####}", "Phone mask", "(555) 123-4567")
    ];

    // ── Pure Helpers ──────────────────────────────────────

    public static object ConvertArgument(int index, string typeName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return typeName == "String" ? "" : throw new FormatException(
                "Argument {" + index + "} (" + typeName + ") requires a value");
        }

        switch (typeName)
        {
            case "String":
                return value;

            case "Int32":
                if (int.TryParse(value, out int intVal))
                    return intVal;
                throw new FormatException("Cannot parse '" + value + "' as Int32");

            case "Double":
                if (double.TryParse(value, out double dblVal))
                    return dblVal;
                throw new FormatException("Cannot parse '" + value + "' as Double");

            case "DateTime":
                if (DateTime.TryParse(value, out DateTime dtVal))
                    return dtVal;
                throw new FormatException("Cannot parse '" + value + "' as DateTime");

            case "Decimal":
                if (decimal.TryParse(value, out decimal decVal))
                    return decVal;
                throw new FormatException("Cannot parse '" + value + "' as Decimal");

            case "Boolean":
                if (bool.TryParse(value, out bool boolVal))
                    return boolVal;
                throw new FormatException("Cannot parse '" + value + "' as Boolean");

            case "TimeSpan":
                if (TimeSpan.TryParse(value, out TimeSpan tsVal))
                    return tsVal;
                throw new FormatException("Cannot parse '" + value + "' as TimeSpan");

            case "Guid":
                if (Guid.TryParse(value, out Guid guidVal))
                    return guidVal;
                throw new FormatException("Cannot parse '" + value + "' as Guid");

            default:
                return value;
        }
    }

    public static string GetCodeRepresentation(string typeName, string value)
    {
        return typeName switch
        {
            "String" => "\"" + value + "\"",
            "Int32" => value,
            "Double" => value + "d",
            "DateTime" => "DateTime.Parse(\"" + value + "\")",
            "Decimal" => value + "m",
            "Boolean" => value.ToLowerInvariant(),
            "TimeSpan" => "TimeSpan.Parse(\"" + value + "\")",
            "Guid" => "Guid.Parse(\"" + value + "\")",
            _ => "\"" + value + "\""
        };
    }

    public static string GetPlaceholderForType(string typeName)
    {
        return typeName switch
        {
            "String" => "any text",
            "Int32" => "e.g. 42",
            "Double" => "e.g. 3.14",
            "DateTime" => "e.g. 2024-06-15 or 2024-06-15T14:30:00",
            "Decimal" => "e.g. 99.99",
            "Boolean" => "true or false",
            "TimeSpan" => "e.g. 01:30:00",
            "Guid" => "e.g. 550e8400-e29b-41d4-a716-446655440000",
            _ => ""
        };
    }

    public static string GetDefaultName(int index)
    {
        return "arg" + index;
    }

    public static string GetDefaultValue(string typeName)
    {
        return typeName switch
        {
            "String" => "",
            "Int32" => "0",
            "Double" => "0.0",
            "DateTime" => "2024-06-15",
            "Decimal" => "0.00",
            "Boolean" => "true",
            "TimeSpan" => "00:00:00",
            "Guid" => "00000000-0000-0000-0000-000000000000",
            _ => ""
        };
    }
}
