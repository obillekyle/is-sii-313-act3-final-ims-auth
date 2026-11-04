using System.ComponentModel;
using System.Drawing.Drawing2D;
using Kit;

namespace Obille_Try.UI {
  public enum Surface { Page, Raised }

  // The shell's regions (the sidebar, the header, the account row): the
  // page or raised surface of the kit's theme, with 1 px lines in the
  // theme's line color on the edges named in Edges.
  public class BorderPanel : Panel {
    AnchorStyles edges = AnchorStyles.None;
    Surface surface = Surface.Raised;

    public BorderPanel() {
      SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
      KitTheme.Changed += OnThemeChanged;
      ApplyColor();
    }

    [Category("Appearance"), DefaultValue(AnchorStyles.None)]
    public AnchorStyles Edges { get => edges; set { edges = value; Invalidate(); } }

    // Raised is the palette's Bg (the sidebar, the header); Page is its
    // BgSubtle, the area the content's panels sit on.
    [Category("Appearance"), DefaultValue(Surface.Raised)]
    public Surface Surface { get => surface; set { surface = value; ApplyColor(); } }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }

    void ApplyColor() => base.BackColor = surface == Surface.Page ? KitTheme.Palette.BgSubtle : KitTheme.Palette.Bg;

    void OnThemeChanged(object? sender, EventArgs e) {
      if (IsDisposed) return;
      ApplyColor();
      Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e) {
      base.OnPaint(e);
      using var pen = new Pen(KitTheme.Palette.Line);
      var g = e.Graphics;
      if (edges.HasFlag(AnchorStyles.Top)) g.DrawLine(pen, 0, 0, Width, 0);
      if (edges.HasFlag(AnchorStyles.Bottom)) g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
      if (edges.HasFlag(AnchorStyles.Left)) g.DrawLine(pen, 0, 0, 0, Height);
      if (edges.HasFlag(AnchorStyles.Right)) g.DrawLine(pen, Width - 1, 0, Width - 1, Height);
    }

    protected override void Dispose(bool disposing) {
      if (disposing) KitTheme.Changed -= OnThemeChanged;
      base.Dispose(disposing);
    }
  }

  // The SS mark: the two letters as one rounded stroke in safety yellow on
  // a charcoal tile, drawn from a 64-unit outline.
  // The brand's own colors, so it keeps them in either theme.
  public class LogoMark : Control {
    static readonly Color Charcoal = Color.FromArgb(0x2B, 0x2D, 0x2F);
    static readonly Color SafetyYellow = Color.FromArgb(0xFF, 0xCD, 0x00);

    public LogoMark() {
      SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
          | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
      BackColor = Color.Transparent;
    }

    static void Letter(GraphicsPath p, float ox) {
      p.StartFigure();
      p.AddLine(30 + ox, 19, 18 + ox, 19);
      p.AddArc(13 + ox, 19, 10, 10, 270, -90);
      p.AddLine(13 + ox, 24, 13 + ox, 27);
      p.AddArc(13 + ox, 22, 10, 10, 180, -90);
      p.AddLine(18 + ox, 32, 22 + ox, 32);
      p.AddArc(17 + ox, 32, 10, 10, 270, 90);
      p.AddLine(27 + ox, 37, 27 + ox, 40);
      p.AddArc(17 + ox, 35, 10, 10, 0, 90);
      p.AddLine(22 + ox, 45, 10 + ox, 45);
    }

    protected override void OnPaint(PaintEventArgs e) => DrawMark(e.Graphics, new RectangleF(0, 0, Math.Min(Width, Height), Math.Min(Width, Height)));

    // The mark in a square box, for a control that draws it among other
    // things (the sidebar's brand row).
    public static void DrawMark(Graphics g, RectangleF box) {
      var state = g.Save();
      Painter.Smooth(g);
      g.TranslateTransform(box.X, box.Y);
      g.ScaleTransform(box.Width / 64f, box.Height / 64f);
      using (var tile = Painter.Round(new RectangleF(0, 0, 64, 64), 14))
      using (var b = new SolidBrush(Charcoal))
        g.FillPath(b, tile);
      using var path = new GraphicsPath();
      Letter(path, 0);
      Letter(path, 24);
      using var pen = new Pen(SafetyYellow, 6) { LineJoin = LineJoin.Round };
      g.DrawPath(pen, path);
      g.Restore(state);
    }
  }

  // The delete dialog's picture, after the sketch: the bin on the danger
  // tone's soft circle.
  public class DeleteArt : KitControl {
    public DeleteArt() {
      SetStyle(ControlStyles.Selectable, false);
      TabStop = false;
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var d = S(72);
      // At the top, level with the first name beside it.
      var circle = new RectangleF((Width - d) / 2f, 0, d, d);
      using (var b = new SolidBrush(P.BadSoft)) g.FillEllipse(b, circle);
      Icons.Draw(g, "trash-2", RectangleF.Inflate(circle, -S(20), -S(20)), P.Bad, S(2f));
    }
  }
}
