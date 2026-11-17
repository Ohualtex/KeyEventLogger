using System.Globalization;

namespace KeyEventLogger;

internal enum KeyEventKind { KeyDown, KeyUp }

internal sealed record KeyEvent(DateTimeOffset Time, Keys Key, KeyEventKind Kind)
{
    public override string ToString() =>
        $"{Time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)}   {Key,-18} {Kind}";

    public string ToExportLine() =>
        $"{Time.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)}\t{Key}\t{Kind}";
}
