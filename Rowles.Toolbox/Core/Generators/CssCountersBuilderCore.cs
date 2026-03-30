namespace Rowles.Toolbox.Core.Generators;

public static class CssCountersBuilderCore
{
    public sealed class CounterDef
    {
        public string Name { get; set; }
        public int InitialValue { get; set; }
        public int IncrementValue { get; set; }

        public CounterDef(string name, int initialValue, int incrementValue)
        {
            Name = name;
            InitialValue = initialValue;
            IncrementValue = incrementValue;
        }
    }

    public sealed class ResetRule
    {
        public string Selector { get; set; }
        public string CounterName { get; set; }
        public int ResetTo { get; set; }

        public ResetRule(string selector, string counterName, int resetTo)
        {
            Selector = selector;
            CounterName = counterName;
            ResetTo = resetTo;
        }
    }

    public sealed class IncrementRule
    {
        public string Selector { get; set; }
        public string CounterName { get; set; }
        public int IncrementBy { get; set; }

        public IncrementRule(string selector, string counterName, int incrementBy)
        {
            Selector = selector;
            CounterName = counterName;
            IncrementBy = incrementBy;
        }
    }

    public sealed class DisplayRule
    {
        public string Selector { get; set; }
        public string ContentTemplate { get; set; }
        public string Style { get; set; }
        public string Separator { get; set; }

        public DisplayRule(string selector, string contentTemplate, string style, string separator)
        {
            Selector = selector;
            ContentTemplate = contentTemplate;
            Style = style;
            Separator = separator;
        }
    }

    public sealed record PreviewLine(int Depth, string Number, string Text);

    public sealed record CounterPreset(
        string Name,
        (string Name, int Init, int Inc)[] Counters,
        (string Selector, string Counter, int To)[] Resets,
        (string Selector, string Counter, int By)[] Increments,
        (string Selector, string Content, string Style, string Sep)[] Displays);

    public static string FormatCounter(int value, string style)
    {
        return style switch
        {
            "decimal-leading-zero" => value.ToString("D2"),
            "lower-roman" => ToRoman(value).ToLowerInvariant(),
            "upper-roman" => ToRoman(value),
            "lower-alpha" => value >= 1 && value <= 26 ? ((char)('a' + value - 1)).ToString() : value.ToString(),
            "upper-alpha" => value >= 1 && value <= 26 ? ((char)('A' + value - 1)).ToString() : value.ToString(),
            "lower-greek" => value >= 1 && value <= 24 ? ((char)('\u03B1' + value - 1)).ToString() : value.ToString(),
            _ => value.ToString()
        };
    }

    public static string ToRoman(int number)
    {
        if (number <= 0 || number > 3999) return number.ToString();
        (int Value, string Numeral)[] romanNumerals =
        [
            (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
            (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
            (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
        ];
        System.Text.StringBuilder sb = new();
        foreach ((int val, string numeral) in romanNumerals)
        {
            while (number >= val)
            {
                sb.Append(numeral);
                number -= val;
            }
        }
        return sb.ToString();
    }

    public static string SanitiseIdent(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "counter";
        string sanitised = System.Text.RegularExpressions.Regex.Replace(value.Trim(), @"[^a-zA-Z0-9_-]", "");
        if (sanitised.Length == 0) return "counter";
        if (char.IsDigit(sanitised[0])) sanitised = "_" + sanitised;
        return sanitised;
    }

    public static int ParseInt(object? value, int fallback)
    {
        if (value is null) return fallback;
        return int.TryParse(value.ToString(), out int result) ? result : fallback;
    }

    public static string GenerateCss(List<ResetRule> resetRules, List<IncrementRule> incrementRules, List<DisplayRule> displayRules)
    {
        System.Text.StringBuilder sb = new();

        Dictionary<string, List<ResetRule>> resetsBySelector = resetRules
            .Where(r => !string.IsNullOrWhiteSpace(r.Selector) && !string.IsNullOrWhiteSpace(r.CounterName))
            .GroupBy(r => r.Selector)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (KeyValuePair<string, List<ResetRule>> group in resetsBySelector)
        {
            sb.AppendLine($"{group.Key} {{");
            List<string> parts = new();
            foreach (ResetRule rule in group.Value)
            {
                string resetValue = rule.ResetTo != 0 ? $" {rule.ResetTo}" : string.Empty;
                parts.Add($"{rule.CounterName}{resetValue}");
            }
            sb.AppendLine($"  counter-reset: {string.Join(" ", parts)};");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        Dictionary<string, List<IncrementRule>> incsBySelector = incrementRules
            .Where(r => !string.IsNullOrWhiteSpace(r.Selector) && !string.IsNullOrWhiteSpace(r.CounterName))
            .GroupBy(r => r.Selector)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (KeyValuePair<string, List<IncrementRule>> group in incsBySelector)
        {
            sb.AppendLine($"{group.Key} {{");
            List<string> parts = new();
            foreach (IncrementRule rule in group.Value)
            {
                string incValue = rule.IncrementBy != 1 ? $" {rule.IncrementBy}" : string.Empty;
                parts.Add($"{rule.CounterName}{incValue}");
            }
            sb.AppendLine($"  counter-increment: {string.Join(" ", parts)};");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        foreach (DisplayRule rule in displayRules)
        {
            if (string.IsNullOrWhiteSpace(rule.Selector) || string.IsNullOrWhiteSpace(rule.ContentTemplate))
                continue;

            sb.AppendLine($"{rule.Selector} {{");
            string contentValue = BuildContentValue(rule);
            sb.AppendLine($"  content: {contentValue};");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        string result = sb.ToString().TrimEnd();
        return string.IsNullOrWhiteSpace(result) ? "/* Add counters and rules to generate CSS */" : result;
    }

    public static string BuildContentValue(DisplayRule rule)
    {
        string template = rule.ContentTemplate.Trim();

        if (!string.IsNullOrWhiteSpace(template))
        {
            if (rule.Style != "decimal")
            {
                template = System.Text.RegularExpressions.Regex.Replace(
                    template,
                    @"counter\((\w+)\)",
                    m => $"counter({m.Groups[1].Value}, {rule.Style})");

                template = System.Text.RegularExpressions.Regex.Replace(
                    template,
                    @"counters\((\w+),\s*""([^""]*)""\)",
                    m => $"counters({m.Groups[1].Value}, \"{m.Groups[2].Value}\", {rule.Style})");
            }
            return template;
        }

        return "\"\"";
    }

    public static List<PreviewLine> GeneratePreview(
        List<CounterDef> counters, List<IncrementRule> incrementRules,
        List<DisplayRule> displayRules)
    {
        List<PreviewLine> lines = new();

        if (counters.Count == 0 || incrementRules.Count == 0 || displayRules.Count == 0)
            return lines;

        List<string> orderedCounters = incrementRules
            .Where(r => !string.IsNullOrWhiteSpace(r.CounterName))
            .Select(r => r.CounterName)
            .Distinct()
            .ToList();

        if (orderedCounters.Count == 0)
            return lines;

        Dictionary<string, int> counterValues = new();
        foreach (CounterDef c in counters)
        {
            counterValues[c.Name] = c.InitialValue;
        }

        if (orderedCounters.Count == 1)
        {
            string counterName = orderedCounters[0];
            CounterDef? def = counters.FirstOrDefault(c => c.Name == counterName);
            int inc = def?.IncrementValue ?? 1;

            string[] sampleLabels = ["First item", "Second item", "Third item", "Fourth item"];
            for (int i = 0; i < 4; i++)
            {
                counterValues[counterName] += inc;
                string number = FormatCounter(counterValues[counterName], GetStyleForCounter(counterName, displayRules));
                lines.Add(new PreviewLine(0, number + GetSeparatorText(counterName, displayRules), sampleLabels[i]));
            }
        }
        else if (orderedCounters.Count == 2)
        {
            string parent = orderedCounters[0];
            string child = orderedCounters[1];
            CounterDef? parentDef = counters.FirstOrDefault(c => c.Name == parent);
            CounterDef? childDef = counters.FirstOrDefault(c => c.Name == child);
            int parentInc = parentDef?.IncrementValue ?? 1;
            int childInc = childDef?.IncrementValue ?? 1;

            string[][] sampleLabels =
            [
                ["First heading", "Sub-heading A", "Sub-heading B"],
                ["Second heading", "Sub-heading C", "Sub-heading D"],
                ["Third heading", "Sub-heading E"]
            ];

            foreach (string[] group in sampleLabels)
            {
                counterValues[parent] += parentInc;
                counterValues[child] = childDef?.InitialValue ?? 0;
                string parentNum = FormatCounter(counterValues[parent], GetStyleForCounter(parent, displayRules));
                lines.Add(new PreviewLine(0, parentNum + GetSeparatorText(parent, displayRules), group[0]));

                for (int j = 1; j < group.Length; j++)
                {
                    counterValues[child] += childInc;
                    string childNum = FormatCounter(counterValues[parent], GetStyleForCounter(parent, displayRules)) + "." +
                                     FormatCounter(counterValues[child], GetStyleForCounter(child, displayRules));
                    lines.Add(new PreviewLine(1, childNum + GetSeparatorText(child, displayRules), group[j]));
                }
            }
        }
        else
        {
            string l1 = orderedCounters[0];
            string l2 = orderedCounters[1];
            string l3 = orderedCounters.Count > 2 ? orderedCounters[2] : orderedCounters[1];
            CounterDef? l1Def = counters.FirstOrDefault(c => c.Name == l1);
            CounterDef? l2Def = counters.FirstOrDefault(c => c.Name == l2);
            CounterDef? l3Def = counters.FirstOrDefault(c => c.Name == l3);
            int l1Inc = l1Def?.IncrementValue ?? 1;
            int l2Inc = l2Def?.IncrementValue ?? 1;
            int l3Inc = l3Def?.IncrementValue ?? 1;

            counterValues[l1] += l1Inc;
            counterValues[l2] = l2Def?.InitialValue ?? 0;
            string n1 = FormatCounter(counterValues[l1], GetStyleForCounter(l1, displayRules));
            lines.Add(new PreviewLine(0, n1 + ".", "Section"));

            counterValues[l2] += l2Inc;
            counterValues[l3] = l3Def?.InitialValue ?? 0;
            lines.Add(new PreviewLine(1, $"{n1}.{FormatCounter(counterValues[l2], GetStyleForCounter(l2, displayRules))}", "Subsection"));

            counterValues[l3] += l3Inc;
            lines.Add(new PreviewLine(2, $"{n1}.{FormatCounter(counterValues[l2], GetStyleForCounter(l2, displayRules))}.{FormatCounter(counterValues[l3], GetStyleForCounter(l3, displayRules))}", "Clause"));

            counterValues[l3] += l3Inc;
            lines.Add(new PreviewLine(2, $"{n1}.{FormatCounter(counterValues[l2], GetStyleForCounter(l2, displayRules))}.{FormatCounter(counterValues[l3], GetStyleForCounter(l3, displayRules))}", "Clause"));

            counterValues[l2] += l2Inc;
            counterValues[l3] = l3Def?.InitialValue ?? 0;
            lines.Add(new PreviewLine(1, $"{n1}.{FormatCounter(counterValues[l2], GetStyleForCounter(l2, displayRules))}", "Subsection"));

            counterValues[l1] += l1Inc;
            counterValues[l2] = l2Def?.InitialValue ?? 0;
            string n2 = FormatCounter(counterValues[l1], GetStyleForCounter(l1, displayRules));
            lines.Add(new PreviewLine(0, n2 + ".", "Section"));

            counterValues[l2] += l2Inc;
            counterValues[l3] = l3Def?.InitialValue ?? 0;
            lines.Add(new PreviewLine(1, $"{n2}.{FormatCounter(counterValues[l2], GetStyleForCounter(l2, displayRules))}", "Subsection"));

            counterValues[l3] += l3Inc;
            lines.Add(new PreviewLine(2, $"{n2}.{FormatCounter(counterValues[l2], GetStyleForCounter(l2, displayRules))}.{FormatCounter(counterValues[l3], GetStyleForCounter(l3, displayRules))}", "Clause"));
        }

        return lines;
    }

    public static string GetStyleForCounter(string counterName, List<DisplayRule> displayRules)
    {
        DisplayRule? rule = displayRules.FirstOrDefault(d =>
            d.ContentTemplate.Contains($"counter({counterName}") ||
            d.ContentTemplate.Contains($"counters({counterName}"));
        return rule?.Style ?? "decimal";
    }

    public static string GetSeparatorText(string counterName, List<DisplayRule> displayRules)
    {
        DisplayRule? rule = displayRules.FirstOrDefault(d =>
            d.ContentTemplate.Contains($"counter({counterName}") ||
            d.ContentTemplate.Contains($"counters({counterName}"));
        if (rule is null) return ". ";

        string template = rule.ContentTemplate;
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(template, "\"([^\"]*)\"\\s*$");
        if (match.Success) return match.Groups[1].Value;

        return ". ";
    }
}
