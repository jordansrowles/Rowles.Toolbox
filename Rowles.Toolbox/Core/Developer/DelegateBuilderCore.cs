using System.Text;

namespace Rowles.Toolbox.Core.Developer;

public static class DelegateBuilderCore
{
    // ── Types ──────────────────────────────────────────────

    public record struct ModeInfo(string Label, string Icon);
    public record struct Snippet(string Label, string Code);
    public record struct ValidationMsg(string Text, string Icon, string Css);
    public record struct RefRow(string Type, string Equivalent, string Notes);

    // ── Static Data ───────────────────────────────────────

    public static readonly Dictionary<string, ModeInfo> Modes = new()
    {
        ["func"]      = new("Func<>",      "function"),
        ["action"]    = new("Action<>",     "player-play"),
        ["predicate"] = new("Predicate<>",  "filter"),
        ["delegate"]  = new("Custom Delegate", "code"),
    };

    public static readonly string[] CommonTypes =
    [
        "int", "long", "float", "double", "decimal",
        "string", "bool", "char", "byte", "object",
        "DateTime", "DateTimeOffset", "TimeSpan", "Guid",
        "T", "T1", "T2",
        "IEnumerable<T>", "List<T>", "Dictionary<TKey, TValue>",
        "Task", "Task<T>",
        "CancellationToken",
        "Span<T>", "ReadOnlySpan<T>", "Memory<T>",
    ];

    public static readonly RefRow[] ReferenceRows =
    [
        new("Action",                         "delegate void ()",               "0 params"),
        new("Action<T>",                      "delegate void (T)",              "1 param, up to 16"),
        new("Func<TResult>",                  "delegate TResult ()",            "0 params + return"),
        new("Func<T, TResult>",               "delegate TResult (T)",           "1 param + return, up to 16"),
        new("Predicate<T>",                   "Func<T, bool>",                  "1 param, returns bool"),
        new("Converter<TInput, TOutput>",     "Func<TInput, TOutput>",          "Conversion delegate"),
        new("Comparison<T>",                  "Func<T, T, int>",                "Sorting delegate"),
        new("EventHandler",                   "delegate void (object, EventArgs)", "Standard event"),
        new("EventHandler<TEventArgs>",       "delegate void (object, TEventArgs)", "Generic event"),
    ];

    public static readonly HashSet<string> RefStructTypes = new(StringComparer.Ordinal)
    {
        "Span<T>", "ReadOnlySpan<T>",
    };

    // ── Helpers ────────────────────────────────────────────

    public static string DefaultParamName(int index) => index switch
    {
        0 => "x",
        1 => "y",
        2 => "z",
        _ => $"p{index + 1}",
    };

    public static string WrapInTask(string type) =>
        type == "void" ? "Task" : $"Task<{type}>";

    public static string BuildLambdaBody(string returnType, List<string> paramNames)
    {
        if (returnType == "bool" && paramNames.Count > 0)
            return $"return {paramNames[0]} != null;";
        if (returnType == "int" && paramNames.Count > 0)
            return $"return {paramNames[0]};";
        if (returnType == "string")
            return paramNames.Count > 0
                ? $"return {paramNames[0]}.ToString();"
                : "return \"result\";";
        return "return default;";
    }

    // ── Validation ─────────────────────────────────────────

    public static List<ValidationMsg> GetValidations(
        string mode, int paramCount, string returnType,
        IEnumerable<string> resolvedParamTypes)
    {
        var msgs = new List<ValidationMsg>();

        if (paramCount > 16 && mode is "func" or "action")
            msgs.Add(new("Func<> and Action<> support a maximum of 16 type parameters.", "alert-triangle",
                "border-amber-300 dark:border-amber-700 bg-amber-50 dark:bg-amber-900/20 text-amber-700 dark:text-amber-400"));

        if (mode is "func" or "action" or "predicate")
        {
            bool hasRefStruct = resolvedParamTypes.Any(t => RefStructTypes.Contains(t));
            if (mode is "func" or "predicate" && RefStructTypes.Contains(returnType))
                hasRefStruct = true;

            if (hasRefStruct)
                msgs.Add(new("Span<T> and ReadOnlySpan<T> are ref structs and cannot be used as generic type arguments for Func<>/Action<>. Use a custom delegate instead.", "alert-circle",
                    "border-red-300 dark:border-red-700 bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-400"));
        }

        if (mode == "predicate")
            msgs.Add(new("Predicate<T> is equivalent to Func<T, bool>. Consider using Func<T, bool> for better LINQ compatibility.", "info-circle",
                "border-blue-300 dark:border-blue-700 bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-400"));

        return msgs;
    }

    // ── Code Generation ────────────────────────────────────

    public static List<Snippet> GenerateFuncSnippets(
        IReadOnlyList<string> paramTypes,
        IReadOnlyList<string> paramNames,
        string resolvedReturnType,
        bool isAsync)
    {
        var result = new List<Snippet>();
        string ret = resolvedReturnType;
        string asyncRet = isAsync ? WrapInTask(ret) : ret;
        var allArgs = new List<string>(paramTypes) { asyncRet };
        string typeDecl = $"Func<{string.Join(", ", allArgs)}>";

        result.Add(new("Type declaration", typeDecl));

        string lambdaParams = paramNames.Count == 0 ? "()" :
            paramNames.Count == 1 ? paramNames[0] :
            $"({string.Join(", ", paramNames)})";
        result.Add(new("Variable declaration", $"{typeDecl} myFunc = {lambdaParams} => ...;"));

        result.Add(new("Method parameter usage", $"void DoWork({typeDecl} handler)\n{{\n    var result = handler({string.Join(", ", paramNames.Select(n => n + "Value"))});\n}}"));

        string typedParams = string.Join(", ", paramTypes.Zip(paramNames, (t, n) => $"{t} {n}"));
        string lambdaBody = BuildLambdaBody(ret, paramNames.ToList());
        if (isAsync)
        {
            result.Add(new("Async lambda example", $"async ({typedParams}) =>\n{{\n    await Task.Delay(100);\n    {lambdaBody}\n}}"));
        }
        else
        {
            if (paramNames.Count == 0)
                result.Add(new("Lambda example", $"() => {{ {lambdaBody} }}"));
            else
                result.Add(new("Lambda example", $"({typedParams}) => {{ {lambdaBody} }}"));
        }

        if (paramNames.Count <= 1 && !isAsync)
        {
            string methodGroupExample = paramNames.Count == 0
                ? $"Func<{asyncRet}> getter = GetValue;"
                : $"Func<{string.Join(", ", allArgs)}> converter = Convert;";
            result.Add(new("Method group example", methodGroupExample));
        }

        return result;
    }

    public static List<Snippet> GenerateActionSnippets(
        IReadOnlyList<string> paramTypes,
        IReadOnlyList<string> paramNames,
        bool isAsync)
    {
        var result = new List<Snippet>();

        string typeDecl = paramTypes.Count == 0
            ? (isAsync ? "Func<Task>" : "Action")
            : (isAsync
                ? $"Func<{string.Join(", ", paramTypes)}, Task>"
                : $"Action<{string.Join(", ", paramTypes)}>");

        result.Add(new("Type declaration", typeDecl));

        string lambdaParams = paramNames.Count == 0 ? "()" :
            paramNames.Count == 1 ? paramNames[0] :
            $"({string.Join(", ", paramNames)})";

        result.Add(new("Variable declaration", $"{typeDecl} myAction = {lambdaParams} => {{ ... }};"));

        result.Add(new("Method parameter usage", $"void DoWork({typeDecl} callback)\n{{\n    callback({string.Join(", ", paramNames.Select(n => n + "Value"))});\n}}"));

        string typedParams = string.Join(", ", paramTypes.Zip(paramNames, (t, n) => $"{t} {n}"));
        if (isAsync)
        {
            result.Add(new("Async lambda example", $"async ({typedParams}) =>\n{{\n    await Task.Delay(100);\n    Console.WriteLine(\"done\");\n}}"));
        }
        else
        {
            string lp = paramNames.Count == 0 ? "()" : $"({typedParams})";
            result.Add(new("Lambda example", $"{lp} => {{ Console.WriteLine(\"executed\"); }}"));
        }

        if (paramNames.Count <= 1 && !isAsync)
        {
            string mg = paramNames.Count == 0
                ? "Action doWork = Console.Clear;"
                : $"{typeDecl} printer = Console.WriteLine;";
            result.Add(new("Method group example", mg));
        }

        return result;
    }

    public static List<Snippet> GeneratePredicateSnippets(
        string paramType,
        string paramName)
    {
        var result = new List<Snippet>();

        string typeDecl = $"Predicate<{paramType}>";
        result.Add(new("Type declaration", typeDecl));

        string funcEquiv = $"Func<{paramType}, bool>";
        result.Add(new("Func<> equivalent (LINQ-compatible)", funcEquiv));

        result.Add(new("Variable declaration", $"Predicate<{paramType}> predicate = {paramName} => {paramName} != null;"));

        result.Add(new("Method parameter usage", $"void Filter(Predicate<{paramType}> match)\n{{\n    bool result = match({paramName}Value);\n}}"));

        result.Add(new("Lambda example", $"({paramType} {paramName}) => {{ return {paramName} != null; }}"));

        result.Add(new("Usage with List<T>.FindAll", $"var items = list.FindAll({paramName} => {paramName} != null);"));

        return result;
    }

    public static List<Snippet> GenerateDelegateSnippets(
        IReadOnlyList<string> paramTypesWithModifier,
        IReadOnlyList<string> paramNames,
        string resolvedReturnType,
        bool isAsync,
        string accessModifier,
        string delegateName)
    {
        var result = new List<Snippet>();
        var name = string.IsNullOrWhiteSpace(delegateName) ? "MyDelegate" : delegateName;
        var ret = resolvedReturnType;
        var asyncRet = isAsync ? WrapInTask(ret) : ret;
        var displayRet = asyncRet == "void" ? "void" : asyncRet;

        var paramList = string.Join(", ",
            paramTypesWithModifier.Zip(paramNames, (t, n) => $"{t} {n}"));

        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Represents a method that {(ret == "void" ? "performs an action" : $"returns {ret}")} with {paramNames.Count} parameter(s).");
        sb.AppendLine("/// </summary>");
        sb.Append($"{accessModifier} delegate {displayRet} {name}({paramList});");
        result.Add(new("Delegate declaration", sb.ToString()));

        string lambdaParams = paramNames.Count == 0 ? "()" :
            paramNames.Count == 1 ? paramNames[0] :
            $"({string.Join(", ", paramNames)})";
        result.Add(new("Variable declaration", $"{name} handler = {lambdaParams} => {{ ... }};"));

        result.Add(new("Method parameter usage", $"void Execute({name} handler)\n{{\n    {(ret == "void" ? "" : "var result = ")}handler({string.Join(", ", paramNames.Select(n => n + "Value"))});\n}}"));

        string typedParams = string.Join(", ",
            paramTypesWithModifier.Zip(paramNames, (t, n) => $"{t} {n}"));
        if (isAsync)
        {
            string body = ret == "void"
                ? "await Task.Delay(100);"
                : $"await Task.Delay(100);\n    return default;";
            result.Add(new("Async lambda example", $"async ({typedParams}) =>\n{{\n    {body}\n}}"));
        }
        else
        {
            string body = ret == "void"
                ? "Console.WriteLine(\"executed\");"
                : "return default;";
            string lp = paramNames.Count == 0 ? "()" : $"({typedParams})";
            result.Add(new("Lambda example", $"{lp} => {{ {body} }}"));
        }

        if (ret == "void")
        {
            result.Add(new("Event-style usage", $"event {name}? OnExecuted;\n\nprotected void RaiseExecuted({paramList})\n{{\n    OnExecuted?.Invoke({string.Join(", ", paramNames)});\n}}"));
        }

        return result;
    }
}
