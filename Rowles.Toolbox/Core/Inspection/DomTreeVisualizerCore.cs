using System.Text;
using System.Text.RegularExpressions;

namespace Rowles.Toolbox.Core.Inspection;

public static class DomTreeVisualizerCore
{
    public static readonly HashSet<string> SelfClosingTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "hr", "img", "input",
        "link", "meta", "param", "source", "track", "wbr"
    };

    public static readonly Regex TagRegex = new(
        @"<(/?)(\w[\w-]*)((?:\s+[\w:.-]+(?:\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>""']*))?)*)\s*(\/?)>",
        RegexOptions.Compiled);

    public static readonly Regex AttrRegex = new(
        @"([\w:.-]+)(?:\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s>""']*)))?",
        RegexOptions.Compiled);

    public const string DefaultHtml = """
        <div id="app" class="container mx-auto">
          <header class="header bg-blue-500">
            <nav id="main-nav" class="flex items-center">
              <a href="/" class="logo">Home</a>
              <a href="/about">About</a>
            </nav>
          </header>
          <main class="content">
            <h1 class="title">Hello World</h1>
            <p>Welcome to the <strong>DOM Tree</strong> visualizer.</p>
            <img src="image.png" alt="Example" />
            <ul class="list">
              <li class="item active">Item 1</li>
              <li class="item">Item 2</li>
              <li class="item">Item 3</li>
            </ul>
          </main>
          <footer class="footer">
            <p>&copy; 2025</p>
          </footer>
        </div>
        """;

    public sealed class HtmlNode
    {
        public string TagName { get; set; } = string.Empty;
        public string? Id { get; set; }
        public List<string> Classes { get; set; } = [];
        public Dictionary<string, string> Attributes { get; set; } = new();
        public List<HtmlNode> Children { get; set; } = [];
        public int Depth { get; set; }
        public bool IsExpanded { get; set; } = true;
        public bool IsTextNode { get; set; }
        public string? TextContent { get; set; }
        public bool IsMatched { get; set; }
    }

    public static List<HtmlNode> ParseHtml(string inputHtml)
    {
        List<HtmlNode> rootNodes = [];

        if (string.IsNullOrWhiteSpace(inputHtml))
            return rootNodes;

        MatchCollection matches = TagRegex.Matches(inputHtml);
        Stack<HtmlNode> stack = new();
        int lastIndex = 0;

        foreach (Match match in matches)
        {
            if (match.Index > lastIndex)
            {
                string text = inputHtml[lastIndex..match.Index].Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    HtmlNode textNode = new()
                    {
                        IsTextNode = true,
                        TextContent = text,
                        Depth = stack.Count
                    };

                    if (stack.Count > 0)
                        stack.Peek().Children.Add(textNode);
                    else
                        rootNodes.Add(textNode);
                }
            }

            string isClosing = match.Groups[1].Value;
            string tagName = match.Groups[2].Value.ToLowerInvariant();
            string attributes = match.Groups[3].Value;
            string selfCloseSlash = match.Groups[4].Value;

            if (isClosing == "/")
            {
                if (stack.Count > 0)
                {
                    Stack<HtmlNode> temp = new();
                    bool found = false;

                    while (stack.Count > 0)
                    {
                        HtmlNode top = stack.Pop();
                        if (top.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                        temp.Push(top);
                    }

                    if (!found)
                    {
                        while (temp.Count > 0)
                            stack.Push(temp.Pop());
                    }
                }
            }
            else
            {
                HtmlNode node = new()
                {
                    TagName = tagName,
                    Depth = stack.Count,
                    IsExpanded = true
                };

                if (!string.IsNullOrWhiteSpace(attributes))
                {
                    MatchCollection attrMatches = AttrRegex.Matches(attributes);
                    foreach (Match attrMatch in attrMatches)
                    {
                        string name = attrMatch.Groups[1].Value;
                        string value = attrMatch.Groups[2].Success ? attrMatch.Groups[2].Value
                                     : attrMatch.Groups[3].Success ? attrMatch.Groups[3].Value
                                     : attrMatch.Groups[4].Success ? attrMatch.Groups[4].Value
                                     : string.Empty;

                        if (name.Equals("id", StringComparison.OrdinalIgnoreCase))
                            node.Id = value;
                        else if (name.Equals("class", StringComparison.OrdinalIgnoreCase))
                            node.Classes = [.. value.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
                        else
                            node.Attributes[name] = value;
                    }
                }

                if (stack.Count > 0)
                    stack.Peek().Children.Add(node);
                else
                    rootNodes.Add(node);

                bool isSelfClose = selfCloseSlash == "/" || SelfClosingTags.Contains(tagName);
                if (!isSelfClose)
                    stack.Push(node);
            }

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < inputHtml.Length)
        {
            string text = inputHtml[lastIndex..].Trim();
            if (!string.IsNullOrEmpty(text))
            {
                HtmlNode textNode = new()
                {
                    IsTextNode = true,
                    TextContent = text,
                    Depth = stack.Count
                };

                if (stack.Count > 0)
                    stack.Peek().Children.Add(textNode);
                else
                    rootNodes.Add(textNode);
            }
        }

        return rootNodes;
    }

    public static (int TotalElements, int MaxDepth, int ElementsWithId, int ElementsWithClasses, Dictionary<string, int> UniqueTags) ComputeStatistics(List<HtmlNode> rootNodes)
    {
        int totalElements = 0;
        int maxDepth = 0;
        int elementsWithId = 0;
        int elementsWithClasses = 0;
        Dictionary<string, int> uniqueTags = new();

        CollectStats(rootNodes, ref totalElements, ref maxDepth, ref elementsWithId, ref elementsWithClasses, uniqueTags);
        return (totalElements, maxDepth, elementsWithId, elementsWithClasses, uniqueTags);
    }

    private static void CollectStats(List<HtmlNode> nodes, ref int totalElements, ref int maxDepth,
        ref int elementsWithId, ref int elementsWithClasses, Dictionary<string, int> uniqueTags)
    {
        foreach (HtmlNode node in nodes)
        {
            if (!node.IsTextNode)
            {
                totalElements++;
                maxDepth = Math.Max(maxDepth, node.Depth + 1);

                if (node.Id is not null)
                    elementsWithId++;

                if (node.Classes.Count > 0)
                    elementsWithClasses++;

                if (uniqueTags.ContainsKey(node.TagName))
                    uniqueTags[node.TagName]++;
                else
                    uniqueTags[node.TagName] = 1;
            }

            CollectStats(node.Children, ref totalElements, ref maxDepth, ref elementsWithId, ref elementsWithClasses, uniqueTags);
        }
    }

    public static (string? FilterTag, string? FilterId, string? FilterClass) ParseSelector(string selector)
    {
        string? filterTag = null;
        string? filterId = null;
        string? filterClass = null;

        int hashIndex = selector.IndexOf('#');
        int dotIndex = selector.IndexOf('.');

        if (hashIndex >= 0)
        {
            filterTag = hashIndex > 0 ? selector[..hashIndex].ToLowerInvariant() : null;
            string remainder = selector[(hashIndex + 1)..];
            int nextDot = remainder.IndexOf('.');
            filterId = nextDot >= 0 ? remainder[..nextDot] : remainder;
        }
        else if (dotIndex >= 0)
        {
            filterTag = dotIndex > 0 ? selector[..dotIndex].ToLowerInvariant() : null;
            filterClass = selector[(dotIndex + 1)..];
        }
        else
        {
            filterTag = selector.ToLowerInvariant();
        }

        return (filterTag, filterId, filterClass);
    }

    public static void ClearMatches(List<HtmlNode> nodes)
    {
        foreach (HtmlNode node in nodes)
        {
            node.IsMatched = false;
            ClearMatches(node.Children);
        }
    }

    public static int MatchSelectorNodes(List<HtmlNode> nodes, string? tag, string? id, string? className)
    {
        int matchCount = 0;
        foreach (HtmlNode node in nodes)
        {
            if (!node.IsTextNode)
            {
                bool matches = true;

                if (tag is not null && !node.TagName.Equals(tag, StringComparison.OrdinalIgnoreCase))
                    matches = false;

                if (id is not null && !string.Equals(node.Id, id, StringComparison.OrdinalIgnoreCase))
                    matches = false;

                if (className is not null && !node.Classes.Exists(c => c.Equals(className, StringComparison.OrdinalIgnoreCase)))
                    matches = false;

                if (matches)
                {
                    node.IsMatched = true;
                    matchCount++;
                }
            }

            matchCount += MatchSelectorNodes(node.Children, tag, id, className);
        }
        return matchCount;
    }

    public static string GeneratePrettyPrint(List<HtmlNode> rootNodes)
    {
        StringBuilder sb = new();
        PrettyPrintNodes(sb, rootNodes, 0);
        return sb.ToString().TrimEnd();
    }

    private static void PrettyPrintNodes(StringBuilder sb, List<HtmlNode> nodes, int indent)
    {
        foreach (HtmlNode node in nodes)
        {
            string indentation = new(' ', indent * 2);

            if (node.IsTextNode)
            {
                sb.AppendLine($"{indentation}{node.TextContent}");
                continue;
            }

            sb.Append($"{indentation}<{node.TagName}");

            if (node.Id is not null)
                sb.Append($" id=\"{node.Id}\"");

            if (node.Classes.Count > 0)
                sb.Append($" class=\"{string.Join(' ', node.Classes)}\"");

            foreach (KeyValuePair<string, string> attr in node.Attributes)
                sb.Append($" {attr.Key}=\"{attr.Value}\"");

            if (SelfClosingTags.Contains(node.TagName))
            {
                sb.AppendLine(" />");
            }
            else if (node.Children.Count == 0)
            {
                sb.AppendLine($"></{node.TagName}>");
            }
            else if (node.Children.Count == 1 && node.Children[0].IsTextNode)
            {
                sb.AppendLine($">{node.Children[0].TextContent}</{node.TagName}>");
            }
            else
            {
                sb.AppendLine(">");
                PrettyPrintNodes(sb, node.Children, indent + 1);
                sb.AppendLine($"{indentation}</{node.TagName}>");
            }
        }
    }

    public static void SetExpandState(List<HtmlNode> nodes, bool expanded)
    {
        foreach (HtmlNode node in nodes)
        {
            node.IsExpanded = expanded;
            SetExpandState(node.Children, expanded);
        }
    }

    public static string TruncateText(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;
        return text[..maxLength] + "…";
    }
}
