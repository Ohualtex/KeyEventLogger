using System.Globalization;

namespace KeyEventLogger;

internal sealed record KeyEvent(DateTimeOffset Time, Keys Key)
{
    public override string ToString() =>
        $"{Time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)}   {Key}";

    public string ToExportLine() =>
        $"{Time.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)}\t{Key}";
}
