using Spectre.Console;

namespace Edrakon.Logging;


public static class EdraLogger
{
    public static string GetColor(this LogLevel type)
    {
        return type switch
        {
            LogLevel.DEBUG => "[[[bold yellow]DEBUG[/]]]",
            LogLevel.ERROR => "[[[bold red]ERROR[/]]]",
            LogLevel.WARN  => "[[[bold darkorange3_1]WARN[/]]]",
            LogLevel.INFO  => "[[[bold green]INFO[/]]]",
            _ => ""
        };
    }
    public static void Log(object? message, LogLevel type = LogLevel.INFO)
    {
        AnsiConsole.MarkupLine($"{type.GetColor()} {(message?.ToString() ?? "null").EscapeMarkup()}");
    }
}



public enum LogLevel : int
{
    INFO,
    WARN,
    ERROR,
    DEBUG,
}