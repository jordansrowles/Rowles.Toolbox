namespace Rowles.Toolbox.Core.MathConverters;

public static class ProjectileMotionCore
{
    public sealed record TrajectoryRow(double T, double X, double Y, double VxVal, double Vy);

    public static string F(double v) =>
        double.IsNaN(v) || double.IsInfinity(v) ? "—" : v.ToString("F2");
}
