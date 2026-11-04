using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Kit {
  public enum ButtonVariant { Primary, Secondary, Ghost, Danger }

  // One button for the whole app. Primary for the one action a
  // screen is for, secondary outlined, ghost for toolbars, danger for a
  // delete. A Button underneath, so a form's AcceptButton, CancelButton and
  // DialogResult work with it as with any button.
  [ToolboxItem(true)]
  [Description("A button: primary, secondary, ghost or danger, with an optional icon before or after its text.")]
  public class KitButton : Button {
    ButtonVariant variant = ButtonVariant.Secondary;
    ControlSize size = ControlSize.Medium;
    string icon = "", iconRight = "", reason = "", shortcut = "";
    bool iconOnly, loading, toggle, pressedState, down;
    readonly System.Windows.Forms.Timer spin = new() { Interval = 40 };
    float spinAngle;
    ToolTip? tip;

    public KitButton() {
      SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
          | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
      base.BackColor = Color.Transparent;
      spin.Tick += (_, _) => { spinAngle = (spinAngle + 30) % 360; Invalidate(); };
      KitTheme.Changed += OnThemeChanged;
    }

    [DefaultValue(typeof(Color), "Transparent")]
    public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }

    [Category("Appearance"), DefaultValue(ButtonVariant.Secondary)]
    [Description("Primary for the one action a screen is for, secondary outlined, ghost for toolbars and lists, danger for a delete.")]
    public ButtonVariant Variant { get => variant; set { variant = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue(ControlSize.Medium)]
    [Description("The height: 28, 32 or 40 px at the comfortable density. Small also sets the text at 13 px.")]
    public ControlSize ControlSize { get => size; set { size = value; Relayout(); } }

    [Category("Appearance"), DefaultValue(""), IconName, TypeConverter(typeof(IconNameConverter))]
    [Description("An icon before the text, by its name in the kit's icon set. A spinner takes its place while loading.")]
    public string Icon { get => icon; set { icon = value ?? ""; Relayout(); } }

    [Category("Appearance"), DefaultValue(""), IconName, TypeConverter(typeof(IconNameConverter))]
    [Description("An icon after the text (chevron-down, arrow-right). Not drawn on an icon-only button.")]
    public string IconRight { get => iconRight; set { iconRight = value ?? ""; Relayout(); } }

    [Category("Appearance"), DefaultValue(false)]
    [Description("Shows only the icon, in a square. The text then names the button and becomes its tooltip.")]
    public bool IconOnly { get => iconOnly; set { iconOnly = value; UpdateTip(); Relayout(); } }

    [Category("Behavior"), DefaultValue(false)]
    [Description("Swaps the icon for a spinner and takes no clicks while work is under way.")]
    public bool Loading {
      get => loading;
      set {
        loading = value;
        if (value) spin.Start(); else spin.Stop();
        Invalidate();
      }
    }

    [Category("Behavior"), DefaultValue("")]
    [Description("Why the button is off, written after it with a circled i while it is disabled: a sentence that says what to do.")]
    public string Reason { get => reason; set { reason = value ?? ""; Relayout(); } }

    [Category("Behavior"), DefaultValue(false)]
    [Description("Makes it a toggle: a click flips Pressed, drawn in the accent's soft fill on ghost and secondary.")]
    public bool Toggle { get => toggle; set { toggle = value; Invalidate(); } }

    [Category("Behavior"), DefaultValue(false)]
    [Description("A toggle's state.")]
    public bool Pressed { get => pressedState; set { if (pressedState == value) return; pressedState = value; Invalidate(); PressedChanged?.Invoke(this, EventArgs.Empty); } }

    [Category("Behavior"), DefaultValue("")]
    [Description("A shortcut shown in the tooltip after the text (Ctrl+S). The button does not listen for it.")]
    public string Shortcut { get => shortcut; set { shortcut = value ?? ""; UpdateTip(); } }

    public event EventHandler? PressedChanged;

    int S(int px) => LogicalToDeviceUnits(px);
    int Pad => S(size == ControlSize.Small ? 8 : size == ControlSize.Large ? 16 : 12);
    int Gap => S(size == ControlSize.Small ? 4 : 6);
    int IconPx => S(size == ControlSize.Small ? 14 : 16);
    int BodyHeight => S(KitTheme.Metrics.Height(size));
    Font TextFont => KitTheme.Font(size == ControlSize.Small ? 13 : 14, strong: true);
    bool ShowsReason => !Enabled && reason.Length > 0;

    void OnThemeChanged(object? sender, EventArgs e) {
      if (IsDisposed) return;
      Relayout();
    }

    void Relayout() {
      if (AutoSize) Size = GetPreferredSize(Size.Empty);
      else SetBounds(Left, Top, Width, Height);
      Invalidate();
    }

    void UpdateTip() {
      var text = iconOnly ? Text : "";
      if (shortcut.Length > 0) text = (text.Length > 0 ? text : Text) + " (" + shortcut + ")";
      if (text.Length == 0) {
        tip?.SetToolTip(this, null);
        return;
      }
      tip ??= new ToolTip();
      tip.SetToolTip(this, text);
    }

    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); UpdateTip(); Relayout(); }
    protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Relayout(); }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
      if (iconOnly && !ShowsReason) width = BodyHeight;
      base.SetBoundsCore(x, y, width, BodyHeight, specified);
    }

    int BodyWidth() {
      if (iconOnly) return BodyHeight;
      var w = Pad * 2 + TextRenderer.MeasureText(Text, TextFont, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
      if (icon.Length > 0 || loading) w += IconPx + Gap;
      if (iconRight.Length > 0) w += IconPx + Gap;
      return Math.Max(w, BodyHeight);
    }

    public override Size GetPreferredSize(Size proposedSize) {
      var w = BodyWidth();
      if (ShowsReason) w += S(8) + S(14) + S(6) + Painter.Measure(reason, KitTheme.Font(13)).Width + 1;
      return new Size(w, BodyHeight);
    }

    // The hover fill eases in and out over Motion.Fast; a press lands at once.
    HoverFade? fade;
    HoverFade Fade => fade ??= new HoverFade(this);

    protected override void OnMouseEnter(EventArgs e) { Fade.Set(0, true); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { down = false; Fade.Set(0, false); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { down = true; Invalidate(); } base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }

    protected override void OnClick(EventArgs e) {
      if (loading) return;
      if (toggle) Pressed = !Pressed;
      base.OnClick(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      // ButtonBase paints no background of its own, so a transparent
      // button paints its parent first or shows a stale buffer.
      ButtonRenderer.DrawParentBackground(g, ClientRectangle, this);
      Painter.Smooth(g);
      var p = KitTheme.Palette;
      var bodyW = ShowsReason ? BodyWidth() : Width;
      var body = new RectangleF(0, 0, bodyW, Height);
      float radius = S(KitTheme.Metrics.RadiusControl);

      Color fill = Color.Empty, border = Color.Empty, ink = p.Ink;
      var on = toggle && pressedState;
      var t = Fade[0];
      switch (variant) {
        case ButtonVariant.Primary:
          fill = down ? Palette.Mix(p.Inverse, 0.74, p.Bg) : Motion.Lerp(p.Inverse, Palette.Mix(p.Inverse, 0.86, p.Bg), t);
          ink = p.InverseInk;
          break;
        case ButtonVariant.Secondary:
          fill = on ? p.AccentSoft : down ? p.BgPress : Motion.Lerp(p.Bg, p.BgHover, t);
          border = on ? p.Accent : p.LineStrong;
          if (on) ink = p.AccentInk;
          break;
        case ButtonVariant.Ghost:
          fill = on ? p.AccentSoft : down ? p.BgPress : t > 0 ? Motion.Lerp(Color.Empty, p.BgHover, t) : Color.Empty;
          if (on) ink = p.AccentInk;
          break;
        case ButtonVariant.Danger:
          fill = down ? Palette.Mix(p.Bad, 0.88, Color.Black) : Motion.Lerp(p.Bad, Palette.Mix(p.Bad, 0.88, Color.Black), t);
          ink = p.Dark ? ColorTranslator.FromHtml("#1e1e1e") : Color.White;
          break;
      }
      if (!Enabled) {
        ink = p.InkDisabled;
        if (variant == ButtonVariant.Ghost) { fill = Color.Empty; border = Color.Empty; } else { fill = p.BgSubtle; border = p.Line; }
      }
      if (!fill.IsEmpty) Painter.Fill(g, fill, body, radius);
      if (!border.IsEmpty) Painter.Outline(g, border, body, radius);
      if (Focused && ShowFocusCues) Painter.FocusRing(g, body, radius, S(2));

      // The content, centered: [icon or spinner] text [icon right]
      var font = TextFont;
      var textW = iconOnly ? 0 : TextRenderer.MeasureText(Text, font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
      var lead = icon.Length > 0 || loading;
      var trail = iconRight.Length > 0 && !iconOnly;
      var contentW = textW + (lead ? IconPx + (textW > 0 ? Gap : 0) : 0) + (trail ? IconPx + Gap : 0);
      var x = (bodyW - contentW) / 2f;
      var iy = (Height - IconPx) / 2f;
      if (lead) {
        var box = new RectangleF(x, iy, IconPx, IconPx);
        if (loading) DrawSpinner(g, box, ink);
        else Icons.Draw(g, icon, box, ink);
        x += IconPx + (textW > 0 ? Gap : 0);
      }
      if (textW > 0) {
        Painter.Text(g, Text, font, new Rectangle((int)x, 0, textW + 1, Height), ink);
        x += textW + Gap;
      }
      if (trail) Icons.Draw(g, iconRight, new RectangleF(x, iy, IconPx, IconPx), ink);

      if (ShowsReason) {
        var rx = bodyW + S(8);
        var small = KitTheme.Font(13);
        Icons.Draw(g, "info", new RectangleF(rx, (Height - S(14)) / 2f, S(14), S(14)), p.Ink2);
        Painter.Text(g, reason, small, new Rectangle(rx + S(20), 0, Width - rx - S(20), Height), p.Ink2);
      }
    }

    void DrawSpinner(Graphics g, RectangleF box, Color color) {
      var r = RectangleF.Inflate(box, -S(1), -S(1));
      using var pen = new Pen(color, S(2)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
      g.DrawArc(pen, r, spinAngle, 270);
    }

    protected override void Dispose(bool disposing) {
      if (disposing) {
        KitTheme.Changed -= OnThemeChanged;
        spin.Dispose();
        tip?.Dispose();
      }
      base.Dispose(disposing);
    }
  }

  // A menu button: a button whose click opens a list of commands.
  // Point DropDown at a ContextMenuStrip on the form and fill it in the
  // designer; it opens under the button, in the kit's colors.
  [ToolboxItem(true)]
  [Description("A button that opens a list of commands: point DropDown at a ContextMenuStrip on the form.")]
  public class MenuButton : KitButton {
    public MenuButton() {
      IconRight = "chevron-down";
    }

    [Category("Appearance"), DefaultValue("chevron-down"), IconName, TypeConverter(typeof(IconNameConverter))]
    [Description("An icon after the text; a chevron pointing down, unless set otherwise.")]
    public new string IconRight { get => base.IconRight; set => base.IconRight = value; }

    [Category("Behavior"), DefaultValue(null)]
    [Description("The commands, in a ContextMenuStrip on the same form.")]
    public ContextMenuStrip? DropDown { get; set; }

    protected override void OnClick(EventArgs e) {
      base.OnClick(e);
      KitMenu.Show(DropDown, this);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
      if (e.KeyCode == Keys.Down || (e.Alt && e.KeyCode == Keys.Down)) {
        KitMenu.Show(DropDown, this);
        e.Handled = true;
      }
      base.OnKeyDown(e);
    }
  }

  // One main action with its close variants behind a chevron:
  // "Export" with "Export as CSV" and "Print". Click is the main action; the
  // chevron opens DropDown.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(Click))]
  [Description("One main action with its close variants behind a chevron.")]
  public class SplitButton : KitControl {
    ButtonVariant variant = ButtonVariant.Secondary;
    ControlSize size = ControlSize.Medium;
    string icon = "";
    bool overChevron;

    public SplitButton() {
      SetStyle(ControlStyles.Selectable, true);
      TabStop = true;
    }

    [Category("Appearance"), DefaultValue(ButtonVariant.Secondary)]
    [Description("Primary or secondary, for both halves.")]
    public ButtonVariant Variant { get => variant; set { variant = value == ButtonVariant.Primary ? value : ButtonVariant.Secondary; Invalidate(); } }

    [Category("Appearance"), DefaultValue(ControlSize.Medium)]
    public ControlSize ControlSize { get => size; set { size = value == ControlSize.Large ? ControlSize.Medium : value; SetBounds(Left, Top, Width, Height); Invalidate(); } }

    [Category("Appearance"), DefaultValue(""), IconName, TypeConverter(typeof(IconNameConverter))]
    [Description("An icon before the main action's text.")]
    public string Icon { get => icon; set { icon = value ?? ""; Invalidate(); } }

    [Category("Behavior"), DefaultValue(null)]
    [Description("The close variants, in a ContextMenuStrip on the same form.")]
    public ContextMenuStrip? DropDown { get; set; }

    int ChevronWidth => S(M.Height(size));

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) =>
        base.SetBoundsCore(x, y, width, S(M.Height(size)), specified);

    public override Size GetPreferredSize(Size proposedSize) {
      var w = S(12) * 2 + Painter.Measure(Text, KitTheme.Font(14, true)).Width + (icon.Length > 0 ? S(22) : 0) + ChevronWidth;
      return new Size(w, S(M.Height(size)));
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      var over = e.X >= Width - ChevronWidth;
      if (over != overChevron) { overChevron = over; Invalidate(); }
      // Each half fades on its own: key 1 the action, key 2 the chevron.
      Fade.Set(1, !over);
      Fade.Set(2, over);
      base.OnMouseMove(e);
    }

    protected override void OnMouseClick(MouseEventArgs e) {
      Focus();
      if (e.X >= Width - ChevronWidth) KitMenu.Show(DropDown, this);
      else base.OnClick(EventArgs.Empty);
      base.OnMouseClick(e);
    }

    protected override void OnClick(EventArgs e) { }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Down || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e) {
      if (e.KeyCode is Keys.Enter or Keys.Space) { base.OnClick(EventArgs.Empty); e.Handled = true; } else if (e.KeyCode == Keys.Down) { KitMenu.Show(DropDown, this); e.Handled = true; }
      base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var primary = variant == ButtonVariant.Primary;
      var body = new RectangleF(0, 0, Width, Height);
      float radius = S(M.RadiusControl);
      var ink = primary ? P.InverseInk : P.Ink;
      if (!Enabled) ink = P.InkDisabled;
      var baseFill = !Enabled ? P.BgSubtle : primary ? P.Inverse : P.Bg;
      Painter.Fill(g, baseFill, body, radius);
      if (Enabled) {
        var hoverFill = primary ? Palette.Mix(P.Inverse, Pressed ? 0.74 : 0.86, P.Bg) : Pressed ? P.BgPress : P.BgHover;
        var state = g.Save();
        using (var clip = Painter.Round(body, radius)) g.SetClip(clip);
        foreach (var (key, half) in new[] { (1, new RectangleF(0, 0, Width - ChevronWidth, Height)), (2, new RectangleF(Width - ChevronWidth, 0, ChevronWidth, Height)) }) {
          var t = Pressed && (key == 2) == overChevron ? 1f : Fade[key];
          if (t <= 0) continue;
          using var b = new SolidBrush(Motion.Lerp(baseFill, hoverFill, t));
          g.FillRectangle(b, half);
        }
        g.Restore(state);
      }
      if (!primary || !Enabled) Painter.Outline(g, Enabled ? P.LineStrong : P.Line, body, radius);
      using (var div = new Pen(primary ? Color.FromArgb(90, P.InverseInk) : P.LineStrong))
        g.DrawLine(div, Width - ChevronWidth, S(1), Width - ChevronWidth, Height - S(1));
      if (ShowFocus) Painter.FocusRing(g, body, radius, S(2));

      var x = S(12);
      if (icon.Length > 0) {
        Icons.Draw(g, icon, new RectangleF(x, (Height - S(16)) / 2f, S(16), S(16)), ink);
        x += S(22);
      }
      Painter.Text(g, Text, KitTheme.Font(size == ControlSize.Small ? 13 : 14, true), new Rectangle(x, 0, Width - ChevronWidth - x, Height), ink);
      Icons.Draw(g, "chevron-down", new RectangleF(Width - ChevronWidth + (ChevronWidth - S(16)) / 2f, (Height - S(16)) / 2f, S(16), S(16)), ink);
    }
  }

  // Opens a menu under a control in the kit's colors and type.
  public static class KitMenu {
    public static void Show(ContextMenuStrip? menu, Control under) {
      if (menu == null) return;
      Style(menu);
      if (menu.Width < under.Width) menu.MinimumSize = new Size(under.Width, 0);
      menu.Show(under, new Point(0, under.Height + under.LogicalToDeviceUnits(4)));
    }

    public static void Style(ToolStrip menu) {
      menu.Renderer = new KitMenuRenderer();
      menu.Font = KitTheme.Font(14);
      menu.BackColor = KitTheme.Palette.Bg;
      menu.ForeColor = KitTheme.Palette.Ink;
      foreach (ToolStripItem item in menu.Items) item.ForeColor = item.ForeColor == Color.Empty || item.ForeColor == SystemColors.ControlText ? KitTheme.Palette.Ink : item.ForeColor;
    }
  }

  public sealed class KitMenuRenderer : ToolStripProfessionalRenderer {
    public KitMenuRenderer() : base(new KitMenuColors()) { RoundedEdges = false; }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
      if (!e.Item.Enabled) e.TextColor = KitTheme.Palette.InkDisabled;
      base.OnRenderItemText(e);
    }

    sealed class KitMenuColors : ProfessionalColorTable {
      static Palette P => KitTheme.Palette;
      public override Color MenuItemSelected => P.BgHover;
      public override Color MenuItemBorder => P.BgHover;
      public override Color MenuItemSelectedGradientBegin => P.BgHover;
      public override Color MenuItemSelectedGradientEnd => P.BgHover;
      public override Color MenuItemPressedGradientBegin => P.BgPress;
      public override Color MenuItemPressedGradientMiddle => P.BgPress;
      public override Color MenuItemPressedGradientEnd => P.BgPress;
      public override Color MenuStripGradientBegin => P.Bg;
      public override Color MenuStripGradientEnd => P.Bg;
      public override Color ToolStripDropDownBackground => P.Bg;
      public override Color ImageMarginGradientBegin => P.Bg;
      public override Color ImageMarginGradientMiddle => P.Bg;
      public override Color ImageMarginGradientEnd => P.Bg;
      public override Color MenuBorder => P.Line;
      public override Color SeparatorDark => P.Line;
      public override Color SeparatorLight => P.Bg;
      public override Color CheckBackground => P.AccentSoft;
      public override Color CheckSelectedBackground => P.AccentSoft;
      public override Color CheckPressedBackground => P.AccentSoft;
      public override Color ButtonSelectedBorder => P.BgHover;
    }
  }
}
