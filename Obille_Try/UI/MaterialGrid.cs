using System.ComponentModel;
using Kit;
using Obille_Try.Data;

namespace Obille_Try.UI {
  public sealed class RowMenuEventArgs : EventArgs {
    public RowMenuEventArgs(MaterialRow row, Point screen) { Row = row; Screen = screen; }
    public MaterialRow Row { get; }
    public Point Screen { get; }
  }

  // The materials table, on the kit's table: the kit gives it the frame,
  // the header, the rules, the hover and picked fills and the checkbox in
  // each row header. This class paints the cells that hold two lines from
  // the row's MaterialRow: the name over its code and bin, the stock with a
  // reorder note when it is low, the unit cost over its unit, and the row's
  // menu button. Each cell's Value still holds its text, for screen readers.
  // Sorting is by the page's whole list rather than the rows on screen, so a
  // header click raises SortRequested and the page sorts and refills.
  public class MaterialGrid : KitTable {
    public MaterialGrid() {
      Selectable = true;
      AllowUserToDeleteRows = false;
      AllowUserToResizeColumns = false;
      StandardTab = true;
      CheckedRowsChanged += (_, _) => PickedChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override int RowHeight => S(56);

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SortColumn { get; set; } = "Material";

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool SortAscending { get; set; } = true;

    public event EventHandler<RowMenuEventArgs>? RowMenuRequested;
    public event EventHandler<MaterialRow>? RowOpened;
    public event EventHandler? SortRequested;
    public event EventHandler? PickedChanged;

    public int PickedCount => CheckedRows.Count;

    public bool IsPicked(int row) => CheckedRows.Contains(row);

    public void ClearPicked() => ClearChecked();

    public MaterialRow? RowAt(int index) =>
        index >= 0 && index < Rows.Count ? Rows[index].Tag as MaterialRow : null;

    static bool Sortable(string name) => name is "Material" or "Stock" or "Cost";

    // A column is known by its DataPropertyName (Material, Stock, Cost,
    // Actions), never by its Name: Visual Studio's designer sets a column's
    // Name to its field's name on save (materialColumn), and a lookup by
    // "Material" then threw at the first refill. The grid is never bound,
    // so DataPropertyName only labels the column.
    string KeyOf(int column) => column >= 0 && column < Columns.Count ? Columns[column].DataPropertyName : "";

    int IndexOf(string key) {
      foreach (DataGridViewColumn c in Columns)
        if (c.DataPropertyName == key) return c.Index;
      throw new InvalidOperationException($"The materials table has no column whose DataPropertyName is {key}.");
    }

    public void ShowRows(IEnumerable<MaterialRow> rows) {
      Rows.Clear();
      foreach (var r in rows) {
        var i = Rows.Add();
        var row = Rows[i];
        row.Tag = r;
        row.Cells[IndexOf("Material")].Value = $"{r.Name}, {r.Code}, {r.Bin}";
        row.Cells[IndexOf("Stock")].Value = r.Quantity(r.Stock);
        row.Cells[IndexOf("Cost")].Value = $"₱{r.Cost:N2} per {r.Unit}";
      }
      PickedChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnCellMouseClick(DataGridViewCellMouseEventArgs e) {
      base.OnCellMouseClick(e);
      if (e.ColumnIndex < 0) return;
      var name = KeyOf(e.ColumnIndex);
      if (e.RowIndex == -1) {
        if (!Sortable(name)) return;
        SortAscending = SortColumn != name || !SortAscending;
        SortColumn = name;
        SortRequested?.Invoke(this, EventArgs.Empty);
        return;
      }
      if (e.RowIndex < 0) return;
      if (e.Button == MouseButtons.Right || name == "Actions") {
        var at = e.Button == MouseButtons.Right ? Cursor.Position : PointToScreen(MenuAnchor(e.RowIndex));
        OpenMenu(e.RowIndex, at);
      }
    }

    // Under the row's menu button, so the menu opens from it.
    Point MenuAnchor(int row) {
      var box = ActionBox(GetCellDisplayRectangle(IndexOf("Actions"), row, false));
      return new Point(box.X, box.Bottom + S(4));
    }

    void OpenMenu(int row, Point screen) {
      if (RowAt(row) is { } r) RowMenuRequested?.Invoke(this, new RowMenuEventArgs(r, screen));
    }

    protected override void OnCellDoubleClick(DataGridViewCellEventArgs e) {
      base.OnCellDoubleClick(e);
      if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && KeyOf(e.ColumnIndex) != "Actions" && RowAt(e.RowIndex) is { } r)
        RowOpened?.Invoke(this, r);
    }

    protected override bool ProcessDialogKey(Keys keyData) {
      var row = CurrentCell?.RowIndex ?? -1;
      switch (keyData) {
        case Keys.Enter when RowAt(row) is { } r:
          RowOpened?.Invoke(this, r);
          return true;
        case Keys.Apps when row >= 0:
        case Keys.Shift | Keys.F10 when row >= 0:
          OpenMenu(row, PointToScreen(MenuAnchor(row)));
          return true;
      }
      // Space ticks the row, in the kit's table.
      return base.ProcessDialogKey(keyData);
    }

    // The row's menu button: a 28 px ghost button, the cell's inset from
    // the right edge.
    Rectangle ActionBox(Rectangle cell) {
      var size = S(28);
      var (_, right) = PadFor(IndexOf("Actions"));
      return new Rectangle(cell.Right - right - size, cell.Y + (cell.Height - size) / 2, size, size);
    }

    protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e) {
      if (e.Graphics == null || e.ColumnIndex < 0) {
        base.OnCellPainting(e);
        return;
      }
      var g = e.Graphics;
      var b = e.CellBounds;
      var name = KeyOf(e.ColumnIndex);

      if (e.RowIndex == -1) {
        // The kit draws the header and its text; the sort mark goes
        // beside the text: after it on the left, before it on the right.
        base.OnCellPainting(e);
        if (!Sortable(name)) return;
        var font = ColumnHeadersDefaultCellStyle.Font!;
        var (pl, pr) = PadFor(e.ColumnIndex);
        var tw = Painter.Measure(Columns[e.ColumnIndex].HeaderText, font).Width;
        var right = RightAligned(Columns[e.ColumnIndex]);
        var px = S(14);
        var x = right ? b.Right - pr - tw - S(6) - px : b.X + pl + tw + S(6);
        var active = SortColumn == name;
        var icon = !active ? "arrow-up-down" : SortAscending ? "arrow-up" : "arrow-down";
        Icons.Draw(g, icon, new RectangleF(x, b.Y + (b.Height - px) / 2f, px, px), active ? P.Ink : P.Ink3);
        return;
      }

      if (RowAt(e.RowIndex) is not { } r || name is not ("Material" or "Stock" or "Cost" or "Actions")) {
        base.OnCellPainting(e);
        return;
      }

      using (var fill = new SolidBrush(RowFill(e.RowIndex))) g.FillRectangle(fill, b);
      if (e.RowIndex != Rows.Count - 1) Rule(g, b);

      var (left, rightPad) = PadFor(e.ColumnIndex);
      var main = KitTheme.Font(14);
      var sub = KitTheme.Font(13);
      int h1 = main.Height, h2 = sub.Height;
      var top2 = b.Y + (b.Height - h1 - h2 - S(2)) / 2;
      var top1 = b.Y + (b.Height - h1) / 2;
      var inner = new Rectangle(b.X + left, b.Y, b.Width - left - rightPad, b.Height);

      switch (name) {
        case "Material":
          Painter.Text(g, r.Name, main, new Rectangle(inner.X, top2, inner.Width, h1), P.Ink);
          Painter.Text(g, $"{r.Code}, {r.Bin}", sub, new Rectangle(inner.X, top2 + h1 + S(2), inner.Width, h2), P.Ink2);
          break;
        case "Stock":
          Painter.Text(g, r.Quantity(r.Stock), main, new Rectangle(inner.X, r.IsLow ? top2 : top1, inner.Width, h1), P.Ink, TextFormatFlags.Right);
          if (r.IsLow) {
            // At or under its reorder point: the point, in the
            // warning ink, with a dot so it does not rest on color.
            var note = $"Reorder at {r.ReorderPoint:N0}";
            var nw = Painter.Measure(note, sub).Width;
            var noteBox = new Rectangle(inner.Right - nw, top2 + h1 + S(2), nw + 1, h2);
            Painter.Text(g, note, sub, noteBox, P.Warn);
            Painter.Smooth(g);
            using var dot = new SolidBrush(P.Warn);
            var d = S(6);
            g.FillEllipse(dot, noteBox.X - d - S(5), noteBox.Y + (h2 - d) / 2f, d, d);
          }
          break;
        case "Cost":
          Painter.Text(g, $"₱{r.Cost:N2}", main, new Rectangle(inner.X, top2, inner.Width, h1), P.Ink, TextFormatFlags.Right);
          Painter.Text(g, $"per {r.Unit}", sub, new Rectangle(inner.X, top2 + h1 + S(2), inner.Width, h2), P.Ink2, TextFormatFlags.Right);
          break;
        case "Actions":
          var box = ActionBox(b);
          Painter.Smooth(g);
          if (RowFill(e.RowIndex) != P.Bg) Painter.Fill(g, P.BgHover, box, S(M.RadiusControl));
          Icons.Draw(g, "ellipsis", RectangleF.Inflate(box, -S(6), -S(6)), P.Ink2);
          break;
      }
      e.Handled = true;
    }

    static Metrics M => KitTheme.Metrics;
  }
}
