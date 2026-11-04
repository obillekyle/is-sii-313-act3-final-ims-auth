using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Kit {
  // What every painted kit control shares: it repaints when the theme
  // changes, scales its sizes with the screen, shows its parent through
  // where it draws nothing, and knows when it is hovered or pressed.
  [ToolboxItem(false)]
  public abstract class KitControl : Control {
    protected KitControl() {
      SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
          | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
      base.BackColor = Color.Transparent;
      KitTheme.Changed += ThemeChanged;
      Fade = new HoverFade(this);
    }

    // Hovers ease over Motion.Fast. Key 0 is the whole control, set by
    // entering and leaving it; a control with rows or options uses its
    // own keys from 1 (an index plus one).
    protected HoverFade Fade { get; }

    // The whole control's hover, eased from 0 to 1.
    protected float HoverT => Fade[0];

    [DefaultValue(typeof(Color), "Transparent")]
    public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }

    protected static Palette P => KitTheme.Palette;
    protected static Metrics M => KitTheme.Metrics;

    protected int S(int px) => LogicalToDeviceUnits(px);
    protected float S(float px) => px * DeviceDpi / 96f;

    protected bool Hovered { get; private set; }
    protected bool Pressed { get; private set; }
    protected bool ShowFocus => Focused && ShowFocusCues;

    void ThemeChanged(object? sender, EventArgs e) {
      if (IsDisposed) return;
      if (InvokeRequired) BeginInvoke(OnThemeChanged);
      else OnThemeChanged();
    }

    // Sizes that come from the density (a control's height) are set again
    // here, then everything repaints.
    protected virtual void OnThemeChanged() {
      if (Parent != null) Parent.PerformLayout(this, nameof(Bounds));
      SetBounds(Left, Top, Width, Height);
      Invalidate();
    }

    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }
    protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    protected override void OnMouseEnter(EventArgs e) { Hovered = true; Fade.Set(0, true); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { Hovered = false; Pressed = false; Fade.Clear(); base.OnMouseLeave(e); }

    protected override void OnMouseDown(MouseEventArgs e) {
      if (e.Button == MouseButtons.Left) { Pressed = true; Invalidate(); }
      base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
      Pressed = false;
      Invalidate();
      base.OnMouseUp(e);
    }

    protected override void Dispose(bool disposing) {
      if (disposing) KitTheme.Changed -= ThemeChanged;
      base.Dispose(disposing);
    }
  }

  // A kit control one control-height tall (a button, an input, a select):
  // its height comes from Size and the density, so it is set here rather
  // than dragged in the designer, as a TextBox's is.
  [ToolboxItem(false)]
  public abstract class KitSizedControl : KitControl {
    ControlSize controlSize = ControlSize.Medium;

    [Category("Appearance"), DefaultValue(ControlSize.Medium)]
    [Description("The height: 28, 32 or 40 px at the comfortable density, 24, 28 or 36 at the compact one.")]
    public ControlSize ControlSize {
      get => controlSize;
      set { controlSize = value; SetBounds(Left, Top, Width, Height); Invalidate(); }
    }

    protected int ControlHeight => S(M.Height(controlSize));

    protected Font BodyFont => KitTheme.Font(controlSize == ControlSize.Small ? 13 : 14);

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
      base.SetBoundsCore(x, y, width, ControlHeight, specified);
    }
  }

  public static class Painter {
    public static void Smooth(Graphics g) {
      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.PixelOffsetMode = PixelOffsetMode.HighQuality;
    }

    public static GraphicsPath Round(RectangleF r, float radius) {
      var path = new GraphicsPath();
      var d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
      if (d <= 0.5f) {
        path.AddRectangle(r);
        return path;
      }
      path.AddArc(r.X, r.Y, d, d, 180, 90);
      path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
      path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
      path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
      path.CloseFigure();
      return path;
    }

    public static void Fill(Graphics g, Color color, RectangleF r, float radius) {
      if (color.A == 0) return;
      using var path = Round(r, radius);
      using var brush = new SolidBrush(color);
      g.FillPath(brush, path);
    }

    // A 1 px line drawn inside the box, on pixel centers.
    public static void Outline(Graphics g, Color color, RectangleF r, float radius, float width = 1) {
      if (color.A == 0) return;
      var inner = RectangleF.Inflate(r, -width / 2, -width / 2);
      using var path = Round(inner, Math.Max(0, radius - width / 2));
      using var pen = new Pen(color, width);
      g.DrawPath(pen, path);
    }

    // The kit's focus ring: 2 px in the accent's text color. A control
    // cannot paint outside its own bounds, so it sits on the edge.
    public static void FocusRing(Graphics g, RectangleF r, float radius, float width) =>
        Outline(g, KitTheme.Palette.AccentInk, r, radius, width);

    public static void Text(Graphics g, string? text, Font font, Rectangle box, Color color,
        TextFormatFlags align = TextFormatFlags.Left | TextFormatFlags.VerticalCenter) {
      if (string.IsNullOrEmpty(text)) return;
      TextRenderer.DrawText(g, text, font, box, color,
          align | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }

    // Text on a known solid fill: ClearType blends its edges with that
    // color rather than whatever the buffer held, so small text on a
    // filled shape (a key, a step's mark) shows no color fringes.
    public static void TextOn(Graphics g, string? text, Font font, Rectangle box, Color color, Color fill,
        TextFormatFlags align = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter) {
      if (string.IsNullOrEmpty(text)) return;
      TextRenderer.DrawText(g, text, font, box, color, fill,
          align | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
    }

    // Scrollbars in the mode's colors on a native control and the
    // controls inside it: Windows' dark Explorer theme on a dark page.
    [System.Runtime.InteropServices.DllImport("uxtheme.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    static extern int SetWindowTheme(IntPtr hwnd, string? appName, string? idList);

    public static void ThemeScrollbars(Control c) {
      void Apply(Control x) {
        if (x.IsHandleCreated) _ = SetWindowTheme(x.Handle, KitTheme.IsDark ? "DarkMode_Explorer" : "Explorer", null);
        else x.HandleCreated += (_, _) => _ = SetWindowTheme(x.Handle, KitTheme.IsDark ? "DarkMode_Explorer" : "Explorer", null);
        foreach (Control child in x.Controls) Apply(child);
      }
      Apply(c);
      c.Invalidate(true);
    }

    public static void Wrapped(Graphics g, string? text, Font font, Rectangle box, Color color,
        TextFormatFlags align = TextFormatFlags.Left | TextFormatFlags.Top) {
      if (string.IsNullOrEmpty(text)) return;
      TextRenderer.DrawText(g, text, font, box, color,
          align | TextFormatFlags.NoPadding | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
    }

    public static Size Measure(string? text, Font font) => string.IsNullOrEmpty(text)
        ? Size.Empty
        : TextRenderer.MeasureText(text, font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

    public static int MeasureHeight(string? text, Font font, int width) => string.IsNullOrEmpty(text)
        ? 0
        : TextRenderer.MeasureText(text, font, new Size(Math.Max(1, width), int.MaxValue),
            TextFormatFlags.NoPadding | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;

    // A soft shadow under a tile that stands alone: a hairline
    // ring with it in the dark mode, where a shadow barely shows.
    public static void Card(Graphics g, RectangleF body, float radius, float scale) {
      var p = KitTheme.Palette;
      var spread = (int)Math.Round(6 * scale);
      for (var i = spread; i >= 1; i--) {
        var r = RectangleF.Inflate(body, i, i);
        r.Offset(0, spread / 3f);
        var alpha = (int)((p.Dark ? 20 : 9) * (1 - (i - 1) / (float)spread));
        Fill(g, Color.FromArgb(alpha, 0, 0, 0), r, radius + i);
      }
      Fill(g, p.Bg, body, radius);
      if (p.Dark) Outline(g, p.Line, body, radius);
    }
  }
}
