using Kit;
using Obille_Try.Data;

namespace Obille_Try.Dialogs {
  // New material, or Edit material when opened from a row. A prototype:
  // nothing is checked or saved, and Add material closes the dialog.
  public partial class MaterialDialog : KitDialog {
    static string CostLabel(string unitDisplay) =>
        $"Standard cost per {SampleData.Units.First(u => u.Display == unitDisplay).Singular}, in pesos";

    public MaterialDialog() {
      InitializeComponent();
      categoryBox.Options = SampleData.Categories;
      unitBox.Options = SampleData.Units.Select(u => u.Display).ToArray();
      supplierBox.Options = SampleData.Suppliers;
      // The sketch's new material: a steel item counted in pieces.
      categoryBox.SelectedOption = "Steel";
      unitBox.SelectedOption = "Pieces (pc)";
      costField.Label = CostLabel("Pieces (pc)");
      unitBox.SelectedIndexChanged += (_, _) => {
        if (unitBox.SelectedOption is { } unit) costField.Label = CostLabel(unit);
      };
      FitHeight();

      // The page keeps one of these and opens it again, so what the
      // designer and the lines above set is kept to put back.
      var (title, action, code, category, unit) = (Title, addButton.Text, codeBox.Text, categoryBox.SelectedIndex, unitBox.SelectedIndex);
      clear = () => {
        Title = title;
        addButton.Text = action;
        nameBox.Text = "";
        codeBox.Text = code;
        categoryBox.SelectedIndex = category;
        unitBox.SelectedIndex = unit;
        costBox.Text = "";
        reorderPointBox.Value = null;
        reorderQuantityBox.Value = null;
        supplierBox.SelectedIndex = -1;
        binBox.Text = "";
      };
    }

    readonly Action clear;

    public string MaterialName => nameBox.Text.Trim().Length > 0 ? nameBox.Text.Trim() : "New material";

    // Back to a blank New material.
    public void Clear() => clear();

    public void LoadRow(MaterialRow row) {
      Title = "Edit material";
      addButton.Text = "Save changes";
      nameBox.Text = row.Name;
      codeBox.Text = row.Code;
      categoryBox.SelectedOption = row.Category;
      unitBox.SelectedOption = SampleData.Units.First(u => u.Abbr == row.Unit).Display;
      costBox.Text = row.Cost.ToString("N2");
      reorderPointBox.Value = row.ReorderPoint;
      reorderQuantityBox.Value = row.ReorderPoint * 2;
      supplierBox.SelectedOption = row.Supplier;
      binBox.Text = row.Bin;
    }

    // As tall as its header, its fields and its footer; the fields' own
    // height depends on the hints that wrap at this width.
    void FitHeight() {
      fields.PerformLayout();
      ClientSize = new Size(ClientSize.Width, HeaderHeight + fields.Height + LogicalToDeviceUnits(16) + footer.Height);
    }

    protected override void OnShown(EventArgs e) {
      base.OnShown(e);
      nameBox.TextBox.Focus();
    }
  }
}
