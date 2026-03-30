using System.Xml;
using System.Xml.XPath;

namespace Rowles.Toolbox.Core.Inspection;

public static class XPathTesterCore
{
    public sealed record XPathResult(string NodeType, string Name, string Value, string Path);
    public sealed record XPathExample(string Expression, string Description);

    public sealed class NamespaceEntry
    {
        public string Prefix { get; set; } = "";
        public string Uri { get; set; } = "";
    }

    public static readonly List<XPathExample> Examples =
    [
        new("//element", "Select all elements"),
        new("/root/child", "Absolute path"),
        new("//element[@attr]", "Elements with attribute"),
        new("//element[@attr='value']", "Attribute equals"),
        new("//element[text()='value']", "Text content match"),
        new("//element[position()=1]", "First element"),
        new("//element[contains(@class,'x')]", "Contains"),
        new("//element[starts-with(@id,'nav')]", "Starts with"),
        new("//element[not(@disabled)]", "Not"),
        new("count(//element)", "Count"),
    ];

    public static (List<XPathResult> Results, HashSet<int> HighlightedLines, string? ScalarResult, string? ScalarResultType, string? XmlError, string? XPathError) Evaluate(
        string xmlInput, string xpathExpression, List<NamespaceEntry> namespaces)
    {
        List<XPathResult> results = [];
        HashSet<int> highlightedLines = [];
        string? xmlError = null;
        string? xpathError = null;
        string? scalarResult = null;
        string? scalarResultType = null;

        if (string.IsNullOrWhiteSpace(xmlInput))
        {
            return (results, highlightedLines, null, null, "XML input is empty.", null);
        }

        if (string.IsNullOrWhiteSpace(xpathExpression))
        {
            return (results, highlightedLines, null, null, null, "XPath expression is empty.");
        }

        XPathDocument document;
        try
        {
            using StringReader stringReader = new StringReader(xmlInput);
            document = new XPathDocument(stringReader);
        }
        catch (XmlException ex)
        {
            return (results, highlightedLines, null, null, $"Line {ex.LineNumber}, Position {ex.LinePosition}: {ex.Message}", null);
        }

        XPathNavigator navigator = document.CreateNavigator();

        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(navigator.NameTable);
        foreach (NamespaceEntry ns in namespaces)
        {
            if (!string.IsNullOrWhiteSpace(ns.Prefix) && !string.IsNullOrWhiteSpace(ns.Uri))
            {
                try
                {
                    namespaceManager.AddNamespace(ns.Prefix, ns.Uri);
                }
                catch (XmlException ex)
                {
                    return (results, highlightedLines, null, null, null, $"Invalid namespace '{ns.Prefix}': {ex.Message}");
                }
            }
        }

        try
        {
            XPathExpression compiled = navigator.Compile(xpathExpression);
            compiled.SetContext(namespaceManager);

            if (compiled.ReturnType == XPathResultType.NodeSet)
            {
                XPathNodeIterator? iterator = navigator.Select(compiled);
                while (iterator.MoveNext())
                {
                    XPathNavigator? current = iterator.Current;
                    if (current is null) continue;

                    string path = BuildXPath(current);
                    string nodeType = current.NodeType.ToString();
                    string name = current.NodeType == XPathNodeType.Text
                        ? "#text"
                        : current.NodeType == XPathNodeType.Comment
                            ? "#comment"
                            : current.Name;
                    string value = current.Value;

                    results.Add(new XPathResult(nodeType, name, value, path));

                    if (current is IXmlLineInfo lineInfo && lineInfo.HasLineInfo())
                    {
                        highlightedLines.Add(lineInfo.LineNumber);
                    }
                    else
                    {
                        FindMatchingLines(current, xmlInput, highlightedLines);
                    }

                    if (results.Count >= 500) break;
                }
            }
            else
            {
                object? result = navigator.Evaluate(compiled);
                if (result is XPathNodeIterator nodeIterator)
                {
                    while (nodeIterator.MoveNext())
                    {
                        XPathNavigator? current = nodeIterator.Current;
                        if (current is null) continue;

                        results.Add(new XPathResult(
                            current.NodeType.ToString(),
                            current.Name,
                            current.Value,
                            BuildXPath(current)));

                        if (results.Count >= 500) break;
                    }
                }
                else if (result is not null)
                {
                    scalarResultType = result.GetType().Name;
                    scalarResult = result.ToString();
                }
            }
        }
        catch (XPathException ex)
        {
            xpathError = ex.Message;
        }

        return (results, highlightedLines, scalarResult, scalarResultType, xmlError, xpathError);
    }

    public static string BuildXPath(XPathNavigator node)
    {
        List<string> parts = [];
        XPathNavigator current = node.Clone();

        while (true)
        {
            string part = current.NodeType switch
            {
                XPathNodeType.Root => "",
                XPathNodeType.Element => BuildElementPart(current),
                XPathNodeType.Attribute => $"@{current.Name}",
                XPathNodeType.Text => "text()",
                XPathNodeType.Comment => "comment()",
                XPathNodeType.ProcessingInstruction => $"processing-instruction('{current.Name}')",
                _ => current.Name
            };

            if (!string.IsNullOrEmpty(part))
            {
                parts.Add(part);
            }

            if (!current.MoveToParent() || current.NodeType == XPathNodeType.Root)
            {
                break;
            }
        }

        parts.Reverse();
        return "/" + string.Join("/", parts);
    }

    private static string BuildElementPart(XPathNavigator element)
    {
        string name = element.Name;
        int position = 1;
        XPathNavigator sibling = element.Clone();

        while (sibling.MoveToPrevious())
        {
            if (sibling.NodeType == XPathNodeType.Element && sibling.Name == name)
            {
                position++;
            }
        }

        bool hasSiblingWithSameName = false;
        XPathNavigator next = element.Clone();
        while (next.MoveToNext())
        {
            if (next.NodeType == XPathNodeType.Element && next.Name == name)
            {
                hasSiblingWithSameName = true;
                break;
            }
        }

        if (position > 1 || hasSiblingWithSameName)
        {
            return $"{name}[{position}]";
        }

        return name;
    }

    private static void FindMatchingLines(XPathNavigator node, string xmlInput, HashSet<int> highlightedLines)
    {
        string searchTerm = node.NodeType switch
        {
            XPathNodeType.Element => $"<{node.Name}",
            XPathNodeType.Attribute => $"{node.Name}=",
            XPathNodeType.Comment => $"<!--",
            _ => node.Value.Trim()
        };

        if (string.IsNullOrEmpty(searchTerm)) return;

        string[] lines = xmlInput.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(searchTerm, StringComparison.Ordinal))
            {
                highlightedLines.Add(i + 1);
            }
        }
    }

    public static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Length <= maxLength ? value : value[..maxLength] + "…";
    }

    public static string NodeTypeBadgeClass(string nodeType) => nodeType switch
    {
        "Element" => "bg-emerald-100 dark:bg-emerald-900 text-emerald-700 dark:text-emerald-300",
        "Attribute" => "bg-amber-100 dark:bg-amber-900 text-amber-700 dark:text-amber-300",
        "Text" => "bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300",
        "Comment" => "bg-purple-100 dark:bg-purple-900 text-purple-700 dark:text-purple-300",
        _ => "bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300"
    };
}
