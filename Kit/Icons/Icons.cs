using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Kit {
  // The kit's icons: Lucide, by their names ("chevron-right", "trash-2"),
  // drawn from their 24-unit outlines at any size, with a stroke of 2 on that
  // grid.
  public static class Icons {
    static readonly Dictionary<string, (GraphicsPath Path, bool Fill, bool Stroke)[]> parsed = new();

    public static IReadOnlyCollection<string> Names => IconData.All.Keys;

    public static bool Exists(string? name) => !string.IsNullOrEmpty(name) && IconData.All.ContainsKey(name);

    static (GraphicsPath Path, bool Fill, bool Stroke)[]? Shapes(string name) {
      lock (parsed) {
        if (parsed.TryGetValue(name, out var s)) return s;
        if (!IconData.All.TryGetValue(name, out var data)) return null;
        s = data.Select(d => (SvgPath.Parse(d.D), d.Fill, d.Stroke)).ToArray();
        parsed[name] = s;
        return s;
      }
    }

    // Draws the icon centered in box, as large as the box's shorter side.
    public static void Draw(Graphics g, string? name, RectangleF box, Color color, float stroke = 2f) {
      if (name == null || Shapes(name) is not { } shapes) return;
      var size = Math.Min(box.Width, box.Height);
      var scale = size / 24f;
      var state = g.Save();
      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.PixelOffsetMode = PixelOffsetMode.HighQuality;
      g.TranslateTransform(box.X + (box.Width - size) / 2, box.Y + (box.Height - size) / 2);
      g.ScaleTransform(scale, scale);
      using var pen = new Pen(color, stroke) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
      using var brush = new SolidBrush(color);
      foreach (var (path, fill, strokeIt) in shapes) {
        if (fill) g.FillPath(brush, path);
        if (strokeIt) g.DrawPath(pen, path);
      }
      g.Restore(state);
    }

    public static Bitmap ToBitmap(string? name, int px, Color color) {
      var bmp = new Bitmap(px, px);
      using var g = Graphics.FromImage(bmp);
      Draw(g, name, new RectangleF(0, 0, px, px), color);
      return bmp;
    }
  }

  // The icon picker in Visual Studio's Properties window: every icon name in
  // a dropdown, typed to jump. A name not in the set is still accepted as
  // typed, so a project can name an icon it adds later.
  public sealed class IconNameConverter : StringConverter {
    static StandardValuesCollection? values;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => false;

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context) =>
        values ??= new StandardValuesCollection(new[] { "" }.Concat(Icons.Names.OrderBy(n => n, StringComparer.Ordinal)).ToArray());
  }

  // Marks a property as an icon name: the gallery and any app that calls
  // KitEditors.Install() show the visual icon grid for it.
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class IconNameAttribute : Attribute { }
}
