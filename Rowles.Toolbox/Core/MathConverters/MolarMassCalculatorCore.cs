using System.Globalization;

namespace Rowles.Toolbox.Core.MathConverters;

public static class MolarMassCalculatorCore
{
    public enum ElementCategory { Metal, Nonmetal, NobleGas }

    public sealed record ElementData(
        double AtomicMass,
        string Name,
        int AtomicNumber,
        ElementCategory Category);

    public sealed record ElementRow(
        string Symbol,
        string Name,
        int Count,
        double AtomicMass,
        double Contribution,
        double PercentByMass,
        int AtomicNumber,
        ElementCategory Category);

    public sealed record ParseResult(
        Dictionary<string, int>? Elements = null,
        string? Error = null);

    // ── Formula parser (stack-based recursive descent) ────────────────────────
    //
    //  Grammar handled:
    //    formula    = term+
    //    term       = element count? | '(' formula ')' count?
    //    element    = [A-Z][a-z]?
    //    count      = [0-9]+

    public static ParseResult ParseFormula(string formula)
    {
        Stack<Dictionary<string, int>> stack = new();
        stack.Push(new Dictionary<string, int>());

        int i = 0;
        while (i < formula.Length)
        {
            char c = formula[i];

            if (c == '(')
            {
                stack.Push(new Dictionary<string, int>());
                i++;
            }
            else if (c == ')')
            {
                if (stack.Count < 2)
                    return new ParseResult(Error: "Unbalanced parentheses: ')' has no matching '('.");

                i++;
                int multiplier = 0;
                while (i < formula.Length && char.IsDigit(formula[i]))
                {
                    multiplier = multiplier * 10 + (formula[i] - '0');
                    i++;
                }
                if (multiplier == 0) multiplier = 1;

                Dictionary<string, int> group = stack.Pop();
                Dictionary<string, int> parent = stack.Peek();
                foreach (KeyValuePair<string, int> pair in group)
                {
                    if (!parent.TryAdd(pair.Key, pair.Value * multiplier))
                        parent[pair.Key] += pair.Value * multiplier;
                }
            }
            else if (char.IsUpper(c))
            {
                int start = i++;
                while (i < formula.Length && char.IsLower(formula[i]))
                    i++;
                string symbol = formula[start..i];

                int count = 0;
                while (i < formula.Length && char.IsDigit(formula[i]))
                {
                    count = count * 10 + (formula[i] - '0');
                    i++;
                }
                if (count == 0) count = 1;

                Dictionary<string, int> current = stack.Peek();
                if (!current.TryAdd(symbol, count))
                    current[symbol] += count;
            }
            else
            {
                return new ParseResult(Error: $"Unexpected character '{c}' at position {i + 1} in the formula.");
            }
        }

        if (stack.Count != 1)
            return new ParseResult(Error: "Unbalanced parentheses: one or more '(' are missing a closing ')'.");

        return new ParseResult(Elements: stack.Pop());
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    public static string RowBgClass(ElementCategory cat) => cat switch
    {
        ElementCategory.Metal    => "bg-amber-50/60 dark:bg-amber-900/10",
        ElementCategory.Nonmetal => "bg-emerald-50/60 dark:bg-emerald-900/10",
        ElementCategory.NobleGas => "bg-purple-50/60 dark:bg-purple-900/10",
        _                        => string.Empty
    };

    public static string ProgressBarClass(ElementCategory cat) => cat switch
    {
        ElementCategory.Metal    => "bg-amber-400",
        ElementCategory.Nonmetal => "bg-emerald-500",
        ElementCategory.NobleGas => "bg-purple-500",
        _                        => "bg-blue-500"
    };

    public static string BarWidthStyle(double pct) =>
        $"width: {Math.Min(100.0, Math.Max(0.0, pct)).ToString("0.##", CultureInfo.InvariantCulture)}%";

    // ── Periodic table factory helpers ────────────────────────────────────────

    public static ElementData Met(double m, string n, int z) => new(m, n, z, ElementCategory.Metal);
    public static ElementData Non(double m, string n, int z) => new(m, n, z, ElementCategory.Nonmetal);
    public static ElementData Gas(double m, string n, int z) => new(m, n, z, ElementCategory.NobleGas);

    // ── Periodic table — all 118 elements, IUPAC standard atomic weights ──────

    public static readonly Dictionary<string, ElementData> PeriodicTable = new()
    {
        // Period 1
        ["H"]  = Non(1.008,   "Hydrogen",        1),
        ["He"] = Gas(4.003,   "Helium",           2),
        // Period 2
        ["Li"] = Met(6.941,   "Lithium",          3),
        ["Be"] = Met(9.012,   "Beryllium",        4),
        ["B"]  = Non(10.811,  "Boron",            5),
        ["C"]  = Non(12.011,  "Carbon",           6),
        ["N"]  = Non(14.007,  "Nitrogen",         7),
        ["O"]  = Non(15.999,  "Oxygen",           8),
        ["F"]  = Non(18.998,  "Fluorine",         9),
        ["Ne"] = Gas(20.180,  "Neon",            10),
        // Period 3
        ["Na"] = Met(22.990,  "Sodium",          11),
        ["Mg"] = Met(24.305,  "Magnesium",       12),
        ["Al"] = Met(26.982,  "Aluminium",       13),
        ["Si"] = Non(28.086,  "Silicon",         14),
        ["P"]  = Non(30.974,  "Phosphorus",      15),
        ["S"]  = Non(32.065,  "Sulfur",          16),
        ["Cl"] = Non(35.453,  "Chlorine",        17),
        ["Ar"] = Gas(39.948,  "Argon",           18),
        // Period 4
        ["K"]  = Met(39.098,  "Potassium",       19),
        ["Ca"] = Met(40.078,  "Calcium",         20),
        ["Sc"] = Met(44.956,  "Scandium",        21),
        ["Ti"] = Met(47.867,  "Titanium",        22),
        ["V"]  = Met(50.942,  "Vanadium",        23),
        ["Cr"] = Met(51.996,  "Chromium",        24),
        ["Mn"] = Met(54.938,  "Manganese",       25),
        ["Fe"] = Met(55.845,  "Iron",            26),
        ["Co"] = Met(58.933,  "Cobalt",          27),
        ["Ni"] = Met(58.693,  "Nickel",          28),
        ["Cu"] = Met(63.546,  "Copper",          29),
        ["Zn"] = Met(65.38,   "Zinc",            30),
        ["Ga"] = Met(69.723,  "Gallium",         31),
        ["Ge"] = Non(72.630,  "Germanium",       32),
        ["As"] = Non(74.922,  "Arsenic",         33),
        ["Se"] = Non(78.971,  "Selenium",        34),
        ["Br"] = Non(79.904,  "Bromine",         35),
        ["Kr"] = Gas(83.798,  "Krypton",         36),
        // Period 5
        ["Rb"] = Met(85.468,  "Rubidium",        37),
        ["Sr"] = Met(87.620,  "Strontium",       38),
        ["Y"]  = Met(88.906,  "Yttrium",         39),
        ["Zr"] = Met(91.224,  "Zirconium",       40),
        ["Nb"] = Met(92.906,  "Niobium",         41),
        ["Mo"] = Met(95.960,  "Molybdenum",      42),
        ["Tc"] = Met(98.0,    "Technetium",      43),
        ["Ru"] = Met(101.07,  "Ruthenium",       44),
        ["Rh"] = Met(102.91,  "Rhodium",         45),
        ["Pd"] = Met(106.42,  "Palladium",       46),
        ["Ag"] = Met(107.87,  "Silver",          47),
        ["Cd"] = Met(112.41,  "Cadmium",         48),
        ["In"] = Met(114.82,  "Indium",          49),
        ["Sn"] = Met(118.71,  "Tin",             50),
        ["Sb"] = Non(121.76,  "Antimony",        51),
        ["Te"] = Non(127.60,  "Tellurium",       52),
        ["I"]  = Non(126.90,  "Iodine",          53),
        ["Xe"] = Gas(131.29,  "Xenon",           54),
        // Period 6
        ["Cs"] = Met(132.91,  "Caesium",         55),
        ["Ba"] = Met(137.33,  "Barium",          56),
        ["La"] = Met(138.91,  "Lanthanum",       57),
        ["Ce"] = Met(140.12,  "Cerium",          58),
        ["Pr"] = Met(140.91,  "Praseodymium",    59),
        ["Nd"] = Met(144.24,  "Neodymium",       60),
        ["Pm"] = Met(145.0,   "Promethium",      61),
        ["Sm"] = Met(150.36,  "Samarium",        62),
        ["Eu"] = Met(151.96,  "Europium",        63),
        ["Gd"] = Met(157.25,  "Gadolinium",      64),
        ["Tb"] = Met(158.93,  "Terbium",         65),
        ["Dy"] = Met(162.50,  "Dysprosium",      66),
        ["Ho"] = Met(164.93,  "Holmium",         67),
        ["Er"] = Met(167.26,  "Erbium",          68),
        ["Tm"] = Met(168.93,  "Thulium",         69),
        ["Yb"] = Met(173.05,  "Ytterbium",       70),
        ["Lu"] = Met(174.97,  "Lutetium",        71),
        ["Hf"] = Met(178.49,  "Hafnium",         72),
        ["Ta"] = Met(180.95,  "Tantalum",        73),
        ["W"]  = Met(183.84,  "Tungsten",        74),
        ["Re"] = Met(186.21,  "Rhenium",         75),
        ["Os"] = Met(190.23,  "Osmium",          76),
        ["Ir"] = Met(192.22,  "Iridium",         77),
        ["Pt"] = Met(195.08,  "Platinum",        78),
        ["Au"] = Met(196.97,  "Gold",            79),
        ["Hg"] = Met(200.59,  "Mercury",         80),
        ["Tl"] = Met(204.38,  "Thallium",        81),
        ["Pb"] = Met(207.20,  "Lead",            82),
        ["Bi"] = Met(208.98,  "Bismuth",         83),
        ["Po"] = Met(209.0,   "Polonium",        84),
        ["At"] = Non(210.0,   "Astatine",        85),
        ["Rn"] = Gas(222.0,   "Radon",           86),
        // Period 7
        ["Fr"] = Met(223.0,   "Francium",        87),
        ["Ra"] = Met(226.0,   "Radium",          88),
        ["Ac"] = Met(227.0,   "Actinium",        89),
        ["Th"] = Met(232.04,  "Thorium",         90),
        ["Pa"] = Met(231.04,  "Protactinium",    91),
        ["U"]  = Met(238.03,  "Uranium",         92),
        ["Np"] = Met(237.0,   "Neptunium",       93),
        ["Pu"] = Met(244.0,   "Plutonium",       94),
        ["Am"] = Met(243.0,   "Americium",       95),
        ["Cm"] = Met(247.0,   "Curium",          96),
        ["Bk"] = Met(247.0,   "Berkelium",       97),
        ["Cf"] = Met(251.0,   "Californium",     98),
        ["Es"] = Met(252.0,   "Einsteinium",     99),
        ["Fm"] = Met(257.0,   "Fermium",        100),
        ["Md"] = Met(258.0,   "Mendelevium",    101),
        ["No"] = Met(259.0,   "Nobelium",       102),
        ["Lr"] = Met(262.0,   "Lawrencium",     103),
        // Transactinides (periods 7, groups 4-18)
        ["Rf"] = Met(267.0,   "Rutherfordium",  104),
        ["Db"] = Met(268.0,   "Dubnium",        105),
        ["Sg"] = Met(271.0,   "Seaborgium",     106),
        ["Bh"] = Met(272.0,   "Bohrium",        107),
        ["Hs"] = Met(270.0,   "Hassium",        108),
        ["Mt"] = Met(278.0,   "Meitnerium",     109),
        ["Ds"] = Met(281.0,   "Darmstadtium",   110),
        ["Rg"] = Met(282.0,   "Roentgenium",    111),
        ["Cn"] = Met(285.0,   "Copernicium",    112),
        ["Nh"] = Met(286.0,   "Nihonium",       113),
        ["Fl"] = Met(289.0,   "Flerovium",      114),
        ["Mc"] = Met(290.0,   "Moscovium",      115),
        ["Lv"] = Met(293.0,   "Livermorium",    116),
        ["Ts"] = Non(294.0,   "Tennessine",     117),
        ["Og"] = Gas(294.0,   "Oganesson",      118),
    };
}
