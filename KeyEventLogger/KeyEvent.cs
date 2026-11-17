using System.Globalization;

namespace KeyEventLogger;

internal sealed record KeyEvent(DateTimeOffset Time, Keys Key, bool OverlapsAnotherPress)
{
    public override string ToString() => FormatDisplay(indentOverlaps: false);

    public string FormatDisplay(bool indentOverlaps)
    {
        string indent = indentOverlaps && OverlapsAnotherPress ? "    " : "";
        return $"{Time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)}   {indent}{Key}";
    }

    public string ToExportLine() =>
        $"{Time.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)}\t{Key}";
}
