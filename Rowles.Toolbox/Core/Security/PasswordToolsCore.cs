namespace Rowles.Toolbox.Core.Security;

public static class PasswordToolsCore
{
    public static string BuildCharPool(bool includeLowercase, bool includeUppercase, bool includeDigits, bool includeSymbols, bool excludeAmbiguous)
    {
        const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string DigitChars = "0123456789";
        const string SymbolChars = "!@#$%^&*()_+-=[]{}|;:',.<>?/~";
        const string AmbiguousChars = "0O1Il|";

        System.Text.StringBuilder pool = new();
        if (includeLowercase) pool.Append(LowercaseChars);
        if (includeUppercase) pool.Append(UppercaseChars);
        if (includeDigits) pool.Append(DigitChars);
        if (includeSymbols) pool.Append(SymbolChars);

        if (!excludeAmbiguous) return pool.ToString();

        System.Text.StringBuilder filtered = new(pool.Length);
        foreach (char c in pool.ToString())
        {
            if (!AmbiguousChars.Contains(c))
                filtered.Append(c);
        }
        return filtered.ToString();
    }

    public static int GetPoolSize(bool hasLower, bool hasUpper, bool hasDigit, bool hasSymbol)
    {
        int size = 0;
        if (hasLower) size += 26;
        if (hasUpper) size += 26;
        if (hasDigit) size += 10;
        if (hasSymbol) size += 33;
        return size;
    }

    public static double CalculateEntropy(int passwordLength, int poolSize)
    {
        if (passwordLength == 0 || poolSize == 0) return 0;
        return passwordLength * Math.Log2(poolSize);
    }

    public static string GetStrengthLabel(double entropy) => entropy switch
    {
        0 => "",
        < 28 => "Weak",
        < 36 => "Fair",
        < 60 => "Good",
        _ => "Strong"
    };

    public static string GetStrengthBarColor(double entropy) => entropy switch
    {
        < 28 => "bg-red-500",
        < 36 => "bg-yellow-500",
        < 60 => "bg-blue-500",
        _ => "bg-green-500"
    };

    public static string GetStrengthTextColor(double entropy) => entropy switch
    {
        < 28 => "text-red-600 dark:text-red-400",
        < 36 => "text-yellow-600 dark:text-yellow-400",
        < 60 => "text-blue-600 dark:text-blue-400",
        _ => "text-green-600 dark:text-green-400"
    };

    public static int GetStrengthPercent(double entropy) => (int)Math.Min(100, entropy * 100.0 / 128.0);

    public static string GetCrackTimeEstimate(double entropy)
    {
        if (entropy <= 0) return "\u2014";
        double combinations = Math.Pow(2, entropy);
        double seconds = combinations / 10_000_000_000.0;
        return FormatDuration(seconds);
    }

    public static string FormatDuration(double seconds)
    {
        if (seconds < 0.001) return "Instantly";
        if (seconds < 1) return "Less than a second";
        if (seconds < 60) return $"{seconds:F0} seconds";
        if (seconds < 3600) return $"{seconds / 60:F0} minutes";
        if (seconds < 86400) return $"{seconds / 3600:F1} hours";
        if (seconds < 31_536_000) return $"{seconds / 86400:F0} days";

        double years = seconds / 31_536_000;
        if (years < 100) return $"{years:F1} years";
        if (years < 1_000) return $"{years:N0} years";
        if (years < 1_000_000) return $"{years / 1_000:F1} thousand years";
        if (years < 1_000_000_000) return $"{years / 1_000_000:F1} million years";
        if (years < 1_000_000_000_000) return $"{years / 1_000_000_000:F1} billion years";
        return "Trillions of years+";
    }

    public static List<string> GetSuggestions(string password, bool hasLower, bool hasUpper, bool hasDigit, bool hasSymbol)
    {
        List<string> suggestions = [];
        if (string.IsNullOrEmpty(password)) return suggestions;

        if (password.Length < 8)
            suggestions.Add("Password is too short \u2014 use at least 8 characters.");
        else if (password.Length < 12)
            suggestions.Add("Use at least 12 characters for better security.");

        if (!hasLower) suggestions.Add("Add lowercase letters (a\u2013z).");
        if (!hasUpper) suggestions.Add("Add uppercase letters (A\u2013Z).");
        if (!hasDigit) suggestions.Add("Add digits (0\u20139).");
        if (!hasSymbol) suggestions.Add("Add special characters (!@#$\u2026).");

        if (password.Length > 0 && password.Distinct().Count() < password.Length / 2)
            suggestions.Add("Avoid repeating characters too often.");

        return suggestions;
    }
}
