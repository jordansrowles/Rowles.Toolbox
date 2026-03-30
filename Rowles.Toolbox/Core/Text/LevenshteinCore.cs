namespace Rowles.Toolbox.Core.Text;

public static class LevenshteinCore
{
    public enum EditType { Insert, Delete, Substitute }

    public sealed class EditOperation
    {
        public int Position { get; init; }
        public EditType Type { get; init; }
        public char? CharA { get; init; }
        public char? CharB { get; init; }
    }

    public static int CalculateDistance(string a, string b)
    {
        int lenA = a.Length;
        int lenB = b.Length;

        if (lenA == 0) return lenB;
        if (lenB == 0) return lenA;

        int[,] matrix = new int[lenA + 1, lenB + 1];

        for (int i = 0; i <= lenA; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= lenB; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= lenA; i++)
        {
            for (int j = 1; j <= lenB; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[lenA, lenB];
    }

    public static double CalculateSimilarity(string a, string b)
    {
        int maxLength = Math.Max(a.Length, b.Length);
        if (maxLength == 0) return 100.0;
        int distance = CalculateDistance(a, b);
        return (1.0 - (double)distance / maxLength) * 100.0;
    }

    public static List<EditOperation> BacktraceOperations(string a, string b)
    {
        int lenA = a.Length;
        int lenB = b.Length;

        if (lenA == 0 && lenB == 0)
            return new List<EditOperation>();

        int[,] matrix = new int[lenA + 1, lenB + 1];

        for (int i = 0; i <= lenA; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= lenB; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= lenA; i++)
        {
            for (int j = 1; j <= lenB; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        List<EditOperation> operations = new List<EditOperation>();
        int row = lenA;
        int col = lenB;

        while (row > 0 || col > 0)
        {
            if (row > 0 && col > 0 && a[row - 1] == b[col - 1])
            {
                row--;
                col--;
            }
            else if (row > 0 && col > 0 && matrix[row, col] == matrix[row - 1, col - 1] + 1)
            {
                operations.Add(new EditOperation
                {
                    Position = row,
                    Type = EditType.Substitute,
                    CharA = a[row - 1],
                    CharB = b[col - 1]
                });
                row--;
                col--;
            }
            else if (col > 0 && matrix[row, col] == matrix[row, col - 1] + 1)
            {
                operations.Add(new EditOperation
                {
                    Position = row + 1,
                    Type = EditType.Insert,
                    CharB = b[col - 1]
                });
                col--;
            }
            else if (row > 0 && matrix[row, col] == matrix[row - 1, col] + 1)
            {
                operations.Add(new EditOperation
                {
                    Position = row,
                    Type = EditType.Delete,
                    CharA = a[row - 1]
                });
                row--;
            }
            else
            {
                row--;
                col--;
            }
        }

        operations.Reverse();
        return operations;
    }
}
