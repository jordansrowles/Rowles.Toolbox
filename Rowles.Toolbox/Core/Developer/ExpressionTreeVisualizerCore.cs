using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rowles.Toolbox.Core.Developer;

public static class ExpressionTreeVisualizerCore
{
    // ─ TreeNode model ─

    public sealed class TreeNode
    {
        public string Role { get; set; } = "";
        public string NodeType { get; set; } = "";
        public string TypeName { get; set; } = "";
        public string? Value { get; set; }
        public string ColorCategory { get; set; } = "default";
        public List<TreeNode> Children { get; set; } = [];
        public bool IsExpanded { get; set; } = true;
    }

    // ─ Legend items ─

    public static readonly (string Label, string Category)[] LegendItems =
    [
        ("Lambda", "lambda"),
        ("Parameter", "parameter"),
        ("Constant", "constant"),
        ("Binary Op", "binary"),
        ("Unary Op", "unary"),
        ("Conditional", "conditional"),
        ("Member Access", "member"),
        ("Method Call", "call"),
        ("Convert", "convert"),
        ("Group", "group"),
    ];

    // ─ Type aliases ─

    private static readonly Dictionary<Type, string> s_typeAliases = new()
    {
        [typeof(int)]     = "int",
        [typeof(string)]  = "string",
        [typeof(bool)]    = "bool",
        [typeof(double)]  = "double",
        [typeof(float)]   = "float",
        [typeof(long)]    = "long",
        [typeof(short)]   = "short",
        [typeof(byte)]    = "byte",
        [typeof(char)]    = "char",
        [typeof(decimal)] = "decimal",
        [typeof(object)]  = "object",
        [typeof(void)]    = "void",
    };

    // ─ Colour scheme ─

    public static (string Border, string Bg, string Text) GetColors(string category) => category switch
    {
        "lambda"      => ("#818cf8", "rgba(99,102,241,0.1)",  "#818cf8"),
        "parameter"   => ("#60a5fa", "rgba(96,165,250,0.1)",  "#60a5fa"),
        "constant"    => ("#4ade80", "rgba(74,222,128,0.1)",   "#4ade80"),
        "binary"      => ("#c084fc", "rgba(192,132,252,0.1)",  "#c084fc"),
        "unary"       => ("#f472b6", "rgba(244,114,182,0.1)",  "#f472b6"),
        "conditional" => ("#fbbf24", "rgba(251,191,36,0.1)",   "#fbbf24"),
        "member"      => ("#2dd4bf", "rgba(45,212,191,0.1)",   "#2dd4bf"),
        "call"        => ("#fb923c", "rgba(251,146,60,0.1)",   "#fb923c"),
        "convert"     => ("#fb7185", "rgba(251,113,133,0.1)",  "#fb7185"),
        "group"       => ("#9ca3af", "rgba(156,163,175,0.05)", "#9ca3af"),
        _             => ("#9ca3af", "rgba(156,163,175,0.1)",  "#9ca3af"),
    };

    // ─ Type formatting ─

    public static string FormatType(Type type)
    {
        if (type.IsGenericType)
        {
            var name = type.Name[..type.Name.IndexOf('`')];
            var args = string.Join(", ", type.GetGenericArguments().Select(FormatType));
            return $"{name}<{args}>";
        }
        return s_typeAliases.TryGetValue(type, out var alias) ? alias : type.Name;
    }

    public static string FormatConstantDisplay(ConstantExpression expr)
    {
        if (expr.Value is null) return "null";
        return expr.Value switch
        {
            string s => $"\"{s}\"",
            bool b   => b ? "true" : "false",
            double d => d.ToString("G"),
            float f  => f.ToString("G") + "f",
            _        => expr.Value.ToString() ?? "null"
        };
    }

    public static string FormatConstantCode(ConstantExpression expr)
    {
        if (expr.Value is null) return "null";
        return expr.Value switch
        {
            int i    => i.ToString(),
            double d => d % 1 == 0 ? $"{d:F1}" : d.ToString("G"),
            float f  => f.ToString("G") + "f",
            long l   => l.ToString() + "L",
            string s => $"\"{s}\"",
            bool b   => b ? "true" : "false",
            _        => expr.Value.ToString() ?? "null"
        };
    }

    // ─ Build tree from expression ─

    public static TreeNode BuildTree(Expression expr, string role)
    {
        return expr switch
        {
            LambdaExpression lambda => new TreeNode
            {
                Role = role,
                NodeType = "Lambda",
                TypeName = FormatType(lambda.Type),
                ColorCategory = "lambda",
                Children =
                [
                    new TreeNode
                    {
                        Role = "Parameters",
                        NodeType = "Group",
                        TypeName = $"{lambda.Parameters.Count} param(s)",
                        ColorCategory = "group",
                        Children = lambda.Parameters.Select(p => BuildTree(p, "")).ToList()
                    },
                    BuildTree(lambda.Body, "Body")
                ]
            },

            BinaryExpression binary => new TreeNode
            {
                Role = role,
                NodeType = binary.NodeType.ToString(),
                TypeName = FormatType(binary.Type),
                ColorCategory = "binary",
                Children = [BuildTree(binary.Left, "Left"), BuildTree(binary.Right, "Right")]
            },

            UnaryExpression unary when unary.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked => new TreeNode
            {
                Role = role,
                NodeType = "Convert",
                Value = $"\u2192 {FormatType(unary.Type)}",
                TypeName = FormatType(unary.Type),
                ColorCategory = "convert",
                Children = [BuildTree(unary.Operand, "Operand")]
            },

            UnaryExpression unary when unary.NodeType == ExpressionType.TypeAs => new TreeNode
            {
                Role = role,
                NodeType = "TypeAs",
                Value = $"\u2192 {FormatType(unary.Type)}",
                TypeName = FormatType(unary.Type),
                ColorCategory = "convert",
                Children = [BuildTree(unary.Operand, "Operand")]
            },

            UnaryExpression unary => new TreeNode
            {
                Role = role,
                NodeType = unary.NodeType.ToString(),
                TypeName = FormatType(unary.Type),
                ColorCategory = "unary",
                Children = [BuildTree(unary.Operand, "Operand")]
            },

            ConditionalExpression cond => new TreeNode
            {
                Role = role,
                NodeType = "Conditional",
                TypeName = FormatType(cond.Type),
                ColorCategory = "conditional",
                Children =
                [
                    BuildTree(cond.Test, "Test"),
                    BuildTree(cond.IfTrue, "IfTrue"),
                    BuildTree(cond.IfFalse, "IfFalse")
                ]
            },

            MemberExpression member => BuildMemberNode(member, role),

            MethodCallExpression call => BuildCallNode(call, role),

            ParameterExpression param => new TreeNode
            {
                Role = role,
                NodeType = "Parameter",
                Value = param.Name,
                TypeName = FormatType(param.Type),
                ColorCategory = "parameter"
            },

            ConstantExpression constant => new TreeNode
            {
                Role = role,
                NodeType = "Constant",
                Value = FormatConstantDisplay(constant),
                TypeName = FormatType(constant.Type),
                ColorCategory = "constant"
            },

            InvocationExpression invoke => new TreeNode
            {
                Role = role,
                NodeType = "Invoke",
                TypeName = FormatType(invoke.Type),
                ColorCategory = "call",
                Children = new List<TreeNode> { BuildTree(invoke.Expression, "Target") }
                    .Concat(invoke.Arguments.Select((a, i) => BuildTree(a, $"Arg[{i}]")))
                    .ToList()
            },

            _ => new TreeNode
            {
                Role = role,
                NodeType = expr.NodeType.ToString(),
                TypeName = FormatType(expr.Type),
                ColorCategory = "default"
            }
        };
    }

    private static TreeNode BuildMemberNode(MemberExpression member, string role)
    {
        var children = new List<TreeNode>();
        if (member.Expression is not null)
        {
            children.Add(BuildTree(member.Expression, "Object"));
        }
        else if (member.Member.DeclaringType is not null)
        {
            children.Add(new TreeNode
            {
                Role = "Type",
                NodeType = "Static",
                Value = FormatType(member.Member.DeclaringType),
                TypeName = FormatType(member.Member.DeclaringType),
                ColorCategory = "group"
            });
        }

        return new TreeNode
        {
            Role = role,
            NodeType = "MemberAccess",
            Value = member.Member.Name,
            TypeName = FormatType(member.Type),
            ColorCategory = "member",
            Children = children
        };
    }

    private static TreeNode BuildCallNode(MethodCallExpression call, string role)
    {
        var children = new List<TreeNode>();

        if (call.Object is not null)
        {
            children.Add(BuildTree(call.Object, "Object"));
        }
        else if (call.Method.DeclaringType is not null)
        {
            children.Add(new TreeNode
            {
                Role = "Type",
                NodeType = "Static",
                Value = FormatType(call.Method.DeclaringType),
                TypeName = FormatType(call.Method.DeclaringType),
                ColorCategory = "group"
            });
        }

        if (call.Arguments.Count > 0)
        {
            children.Add(new TreeNode
            {
                Role = "Arguments",
                NodeType = "Group",
                TypeName = $"{call.Arguments.Count} arg(s)",
                ColorCategory = "group",
                Children = call.Arguments.Select((a, i) => BuildTree(a, $"[{i}]")).ToList()
            });
        }

        return new TreeNode
        {
            Role = role,
            NodeType = "Call",
            Value = call.Method.Name,
            TypeName = FormatType(call.Type),
            ColorCategory = "call",
            Children = children
        };
    }

    // ─ Construction code generation ─

    public static string GenerateConstructionCode(LambdaExpression lambda)
    {
        int varCounter = 0;
        Dictionary<ParameterExpression, string> paramVarNames = new();
        var sb = new StringBuilder();

        sb.AppendLine("// Build expression tree using System.Linq.Expressions");
        sb.AppendLine();

        foreach (var p in lambda.Parameters)
        {
            var varName = p.Name ?? $"p{paramVarNames.Count}";
            paramVarNames[p] = varName;
            sb.AppendLine($"var {varName} = Expression.Parameter(typeof({FormatType(p.Type)}), \"{p.Name}\");");
        }

        if (lambda.Parameters.Count > 0)
            sb.AppendLine();

        var bodyVar = EmitExprCode(lambda.Body, sb, ref varCounter, paramVarNames);

        sb.AppendLine();
        var funcType = FormatType(lambda.Type);
        if (lambda.Parameters.Count > 0)
        {
            var paramsStr = string.Join(", ", lambda.Parameters.Select(p => paramVarNames[p]));
            sb.AppendLine($"var lambda = Expression.Lambda<{funcType}>({bodyVar}, {paramsStr});");
        }
        else
        {
            sb.AppendLine($"var lambda = Expression.Lambda<{funcType}>({bodyVar});");
        }

        return sb.ToString().TrimEnd();
    }

    private static string EmitExprCode(Expression expr, StringBuilder sb, ref int varCounter, Dictionary<ParameterExpression, string> paramVarNames)
    {
        switch (expr)
        {
            case ParameterExpression param:
                return paramVarNames.TryGetValue(param, out var pn) ? pn : (param.Name ?? "param");

            case ConstantExpression constant:
            {
                varCounter++;
                var v = $"c{varCounter}";
                if (constant.Value is null)
                    sb.AppendLine($"var {v} = Expression.Constant(null, typeof({FormatType(constant.Type)}));");
                else
                    sb.AppendLine($"var {v} = Expression.Constant({FormatConstantCode(constant)});");
                return v;
            }

            case BinaryExpression binary:
            {
                var lv = EmitExprCode(binary.Left, sb, ref varCounter, paramVarNames);
                var rv = EmitExprCode(binary.Right, sb, ref varCounter, paramVarNames);
                varCounter++;
                var v = $"e{varCounter}";
                sb.AppendLine($"var {v} = Expression.{binary.NodeType}({lv}, {rv});");
                return v;
            }

            case UnaryExpression unary when unary.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked:
            {
                var ov = EmitExprCode(unary.Operand, sb, ref varCounter, paramVarNames);
                varCounter++;
                var v = $"e{varCounter}";
                sb.AppendLine($"var {v} = Expression.Convert({ov}, typeof({FormatType(unary.Type)}));");
                return v;
            }

            case UnaryExpression unary:
            {
                var ov = EmitExprCode(unary.Operand, sb, ref varCounter, paramVarNames);
                varCounter++;
                var v = $"e{varCounter}";
                sb.AppendLine($"var {v} = Expression.{unary.NodeType}({ov});");
                return v;
            }

            case ConditionalExpression cond:
            {
                var tv = EmitExprCode(cond.Test, sb, ref varCounter, paramVarNames);
                var trueV = EmitExprCode(cond.IfTrue, sb, ref varCounter, paramVarNames);
                var falseV = EmitExprCode(cond.IfFalse, sb, ref varCounter, paramVarNames);
                varCounter++;
                var v = $"e{varCounter}";
                sb.AppendLine($"var {v} = Expression.Condition({tv}, {trueV}, {falseV});");
                return v;
            }

            case MemberExpression member:
            {
                varCounter++;
                var v = $"e{varCounter}";
                var memberKind = member.Member is PropertyInfo ? "Property" : "Field";
                if (member.Expression is not null)
                {
                    var objV = EmitExprCode(member.Expression, sb, ref varCounter, paramVarNames);
                    sb.AppendLine($"var {v} = Expression.{memberKind}({objV}, \"{member.Member.Name}\");");
                }
                else
                {
                    sb.AppendLine($"var {v} = Expression.Property(null, typeof({FormatType(member.Member.DeclaringType!)}), \"{member.Member.Name}\");");
                }
                return v;
            }

            case MethodCallExpression call:
            {
                string? objV = null;
                if (call.Object is not null)
                    objV = EmitExprCode(call.Object, sb, ref varCounter, paramVarNames);

                var argVars = new List<string>();
                foreach (var arg in call.Arguments)
                    argVars.Add(EmitExprCode(arg, sb, ref varCounter, paramVarNames));

                varCounter++;
                var miV = $"mi{varCounter}";
                var v = $"e{varCounter}";

                if (call.Method.IsStatic && call.Object is null)
                {
                    if (call.Method.IsGenericMethod)
                    {
                        var typeArgs = string.Join(", ", call.Method.GetGenericArguments().Select(t => $"typeof({FormatType(t)})"));
                        sb.AppendLine($"// Find the {call.Method.Name} method and make it generic");
                        sb.AppendLine($"var {miV} = typeof({FormatType(call.Method.DeclaringType!)})");
                        sb.AppendLine($"    .GetMethods().First(m => m.Name == \"{call.Method.Name}\" && m.GetParameters().Length == {call.Method.GetParameters().Length})");
                        sb.AppendLine($"    .MakeGenericMethod({typeArgs});");
                    }
                    else
                    {
                        var paramTypes = string.Join(", ", call.Method.GetParameters().Select(p => $"typeof({FormatType(p.ParameterType)})"));
                        sb.AppendLine($"var {miV} = typeof({FormatType(call.Method.DeclaringType!)}).GetMethod(\"{call.Method.Name}\", [{paramTypes}])!;");
                    }
                    var argsStr = string.Join(", ", argVars);
                    sb.AppendLine($"var {v} = Expression.Call({miV}, {argsStr});");
                }
                else if (objV is not null)
                {
                    if (call.Method.GetParameters().Length == 0)
                    {
                        sb.AppendLine($"var {miV} = typeof({FormatType(call.Method.DeclaringType!)}).GetMethod(\"{call.Method.Name}\", Type.EmptyTypes)!;");
                    }
                    else
                    {
                        var paramTypes = string.Join(", ", call.Method.GetParameters().Select(p => $"typeof({FormatType(p.ParameterType)})"));
                        sb.AppendLine($"var {miV} = typeof({FormatType(call.Method.DeclaringType!)}).GetMethod(\"{call.Method.Name}\", [{paramTypes}])!;");
                    }
                    var argsStr = argVars.Count > 0 ? ", " + string.Join(", ", argVars) : "";
                    sb.AppendLine($"var {v} = Expression.Call({objV}, {miV}{argsStr});");
                }
                else
                {
                    sb.AppendLine($"var {v} = /* unresolved call to {call.Method.Name} */;");
                }
                return v;
            }

            case LambdaExpression nested:
            {
                var innerParams = new List<string>();
                foreach (var p in nested.Parameters)
                {
                    var name = $"inner_{p.Name ?? $"p{paramVarNames.Count}"}";
                    paramVarNames[p] = name;
                    innerParams.Add(name);
                    sb.AppendLine($"var {name} = Expression.Parameter(typeof({FormatType(p.Type)}), \"{p.Name}\");");
                }
                var bodyV = EmitExprCode(nested.Body, sb, ref varCounter, paramVarNames);
                varCounter++;
                var v = $"e{varCounter}";
                var ft = FormatType(nested.Type);
                var pStr = string.Join(", ", innerParams);
                sb.AppendLine($"var {v} = Expression.Lambda<{ft}>({bodyV}, {pStr});");
                return v;
            }

            default:
            {
                varCounter++;
                var v = $"e{varCounter}";
                sb.AppendLine($"// Unhandled node: {expr.NodeType}");
                sb.AppendLine($"var {v} = /* {expr.NodeType} */;");
                return v;
            }
        }
    }

    // ─ Expand/Collapse ─

    public static void SetExpandState(TreeNode? node, bool expanded)
    {
        if (node is null) return;
        node.IsExpanded = expanded;
        foreach (var child in node.Children)
            SetExpandState(child, expanded);
    }

    // ─ Preset expressions ─

    public static readonly List<(string Name, string Description, Func<LambdaExpression> Builder)> Presets =
    [
        (
            "x => x + 1",
            "Simple arithmetic \u2014 adds a constant to a parameter",
            () =>
            {
                var x = Expression.Parameter(typeof(int), "x");
                return Expression.Lambda<Func<int, int>>(
                    Expression.Add(x, Expression.Constant(1)), x);
            }
        ),
        (
            "x => x * x + 2 * x + 1",
            "Polynomial expression \u2014 demonstrates nested binary operations",
            () =>
            {
                var x = Expression.Parameter(typeof(int), "x");
                var xx = Expression.Multiply(x, x);
                var twoX = Expression.Multiply(Expression.Constant(2), x);
                var body = Expression.Add(Expression.Add(xx, twoX), Expression.Constant(1));
                return Expression.Lambda<Func<int, int>>(body, x);
            }
        ),
        (
            "(x, y) => x + y",
            "Binary parameters \u2014 lambda with two inputs",
            () =>
            {
                var x = Expression.Parameter(typeof(int), "x");
                var y = Expression.Parameter(typeof(int), "y");
                return Expression.Lambda<Func<int, int, int>>(
                    Expression.Add(x, y), x, y);
            }
        ),
        (
            "x => x > 5 && x < 10",
            "Boolean logic \u2014 AndAlso with two comparisons",
            () =>
            {
                var x = Expression.Parameter(typeof(int), "x");
                var gt5 = Expression.GreaterThan(x, Expression.Constant(5));
                var lt10 = Expression.LessThan(x, Expression.Constant(10));
                return Expression.Lambda<Func<int, bool>>(
                    Expression.AndAlso(gt5, lt10), x);
            }
        ),
        (
            "x => x > 0 ? x : -x",
            "Conditional \u2014 ternary operator (absolute value)",
            () =>
            {
                var x = Expression.Parameter(typeof(int), "x");
                var test = Expression.GreaterThan(x, Expression.Constant(0));
                return Expression.Lambda<Func<int, int>>(
                    Expression.Condition(test, x, Expression.Negate(x)), x);
            }
        ),
        (
            "s => s.Length > 3",
            "Member access \u2014 reads a property from a parameter",
            () =>
            {
                var s = Expression.Parameter(typeof(string), "s");
                var length = Expression.Property(s, "Length");
                return Expression.Lambda<Func<string, bool>>(
                    Expression.GreaterThan(length, Expression.Constant(3)), s);
            }
        ),
        (
            "s => s.ToUpper()",
            "Method call \u2014 instance method invocation",
            () =>
            {
                var s = Expression.Parameter(typeof(string), "s");
                var toUpper = typeof(string).GetMethod("ToUpper", Type.EmptyTypes)!;
                return Expression.Lambda<Func<string, string>>(
                    Expression.Call(s, toUpper), s);
            }
        ),
        (
            "x => Math.Sqrt(x)",
            "Static method call \u2014 calls a BCL math function",
            () =>
            {
                var x = Expression.Parameter(typeof(double), "x");
                var sqrt = typeof(Math).GetMethod("Sqrt", [typeof(double)])!;
                return Expression.Lambda<Func<double, double>>(
                    Expression.Call(sqrt, x), x);
            }
        ),
        (
            "(a, b) => a > b ? a : b",
            "Complex conditional \u2014 max of two values",
            () =>
            {
                var a = Expression.Parameter(typeof(int), "a");
                var b = Expression.Parameter(typeof(int), "b");
                return Expression.Lambda<Func<int, int, int>>(
                    Expression.Condition(Expression.GreaterThan(a, b), a, b), a, b);
            }
        ),
        (
            "x => x % 2 == 0",
            "Modulo check \u2014 tests if a number is even",
            () =>
            {
                var x = Expression.Parameter(typeof(int), "x");
                var mod = Expression.Modulo(x, Expression.Constant(2));
                return Expression.Lambda<Func<int, bool>>(
                    Expression.Equal(mod, Expression.Constant(0)), x);
            }
        ),
        (
            "(x, y) => x * x + y * y",
            "Pythagorean \u2014 sum of squares with two parameters",
            () =>
            {
                var x = Expression.Parameter(typeof(int), "x");
                var y = Expression.Parameter(typeof(int), "y");
                var xx = Expression.Multiply(x, x);
                var yy = Expression.Multiply(y, y);
                return Expression.Lambda<Func<int, int, int>>(
                    Expression.Add(xx, yy), x, y);
            }
        ),
        (
            "s => s != null && s.Length > 0",
            "Null check combined with member access",
            () =>
            {
                var s = Expression.Parameter(typeof(string), "s");
                var notNull = Expression.NotEqual(s, Expression.Constant(null, typeof(string)));
                var lengthGt0 = Expression.GreaterThan(
                    Expression.Property(s, "Length"), Expression.Constant(0));
                return Expression.Lambda<Func<string, bool>>(
                    Expression.AndAlso(notNull, lengthGt0), s);
            }
        ),
        (
            "x => (double)x / 100.0",
            "Type conversion \u2014 Convert node followed by division",
            () =>
            {
                var x = Expression.Parameter(typeof(int), "x");
                var conv = Expression.Convert(x, typeof(double));
                return Expression.Lambda<Func<int, double>>(
                    Expression.Divide(conv, Expression.Constant(100.0)), x);
            }
        ),
        (
            "() => DateTime.Now",
            "Parameterless lambda \u2014 static property access",
            () =>
            {
                var now = Expression.Property(null, typeof(DateTime), "Now");
                return Expression.Lambda<Func<DateTime>>(now);
            }
        ),
        (
            "items => items.Where(x => x > 5).Count()",
            "Nested lambda with LINQ \u2014 uses extension methods and an inner lambda predicate (complex tree)",
            () =>
            {
                var items = Expression.Parameter(typeof(IEnumerable<int>), "items");
                var x = Expression.Parameter(typeof(int), "x");
                var predicate = Expression.Lambda<Func<int, bool>>(
                    Expression.GreaterThan(x, Expression.Constant(5)), x);

                var whereMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(m => m.Name == "Where" && m.GetParameters().Length == 2)
                    .First(m => m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                    .MakeGenericMethod(typeof(int));

                var whereCall = Expression.Call(whereMethod, items, predicate);

                var countMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .First(m => m.Name == "Count" && m.GetParameters().Length == 1 && m.GetGenericArguments().Length == 1)
                    .MakeGenericMethod(typeof(int));

                var countCall = Expression.Call(countMethod, whereCall);
                return Expression.Lambda<Func<IEnumerable<int>, int>>(countCall, items);
            }
        ),
    ];
}
