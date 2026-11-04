using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Kit {
  // The choice controls share one shape of data: Options, a list of labels,
  // and SelectedIndex into it (-1 for none), with SelectedIndexChanged.
  [ToolboxItem(false)]
  public abstract class KitChoiceControl : KitControl {
    string[] options = Array.Empty<string>();
    int selected = -1;

    protected KitChoiceControl() {
      SetStyle(ControlStyles.Selectable, true);
      TabStop = true;
    }

    [Category("Data")]
    [Description("The choices, one label per line.")]
    public string[] Options {
      get => options;
      set {
        options = value ?? Array.Empty<string>();
        if (selected >= options.Length) selected = -1;
        OnOptionsChanged();
        Invalidate();
      }
    }
    bool ShouldSerializeOptions() => options.Length > 0;
    void ResetOptions() => Options = Array.Empty<string>();

    [Category("Data"), DefaultValue(-1)]
    [Description("The chosen option's position in Options, from 0; -1 for none.")]
    public int SelectedIndex {
      get => selected;
      set {
        var v = value < -1 || value >= options.Length ? -1 : value;
        if (v == selected) return;
        selected = v;
        Invalidate();
        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    // The chosen option by its words. Setting words that are not an option
    // chooses nothing.
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? SelectedOption {
      get => selected >= 0 && selected < options.Length ? options[selected] : null;
      set => SelectedIndex = value == null ? -1 : Array.IndexOf(options, value);
    }

    public event EventHandler? SelectedIndexChanged;

    protected virtual void OnOptionsChanged() { }
  }

  // The platform's own kind of select, drawn to match the inputs:
  // the chosen option, or the placeholder in gray, and a
  // chevron; the options open in a menu under it.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(SelectedIndexChanged)), DefaultProperty(nameof(Options))]
  [Description("A select: the options open in a menu under the field.")]
  public class Select : KitChoiceControl, IKitInvalid {
    ControlSize size = ControlSize.Medium;
    string placeholder = "";
    bool invalid;

    [Category("Appearance"), DefaultValue(false)]
    [Description("Outlines the select in red: its value is wrong. A Field with an error sets it.")]
    public bool Invalid { get => invalid; set { invalid = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue(ControlSize.Medium)]
    public ControlSize ControlSize { get => size; set { size = value; SetBounds(Left, Top, Width, Height); Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("Shown in gray while nothing is chosen. It is never a choice.")]
    public string Placeholder { get => placeholder; set { placeholder = value ?? ""; Invalidate(); } }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) =>
        base.SetBoundsCore(x, y, width, S(M.Height(size)), specified);

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Up or Keys.Down || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e) {
      if ((e.Alt && e.KeyCode == Keys.Down) || e.KeyCode is Keys.F4 or Keys.Space or Keys.Enter) { Open(); e.Handled = true; } else if (e.KeyCode == Keys.Down && Options.Length > 0) { SelectedIndex = Math.Min(Options.Length - 1, SelectedIndex + 1); e.Handled = true; } else if (e.KeyCode == Keys.Up && Options.Length > 0) { SelectedIndex = Math.Max(0, SelectedIndex - 1); e.Handled = true; }
      base.OnKeyDown(e);
    }

    protected override void OnMouseClick(MouseEventArgs e) {
      Focus();
      Open();
      base.OnMouseClick(e);
    }

    void Open() {
      if (!Enabled || Options.Length == 0) return;
      var menu = new ContextMenuStrip { ShowCheckMargin = true, ShowImageMargin = false };
      for (var i = 0; i < Options.Length; i++) {
        var index = i;
        var item = new ToolStripMenuItem(Options[i]) { Checked = i == SelectedIndex };
        item.Click += (_, _) => SelectedIndex = index;
        menu.Items.Add(item);
      }
      menu.Closed += (_, _) => BeginInvoke(menu.Dispose);
      KitMenu.Show(menu, this);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var r = new RectangleF(0, 0, Width, Height);
      float radius = S(M.RadiusControl);
      Painter.Fill(g, Enabled ? P.Bg : P.BgSubtle, r, radius);
      if (invalid) Painter.Outline(g, P.Bad, r, radius, ShowFocus ? S(2) : 1);
      else if (ShowFocus) Painter.FocusRing(g, r, radius, S(2));
      else Painter.Outline(g, Enabled ? Motion.Lerp(P.LineStrong, P.Ink3, HoverT) : P.Line, r, radius);
      var pad = S(size == ControlSize.Small ? 8 : 12);
      var font = KitTheme.Font(size == ControlSize.Small ? 13 : 14);
      var chosen = SelectedOption;
      Painter.Text(g, chosen ?? placeholder, font, new Rectangle(pad, 0, Width - pad * 2 - S(20), Height),
          !Enabled ? P.InkDisabled : chosen == null ? P.Ink3 : P.Ink);
      Icons.Draw(g, "chevron-down", new RectangleF(Width - pad - S(16), (Height - S(16)) / 2f, S(16), S(16)), Enabled ? P.Ink2 : P.InkDisabled);
    }
  }

  // A searchable list: typing filters the options, the
  // arrows move through what matched, Enter takes it. With Creatable, text
  // that matches no option can be taken as a new value.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(ValueChanged)), DefaultProperty(nameof(Options))]
  [Description("A field that filters a list of options as it is typed in.")]
  public class Combobox : KitInputBase {
    string[] options = Array.Empty<string>();
    string value = "", emptyText = "No matches";
    int limit;
    bool creatable, settingText;
    PopupList? popup;

    [Category("Data")]
    [Description("The choices, one label per line.")]
    public string[] Options { get => options; set => options = value ?? Array.Empty<string>(); }
    bool ShouldSerializeOptions() => options.Length > 0;
    void ResetOptions() => Options = Array.Empty<string>();

    [Category("Data"), DefaultValue("")]
    [Description("The chosen option, or the new value typed when Creatable is on.")]
    public string Value {
      get => value;
      set {
        this.value = value ?? "";
        settingText = true;
        Box.Text = this.value;
        settingText = false;
        ValueChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    [Category("Behavior"), DefaultValue(false)]
    [Description("Offers to take the typed text as a new value when it matches no option.")]
    public bool Creatable { get => creatable; set => creatable = value; }

    [Category("Appearance"), DefaultValue("No matches")]
    [Description("The line the list shows when nothing matches.")]
    public string EmptyText { get => emptyText; set => emptyText = value ?? ""; }

    [Category("Behavior"), DefaultValue(0)]
    [Description("The most rows the list shows; 0 for all. With more matches, its last line says how many more there are.")]
    public int Limit { get => limit; set => limit = Math.Max(0, value); }

    public event EventHandler? ValueChanged;

    // The text is what is being typed; Value is what is chosen, and only
    // Value is saved.
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string Text { get => base.Text; set => base.Text = value; }

    protected override int RightInset => base.RightInset + S(22);

    List<string> Matches() {
      var q = Box.Text.Trim();
      return q.Length == 0 || q == value
          ? options.ToList()
          : options.Where(o => o.Contains(q, StringComparison.CurrentCultureIgnoreCase)).ToList();
    }

    protected override void OnBoxTextChanged() {
      if (settingText) return;
      ShowList();
    }

    void ShowList() {
      if (!Box.Focused || !Enabled) return;
      popup ??= new PopupList(this, Take);
      var matches = Matches();
      var shown = limit > 0 ? matches.Take(limit).ToList() : matches;
      var rows = shown.Select(m => new PopupRow(m, m)).ToList();
      var typed = Box.Text.Trim();
      if (creatable && typed.Length > 0 && !options.Any(o => string.Equals(o, typed, StringComparison.CurrentCultureIgnoreCase)))
        rows.Add(new PopupRow($"Add \"{typed}\"", typed));
      string? footer = limit > 0 && matches.Count > limit ? $"{matches.Count - limit} more; type to narrow them" : null;
      popup.Open(rows, rows.Count == 0 ? emptyText : null, footer, value);
    }

    void Take(string chosen) {
      Value = chosen;
      Box.SelectionStart = Box.TextLength;
      popup?.Hide();
    }

    protected override void OnBoxKeyDown(KeyEventArgs e) {
      switch (e.KeyCode) {
        case Keys.Down:
          if (popup is not { Visible: true }) ShowList(); else popup.StepBy(1);
          e.Handled = true;
          break;
        case Keys.Up when popup is { Visible: true }:
          popup.StepBy(-1);
          e.Handled = true;
          break;
        case Keys.Enter when popup is { Visible: true }:
          popup.TakeCurrent();
          e.Handled = e.SuppressKeyPress = true;
          break;
        case Keys.Escape when popup is { Visible: true }:
          popup.Hide();
          e.Handled = e.SuppressKeyPress = true;
          break;
      }
    }

    // Leaving with text that is no option puts the chosen value back,
    // unless new values are allowed.
    protected override void OnBoxLeave() {
      popup?.Hide();
      var typed = Box.Text.Trim();
      var match = options.FirstOrDefault(o => string.Equals(o, typed, StringComparison.CurrentCultureIgnoreCase));
      if (match != null) Value = match;
      else if (creatable && typed.Length > 0) Value = typed;
      else if (typed.Length == 0) Value = "";
      else Value = value;
    }

    protected override void OnMouseClick(MouseEventArgs e) {
      base.OnMouseClick(e);
      if (e.X >= Width - S(32)) { Box.Focus(); ShowList(); }
    }

    protected override void PaintAdornments(Graphics g) =>
        Icons.Draw(g, "chevron-down", new RectangleF(Width - S(12) - S(16), (Height - S(16)) / 2f, S(16), S(16)), Enabled ? P.Ink2 : P.InkDisabled);

    protected override void Dispose(bool disposing) {
      if (disposing) popup?.Dispose();
      base.Dispose(disposing);
    }
  }

  internal readonly record struct PopupRow(string Label, string Value);

  // A list under a field that never takes focus from it, so typing goes on
  // in the field while the list follows. Closes when the field's form moves
  // or loses focus.
  internal sealed class PopupList : Form {
    readonly Control owner;
    readonly Action<string> take;
    List<PopupRow> rows = new();
    string? message, footer;
    int current = -1;

    public PopupList(Control owner, Action<string> take) {
      this.owner = owner;
      this.take = take;
      FormBorderStyle = FormBorderStyle.None;
      ShowInTaskbar = false;
      StartPosition = FormStartPosition.Manual;
      DoubleBuffered = true;
      KeyPreview = false;
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams {
      get {
        var cp = base.CreateParams;
        cp.ExStyle |= 0x08000000 | 0x00000080; // WS_EX_NOACTIVATE, WS_EX_TOOLWINDOW
        cp.ClassStyle |= 0x20000; // CS_DROPSHADOW
        return cp;
      }
    }

    int RowH => owner.LogicalToDeviceUnits(KitTheme.Metrics.Row - 8);
    int Pad => owner.LogicalToDeviceUnits(4);

    public void Open(List<PopupRow> list, string? empty, string? foot, string chosen) {
      rows = list;
      message = list.Count == 0 ? empty : null;
      footer = foot;
      current = Math.Max(0, rows.FindIndex(r => r.Value == chosen));
      if (rows.Count == 0) current = -1;
      var lines = rows.Count + (message != null ? 1 : 0) + (footer != null ? 1 : 0);
      var shownLines = Math.Min(lines, 8);
      var screen = owner.RectangleToScreen(owner.ClientRectangle);
      Bounds = new Rectangle(screen.Left, screen.Bottom + owner.LogicalToDeviceUnits(4), screen.Width, shownLines * RowH + Pad * 2);
      BackColor = KitTheme.Palette.Bg;
      if (!Visible) {
        var form = owner.FindForm();
        if (form != null) {
          Owner = form;
          form.Move -= OwnerMoved;
          form.Move += OwnerMoved;
          form.Deactivate -= OwnerMoved;
          form.Deactivate += OwnerMoved;
        }
        Show();
      }
      Invalidate();
    }

    void OwnerMoved(object? sender, EventArgs e) => Hide();

    public void StepBy(int by) {
      if (rows.Count == 0) return;
      current = Math.Clamp(current + by, 0, rows.Count - 1);
      Invalidate();
    }

    public void TakeCurrent() {
      if (current >= 0 && current < rows.Count) take(rows[current].Value);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      var i = (e.Y - Pad) / RowH;
      if (i >= 0 && i < rows.Count && i != current) { current = i; Invalidate(); }
      base.OnMouseMove(e);
    }

    protected override void OnMouseDown(MouseEventArgs e) {
      var i = (e.Y - Pad) / RowH;
      if (i >= 0 && i < rows.Count) take(rows[i].Value);
      base.OnMouseDown(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      var p = KitTheme.Palette;
      g.Clear(p.Bg);
      using (var pen = new Pen(p.Line)) g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
      var font = KitTheme.Font(14);
      var x = owner.LogicalToDeviceUnits(12);
      var y = Pad;
      for (var i = 0; i < rows.Count && y < Height; i++, y += RowH) {
        var r = new Rectangle(Pad, y, Width - Pad * 2, RowH);
        if (i == current) { Painter.Smooth(g); Painter.Fill(g, p.BgHover, r, owner.LogicalToDeviceUnits(4)); }
        Painter.Text(g, rows[i].Label, font, new Rectangle(x, y, Width - x * 2, RowH), p.Ink);
      }
      if (message != null) { Painter.Text(g, message, font, new Rectangle(x, y, Width - x * 2, RowH), p.Ink3); y += RowH; }
      if (footer != null) Painter.Text(g, footer, KitTheme.Font(13), new Rectangle(x, y, Width - x * 2, RowH), p.Ink3);
    }
  }

  // A real two-state box with an optional third, mixed state:
  // 18 px, filled in the inverse color when checked.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(CheckedChanged)), DefaultProperty(nameof(Checked))]
  [Description("A checkbox with its words beside it; a mixed state for a select-all.")]
  public class Checkbox : KitControl {
    bool isChecked, indeterminate;

    public Checkbox() {
      SetStyle(ControlStyles.Selectable, true);
      TabStop = true;
    }

    [Category("Behavior"), DefaultValue(false)]
    public bool Checked {
      get => isChecked;
      set { if (isChecked == value && !indeterminate) return; isChecked = value; indeterminate = false; Invalidate(); CheckedChanged?.Invoke(this, EventArgs.Empty); }
    }

    [Category("Behavior"), DefaultValue(false)]
    [Description("Draws the mixed state, a dash, for a select-all over a partly chosen list. A click clears it.")]
    public bool Indeterminate { get => indeterminate; set { indeterminate = value; Invalidate(); } }

    public event EventHandler? CheckedChanged;

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) =>
        base.SetBoundsCore(x, y, width, Math.Max(S(20), Painter.Measure("Ag", KitTheme.Font(14)).Height), specified);

    public override Size GetPreferredSize(Size proposedSize) =>
        new(S(18) + (Text.Length > 0 ? S(8) + Painter.Measure(Text, KitTheme.Font(14)).Width + 1 : 0), Height);

    protected override void OnClick(EventArgs e) {
      if (Enabled) Checked = indeterminate || !isChecked;
      Focus();
      base.OnClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
      if (e.KeyCode == Keys.Space) { OnClick(EventArgs.Empty); e.Handled = true; }
      base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var box = new RectangleF(0, (Height - S(18)) / 2f, S(18), S(18));
      float radius = S(M.RadiusTag);
      var on = isChecked || indeterminate;
      if (!Enabled) {
        Painter.Fill(g, P.BgSubtle, box, radius);
        Painter.Outline(g, P.Line, box, radius);
      } else if (on) Painter.Fill(g, P.Inverse, box, radius);
      else {
        Painter.Fill(g, P.Bg, box, radius);
        Painter.Outline(g, Motion.Lerp(P.LineStrong, P.Ink3, HoverT), box, radius);
      }
      var mark = Enabled ? P.InverseInk : P.InkDisabled;
      if (indeterminate) Icons.Draw(g, "minus", RectangleF.Inflate(box, -S(3), -S(3)), mark, 3);
      else if (isChecked) Icons.Draw(g, "check", RectangleF.Inflate(box, -S(3), -S(3)), mark, 3);
      if (ShowFocus) Painter.FocusRing(g, RectangleF.Inflate(box, S(2), S(2)), radius + S(2), S(2));
      if (Text.Length > 0)
        Painter.Text(g, Text, KitTheme.Font(14), new Rectangle(S(26), 0, Width - S(26), Height), Enabled ? P.Ink : P.InkDisabled);
    }
  }

  // An on and off setting that applies at once: the words
  // first, the switch at the row's right end.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(CheckedChanged)), DefaultProperty(nameof(Checked))]
  [Description("An on and off setting that applies at once, its words before it.")]
  public class Switch : KitControl {
    bool isOn;

    public Switch() {
      SetStyle(ControlStyles.Selectable, true);
      TabStop = true;
    }

    [Category("Behavior"), DefaultValue(false)]
    public bool Checked { get => isOn; set { if (isOn == value) return; isOn = value; Invalidate(); CheckedChanged?.Invoke(this, EventArgs.Empty); } }

    public event EventHandler? CheckedChanged;

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) =>
        base.SetBoundsCore(x, y, width, S(24), specified);

    public override Size GetPreferredSize(Size proposedSize) =>
        new(S(32) + (Text.Length > 0 ? S(12) + Painter.Measure(Text, KitTheme.Font(14)).Width + 1 : 0), S(24));

    protected override void OnClick(EventArgs e) {
      if (Enabled) Checked = !isOn;
      Focus();
      base.OnClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
      if (e.KeyCode is Keys.Space or Keys.Enter) { OnClick(EventArgs.Empty); e.Handled = true; }
      base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var track = new RectangleF(Width - S(32), (Height - S(18)) / 2f, S(32), S(18));
      var fill = !Enabled ? P.Line : isOn ? P.AccentInk : P.Ink3;
      Painter.Fill(g, fill, track, track.Height / 2);
      var d = S(14);
      var tx = isOn ? track.Right - S(2) - d : track.X + S(2);
      using (var b = new SolidBrush(P.Bg)) g.FillEllipse(b, tx, track.Y + S(2), d, d);
      if (ShowFocus) Painter.FocusRing(g, RectangleF.Inflate(track, S(2), S(2)), track.Height / 2 + S(2), S(2));
      if (Text.Length > 0)
        Painter.Text(g, Text, KitTheme.Font(14), new Rectangle(0, 0, (int)track.X - S(12), Height), Enabled ? P.Ink : P.InkDisabled);
    }
  }

  public enum Direction { Column, Row }

  // Radios sharing one choice: arrow keys move and choose,
  // as the platform's do. Stacked in a column, or set in a row.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(SelectedIndexChanged)), DefaultProperty(nameof(Options))]
  [Description("A set of radios for one choice among a few long options.")]
  public class RadioGroup : KitChoiceControl {
    Direction direction = Direction.Column;
    int hover = -1;

    [Category("Layout"), DefaultValue(Direction.Column)]
    [Description("Stacks the options in a column, or sets them in a row.")]
    public Direction Direction { get => direction; set { direction = value; Invalidate(); } }

    int RowH => S(28);
    Font TextFont => KitTheme.Font(14);

    Rectangle ItemBounds(int i) {
      if (direction == Direction.Column) return new Rectangle(0, i * RowH, Width, RowH);
      var x = 0;
      for (var k = 0; k < i; k++) x += S(26) + Painter.Measure(Options[k], TextFont).Width + S(20);
      return new Rectangle(x, 0, S(26) + Painter.Measure(Options[i], TextFont).Width, RowH);
    }

    public override Size GetPreferredSize(Size proposedSize) {
      if (Options.Length == 0) return new Size(S(120), RowH);
      if (direction == Direction.Column)
        return new Size(S(26) + Options.Max(o => Painter.Measure(o, TextFont).Width) + 1, Options.Length * RowH);
      var last = ItemBounds(Options.Length - 1);
      return new Size(last.Right + 1, RowH);
    }

    protected override void OnOptionsChanged() {
      if (AutoSize || Height < GetPreferredSize(Size.Empty).Height) Size = GetPreferredSize(Size.Empty);
    }

    int HitTest(Point pt) {
      for (var i = 0; i < Options.Length; i++) if (ItemBounds(i).Contains(pt)) return i;
      return -1;
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      var h = HitTest(e.Location);
      if (h != hover) {
        // An option's hover eases in and out (key: its index + 1).
        if (hover >= 0) Fade.Set(hover + 1, false);
        if (h >= 0) Fade.Set(h + 1, true);
        hover = h;
        Invalidate();
      }
      base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hover = -1; base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e) {
      var i = HitTest(e.Location);
      if (i >= 0 && Enabled) SelectedIndex = i;
      Focus();
      base.OnMouseClick(e);
    }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e) {
      if (Options.Length > 0 && e.KeyCode is Keys.Down or Keys.Right) { SelectedIndex = (SelectedIndex + 1) % Options.Length; e.Handled = true; } else if (Options.Length > 0 && e.KeyCode is Keys.Up or Keys.Left) { SelectedIndex = SelectedIndex <= 0 ? Options.Length - 1 : SelectedIndex - 1; e.Handled = true; }
      base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      for (var i = 0; i < Options.Length; i++) {
        var r = ItemBounds(i);
        var dot = new RectangleF(r.X, r.Y + (r.Height - S(18)) / 2f, S(18), S(18));
        var on = i == SelectedIndex;
        using (var fill = new SolidBrush(Enabled ? P.Bg : P.BgSubtle)) g.FillEllipse(fill, dot);
        var ring = !Enabled ? P.Line : on ? P.Inverse : Fade.Mix(i + 1, P.LineStrong, P.Ink3);
        using (var pen = new Pen(ring, on ? S(5) : 1)) {
          var inset = on ? S(2.5f) : 0.5f;
          g.DrawEllipse(pen, RectangleF.Inflate(dot, -inset, -inset));
        }
        if (ShowFocus && (on || (SelectedIndex < 0 && i == 0)))
          using (var focus = new Pen(P.AccentInk, S(2))) g.DrawEllipse(focus, RectangleF.Inflate(dot, S(2), S(2)));
        Painter.Text(g, Options[i], TextFont, new Rectangle(r.X + S(26), r.Y, r.Width - S(26), r.Height), Enabled ? P.Ink : P.InkDisabled);
      }
    }
  }

  // Two to five short choices side by side: a view or a
  // mode. The chosen one sits raised on a subtle track.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(SelectedIndexChanged)), DefaultProperty(nameof(Options))]
  [Description("Two to five short choices side by side, such as a view or a mode.")]
  public class Segmented : KitChoiceControl {
    ControlSize size = ControlSize.Medium;
    string[] icons = Array.Empty<string>();
    bool iconOnly;
    int hover = -1;

    [Category("Appearance"), DefaultValue(ControlSize.Medium)]
    public ControlSize ControlSize { get => size; set { size = value == ControlSize.Large ? ControlSize.Medium : value; SetBounds(Left, Top, Width, Height); Invalidate(); } }

    [Category("Appearance")]
    [Description("An icon per option, by name, in the same order as Options.")]
    public string[] OptionIcons { get => icons; set { icons = value ?? Array.Empty<string>(); Invalidate(); } }
    bool ShouldSerializeOptionIcons() => icons.Length > 0;
    void ResetOptionIcons() => OptionIcons = Array.Empty<string>();

    [Category("Appearance"), DefaultValue(false)]
    [Description("Shows only the icons; each label becomes its option's tooltip and name.")]
    public bool IconOnly { get => iconOnly; set { iconOnly = value; Invalidate(); } }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) =>
        base.SetBoundsCore(x, y, width, S(M.Height(size)), specified);

    Font TextFont => KitTheme.Font(size == ControlSize.Small ? 13 : 14, true);

    int ItemWidth(int i) => iconOnly ? Height - S(4) : S(24) + Painter.Measure(Options[i], TextFont).Width + (i < icons.Length && icons[i].Length > 0 ? S(22) : 0);

    Rectangle ItemBounds(int i) {
      var x = S(2);
      for (var k = 0; k < i; k++) x += ItemWidth(k);
      return new Rectangle(x, S(2), ItemWidth(i), Height - S(4));
    }

    public override Size GetPreferredSize(Size proposedSize) =>
        new(Options.Length == 0 ? S(120) : ItemBounds(Options.Length - 1).Right + S(2), S(M.Height(size)));

    protected override void OnOptionsChanged() => Width = GetPreferredSize(Size.Empty).Width;

    int HitTest(Point pt) {
      for (var i = 0; i < Options.Length; i++) if (ItemBounds(i).Contains(pt)) return i;
      return -1;
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      var h = HitTest(e.Location);
      if (h != hover) {
        // An option's hover eases in and out (key: its index + 1).
        if (hover >= 0) Fade.Set(hover + 1, false);
        if (h >= 0) Fade.Set(h + 1, true);
        hover = h;
        Invalidate();
      }
      base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hover = -1; base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e) {
      var i = HitTest(e.Location);
      if (i >= 0 && Enabled) SelectedIndex = i;
      Focus();
      base.OnMouseClick(e);
    }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e) {
      if (Options.Length > 0 && e.KeyCode == Keys.Right) { SelectedIndex = Math.Min(Options.Length - 1, SelectedIndex + 1); e.Handled = true; } else if (Options.Length > 0 && e.KeyCode == Keys.Left) { SelectedIndex = Math.Max(0, SelectedIndex - 1); e.Handled = true; }
      base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      float radius = S(M.RadiusControl);
      var whole = new RectangleF(0, 0, Width, Height);
      Painter.Fill(g, P.BgSubtle, whole, radius);
      for (var i = 0; i < Options.Length; i++) {
        var r = ItemBounds(i);
        var on = i == SelectedIndex;
        if (on) {
          Painter.Fill(g, P.Dark ? P.BgPress : P.Bg, r, radius - S(2));
          if (!P.Dark) Painter.Outline(g, P.Line, r, radius - S(2));
        } else if (Enabled && Fade[i + 1] > 0) Painter.Fill(g, Fade.Mix(i + 1, Color.Empty, P.BgHover), r, radius - S(2));
        var ink = !Enabled ? P.InkDisabled : on ? P.Ink : P.Ink2;
        var hasIcon = i < icons.Length && icons[i].Length > 0;
        if (iconOnly && hasIcon) {
          Icons.Draw(g, icons[i], new RectangleF(r.X + (r.Width - S(16)) / 2f, r.Y + (r.Height - S(16)) / 2f, S(16), S(16)), ink);
          continue;
        }
        var x = r.X + S(12);
        if (hasIcon) {
          Icons.Draw(g, icons[i], new RectangleF(x, r.Y + (r.Height - S(16)) / 2f, S(16), S(16)), ink);
          x += S(22);
        }
        Painter.Text(g, Options[i], TextFont, new Rectangle(x, r.Y, r.Right - x, r.Height), ink);
      }
      if (ShowFocus) Painter.FocusRing(g, whole, radius, S(2));
    }
  }

  // A value along a range: drag, click the track, or use the
  // arrows, Page Up and Down, Home and End. With ShowValue, the value sits
  // in a box at the right end, with its unit.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(ValueChanged)), DefaultProperty(nameof(Value))]
  [Description("A value along a range, with an optional readout of it at the right end.")]
  public class Slider : KitControl {
    double min, max = 100, step = 1, value;
    bool showValue, dragging;
    string unit = "";

    public Slider() {
      SetStyle(ControlStyles.Selectable, true);
      TabStop = true;
    }

    [Category("Data"), DefaultValue(0d)]
    public double Minimum { get => min; set { min = value; Value = this.value; Invalidate(); } }

    [Category("Data"), DefaultValue(100d)]
    public double Maximum { get => max; set { max = value; Value = this.value; Invalidate(); } }

    [Category("Data"), DefaultValue(1d)]
    [Description("The distance between the values the thumb stops at; 0 lets it rest anywhere.")]
    public double Step { get => step; set => step = Math.Max(0, value); }

    [Category("Data"), DefaultValue(0d)]
    public double Value {
      get => value;
      set {
        var v = Math.Clamp(value, Math.Min(min, max), Math.Max(min, max));
        if (step > 0) v = min + Math.Round((v - min) / step) * step;
        if (v == this.value) return;
        this.value = v;
        Invalidate();
        ValueChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    [Category("Appearance"), DefaultValue(false)]
    [Description("Shows the value in a box at the right end.")]
    public bool ShowValue { get => showValue; set { showValue = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("A unit after the value in the box: dB, %, px.")]
    public string Unit { get => unit; set { unit = value ?? ""; Invalidate(); } }

    public event EventHandler? ValueChanged;

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) =>
        base.SetBoundsCore(x, y, width, S(M.HeightMedium), specified);

    string Readout => (step >= 1 || step == 0 ? value.ToString("0.##") : value.ToString("0.###")) + (unit.Length > 0 ? " " + unit : "");
    int ReadoutWidth => showValue ? Painter.Measure((max >= 1000 ? "0000.0" : "000.0") + (unit.Length > 0 ? " " + unit : ""), KitTheme.Font(13)).Width + S(16) : 0;
    RectangleF Track => new(S(8), Height / 2f - S(2), Width - S(16) - (showValue ? ReadoutWidth + S(12) : 0), S(4));
    float Fraction => max == min ? 0 : (float)((value - min) / (max - min));

    void SetFromX(int x) {
      var t = Track;
      Value = min + Math.Clamp((x - t.X) / t.Width, 0, 1) * (max - min);
    }

    protected override void OnMouseDown(MouseEventArgs e) {
      base.OnMouseDown(e);
      Focus();
      if (e.Button != MouseButtons.Left || !Enabled || e.X > Track.Right + S(8)) return;
      dragging = true;
      Capture = true;
      SetFromX(e.X);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      base.OnMouseMove(e);
      if (dragging) SetFromX(e.X);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
      dragging = false;
      Capture = false;
      base.OnMouseUp(e);
    }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down or Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e) {
      var unitStep = step > 0 ? step : (max - min) / 100;
      var page = (max - min) / 10;
      switch (e.KeyCode) {
        case Keys.Right or Keys.Up: Value = value + unitStep; break;
        case Keys.Left or Keys.Down: Value = value - unitStep; break;
        case Keys.PageUp: Value = value + page; break;
        case Keys.PageDown: Value = value - page; break;
        case Keys.Home: Value = min; break;
        case Keys.End: Value = max; break;
        default: base.OnKeyDown(e); return;
      }
      e.Handled = true;
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var t = Track;
      Painter.Fill(g, Enabled ? P.Line : P.BgSubtle, t, t.Height / 2);
      var filled = new RectangleF(t.X, t.Y, t.Width * Fraction, t.Height);
      Painter.Fill(g, Enabled ? P.Inverse : P.InkDisabled, filled, t.Height / 2);
      var d = S(14);
      var cx = t.X + t.Width * Fraction;
      var thumb = new RectangleF(cx - d / 2f, Height / 2f - d / 2f, d, d);
      using (var b = new SolidBrush(P.Bg)) g.FillEllipse(b, thumb);
      using (var pen = new Pen(Enabled ? P.Inverse : P.InkDisabled, S(2))) g.DrawEllipse(pen, RectangleF.Inflate(thumb, -S(1), -S(1)));
      if (ShowFocus)
        using (var focus = new Pen(P.AccentInk, S(2))) g.DrawEllipse(focus, RectangleF.Inflate(thumb, S(3), S(3)));
      if (showValue) {
        var box = new RectangleF(Width - ReadoutWidth, (Height - S(28)) / 2f, ReadoutWidth, S(28));
        if (HoverT > 0) Painter.Outline(g, Motion.Lerp(Color.Empty, P.LineStrong, HoverT), box, S(M.RadiusControl));
        Painter.Text(g, Readout, KitTheme.Font(13), Rectangle.Round(box), P.Ink, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
      }
    }
  }
}
