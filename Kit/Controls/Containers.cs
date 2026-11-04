using System.ComponentModel;

namespace Kit {
  // What the kit's containers share: a Panel underneath, so Visual Studio's
  // designer takes dropped controls, painted by the kit and repainted when
  // the theme changes.
  [ToolboxItem(false)]
  public abstract class KitContainer : Panel {
    protected KitContainer() {
      SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
          | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
      base.BackColor = Color.Transparent;
      KitTheme.Changed += ThemeChanged;
    }

    [DefaultValue(typeof(Color), "Transparent")]
    public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }

    protected static Palette P => KitTheme.Palette;
    protected static Metrics M => KitTheme.Metrics;
    protected int S(int px) => LogicalToDeviceUnits(px);

    void ThemeChanged(object? sender, EventArgs e) {
      if (IsDisposed) return;
      if (InvokeRequired) BeginInvoke(OnThemeChanged);
      else OnThemeChanged();
    }

    protected virtual void OnThemeChanged() {
      Arrange();
      Invalidate();
    }

    // Sets the padding the container's own drawing needs, and its height
    // where it sizes itself to its content.
    protected virtual void Arrange() { }

    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Arrange(); Invalidate(); }

    protected override void OnControlAdded(ControlEventArgs e) { base.OnControlAdded(e); Arrange(); }
    protected override void OnControlRemoved(ControlEventArgs e) { base.OnControlRemoved(e); Arrange(); }

    protected override void Dispose(bool disposing) {
      if (disposing) KitTheme.Changed -= ThemeChanged;
      base.Dispose(disposing);
    }
  }

  // The label comes first, then the control, then the error under it.
  // Drop one control in and it is placed under the label. A
  // required field's label ends in an asterisk; a hint sits behind a ?
  // after it; a help line sits under the control until an error takes
  // its place.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Label))]
  [Description("A label above one control, an asterisk when it is required, a hint behind a ?, and the error under it.")]
  public class Field : KitContainer {
    string label = "Label", hint = "", error = "", help = "";
    bool optional, inline, side;
    int labelWidth = 140;
    readonly ToolTip tip = new();
    bool overHint;

    public Field() => Size = new Size(280, 64);

    [Category("Appearance"), DefaultValue("Label")]
    [Description("Names the control. It sits above the control, beside it with Inline, or to its left with Side.")]
    public string Label { get => label; set { label = value ?? ""; Arrange(); Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("Help for the field, behind a ? after the label.")]
    public string Hint { get => hint; set { hint = value ?? ""; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("The error message, shown under the control with an alert icon. Empty for none.")]
    public string Error { get => error; set { error = value ?? ""; Arrange(); Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("A line under the control in quieter text, such as a rule the value must meet. An error takes its place while there is one.")]
    public string Help { get => help; set { help = value ?? ""; Arrange(); Invalidate(); } }

    [Category("Behavior"), DefaultValue(false)]
    [Description("Marks the field as one that may be left empty: no asterisk after the label.")]
    public bool Optional { get => optional; set { optional = value; Invalidate(); } }

    [Category("Layout"), DefaultValue(false)]
    [Description("Puts the label after the control on the same line, for a checkbox or a switch.")]
    public bool Inline { get => inline; set { inline = value; Arrange(); Invalidate(); } }

    [Category("Layout"), DefaultValue(false)]
    [Description("Puts the label to the left of the control, in a column LabelWidth wide.")]
    public bool Side { get => side; set { side = value; Arrange(); Invalidate(); } }

    [Category("Layout"), DefaultValue(140)]
    [Description("The label column's width in px when Side is on.")]
    public int LabelWidth { get => labelWidth; set { labelWidth = Math.Max(40, value); Arrange(); Invalidate(); } }

    Font LabelFont => KitTheme.Font(13, true);
    int LabelLine => LabelFont.Height;
    // The room under the control: the error, indented past its icon, or
    // the help line, or none.
    int ErrorHeight =>
        error.Length > 0 ? S(6) + Painter.MeasureHeight(error, KitTheme.Font(13), Math.Max(10, Width - S(20))) :
        help.Length > 0 ? S(6) + Painter.MeasureHeight(help, KitTheme.Font(13), Math.Max(10, Width - Padding.Left)) : 0;
    Control? Inner => Controls.Count > 0 ? Controls[0] : null;

    protected override void OnControlAdded(ControlEventArgs e) {
      if (e.Control != null && e.Control.Dock == DockStyle.None)
        e.Control.Dock = inline ? DockStyle.Left : DockStyle.Top;
      if (e.Control != null) e.Control.SizeChanged += (_, _) => Arrange();
      base.OnControlAdded(e);
    }

    // The label names the control for screen readers and UI Automation:
    // the text box itself for a kit input, which is what takes the focus.
    // A name set on it by hand is kept.
    string? givenName;

    void NameInner() {
      if (Inner is not { } c) return;
      Control target = c is KitInputBase input ? input.TextBox : c;
      if (!string.IsNullOrEmpty(target.AccessibleName) && target.AccessibleName != givenName) return;
      target.AccessibleName = givenName = label;
    }

    protected override void Arrange() {
      NameInner();
      if (Inner is { } c) c.Dock = inline ? DockStyle.Left : DockStyle.Top;
      if (Inner is IKitInvalid marked) marked.Invalid = error.Length > 0;
      var top = inline || side ? 0 : LabelLine + S(6);
      Padding = new Padding(side ? S(labelWidth) : 0, top, 0, ErrorHeight);
      var content = Inner?.Height ?? S(M.HeightMedium);
      var h = Padding.Vertical + (inline ? Math.Max(content, LabelLine) : content);
      if (Height != h) Height = h;
    }

    protected override void OnSizeChanged(EventArgs e) {
      base.OnSizeChanged(e);
      if (error.Length > 0 || help.Length > 0) Arrange();
    }

    Rectangle HintBox() {
      var text = LabelText;
      var w = Painter.Measure(text, LabelFont).Width;
      var x = inline && Inner != null ? Inner.Right + S(8) : 0;
      var y = inline || side ? (Height - ErrorHeight - LabelLine) / 2 : 0;
      return new Rectangle(x + w + S(4), y + (LabelLine - S(14)) / 2, S(14), S(14));
    }

    string LabelText => label + (optional || label.Length == 0 ? "" : " *");

    protected override void OnMouseMove(MouseEventArgs e) {
      var over = hint.Length > 0 && Rectangle.Inflate(HintBox(), S(3), S(3)).Contains(e.Location);
      if (over != overHint) {
        overHint = over;
        if (over) tip.Show(hint, this, HintBox().Left, HintBox().Bottom + S(6));
        else tip.Hide(this);
      }
      base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) {
      overHint = false;
      tip.Hide(this);
      base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var ink = Enabled ? P.Ink : P.InkDisabled;
      var x = inline && Inner != null ? Inner.Right + S(8) : 0;
      var y = inline || side ? (Height - ErrorHeight - LabelLine) / 2 : 0;
      if (side && Inner != null) y = Inner.Top + (Inner.Height - LabelLine) / 2;
      var width = side ? S(labelWidth) - S(12) : Width - x;
      Painter.Text(g, LabelText, LabelFont, new Rectangle(x, y, width, LabelLine), ink);
      if (hint.Length > 0) Icons.Draw(g, "circle-question-mark", HintBox(), P.Ink3);
      if (error.Length > 0) {
        var ey = Height - ErrorHeight + S(6);
        Icons.Draw(g, "circle-alert", new RectangleF(Padding.Left, ey + S(1), S(14), S(14)), P.Bad);
        Painter.Wrapped(g, error, KitTheme.Font(13), new Rectangle(Padding.Left + S(20), ey, Width - Padding.Left - S(20), ErrorHeight), P.Bad);
      } else if (help.Length > 0) {
        var hy = Height - ErrorHeight + S(6);
        Painter.Wrapped(g, help, KitTheme.Font(13), new Rectangle(Padding.Left, hy, Width - Padding.Left, ErrorHeight), Enabled ? P.Ink2 : P.InkDisabled);
      }
    }

    protected override void Dispose(bool disposing) {
      if (disposing) tip.Dispose();
      base.Dispose(disposing);
    }
  }

  // A bordered region with a header: a title, a count or note
  // beside it in quieter text, and the body under it with 16 px of padding,
  // or none with Flush for a table that runs edge to edge.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Title))]
  [Description("A bordered region with a title and a note in its header.")]
  public class KitPanel : KitContainer {
    string title = "", note = "";
    bool flush;

    public KitPanel() {
      Size = new Size(360, 200);
      Arrange();
    }

    [Category("Appearance"), DefaultValue("")]
    [Description("The heading in the panel's header. Without one, the panel has no header.")]
    public string Title { get => title; set { title = value ?? ""; Arrange(); Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("A count or a short fact beside the title, in quieter text.")]
    public string Note { get => note; set { note = value ?? ""; Invalidate(); } }

    [Category("Layout"), DefaultValue(false)]
    [Description("Drops the body's 16 px padding, for a table or a list that runs edge to edge.")]
    public bool Flush { get => flush; set { flush = value; Arrange(); } }

    int HeaderHeight => title.Length > 0 ? S(48) : 0;

    protected override void Arrange() {
      // Flush keeps a 1 px inset for the frame, and the corner radius
      // at the bottom, so a square child stays inside the rounded corners.
      var pad = flush ? S(1) : S(16);
      Padding = new Padding(pad, HeaderHeight + (flush ? 0 : pad), pad, flush ? S(M.RadiusPanel) : pad);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var r = new RectangleF(0, 0, Width, Height);
      Painter.Fill(g, P.Bg, r, S(M.RadiusPanel));
      Painter.Outline(g, P.Line, r, S(M.RadiusPanel));
      if (title.Length == 0) return;
      var font = KitTheme.Font(16, true);
      var tw = Painter.Measure(title, font).Width;
      Painter.Text(g, title, font, new Rectangle(S(16), 0, Width - S(32), HeaderHeight), P.Ink);
      if (note.Length > 0)
        Painter.Text(g, note, KitTheme.Font(13), new Rectangle(S(16) + tw + S(8), 0, Width - tw - S(40), HeaderHeight), P.Ink2);
      using (var pen = new Pen(P.Line)) g.DrawLine(pen, 0, HeaderHeight - 1, Width, HeaderHeight - 1);
    }
  }

  // A section that opens and closes: the title in a
  // summary row, its note and chevron at the right end, a line under the
  // section; the controls inside show while it is open. The title, the
  // content and the line share both edges.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Title)), DefaultEvent(nameof(ExpandedChanged))]
  [Description("A section that opens and closes from its title row.")]
  public class Disclosure : KitContainer {
    string title = "Section", note = "";
    bool expanded = true, hover;
    int openHeight = 160;

    public Disclosure() {
      SetStyle(ControlStyles.Selectable, true);
      TabStop = true;
      Size = new Size(360, 160);
      Arrange();
    }

    [Category("Appearance"), DefaultValue("Section")]
    public string Title { get => title; set { title = value ?? ""; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("A count or a short fact after the title, in quieter text.")]
    public string Note { get => note; set { note = value ?? ""; Invalidate(); } }

    [Category("Behavior"), DefaultValue(true)]
    [Description("Shows the section open.")]
    public bool Expanded {
      get => expanded;
      set {
        if (expanded == value) return;
        if (expanded) openHeight = Height;
        expanded = value;
        foreach (Control c in Controls) c.Visible = value;
        Height = value ? openHeight : HeaderHeight;
        Invalidate();
        ExpandedChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    public event EventHandler? ExpandedChanged;

    int HeaderHeight => S(M.Row);

    protected override void Arrange() => Padding = new Padding(0, HeaderHeight + S(4), 0, S(12));

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
      if (!expanded) height = HeaderHeight;
      base.SetBoundsCore(x, y, width, height, specified);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      var h = e.Y < HeaderHeight;
      if (h != hover) { hover = h; Invalidate(); }
      base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e) {
      if (e.Y < HeaderHeight) { Focus(); Expanded = !expanded; }
      base.OnMouseClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
      if (e.KeyCode is Keys.Enter or Keys.Space) { Expanded = !expanded; e.Handled = true; }
      base.OnKeyDown(e);
    }

    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var head = new RectangleF(0, 0, Width, HeaderHeight);
      if (hover) Painter.Fill(g, P.BgHover, head, S(M.RadiusControl));
      if (Focused && ShowFocusCues) Painter.FocusRing(g, head, S(M.RadiusControl), S(2));
      var font = KitTheme.Font(14, true);
      var chevronX = Width - S(16);
      var noteW = note.Length > 0 ? Painter.Measure(note, KitTheme.Font(13)).Width + 1 : 0;
      Painter.Text(g, title, font, new Rectangle(0, 0, chevronX - noteW - S(16), HeaderHeight), P.Ink);
      if (note.Length > 0)
        Painter.Text(g, note, KitTheme.Font(13), new Rectangle(chevronX - S(8) - noteW, 0, noteW, HeaderHeight), P.Ink2);
      // Pointing right when closed, turned down when open.
      Icons.Draw(g, expanded ? "chevron-down" : "chevron-right", new RectangleF(chevronX, (HeaderHeight - S(16)) / 2f, S(16), S(16)), P.Ink2);
      using (var pen = new Pen(P.Line)) g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
    }
  }

  // The top of every page: the trail above, the page's
  // title, a status badge beside it, one line of facts under it, and the
  // page's actions at the right. Drop the action buttons in; they line up
  // from the right on the title's row.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Title))]
  [Description("A page's top: the trail, the title with a status, a line of facts, and the actions at the right.")]
  public class PageHeader : KitContainer {
    string title = "Page title", description = "", status = "";
    string[] crumbs = Array.Empty<string>();
    Tone statusTone = Tone.Neutral;

    public PageHeader() {
      Size = new Size(640, 72);
      Arrange();
    }

    [Category("Appearance"), DefaultValue("Page title")]
    public string Title { get => title; set { title = value ?? ""; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("Facts about what is on screen, in a line under the title.")]
    public string Description { get => description; set { description = value ?? ""; Arrange(); Invalidate(); } }

    [Category("Appearance")]
    [Description("The trail of pages above this one, shown over the title, top level first.")]
    public string[] Crumbs { get => crumbs; set { crumbs = value ?? Array.Empty<string>(); Arrange(); Invalidate(); } }
    bool ShouldSerializeCrumbs() => crumbs.Length > 0;
    void ResetCrumbs() => Crumbs = Array.Empty<string>();

    [Category("Appearance"), DefaultValue("")]
    [Description("A state shown as a badge beside the title. Empty for none.")]
    public string Status { get => status; set { status = value ?? ""; Invalidate(); } }

    [Category("Appearance"), DefaultValue(Tone.Neutral)]
    public Tone StatusTone { get => statusTone; set { statusTone = value; Invalidate(); } }

    int CrumbHeight => crumbs.Length > 0 ? S(24) : 0;
    int TitleHeight => S(36);
    int DescriptionHeight => description.Length > 0 ? S(22) : 0;

    protected override void Arrange() {
      var h = CrumbHeight + TitleHeight + DescriptionHeight + S(8);
      if (Height != h) Height = h;
      LayoutActions();
    }

    protected override void OnLayout(LayoutEventArgs levent) {
      base.OnLayout(levent);
      LayoutActions();
    }

    void LayoutActions() {
      var x = Width;
      for (var i = 0; i < Controls.Count; i++) {
        var c = Controls[i];
        if (!c.Visible) continue;
        c.Dock = DockStyle.None;
        c.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        x -= c.Width;
        c.Location = new Point(x, CrumbHeight + (TitleHeight - c.Height) / 2);
        x -= S(8);
      }
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var y = 0;
      if (crumbs.Length > 0) {
        var x = 0;
        var font = KitTheme.Font(13);
        for (var i = 0; i < crumbs.Length; i++) {
          var last = i == crumbs.Length - 1;
          var w = Painter.Measure(crumbs[i], font).Width + 1;
          Painter.Text(g, crumbs[i], font, new Rectangle(x, 0, w, CrumbHeight), last ? P.Ink : P.Ink2);
          x += w;
          if (last) break;
          Icons.Draw(g, "chevron-right", new RectangleF(x + S(4), (CrumbHeight - S(12)) / 2f, S(12), S(12)), P.Ink3);
          x += S(20);
        }
        y += CrumbHeight;
      }
      var titleFont = KitTheme.Font(24, true);
      var tw = Painter.Measure(title, titleFont).Width;
      Painter.Text(g, title, titleFont, new Rectangle(0, y, Width, TitleHeight), P.Ink);
      if (status.Length > 0) {
        var (ink, soft) = P.ToneColors(statusTone);
        var bw = S(16) + Painter.Measure(status, KitTheme.Font(12, true)).Width + 1;
        var badge = new RectangleF(tw + S(12), y + (TitleHeight - S(20)) / 2f, bw, S(20));
        Painter.Fill(g, soft, badge, badge.Height / 2);
        Painter.Text(g, status, KitTheme.Font(12, true), Rectangle.Round(badge), ink, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
      }
      y += TitleHeight;
      if (description.Length > 0)
        Painter.Text(g, description, KitTheme.Font(14), new Rectangle(0, y, Width, DescriptionHeight), P.Ink2);
    }
  }
}
