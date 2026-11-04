using Kit;
using Obille_Try.Data;

namespace Obille_Try.Dialogs {
  // Names what is deleted and the stock that goes with it. Cancel holds the
  // focus, so Enter alone deletes nothing.
  public partial class DeleteMaterialDialog : KitDialog {
    public DeleteMaterialDialog() : this(SampleData.Materials.Take(1).ToList()) {
    }

    public DeleteMaterialDialog(IReadOnlyList<MaterialRow> rows) {
      InitializeComponent();

      if (rows.Count == 1) {
        var r = rows[0];
        subjectLabel.Text = $"{r.Name} ({r.Code})";
        Description = $"Its {r.Quantity(r.Stock)} in stock come off the count. This can't be undone.";
      } else {
        Title = $"Delete {rows.Count} materials?";
        deleteButton.Text = $"Delete {rows.Count}";
        // The code in brackets, so a wrapped code stays with its name
        // rather than reading as an entry of its own.
        var shown = rows.Take(3).Select(r => $"{r.Name} ({r.Code})").ToList();
        if (rows.Count > 3) shown.Add($"and {rows.Count - 3} more");
        subjectLabel.Text = string.Join(Environment.NewLine, shown);
        Description = "Their stock comes off the count. This can't be undone.";
      }

      // The list beside the picture is as tall as its lines, and never
      // shorter than the picture. The body fills the space between the
      // header and the footer, so the dialog is sized and the body
      // follows; setting the body's height first is undone by its dock
      // (a five-name list kept the one-name dialog's 246 px that way).
      var listWidth = ClientSize.Width - Padding.Horizontal - art.Width;
      var listHeight = subjectLabel.GetPreferredSize(new Size(listWidth, 0)).Height;
      var bodyHeight = body.Padding.Vertical + Math.Max(art.Height, listHeight);
      ClientSize = new Size(ClientSize.Width, HeaderHeight + bodyHeight + footer.Height);
    }

    protected override void OnShown(EventArgs e) {
      base.OnShown(e);
      cancelButton.Focus();
    }
  }
}
