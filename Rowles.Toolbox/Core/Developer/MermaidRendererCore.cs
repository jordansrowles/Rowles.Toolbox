namespace Rowles.Toolbox.Core.Developer;

public static class MermaidRendererCore
{
    public sealed record DiagramPreset(string Label, string Icon, string Code);

    public static readonly List<DiagramPreset> Presets =
    [
        new("Flowchart", "chart-dots-3",
            "graph TD\n    A[Start] --> B{Decision}\n    B -->|Yes| C[Action 1]\n    B -->|No| D[Action 2]\n    C --> E[End]\n    D --> E"),

        new("Sequence", "arrows-exchange",
            "sequenceDiagram\n    Alice->>Bob: Hello Bob\n    Bob-->>Alice: Hi Alice\n    Alice->>Bob: How are you?\n    Bob-->>Alice: Great!"),

        new("ER", "database",
            "erDiagram\n    CUSTOMER ||--o{ ORDER : places\n    ORDER ||--|{ LINE-ITEM : contains\n    PRODUCT ||--o{ LINE-ITEM : \"is in\""),

        new("Gantt", "calendar-time",
            "gantt\n    title Project Plan\n    dateFormat YYYY-MM-DD\n    section Phase 1\n    Design    :a1, 2024-01-01, 30d\n    Develop   :a2, after a1, 60d\n    section Phase 2\n    Testing   :a3, after a2, 20d\n    Deploy    :a4, after a3, 10d"),

        new("Class", "hierarchy-3",
            "classDiagram\n    Animal <|-- Duck\n    Animal <|-- Fish\n    Animal : +int age\n    Animal : +String gender\n    Animal : +swim()\n    Duck : +String beakColor\n    Duck : +quack()\n    Fish : +int sizeInFeet\n    Fish : +canEat()"),

        new("State", "arrows-split",
            "stateDiagram-v2\n    [*] --> Idle\n    Idle --> Processing : submit\n    Processing --> Success : done\n    Processing --> Error : fail\n    Success --> [*]\n    Error --> Idle : retry"),

        new("Pie", "chart-pie",
            "pie title Languages Used\n    \"C#\" : 45\n    \"TypeScript\" : 30\n    \"Python\" : 15\n    \"Go\" : 10"),
    ];
}
