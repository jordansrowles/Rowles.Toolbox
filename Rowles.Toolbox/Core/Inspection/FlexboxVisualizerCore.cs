namespace Rowles.Toolbox.Core.Inspection;

public static class FlexboxVisualizerCore
{
    public static readonly string[] ItemColors =
    [
        "#6366f1", "#f43f5e", "#10b981", "#f59e0b", "#0ea5e9", "#a855f7",
        "#ec4899", "#f97316", "#14b8a6", "#84cc16", "#06b6d4", "#d946ef"
    ];

    public static readonly string[] SizeVariants =
    [
        "Item", "Wider content here", "Tall", "Small", "Medium item",
        "A bit of text", "XL", "Narrow", "Expanded content block", "Tiny", "Standard", "Short"
    ];

    public sealed class FlexItem
    {
        public int Number { get; set; }
        public int FlexGrow { get; set; }
        public int FlexShrink { get; set; } = 1;
        public string FlexBasis { get; set; } = "auto";
        public int Order { get; set; }
        public string AlignSelf { get; set; } = "auto";
    }

    public static string GetContainerStyle(string flexDirection, string flexWrap, string justifyContent,
        string alignItems, string alignContent, int gap)
    {
        return $"display: flex; flex-direction: {flexDirection}; flex-wrap: {flexWrap}; " +
               $"justify-content: {justifyContent}; align-items: {alignItems}; " +
               $"align-content: {alignContent}; gap: {gap}px; " +
               "min-height: 300px; border: 2px dashed #6b7280; padding: 8px; border-radius: 8px; " +
               "background: repeating-linear-gradient(45deg, transparent, transparent 10px, rgba(107,114,128,0.05) 10px, rgba(107,114,128,0.05) 20px);";
    }

    public static string GetItemStyle(FlexItem item, int index)
    {
        string color = ItemColors[index % 12];
        string alignSelf = item.AlignSelf != "auto" ? $"align-self: {item.AlignSelf}; " : "";

        string padding = (index % 3) switch
        {
            0 => "padding: 16px 24px;",
            1 => "padding: 24px 16px;",
            _ => "padding: 20px;"
        };

        return $"flex-grow: {item.FlexGrow}; flex-shrink: {item.FlexShrink}; " +
               $"flex-basis: {item.FlexBasis}; order: {item.Order}; {alignSelf}" +
               $"background: {color}; color: white; border-radius: 8px; {padding} " +
               "display: flex; flex-direction: column; align-items: center; justify-content: center; " +
               "min-width: 40px; min-height: 40px; text-align: center;";
    }

    public static string GetItemSizeLabel(int index)
    {
        return SizeVariants[index % SizeVariants.Length];
    }

    public static string GenerateCSS(string flexDirection, string flexWrap, string justifyContent,
        string alignItems, string alignContent, int gap, List<FlexItem> items)
    {
        string css = ".container {\n" +
                     "  display: flex;\n" +
                     $"  flex-direction: {flexDirection};\n" +
                     $"  flex-wrap: {flexWrap};\n" +
                     $"  justify-content: {justifyContent};\n" +
                     $"  align-items: {alignItems};\n" +
                     $"  align-content: {alignContent};\n" +
                     $"  gap: {gap}px;\n" +
                     "}\n";

        bool hasCustomItems = false;
        for (int i = 0; i < items.Count; i++)
        {
            FlexItem item = items[i];
            bool isDefault = item.FlexGrow == 0 && item.FlexShrink == 1 &&
                             item.FlexBasis == "auto" && item.Order == 0 &&
                             item.AlignSelf == "auto";
            if (!isDefault)
            {
                hasCustomItems = true;
                break;
            }
        }

        if (hasCustomItems)
        {
            for (int i = 0; i < items.Count; i++)
            {
                FlexItem item = items[i];
                bool isDefault = item.FlexGrow == 0 && item.FlexShrink == 1 &&
                                 item.FlexBasis == "auto" && item.Order == 0 &&
                                 item.AlignSelf == "auto";
                if (isDefault)
                {
                    continue;
                }

                css += $"\n.item-{i + 1} {{\n";
                if (item.FlexGrow != 0)
                {
                    css += $"  flex-grow: {item.FlexGrow};\n";
                }
                if (item.FlexShrink != 1)
                {
                    css += $"  flex-shrink: {item.FlexShrink};\n";
                }
                if (item.FlexBasis != "auto")
                {
                    css += $"  flex-basis: {item.FlexBasis};\n";
                }
                if (item.Order != 0)
                {
                    css += $"  order: {item.Order};\n";
                }
                if (item.AlignSelf != "auto")
                {
                    css += $"  align-self: {item.AlignSelf};\n";
                }
                css += "}\n";
            }
        }

        return css;
    }

    public static List<FlexItem> CreateHolyGrailPreset()
    {
        return
        [
            new FlexItem { Number = 1, FlexGrow = 0, FlexShrink = 0, FlexBasis = "60px", AlignSelf = "auto" },
            new FlexItem { Number = 2, FlexGrow = 0, FlexShrink = 0, FlexBasis = "80px", AlignSelf = "auto" },
            new FlexItem { Number = 3, FlexGrow = 1, FlexShrink = 1, FlexBasis = "auto", AlignSelf = "auto" },
            new FlexItem { Number = 4, FlexGrow = 0, FlexShrink = 0, FlexBasis = "80px", AlignSelf = "auto" },
            new FlexItem { Number = 5, FlexGrow = 0, FlexShrink = 0, FlexBasis = "60px", AlignSelf = "auto" },
        ];
    }

    public static List<FlexItem> CreateSidebarPreset()
    {
        return
        [
            new FlexItem { Number = 1, FlexGrow = 0, FlexShrink = 0, FlexBasis = "200px", AlignSelf = "auto" },
            new FlexItem { Number = 2, FlexGrow = 1, FlexShrink = 1, FlexBasis = "auto", AlignSelf = "auto" },
        ];
    }

    public static List<FlexItem> CreateCardGridPreset()
    {
        List<FlexItem> items = [];
        for (int i = 0; i < 6; i++)
        {
            items.Add(new FlexItem { Number = i + 1, FlexGrow = 0, FlexShrink = 1, FlexBasis = "200px", AlignSelf = "auto" });
        }
        return items;
    }

    public static List<FlexItem> CreateCenteredPreset()
    {
        return
        [
            new FlexItem { Number = 1, FlexGrow = 0, FlexShrink = 1, FlexBasis = "auto", AlignSelf = "auto" },
        ];
    }

    public static List<FlexItem> CreateNavBarPreset()
    {
        List<FlexItem> items = [];
        for (int i = 0; i < 5; i++)
        {
            items.Add(new FlexItem { Number = i + 1, FlexGrow = 0, FlexShrink = 1, FlexBasis = "auto", AlignSelf = "auto" });
        }
        return items;
    }
}
