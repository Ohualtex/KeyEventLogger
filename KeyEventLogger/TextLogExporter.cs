namespace KeyEventLogger;

internal static class TextLogExporter
{
    public static void Save(string path, IEnumerable<KeyEvent> entries) =>
        File.WriteAllLines(path, entries.Select(entry => entry.ToExportLine()).Prepend("Timestamp\tKey"));
}
