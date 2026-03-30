using System.Text;

namespace Rowles.Toolbox.Core.Developer;

public static class StructLayoutVisualiserCore
{
    // ── Types ──────────────────────────────────────────────

    public sealed record TypeInfo(string Name, int Size, int Alignment);
    public sealed record PackOption(string Label, int Value);
    public sealed record ByteCell(int FieldIndex, string FieldName, string ShortLabel, int ByteWithinField, bool IsPadding);
    public sealed record FieldLayoutInfo(int FieldIndex, string Name, string TypeName, int Offset, int Size, int Alignment, int PaddingBefore);
    public sealed record LayoutResult(int TotalSize, int PayloadSize, int PaddingSize, double WastePercent, int LargestAlignment, int TailPadding, List<FieldLayoutInfo> FieldLayouts);

    public sealed class StructField
    {
        public string Name { get; set; } = "";
        public string TypeName { get; set; } = "int";
        public int Size { get; set; } = 4;
        public int Alignment { get; set; } = 4;
    }

    // ── Static Data ───────────────────────────────────────

    public static readonly List<TypeInfo> TypeInfos = new()
    {
        new("byte", 1, 1),
        new("sbyte", 1, 1),
        new("bool", 1, 1),
        new("short", 2, 2),
        new("ushort", 2, 2),
        new("char", 2, 2),
        new("Half", 2, 2),
        new("int", 4, 4),
        new("uint", 4, 4),
        new("float", 4, 4),
        new("long", 8, 8),
        new("ulong", 8, 8),
        new("double", 8, 8),
        new("nint", 8, 8),
        new("nuint", 8, 8),
        new("decimal", 16, 8),
        new("Guid", 16, 4),
        new("Int128", 16, 16)
    };

    public static readonly List<PackOption> PackOptions = new()
    {
        new("Sequential (default)", 0),
        new("Pack=1", 1),
        new("Pack=2", 2),
        new("Pack=4", 4),
        new("Pack=8", 8)
    };

    // ── Field colour palette ──────────────────────────────

    public static readonly string[] FieldBgClasses =
    [
        "bg-blue-100 dark:bg-blue-900/40",
        "bg-rose-100 dark:bg-rose-900/40",
        "bg-emerald-100 dark:bg-emerald-900/40",
        "bg-violet-100 dark:bg-violet-900/40",
        "bg-amber-100 dark:bg-amber-900/40",
        "bg-cyan-100 dark:bg-cyan-900/40",
        "bg-pink-100 dark:bg-pink-900/40",
        "bg-lime-100 dark:bg-lime-900/40",
        "bg-orange-100 dark:bg-orange-900/40",
        "bg-indigo-100 dark:bg-indigo-900/40",
        "bg-teal-100 dark:bg-teal-900/40",
        "bg-fuchsia-100 dark:bg-fuchsia-900/40"
    ];

    public static readonly string[] FieldTextClasses =
    [
        "text-blue-800 dark:text-blue-300",
        "text-rose-800 dark:text-rose-300",
        "text-emerald-800 dark:text-emerald-300",
        "text-violet-800 dark:text-violet-300",
        "text-amber-800 dark:text-amber-300",
        "text-cyan-800 dark:text-cyan-300",
        "text-pink-800 dark:text-pink-300",
        "text-lime-800 dark:text-lime-300",
        "text-orange-800 dark:text-orange-300",
        "text-indigo-800 dark:text-indigo-300",
        "text-teal-800 dark:text-teal-300",
        "text-fuchsia-800 dark:text-fuchsia-300"
    ];

    public static readonly string[] FieldBorderClasses =
    [
        "border-blue-300 dark:border-blue-700",
        "border-rose-300 dark:border-rose-700",
        "border-emerald-300 dark:border-emerald-700",
        "border-violet-300 dark:border-violet-700",
        "border-amber-300 dark:border-amber-700",
        "border-cyan-300 dark:border-cyan-700",
        "border-pink-300 dark:border-pink-700",
        "border-lime-300 dark:border-lime-700",
        "border-orange-300 dark:border-orange-700",
        "border-indigo-300 dark:border-indigo-700",
        "border-teal-300 dark:border-teal-700",
        "border-fuchsia-300 dark:border-fuchsia-700"
    ];

    public static readonly string[] FieldDotClasses =
    [
        "bg-blue-400",
        "bg-rose-400",
        "bg-emerald-400",
        "bg-violet-400",
        "bg-amber-400",
        "bg-cyan-400",
        "bg-pink-400",
        "bg-lime-400",
        "bg-orange-400",
        "bg-indigo-400",
        "bg-teal-400",
        "bg-fuchsia-400"
    ];

    // ── Colour helpers ────────────────────────────────────

    public static string GetFieldColourClass(int fieldIndex) => FieldDotClasses[fieldIndex % FieldDotClasses.Length];
    public static string GetFieldBgClass(int fieldIndex) => FieldBgClasses[fieldIndex % FieldBgClasses.Length];
    public static string GetFieldTextClass(int fieldIndex) => FieldTextClasses[fieldIndex % FieldTextClasses.Length];
    public static string GetFieldBorderClass(int fieldIndex) => FieldBorderClasses[fieldIndex % FieldBorderClasses.Length];

    // ── Layout Computation ────────────────────────────────

    public static LayoutResult ComputeLayout(List<StructField> fields, int pack)
    {
        if (fields.Count == 0)
            return new LayoutResult(0, 0, 0, 0, 0, 0, new());

        int effectivePack = pack <= 0 ? int.MaxValue : pack;
        int currentOffset = 0;
        int largestAlignment = 1;
        int payloadSize = 0;
        int totalPaddingBefore = 0;
        List<FieldLayoutInfo> layoutInfos = new();

        for (int i = 0; i < fields.Count; i++)
        {
            StructField field = fields[i];
            int fieldAlignment = Math.Min(field.Alignment, effectivePack);
            if (fieldAlignment > largestAlignment)
                largestAlignment = fieldAlignment;

            int paddingBefore = 0;
            if (fieldAlignment > 1)
            {
                int remainder = currentOffset % fieldAlignment;
                if (remainder != 0)
                    paddingBefore = fieldAlignment - remainder;
            }

            int fieldOffset = currentOffset + paddingBefore;
            totalPaddingBefore += paddingBefore;

            layoutInfos.Add(new FieldLayoutInfo(i, field.Name, field.TypeName, fieldOffset, field.Size, fieldAlignment, paddingBefore));

            currentOffset = fieldOffset + field.Size;
            payloadSize += field.Size;
        }

        int tailPadding = 0;
        if (largestAlignment > 1)
        {
            int remainder = currentOffset % largestAlignment;
            if (remainder != 0)
                tailPadding = largestAlignment - remainder;
        }

        int totalSize = currentOffset + tailPadding;
        int totalPadding = totalPaddingBefore + tailPadding;
        double wastePct = totalSize > 0 ? (totalPadding * 100.0 / totalSize) : 0;

        return new LayoutResult(totalSize, payloadSize, totalPadding, wastePct, largestAlignment, tailPadding, layoutInfos);
    }

    public static List<ByteCell> BuildByteMap(List<StructField> fields, LayoutResult layout)
    {
        List<ByteCell> map = new();
        if (layout.TotalSize == 0) return map;

        for (int b = 0; b < layout.TotalSize; b++)
        {
            map.Add(new ByteCell(-1, "", "", 0, true));
        }

        foreach (FieldLayoutInfo fli in layout.FieldLayouts)
        {
            string shortName = fli.Name.Length <= 3 ? fli.Name : fli.Name[..2];
            for (int b = 0; b < fli.Size; b++)
            {
                int mapIdx = fli.Offset + b;
                if (mapIdx < map.Count)
                {
                    map[mapIdx] = new ByteCell(fli.FieldIndex, fli.Name, shortName, b, false);
                }
            }
        }

        return map;
    }

    public static string GenerateCode(List<StructField> fields, int selectedPack, LayoutResult layoutResult)
    {
        StringBuilder sb = new();
        sb.AppendLine("using System.Runtime.InteropServices;");
        sb.AppendLine();

        if (selectedPack > 0)
        {
            sb.AppendLine($"[StructLayout(LayoutKind.Sequential, Pack = {selectedPack})]");
        }
        else
        {
            sb.AppendLine("[StructLayout(LayoutKind.Sequential)]");
        }

        sb.AppendLine("public struct MyStruct");
        sb.AppendLine("{");

        foreach (StructField field in fields)
        {
            sb.AppendLine($"    public {field.TypeName} {field.Name};");
        }

        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"// Total size: {layoutResult.TotalSize} bytes");
        sb.AppendLine($"// Payload:    {layoutResult.PayloadSize} bytes");
        sb.AppendLine($"// Padding:    {layoutResult.PaddingSize} bytes ({layoutResult.WastePercent:F1}% waste)");

        return sb.ToString();
    }
}
