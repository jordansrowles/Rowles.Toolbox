namespace Rowles.Toolbox.Core.Developer;

public static class SpanSliceCalculatorCore
{
    public const int MaxDisplayCells = 64;

    // ── Types ──────────────────────────────────────────────

    public enum SliceMethod
    {
        SpanSliceStart,
        SpanSliceStartLength,
        RangeIndexSyntax,
        ArraySegmentOffsetCount,
        StringSubstring,
        MemorySlice
    }

    public sealed record EquivalentExpression(string Api, string Code);
    public sealed record EdgeCaseRow(string Description, string Expression, string Result, bool IsValid);

    public sealed record ValidationResult(
        bool IsValid,
        int ResolvedStart,
        int ResolvedEnd,
        string ExceptionType,
        string ExceptionMessage);

    // ── Display Helpers ───────────────────────────────────

    public static string GetMethodDisplayName(SliceMethod method) => method switch
    {
        SliceMethod.SpanSliceStart => "Span<T>.Slice(start)",
        SliceMethod.SpanSliceStartLength => "Span<T>.Slice(start, length)",
        SliceMethod.RangeIndexSyntax => "array[start..end]",
        SliceMethod.ArraySegmentOffsetCount => "ArraySegment<T>(array, offset, count)",
        SliceMethod.StringSubstring => "string.Substring(startIndex, length)",
        SliceMethod.MemorySlice => "Memory<T>.Slice(start, length)",
        _ => method.ToString()
    };

    public static string GetMethodSignature(SliceMethod method, int bufferLength, int startIndex, int count)
    {
        string len = bufferLength.ToString();
        string s = startIndex.ToString();
        string c = count.ToString();
        return method switch
        {
            SliceMethod.SpanSliceStart =>
                $"new T[{len}].AsSpan().Slice({s})",
            SliceMethod.SpanSliceStartLength =>
                $"new T[{len}].AsSpan().Slice({s}, {c})",
            SliceMethod.RangeIndexSyntax =>
                $"new T[{len}][{s}..{c}]",
            SliceMethod.ArraySegmentOffsetCount =>
                $"new ArraySegment<T>(new T[{len}], {s}, {c})",
            SliceMethod.StringSubstring =>
                $"new string('x', {len}).Substring({s}, {c})",
            SliceMethod.MemorySlice =>
                $"new T[{len}].AsMemory().Slice({s}, {c})",
            _ => string.Empty
        };
    }

    public static string GetStartLabel(SliceMethod method) => method switch
    {
        SliceMethod.RangeIndexSyntax => "Start Index",
        SliceMethod.ArraySegmentOffsetCount => "Offset",
        SliceMethod.StringSubstring => "Start Index",
        _ => "Start / Offset"
    };

    public static string GetSecondParamLabel(SliceMethod method) => method switch
    {
        SliceMethod.RangeIndexSyntax => "End Index (exclusive)",
        SliceMethod.ArraySegmentOffsetCount => "Count",
        SliceMethod.StringSubstring => "Length",
        _ => "Length / Count"
    };

    // ── Validation ────────────────────────────────────────

    public static ValidationResult ValidateSlice(SliceMethod method, int bufferLength, int startIndex, int count)
    {
        bool isValid = true;
        int resolvedStart = 0;
        int resolvedEnd = 0;
        string exceptionType = string.Empty;
        string exceptionMessage = string.Empty;

        void SetException(string type, string message)
        {
            isValid = false;
            exceptionType = type;
            exceptionMessage = message;
        }

        switch (method)
        {
            case SliceMethod.SpanSliceStart:
                if ((uint)startIndex > (uint)bufferLength)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        startIndex < 0
                            ? "start ('start') must be a non-negative value. (Parameter 'start')"
                            : "start ('start') must be less than or equal to the length of the span. (Parameter 'start')");
                }
                if (isValid)
                {
                    resolvedStart = startIndex;
                    resolvedEnd = bufferLength;
                }
                break;

            case SliceMethod.SpanSliceStartLength:
            case SliceMethod.MemorySlice:
                if ((uint)startIndex > (uint)bufferLength)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        startIndex < 0
                            ? "start ('start') must be a non-negative value. (Parameter 'start')"
                            : "start ('start') must be less than or equal to the length of the span. (Parameter 'start')");
                }
                else if ((uint)count > (uint)(bufferLength - startIndex))
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        count < 0
                            ? "length ('length') must be a non-negative value. (Parameter 'length')"
                            : "Specified argument was out of the range of valid values. (Parameter 'length')");
                }
                if (isValid)
                {
                    resolvedStart = startIndex;
                    resolvedEnd = startIndex + count;
                }
                break;

            case SliceMethod.RangeIndexSyntax:
                if (startIndex < 0)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        "Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'start')");
                }
                else if (count < 0)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        "Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'end')");
                }
                else if (startIndex > bufferLength)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        "Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'start')");
                }
                else if (count > bufferLength)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        "Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'end')");
                }
                else if (count < startIndex)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        "Specified argument was out of the range of valid values. (Parameter 'length')");
                }
                if (isValid)
                {
                    resolvedStart = startIndex;
                    resolvedEnd = count;
                }
                break;

            case SliceMethod.ArraySegmentOffsetCount:
                if (startIndex < 0)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        "Non-negative number required. (Parameter 'offset')");
                }
                else if (count < 0)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        "Non-negative number required. (Parameter 'count')");
                }
                else if (bufferLength - startIndex < count)
                {
                    SetException(
                        "System.ArgumentException",
                        "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
                }
                if (isValid)
                {
                    resolvedStart = startIndex;
                    resolvedEnd = startIndex + count;
                }
                break;

            case SliceMethod.StringSubstring:
                if (startIndex < 0)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        "StartIndex cannot be less than zero. (Parameter 'startIndex')");
                }
                else if (startIndex > bufferLength)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        "startIndex cannot be larger than length of string. (Parameter 'startIndex')");
                }
                else if (count < 0)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        "Length cannot be less than zero. (Parameter 'length')");
                }
                else if (startIndex + count > bufferLength)
                {
                    SetException(
                        "System.ArgumentOutOfRangeException",
                        "Index and length must refer to a location within the string. (Parameter 'length')");
                }
                if (isValid)
                {
                    resolvedStart = startIndex;
                    resolvedEnd = startIndex + count;
                }
                break;
        }

        if (!isValid)
        {
            resolvedStart = Math.Max(0, startIndex);
            resolvedEnd = method == SliceMethod.RangeIndexSyntax
                ? Math.Max(resolvedStart, count)
                : resolvedStart + Math.Max(0, count);
            if (method == SliceMethod.SpanSliceStart)
            {
                resolvedEnd = bufferLength;
            }
        }

        return new ValidationResult(isValid, resolvedStart, resolvedEnd, exceptionType, exceptionMessage);
    }

    // ── Cell Display ──────────────────────────────────────

    public static string GetCellClass(int idx, int bufferLength, bool isValid, int resolvedStart, int resolvedEnd)
    {
        bool inSlice = isValid && idx >= resolvedStart && idx < resolvedEnd;
        bool outOfBounds = idx >= bufferLength;

        if (outOfBounds)
        {
            return "bg-red-100 dark:bg-red-900/40 border-red-400 dark:border-red-600";
        }
        if (inSlice)
        {
            return "bg-blue-100 dark:bg-blue-900/60 border-blue-400 dark:border-blue-500 text-blue-700 dark:text-blue-300 font-bold";
        }
        return "bg-gray-50 dark:bg-gray-800 text-gray-500 dark:text-gray-400";
    }

    public static string GetCellTooltip(int idx, int bufferLength, bool isValid, int resolvedStart, int resolvedEnd)
    {
        if (idx >= bufferLength)
        {
            return $"Index {idx}: out of bounds (buffer length is {bufferLength})";
        }
        bool inSlice = isValid && idx >= resolvedStart && idx < resolvedEnd;
        return inSlice
            ? $"Index {idx}: included in slice"
            : $"Index {idx}: not in slice";
    }

    // ── Equivalent Expressions ────────────────────────────

    public static List<EquivalentExpression> BuildEquivalentExpressions(
        int bufferLength, int resolvedStart, int resolvedEnd, bool isValid)
    {
        var expressions = new List<EquivalentExpression>();
        if (!isValid) return expressions;

        int start = resolvedStart;
        int end = resolvedEnd;
        int length = end - start;

        expressions.Add(new EquivalentExpression(
            "Span<T>",
            $"span.Slice({start}, {length})"));

        if (length == bufferLength - start)
        {
            expressions.Add(new EquivalentExpression(
                "Span<T>",
                $"span.Slice({start})"));
        }

        expressions.Add(new EquivalentExpression(
            "Range",
            $"array[{start}..{end}]"));

        expressions.Add(new EquivalentExpression(
            "ArraySegment",
            $"new ArraySegment<T>(array, {start}, {length})"));

        expressions.Add(new EquivalentExpression(
            "Substring",
            $"str.Substring({start}, {length})"));

        expressions.Add(new EquivalentExpression(
            "Memory<T>",
            $"memory.Slice({start}, {length})"));

        if (start == 0)
        {
            expressions.Add(new EquivalentExpression(
                "Range",
                $"array[..{end}]"));
        }

        if (end == bufferLength)
        {
            expressions.Add(new EquivalentExpression(
                "Range",
                $"array[{start}..]"));
        }

        if (end == bufferLength && bufferLength > 0)
        {
            expressions.Add(new EquivalentExpression(
                "Range (^n)",
                $"array[^{bufferLength - start}..]"));
        }

        return expressions;
    }

    // ── Edge Cases ────────────────────────────────────────

    public static List<EdgeCaseRow> BuildEdgeCases(int bufferLength)
    {
        var edgeCases = new List<EdgeCaseRow>();
        int len = bufferLength;

        AddEdgeCase(edgeCases, "Empty slice at start", len, 0, 0);
        AddEdgeCase(edgeCases, "Empty slice at end", len, len, 0);
        AddEdgeCase(edgeCases, "Full buffer", len, 0, len);

        if (len > 0)
        {
            AddEdgeCase(edgeCases, "First element only", len, 0, 1);
            AddEdgeCase(edgeCases, "Last element only", len, len - 1, 1);
        }

        AddEdgeCase(edgeCases, "Off-by-one (length+1)", len, 0, len + 1);
        AddEdgeCase(edgeCases, "Start at length", len, len, 0);
        AddEdgeCase(edgeCases, "Start past end", len, len + 1, 0);
        AddEdgeCase(edgeCases, "Negative start", len, -1, 1);
        AddEdgeCase(edgeCases, "Negative length", len, 0, -1);
        AddEdgeCase(edgeCases, "Max overflow", len, len, 1);

        if (len == 0)
        {
            AddEdgeCase(edgeCases, "Zero-length buffer, Slice(0,0)", 0, 0, 0);
            AddEdgeCase(edgeCases, "Zero-length buffer, Slice(0,1)", 0, 0, 1);
            AddEdgeCase(edgeCases, "Zero-length buffer, Slice(1,0)", 0, 1, 0);
        }

        return edgeCases;
    }

    private static void AddEdgeCase(List<EdgeCaseRow> edgeCases, string description, int bufLen, int start, int length)
    {
        string expression = $"Slice({start}, {length})";
        bool valid = true;
        string result;

        if ((uint)start > (uint)bufLen)
        {
            valid = false;
            result = "ArgumentOutOfRangeException";
        }
        else if ((uint)length > (uint)(bufLen - start))
        {
            valid = false;
            result = "ArgumentOutOfRangeException";
        }
        else
        {
            int endIdx = start + length;
            result = $"[{start}..{endIdx}) len={length}";
        }

        edgeCases.Add(new EdgeCaseRow(description, expression, result, valid));
    }
}
