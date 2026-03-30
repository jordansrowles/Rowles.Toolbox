namespace Rowles.Toolbox.Shared;

/// <summary>
/// Singleton service that manages tool search state across components.
/// NavMenu writes the query; both NavMenu and Home subscribe to changes.
/// </summary>
public sealed class ToolSearchService : IDisposable
{
    private readonly record struct SearchableEntry(string Route, string SearchText);

    private readonly List<SearchableEntry> _entries;
    private HashSet<string> _matchingRoutes = [];

    public ToolSearchService()
    {
        _entries = ToolRegistry.AllTools
            .Where(x => x.Tool.Route is not null)
            .Select(x => new SearchableEntry(
                x.Tool.Route!,
                string.Join(' ', [
                    x.Tool.Name,
                    x.Tool.Description ?? "",
                    x.Section.Name,
                    x.Group.Name,
                    .. x.Tool.Tags
                ]).ToLowerInvariant()))
            .ToList();
    }

    public string Query { get; private set; } = "";
    public bool HasQuery => !string.IsNullOrWhiteSpace(Query);

    public event Action? OnSearchChanged;

    public bool IsMatch(string? route)
    {
        if (!HasQuery || string.IsNullOrEmpty(route)) return false;
        return _matchingRoutes.Contains(route);
    }

    public bool GroupHasMatch(ToolGroup group)
    {
        if (!HasQuery) return false;
        return group.Tools.Any(t => t.Route is not null && _matchingRoutes.Contains(t.Route));
    }

    public bool SectionHasMatch(ToolSection section)
    {
        if (!HasQuery) return false;
        return section.Groups.Any(GroupHasMatch);
    }

    public void SetQuery(string query)
    {
        Query = query;

        if (string.IsNullOrWhiteSpace(query))
        {
            _matchingRoutes = [];
        }
        else
        {
            string[] terms = query.ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            _matchingRoutes = _entries
                .Where(e => terms.All(t => e.SearchText.Contains(t, StringComparison.Ordinal)))
                .Select(e => e.Route)
                .ToHashSet();
        }

        OnSearchChanged?.Invoke();
    }

    public void Dispose()
    {
        OnSearchChanged = null;
    }
}
