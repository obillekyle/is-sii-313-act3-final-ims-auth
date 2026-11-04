using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Kit {
  // A status is a word in a shape; color is the second channel,
  // never the only one. A status tone draws a dot before the word.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Text))]
  [Description("A status word in a pill, with a dot in its tone's color.")]
  public class Badge : KitControl {
    Tone tone = Tone.Neutral;
    bool plain;

    public Badge() => AutoSize = true;

    [DefaultValue(true)]
    public override bool AutoSize { get => base.AutoSize; set => base.AutoSize = value; }

    [Category("Appearance"), DefaultValue(Tone.Neutral)]
    [Description("Sets the fill and the text color; the word carries the meaning.")]
    public Tone Tone { get => tone; set { tone = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue(false)]
    [Description("Leaves out the dot a status tone draws.")]
    public bool Plain { get => plain; set { plain = value; Refit(); } }

    bool HasDot => !plain && tone is Tone.Info or Tone.Success or Tone.Warning or Tone.Danger;
    Font TextFont => KitTheme.Font(12, true);

    void Refit() {
      if (AutoSize) Size = GetPreferredSize(Size.Empty);
      Invalidate();
    }

    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Refit(); }

    public override Size GetPreferredSize(Size proposedSize) =>
        new(S(16) + (HasDot ? S(12) : 0) + Painter.Measure(Text, TextFont).Width + 1, S(20));

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var (ink, soft) = P.ToneColors(tone);
      var r = new RectangleF(0, (Height - S(20)) / 2f, Width, S(20));
      Painter.Fill(g, soft, r, r.Height / 2);
      var x = S(8);
      if (HasDot) {
        using var b = new SolidBrush(ink);
        g.FillEllipse(b, x, r.Y + (r.Height - S(6)) / 2f, S(6), S(6));
        x += S(12);
      }
      Painter.Text(g, Text, TextFont, new Rectangle(x, (int)r.Y, Width - x, (int)r.Height), ink);
    }
  }

  // An inline message that stays until its cause is gone: an
  // icon in its tone's color, a bold first line that says the state, the
  // body under it, and an optional Dismiss button.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Text)), DefaultEvent(nameof(Dismissed))]
  [Description("An inline message: an icon, a bold title, the body, and an optional dismiss button.")]
  public class Alert : KitControl {
    Tone tone = Tone.Info;
    string title = "";
    bool dismissable, overClose;

    [Category("Appearance"), DefaultValue(Tone.Info)]
    [Description("Sets the color and the icon.")]
    public Tone Tone { get => tone; set { tone = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("The bold first line, which says the state.")]
    public string Title { get => title; set { title = value ?? ""; FitHeight(); } }

    [Category("Behavior"), DefaultValue(false)]
    [Description("Adds a Dismiss button at the end.")]
    public bool Dismissable { get => dismissable; set { dismissable = value; FitHeight(); } }

    public event EventHandler? Dismissed;

    static string IconFor(Tone t) => t switch {
      Tone.Success => "circle-check",
      Tone.Warning => "triangle-alert",
      Tone.Danger => "circle-alert",
      _ => "info",
    };

    (Color Bg, Color Fg, Color Line) Colors() {
      if (tone == Tone.Neutral) return (P.BgSubtle, P.Ink2, P.Line);
      var (ink, soft) = P.ToneColors(tone);
      var hue = tone switch { Tone.Success => P.Ok, Tone.Warning => P.Warn, Tone.Danger => P.Bad, _ => P.Accent };
      return (soft, ink, Palette.Mix(hue, 0.3, soft));
    }

    int TextLeft => S(14) + S(18) + S(10);
    int TextWidth => Math.Max(10, Width - TextLeft - S(12) - (dismissable ? S(32) : 0));

    void FitHeight() {
      var h = S(12) * 2;
      if (title.Length > 0) h += Painter.MeasureHeight(title, KitTheme.Font(14, true), TextWidth) + S(2);
      h += Painter.MeasureHeight(Text, KitTheme.Font(14), TextWidth);
      h = Math.Max(h, S(44));
      if (Height != h) Height = h;
      Invalidate();
    }

    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); FitHeight(); }
    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); FitHeight(); }
    protected override void OnThemeChanged() { FitHeight(); base.OnThemeChanged(); }

    Rectangle CloseBox => new(Width - S(12) - S(28), S(8), S(28), S(28));

    protected override void OnMouseMove(MouseEventArgs e) {
      var over = dismissable && CloseBox.Contains(e.Location);
      if (over != overClose) { overClose = over; Invalidate(); }
      base.OnMouseMove(e);
    }

    protected override void OnMouseClick(MouseEventArgs e) {
      if (dismissable && CloseBox.Contains(e.Location)) Dismissed?.Invoke(this, EventArgs.Empty);
      base.OnMouseClick(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var (bg, fg, line) = Colors();
      var r = new RectangleF(0, 0, Width, Height);
      Painter.Fill(g, bg, r, S(M.RadiusPanel));
      Painter.Outline(g, line, r, S(M.RadiusPanel));
      Icons.Draw(g, IconFor(tone), new RectangleF(S(14), S(13), S(18), S(18)), fg);
      var y = S(12);
      if (title.Length > 0) {
        var th = Painter.MeasureHeight(title, KitTheme.Font(14, true), TextWidth);
        Painter.Wrapped(g, title, KitTheme.Font(14, true), new Rectangle(TextLeft, y, TextWidth, th), P.Ink);
        y += th + S(2);
      }
      Painter.Wrapped(g, Text, KitTheme.Font(14), new Rectangle(TextLeft, y, TextWidth, Height - y), title.Length > 0 ? P.Ink2 : P.Ink);
      if (dismissable) {
        if (overClose) Painter.Fill(g, P.BgHover, CloseBox, S(M.RadiusControl));
        Icons.Draw(g, "x", RectangleF.Inflate(CloseBox, -S(6), -S(6)), P.Ink2);
      }
    }
  }

  // A person's photo, or their initials on a color picked from their name.
  // As wide as it is tall; the initials are 40% of it.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(PersonName))]
  [Description("A person's photo, or their initials on a color picked from their name.")]
  public class Avatar : KitControl {
    string personName = "";
    Image? image;

    public Avatar() => Size = new Size(32, 32);

    [Category("Appearance"), DefaultValue("")]
    [Description("The person's name: the initials come from its first two words, and the color from all of it.")]
    public string PersonName { get => personName; set { personName = value ?? ""; AccessibleName = personName; Invalidate(); } }

    [Category("Appearance"), DefaultValue(null)]
    [Description("The photo. Without one, the initials show.")]
    public Image? Image { get => image; set { image = value; Invalidate(); } }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
      var side = specified.HasFlag(BoundsSpecified.Width) && !specified.HasFlag(BoundsSpecified.Height) ? width : height;
      base.SetBoundsCore(x, y, side, side, specified);
    }

    string Initials => string.Concat(personName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => char.ToUpperInvariant(w[0])));

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var r = new RectangleF(0, 0, Width - 1, Height - 1);
      using var circle = new GraphicsPath();
      circle.AddEllipse(r);
      if (image != null) {
        var state = g.Save();
        g.SetClip(circle);
        g.DrawImage(image, r);
        g.Restore(state);
        return;
      }
      // A hue from the name (h * 31 + code, kept to 32
      // bits), at a lightness and chroma that keep white text readable
      // on every hue: oklch(0.52 0.13 hue).
      uint hash = 0;
      foreach (var ch in personName) hash = unchecked(hash * 31 + ch);
      var fill = Oklch(0.52, 0.13, hash % 360);
      using (var b = new SolidBrush(fill)) g.FillPath(b, circle);
      var font = KitTheme.Font(Height / (float)DeviceDpi * 96f * 0.4f, true);
      Painter.Text(g, Initials, font, Rectangle.Round(r), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    // OKLCH to sRGB (Björn Ottosson's OKLab matrices).
    static Color Oklch(double l, double c, double hueDeg) {
      var h = hueDeg * Math.PI / 180;
      double a = c * Math.Cos(h), bb = c * Math.Sin(h);
      var l_ = l + 0.3963377774 * a + 0.2158037573 * bb;
      var m_ = l - 0.1055613458 * a - 0.0638541728 * bb;
      var s_ = l - 0.0894841775 * a - 1.2914855480 * bb;
      double L3 = l_ * l_ * l_, M3 = m_ * m_ * m_, S3 = s_ * s_ * s_;
      var r = 4.0767416621 * L3 - 3.3077115913 * M3 + 0.2309699292 * S3;
      var g = -1.2684380046 * L3 + 2.6097574011 * M3 - 0.3413193965 * S3;
      var b = -0.0041960863 * L3 - 0.7034186147 * M3 + 1.7076147010 * S3;
      static int Gamma(double x) {
        x = Math.Clamp(x, 0, 1);
        var v = x <= 0.0031308 ? 12.92 * x : 1.055 * Math.Pow(x, 1 / 2.4) - 0.055;
        return (int)Math.Round(v * 255);
      }
      return Color.FromArgb(Gamma(r), Gamma(g), Gamma(b));
    }
  }

  // One icon from the kit's set, by name, as large as the control.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(IconName))]
  [Description("One icon from the kit's set, by name, as large as the control.")]
  public class KitIcon : KitControl {
    string iconName = "circle-question-mark";
    Color color = Color.Empty;

    public KitIcon() => Size = new Size(16, 16);

    [Category("Appearance"), DefaultValue("circle-question-mark"), IconName, TypeConverter(typeof(IconNameConverter))]
    [Description("The icon's Lucide name (chevron-right, trash-2).")]
    public string IconName { get => iconName; set { iconName = value ?? ""; Invalidate(); } }

    [Category("Appearance")]
    [Description("The icon's color; the theme's text color when empty.")]
    public Color IconColor { get => color; set { color = value; Invalidate(); } }
    bool ShouldSerializeIconColor() => !color.IsEmpty;
    void ResetIconColor() => IconColor = Color.Empty;

    protected override void OnPaint(PaintEventArgs e) =>
        Icons.Draw(e.Graphics, iconName, new RectangleF(0, 0, Width, Height), color.IsEmpty ? (Enabled ? P.Ink : P.InkDisabled) : color);
  }

  // A keyboard key, or a chord when given several, each in its
  // own box: Ctrl, K.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Keys))]
  [Description("A keyboard key or a chord, each key in a box of its own.")]
  public class Kbd : KitControl {
    string[] keys = { "Ctrl", "K" };

    public Kbd() => AutoSize = true;

    [DefaultValue(true)]
    public override bool AutoSize { get => base.AutoSize; set => base.AutoSize = value; }

    [Category("Appearance")]
    [Description("The keys, in order: one key, or the keys of a chord pressed together.")]
    public string[] Keys { get => keys; set { keys = value ?? Array.Empty<string>(); if (AutoSize) Size = GetPreferredSize(Size.Empty); Invalidate(); } }
    bool ShouldSerializeKeys() => !keys.SequenceEqual(new[] { "Ctrl", "K" });
    void ResetKeys() => Keys = new[] { "Ctrl", "K" };

    Font KeyFont => KitTheme.Font(12);
    int KeyWidth(string k) => Math.Max(S(20), Painter.Measure(k, KeyFont).Width + S(10));

    public override Size GetPreferredSize(Size proposedSize) =>
        new(keys.Sum(KeyWidth) + Math.Max(0, keys.Length - 1) * S(4) + 1, S(22));

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var x = 0f;
      foreach (var k in keys) {
        var r = new RectangleF(x, (Height - S(20)) / 2f, KeyWidth(k), S(20));
        Painter.Fill(g, P.BgSubtle, r, S(M.RadiusTag));
        Painter.Outline(g, P.Line, r, S(M.RadiusTag));
        using (var pen = new Pen(P.Line, S(2))) g.DrawLine(pen, r.X + S(2), r.Bottom - S(1), r.Right - S(2), r.Bottom - S(1));
        var label = Rectangle.Round(RectangleF.Inflate(r, -S(2), -S(3)));
        Painter.TextOn(g, k, KeyFont, label, P.Ink2, P.BgSubtle);
        x = r.Right + S(4);
      }
    }
  }

  // Placeholder lines in the shape of what is loading, so
  // the layout does not jump when it arrives. With more than one line, the
  // last is 60% wide, as a paragraph's is.
  [ToolboxItem(true)]
  [Description("Placeholder lines in the shape of what is loading.")]
  public class Skeleton : KitControl {
    int lines = 1, lineHeight = 12;
    bool circle;
    readonly System.Windows.Forms.Timer pulse = new() { Interval = 50 };
    float phase;

    public Skeleton() {
      pulse.Tick += (_, _) => { phase = (phase + 0.04f) % 2f; Invalidate(); };
      pulse.Start();
    }

    [Category("Appearance"), DefaultValue(1)]
    public int Lines { get => lines; set { lines = Math.Max(1, value); Invalidate(); } }

    [Category("Appearance"), DefaultValue(12)]
    [Description("Each line's height in px.")]
    public int LineHeight { get => lineHeight; set { lineHeight = Math.Max(4, value); Invalidate(); } }

    [Category("Appearance"), DefaultValue(false)]
    [Description("Draws a circle in place of the lines, for an avatar.")]
    public bool Circle { get => circle; set { circle = value; Invalidate(); } }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var t = phase < 1 ? phase : 2 - phase;
      var fill = Palette.Mix(P.BgHover, 0.6 + 0.4 * t, P.BgSubtle);
      if (circle) {
        var d = Math.Min(Width, Height) - 1;
        using var b = new SolidBrush(fill);
        g.FillEllipse(b, 0, 0, d, d);
        return;
      }
      var h = S(lineHeight);
      var gap = S(8);
      for (var i = 0; i < lines; i++) {
        var w = lines > 1 && i == lines - 1 ? Width * 0.6f : Width;
        Painter.Fill(g, fill, new RectangleF(0, i * (h + gap), w, h), S(M.RadiusTag));
      }
    }

    protected override void Dispose(bool disposing) {
      if (disposing) pulse.Dispose();
      base.Dispose(disposing);
    }
  }

  // What a list shows with nothing in it: an icon in a
  // ring, why it is empty in one bold line, a sentence, and the one action
  // that fills it.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Title)), DefaultEvent(nameof(ActionClick))]
  [Description("An empty list's message: an icon in a ring, a bold line, a sentence and one action.")]
  public class EmptyState : KitControl {
    string icon = "inbox", title = "", actionText = "";
    bool compact;
    readonly KitButton action = new() { Visible = false, AutoSize = true };

    public EmptyState() {
      action.Click += (_, _) => ActionClick?.Invoke(this, EventArgs.Empty);
      Controls.Add(action);
      Size = new Size(360, 220);
    }

    [Category("Appearance"), DefaultValue("inbox"), IconName, TypeConverter(typeof(IconNameConverter))]
    public string Icon { get => icon; set { icon = value ?? ""; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("The bold line that says why the list is empty.")]
    public string Title { get => title; set { title = value ?? ""; PerformLayout(); Invalidate(); } }

    [Category("Appearance"), DefaultValue(false)]
    [Description("Halves the padding, for an empty list inside a section.")]
    public bool Compact { get => compact; set { compact = value; PerformLayout(); Invalidate(); } }

    [Category("Behavior"), DefaultValue("")]
    [Description("The one action that fills the list. Empty for none.")]
    public string ActionText { get => actionText; set { actionText = value ?? ""; action.Text = actionText; action.Visible = actionText.Length > 0; PerformLayout(); } }

    public event EventHandler? ActionClick;

    int Pad => S(compact ? 20 : 40);
    int TextWidth => Math.Min(Width - S(32), S(420));

    int ContentHeight() {
      var h = S(56) + S(16);
      if (title.Length > 0) h += Painter.MeasureHeight(title, KitTheme.Font(16, true), TextWidth) + S(4);
      if (Text.Length > 0) h += Painter.MeasureHeight(Text, KitTheme.Font(14), TextWidth);
      if (actionText.Length > 0) h += S(16) + action.Height;
      return h;
    }

    public override Size GetPreferredSize(Size proposedSize) => new(Width, ContentHeight() + Pad * 2);

    protected override void OnLayout(LayoutEventArgs e) {
      base.OnLayout(e);
      if (!action.Visible) return;
      action.Location = new Point((Width - action.Width) / 2, Math.Max(0, (Height - ContentHeight()) / 2) + ContentHeight() - action.Height);
    }

    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); PerformLayout(); }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var y = Math.Max(0, (Height - ContentHeight()) / 2);
      var ring = new RectangleF((Width - S(56)) / 2f, y, S(56), S(56));
      using (var b = new SolidBrush(P.BgSubtle)) g.FillEllipse(b, ring);
      Icons.Draw(g, icon, RectangleF.Inflate(ring, -S(16), -S(16)), P.Ink2);
      y += S(56) + S(16);
      var x = (Width - TextWidth) / 2;
      if (title.Length > 0) {
        var th = Painter.MeasureHeight(title, KitTheme.Font(16, true), TextWidth);
        Painter.Wrapped(g, title, KitTheme.Font(16, true), new Rectangle(x, y, TextWidth, th), P.Ink, TextFormatFlags.HorizontalCenter);
        y += th + S(4);
      }
      if (Text.Length > 0)
        Painter.Wrapped(g, Text, KitTheme.Font(14), new Rectangle(x, y, TextWidth, Height - y), P.Ink2, TextFormatFlags.HorizontalCenter);
    }
  }

  // How far along a known amount of work is: a bar, the
  // percent and the time left under it ("64% • About 40 s left"); with no
  // value, a short bar runs across the track to say work is happening.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Value))]
  [Description("A progress bar with the percent and the time left under it.")]
  public class Progress : KitControl {
    double? value;
    double max = 100;
    string label = "", detail = "";
    int? remaining;
    readonly System.Windows.Forms.Timer run = new() { Interval = 30 };
    float runAt;

    public Progress() {
      run.Tick += (_, _) => { runAt = (runAt + 0.015f) % 1.3f; Invalidate(); };
      Size = new Size(320, 52);
    }

    [Category("Data"), DefaultValue(null)]
    [Description("How much is done, in the units of Maximum. Empty for work of unknown length.")]
    public double? Value { get => value; set { this.value = value; if (value == null) run.Start(); else run.Stop(); Invalidate(); } }

    [Category("Data"), DefaultValue(100d)]
    public double Maximum { get => max; set { max = value <= 0 ? 100 : value; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("Names the work, above the bar.")]
    public string Label { get => label; set { label = value ?? ""; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("The text under the bar in place of the percent, as in 12 of 40 files.")]
    public string Detail { get => detail; set { detail = value ?? ""; Invalidate(); } }

    [Category("Data"), DefaultValue(null)]
    [Description("The seconds the work has left, shown after the percent.")]
    public int? Remaining { get => remaining; set { remaining = value; Invalidate(); } }

    static string TimeLeft(int s) => s < 60 ? $"About {s} s left" : $"About {Math.Round(s / 60.0)} min left";

    protected override void OnHandleCreated(EventArgs e) {
      base.OnHandleCreated(e);
      if (value == null) run.Start();
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var y = 0;
      if (label.Length > 0) {
        Painter.Text(g, label, KitTheme.Font(14, true), new Rectangle(0, 0, Width, S(20)), P.Ink);
        y += S(24);
      }
      var track = new RectangleF(0, y, Width, S(4));
      Painter.Fill(g, P.BgHover, track, track.Height / 2);
      if (value is { } v) {
        var f = (float)Math.Clamp(v / max, 0, 1);
        Painter.Fill(g, P.Inverse, new RectangleF(0, y, Width * f, track.Height), track.Height / 2);
        var words = detail.Length > 0 ? detail : $"{Math.Round(f * 100)}%";
        if (remaining is { } r) words += " • " + TimeLeft(r);
        Painter.Text(g, words, KitTheme.Font(13), new Rectangle(0, y + S(10), Width, S(18)), P.Ink2);
      } else {
        var state = g.Save();
        using (var clip = Painter.Round(track, track.Height / 2)) g.SetClip(clip);
        var w = Width * 0.3f;
        Painter.Fill(g, P.Inverse, new RectangleF(runAt * Width - w, y, w, track.Height), track.Height / 2);
        g.Restore(state);
        if (detail.Length > 0) Painter.Text(g, detail, KitTheme.Font(13), new Rectangle(0, y + S(10), Width, S(18)), P.Ink2);
      }
    }

    protected override void Dispose(bool disposing) {
      if (disposing) run.Dispose();
      base.Dispose(disposing);
    }
  }

  // One amount against a limit: a teacher's load, a room's
  // seats. The bar turns the warning color near the limit and the danger
  // color past it, with words that say so.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Value))]
  [Description("One amount against a limit, turning amber near it and red past it.")]
  public class Meter : KitControl {
    double value, max = 100, warnAt = 0.85, badAt = 1;
    string label = "", unit = "", warnText = "Near the limit", badText = "Over the limit";

    public Meter() => Size = new Size(320, 52);

    [Category("Appearance"), DefaultValue("")] public string Label { get => label; set { label = value ?? ""; Invalidate(); } }
    [Category("Data"), DefaultValue(0d)] public double Value { get => value; set { this.value = value; Invalidate(); } }
    [Category("Data"), DefaultValue(100d)] public double Maximum { get => max; set { max = value <= 0 ? 100 : value; Invalidate(); } }
    [Category("Appearance"), DefaultValue("")] public string Unit { get => unit; set { unit = value ?? ""; Invalidate(); } }

    [Category("Data"), DefaultValue(0.85)]
    [Description("The fraction of Maximum, 0 to 1, from which the bar turns the warning color.")]
    public double WarnAt { get => warnAt; set { warnAt = value; Invalidate(); } }

    [Category("Data"), DefaultValue(1d)]
    [Description("The fraction of Maximum past which the bar turns the danger color.")]
    public double BadAt { get => badAt; set { badAt = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue("Near the limit")] public string WarnText { get => warnText; set { warnText = value ?? ""; Invalidate(); } }
    [Category("Appearance"), DefaultValue("Over the limit")] public string BadText { get => badText; set { badText = value ?? ""; Invalidate(); } }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var f = value / max;
      var bad = f > badAt;
      var warn = !bad && f >= warnAt;
      var y = 0;
      if (label.Length > 0) {
        Painter.Text(g, label, KitTheme.Font(14, true), new Rectangle(0, 0, Width, S(20)), P.Ink);
        y += S(24);
      }
      var track = new RectangleF(0, y, Width, S(6));
      Painter.Fill(g, P.BgHover, track, track.Height / 2);
      Painter.Fill(g, bad ? P.Bad : warn ? P.Warn : P.Inverse, new RectangleF(0, y, Width * (float)Math.Clamp(f, 0, 1), track.Height), track.Height / 2);
      var amount = $"{value:0.##} of {max:0.##}" + (unit.Length > 0 ? " " + unit : "");
      var note = bad ? badText : warn ? warnText : "";
      Painter.Text(g, amount, KitTheme.Font(13), new Rectangle(0, y + S(10), Width, S(18)), P.Ink2);
      if (note.Length > 0)
        Painter.Text(g, note, KitTheme.Font(13, true), new Rectangle(0, y + S(10), Width, S(18)), bad ? P.Bad : P.Warn, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }
  }

  // A trend in a line's worth of space: the past in gray,
  // the latest point in the accent.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Values))]
  [Description("A trend in a line's worth of space, the latest point in the accent.")]
  public class Sparkline : KitControl {
    float[] values = { 3, 4, 3.5f, 5, 4.6f, 6, 5.4f };

    public Sparkline() => Size = new Size(96, 28);

    [Category("Data")]
    [Description("The points, oldest first, spread across the width.")]
    public float[] Values { get => values; set { values = value ?? Array.Empty<float>(); Invalidate(); } }
    bool ShouldSerializeValues() => values.Length > 0;
    void ResetValues() => Values = Array.Empty<float>();

    internal static void DrawLine(Graphics g, float[] values, RectangleF box, Color line, Color last, float width) {
      if (values.Length < 2) return;
      float min = values.Min(), max = values.Max(), span = Math.Max(1e-6f, max - min);
      var pts = values.Select((v, i) => new PointF(box.X + i * box.Width / (values.Length - 1), box.Bottom - (v - min) / span * box.Height)).ToArray();
      using var pen = new Pen(line, width) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
      g.DrawLines(pen, pts);
      using var dot = new SolidBrush(last);
      var p = pts[^1];
      g.FillEllipse(dot, p.X - width * 1.5f, p.Y - width * 1.5f, width * 3, width * 3);
    }

    protected override void OnPaint(PaintEventArgs e) {
      Painter.Smooth(e.Graphics);
      DrawLine(e.Graphics, values, new RectangleF(S(3), S(3), Width - S(6), Height - S(6)), P.Ink3, P.AccentInk, S(1.5f));
    }
  }

  // One figure with what it is compared against: its label
  // above it, the change over a period colored by whether it is good news,
  // and a sparkline of the last values beside it.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Value))]
  [Description("A figure with its label, its change over a period and a sparkline of its trend.")]
  public class StatTile : KitControl, IKitBleed {
    // The card's shadow room around its body, which Flex and
    // Grid lay outside the cell so the card itself lines up.
    [Browsable(false)]
    public Padding Bleed => new(S(6), S(4), S(6), S(8));

    string label = "", value = "", unit = "", period = "";
    double? change;
    bool upIsGood = true, percent;
    float[] trend = Array.Empty<float>();

    public StatTile() => Size = new Size(240, 112);

    [Category("Appearance"), DefaultValue("")] public string Label { get => label; set { label = value ?? ""; Invalidate(); } }
    [Category("Data"), DefaultValue("")]
    [Description("The figure, as it should read (2.7M, 1,069, 98%).")]
    public string Value { get => value; set { this.value = value ?? ""; Invalidate(); } }
    [Category("Appearance"), DefaultValue("")] public string Unit { get => unit; set { unit = value ?? ""; Invalidate(); } }

    [Category("Data"), DefaultValue(null)]
    [Description("The change against the period, signed, in the figure's unit.")]
    public double? Change { get => change; set { change = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("The period the change is measured against (vs last week).")]
    public string Period { get => period; set { period = value ?? ""; Invalidate(); } }

    [Category("Behavior"), DefaultValue(true)]
    [Description("Whether a rise is good news, which colors the change. Off for absences, overdue items, low stock.")]
    public bool UpIsGood { get => upIsGood; set { upIsGood = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue(false)]
    [Description("Shows the change as a percent rather than an amount.")]
    public bool Percent { get => percent; set { percent = value; Invalidate(); } }

    [Category("Data")]
    [Description("Recent values, oldest first, drawn as a sparkline beside the figure.")]
    public float[] Trend { get => trend; set { trend = value ?? Array.Empty<float>(); Invalidate(); } }
    bool ShouldSerializeTrend() => trend.Length > 0;
    void ResetTrend() => Trend = Array.Empty<float>();

    // A tile that opens something shows a hand, a stronger outline on hover,
    // a focus stop, and Enter or Space raising Click.
    bool clickable;

    [Category("Behavior"), DefaultValue(false)]
    [Description("Opens what is behind the figure (the list it counts) on a click, Enter or Space.")]
    public bool Clickable {
      get => clickable;
      set {
        clickable = value;
        SetStyle(ControlStyles.Selectable, value);
        TabStop = value;
        Cursor = value ? Cursors.Hand : Cursors.Default;
        AccessibleRole = value ? AccessibleRole.Link : AccessibleRole.Default;
        Invalidate();
      }
    }

    protected override void OnMouseDown(MouseEventArgs e) {
      if (clickable) Focus();
      base.OnMouseDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
      if (clickable && e.KeyCode is Keys.Enter or Keys.Space) {
        OnClick(EventArgs.Empty);
        e.Handled = true;
      }
      base.OnKeyDown(e);
    }

    // English wants another unit for exactly one ("Down 1 tools" otherwise),
    // so the tile takes both.
    string unitOne = "";

    [Category("Appearance"), DefaultValue("")]
    [Description("The unit for exactly one (tool where Unit is tools), for the figure and the change. Empty uses Unit.")]
    public string UnitOne { get => unitOne; set { unitOne = value ?? ""; Invalidate(); } }

    string UnitFor(bool one) => one && unitOne.Length > 0 ? unitOne : unit;

    string ChangeText() {
      if (change is not { } c) return "";
      var u = UnitFor(Math.Abs(c) == 1);
      var amount = percent ? $"{Math.Abs(c):0.#}%" : $"{Math.Abs(c):0.##}" + (u.Length > 0 ? " " + u : "");
      var word = c > 0 ? "Up" : c < 0 ? "Down" : "No change";
      var text = c == 0 ? word : $"{word} {amount}";
      return period.Length > 0 ? $"{text} {period}" : text;
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var body = new RectangleF(S(6), S(4), Width - S(12), Height - S(12));
      Painter.Card(g, body, S(M.RadiusPanel), S(1f));
      if (clickable && HoverT > 0) Painter.Outline(g, Motion.Lerp(Color.Empty, P.LineStrong, HoverT), body, S(M.RadiusPanel));
      if (clickable && ShowFocus) Painter.FocusRing(g, body, S(M.RadiusPanel), S(2));
      // A chevron at the top right says the tile opens something, before
      // a pointer is over it.
      if (clickable)
        Icons.Draw(g, "chevron-right", new RectangleF(body.Right - S(16) - S(16), body.Y + S(14), S(16), S(16)), Motion.Lerp(P.Ink3, P.Ink, HoverT));
      var inner = Rectangle.Round(RectangleF.Inflate(body, -S(16), -S(14)));
      var labelH = KitTheme.Font(13).Height;
      Painter.Text(g, label, KitTheme.Font(13), new Rectangle(inner.X, inner.Y, inner.Width, labelH), P.Ink2);
      var vy = inner.Y + labelH + S(4);
      var big = KitTheme.Font(24, true);
      var vw = Painter.Measure(value, big).Width;
      Painter.Text(g, value, big, new Rectangle(inner.X, vy, vw + 1, big.Height), P.Ink);
      var valueUnit = UnitFor(value == "1");
      if (valueUnit.Length > 0)
        Painter.Text(g, valueUnit, KitTheme.Font(14), new Rectangle(inner.X + vw + S(4), vy, inner.Width, big.Height), P.Ink2);
      if (change is { } c) {
        var good = c == 0 ? (bool?)null : (c > 0) == upIsGood;
        var color = good == null ? P.Ink2 : good.Value ? P.Ok : P.Bad;
        var font = KitTheme.Font(13);
        Painter.Text(g, ChangeText(), font, new Rectangle(inner.X, inner.Bottom - font.Height, inner.Width, font.Height), color);
      }
      if (trend.Length > 1) {
        var sw = Math.Min(S(96), inner.Width - vw - S(24));
        if (sw > S(40))
          // Its end dot's radius inside the line's box, so the dot's
          // edge, not its center, lands on the 16 px inset.
          Sparkline.DrawLine(g, trend, new RectangleF(inner.Right - sw, vy + S(6), sw - S(3), big.Height - S(12)), P.Ink3, P.AccentInk, S(1.5f));
      }
    }
  }
}
