using System.Text.Json;
using Microsoft.JSInterop;

namespace Rowles.Toolbox.Shared;

public sealed record RecentEntry
{
    public string Label { get; init; } = "";
    public string Timestamp { get; init; } = "";
    public string StateJson { get; init; } = "{}";
}

public sealed class LocalStorageService
{
    private readonly IJSRuntime _js;
    private const string StatePrefix = "toolbox:state:";
    private const string RecentPrefix = "toolbox:recent:";
    private const int MaxRecentEntries = 20;

    public LocalStorageService(IJSRuntime js) => _js = js;

    public async Task SaveStateAsync<T>(string toolKey, T state)
    {
        try
        {
            string json = JsonSerializer.Serialize(state);
            await _js.InvokeVoidAsync("localStorage.setItem", $"{StatePrefix}{toolKey}", json);
        }
        catch { /* localStorage unavailable or quota exceeded */ }
    }

    public async Task<T?> LoadStateAsync<T>(string toolKey)
    {
        try
        {
            string? json = await _js.InvokeAsync<string?>("localStorage.getItem", $"{StatePrefix}{toolKey}");
            if (string.IsNullOrEmpty(json)) return default;
            return JsonSerializer.Deserialize<T>(json);
        }
        catch { return default; }
    }

    public async Task AddRecentAsync(string toolKey, string label, object? state = null)
    {
        try
        {
            List<RecentEntry> entries = await GetRecentAsync(toolKey);
            RecentEntry entry = new()
            {
                Label = label,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                StateJson = state is not null ? JsonSerializer.Serialize(state) : "{}"
            };
            entries.Insert(0, entry);
            if (entries.Count > MaxRecentEntries)
                entries.RemoveRange(MaxRecentEntries, entries.Count - MaxRecentEntries);
            string json = JsonSerializer.Serialize(entries);
            await _js.InvokeVoidAsync("localStorage.setItem", $"{RecentPrefix}{toolKey}", json);
        }
        catch { /* localStorage unavailable or quota exceeded */ }
    }

    public async Task<List<RecentEntry>> GetRecentAsync(string toolKey)
    {
        try
        {
            string? json = await _js.InvokeAsync<string?>("localStorage.getItem", $"{RecentPrefix}{toolKey}");
            if (string.IsNullOrEmpty(json)) return [];
            return JsonSerializer.Deserialize<List<RecentEntry>>(json) ?? [];
        }
        catch { return []; }
    }

    public async Task ClearRecentAsync(string toolKey)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", $"{RecentPrefix}{toolKey}");
        }
        catch { /* ignore */ }
    }
}
