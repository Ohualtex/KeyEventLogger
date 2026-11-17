namespace KeyEventLogger;

internal sealed class MainForm : Form
{
    private static readonly Color Ink = Color.FromArgb(22, 34, 51);
    private static readonly Color Accent = Color.FromArgb(34, 84, 158);
    private static readonly Color Surface = Color.FromArgb(242, 245, 249);

    private readonly ListBox rows = new()
    {
        Dock = DockStyle.Fill,
        IntegralHeight = false,
        SelectionMode = SelectionMode.None,
        DrawMode = DrawMode.OwnerDrawFixed,
        AccessibleName = "Keyboard events",
        BackColor = Color.White,
        ForeColor = Ink
    };
    private readonly Label status = new() { AutoSize = true, Anchor = AnchorStyles.Right };
    private readonly Label count = new() { AutoSize = true, Anchor = AnchorStyles.Right };
    private readonly CheckBox indentOverlaps = new()
    {
        Text = "Indent overlapping presses",
        AutoSize = true,
        Checked = true,
        Margin = new Padding(4, 8, 0, 0)
    };
    private readonly Button startButton = CreateButton("Start", primary: true);
    private readonly Button stopButton = CreateButton("Stop");
    private readonly Button clearButton = CreateButton("Clear");
    private readonly Button exportButton = CreateButton("Export TXT");
    private readonly Font headingFont = new("Segoe UI", 21, FontStyle.Bold);
    private readonly Font rowFont = new(FontFamily.GenericMonospace, 11);

    private readonly HashSet<Keys> heldKeys = [];
    private bool windowActive;
    private bool recording;
    private bool exporting;

    public MainForm()
    {
        Text = "Key Event Logger";
        ClientSize = new Size(720, 520);
        MinimumSize = new Size(580, 370);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Surface;
        ForeColor = Ink;
        KeyPreview = true;
        rows.Font = rowFont;
        rows.ItemHeight = rowFont.Height + 2;
        rows.DrawItem += DrawEventRow;
        indentOverlaps.CheckedChanged += (_, _) => rows.Invalidate();

        startButton.Click += (_, _) => SetRecording(true);
        stopButton.Click += (_, _) => SetRecording(false);
        clearButton.Click += (_, _) =>
        {
            rows.Items.Clear();
            RefreshStatus();
        };
        exportButton.Click += (_, _) => Export();
        Activated += (_, _) => SetWindowActive(true);
        Deactivate += (_, _) => SetWindowActive(false);
        Controls.Add(BuildLayout());
        RefreshStatus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        RecordPress(e.KeyCode);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        heldKeys.Remove(e.KeyCode);
    }

    private void RecordPress(Keys key)
    {
        if (!recording || exporting || !windowActive || !heldKeys.Add(key))
        {
            return;
        }

        rows.Items.Add(new KeyEvent(DateTimeOffset.Now, key, heldKeys.Count > 1));
        rows.TopIndex = rows.Items.Count - 1;
        RefreshStatus();
    }

    private void DrawEventRow(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0)
        {
            return;
        }

        var entry = (KeyEvent)rows.Items[e.Index];
        e.DrawBackground();
        TextRenderer.DrawText(e.Graphics, entry.FormatDisplay(indentOverlaps.Checked), rowFont,
            e.Bounds, Ink, TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
        e.DrawFocusRectangle();
    }

    private void SetRecording(bool value)
    {
        recording = value;
        heldKeys.Clear();
        RefreshStatus();
        if (value)
        {
            rows.Focus();
        }
    }

    private void SetWindowActive(bool value)
    {
        windowActive = value;
        if (!value)
        {
            heldKeys.Clear();
        }
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        bool active = recording && !exporting && windowActive;
        status.Text = active ? "Recording" : recording ? "Waiting for focus" : "Stopped";
        status.ForeColor = active ? Color.FromArgb(18, 103, 62) : Ink;
        count.Text = $"{rows.Items.Count} events";
        startButton.Enabled = !recording;
        stopButton.Enabled = recording;
        clearButton.Enabled = rows.Items.Count > 0;
        exportButton.Enabled = clearButton.Enabled;
    }

    private void Export()
    {
        exporting = true;
        RefreshStatus();
        try
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Export keyboard events",
                Filter = "Text files (*.txt)|*.txt",
                DefaultExt = "txt",
                FileName = "key-events.txt"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                TextLogExporter.Save(dialog.FileName, rows.Items.Cast<KeyEvent>());
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Could not export the log.\n\n{error.Message}", Text,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            exporting = false;
            RefreshStatus();
        }
    }

    private Control BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            RowCount = 5,
            ColumnCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = new Label { Text = Text, Font = headingFont, AutoSize = true };
        var hint = new Label
        {
            Text = "Press Start, then try a few keys in this window.",
            AutoSize = true,
            Margin = new Padding(4, 4, 0, 14)
        };
        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 12)
        };
        actions.Controls.AddRange([startButton, stopButton, clearButton, exportButton]);

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(status, 1, 0);
        layout.Controls.Add(hint, 0, 1);
        layout.SetColumnSpan(hint, 2);
        layout.Controls.Add(actions, 0, 2);
        layout.SetColumnSpan(actions, 2);
        layout.Controls.Add(rows, 0, 3);
        layout.SetColumnSpan(rows, 2);
        layout.Controls.Add(indentOverlaps, 0, 4);
        layout.Controls.Add(count, 1, 4);
        count.Margin = new Padding(0, 10, 3, 0);
        return layout;
    }

    private static Button CreateButton(string text, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            Size = new Size(112, 36),
            Margin = new Padding(3, 0, 7, 0),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = primary ? Accent : Color.White,
            ForeColor = primary ? Color.White : Ink
        };
        button.FlatAppearance.BorderColor = primary ? Accent : Color.FromArgb(190, 201, 215);
        return button;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            base.Dispose(true);
            headingFont.Dispose();
            rowFont.Dispose();
        }
        else
        {
            base.Dispose(false);
        }
    }
}
