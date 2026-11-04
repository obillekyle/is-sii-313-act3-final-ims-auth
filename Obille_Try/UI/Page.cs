using System.ComponentModel;
using Kit;

namespace Obille_Try.UI {
  // A page of the shell: a UserControl on the theme's page surface
  // (the palette's BgSubtle), which its panels and tiles sit on. The color follows
  // the theme, so the designer does not write it.
  public class Page : UserControl {
    public Page() {
      base.BackColor = KitTheme.Palette.BgSubtle;
      KitTheme.Changed += OnThemeChanged;
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }

    void OnThemeChanged(object? sender, EventArgs e) {
      if (!IsDisposed) base.BackColor = KitTheme.Palette.BgSubtle;
    }

    protected override void Dispose(bool disposing) {
      if (disposing) KitTheme.Changed -= OnThemeChanged;
      base.Dispose(disposing);
    }
  }
}
