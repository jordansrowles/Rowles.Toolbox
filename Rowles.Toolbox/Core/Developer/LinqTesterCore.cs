namespace Rowles.Toolbox.Core.Developer;

public static class LinqTesterCore
{
    // -- Records --

    public sealed record Person(string Name, int Age, string City, decimal Salary);
    public sealed record Order(int Id, string Product, int Quantity, decimal Price, string Category);
    public sealed record PipelineStep(string Method, string Parameter);
    public sealed record GroupSummary(string Key, int Count, decimal? Sum);

    // -- Sample data --

    public static readonly string[] Words =
        ["apple", "banana", "cherry", "date", "elderberry", "fig", "grape", "honeydew", "kiwi", "lemon"];

    public static readonly List<Person> SamplePeople =
    [
        new("Alice",   28, "London",    52_000m),
        new("Bob",     35, "New York",  78_000m),
        new("Charlie", 42, "London",    91_000m),
        new("Diana",   31, "Paris",     64_000m),
        new("Eve",     26, "Berlin",    48_000m),
        new("Frank",   55, "New York",  120_000m),
        new("Grace",   38, "London",    85_000m),
        new("Hank",    29, "Paris",     56_000m),
        new("Ivy",     44, "Berlin",    72_000m),
        new("Jack",    33, "Tokyo",     68_000m),
        new("Karen",   50, "Tokyo",     95_000m),
        new("Leo",     23, "New York",  42_000m),
    ];

    public static readonly List<Order> SampleOrders =
    [
        new(1,  "Laptop",       1,  999.99m, "Electronics"),
        new(2,  "Mouse",        3,  25.50m,  "Electronics"),
        new(3,  "Desk Chair",   1,  349.00m, "Furniture"),
        new(4,  "Notebook",     12, 4.99m,   "Stationery"),
        new(5,  "Monitor",      2,  450.00m, "Electronics"),
        new(6,  "Pen Set",      5,  12.99m,  "Stationery"),
        new(7,  "Standing Desk",1,  599.00m, "Furniture"),
        new(8,  "Webcam",       1,  79.99m,  "Electronics"),
        new(9,  "Bookshelf",    1,  189.00m, "Furniture"),
        new(10, "Headphones",   2,  149.99m, "Electronics"),
        new(11, "Stapler",      3,  8.99m,   "Stationery"),
        new(12, "Keyboard",     2,  69.99m,  "Electronics"),
    ];

    public static readonly string[] AvailableMethods =
    [
        "Where", "Select", "OrderBy", "OrderByDescending",
        "Take", "Skip", "Distinct", "Reverse",
        "GroupBy", "First", "FirstOrDefault", "Last",
        "Count", "Sum", "Average", "Min", "Max",
        "Any", "All", "Contains",
        "Aggregate", "SelectMany", "Zip", "Concat",
        "Union", "Intersect", "Except"
    ];

    // -- Pipeline helpers --

    public static bool NeedsParameter(string method) => method switch
    {
        "Distinct" or "Reverse" or "First" or "FirstOrDefault" or "Last"
            or "Count" or "Sum" or "Average" or "Min" or "Max" => false,
        _ => true,
    };

    public static string GetPlaceholder(string method, string selectedSource) => method switch
    {
        "Where" => selectedSource switch
        {
            "Numbers" => "x > 5, x % 2 == 0",
            "Words"   => "x.Length > 4, x.Contains(\"an\")",
            "People"  => "p.Age > 30, p.City == \"London\"",
            "Orders"  => "o.Price > 50, o.Category == \"Electronics\"",
            _         => "expression"
        },
        "Select" => selectedSource switch
        {
            "Numbers" => "x * 2, x * x",
            "Words"   => "x.ToUpper(), x.Length",
            "People"  => "p.Name, p.Age",
            "Orders"  => "o.Product, o.Price",
            _         => "property or expression"
        },
        "OrderBy" or "OrderByDescending" => selectedSource switch
        {
            "Numbers" => "x",
            "Words"   => "x.Length, x",
            "People"  => "p.Age, p.Name, p.Salary",
            "Orders"  => "o.Price, o.Product",
            _         => "key"
        },
        "GroupBy" => selectedSource switch
        {
            "People" => "p.City",
            "Orders" => "o.Category",
            _        => "key"
        },
        "Take" or "Skip" => "n (e.g. 5)",
        "Any" or "All"   => "predicate",
        "Contains"       => "value",
        "Aggregate"      => "accumulator expression",
        _                => "expression"
    };

    // -- Code preview --

    public static string GenerateCodePreview(string selectedSource, List<PipelineStep> pipeline)
    {
        string source = selectedSource switch
        {
            "Numbers" => "var data = Enumerable.Range(1, 20);",
            "Words"   => "var data = new[] {\n    \"apple\", \"banana\", \"cherry\",\n    \"date\", \"elderberry\", \"fig\",\n    \"grape\", \"honeydew\", \"kiwi\", \"lemon\"\n};",
            "People"  => "var data = people; // List<Person>",
            "Orders"  => "var data = orders; // List<Order>",
            _         => "var data = ...;"
        };

        if (pipeline.Count == 0)
            return source + "\n\n// Add pipeline steps…";

        string paramName = selectedSource switch
        {
            "People" => "p",
            "Orders" => "o",
            _        => "x"
        };

        string chain = "\nvar result = data";
        foreach (PipelineStep step in pipeline)
        {
            string lambda = step.Parameter.Trim();
            chain += step.Method switch
            {
                "Where" or "Any" or "All" =>
                    $"\n    .{step.Method}({paramName} => {(string.IsNullOrEmpty(lambda) ? "/* predicate */" : lambda)})",
                "Select" when lambda.Contains(',') =>
                    $"\n    .{step.Method}({paramName} => new {{ {lambda} }})",
                "Select" =>
                    $"\n    .{step.Method}({paramName} => {(string.IsNullOrEmpty(lambda) ? "/* selector */" : lambda)})",
                "OrderBy" or "OrderByDescending" =>
                    $"\n    .{step.Method}({paramName} => {(string.IsNullOrEmpty(lambda) ? "/* key */" : lambda)})",
                "GroupBy" =>
                    $"\n    .{step.Method}({paramName} => {(string.IsNullOrEmpty(lambda) ? "/* key */" : lambda)})",
                "Take" or "Skip" =>
                    $"\n    .{step.Method}({(string.IsNullOrEmpty(lambda) ? "n" : lambda)})",
                "Contains" =>
                    $"\n    .{step.Method}({(string.IsNullOrEmpty(lambda) ? "value" : lambda)})",
                "Aggregate" =>
                    $"\n    .{step.Method}((acc, {paramName}) => {(string.IsNullOrEmpty(lambda) ? "/* accumulator */" : lambda)})",
                _ =>
                    $"\n    .{step.Method}()"
            };
        }
        chain += ";";
        return source + "\n" + chain;
    }

    // -- Execution engine --

    public static object ApplyStep(object input, PipelineStep step)
    {
        string param = step.Parameter.Trim();

        if (input is string)
            throw new InvalidOperationException($"Cannot apply .{step.Method}() to a scalar result.");

        if (input is List<(string Key, List<object> Items)> groups)
            return ApplyToGroups(groups, step, param);

        List<object> list = input as List<object>
            ?? throw new InvalidOperationException("Unexpected pipeline state.");

        if (list.Count == 0 && IsTerminal(step.Method))
            throw new InvalidOperationException($".{step.Method}() failed: sequence is empty.");

        return step.Method switch
        {
            "Where"              => list.Where(x => EvalPredicate(x, param)).ToList() as object,
            "Select"             => ApplySelect(list, param),
            "OrderBy"            => list.OrderBy(x => EvalKey(x, param)).ToList(),
            "OrderByDescending"  => list.OrderByDescending(x => EvalKey(x, param)).ToList(),
            "Take"               => list.Take(ParseInt(param)).ToList(),
            "Skip"               => list.Skip(ParseInt(param)).ToList(),
            "Distinct"           => list.Distinct().ToList(),
            "Reverse"            => Enumerable.Reverse(list).ToList(),
            "GroupBy"            => ApplyGroupBy(list, param),
            "First"              => list.First(),
            "FirstOrDefault"     => list.FirstOrDefault() ?? "(null)",
            "Last"               => list.Last(),
            "Count"              => list.Count,
            "Sum"                => list.Sum(x => ToDecimal(x)),
            "Average"            => list.Average(x => ToDecimal(x)),
            "Min"                => list.Min(x => ToDecimal(x))!,
            "Max"                => list.Max(x => ToDecimal(x))!,
            "Any" when string.IsNullOrEmpty(param) => list.Any(),
            "Any"                => list.Any(x => EvalPredicate(x, param)),
            "All"                => list.All(x => EvalPredicate(x, param)),
            "Contains"           => list.Any(x => x?.ToString() == param),
            _ => throw new NotSupportedException($"Method '{step.Method}' is not yet supported in the interpreter. Try a simpler expression.")
        };
    }

    public static bool IsTerminal(string method) =>
        method is "First" or "Last";

    private static object ApplyToGroups(List<(string Key, List<object> Items)> groups, PipelineStep step, string param)
    {
        return step.Method switch
        {
            "Select" => ApplyGroupSelect(groups),
            "Count"  => groups.Count,
            "OrderBy" => groups.OrderBy(g => g.Key).ToList() as object,
            "OrderByDescending" => groups.OrderByDescending(g => g.Key).ToList(),
            "Take"   => groups.Take(ParseInt(param)).ToList(),
            "Skip"   => groups.Skip(ParseInt(param)).ToList(),
            _ => throw new NotSupportedException($"Method '.{step.Method}()' is not supported after GroupBy. Try Select, Count, OrderBy, Take, or Skip.")
        };
    }

    private static object ApplyGroupSelect(List<(string Key, List<object> Items)> groups)
    {
        List<object> results = [];
        foreach ((string Key, List<object> Items) g in groups)
        {
            bool hasNumeric = g.Items.Any(i => i is int or decimal or double or float);
            if (hasNumeric)
            {
                decimal sum = g.Items.Sum(i => ToDecimal(i));
                results.Add(new GroupSummary(g.Key, g.Items.Count, sum));
            }
            else
            {
                results.Add(new GroupSummary(g.Key, g.Items.Count, null));
            }
        }
        return results;
    }

    private static object ApplyGroupBy(List<object> list, string param)
    {
        if (string.IsNullOrEmpty(param))
            throw new ArgumentException("GroupBy requires a key expression (e.g. p.City, o.Category).");

        return list
            .GroupBy(x => EvalKey(x, param)?.ToString() ?? "(null)")
            .Select(g => (Key: g.Key, Items: g.ToList()))
            .ToList();
    }

    private static object ApplySelect(List<object> list, string param)
    {
        if (string.IsNullOrEmpty(param))
            throw new ArgumentException("Select requires an expression.");

        if (param.Contains(','))
        {
            string[] props = param.Split(',', StringSplitOptions.TrimEntries);
            return list.Select(x =>
            {
                Dictionary<string, object?> dict = [];
                foreach (string p in props)
                    dict[StripPrefix(p)] = EvalExpression(x, p);
                return (object)dict;
            }).ToList();
        }

        return list.Select(x => EvalExpression(x, param) ?? (object)"(null)").ToList();
    }

    // -- Expression interpreter --

    public static bool EvalPredicate(object item, string expr)
    {
        if (string.IsNullOrWhiteSpace(expr))
            return true;

        if (expr.Contains("&&"))
        {
            string[] parts = expr.Split("&&", StringSplitOptions.TrimEntries);
            return parts.All(p => EvalPredicate(item, p));
        }
        if (expr.Contains("||"))
        {
            string[] parts = expr.Split("||", StringSplitOptions.TrimEntries);
            return parts.Any(p => EvalPredicate(item, p));
        }

        if (TrySplitComparison(expr, out string left, out string op, out string right))
        {
            object? leftVal = EvalExpression(item, left.Trim());
            object? rightVal = ParseLiteral(right.Trim()) ?? EvalExpression(item, right.Trim());

            if (op is "==" or "!=")
            {
                bool eq = string.Equals(leftVal?.ToString(), rightVal?.ToString(), StringComparison.Ordinal);
                return op == "==" ? eq : !eq;
            }

            decimal lNum = ToDecimal(leftVal);
            decimal rNum = ToDecimal(rightVal);
            return op switch
            {
                ">"  => lNum > rNum,
                ">=" => lNum >= rNum,
                "<"  => lNum < rNum,
                "<=" => lNum <= rNum,
                _    => throw new NotSupportedException($"Unknown operator '{op}'")
            };
        }

        if (expr.Contains(".Contains("))
            return EvalStringMethodBool(item, expr, "Contains");
        if (expr.Contains(".StartsWith("))
            return EvalStringMethodBool(item, expr, "StartsWith");
        if (expr.Contains(".EndsWith("))
            return EvalStringMethodBool(item, expr, "EndsWith");

        throw new NotSupportedException($"Cannot interpret predicate: '{expr}'. Use comparisons like x > 5, p.Age > 30, or p.City == \"London\".");
    }

    private static bool EvalStringMethodBool(object item, string expr, string methodName)
    {
        int dotIdx = expr.IndexOf($".{methodName}(", StringComparison.Ordinal);
        string target = expr[..dotIdx].Trim();
        string argPart = expr[(dotIdx + methodName.Length + 2)..];
        argPart = argPart.TrimEnd(')').Trim().Trim('"');

        string? targetVal = EvalExpression(item, target)?.ToString();
        if (targetVal is null) return false;

        return methodName switch
        {
            "Contains"   => targetVal.Contains(argPart, StringComparison.OrdinalIgnoreCase),
            "StartsWith" => targetVal.StartsWith(argPart, StringComparison.OrdinalIgnoreCase),
            "EndsWith"   => targetVal.EndsWith(argPart, StringComparison.OrdinalIgnoreCase),
            _            => false
        };
    }

    public static object? EvalExpression(object item, string expr)
    {
        expr = expr.Trim();

        if (decimal.TryParse(expr, out decimal litNum))
            return litNum;

        if (expr.StartsWith('"') && expr.EndsWith('"'))
            return expr[1..^1];

        if (expr is "x" or "p" or "o")
            return item;

        if (expr.Contains('.'))
        {
            string[] parts = expr.Split('.', 2);
            string propName = parts[1];

            if (propName.EndsWith("()"))
            {
                string method = propName[..^2];
                object? baseVal = EvalExpression(item, parts[0]);
                string? str = baseVal?.ToString();
                return method switch
                {
                    "ToUpper" => str?.ToUpper(),
                    "ToLower" => str?.ToLower(),
                    "Trim"    => str?.Trim(),
                    "ToString" => str,
                    _         => throw new NotSupportedException($"Method '{method}()' is not supported.")
                };
            }

            if (propName == "Length" && item is string s)
                return s.Length;

            if (propName.StartsWith("Contains(") || propName.StartsWith("StartsWith(") || propName.StartsWith("EndsWith("))
            {
                string methodName = propName[..propName.IndexOf('(')];
                return EvalStringMethodBool(item, expr, methodName);
            }

            return GetProperty(item, propName);
        }

        if (TrySplitArithmetic(expr, out string aLeft, out char aOp, out string aRight))
        {
            decimal lv = ToDecimal(EvalExpression(item, aLeft));
            decimal rv = ToDecimal(EvalExpression(item, aRight));
            return aOp switch
            {
                '+' => lv + rv,
                '-' => lv - rv,
                '*' => lv * rv,
                '/' => rv != 0 ? lv / rv : throw new DivideByZeroException(),
                '%' => lv % rv,
                _   => throw new NotSupportedException($"Arithmetic operator '{aOp}' is not supported.")
            };
        }

        throw new NotSupportedException($"Cannot interpret expression: '{expr}'.");
    }

    public static object? GetProperty(object item, string propName)
    {
        return item switch
        {
            Person p => propName switch
            {
                "Name"   => p.Name,
                "Age"    => p.Age,
                "City"   => p.City,
                "Salary" => p.Salary,
                _        => throw new ArgumentException($"Person has no property '{propName}'. Available: Name, Age, City, Salary.")
            },
            Order o => propName switch
            {
                "Id"       => o.Id,
                "Product"  => o.Product,
                "Quantity" => o.Quantity,
                "Price"    => o.Price,
                "Category" => o.Category,
                _          => throw new ArgumentException($"Order has no property '{propName}'. Available: Id, Product, Quantity, Price, Category.")
            },
            _ => throw new NotSupportedException($"Property access '.{propName}' is not supported for this data type.")
        };
    }

    public static object? EvalKey(object item, string expr) => EvalExpression(item, expr);

    public static string StripPrefix(string expr)
    {
        expr = expr.Trim();
        if (expr.Length > 2 && expr[1] == '.')
            return expr[2..];
        return expr;
    }

    // -- Parsing helpers --

    public static bool TrySplitComparison(string expr, out string left, out string op, out string right)
    {
        string[] ops = ["==", "!=", ">=", "<=", ">", "<"];
        foreach (string candidate in ops)
        {
            int idx = FindOperatorOutsideQuotes(expr, candidate);
            if (idx >= 0)
            {
                left = expr[..idx];
                op = candidate;
                right = expr[(idx + candidate.Length)..];
                return true;
            }
        }
        left = op = right = "";
        return false;
    }

    public static int FindOperatorOutsideQuotes(string expr, string op)
    {
        bool inQuote = false;
        for (int i = 0; i <= expr.Length - op.Length; i++)
        {
            if (expr[i] == '"') { inQuote = !inQuote; continue; }
            if (!inQuote && expr[i..(i + op.Length)] == op)
            {
                if (op.Length == 1 && i + 1 < expr.Length && expr[i + 1] == '=')
                    continue;
                if (op == "=" && i > 0 && expr[i - 1] == '=')
                    continue;
                if (op == "=" && i + 1 < expr.Length && expr[i + 1] == '=')
                    continue;
                return i;
            }
        }
        return -1;
    }

    public static bool TrySplitArithmetic(string expr, out string left, out char op, out string right)
    {
        bool inQuote = false;
        int parenDepth = 0;

        foreach (char[] group in new[] { new[] { '+', '-' }, new[] { '*', '/', '%' } })
        {
            for (int i = expr.Length - 1; i > 0; i--)
            {
                char c = expr[i];
                if (c == '"') { inQuote = !inQuote; continue; }
                if (c == ')') { parenDepth++; continue; }
                if (c == '(') { parenDepth--; continue; }
                if (inQuote || parenDepth > 0) continue;
                if (c == '.') continue;

                if (group.Contains(c))
                {
                    string l = expr[..i].Trim();
                    string r = expr[(i + 1)..].Trim();
                    if (l.Length > 0 && r.Length > 0)
                    {
                        left = l;
                        op = c;
                        right = r;
                        return true;
                    }
                }
            }
        }

        left = right = "";
        op = '\0';
        return false;
    }

    public static object? ParseLiteral(string s)
    {
        s = s.Trim();
        if (s.StartsWith('"') && s.EndsWith('"'))
            return s[1..^1];
        if (decimal.TryParse(s, out decimal d))
            return d;
        if (bool.TryParse(s, out bool b))
            return b;
        return null;
    }

    public static decimal ToDecimal(object? value) => value switch
    {
        int i       => i,
        decimal d   => d,
        double dbl  => (decimal)dbl,
        float f     => (decimal)f,
        long l      => l,
        string s when decimal.TryParse(s, out decimal parsed) => parsed,
        bool b      => b ? 1m : 0m,
        _           => throw new InvalidOperationException($"Cannot convert '{value}' (type: {value?.GetType().Name ?? "null"}) to a number.")
    };

    public static int ParseInt(string s)
    {
        if (int.TryParse(s.Trim(), out int result))
            return result;
        throw new ArgumentException($"Expected a number but got '{s}'.");
    }

    // -- Formatting --

    public static string FormatValue(object? value) => value switch
    {
        null    => "(null)",
        decimal d => d.ToString(d == Math.Floor(d) ? "N0" : "N2"),
        bool b  => b.ToString(),
        _       => value.ToString() ?? "(null)"
    };

    public static string FormatItem(object item) => item switch
    {
        Person p => p.Name,
        Order o  => o.Product,
        _        => item.ToString() ?? "(null)"
    };
}
