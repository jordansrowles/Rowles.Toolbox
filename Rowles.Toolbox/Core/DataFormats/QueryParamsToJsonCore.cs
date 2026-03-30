using System.Text.Json;

namespace Rowles.Toolbox.Core.DataFormats;

public static class QueryParamsToJsonCore
{
    public static string GetJsonScalarString(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.String => el.GetString() ?? "",
            JsonValueKind.Number => el.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "",
            _ => el.GetRawText()
        };
    }
}
