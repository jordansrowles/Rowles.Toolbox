namespace Rowles.Toolbox.Core.Developer;

public static class BigOReferenceCore
{
    public enum Rating { Excellent, Good, Fair, Bad, Horrible }

    public sealed record ComplexityInfo(string Notation, string Name, string Description, string Examples, Rating Rating);

    public sealed record DataStructureInfo(
        string Name,
        string AccessAvg, string AccessWorst,
        string SearchAvg, string SearchWorst,
        string InsertAvg, string InsertWorst,
        string DeleteAvg, string DeleteWorst,
        string Space);

    public sealed record SortAlgorithmInfo(string Name, string Best, string Average, string Worst, string Space, bool Stable);

    public sealed record GrowthEntry(string Label, string DisplayValue, double WidthPercent, string Color);

    public sealed record DotNetCollectionInfo(string TypeName, string Add, string Remove, string Lookup, string IndexAccess, string Notes);

    public static readonly ComplexityInfo[] Complexities =
    [
        new("O(1)",      "Constant",      "Execution time does not depend on input size",                   "Hash table lookup, array index access",             Rating.Excellent),
        new("O(log n)",  "Logarithmic",   "Halves the problem space each step",                            "Binary search, balanced BST lookup",                Rating.Excellent),
        new("O(n)",      "Linear",        "Grows directly proportional to input size",                     "Linear search, single loop over array",             Rating.Good),
        new("O(n log n)","Linearithmic",  "Slightly worse than linear — typical for efficient sorts",      "Merge sort, quick sort (avg), heap sort",           Rating.Fair),
        new("O(n²)",     "Quadratic",     "Nested iteration — slows quickly with large inputs",            "Bubble sort, selection sort, insertion sort (worst)",Rating.Bad),
        new("O(n³)",     "Cubic",         "Triple-nested iteration — rarely acceptable for large n",       "Naive matrix multiplication, Floyd-Warshall",      Rating.Bad),
        new("O(2ⁿ)",    "Exponential",   "Doubles with each additional element — intractable for large n","Recursive Fibonacci (naive), power set",            Rating.Horrible),
        new("O(n!)",     "Factorial",     "Grows astronomically — only feasible for tiny inputs",          "Travelling salesman (brute force), permutations",  Rating.Horrible),
    ];

    public static readonly DataStructureInfo[] DataStructures =
    [
        new("Array",                "O(1)",      "O(1)",      "O(n)",      "O(n)",      "O(n)",      "O(n)",      "O(n)",      "O(n)",      "O(n)"),
        new("Singly Linked List",   "O(n)",      "O(n)",      "O(n)",      "O(n)",      "O(1)",      "O(1)",      "O(1)",      "O(1)",      "O(n)"),
        new("Doubly Linked List",   "O(n)",      "O(n)",      "O(n)",      "O(n)",      "O(1)",      "O(1)",      "O(1)",      "O(1)",      "O(n)"),
        new("Stack",                "O(n)",      "O(n)",      "O(n)",      "O(n)",      "O(1)",      "O(1)",      "O(1)",      "O(1)",      "O(n)"),
        new("Queue",                "O(n)",      "O(n)",      "O(n)",      "O(n)",      "O(1)",      "O(1)",      "O(1)",      "O(1)",      "O(n)"),
        new("HashSet",              "—",         "—",         "O(1)",      "O(n)",      "O(1)",      "O(n)",      "O(1)",      "O(n)",      "O(n)"),
        new("Dictionary / HashMap", "—",         "—",         "O(1)",      "O(n)",      "O(1)",      "O(n)",      "O(1)",      "O(n)",      "O(n)"),
        new("SortedSet / TreeSet",  "—",         "—",         "O(log n)",  "O(log n)",  "O(log n)",  "O(log n)",  "O(log n)",  "O(log n)",  "O(n)"),
        new("Binary Search Tree",   "O(log n)",  "O(n)",      "O(log n)",  "O(n)",      "O(log n)",  "O(n)",      "O(log n)",  "O(n)",      "O(n)"),
        new("Heap",                 "—",         "—",         "O(n)",      "O(n)",      "O(log n)",  "O(log n)",  "O(log n)",  "O(log n)",  "O(n)"),
        new("Graph (Adj. List)",    "—",         "—",         "O(V)",      "O(V)",      "O(1)",      "O(1)",      "O(E)",      "O(E)",      "O(V+E)"),
    ];

    public static readonly SortAlgorithmInfo[] SortAlgorithms =
    [
        new("Bubble Sort",    "O(n)",      "O(n²)",     "O(n²)",     "O(1)",      true),
        new("Selection Sort", "O(n²)",     "O(n²)",     "O(n²)",     "O(1)",      false),
        new("Insertion Sort", "O(n)",      "O(n²)",     "O(n²)",     "O(1)",      true),
        new("Merge Sort",     "O(n log n)","O(n log n)","O(n log n)","O(n)",      true),
        new("Quick Sort",     "O(n log n)","O(n log n)","O(n²)",     "O(log n)",  false),
        new("Heap Sort",      "O(n log n)","O(n log n)","O(n log n)","O(1)",      false),
        new("Counting Sort",  "O(n + k)",  "O(n + k)",  "O(n + k)",  "O(k)",      true),
        new("Radix Sort",     "O(nk)",     "O(nk)",     "O(nk)",     "O(n + k)",  true),
        new("Bucket Sort",    "O(n + k)",  "O(n + k)",  "O(n²)",     "O(n)",      true),
        new("Tim Sort",       "O(n)",      "O(n log n)","O(n log n)","O(n)",      true),
    ];

    public static readonly int[] GrowthInputSizes = [10, 100, 1000];

    public static readonly DotNetCollectionInfo[] DotNetCollections =
    [
        new("List<T>",                          "O(1)*",     "O(n)",      "O(n)",      "O(1)",  "Amortised O(1) add at end; O(n) insert at index"),
        new("Dictionary<TKey,TValue>",           "O(1)",      "O(1)",      "O(1)",      "—",     "Average case; O(n) worst on hash collision"),
        new("HashSet<T>",                        "O(1)",      "O(1)",      "O(1)",      "—",     "Average case; O(n) worst on hash collision"),
        new("SortedDictionary<TKey,TValue>",     "O(log n)",  "O(log n)",  "O(log n)",  "—",     "Red-black tree backed; ordered enumeration"),
        new("SortedSet<T>",                      "O(log n)",  "O(log n)",  "O(log n)",  "—",     "Red-black tree; supports range views"),
        new("LinkedList<T>",                     "O(1)",      "O(1)",      "O(n)",      "O(n)",  "O(1) add/remove at known node; O(n) search"),
        new("Stack<T>",                          "O(1)",      "O(1)",      "O(n)",      "—",     "Push/Pop are O(1); backed by array"),
        new("Queue<T>",                          "O(1)",      "O(1)",      "O(n)",      "—",     "Enqueue/Dequeue are O(1); circular buffer"),
        new("ConcurrentDictionary<TKey,TValue>", "O(1)",      "O(1)",      "O(1)",      "—",     "Lock-striped; fine-grained concurrency"),
        new("ConcurrentQueue<T>",                "O(1)",      "O(1)",      "O(n)",      "—",     "Lock-free; segment-based implementation"),
        new("ImmutableArray<T>",                 "O(n)",      "O(n)",      "O(n)",      "O(1)",  "Copy-on-write; O(n) for structural changes"),
        new("ImmutableDictionary<TKey,TValue>",  "O(log n)",  "O(log n)",  "O(log n)",  "—",     "AVL tree; persistent data structure"),
    ];

    public static List<GrowthEntry> ComputeGrowthBars(int n)
    {
        List<(string Label, double Value, string Color)> raw =
        [
            ("O(1)",      1,                                        "bg-green-500"),
            ("O(log n)",  Math.Log2(n),                             "bg-green-600"),
            ("O(n)",      n,                                        "bg-yellow-500"),
            ("O(n log n)",n * Math.Log2(n),                         "bg-yellow-600"),
            ("O(n²)",     (double)n * n,                            "bg-orange-500"),
            ("O(n³)",     (double)n * n * n,                        "bg-red-500"),
            ("O(2ⁿ)",    n <= 20 ? Math.Pow(2, n) : double.PositiveInfinity, "bg-red-700"),
            ("O(n!)",     n <= 12 ? Factorial(n) : double.PositiveInfinity,   "bg-red-900"),
        ];

        double maxFinite = 0;
        foreach ((string _, double value, string _) in raw)
        {
            if (!double.IsInfinity(value) && value > maxFinite)
            {
                maxFinite = value;
            }
        }

        if (maxFinite == 0) maxFinite = 1;

        List<GrowthEntry> entries = new(raw.Count);
        foreach ((string label, double value, string color) in raw)
        {
            if (double.IsInfinity(value))
            {
                entries.Add(new GrowthEntry(label, "∞", 100, color));
            }
            else
            {
                double pct = Math.Max(1, value / maxFinite * 100);
                string display = value >= 1_000_000_000
                    ? value.ToString("0.##E+0")
                    : value >= 1_000_000
                        ? $"{value / 1_000_000:0.#}M"
                        : value >= 1_000
                            ? $"{value / 1_000:0.#}K"
                            : value.ToString("0.#");
                entries.Add(new GrowthEntry(label, display, pct, color));
            }
        }

        return entries;
    }

    public static double Factorial(int n)
    {
        double result = 1;
        for (int i = 2; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }

    public static string ComplexityColor(string complexity)
    {
        string trimmed = complexity.Replace("*", "").Trim();
        return trimmed switch
        {
            "O(1)"      => "bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200",
            "O(1)*"     => "bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200",
            "O(log n)"  => "bg-lime-100 dark:bg-lime-900 text-lime-800 dark:text-lime-200",
            "O(n)"      => "bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200",
            "O(n log n)" => "bg-amber-100 dark:bg-amber-900 text-amber-800 dark:text-amber-200",
            "O(n + k)"  => "bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200",
            "O(nk)"     => "bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200",
            "O(k)"      => "bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200",
            "O(n²)"     => "bg-red-100 dark:bg-red-900 text-red-800 dark:text-red-200",
            "O(n³)"     => "bg-red-100 dark:bg-red-900 text-red-800 dark:text-red-200",
            "O(2ⁿ)"    => "bg-red-200 dark:bg-red-950 text-red-900 dark:text-red-200",
            "O(n!)"     => "bg-red-200 dark:bg-red-950 text-red-900 dark:text-red-200",
            "O(V)"      => "bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200",
            "O(E)"      => "bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200",
            "O(V+E)"    => "bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200",
            _           => "bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300",
        };
    }
}
