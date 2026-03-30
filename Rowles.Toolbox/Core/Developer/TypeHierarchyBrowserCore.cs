namespace Rowles.Toolbox.Core.Developer;

public static class TypeHierarchyBrowserCore
{
    // ── Types ──────────────────────────────────────────────

    public enum TypeKind { Class, Struct, Interface, Enum, Delegate }

    public sealed record TypeEntry(
        string FullName,
        string ShortName,
        List<string> BaseTypeChain,
        List<string> Interfaces,
        string Category,
        TypeKind Kind,
        List<string> Modifiers
    );

    // ── Helpers ────────────────────────────────────────────

    public static List<string> Chain(params string[] items) => new(items);
    public static List<string> Ifaces(params string[] items) => new(items);
    public static List<string> Mods(params string[] items) => new(items);
    public static List<string> NoIfaces() => new();
    public static List<string> NoMods() => new();

    // ── Display Helpers ───────────────────────────────────

    public static string GetCategoryIcon(string cat) => cat switch
    {
        "All" => "apps",
        "Primitives" => "box",
        "Collections" => "stack-2",
        "Interfaces" => "plug",
        "IO" => "file-text",
        "Text" => "typography",
        "Threading" => "refresh",
        "Exceptions" => "alert-triangle",
        "Spans" => "brackets",
        "Other" => "dots",
        _ => "point"
    };

    public static string GetKindBadgeColor(TypeKind kind) => kind switch
    {
        TypeKind.Class => "bg-green-100 dark:bg-green-900/40 text-green-700 dark:text-green-300",
        TypeKind.Struct => "bg-amber-100 dark:bg-amber-900/40 text-amber-700 dark:text-amber-300",
        TypeKind.Interface => "bg-purple-100 dark:bg-purple-900/40 text-purple-700 dark:text-purple-300",
        TypeKind.Enum => "bg-cyan-100 dark:bg-cyan-900/40 text-cyan-700 dark:text-cyan-300",
        TypeKind.Delegate => "bg-rose-100 dark:bg-rose-900/40 text-rose-700 dark:text-rose-300",
        _ => "bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-300"
    };

    public static string GetKindAbbrev(TypeKind kind) => kind switch
    {
        TypeKind.Class => "C",
        TypeKind.Struct => "S",
        TypeKind.Interface => "I",
        TypeKind.Enum => "E",
        TypeKind.Delegate => "D",
        _ => "?"
    };

    public static List<TypeEntry> GetFilteredTypes(
        IEnumerable<TypeEntry> allTypes,
        string selectedCategory,
        string searchQuery)
    {
        IEnumerable<TypeEntry> result = allTypes;

        if (selectedCategory != "All")
        {
            result = result.Where(t => t.Category == selectedCategory);
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string query = searchQuery.Trim();
            result = result.Where(t =>
                t.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                t.ShortName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                t.Interfaces.Any(i => i.Contains(query, StringComparison.OrdinalIgnoreCase))
            );
        }

        return result.ToList();
    }

    // ── Static type dataset ───────────────────────────────

    public static readonly List<TypeEntry> Types = new()
    {
        // ======== Primitives ========

        new TypeEntry("System.Object", "Object", Chain("Object"), NoIfaces(), "Primitives", TypeKind.Class, NoMods()),

        new TypeEntry("System.String", "String", Chain("Object", "String"),
            Ifaces("IComparable", "IComparable<string>", "IEnumerable<char>", "IEnumerable", "IEquatable<string>", "ICloneable", "IConvertible"),
            "Primitives", TypeKind.Class, Mods("sealed")),

        new TypeEntry("System.Boolean", "Boolean", Chain("Object", "ValueType", "Boolean"),
            Ifaces("IComparable", "IComparable<bool>", "IEquatable<bool>", "IConvertible"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Byte", "Byte", Chain("Object", "ValueType", "Byte"),
            Ifaces("IComparable", "IComparable<byte>", "IEquatable<byte>", "IConvertible", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.SByte", "SByte", Chain("Object", "ValueType", "SByte"),
            Ifaces("IComparable", "IComparable<sbyte>", "IEquatable<sbyte>", "IConvertible", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Int16", "Int16", Chain("Object", "ValueType", "Int16"),
            Ifaces("IComparable", "IComparable<short>", "IEquatable<short>", "IConvertible", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Int32", "Int32", Chain("Object", "ValueType", "Int32"),
            Ifaces("IComparable", "IComparable<int>", "IEquatable<int>", "IConvertible", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Int64", "Int64", Chain("Object", "ValueType", "Int64"),
            Ifaces("IComparable", "IComparable<long>", "IEquatable<long>", "IConvertible", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.UInt16", "UInt16", Chain("Object", "ValueType", "UInt16"),
            Ifaces("IComparable", "IComparable<ushort>", "IEquatable<ushort>", "IConvertible", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.UInt32", "UInt32", Chain("Object", "ValueType", "UInt32"),
            Ifaces("IComparable", "IComparable<uint>", "IEquatable<uint>", "IConvertible", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.UInt64", "UInt64", Chain("Object", "ValueType", "UInt64"),
            Ifaces("IComparable", "IComparable<ulong>", "IEquatable<ulong>", "IConvertible", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Single", "Single", Chain("Object", "ValueType", "Single"),
            Ifaces("IComparable", "IComparable<float>", "IEquatable<float>", "IConvertible", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Double", "Double", Chain("Object", "ValueType", "Double"),
            Ifaces("IComparable", "IComparable<double>", "IEquatable<double>", "IConvertible", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Decimal", "Decimal", Chain("Object", "ValueType", "Decimal"),
            Ifaces("IComparable", "IComparable<decimal>", "IEquatable<decimal>", "IConvertible", "IFormattable", "ISpanFormattable", "IDeserializationCallback"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Char", "Char", Chain("Object", "ValueType", "Char"),
            Ifaces("IComparable", "IComparable<char>", "IEquatable<char>", "IConvertible"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.IntPtr", "IntPtr", Chain("Object", "ValueType", "IntPtr"),
            Ifaces("IComparable", "IComparable<nint>", "IEquatable<nint>", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.UIntPtr", "UIntPtr", Chain("Object", "ValueType", "UIntPtr"),
            Ifaces("IComparable", "IComparable<nuint>", "IEquatable<nuint>", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Half", "Half", Chain("Object", "ValueType", "Half"),
            Ifaces("IComparable", "IComparable<Half>", "IEquatable<Half>", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Int128", "Int128", Chain("Object", "ValueType", "Int128"),
            Ifaces("IComparable", "IComparable<Int128>", "IEquatable<Int128>", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        new TypeEntry("System.UInt128", "UInt128", Chain("Object", "ValueType", "UInt128"),
            Ifaces("IComparable", "IComparable<UInt128>", "IEquatable<UInt128>", "IFormattable", "ISpanFormattable"),
            "Primitives", TypeKind.Struct, NoMods()),

        // ======== Collections ========

        new TypeEntry("System.Collections.Generic.List<T>", "List<T>", Chain("Object", "List<T>"),
            Ifaces("IList<T>", "ICollection<T>", "IEnumerable<T>", "IEnumerable", "IList", "ICollection", "IReadOnlyList<T>", "IReadOnlyCollection<T>"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Generic.Dictionary<TKey,TValue>", "Dictionary<TKey,TValue>", Chain("Object", "Dictionary<TKey,TValue>"),
            Ifaces("IDictionary<TKey,TValue>", "ICollection<KeyValuePair<TKey,TValue>>", "IEnumerable<KeyValuePair<TKey,TValue>>", "IEnumerable", "IDictionary", "ICollection", "IReadOnlyDictionary<TKey,TValue>", "IReadOnlyCollection<KeyValuePair<TKey,TValue>>", "IDeserializationCallback", "ISerializable"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Generic.HashSet<T>", "HashSet<T>", Chain("Object", "HashSet<T>"),
            Ifaces("ICollection<T>", "IEnumerable<T>", "IEnumerable", "ISet<T>", "IReadOnlyCollection<T>", "IReadOnlySet<T>", "IDeserializationCallback", "ISerializable"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Generic.Queue<T>", "Queue<T>", Chain("Object", "Queue<T>"),
            Ifaces("IEnumerable<T>", "IEnumerable", "ICollection", "IReadOnlyCollection<T>"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Generic.Stack<T>", "Stack<T>", Chain("Object", "Stack<T>"),
            Ifaces("IEnumerable<T>", "IEnumerable", "ICollection", "IReadOnlyCollection<T>"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Generic.LinkedList<T>", "LinkedList<T>", Chain("Object", "LinkedList<T>"),
            Ifaces("ICollection<T>", "IEnumerable<T>", "IEnumerable", "ICollection", "IDeserializationCallback", "ISerializable"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Generic.SortedSet<T>", "SortedSet<T>", Chain("Object", "SortedSet<T>"),
            Ifaces("ICollection<T>", "IEnumerable<T>", "IEnumerable", "ISet<T>", "ICollection", "IReadOnlyCollection<T>", "IReadOnlySet<T>", "IDeserializationCallback", "ISerializable"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Generic.SortedDictionary<TKey,TValue>", "SortedDictionary<TKey,TValue>", Chain("Object", "SortedDictionary<TKey,TValue>"),
            Ifaces("IDictionary<TKey,TValue>", "ICollection<KeyValuePair<TKey,TValue>>", "IEnumerable<KeyValuePair<TKey,TValue>>", "IEnumerable", "IDictionary", "ICollection"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Array", "Array", Chain("Object", "Array"),
            Ifaces("IList", "ICollection", "IEnumerable", "IStructuralComparable", "IStructuralEquatable", "ICloneable"),
            "Collections", TypeKind.Class, Mods("abstract")),

        new TypeEntry("System.Collections.ArrayList", "ArrayList", Chain("Object", "ArrayList"),
            Ifaces("IList", "ICollection", "IEnumerable", "ICloneable"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Hashtable", "Hashtable", Chain("Object", "Hashtable"),
            Ifaces("IDictionary", "ICollection", "IEnumerable", "ICloneable", "IDeserializationCallback", "ISerializable"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>", "ConcurrentDictionary<TKey,TValue>", Chain("Object", "ConcurrentDictionary<TKey,TValue>"),
            Ifaces("IDictionary<TKey,TValue>", "ICollection<KeyValuePair<TKey,TValue>>", "IEnumerable<KeyValuePair<TKey,TValue>>", "IEnumerable", "IDictionary", "ICollection", "IReadOnlyDictionary<TKey,TValue>", "IReadOnlyCollection<KeyValuePair<TKey,TValue>>"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Concurrent.ConcurrentQueue<T>", "ConcurrentQueue<T>", Chain("Object", "ConcurrentQueue<T>"),
            Ifaces("IProducerConsumerCollection<T>", "IEnumerable<T>", "IEnumerable", "ICollection", "IReadOnlyCollection<T>"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Concurrent.ConcurrentBag<T>", "ConcurrentBag<T>", Chain("Object", "ConcurrentBag<T>"),
            Ifaces("IProducerConsumerCollection<T>", "IEnumerable<T>", "IEnumerable", "ICollection", "IReadOnlyCollection<T>"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.Immutable.ImmutableArray<T>", "ImmutableArray<T>", Chain("Object", "ValueType", "ImmutableArray<T>"),
            Ifaces("IReadOnlyList<T>", "IReadOnlyCollection<T>", "IEnumerable<T>", "IEnumerable", "IList<T>", "ICollection<T>", "IEquatable<ImmutableArray<T>>", "IImmutableList<T>", "IStructuralComparable", "IStructuralEquatable"),
            "Collections", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Collections.Immutable.ImmutableList<T>", "ImmutableList<T>", Chain("Object", "ImmutableList<T>"),
            Ifaces("IImmutableList<T>", "IReadOnlyList<T>", "IReadOnlyCollection<T>", "IEnumerable<T>", "IEnumerable", "IList<T>", "ICollection<T>", "IList", "ICollection"),
            "Collections", TypeKind.Class, Mods("sealed")),

        new TypeEntry("System.Collections.ObjectModel.ReadOnlyCollection<T>", "ReadOnlyCollection<T>", Chain("Object", "ReadOnlyCollection<T>"),
            Ifaces("IList<T>", "ICollection<T>", "IEnumerable<T>", "IEnumerable", "IList", "ICollection", "IReadOnlyList<T>", "IReadOnlyCollection<T>"),
            "Collections", TypeKind.Class, NoMods()),

        new TypeEntry("System.Collections.ObjectModel.ObservableCollection<T>", "ObservableCollection<T>", Chain("Object", "Collection<T>", "ObservableCollection<T>"),
            Ifaces("IList<T>", "ICollection<T>", "IEnumerable<T>", "IEnumerable", "IList", "ICollection", "IReadOnlyList<T>", "IReadOnlyCollection<T>", "INotifyCollectionChanged", "INotifyPropertyChanged"),
            "Collections", TypeKind.Class, NoMods()),

        // ======== Interfaces ========

        new TypeEntry("System.Collections.IEnumerable", "IEnumerable", Chain(), NoIfaces(), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.Collections.Generic.IEnumerable<T>", "IEnumerable<T>", Chain(),
            Ifaces("IEnumerable"), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.Collections.ICollection", "ICollection", Chain(),
            Ifaces("IEnumerable"), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.Collections.Generic.ICollection<T>", "ICollection<T>", Chain(),
            Ifaces("IEnumerable<T>", "IEnumerable"), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.Collections.IList", "IList", Chain(),
            Ifaces("ICollection", "IEnumerable"), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.Collections.Generic.IList<T>", "IList<T>", Chain(),
            Ifaces("ICollection<T>", "IEnumerable<T>", "IEnumerable"), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.Collections.IDictionary", "IDictionary", Chain(),
            Ifaces("ICollection", "IEnumerable"), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.Collections.Generic.IDictionary<TKey,TValue>", "IDictionary<TKey,TValue>", Chain(),
            Ifaces("ICollection<KeyValuePair<TKey,TValue>>", "IEnumerable<KeyValuePair<TKey,TValue>>", "IEnumerable"), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.IComparable", "IComparable", Chain(), NoIfaces(), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.IComparable<T>", "IComparable<T>", Chain(), NoIfaces(), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.IEquatable<T>", "IEquatable<T>", Chain(), NoIfaces(), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.IDisposable", "IDisposable", Chain(), NoIfaces(), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.IAsyncDisposable", "IAsyncDisposable", Chain(), NoIfaces(), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.ICloneable", "ICloneable", Chain(), NoIfaces(), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.IFormattable", "IFormattable", Chain(), NoIfaces(), "Interfaces", TypeKind.Interface, NoMods()),

        new TypeEntry("System.ISpanFormattable", "ISpanFormattable", Chain(),
            Ifaces("IFormattable"), "Interfaces", TypeKind.Interface, NoMods()),

        // ======== IO ========

        new TypeEntry("System.IO.Stream", "Stream", Chain("Object", "MarshalByRefObject", "Stream"),
            Ifaces("IDisposable", "IAsyncDisposable"), "IO", TypeKind.Class, Mods("abstract")),

        new TypeEntry("System.IO.MemoryStream", "MemoryStream", Chain("Object", "MarshalByRefObject", "Stream", "MemoryStream"),
            Ifaces("IDisposable", "IAsyncDisposable"), "IO", TypeKind.Class, NoMods()),

        new TypeEntry("System.IO.FileStream", "FileStream", Chain("Object", "MarshalByRefObject", "Stream", "FileStream"),
            Ifaces("IDisposable", "IAsyncDisposable"), "IO", TypeKind.Class, NoMods()),

        new TypeEntry("System.IO.TextReader", "TextReader", Chain("Object", "MarshalByRefObject", "TextReader"),
            Ifaces("IDisposable"), "IO", TypeKind.Class, Mods("abstract")),

        new TypeEntry("System.IO.TextWriter", "TextWriter", Chain("Object", "MarshalByRefObject", "TextWriter"),
            Ifaces("IDisposable", "IAsyncDisposable"), "IO", TypeKind.Class, Mods("abstract")),

        new TypeEntry("System.IO.StreamReader", "StreamReader", Chain("Object", "MarshalByRefObject", "TextReader", "StreamReader"),
            Ifaces("IDisposable"), "IO", TypeKind.Class, NoMods()),

        new TypeEntry("System.IO.StreamWriter", "StreamWriter", Chain("Object", "MarshalByRefObject", "TextWriter", "StreamWriter"),
            Ifaces("IDisposable", "IAsyncDisposable"), "IO", TypeKind.Class, NoMods()),

        new TypeEntry("System.IO.BinaryReader", "BinaryReader", Chain("Object", "BinaryReader"),
            Ifaces("IDisposable"), "IO", TypeKind.Class, NoMods()),

        new TypeEntry("System.IO.BinaryWriter", "BinaryWriter", Chain("Object", "BinaryWriter"),
            Ifaces("IDisposable", "IAsyncDisposable"), "IO", TypeKind.Class, NoMods()),

        new TypeEntry("System.IO.Path", "Path", Chain("Object", "Path"),
            NoIfaces(), "IO", TypeKind.Class, Mods("static")),

        new TypeEntry("System.IO.File", "File", Chain("Object", "File"),
            NoIfaces(), "IO", TypeKind.Class, Mods("static")),

        new TypeEntry("System.IO.Directory", "Directory", Chain("Object", "Directory"),
            NoIfaces(), "IO", TypeKind.Class, Mods("static")),

        // ======== Text ========

        new TypeEntry("System.Text.StringBuilder", "StringBuilder", Chain("Object", "StringBuilder"),
            Ifaces("ISerializable"), "Text", TypeKind.Class, Mods("sealed")),

        new TypeEntry("System.Text.RegularExpressions.Regex", "Regex", Chain("Object", "Regex"),
            Ifaces("ISerializable"), "Text", TypeKind.Class, NoMods()),

        new TypeEntry("System.Text.Encoding", "Encoding", Chain("Object", "Encoding"),
            Ifaces("ICloneable"), "Text", TypeKind.Class, Mods("abstract")),

        new TypeEntry("System.Text.UTF8Encoding", "UTF8Encoding", Chain("Object", "Encoding", "UTF8Encoding"),
            Ifaces("ICloneable"), "Text", TypeKind.Class, NoMods()),

        new TypeEntry("System.Text.ASCIIEncoding", "ASCIIEncoding", Chain("Object", "Encoding", "ASCIIEncoding"),
            Ifaces("ICloneable"), "Text", TypeKind.Class, NoMods()),

        // ======== Threading ========

        new TypeEntry("System.Threading.Tasks.Task", "Task", Chain("Object", "Task"),
            Ifaces("IAsyncResult", "IDisposable"), "Threading", TypeKind.Class, NoMods()),

        new TypeEntry("System.Threading.Tasks.Task<TResult>", "Task<TResult>", Chain("Object", "Task", "Task<TResult>"),
            Ifaces("IAsyncResult", "IDisposable"), "Threading", TypeKind.Class, NoMods()),

        new TypeEntry("System.Threading.Tasks.ValueTask", "ValueTask", Chain("Object", "ValueType", "ValueTask"),
            Ifaces("IEquatable<ValueTask>"), "Threading", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Threading.Tasks.ValueTask<TResult>", "ValueTask<TResult>", Chain("Object", "ValueType", "ValueTask<TResult>"),
            Ifaces("IEquatable<ValueTask<TResult>>"), "Threading", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Threading.Thread", "Thread", Chain("Object", "CriticalFinalizerObject", "Thread"),
            NoIfaces(), "Threading", TypeKind.Class, Mods("sealed")),

        new TypeEntry("System.Threading.CancellationToken", "CancellationToken", Chain("Object", "ValueType", "CancellationToken"),
            Ifaces("IEquatable<CancellationToken>"), "Threading", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Threading.CancellationTokenSource", "CancellationTokenSource", Chain("Object", "CancellationTokenSource"),
            Ifaces("IDisposable"), "Threading", TypeKind.Class, NoMods()),

        new TypeEntry("System.Threading.SemaphoreSlim", "SemaphoreSlim", Chain("Object", "SemaphoreSlim"),
            Ifaces("IDisposable"), "Threading", TypeKind.Class, NoMods()),

        new TypeEntry("System.Threading.Mutex", "Mutex", Chain("Object", "MarshalByRefObject", "WaitHandle", "Mutex"),
            Ifaces("IDisposable"), "Threading", TypeKind.Class, Mods("sealed")),

        new TypeEntry("System.Threading.Timer", "Timer", Chain("Object", "MarshalByRefObject", "Timer"),
            Ifaces("IDisposable", "IAsyncDisposable"), "Threading", TypeKind.Class, Mods("sealed")),

        // ======== Exceptions ========

        new TypeEntry("System.Exception", "Exception", Chain("Object", "Exception"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.SystemException", "SystemException", Chain("Object", "Exception", "SystemException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.ArgumentException", "ArgumentException", Chain("Object", "Exception", "SystemException", "ArgumentException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.ArgumentNullException", "ArgumentNullException", Chain("Object", "Exception", "SystemException", "ArgumentException", "ArgumentNullException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.ArgumentOutOfRangeException", "ArgumentOutOfRangeException", Chain("Object", "Exception", "SystemException", "ArgumentException", "ArgumentOutOfRangeException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.InvalidOperationException", "InvalidOperationException", Chain("Object", "Exception", "SystemException", "InvalidOperationException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.NotSupportedException", "NotSupportedException", Chain("Object", "Exception", "SystemException", "NotSupportedException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.NotImplementedException", "NotImplementedException", Chain("Object", "Exception", "SystemException", "NotImplementedException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.NullReferenceException", "NullReferenceException", Chain("Object", "Exception", "SystemException", "NullReferenceException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.IndexOutOfRangeException", "IndexOutOfRangeException", Chain("Object", "Exception", "SystemException", "IndexOutOfRangeException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.FormatException", "FormatException", Chain("Object", "Exception", "SystemException", "FormatException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.IO.IOException", "IOException", Chain("Object", "Exception", "SystemException", "IOException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.Net.Http.HttpRequestException", "HttpRequestException", Chain("Object", "Exception", "HttpRequestException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.Threading.Tasks.TaskCanceledException", "TaskCanceledException", Chain("Object", "Exception", "SystemException", "OperationCanceledException", "TaskCanceledException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.OperationCanceledException", "OperationCanceledException", Chain("Object", "Exception", "SystemException", "OperationCanceledException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.ObjectDisposedException", "ObjectDisposedException", Chain("Object", "Exception", "SystemException", "InvalidOperationException", "ObjectDisposedException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.OverflowException", "OverflowException", Chain("Object", "Exception", "SystemException", "ArithmeticException", "OverflowException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.DivideByZeroException", "DivideByZeroException", Chain("Object", "Exception", "SystemException", "ArithmeticException", "DivideByZeroException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.StackOverflowException", "StackOverflowException", Chain("Object", "Exception", "SystemException", "StackOverflowException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, Mods("sealed")),

        new TypeEntry("System.OutOfMemoryException", "OutOfMemoryException", Chain("Object", "Exception", "SystemException", "OutOfMemoryException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        new TypeEntry("System.AggregateException", "AggregateException", Chain("Object", "Exception", "AggregateException"),
            Ifaces("ISerializable"), "Exceptions", TypeKind.Class, NoMods()),

        // ======== Spans ========

        new TypeEntry("System.Span<T>", "Span<T>", Chain("Object", "ValueType", "Span<T>"),
            NoIfaces(), "Spans", TypeKind.Struct, NoMods()),

        new TypeEntry("System.ReadOnlySpan<T>", "ReadOnlySpan<T>", Chain("Object", "ValueType", "ReadOnlySpan<T>"),
            NoIfaces(), "Spans", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Memory<T>", "Memory<T>", Chain("Object", "ValueType", "Memory<T>"),
            Ifaces("IEquatable<Memory<T>>"), "Spans", TypeKind.Struct, NoMods()),

        new TypeEntry("System.ReadOnlyMemory<T>", "ReadOnlyMemory<T>", Chain("Object", "ValueType", "ReadOnlyMemory<T>"),
            Ifaces("IEquatable<ReadOnlyMemory<T>>"), "Spans", TypeKind.Struct, NoMods()),

        // ======== Other ========

        new TypeEntry("System.Guid", "Guid", Chain("Object", "ValueType", "Guid"),
            Ifaces("IComparable", "IComparable<Guid>", "IEquatable<Guid>", "IFormattable", "ISpanFormattable"),
            "Other", TypeKind.Struct, NoMods()),

        new TypeEntry("System.DateTime", "DateTime", Chain("Object", "ValueType", "DateTime"),
            Ifaces("IComparable", "IComparable<DateTime>", "IEquatable<DateTime>", "IConvertible", "IFormattable", "ISpanFormattable", "ISerializable"),
            "Other", TypeKind.Struct, NoMods()),

        new TypeEntry("System.DateTimeOffset", "DateTimeOffset", Chain("Object", "ValueType", "DateTimeOffset"),
            Ifaces("IComparable", "IComparable<DateTimeOffset>", "IEquatable<DateTimeOffset>", "IFormattable", "ISpanFormattable", "ISerializable", "IDeserializationCallback"),
            "Other", TypeKind.Struct, NoMods()),

        new TypeEntry("System.TimeSpan", "TimeSpan", Chain("Object", "ValueType", "TimeSpan"),
            Ifaces("IComparable", "IComparable<TimeSpan>", "IEquatable<TimeSpan>", "IFormattable", "ISpanFormattable"),
            "Other", TypeKind.Struct, NoMods()),

        new TypeEntry("System.DateOnly", "DateOnly", Chain("Object", "ValueType", "DateOnly"),
            Ifaces("IComparable", "IComparable<DateOnly>", "IEquatable<DateOnly>", "IFormattable", "ISpanFormattable"),
            "Other", TypeKind.Struct, NoMods()),

        new TypeEntry("System.TimeOnly", "TimeOnly", Chain("Object", "ValueType", "TimeOnly"),
            Ifaces("IComparable", "IComparable<TimeOnly>", "IEquatable<TimeOnly>", "IFormattable", "ISpanFormattable"),
            "Other", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Uri", "Uri", Chain("Object", "Uri"),
            Ifaces("ISerializable"), "Other", TypeKind.Class, NoMods()),

        new TypeEntry("System.Version", "Version", Chain("Object", "Version"),
            Ifaces("IComparable", "IComparable<Version>", "IEquatable<Version>", "ICloneable", "IFormattable", "ISpanFormattable"),
            "Other", TypeKind.Class, Mods("sealed")),

        new TypeEntry("System.Tuple", "Tuple", Chain("Object", "Tuple"),
            NoIfaces(), "Other", TypeKind.Class, Mods("static")),

        new TypeEntry("System.ValueTuple", "ValueTuple", Chain("Object", "ValueType", "ValueTuple"),
            Ifaces("IEquatable<ValueTuple>", "IStructuralEquatable", "IStructuralComparable", "IComparable", "IComparable<ValueTuple>", "ITuple"),
            "Other", TypeKind.Struct, NoMods()),

        new TypeEntry("System.Lazy<T>", "Lazy<T>", Chain("Object", "Lazy<T>"),
            NoIfaces(), "Other", TypeKind.Class, NoMods()),

        new TypeEntry("System.Nullable<T>", "Nullable<T>", Chain("Object", "ValueType", "Nullable<T>"),
            NoIfaces(), "Other", TypeKind.Struct, NoMods()),

        new TypeEntry("System.WeakReference<T>", "WeakReference<T>", Chain("Object", "WeakReference<T>"),
            Ifaces("ISerializable"), "Other", TypeKind.Class, Mods("sealed")),

        new TypeEntry("System.Type", "Type", Chain("Object", "MemberInfo", "Type"),
            Ifaces("IReflect"), "Other", TypeKind.Class, Mods("abstract")),

        new TypeEntry("System.Attribute", "Attribute", Chain("Object", "Attribute"),
            NoIfaces(), "Other", TypeKind.Class, Mods("abstract")),

        new TypeEntry("System.Enum", "Enum", Chain("Object", "ValueType", "Enum"),
            Ifaces("IComparable", "IConvertible", "IFormattable"), "Other", TypeKind.Class, Mods("abstract")),

        new TypeEntry("System.Delegate", "Delegate", Chain("Object", "Delegate"),
            Ifaces("ICloneable", "ISerializable"), "Other", TypeKind.Class, Mods("abstract")),

        new TypeEntry("System.MulticastDelegate", "MulticastDelegate", Chain("Object", "Delegate", "MulticastDelegate"),
            Ifaces("ICloneable", "ISerializable"), "Other", TypeKind.Class, Mods("abstract")),

        new TypeEntry("System.EventHandler", "EventHandler", Chain("Object", "Delegate", "MulticastDelegate", "EventHandler"),
            NoIfaces(), "Other", TypeKind.Delegate, NoMods()),

        new TypeEntry("System.Action", "Action", Chain("Object", "Delegate", "MulticastDelegate", "Action"),
            NoIfaces(), "Other", TypeKind.Delegate, NoMods()),

        new TypeEntry("System.Func<TResult>", "Func<TResult>", Chain("Object", "Delegate", "MulticastDelegate", "Func<TResult>"),
            NoIfaces(), "Other", TypeKind.Delegate, NoMods()),

        new TypeEntry("System.Predicate<T>", "Predicate<T>", Chain("Object", "Delegate", "MulticastDelegate", "Predicate<T>"),
            NoIfaces(), "Other", TypeKind.Delegate, NoMods()),
    };
}
