using System.Globalization;

namespace Rowles.Toolbox.Core.MathConverters;

public static class WaveCalculatorCore
{
    public sealed class WaveExample
    {
        public string Label { get; init; } = "";
        public double Velocity { get; init; }
        public double Frequency { get; init; }
        public double Wavelength { get; init; }
        public string Medium { get; init; } = "custom";
        public string SolveFor { get; init; } = "wavelength";
    }

    public sealed class EmBand
    {
        public string Name { get; init; } = "";
        public double MinWavelength { get; init; }
        public double MaxWavelength { get; init; }
        public string Colour { get; init; } = "#888";
        public string RangeLabel { get; init; } = "";
    }

    public static readonly WaveExample[] QuickExamples = new WaveExample[]
    {
        new WaveExample { Label = "AM Radio (1 MHz)", Velocity = 3e8, Frequency = 1e6, Wavelength = 300.0, Medium = "light", SolveFor = "wavelength" },
        new WaveExample { Label = "FM Radio (100 MHz)", Velocity = 3e8, Frequency = 100e6, Wavelength = 3.0, Medium = "light", SolveFor = "wavelength" },
        new WaveExample { Label = "WiFi 2.4 GHz", Velocity = 3e8, Frequency = 2.4e9, Wavelength = 0.125, Medium = "light", SolveFor = "wavelength" },
        new WaveExample { Label = "WiFi 5 GHz", Velocity = 3e8, Frequency = 5e9, Wavelength = 0.06, Medium = "light", SolveFor = "wavelength" },
        new WaveExample { Label = "Green Light (550 nm)", Velocity = 3e8, Frequency = 5.454e14, Wavelength = 550e-9, Medium = "light", SolveFor = "frequency" },
        new WaveExample { Label = "Middle C (261.6 Hz)", Velocity = 343.0, Frequency = 261.6, Wavelength = 1.3117, Medium = "air", SolveFor = "wavelength" },
        new WaveExample { Label = "Ultrasound (40 kHz)", Velocity = 343.0, Frequency = 40000.0, Wavelength = 0.008575, Medium = "air", SolveFor = "wavelength" },
    };

    public static readonly EmBand[] EmBands = new EmBand[]
    {
        new EmBand { Name = "Gamma Ray", MinWavelength = 0, MaxWavelength = 1e-11, Colour = "#7c3aed", RangeLabel = "< 0.01 nm" },
        new EmBand { Name = "X-Ray", MinWavelength = 1e-11, MaxWavelength = 1e-8, Colour = "#6366f1", RangeLabel = "0.01 nm to 10 nm" },
        new EmBand { Name = "Ultraviolet", MinWavelength = 1e-8, MaxWavelength = 380e-9, Colour = "#8b5cf6", RangeLabel = "10 nm to 380 nm" },
        new EmBand { Name = "Visible Light", MinWavelength = 380e-9, MaxWavelength = 780e-9, Colour = "linear-gradient(90deg, #7c3aed, #3b82f6, #22c55e, #eab308, #f97316, #ef4444)", RangeLabel = "380 nm to 780 nm" },
        new EmBand { Name = "Infrared", MinWavelength = 780e-9, MaxWavelength = 1e-3, Colour = "#ef4444", RangeLabel = "780 nm to 1 mm" },
        new EmBand { Name = "Microwave", MinWavelength = 1e-3, MaxWavelength = 1.0, Colour = "#f97316", RangeLabel = "1 mm to 1 m" },
        new EmBand { Name = "Radio", MinWavelength = 1.0, MaxWavelength = 1e8, Colour = "#eab308", RangeLabel = "> 1 m" },
    };

    public static string F(double val)
    {
        return val.ToString("G6", CultureInfo.InvariantCulture);
    }

    public static string FormatSI(double val)
    {
        if (val == 0) return "0";

        double absVal = Math.Abs(val);

        if (absVal >= 1e12) return (val / 1e12).ToString("G4", CultureInfo.InvariantCulture) + " T";
        if (absVal >= 1e9)  return (val / 1e9).ToString("G4", CultureInfo.InvariantCulture)  + " G";
        if (absVal >= 1e6)  return (val / 1e6).ToString("G4", CultureInfo.InvariantCulture)  + " M";
        if (absVal >= 1e3)  return (val / 1e3).ToString("G4", CultureInfo.InvariantCulture)  + " k";
        if (absVal >= 1.0)  return val.ToString("G6", CultureInfo.InvariantCulture);
        if (absVal >= 1e-3) return (val * 1e3).ToString("G4", CultureInfo.InvariantCulture)  + " m";
        if (absVal >= 1e-6) return (val * 1e6).ToString("G4", CultureInfo.InvariantCulture)  + " u";
        if (absVal >= 1e-9) return (val * 1e9).ToString("G4", CultureInfo.InvariantCulture)  + " n";
        if (absVal >= 1e-12) return (val * 1e12).ToString("G4", CultureInfo.InvariantCulture) + " p";

        return val.ToString("G4", CultureInfo.InvariantCulture);
    }

    public static string StandingWaveSvgPath(int harmonicNumber)
    {
        System.Text.StringBuilder sb = new();
        int numPoints = 100;
        double startX = 10.0;
        double endX = 110.0;
        double spanX = endX - startX;
        double midY = 25.0;
        double amp = 15.0;

        sb.Append("<polyline points=\"");
        for (int i = 0; i <= numPoints; i++)
        {
            double t = (double)i / numPoints;
            double x = startX + t * spanX;
            double y = midY - amp * Math.Sin(harmonicNumber * Math.PI * t);

            if (i > 0) sb.Append(' ');
            sb.Append(x.ToString("F1", CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(y.ToString("F1", CultureInfo.InvariantCulture));
        }
        sb.Append("\" fill=\"none\" stroke=\"#3b82f6\" stroke-width=\"1.5\" stroke-linejoin=\"round\"/>");

        sb.Append("<polyline points=\"");
        for (int i = 0; i <= numPoints; i++)
        {
            double t = (double)i / numPoints;
            double x = startX + t * spanX;
            double y = midY + amp * Math.Sin(harmonicNumber * Math.PI * t);

            if (i > 0) sb.Append(' ');
            sb.Append(x.ToString("F1", CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(y.ToString("F1", CultureInfo.InvariantCulture));
        }
        sb.Append("\" fill=\"none\" stroke=\"#93c5fd\" stroke-width=\"1\" stroke-dasharray=\"3,2\" stroke-linejoin=\"round\"/>");

        return sb.ToString();
    }

    public static string WavelengthToHexColour(double wavelengthM)
    {
        double nm = wavelengthM * 1e9;

        double r = 0;
        double g = 0;
        double b = 0;

        if (nm >= 380 && nm < 440)
        {
            r = -(nm - 440.0) / (440.0 - 380.0);
            g = 0.0;
            b = 1.0;
        }
        else if (nm >= 440 && nm < 490)
        {
            r = 0.0;
            g = (nm - 440.0) / (490.0 - 440.0);
            b = 1.0;
        }
        else if (nm >= 490 && nm < 510)
        {
            r = 0.0;
            g = 1.0;
            b = -(nm - 510.0) / (510.0 - 490.0);
        }
        else if (nm >= 510 && nm < 580)
        {
            r = (nm - 510.0) / (580.0 - 510.0);
            g = 1.0;
            b = 0.0;
        }
        else if (nm >= 580 && nm < 645)
        {
            r = 1.0;
            g = -(nm - 645.0) / (645.0 - 580.0);
            b = 0.0;
        }
        else if (nm >= 645 && nm <= 780)
        {
            r = 1.0;
            g = 0.0;
            b = 0.0;
        }

        double factor = 1.0;
        if (nm >= 380 && nm < 420)
        {
            factor = 0.3 + 0.7 * (nm - 380.0) / (420.0 - 380.0);
        }
        else if (nm >= 701 && nm <= 780)
        {
            factor = 0.3 + 0.7 * (780.0 - nm) / (780.0 - 700.0);
        }

        int ri = (int)(255.0 * Math.Pow(r * factor, 0.8));
        int gi = (int)(255.0 * Math.Pow(g * factor, 0.8));
        int bi = (int)(255.0 * Math.Pow(b * factor, 0.8));

        ri = Math.Clamp(ri, 0, 255);
        gi = Math.Clamp(gi, 0, 255);
        bi = Math.Clamp(bi, 0, 255);

        return $"#{ri:X2}{gi:X2}{bi:X2}";
    }
}
