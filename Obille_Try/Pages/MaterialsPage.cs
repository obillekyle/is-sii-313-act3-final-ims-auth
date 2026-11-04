using System.ComponentModel;
using Kit;
using Obille_Try.Data;
using Obille_Try.Dialogs;

namespace Obille_Try.Pages {
  // The materials list, 25 to a page by default. A prototype: the filters,
  // the search, the sort and the pages work on the sample rows; adding and
  // editing open their dialogs and report with a toast; a delete takes the
  // rows off the list until the app closes. Nothing is saved.
  public partial class MaterialsPage : UI.Page {
    readonly List<MaterialRow> rows = SampleData.Materials.ToList();
    readonly ContextMenuStrip rowMenu = new();
    MaterialRow? menuRow;

    public MaterialsPage() {
      InitializeComponent();

      // Each filter's first option is "all", which filters nothing.
      categorySelect.Options = new[] { "All categories" }.Concat(SampleData.Categories).ToArray();
      stockSelect.Options = new[] { "All stock" }.Concat(SampleData.StockFilters).ToArray();
      supplierSelect.Options = new[] { "All suppliers" }.Concat(SampleData.Suppliers).ToArray();
      foreach (var select in new[] { categorySelect, stockSelect, supplierSelect }) {
        select.SelectedIndex = 0;
        select.SelectedIndexChanged += (_, _) => Refill(toFirstPage: true);
      }
      searchBox.TextChanged += (_, _) => Refill(toFirstPage: true);

      var px = LogicalToDeviceUnits(16);
      var edit = new ToolStripMenuItem("Edit material", Icons.ToBitmap("pencil", px, KitTheme.Palette.Ink)) {
        ShortcutKeyDisplayString = "Enter",
      };
      edit.Click += (_, _) => { if (menuRow != null) OpenEdit(menuRow); };
      var delete = new ToolStripMenuItem("Delete material", Icons.ToBitmap("trash-2", px, KitTheme.Palette.Bad)) {
        ForeColor = KitTheme.Palette.Bad,
        ShortcutKeyDisplayString = "Del",
      };
      delete.Click += (_, _) => { if (menuRow != null) ConfirmDelete(new[] { menuRow }); };
      rowMenu.Items.AddRange(new ToolStripItem[] { edit, new ToolStripSeparator(), delete });
      KitMenu.Style(rowMenu);

      materialGrid.RowMenuRequested += (_, e) => {
        menuRow = e.Row;
        rowMenu.Show(e.Screen);
      };
      materialGrid.RowOpened += (_, r) => OpenEdit(r);
      materialGrid.SortRequested += (_, _) => Refill();
      materialGrid.PickedChanged += (_, _) => ShowPicked();
      materialGrid.KeyDown += (_, e) => {
        if (e.KeyCode != Keys.Delete) return;
        var picked = Picked();
        if (picked.Count == 0 && materialGrid.RowAt(materialGrid.CurrentCell?.RowIndex ?? -1) is { } r) picked.Add(r);
        if (picked.Count > 0) ConfirmDelete(picked);
        e.Handled = true;
      };

      newButton.Click += (_, _) => OpenNew();
      clearPickedButton.Click += (_, _) => materialGrid.ClearPicked();
      deletePickedButton.Click += (_, _) => ConfirmDelete(Picked());
      pagination.PageChanged += (_, _) => Refill();
      pagination.PageSizeChanged += (_, _) => Refill(toFirstPage: true);

      Refill();
    }

    public void FocusSearch() {
      searchBox.TextBox.Focus();
      searchBox.TextBox.SelectAll();
    }

    public void ShowLowStock() => stockSelect.SelectedOption = "Low stock";

    // One New and Edit material dialog, kept for the page's life and
    // opened again (KitDialog.ShowOver); the window warms it up once it
    // shows.
    MaterialDialog? materialDialog;

    // Hidden from the designer, whose Properties window would otherwise
    // read it and build a dialog.
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public MaterialDialog MaterialDialog {
      get {
        if (materialDialog == null) {
          var made = materialDialog = new MaterialDialog();
          Disposed += (_, _) => made.Dispose();
        }
        return materialDialog;
      }
    }

    public void OpenNew() {
      var dialog = MaterialDialog;
      dialog.Clear();
      if (FindForm() is not { } owner || dialog.ShowOver(owner) != DialogResult.OK) return;
      toaster.Show($"{dialog.MaterialName} added", Tone.Success);
    }

    void OpenEdit(MaterialRow row) {
      var dialog = MaterialDialog;
      dialog.Clear();
      dialog.LoadRow(row);
      if (FindForm() is not { } owner || dialog.ShowOver(owner) != DialogResult.OK) return;
      toaster.Show($"{dialog.MaterialName} saved", Tone.Success);
    }

    void ConfirmDelete(IReadOnlyList<MaterialRow> doomed) {
      using var dialog = new DeleteMaterialDialog(doomed);
      if (FindForm() is not { } owner || dialog.ShowOver(owner) != DialogResult.OK) return;
      var at = doomed.Select(r => (Row: r, Index: rows.IndexOf(r))).ToList();
      foreach (var r in doomed) rows.Remove(r);
      Refill();

      // Undo puts the rows back where they were.
      var message = doomed.Count == 1 ? $"{doomed[0].Name} deleted" : $"{doomed.Count} materials deleted";
      toaster.Show(message, Tone.Success, "Undo", () => {
        foreach (var (row, index) in at.OrderBy(a => a.Index)) rows.Insert(Math.Min(index, rows.Count), row);
        Refill();
      });
    }

    List<MaterialRow> Picked() {
      var list = new List<MaterialRow>();
      for (var i = 0; i < materialGrid.Rows.Count; i++)
        if (materialGrid.IsPicked(i) && materialGrid.RowAt(i) is { } r) list.Add(r);
      return list;
    }

    // The filter bar and the selection bar share one row of the same
    // height, so swapping them never moves the table.
    void ShowPicked() {
      var n = materialGrid.PickedCount;
      toolbar.SuspendLayout();
      bulkBar.Visible = n > 0;
      filterBar.Visible = n == 0;
      pickedLabel.Text = $"{n} selected";
      deletePickedButton.Text = n == 1 ? "Delete 1 material" : $"Delete {n} materials";
      toolbar.ResumeLayout(true);
    }

    void Refill(bool toFirstPage = false) {
      var query = searchBox.Text.Trim();
      var category = categorySelect.SelectedIndex > 0 ? categorySelect.SelectedOption : null;
      var supplier = supplierSelect.SelectedIndex > 0 ? supplierSelect.SelectedOption : null;
      var stock = stockSelect.SelectedIndex > 0 ? stockSelect.SelectedOption : null;
      var shown = rows.Where(r =>
          (category == null || r.Category == category) &&
          (supplier == null || r.Supplier == supplier) &&
          (stock switch {
            null => true,
            "Low stock" => r.IsLow,
            "Out of stock" => r.Stock == 0,
            _ => r.Stock > 0,
          }) &&
          (query.Length == 0
              || r.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
              || r.Code.Contains(query, StringComparison.OrdinalIgnoreCase)
              || r.Bin.Contains(query, StringComparison.OrdinalIgnoreCase)));

      shown = materialGrid.SortColumn switch {
        "Stock" => materialGrid.SortAscending ? shown.OrderBy(r => r.Stock) : shown.OrderByDescending(r => r.Stock),
        "Cost" => materialGrid.SortAscending ? shown.OrderBy(r => r.Cost) : shown.OrderByDescending(r => r.Cost),
        _ => materialGrid.SortAscending ? shown.OrderBy(r => r.Name) : shown.OrderByDescending(r => r.Name),
      };
      var list = shown.ToList();

      // Total clamps the page to the new last one without an event.
      // Moving to page 1 raises PageChanged, whose handler refills, so
      // this pass ends there.
      pagination.Total = list.Count;
      if (toFirstPage && pagination.Page != 1) {
        pagination.Page = 1;
        return;
      }
      var size = pagination.PageSize;
      materialGrid.ShowRows(list.Skip((pagination.Page - 1) * size).Take(size));
      pageHeader.Description = rows.Count == 1 ? "1 material in the Malolos yard" : $"{rows.Count} materials in the Malolos yard";
    }

    private void stockSelect_SelectedIndexChanged(object sender, EventArgs e) {

    }
  }
}
