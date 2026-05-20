using System.Globalization;
using System.Text.RegularExpressions;

namespace AiAgent2.Tools;

public class DateTool
{
    public static string Calculate(string query)
    {
        DateTime now = DateTime.Now;
        DateTime? result = NaturalDateParser(query, now);

        if (result == null)
            return "Unable to interpret the date request.";

        return result.Value.ToString("dddd, dd MMMM yyyy");
    }

    private static DateTime? NaturalDateParser(string input, DateTime now)
    {
        input = input.ToLower().Trim();

        // Remove ordinal suffixes: 23rd -> 23
        input = Regex.Replace(input, @"(\d+)(st|nd|rd|th)", "$1");

        // Replace "this year"
        if (input.Contains("this year"))
            input = input.Replace("this year", now.Year.ToString());

        // Remove "of" (23 of november -> 23 november)
        input = input.Replace(" of ", " ");

        // Try parsing with en-GB (day-month-year)
        if (DateTime.TryParse(input, new CultureInfo("en-GB"), DateTimeStyles.AllowWhiteSpaces, out DateTime gbParsed))
            return gbParsed;

        // Try parsing with invariant culture
        if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime invParsed))
            return invParsed;

        // Basic relative dates
        if (input.Contains("today")) return now;
        if (input.Contains("tomorrow")) return now.AddDays(1);
        if (input.Contains("yesterday")) return now.AddDays(-1);

        // Weekdays
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            string name = day.ToString().ToLower();

            if (input.Contains($"next {name}"))
                return NextWeekday(now, day);

            if (input.Contains($"this {name}"))
                return ThisWeekday(now, day);

            if (input.Contains($"last {name}"))
                return LastWeekday(now, day);

            if (input.Contains($"following {name}") || input.Contains($"{name} after"))
                return NextWeekday(now, day).AddDays(7);

            if (input.Contains($"{name} next week") || input.Contains($"next week {name}"))
                return NextWeekday(now, day).AddDays(7);
        }

        // “in X days”
        if (input.Contains("in") && input.Contains("day"))
        {
            int n = ExtractNumber(input);
            if (n != -1) return now.AddDays(n);
        }

        // “in X weeks”
        if (input.Contains("in") && input.Contains("week"))
        {
            int n = ExtractNumber(input);
            if (n != -1) return now.AddDays(n * 7);
        }

        return null;
    }

    private static int ExtractNumber(string input)
    {
        foreach (var part in input.Split(' '))
            if (int.TryParse(part, out int n))
                return n;
        return -1;
    }

    private static DateTime ThisWeekday(DateTime now, DayOfWeek target)
    {
        int diff = ((int)target - (int)now.DayOfWeek + 7) % 7;
        return now.AddDays(diff);
    }

    private static DateTime NextWeekday(DateTime now, DayOfWeek target)
    {
        int diff = ((int)target - (int)now.DayOfWeek + 7) % 7;
        if (diff == 0) diff = 7;
        return now.AddDays(diff);
    }

    private static DateTime LastWeekday(DateTime now, DayOfWeek target)
    {
        int diff = ((int)now.DayOfWeek - (int)target + 7) % 7;
        if (diff == 0) diff = 7;
        return now.AddDays(-diff);
    }
}
