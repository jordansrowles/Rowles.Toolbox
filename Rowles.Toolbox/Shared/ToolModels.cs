namespace Rowles.Toolbox.Shared;

/// <summary>Top-level navigation section (e.g. "Text", "Developer").</summary>
public sealed record ToolSection
{
    public required string Name { get; init; }
    public required string Icon { get; init; }
    public required string IconColorClass { get; init; }
    public required string HoverColorClass { get; init; }
    public required IReadOnlyList<ToolGroup> Groups { get; init; }
}

/// <summary>Sub-header group within a section (e.g. "Analyse", ".NET").</summary>
public sealed record ToolGroup
{
    public required string Name { get; init; }
    public required string Icon { get; init; }
    public bool IsCollapsible { get; init; }
    public bool IsPlanned { get; init; }
    public required IReadOnlyList<ToolItem> Tools { get; init; }
}

/// <summary>Individual tool entry.</summary>
public sealed record ToolItem
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Route { get; init; }
    public required string Icon { get; init; }
    public bool IsPlanned { get; init; }
    public string[] Tags { get; init; } = [];
}
