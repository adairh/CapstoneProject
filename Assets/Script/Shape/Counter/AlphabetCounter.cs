using System;
using System.Text.RegularExpressions;

public static class AlphabetCounter
{
    private static int count = 0; // Global counter

    /// <summary>
    /// Get the next alphabetic label in sequence.
    /// </summary>
    public static string Next()
    {
        string result = NumberToLabel(count);
        count++;
        return result;
    }

    /// <summary>
    /// Convert a number to an alphabetic label (e.g., 0 -> "A", 25 -> "Z", 26 -> "A'", etc.).
    /// </summary>
    public static string NumberToLabel(int num)
    {
        int cycle = num / 26;  // Number of full A-Z cycles
        char letter = (char)('A' + (num % 26)); // Current letter

        return cycle == 0 ? letter.ToString() : $"{letter}{new string('\'', cycle)}";
    }

    /// <summary>
    /// Convert an alphabetic label back to its numerical index.
    /// </summary>
    public static int LabelToNumber(string label)
    {
        if (string.IsNullOrEmpty(label) || !Regex.IsMatch(label, @"^[A-Z]+'*$"))
            throw new ArgumentException("Invalid label format");

        char letter = label[0];
        int baseValue = letter - 'A'; // Convert 'A' to 0, 'B' to 1, ..., 'Z' to 25

        int cycle = label.Length - 1; // Count number of apostrophes

        return baseValue + (cycle * 26);
    }

    /// <summary>
    /// Reset the counter.
    /// </summary>
    public static void Reset() => count = 0;

    public static int CurrentValue() => count;
}