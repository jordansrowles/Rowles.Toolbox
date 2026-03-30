namespace Rowles.Toolbox.Core.WebNetwork;

public static class DeviceFingerprintCore
{
    public sealed record FingerprintCategory(string Name, string Icon, string HeaderColorClass, List<FingerprintEntry> Entries);

    public sealed record FingerprintEntry(string Property, string Value, string Note);
}
