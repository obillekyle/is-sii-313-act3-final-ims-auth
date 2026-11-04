using System.ComponentModel;

namespace Kit {
  // A line of text in the kit's type and ink, for the words a form shows
  // outside a kit control: a name beside an avatar, a count over a table. A
  // Label keeps the color and font it was given in the designer; this one
  // follows the theme.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Text))]
  [Description("A line of text in the kit's type scale and ink, following the theme.")]
  public class KitLabel : KitControl {
    int textSize = 14;
    bool strong, quiet, wrap;
    ContentAlignment textAlign = ContentAlignment.MiddleLeft;

    public KitLabel() {
      SetStyle(ControlStyles.Selectable, false);
      TabStop = false;
      AccessibleRole = AccessibleRole.StaticText;
      Size = new Size(160, 20);
    }

    [Category("Appearance"), DefaultValue(14)]
    [Description("The size in px from the type scale: 12, 13, 14, 16, 20 or 24.")]
    public int TextSize { get => textSize; set { textSize = Math.Clamp(value, 8, 48); Invalidate(); } }

    [Category("Appearance"), DefaultValue(false)]
    [Description("Semibold, for a name or a heading.")]
    public bool Strong { get => strong; set { strong = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue(false)]
    [Description("The quieter ink, for a role, a date or a note under a name.")]
    public bool Quiet { get => quiet; set { quiet = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue(false)]
    [Description("Breaks long text onto further lines, from the top. Off, one line ends in an ellipsis.")]
    public bool Wrap { get => wrap; set { wrap = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue(ContentAlignment.MiddleLeft)]
    public ContentAlignment TextAlign { get => textAlign; set { textAlign = value; Invalidate(); } }

    Font TextFont => KitTheme.Font(textSize, strong);

    public override Size GetPreferredSize(Size proposedSize) {
      var font = TextFont;
      if (wrap && proposedSize.Width > 0)
        return new Size(proposedSize.Width, Math.Max(font.Height, Painter.MeasureHeight(Text, font, proposedSize.Width)));
      var size = Painter.Measure(Text, font);
      return new Size(size.Width + 1, Math.Max(font.Height, size.Height));
    }

    static TextFormatFlags Flags(ContentAlignment a) {
      var h = a switch {
        ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter => TextFormatFlags.HorizontalCenter,
        ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight => TextFormatFlags.Right,
        _ => TextFormatFlags.Left,
      };
      var v = a switch {
        ContentAlignment.TopLeft or ContentAlignment.TopCenter or ContentAlignment.TopRight => TextFormatFlags.Top,
        ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight => TextFormatFlags.Bottom,
        _ => TextFormatFlags.VerticalCenter,
      };
      return h | v;
    }

    protected override void OnPaint(PaintEventArgs e) {
      var color = !Enabled ? P.InkDisabled : quiet ? P.Ink2 : P.Ink;
      if (wrap) Painter.Wrapped(e.Graphics, Text, TextFont, ClientRectangle, color, Flags(textAlign) & ~TextFormatFlags.VerticalCenter);
      else Painter.Text(e.Graphics, Text, TextFont, ClientRectangle, color, Flags(textAlign));
    }
  }
}
