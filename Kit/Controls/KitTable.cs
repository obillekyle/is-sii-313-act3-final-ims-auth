using System.ComponentModel;

namespace Kit {
  // The one table, on a DataGridView so its columns are
  // added and edited in the designer as any grid's are. The kit gives it
  // its rows and type, numbers set right, the sorted column's arrow, a
  // checkbox per row with a select-all when Selectable, placeholder rows
  // while Loading, and a line saying why it is empty.
  [ToolboxItem(true)]
  [Description("The kit's table: a DataGridView with the kit's rows, sort arrows, row checkboxes and empty and loading states.")]
  public class KitTable : DataGridView {
    bool selectable, loading, framed = true;
    string emptyText = "Nothing here yet.";
    readonly HashSet<int> picked = new();
    readonly System.Windows.Forms.Timer pulse = new() { Interval = 50 };
    float phase;

    public KitTable() {
      DoubleBuffered = true;
      BorderStyle = BorderStyle.None;
      CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
      ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
      EnableHeadersVisualStyles = false;
      RowHeadersVisible = false;
      AllowUserToAddRows = false;
      AllowUserToResizeRows = false;
      SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      MultiSelect = false;
      ReadOnly = true;
      ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      pulse.Tick += (_, _) => { phase = (phase + 0.04f) % 2f; Invalidate(); };
      KitTheme.Changed += OnThemeChanged;
      ApplyTheme();
    }

    // What the theme sets (colors, fonts, padding, the header's height,
    // the rules) is kept out of the designer's file: saved there, it
    // would be set back over the theme each time the form loads, and
    // Visual Studio saved every one of them on the first save of a page.
    const DesignerSerializationVisibility Theme = DesignerSerializationVisibility.Hidden;

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new DataGridViewCellStyle DefaultCellStyle { get => base.DefaultCellStyle; set => base.DefaultCellStyle = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new DataGridViewCellStyle ColumnHeadersDefaultCellStyle { get => base.ColumnHeadersDefaultCellStyle; set => base.ColumnHeadersDefaultCellStyle = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new DataGridViewCellStyle RowHeadersDefaultCellStyle { get => base.RowHeadersDefaultCellStyle; set => base.RowHeadersDefaultCellStyle = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new DataGridViewCellStyle AlternatingRowsDefaultCellStyle { get => base.AlternatingRowsDefaultCellStyle; set => base.AlternatingRowsDefaultCellStyle = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new DataGridViewRow RowTemplate { get => base.RowTemplate; set => base.RowTemplate = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new Color GridColor { get => base.GridColor; set => base.GridColor = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new Color BackgroundColor { get => base.BackgroundColor; set => base.BackgroundColor = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new int ColumnHeadersHeight { get => base.ColumnHeadersHeight; set => base.ColumnHeadersHeight = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new int RowHeadersWidth { get => base.RowHeadersWidth; set => base.RowHeadersWidth = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new bool EnableHeadersVisualStyles { get => base.EnableHeadersVisualStyles; set => base.EnableHeadersVisualStyles = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new DataGridViewCellBorderStyle CellBorderStyle { get => base.CellBorderStyle; set => base.CellBorderStyle = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new DataGridViewHeaderBorderStyle ColumnHeadersBorderStyle { get => base.ColumnHeadersBorderStyle; set => base.ColumnHeadersBorderStyle = value; }

    [Browsable(false), DesignerSerializationVisibility(Theme)]
    public new BorderStyle BorderStyle { get => base.BorderStyle; set => base.BorderStyle = value; }

    [Category("Behavior"), DefaultValue(false)]
    [Description("Adds a checkbox before each row and a select-all in the header.")]
    public bool Selectable {
      get => selectable;
      set {
        selectable = value;
        RowHeadersVisible = value;
        Invalidate();
      }
    }

    // The checkboxes sit in the row headers rather than a column of their
    // own, so the columns keep the positions the designer gave them and
    // Rows.Add(values) fills them in order. As a first column they took
    // every row's first value.
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool RowHeadersVisible { get => base.RowHeadersVisible; set => base.RowHeadersVisible = value; }

    [Category("Behavior"), DefaultValue(false)]
    [Description("Shows five placeholder rows in place of the data while it loads.")]
    public bool Loading {
      get => loading;
      set {
        loading = value;
        if (value) pulse.Start(); else pulse.Stop();
        Invalidate();
      }
    }

    [Category("Appearance"), DefaultValue(true)]
    [Description("Draws the rounded 1 px frame. Off for a table inside a flush KitPanel, whose own frame holds it.")]
    public bool Framed {
      get => framed;
      set {
        framed = value;
        Reshape();
        Invalidate();
      }
    }

    [Category("Appearance"), DefaultValue("Nothing here yet.")]
    [Description("What the table says when it has no rows: why it is empty, in one sentence.")]
    public string EmptyText { get => emptyText; set { emptyText = value ?? ""; Invalidate(); } }

    // The rows ticked, by index.
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyCollection<int> CheckedRows => picked;

    public event EventHandler? CheckedRowsChanged;

    protected static Palette P => KitTheme.Palette;
    protected int S(int px) => LogicalToDeviceUnits(px);

    // A row's height: the density's row (40 px comfortable). A table whose
    // cells hold two lines (a name over its code) overrides it.
    protected virtual int RowHeight => S(KitTheme.Metrics.Row);

    void OnThemeChanged(object? sender, EventArgs e) {
      if (IsDisposed) return;
      ApplyTheme();
      Invalidate();
    }

    void ApplyTheme() {
      var p = P;
      BackgroundColor = p.Bg;
      GridColor = p.Line;
      // Every rule is drawn below, in one color; and a row is shaded only
      // on hover, when picked, or as the keyboard's row while the table
      // has focus, never as a selection made at load.
      CellBorderStyle = DataGridViewCellBorderStyle.None;
      DefaultCellStyle.BackColor = p.Bg;
      DefaultCellStyle.ForeColor = p.Ink;
      DefaultCellStyle.SelectionBackColor = p.Bg;
      DefaultCellStyle.SelectionForeColor = p.Ink;
      DefaultCellStyle.Font = KitTheme.Font(14);
      DefaultCellStyle.Padding = new Padding(S(12), 0, S(12), 0);
      ColumnHeadersDefaultCellStyle.BackColor = p.BgSubtle;
      ColumnHeadersDefaultCellStyle.ForeColor = p.Ink2;
      ColumnHeadersDefaultCellStyle.SelectionBackColor = p.BgSubtle;
      ColumnHeadersDefaultCellStyle.Font = KitTheme.Font(12, true);
      ColumnHeadersDefaultCellStyle.Padding = new Padding(S(12), 0, S(12), 0);
      ColumnHeadersHeight = S(36);
      RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      RowHeadersWidth = S(44);
      RowHeadersDefaultCellStyle.BackColor = p.Bg;
      RowHeadersDefaultCellStyle.SelectionBackColor = p.Bg;
      RowTemplate.Height = RowHeight;
      foreach (DataGridViewRow r in Rows) r.Height = RowTemplate.Height;
      Painter.ThemeScrollbars(this);
    }

    int hoverRow = -1;

    // A row's hover fill eases in and out over Motion.Fast (key: the row's
    // index + 1); the fade repaints the table while it moves.
    HoverFade? fade;
    HoverFade Fade => fade ??= new HoverFade(this);

    protected override void OnCellMouseEnter(DataGridViewCellEventArgs e) {
      base.OnCellMouseEnter(e);
      if (e.RowIndex == hoverRow) return;
      if (hoverRow >= 0) Fade.Set(hoverRow + 1, false);
      if (e.RowIndex >= 0) Fade.Set(e.RowIndex + 1, true);
      hoverRow = e.RowIndex;
      Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e) {
      base.OnMouseLeave(e);
      hoverRow = -1;
      Fade.Clear();
    }

    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

    // Numbers are set right, header and all, and whole numbers get their
    // thousands separators. Decided when a cell is drawn, not when a column
    // is added: the designer writes Columns.AddRange before each column's
    // ValueType and alignment, so at that point every column looks like
    // text. Nothing is written back to the column, so the designer has
    // nothing extra to save.
    public bool RightAligned(DataGridViewColumn c) {
      var t = c.ValueType;
      var numeric = t == typeof(int) || t == typeof(long) || t == typeof(decimal) || t == typeof(double) || t == typeof(float);
      static bool Right(DataGridViewContentAlignment a) =>
          a is DataGridViewContentAlignment.MiddleRight or DataGridViewContentAlignment.TopRight or DataGridViewContentAlignment.BottomRight;
      return numeric || Right(c.DefaultCellStyle.Alignment) || Right(c.HeaderCell.Style.Alignment);
    }

    protected override void OnSizeChanged(EventArgs e) {
      base.OnSizeChanged(e);
      Reshape();
    }

    void Reshape() {
      if (!framed) { Region = null; return; }
      using var path = Painter.Round(new RectangleF(0, 0, Width, Height), S(KitTheme.Metrics.RadiusPanel));
      Region = new Region(path);
    }

    // The space before the first column and after the last: 12 px, or in a
    // flush KitPanel the 16 px of the panel's title, counted from the
    // panel's outer edge.
    int Inset => Parent is KitPanel { Flush: true } panel ? Math.Max(0, S(16) - panel.Padding.Left) : S(12);

    protected (int Left, int Right) PadFor(int column) {
      var first = !selectable && column == FirstShownColumn();
      var last = column == LastShownColumn();
      return (first ? Inset : S(12), last ? Inset : S(12));
    }

    int FirstShownColumn() => Columns.GetFirstColumn(DataGridViewElementStates.Visible)?.Index ?? -1;
    int LastShownColumn() => Columns.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None)?.Index ?? -1;

    // A cell whose Tag is a Tone draws its text in that tone's ink, for
    // the value a person acts on (stock under its reorder point).
    protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e) {
      base.OnCellFormatting(e);
      if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.CellStyle == null) return;
      var (l, r) = PadFor(e.ColumnIndex);
      e.CellStyle.Padding = new Padding(l, 0, r, 0);
      if (RightAligned(Columns[e.ColumnIndex])) e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      if (string.IsNullOrEmpty(e.CellStyle.Format) && e.Value is int or long) {
        e.Value = Convert.ToInt64(e.Value).ToString("N0");
        e.FormattingApplied = true;
      }
      if (Rows[e.RowIndex].Cells[e.ColumnIndex].Tag is Tone tone && tone != Tone.Neutral)
        e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = P.ToneColors(tone).Ink;
    }

    // Picked, then the hover (easing over the keyboard's row or the page),
    // then the keyboard's row while the table has focus, then the page.
    protected Color RowFill(int row) {
      if (picked.Contains(row)) return P.AccentSoft;
      var under = Focused && CurrentCell?.RowIndex == row ? P.BgHover : P.Bg;
      return Fade.Mix(row + 1, under, P.BgSubtle);
    }

    protected void Rule(Graphics g, Rectangle cell) {
      using var line = new Pen(P.Line);
      g.DrawLine(line, cell.Left, cell.Bottom - 1, cell.Right, cell.Bottom - 1);
    }

    public void ClearChecked() {
      picked.Clear();
      Invalidate();
      CheckedRowsChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnCellMouseClick(DataGridViewCellMouseEventArgs e) {
      base.OnCellMouseClick(e);
      if (!selectable || e.ColumnIndex != -1) return;
      if (e.RowIndex == -1) {
        if (picked.Count == Rows.Count) picked.Clear();
        else for (var i = 0; i < Rows.Count; i++) picked.Add(i);
      } else if (e.RowIndex >= 0 && !picked.Remove(e.RowIndex)) picked.Add(e.RowIndex);
      Invalidate();
      CheckedRowsChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override bool ProcessDialogKey(Keys keyData) {
      if (keyData == Keys.Space && selectable && CurrentCell?.RowIndex is int row && row >= 0) {
        if (!picked.Remove(row)) picked.Add(row);
        Invalidate();
        CheckedRowsChanged?.Invoke(this, EventArgs.Empty);
        return true;
      }
      return base.ProcessDialogKey(keyData);
    }

    protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e) {
      base.OnCellPainting(e);
      if (e.Graphics == null) return;
      var g = e.Graphics;
      if (e.ColumnIndex == -1) {
        // The row header: a row's checkbox, or the select-all above them.
        using (var bg = new SolidBrush(e.RowIndex == -1 ? P.BgSubtle : RowFill(e.RowIndex)))
          g.FillRectangle(bg, e.CellBounds);
        if (e.RowIndex == -1 || e.RowIndex != Rows.Count - 1) Rule(g, e.CellBounds);
        if (selectable) {
          var state = e.RowIndex == -1
              ? (picked.Count == 0 ? CheckState.Unchecked : picked.Count == Rows.Count ? CheckState.Checked : CheckState.Indeterminate)
              : picked.Contains(e.RowIndex) ? CheckState.Checked : CheckState.Unchecked;
          DrawCheck(g, e.CellBounds, state);
        }
        e.Handled = true;
        return;
      }
      var col = Columns[e.ColumnIndex];
      if (e.RowIndex >= 0) {
        using (var b = new SolidBrush(RowFill(e.RowIndex))) g.FillRectangle(b, e.CellBounds);
        // Text cells are drawn with the header's own call, so a value
        // and its header share an edge; the grid's painter adds a few
        // pixels of its own margin inside the padding. Other cell
        // types (checkbox, image, button) keep the grid's painter.
        if (e.FormattedValue is string s && Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewTextBoxCell && e.CellStyle != null) {
          var pad = e.CellStyle.Padding;
          var box = new Rectangle(e.CellBounds.X + pad.Left, e.CellBounds.Y, e.CellBounds.Width - pad.Horizontal, e.CellBounds.Height);
          var right = e.CellStyle.Alignment is DataGridViewContentAlignment.MiddleRight or DataGridViewContentAlignment.TopRight or DataGridViewContentAlignment.BottomRight;
          var center = e.CellStyle.Alignment is DataGridViewContentAlignment.MiddleCenter or DataGridViewContentAlignment.TopCenter or DataGridViewContentAlignment.BottomCenter;
          Painter.Text(g, s, e.CellStyle.Font ?? DefaultCellStyle.Font!, box, e.CellStyle.ForeColor,
              (right ? TextFormatFlags.Right : center ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Left) | TextFormatFlags.VerticalCenter);
        } else e.PaintContent(e.CellBounds);
        // No rule under the last row: the frame or the
        // panel below closes the table.
        if (e.RowIndex != Rows.Count - 1) Rule(g, e.CellBounds);
        e.Handled = true;
        return;
      }
      if (e.RowIndex == -1) {
        // The header text is drawn here rather than by the grid, which
        // keeps room for its own sort glyph and so set a right-aligned
        // header about 20 px short of the numbers under it.
        using (var b = new SolidBrush(P.BgSubtle)) g.FillRectangle(b, e.CellBounds);
        Rule(g, e.CellBounds);
        var right = RightAligned(col);
        var (pl, pr) = PadFor(e.ColumnIndex);
        var font = ColumnHeadersDefaultCellStyle.Font!;
        var text = new Rectangle(e.CellBounds.X + pl, e.CellBounds.Y, e.CellBounds.Width - pl - pr, e.CellBounds.Height);
        Painter.Text(g, col.HeaderText, font, text, P.Ink2,
            (right ? TextFormatFlags.Right : TextFormatFlags.Left) | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if (SortedColumn == col) {
          var tw = Painter.Measure(col.HeaderText, font).Width;
          var x = right ? text.Right - tw - S(18) : text.X + tw + S(4);
          Icons.Draw(g, SortOrder == SortOrder.Ascending ? "arrow-up" : "arrow-down",
              new RectangleF(x, e.CellBounds.Y + (e.CellBounds.Height - S(14)) / 2f, S(14), S(14)), P.Ink);
        }
        e.Handled = true;
      }
    }

    void DrawCheck(Graphics g, Rectangle cell, CheckState state) {
      Painter.Smooth(g);
      var box = new RectangleF(cell.X + (cell.Width - S(18)) / 2f, cell.Y + (cell.Height - S(18)) / 2f, S(18), S(18));
      if (state == CheckState.Unchecked) {
        Painter.Fill(g, P.Bg, box, S(4));
        Painter.Outline(g, P.LineStrong, box, S(4));
        return;
      }
      Painter.Fill(g, P.Inverse, box, S(4));
      Icons.Draw(g, state == CheckState.Checked ? "check" : "minus", RectangleF.Inflate(box, -S(3), -S(3)), P.InverseInk, 3);
    }

    protected override void OnPaint(PaintEventArgs e) {
      base.OnPaint(e);
      var g = e.Graphics;
      var top = ColumnHeadersVisible ? ColumnHeadersHeight : 0;
      if (loading) {
        using (var bg = new SolidBrush(P.Bg)) g.FillRectangle(bg, 0, top, Width, Height - top);
        var t = phase < 1 ? phase : 2 - phase;
        var fill = Palette.Mix(P.BgHover, 0.6 + 0.4 * t, P.BgSubtle);
        var rowH = RowTemplate.Height;
        Painter.Smooth(g);
        for (var i = 0; i < 5; i++) {
          var y = top + i * rowH;
          Painter.Fill(g, fill, new RectangleF(S(12), y + rowH / 2f - S(6), Width * (i % 2 == 0 ? 0.55f : 0.4f), S(12)), S(4));
          using var line = new Pen(P.Line);
          g.DrawLine(line, 0, y + rowH - 1, Width, y + rowH - 1);
        }
      } else if (Rows.Count == 0 && emptyText.Length > 0 && !(DesignMode || Site?.DesignMode == true && Columns.Count == 0))
        Painter.Text(g, emptyText, KitTheme.Font(14), new Rectangle(0, top + S(24), Width, S(24)), P.Ink2,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

      // The table's frame: a 1 px line on the panel radius.
      if (!framed) return;
      Painter.Smooth(g);
      Painter.Outline(g, P.Line, new RectangleF(0, 0, Width, Height), S(KitTheme.Metrics.RadiusPanel));
    }

    protected override void OnRowsRemoved(DataGridViewRowsRemovedEventArgs e) {
      base.OnRowsRemoved(e);
      if (picked.Count > 0) { picked.Clear(); CheckedRowsChanged?.Invoke(this, EventArgs.Empty); }
    }

    protected override void Dispose(bool disposing) {
      if (disposing) {
        KitTheme.Changed -= OnThemeChanged;
        pulse.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}
