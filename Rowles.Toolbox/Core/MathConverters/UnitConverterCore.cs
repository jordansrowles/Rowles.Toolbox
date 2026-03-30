namespace Rowles.Toolbox.Core.MathConverters;

public static class UnitConverterCore
{
    public enum UnitCategory { Length, Weight, Temperature, Volume, Area, Speed, Pressure, Energy }

    public record UnitDefinition(string Key, string Label, double Factor);

    public static readonly Dictionary<UnitCategory, List<UnitDefinition>> AllUnits = new()
    {
        [UnitCategory.Length] =
        [
            new("mm", "Millimetre (mm)", 0.001),
            new("cm", "Centimetre (cm)", 0.01),
            new("m", "Metre (m)", 1.0),
            new("km", "Kilometre (km)", 1000.0),
            new("in", "Inch (in)", 0.0254),
            new("ft", "Foot (ft)", 0.3048),
            new("yd", "Yard (yd)", 0.9144),
            new("mi", "Mile (mi)", 1609.344),
            new("nmi", "Nautical Mile (nmi)", 1852.0)
        ],
        [UnitCategory.Weight] =
        [
            new("mg", "Milligram (mg)", 0.000001),
            new("g", "Gram (g)", 0.001),
            new("kg", "Kilogram (kg)", 1.0),
            new("t", "Tonne (t)", 1000.0),
            new("oz", "Ounce (oz)", 0.028349523125),
            new("lb", "Pound (lb)", 0.45359237),
            new("st", "Stone (st)", 6.35029318)
        ],
        [UnitCategory.Temperature] =
        [
            new("C", "Celsius (°C)", 0),
            new("F", "Fahrenheit (°F)", 0),
            new("K", "Kelvin (K)", 0)
        ],
        [UnitCategory.Volume] =
        [
            new("mL", "Millilitre (mL)", 0.001),
            new("L", "Litre (L)", 1.0),
            new("gal_us", "Gallon (US)", 3.785411784),
            new("gal_uk", "Gallon (UK)", 4.54609),
            new("pt_us", "Pint (US)", 0.473176473),
            new("pt_uk", "Pint (UK)", 0.56826125),
            new("cup", "Cup (US)", 0.2365882365),
            new("tbsp", "Tablespoon (US)", 0.0147867648),
            new("tsp", "Teaspoon (US)", 0.00492892159),
            new("fl_oz", "Fluid Ounce (US)", 0.0295735296)
        ],
        [UnitCategory.Area] =
        [
            new("mm2", "Square Millimetre (mm²)", 0.000001),
            new("cm2", "Square Centimetre (cm²)", 0.0001),
            new("m2", "Square Metre (m²)", 1.0),
            new("km2", "Square Kilometre (km²)", 1000000.0),
            new("acre", "Acre", 4046.8564224),
            new("ha", "Hectare (ha)", 10000.0),
            new("sqft", "Square Foot (sq ft)", 0.09290304),
            new("sqin", "Square Inch (sq in)", 0.00064516),
            new("sqyd", "Square Yard (sq yd)", 0.83612736),
            new("sqmi", "Square Mile (sq mi)", 2589988.110336)
        ],
        [UnitCategory.Speed] =
        [
            new("ms", "Metres per second (m/s)", 1.0),
            new("kmh", "Kilometres per hour (km/h)", 0.277777778),
            new("mph", "Miles per hour (mph)", 0.44704),
            new("kn", "Knots (kn)", 0.514444444),
            new("fts", "Feet per second (ft/s)", 0.3048)
        ],
        [UnitCategory.Pressure] =
        [
            new("Pa", "Pascal (Pa)", 1.0),
            new("kPa", "Kilopascal (kPa)", 1000.0),
            new("bar", "Bar", 100000.0),
            new("atm", "Atmosphere (atm)", 101325.0),
            new("psi", "PSI", 6894.757293168),
            new("mmHg", "mmHg", 133.322387415)
        ],
        [UnitCategory.Energy] =
        [
            new("J", "Joule (J)", 1.0),
            new("kJ", "Kilojoule (kJ)", 1000.0),
            new("cal", "Calorie (cal)", 4.184),
            new("kcal", "Kilocalorie (kcal)", 4184.0),
            new("Wh", "Watt-hour (Wh)", 3600.0),
            new("kWh", "Kilowatt-hour (kWh)", 3600000.0),
            new("BTU", "BTU", 1055.05585262),
            new("eV", "Electronvolt (eV)", 1.602176634e-19)
        ]
    };

    public static double ConvertTemperature(string from, string to, double value)
    {
        double celsius = from switch
        {
            "C" => value,
            "F" => (value - 32.0) * 5.0 / 9.0,
            "K" => value - 273.15,
            _ => value
        };

        return to switch
        {
            "C" => celsius,
            "F" => celsius * 9.0 / 5.0 + 32.0,
            "K" => celsius + 273.15,
            _ => celsius
        };
    }

    public static string FormatNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) return "—";
        double abs = Math.Abs(value);
        if (abs == 0) return "0";
        if (abs < 0.000001) return value.ToString("E6");
        if (abs < 0.01) return value.ToString("G8");
        if (abs < 1) return value.ToString("0.######");
        if (abs < 1000) return value.ToString("0.####");
        if (abs < 1000000) return value.ToString("N2");
        return value.ToString("E6");
    }

    public static string CategoryIcon(UnitCategory cat) => cat switch
    {
        UnitCategory.Length => "ti-ruler-measure",
        UnitCategory.Weight => "ti-scale",
        UnitCategory.Temperature => "ti-temperature",
        UnitCategory.Volume => "ti-droplet",
        UnitCategory.Area => "ti-dimensions",
        UnitCategory.Speed => "ti-gauge",
        UnitCategory.Pressure => "ti-arrow-bar-down",
        UnitCategory.Energy => "ti-bolt",
        _ => "ti-calculator"
    };

    public static string CategoryLabel(UnitCategory cat) => cat switch
    {
        UnitCategory.Length => "Length",
        UnitCategory.Weight => "Weight/Mass",
        UnitCategory.Temperature => "Temperature",
        UnitCategory.Volume => "Volume",
        UnitCategory.Area => "Area",
        UnitCategory.Speed => "Speed",
        UnitCategory.Pressure => "Pressure",
        UnitCategory.Energy => "Energy",
        _ => cat.ToString()
    };

    public static double Convert(UnitCategory category, List<UnitDefinition> currentUnits, string from, string to, double value)
    {
        if (category == UnitCategory.Temperature)
        {
            return ConvertTemperature(from, to, value);
        }

        UnitDefinition? fromDef = currentUnits.Find(u => u.Key == from);
        UnitDefinition? toDef = currentUnits.Find(u => u.Key == to);
        if (fromDef is null || toDef is null) return 0;

        double baseValue = value * fromDef.Factor;
        return baseValue / toDef.Factor;
    }
}
