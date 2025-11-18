using System;
public static class LongFormatter
{
    private static string[] units = { "", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};

    public static string FormatLong(this long value, int dec = 1)
    {
        // Start with base unit (empty string)
        int unitIndex = 0;
        double doubleValue = (double)value;

        // Convert to higher units
        while (unitIndex < units.Length - 1 && doubleValue >= 1000)
        {
            doubleValue /= 1000;
            unitIndex++;
        }

        // Round to specified decimal places
        Decimal decimalValue = Math.Round((decimal)doubleValue, dec);

        return $"{decimalValue}{units[unitIndex]}";
    }

    public static string FormatLong(this int value, int dec = 1)
    {
        // Start with base unit (empty string)
        int unitIndex = 0;
        decimal decimalValue = (decimal)value;

        // Convert to higher units
        while (unitIndex < units.Length - 1 && decimalValue >= 1000)
        {
            decimalValue /= 1000;
            unitIndex++;
        }

        // Round to specified decimal places
        decimalValue = Math.Round(decimalValue, dec);

        return $"{decimalValue}{units[unitIndex]}";
    }
}
