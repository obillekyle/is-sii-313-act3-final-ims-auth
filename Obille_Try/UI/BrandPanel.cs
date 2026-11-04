using System.ComponentModel;
using System.Drawing.Drawing2D;
using Kit;

namespace Obille_Try.UI {
  // The left side of the sign-in and register windows: the site photo
  // cropped to fill, the SS mark and the company's name over its top, and
  // what the system is for over its foot. Darkened at both ends so the
  // white text reads over the sky and the ground. One control, so the
  // designer selects it whole. The photo and the text keep their colors in
  // either theme, as the mark does.
  public class BrandPanel : KitControl {
    static readonly Lazy<Image> photo = new(() => {
      using var stream = typeof(BrandPanel).Assembly.GetManifestResourceStream("Obille_Try.UI.site-photo.png")
          ?? throw new InvalidOperationException("The site photo is missing from the assembly.");
      return Image.FromStream(stream);
    });

    string heading = "Stock and deliveries";
    string blurb = "Materials for the yard and every site.";

    public BrandPanel() {
      SetStyle(ControlStyles.Selectable, false);
      TabStop = false;
      Size = new Size(360, 600);
    }

    [Category("Appearance"), DefaultValue("Stock and deliveries")]
    [Description("The large line over the foot of the photo.")]
    public string Heading { get => heading; set { heading = value ?? ""; Invalidate(); } }

    [Category("Appearance"), DefaultValue("Materials for the yard and every site.")]
    [Description("The line under the heading.")]
    public string Blurb { get => blurb; set { blurb = value ?? ""; Invalidate(); } }

    // Opaque from edge to edge, so nothing of the parent is painted first.
    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      var img = photo.Value;

      // Cover: scaled until both sides fill, the overflow cut evenly.
      var scale = Math.Max(Width / (float)img.Width, Height / (float)img.Height);
      var w = img.Width * scale;
      var h = img.Height * scale;
      g.InterpolationMode = InterpolationMode.HighQualityBicubic;
      g.PixelOffsetMode = PixelOffsetMode.HighQuality;
      g.DrawImage(img, new RectangleF((Width - w) / 2, (Height - h) / 2, w, h));

      var top = new Rectangle(0, 0, Width, S(180));
      using (var shade = new LinearGradientBrush(top, Color.FromArgb(225, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), LinearGradientMode.Vertical))
        g.FillRectangle(shade, top.X, top.Y, top.Width, top.Height - 1);
      var foot = new Rectangle(0, Height - S(240), Width, S(240));
      using (var shade = new LinearGradientBrush(foot, Color.FromArgb(0, 0, 0, 0), Color.FromArgb(200, 0, 0, 0), LinearGradientMode.Vertical))
        g.FillRectangle(shade, foot.X, foot.Y + 1, foot.Width, foot.Height - 1);

      // The sidebar's lockup: the name level with the mark's top, what the
      // app is level with its bottom.
      var pad = S(28);
      var mark = new RectangleF(pad, pad, S(40), S(40));
      LogoMark.DrawMark(g, mark);
      var tx = (int)mark.Right + S(12);
      var tw = Width - tx - pad;
      var name = KitTheme.Font(15, true);
      var small = KitTheme.Font(13);
      Painter.Text(g, "SS Construction Co.", name, new Rectangle(tx, (int)mark.Y - S(1), tw, name.Height), Color.White,
          TextFormatFlags.Left | TextFormatFlags.Top);
      Painter.Text(g, "Inventory", small, new Rectangle(tx, (int)mark.Bottom - small.Height + S(1), tw, small.Height),
          Color.FromArgb(230, 255, 255, 255), TextFormatFlags.Left | TextFormatFlags.Top);

      var textWidth = Width - pad * 2;
      var blurbFont = KitTheme.Font(14);
      var headFont = KitTheme.Font(24, true);
      var blurbHeight = Painter.MeasureHeight(blurb, blurbFont, textWidth);
      var blurbTop = Height - pad - blurbHeight;
      Painter.Wrapped(g, blurb, blurbFont, new Rectangle(pad, blurbTop, textWidth, blurbHeight), Color.FromArgb(230, 255, 255, 255));
      var headHeight = Painter.MeasureHeight(heading, headFont, textWidth);
      Painter.Wrapped(g, heading, headFont, new Rectangle(pad, blurbTop - S(6) - headHeight, textWidth, headHeight), Color.White);
    }
  }
}
