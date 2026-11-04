using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Kit {
  public enum InputType { Text, Email, Password, Search, Tel, Url }

  // A control that can show it holds a wrong value: its outline turns red.
  // A Field with an error sets it on the control inside.
  public interface IKitInvalid {
    bool Invalid { get; set; }
  }

  // What every typed field shares: a borderless TextBox inside an outline
  // the kit draws, a 2 px accent ring while it has focus, a gray box when it
  // is read-only. Subclasses add what sits beside the text.
  [ToolboxItem(false)]
  public abstract class KitInputBase : KitControl, IKitInvalid {
    protected readonly KitTextBox Box = new() { BorderStyle = BorderStyle.None };
    ControlSize size = ControlSize.Medium;
    bool invalid;

    [Category("Appearance"), DefaultValue(false)]
    [Description("Outlines the field in red: its value is wrong. A Field with an error sets it.")]
    public bool Invalid { get => invalid; set { invalid = value; Invalidate(); } }

    protected KitInputBase() {
      SetStyle(ControlStyles.Selectable, false);
      Cursor = Cursors.IBeam;
      Box.GotFocus += (_, _) => Invalidate();
      Box.LostFocus += (_, _) => { Invalidate(); OnBoxLeave(); };
      Box.TextChanged += (_, _) => { OnTextChanged(EventArgs.Empty); OnBoxTextChanged(); };
      Box.KeyDown += (_, e) => OnBoxKeyDown(e);
      Controls.Add(Box);
      ApplyColors();
    }

    [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [AllowNull]
    public override string Text { get => Box.Text; set => Box.Text = value ?? ""; }

    [Category("Appearance"), DefaultValue("")]
    [Description("Gray text shown while the field is empty.")]
    public string Placeholder { get => Box.Placeholder; set => Box.Placeholder = value ?? ""; }

    [Category("Behavior"), DefaultValue(false)]
    [Description("The value can be selected and copied but not changed. The box turns gray.")]
    public bool ReadOnly { get => Box.ReadOnly; set { Box.ReadOnly = value; ApplyColors(); Invalidate(); } }

    [Category("Behavior"), DefaultValue(32767)]
    [Description("The most characters the field takes.")]
    public virtual int MaxLength { get => Box.MaxLength; set => Box.MaxLength = value; }

    [Category("Appearance"), DefaultValue(ControlSize.Medium)]
    [Description("The height: 28, 32 or 40 px at the comfortable density. Small also sets the text at 13 px.")]
    public ControlSize ControlSize { get => size; set { size = value; SetBounds(Left, Top, Width, Height); PerformLayout(); Invalidate(); } }

    // The TextBox inside, for anything the kit does not expose.
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TextBox TextBox => Box;

    protected virtual bool FixedHeight => true;
    protected virtual Font TextFont => KitTheme.Font(size == ControlSize.Small ? 13 : 14);
    protected virtual int LeftInset => S(size == ControlSize.Small ? 8 : 12);
    protected virtual int RightInset => S(size == ControlSize.Small ? 8 : 12);
    protected virtual Rectangle BoxBounds => new(0, 0, Width, Height);

    protected virtual void OnBoxTextChanged() { }
    protected virtual void OnBoxKeyDown(KeyEventArgs e) { }
    protected virtual void OnBoxLeave() { }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
      if (FixedHeight) height = S(M.Height(size));
      base.SetBoundsCore(x, y, width, height, specified);
    }

    void ApplyColors() {
      Box.BackColor = !Enabled || Box.ReadOnly ? P.BgSubtle : P.Bg;
      Box.ForeColor = Enabled ? P.Ink : P.InkDisabled;
      Painter.ThemeScrollbars(Box);
    }

    protected override void OnThemeChanged() {
      ApplyColors();
      PerformLayout();
      base.OnThemeChanged();
    }

    protected override void OnEnabledChanged(EventArgs e) {
      ApplyColors();
      base.OnEnabledChanged(e);
    }

    protected override void OnLayout(LayoutEventArgs e) {
      base.OnLayout(e);
      Box.Font = TextFont;
      var outer = BoxBounds;
      var w = Math.Max(10, outer.Width - LeftInset - RightInset);
      if (Box.Multiline)
        Box.SetBounds(outer.X + LeftInset, outer.Y + S(6), w, Math.Max(10, outer.Height - S(12)));
      else
        Box.SetBounds(outer.X + LeftInset, outer.Y + (outer.Height - Box.PreferredHeight) / 2, w, Box.PreferredHeight);
    }

    protected override void OnGotFocus(EventArgs e) {
      base.OnGotFocus(e);
      Box.Focus();
    }

    protected override void OnMouseDown(MouseEventArgs e) {
      base.OnMouseDown(e);
      if (!Box.Focused) Box.Focus();
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var r = (RectangleF)BoxBounds;
      float radius = S(M.RadiusControl);
      Painter.Fill(g, !Enabled || Box.ReadOnly ? P.BgSubtle : P.Bg, r, radius);
      if (invalid) Painter.Outline(g, P.Bad, r, radius, Box.Focused ? S(2) : 1);
      else if (Box.Focused) Painter.FocusRing(g, r, radius, S(2));
      else Painter.Outline(g, Enabled ? Motion.Lerp(P.LineStrong, P.Ink3, HoverT) : P.Line, r, radius);
      PaintAdornments(g);
    }

    protected virtual void PaintAdornments(Graphics g) { }
  }

  // The TextBox inside every kit field. It draws its own placeholder in the
  // theme's hint gray: the native placeholder is a system gray of its own,
  // which put two grays on one form beside the kit's Select.
  [ToolboxItem(false)]
  public sealed class KitTextBox : TextBox {
    string placeholder = "";

    [DefaultValue("")]
    public string Placeholder { get => placeholder; set { placeholder = value; Invalidate(); } }

    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }

    protected override void WndProc(ref Message m) {
      base.WndProc(ref m);
      if (m.Msg != 0x000F || TextLength > 0 || placeholder.Length == 0) return; // WM_PAINT
      using var g = CreateGraphics();
      var align = TextAlign switch {
        HorizontalAlignment.Right => TextFormatFlags.Right,
        HorizontalAlignment.Center => TextFormatFlags.HorizontalCenter,
        _ => TextFormatFlags.Left,
      };
      var flags = align | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | (Multiline ? TextFormatFlags.WordBreak : TextFormatFlags.SingleLine);
      TextRenderer.DrawText(g, placeholder, Font, new Rectangle(1, 0, ClientSize.Width - 2, ClientSize.Height), KitTheme.Palette.Ink3, BackColor, flags);
    }
  }

  // A single-line input: an optional icon before the text,
  // a clear button while there is text, and a show or hide button on a
  // password.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(TextChanged)), DefaultProperty(nameof(Text))]
  [Description("A single-line input, with an optional icon, a clear button and a password's show button.")]
  public class TextInput : KitInputBase {
    InputType type = InputType.Text;
    string icon = "";
    bool clearable, mono, revealed;

    [Category("Behavior"), DefaultValue(InputType.Text)]
    [Description("Password hides the characters and adds a show button. Search puts a magnifier in front when no icon is set.")]
    public InputType Type {
      get => type;
      set {
        type = value;
        Box.UseSystemPasswordChar = value == InputType.Password && !revealed;
        PerformLayout();
        Invalidate();
      }
    }

    [Category("Appearance"), DefaultValue(""), IconName, TypeConverter(typeof(IconNameConverter))]
    [Description("An icon at the left of the box, before the text.")]
    public string Icon { get => icon; set { icon = value ?? ""; PerformLayout(); Invalidate(); } }

    [Category("Behavior"), DefaultValue(false)]
    [Description("Shows a button that empties the field while there is text.")]
    public bool Clearable { get => clearable; set { clearable = value; PerformLayout(); Invalidate(); } }

    [Category("Appearance"), DefaultValue(false)]
    [Description("Sets the text in the monospace font, for codes and IDs.")]
    public bool Mono { get => mono; set { mono = value; PerformLayout(); } }

    string LeadIcon => icon.Length > 0 ? icon : type == InputType.Search ? "search" : "";
    bool ShowClear => clearable && Box.TextLength > 0 && !Box.ReadOnly && Enabled;
    bool ShowEye => type == InputType.Password;

    protected override Font TextFont => mono ? KitTheme.Font(13, mono: true) : base.TextFont;
    protected override int LeftInset => base.LeftInset + (LeadIcon.Length > 0 ? S(24) : 0);
    protected override int RightInset => base.RightInset + (ShowClear ? S(24) : 0) + (ShowEye ? S(24) : 0);

    Rectangle EndButton(int index) => new(Width - S(8) - S(20) * (index + 1) - S(4) * index, (Height - S(20)) / 2, S(20), S(20));

    protected override void OnBoxTextChanged() {
      PerformLayout();
      Invalidate();
    }

    protected override void OnMouseClick(MouseEventArgs e) {
      base.OnMouseClick(e);
      var i = 0;
      if (ShowEye) {
        if (EndButton(i).Contains(e.Location)) {
          revealed = !revealed;
          Box.UseSystemPasswordChar = !revealed;
          Invalidate();
          return;
        }
        i++;
      }
      if (ShowClear && EndButton(i).Contains(e.Location)) {
        Box.Clear();
        Box.Focus();
      }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      base.OnMouseMove(e);
      var overButton = (ShowEye && EndButton(0).Contains(e.Location)) || (ShowClear && EndButton(ShowEye ? 1 : 0).Contains(e.Location));
      Cursor = overButton ? Cursors.Default : Cursors.IBeam;
    }

    protected override void PaintAdornments(Graphics g) {
      var ink = Enabled ? P.Ink3 : P.InkDisabled;
      if (LeadIcon.Length > 0)
        Icons.Draw(g, LeadIcon, new RectangleF(base.LeftInset, (Height - S(16)) / 2f, S(16), S(16)), ink);
      var i = 0;
      if (ShowEye) {
        Icons.Draw(g, revealed ? "eye-off" : "eye", RectangleF.Inflate(EndButton(i), -S(2), -S(2)), P.Ink2);
        i++;
      }
      if (ShowClear) Icons.Draw(g, "x", RectangleF.Inflate(EndButton(i), -S(2), -S(2)), P.Ink2);
    }
  }

  // Multi-line text that grows with its content, from Rows
  // lines up to MaxRows. With MaxLength set, the limit is stated under the
  // box once most of it is used; typing past it is allowed, and the box and
  // the count turn red.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(TextChanged)), DefaultProperty(nameof(Text))]
  [Description("Multi-line text that grows with what is typed, with an optional stated limit.")]
  public class Textarea : KitInputBase {
    int rows = 3, maxRows = 12, limit;
    double threshold = 0.75;

    public Textarea() {
      Box.Multiline = true;
      Box.AcceptsReturn = true;
      Box.WordWrap = true;
      Box.ScrollBars = ScrollBars.None;
      Box.MaxLength = 0;
    }

    [Category("Layout"), DefaultValue(3)]
    [Description("The box's least height, in lines.")]
    public int Rows { get => rows; set { rows = Math.Max(1, value); Grow(); } }

    [Category("Layout"), DefaultValue(12)]
    [Description("The tallest the box grows, in lines. Past that, it scrolls.")]
    public int MaxRows { get => maxRows; set { maxRows = Math.Max(rows, value); Grow(); } }

    [Category("Behavior"), DefaultValue(0)]
    [Description("The character limit, stated under the box. 0 for none.")]
    public override int MaxLength { get => limit; set { limit = Math.Max(0, value); Grow(); } }

    [Category("Behavior"), DefaultValue(0.75)]
    [Description("How much of MaxLength is used, 0 to 1, before the line stating the limit appears.")]
    public double Threshold { get => threshold; set { threshold = Math.Clamp(value, 0, 1); Grow(); } }

    public bool OverLimit => limit > 0 && Box.TextLength > limit;

    protected override bool FixedHeight => false;
    int LineHeight => TextFont.Height;
    bool ShowCount => limit > 0 && Box.TextLength >= limit * threshold;
    int CountHeight => ShowCount ? S(22) : 0;
    protected override Rectangle BoxBounds => new(0, 0, Width, Height - CountHeight);

    protected override void OnBoxTextChanged() => Grow();

    void Grow() {
      if (!IsHandleCreated) { SetBounds(Left, Top, Width, Natural()); return; }
      var lines = Box.TextLength == 0 ? 1 : Box.GetLineFromCharIndex(Box.TextLength) + 1;
      var shown = Math.Clamp(lines, rows, maxRows);
      Box.ScrollBars = lines > maxRows ? ScrollBars.Vertical : ScrollBars.None;
      SetBounds(Left, Top, Width, shown * LineHeight + S(12) + 2 + CountHeight);
      Invalidate();
    }

    int Natural() => rows * LineHeight + S(12) + 2 + CountHeight;

    protected override void OnHandleCreated(EventArgs e) {
      base.OnHandleCreated(e);
      Grow();
    }

    protected override void OnSizeChanged(EventArgs e) {
      base.OnSizeChanged(e);
      if (IsHandleCreated && Box.IsHandleCreated) BeginInvoke(Grow);
    }

    protected override void PaintAdornments(Graphics g) {
      if (OverLimit) {
        var r = (RectangleF)BoxBounds;
        Painter.Outline(g, P.Bad, r, S(M.RadiusControl), S(2));
      }
      if (!ShowCount) return;
      var text = OverLimit
          ? $"{Box.TextLength - limit:N0} over the {limit:N0} character limit"
          : $"{Box.TextLength:N0} of {limit:N0} characters";
      Painter.Text(g, text, KitTheme.Font(13), new Rectangle(0, Height - CountHeight + S(4), Width, S(18)),
          OverLimit ? P.Bad : P.Ink2, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }
  }

  // A number with steppers: − and + joined to the field's
  // right end, the arrow keys step it, and a number typed is kept within
  // Min and Max and rounded to Step when the field is left.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(ValueChanged)), DefaultProperty(nameof(Value))]
  [Description("A number with − and + steppers, held to a minimum, a maximum and a step.")]
  public class NumberInput : KitInputBase {
    decimal? value;
    decimal min = decimal.MinValue, max = decimal.MaxValue, step = 1;
    string unit = "";
    bool steppers = true, settingText;

    public NumberInput() {
      Box.TextAlign = HorizontalAlignment.Left;
    }

    // The text is Value written out, so only Value is saved.
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [AllowNull]
    public override string Text { get => base.Text; set => base.Text = value; }

    [Category("Data"), DefaultValue(null)]
    [Description("The number, or empty for none.")]
    public decimal? Value {
      get => value;
      set {
        var v = value is { } n ? Math.Clamp(n, min, max) : (decimal?)null;
        if (v == this.value && Box.Text == Format(v)) return;
        this.value = v;
        settingText = true;
        Box.Text = Format(v);
        settingText = false;
        Invalidate();
        ValueChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    [Category("Data"), DefaultValue(typeof(decimal), "-79228162514264337593543950335")]
    [Description("The least value. A smaller number typed is raised to it.")]
    public decimal Minimum { get => min; set { min = value; Value = this.value; } }

    [Category("Data"), DefaultValue(typeof(decimal), "79228162514264337593543950335")]
    [Description("The greatest value. A larger number typed is lowered to it.")]
    public decimal Maximum { get => max; set { max = value; Value = this.value; } }

    [Category("Data"), DefaultValue(typeof(decimal), "1")]
    [Description("What a stepper or an arrow key adds. A typed number is rounded to a multiple of it.")]
    public decimal Step { get => step; set => step = value <= 0 ? 1 : value; }

    [Category("Appearance"), DefaultValue("")]
    [Description("A unit after the number (%, px). With one, the number sits at the right, against it.")]
    public string Unit { get => unit; set { unit = value ?? ""; Box.TextAlign = unit.Length > 0 ? HorizontalAlignment.Right : HorizontalAlignment.Left; PerformLayout(); Invalidate(); } }

    [Category("Appearance"), DefaultValue(true)]
    [Description("Draws the − and + buttons.")]
    public bool Steppers { get => steppers; set { steppers = value; PerformLayout(); Invalidate(); } }

    public event EventHandler? ValueChanged;

    int StepWidth => Height;
    protected override int RightInset => base.RightInset + (steppers ? StepWidth * 2 - S(4) : 0) + (unit.Length > 0 ? Painter.Measure(unit, TextFont).Width + S(4) : 0);

    static string Format(decimal? v) => v?.ToString("0.############", CultureInfo.CurrentCulture) ?? "";

    void StepBy(int dir) {
      var start = value ?? (min > 0 ? min : 0);
      Value = start + dir * step;
      Box.SelectionStart = Box.TextLength;
    }

    protected override void OnBoxTextChanged() {
      if (settingText) return;
      if (decimal.TryParse(Box.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var n)) {
        if (n != value) { value = n; ValueChanged?.Invoke(this, EventArgs.Empty); }
      } else if (Box.Text.Length == 0 && value != null) {
        value = null;
        ValueChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    // On leaving the field: kept within the range and rounded to the step.
    protected override void OnBoxLeave() {
      if (value is not { } v) { Box.Text = ""; return; }
      var origin = min > decimal.MinValue ? min : 0;
      var rounded = origin + Math.Round((v - origin) / step) * step;
      Value = rounded;
    }

    protected override void OnBoxKeyDown(KeyEventArgs e) {
      if (e.KeyCode is not (Keys.Up or Keys.Down)) return;
      StepBy(e.KeyCode == Keys.Up ? 1 : -1);
      e.Handled = true;
    }

    protected override void OnMouseDown(MouseEventArgs e) {
      if (steppers && e.X >= Width - StepWidth * 2 && Enabled && !ReadOnly) {
        StepBy(e.X >= Width - StepWidth ? 1 : -1);
        Box.Focus();
        return;
      }
      base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      base.OnMouseMove(e);
      Cursor = steppers && e.X >= Width - StepWidth * 2 ? Cursors.Default : Cursors.IBeam;
    }

    protected override void PaintAdornments(Graphics g) {
      var ink = Enabled ? P.Ink : P.InkDisabled;
      if (unit.Length > 0) {
        var uw = Painter.Measure(unit, TextFont).Width;
        var ux = Width - base.RightInset - (steppers ? StepWidth * 2 - S(4) : 0) - uw;
        Painter.Text(g, unit, TextFont, new Rectangle(ux, 0, uw + 1, Height), P.Ink2);
      }
      if (!steppers) return;
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
      using var pen = new Pen(Box.Focused ? P.AccentInk : P.LineStrong);
      var minus = Width - StepWidth * 2;
      var plus = Width - StepWidth;
      g.DrawLine(pen, minus, S(1), minus, Height - S(2));
      g.DrawLine(pen, plus, S(1), plus, Height - S(2));
      var atMin = value is { } v1 && v1 <= min;
      var atMax = value is { } v2 && v2 >= max;
      Icons.Draw(g, "minus", new RectangleF(minus + (StepWidth - S(14)) / 2f, (Height - S(14)) / 2f, S(14), S(14)), atMin || !Enabled ? P.InkDisabled : ink);
      Icons.Draw(g, "plus", new RectangleF(plus + (StepWidth - S(14)) / 2f, (Height - S(14)) / 2f, S(14), S(14)), atMax || !Enabled ? P.InkDisabled : ink);
    }
  }
}
