using System.Globalization;

namespace OrderSystem.Helpers;

internal static class Display
{
    public static bool UsePlainText { get; set; }
    public static bool IsInteractive => !UsePlainText && !Console.IsInputRedirected && !Console.IsOutputRedirected;
    private static bool PlainConsole => !IsInteractive;
    private static int Width => PlainConsole ? 100 : Math.Max(1, Console.WindowWidth - 1);
    private static int ContentHeight(string footer) => PlainConsole ? 30
        : Math.Max(1, Console.WindowHeight - 3 - WrapLines(footer, Width).Length);

    public static int ShowMenu(string title, string[] options, int selected = 0,
        string backAction = "Back", Func<int, string>? describeAction = null)
    {
        if (options.Length == 0) return -1;
        selected = Math.Clamp(selected, 0, options.Length - 1);
        while (true)
        {
            var lines = options.Select((option, index) =>
                $"{(index == selected ? ">" : " ")} {index + 1}. {option}").ToArray();
            string footer = PlainConsole
                ? $"Enter option number (1-{options.Length}).\n0 / :back: {backAction}"
                : $"Up/Down: Move | 1-{Math.Min(9, options.Length)}: Select\n" +
                  $"Enter: {describeAction?.Invoke(selected) ?? options[selected]}\nEsc: {backAction}";
            Draw(title, lines, footer,
                PlainConsole ? 0 : Math.Max(0, selected - ContentHeight(footer) + 1));
            if (PlainConsole)
            {
                var input = Console.ReadLine();
                if (input is null || input.Trim() is "0" or ":back") return -1;
                if (int.TryParse(input, out int choice) && choice >= 1 && choice <= options.Length)
                    return choice - 1;
                Console.WriteLine($"Choose a number from 1 to {options.Length}.");
                continue;
            }
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.UpArrow: selected = (selected + options.Length - 1) % options.Length; break;
                case ConsoleKey.DownArrow: selected = (selected + 1) % options.Length; break;
                case ConsoleKey.Enter: return selected;
                case ConsoleKey.Escape: return -1;
                default:
                    if (key.KeyChar >= '1' && key.KeyChar <= '9' && key.KeyChar - '1' < options.Length)
                        return key.KeyChar - '1';
                    break;
            }
        }
    }

    public static void ShowScreen(string title, Action displayContent, string returnAction = "Return to previous menu")
    {
        using var output = new StringWriter();
        TextWriter original = Console.Out;
        try
        {
            Console.SetOut(output);
            displayContent();
        }
        finally { Console.SetOut(original); }
        var content = output.ToString().Replace("\r\n", "\n").TrimEnd('\n');
        if (PlainConsole)
        {
            Draw(title, content.Split('\n'), $"Next: {returnAction} (automatic)");
            return;
        }
        int firstLine = 0;
        while (true)
        {
            var lines = WrapLines(content, Width);
            string footer = $"Enter / Esc: {returnAction}";
            if (lines.Length > ContentHeight(footer))
                footer = "Up/Down: Scroll | PgUp/PgDn: Page\n" + footer;
            int pageHeight = ContentHeight(footer);
            int lastStart = Math.Max(0, lines.Length - pageHeight);
            firstLine = Math.Clamp(firstLine, 0, lastStart);
            Draw(title, lines, footer, firstLine);
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow: firstLine = Math.Max(0, firstLine - 1); break;
                case ConsoleKey.DownArrow: firstLine = Math.Min(lastStart, firstLine + 1); break;
                case ConsoleKey.PageUp: firstLine = Math.Max(0, firstLine - pageHeight); break;
                case ConsoleKey.PageDown: firstLine = Math.Min(lastStart, firstLine + pageHeight); break;
                case ConsoleKey.Enter:
                case ConsoleKey.Escape: return;
            }
        }
    }

    public static void Message(string title, string message, string returnAction = "Return to previous menu")
        => ShowScreen(title, () => Console.WriteLine(message), returnAction);

    public static string? ReadText(string title, string prompt,
        string submitAction = "Use this value", string cancelAction = "Cancel entry")
    {
        if (PlainConsole)
        {
            Draw(title, [prompt], $"Enter: {submitAction}\n:back: {cancelAction}");
            var input = Console.ReadLine();
            return input is null || input.Trim() == ":back" ? null : input.Trim();
        }
        string value = "";
        while (true)
        {
            int length = Math.Max(1, Width - 4);
            string[] lines = [.. WrapLines(prompt.Replace("\r\n", "\n"), Width),
                "> " + (value.Length > length ? value[^length..] : value)];
            string footer = $"Enter: {submitAction}\nEsc: {cancelAction}";
            Draw(title, lines, footer, Math.Max(0, lines.Length - ContentHeight(footer)));
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Escape: return null;
                case ConsoleKey.Enter: return value.Trim();
                case ConsoleKey.Backspace:
                    if (value.Length > 0) value = value[..^1];
                    break;
                default:
                    if (!char.IsControl(key.KeyChar) && value.Length < 254) value += key.KeyChar;
                    break;
            }
        }
    }

    public static string? ReadRequired(string title, string prompt, Func<string, string?>? validate = null,
        string submitAction = "Use this value", string cancelAction = "Cancel entry")
    {
        while (true)
        {
            var value = ReadText(title, prompt, submitAction, cancelAction);
            if (value is null) return null;
            string? error = string.IsNullOrWhiteSpace(value) ? "This field is required."
                : value.Length > 254 || value.Any(char.IsControl) ? "Use up to 254 printable characters."
                : validate?.Invoke(value);
            if (error is null) return value;
            Message("Check input", error, "Try this field again");
        }
    }

    public static int? ReadNumber(string title, string prompt, int min = 1, int max = int.MaxValue,
        string submitAction = "Use this quantity", string cancelAction = "Cancel entry")
    {
        while (true)
        {
            var text = ReadText(title, prompt, submitAction, cancelAction);
            if (text is null) return null;
            if (int.TryParse(text, out int number) && number >= min && number <= max) return number;
            Message("Check input", $"Enter a whole number from {min} to {max}.", "Re-enter quantity");
        }
    }

    public static decimal? ReadDecimal(string title, string prompt,
        string submitAction = "Use this price", string cancelAction = "Cancel entry")
    {
        while (true)
        {
            var text = ReadText(title, prompt + " (example: 12.50)", submitAction, cancelAction);
            if (text is null) return null;
            if (decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal number)
                && number > 0 && decimal.Round(number, 2) == number) return number;
            Message("Check input", "Enter a positive price with at most two decimal places. Use a dot for decimals.", "Re-enter price");
        }
    }

    public static decimal? ReadPercentage(string title, string prompt,
        string submitAction = "Use this discount", string cancelAction = "Cancel discount changes")
    {
        while (true)
        {
            var text = ReadText(title, prompt + " (0 to 100; example: 12.50)", submitAction, cancelAction);
            if (text is null) return null;
            if (decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal number)
                && number >= 0 && number <= 100 && decimal.Round(number, 2) == number) return number;
            Message("Check input", "Enter a percentage from 0 to 100 with at most two decimal places.", "Re-enter discount percentage");
        }
    }

    public static DateOnly? ReadDate(string title, string prompt, DateOnly? defaultDate = null,
        string submitAction = "Use this date", string cancelAction = "Cancel date entry")
    {
        while (true)
        {
            var text = ReadText(title, prompt + " (yyyy-MM-dd)" +
                (defaultDate.HasValue ? $" [Empty = {defaultDate:yyyy-MM-dd}]" : ""), submitAction, cancelAction);
            if (text is null) return null;
            if (text.Length == 0 && defaultDate.HasValue) return defaultDate;
            if (DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) return date;
            Message("Check input", "Enter a valid date, for example 2026-09-28.", "Re-enter date");
        }
    }

    public static bool Confirm(string title, string action)
        => ShowMenu(title, [action, "Cancel"], 1, backAction: "Cancel confirmation") == 0;

    private static void Draw(string title, string[] lines, string footer, int firstLine = 0)
    {
        if (PlainConsole)
        {
            Console.WriteLine($"\n=== {title} ===");
            foreach (var line in lines) Console.WriteLine(line);
            if (footer.Length > 0) Console.WriteLine(footer);
            return;
        }
        Console.Clear();
        DrawCenteredContent(title, lines, footer, firstLine);
    }

    private static void DrawCenteredContent(string title, string[] lines, string footer, int firstLine)
    {
        int width = Math.Max(1, Console.WindowWidth - 1);
        int height = Math.Max(1, Console.WindowHeight);
        var footerLines = WrapLines(footer, width);
        var visible = lines.Skip(firstLine).Take(ContentHeight(footer)).ToArray();
        int top = Math.Max(0, (height - (visible.Length + 3 + footerLines.Length)) / 2);
        int contentWidth = Math.Min(width, lines.Select(line => line.Length).DefaultIfEmpty(0).Max());
        int left = (width - contentWidth) / 2;

        void WriteAt(string text, int row, int? column = null)
        {
            if (row >= height) return;
            string fitted = Fit(text, width);
            Console.SetCursorPosition(column ?? (width - fitted.Length) / 2, row);
            Console.Write(fitted);
        }

        WriteAt(title, top);
        WriteAt(new string('=', Math.Min(width, 72)), top + 1);
        for (int index = 0; index < visible.Length; index++)
            WriteAt(Fit(visible[index], contentWidth), top + 2 + index, left);
        for (int index = 0; index < footerLines.Length; index++)
            WriteAt(footerLines[index], top + visible.Length + 3 + index);
    }

    private static string Fit(string value, int width) => value.Length <= width ? value
        : width <= 3 ? value[..width] : value[..(width - 3)] + "...";

    private static string[] WrapLines(string content, int width)
    {
        var lines = new List<string>();
        foreach (var line in content.Split('\n'))
        {
            if (line.Length == 0) lines.Add("");
            for (int offset = 0; offset < line.Length; offset += width)
                lines.Add(line.Substring(offset, Math.Min(width, line.Length - offset)));
        }
        return lines.ToArray();
    }
}
