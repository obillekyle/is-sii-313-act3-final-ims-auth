using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Kit {
  // What the two charts share: a title and a note above,
  // a legend of the series, one y axis from clean ticks with hairline grid
  // lines, the labels along the bottom thinned to what fits, and a readout
  // of one label's values on hover or under the arrow keys.
  [ToolboxItem(false)]
  public abstract class KitChart : KitControl {
    readonly ItemList<ChartSeries> series;
    string title = "", note = "", unit = "";
    string[] labels = Array.Empty<string>();
    protected int Hot = -1;

    protected KitChart() {
      series = new ItemList<ChartSeries>(Invalidate);
      SetStyle(ControlStyles.Selectable, true);
      TabStop = true;
      Size = new Size(480, 240);
    }

    [Category("Appearance"), DefaultValue("")]
    [Description("Says what is plotted, above the chart.")]
    public string Title { get => title; set { title = value ?? ""; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("A secondary line under the title: the period, the scope, or what is missing.")]
    public string Note { get => note; set { note = value ?? ""; Invalidate(); } }

    [Category("Data")]
    [Description("The labels along the bottom, one per point or bar, left to right.")]
    public string[] Labels { get => labels; set { labels = value ?? Array.Empty<string>(); Invalidate(); } }
    bool ShouldSerializeLabels() => labels.Length > 0;
    void ResetLabels() => Labels = Array.Empty<string>();

    [Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Description("The series, each a name and one value per label.")]
    public ItemList<ChartSeries> Series => series;

    [Category("Appearance"), DefaultValue("")]
    [Description("A unit written after every value in the readout.")]
    public string Unit { get => unit; set { unit = value ?? ""; Invalidate(); } }

    protected int Count => labels.Length;

    protected Color SeriesColor(int i) => P.Series[i % P.Series.Length];

    // A legend only names series when there are two or more.
    protected bool ShowLegend => series.Count >= 2;

    protected int HeaderHeight => (title.Length > 0 ? S(24) : 0) + (note.Length > 0 ? S(20) : 0) + (ShowLegend ? S(28) : 0);

    // Room at the right for what a chart writes past its last point.
    protected virtual int RightReserve => S(8);

    // 16 px under the header: the top tick's label is centered on the
    // plot's top line, so half of it rises above the plot.
    protected Rectangle Plot => Rectangle.FromLTRB(S(48), HeaderHeight + S(16), Width - RightReserve, Height - S(26));

    protected static float NiceStep(float raw) {
      if (raw <= 0) return 1;
      var mag = (float)Math.Pow(10, Math.Floor(Math.Log10(raw)));
      foreach (var m in new[] { 1f, 2f, 2.5f, 5f, 10f })
        if (m * mag >= raw) return m * mag;
      return 10 * mag;
    }

    // How a value reads in the readout, and how a tick reads on the axis: a
    // currency reads differently in each, ₱684,300.00 and ₱750K. Set in code:
    // a function has no designer form.
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<float, string>? Format { get; set; }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<float, string>? AxisFormat { get; set; }

    public static string Compact(float v) =>
        Math.Abs(v) >= 1_000_000 ? $"{v / 1_000_000:0.##}M" : Math.Abs(v) >= 10_000 ? $"{v / 1000:0.#}K" : v.ToString("0.##");

    protected string Short(float v) => AxisFormat?.Invoke(v) ?? Compact(v);

    protected string Full(float v) =>
        (Format?.Invoke(v) ?? v.ToString("N2").TrimEnd('0').TrimEnd('.')) + (unit.Length > 0 ? " " + unit : "");

    protected abstract float X(Rectangle plot, int i);

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e) {
      if (Count > 0 && e.KeyCode is Keys.Left or Keys.Right) {
        Hot = Hot < 0 ? Count - 1 : Math.Clamp(Hot + (e.KeyCode == Keys.Left ? -1 : 1), 0, Count - 1);
        Invalidate();
        e.Handled = true;
      }
      base.OnKeyDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      var plot = Plot;
      var next = -1;
      if (Count > 0 && plot.Contains(e.Location)) {
        var best = float.MaxValue;
        for (var i = 0; i < Count; i++) {
          var d = Math.Abs(X(plot, i) - e.X);
          if (d < best) { best = d; next = i; }
        }
      }
      if (next != Hot) { Hot = next; Invalidate(); }
      base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) {
      if (!Focused) { Hot = -1; Invalidate(); }
      base.OnMouseLeave(e);
    }

    protected override void OnLostFocus(EventArgs e) { Hot = -1; base.OnLostFocus(e); }

    protected void PaintFrame(Graphics g, float top, out Func<float, float> y) {
      var plot = Plot;
      var yy = 0;
      if (title.Length > 0) { Painter.Text(g, title, KitTheme.Font(14, true), new Rectangle(0, 0, Width, S(22)), P.Ink); yy += S(24); }
      if (note.Length > 0) { Painter.Text(g, note, KitTheme.Font(13), new Rectangle(0, yy, Width, S(18)), P.Ink2); yy += S(20); }
      var lx = 0;
      for (var i = 0; ShowLegend && i < series.Count; i++) {
        Painter.Smooth(g);
        using (var b = new SolidBrush(SeriesColor(i))) g.FillRectangle(b, lx, yy + S(8), S(10), S(10));
        lx += S(16);
        var w = Painter.Measure(series[i].Name, KitTheme.Font(13)).Width;
        Painter.Text(g, series[i].Name, KitTheme.Font(13), new Rectangle(lx, yy, w + S(2), S(26)), P.Ink2);
        lx += w + S(16);
      }
      var step = NiceStep(top / 4);
      var max = (float)Math.Ceiling(top / step) * step;
      if (max <= 0) max = step;
      float Y(float v) => plot.Bottom - v / max * plot.Height;
      y = Y;
      g.SmoothingMode = SmoothingMode.None;
      using (var grid = new Pen(P.Grid))
      using (var axis = new Pen(P.Axis)) {
        for (var t = 0f; t <= max + step / 2; t += step) {
          var py = (int)Y(t);
          g.DrawLine(t == 0 ? axis : grid, plot.X, py, plot.Right, py);
          Painter.Text(g, Short(t), KitTheme.Font(12), new Rectangle(0, py - S(9), plot.X - S(8), S(18)), P.Ink3, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }
      }
      var labelW = S(56);
      var every = Math.Max(1, (int)Math.Ceiling(Count * labelW / (float)Math.Max(1, plot.Width)));
      for (var i = 0; i < Count; i++) {
        if (i % every != 0 && i != Count - 1) continue;
        if (i != Count - 1 && Count - 1 - i < every && i != 0) continue;
        var cx = (int)X(plot, i);
        // A label that would run past either end is set against that
        // end instead of centered under its point.
        var box = new Rectangle(cx - labelW / 2, plot.Bottom + S(6), labelW, S(18));
        var align = TextFormatFlags.HorizontalCenter;
        if (box.Right > Width) { box.X = Width - labelW; align = TextFormatFlags.Right; } else if (box.X < 0) { box.X = 0; align = TextFormatFlags.Left; }
        Painter.Text(g, labels[i], KitTheme.Font(12), box, i == Hot ? P.Ink : P.Ink3, align | TextFormatFlags.VerticalCenter);
      }
      if (ShowFocus) Painter.FocusRing(g, new RectangleF(0, 0, Width, Height), S(M.RadiusPanel), S(2));
    }

    protected void PaintReadout(Graphics g) {
      if (Hot < 0 || Hot >= Count) return;
      var plot = Plot;
      var lines = new List<(string Text, Color? Swatch, bool Strong)> { (labels[Hot], null, true) };
      for (var s = 0; s < series.Count; s++) {
        var vals = series[s].Values;
        lines.Add(($"{series[s].Name}  {(Hot < vals.Length ? Full(vals[Hot]) : "No data")}", SeriesColor(s), false));
      }
      var lineH = S(18);
      var w = lines.Max(l => Painter.Measure(l.Text, KitTheme.Font(13, l.Strong)).Width) + S(36);
      var h = lineH * lines.Count + S(14);
      var x = X(plot, Hot) + S(12);
      if (x + w > Width - S(4)) x = X(plot, Hot) - S(12) - w;
      var box = new RectangleF(x, plot.Top + S(4), w, h);
      Painter.Smooth(g);
      Painter.Fill(g, P.Inverse, box, S(M.RadiusControl));
      var y = (int)box.Y + S(7);
      foreach (var (text, swatch, strong) in lines) {
        var tx = (int)box.X + S(10);
        if (swatch is { } c) {
          using var b = new SolidBrush(c);
          g.FillRectangle(b, tx, y + S(4), S(10), S(10));
          tx += S(16);
        }
        Painter.Text(g, text, KitTheme.Font(13, strong), new Rectangle(tx, y, (int)box.Right - tx - S(8), lineH), P.InverseInk);
        y += lineH;
      }
    }
  }

  // A trend over time: 2 px lines, a 10% wash under a
  // single series, one y axis from clean ticks.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Series))]
  [Description("Lines over time with a legend, gridlines and a readout on hover.")]
  public class LineChart : KitChart {
    bool zero = true;

    [Category("Behavior"), DefaultValue(true)]
    [Description("Keeps zero on the y axis.")]
    public bool Zero { get => zero; set { zero = value; Invalidate(); } }

    protected override float X(Rectangle plot, int i) =>
        plot.X + (Count <= 1 ? plot.Width / 2f : i * (float)plot.Width / (Count - 1));

    // Two to four lines are named at their right ends.
    bool EndLabels => Series.Count is >= 2 and <= 4;

    protected override int RightReserve => EndLabels
        ? S(12) + Series.Max(s => Painter.Measure(s.Name, KitTheme.Font(12, true)).Width) + S(4)
        : base.RightReserve;

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      var all = Series.SelectMany(s => s.Values.Take(Count)).DefaultIfEmpty(0).ToArray();
      PaintFrame(g, all.Max(), out var y);
      var plot = Plot;
      if (Hot >= 0) {
        using var guide = new Pen(P.Axis);
        var hx = (int)X(plot, Hot);
        g.DrawLine(guide, hx, plot.Top, hx, plot.Bottom);
      }
      Painter.Smooth(g);
      for (var s = 0; s < Series.Count; s++) {
        var vals = Series[s].Values;
        var n = Math.Min(vals.Length, Count);
        if (n < 2) continue;
        var pts = Enumerable.Range(0, n).Select(i => new PointF(X(plot, i), y(vals[i]))).ToArray();
        if (Series.Count == 1) {
          using var wash = new GraphicsPath();
          wash.AddLines(pts);
          wash.AddLine(pts[^1], new PointF(pts[^1].X, plot.Bottom));
          wash.AddLine(new PointF(pts[^1].X, plot.Bottom), new PointF(pts[0].X, plot.Bottom));
          wash.CloseFigure();
          using var b = new SolidBrush(Color.FromArgb(26, SeriesColor(s)));
          g.FillPath(b, wash);
        }
        using var pen = new Pen(SeriesColor(s), S(2)) { LineJoin = LineJoin.Round };
        g.DrawLines(pen, pts);
        if (Hot >= 0 && Hot < n) {
          using var dot = new SolidBrush(SeriesColor(s));
          using var ring = new Pen(P.Bg, S(2));
          var p = pts[Hot];
          g.FillEllipse(dot, p.X - S(4), p.Y - S(4), S(8), S(8));
          g.DrawEllipse(ring, p.X - S(4), p.Y - S(4), S(8), S(8));
        }
      }
      // The end labels step aside while a week is read out: the readout
      // box can cover them, and it names the series itself.
      if (EndLabels && Hot < 0) PaintEndLabels(g, plot, y);
      PaintReadout(g);
    }

    // Each name beside its line's last point, nudged apart where two ends
    // meet, so they never overlap.
    void PaintEndLabels(Graphics g, Rectangle plot, Func<float, float> y) {
      var font = KitTheme.Font(12, true);
      var h = font.Height;
      var ends = Enumerable.Range(0, Series.Count)
          .Where(s => Count > 0 && Series[s].Values.Length > 0)
          .Select(s => (Index: s, Y: y(Series[s].Values[Math.Min(Count, Series[s].Values.Length) - 1])))
          .OrderBy(e => e.Y)
          .ToList();
      var placed = new List<float>();
      foreach (var (s, ey) in ends) {
        var top = ey - h / 2f;
        if (placed.Count > 0 && top < placed[^1] + h) top = placed[^1] + h;
        placed.Add(top);
        Painter.Text(g, Series[s].Name, font, new Rectangle(plot.Right + S(8), (int)top, Width - plot.Right - S(8), h), SeriesColor(s));
      }
    }
  }

  // Columns for comparing amounts across categories: side by
  // side, or stacked; one category can be drawn in the accent with the rest
  // in gray.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Series))]
  [Description("Bars comparing amounts across categories, side by side or stacked.")]
  public class BarChart : KitChart {
    bool stacked;
    int emphasize = -1;

    [Category("Appearance"), DefaultValue(false)]
    [Description("Stacks the series into one bar per category.")]
    public bool Stacked { get => stacked; set { stacked = value; Invalidate(); } }

    [Category("Appearance"), DefaultValue(-1)]
    [Description("The position of one category drawn in the accent, the rest in gray. -1 for none.")]
    public int Emphasize { get => emphasize; set { emphasize = value; Invalidate(); } }

    protected override float X(Rectangle plot, int i) => plot.X + (i + 0.5f) * plot.Width / Math.Max(1, Count);

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      float top = 0;
      for (var i = 0; i < Count; i++) {
        var vals = Series.Select(s => i < s.Values.Length ? s.Values[i] : 0).ToArray();
        top = Math.Max(top, stacked ? vals.Sum() : vals.DefaultIfEmpty(0).Max());
      }
      PaintFrame(g, top, out var y);
      var plot = Plot;
      var slot = plot.Width / (float)Math.Max(1, Count);
      var group = slot * 0.7f;
      Painter.Smooth(g);
      for (var i = 0; i < Count; i++) {
        var cx = X(plot, i);
        if (i == Hot) {
          using var hl = new SolidBrush(P.BgSubtle);
          g.FillRectangle(hl, cx - slot / 2, plot.Top, slot, plot.Height);
        }
        var baseY = (float)plot.Bottom;
        for (var s = 0; s < Series.Count; s++) {
          var v = i < Series[s].Values.Length ? Series[s].Values[i] : 0;
          var color = emphasize >= 0 ? (i == emphasize ? P.Accent : P.SeriesMuted) : SeriesColor(s);
          RectangleF bar;
          if (stacked) {
            var h = plot.Bottom - y(v);
            bar = new RectangleF(cx - group / 2, baseY - h, group, h);
            baseY -= h;
          } else {
            var w = group / Math.Max(1, Series.Count);
            bar = new RectangleF(cx - group / 2 + s * w + S(1), y(v), Math.Max(S(2), w - S(2)), plot.Bottom - y(v));
          }
          using var b = new SolidBrush(color);
          g.FillRectangle(b, bar);
        }
      }
      PaintReadout(g);
    }
  }
}
