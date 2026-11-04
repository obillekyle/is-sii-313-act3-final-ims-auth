using System.ComponentModel;
using Kit;

namespace Obille_Try.UI {
  // What the dashboard's cards share: a kit panel's frame (the page's raised
  // surface, a 1 px line, the panel radius) with a 48 px
  // header holding the title and a quieter note, a line under it.
  public abstract class DashboardCard : KitControl {
    string title = "", note = "";

    protected DashboardCard() {
      SetStyle(ControlStyles.Selectable, false);
      TabStop = false;
    }

    [Category("Appearance"), DefaultValue("")]
    [Description("The card's heading.")]
    public string Title { get => title; set { title = value ?? ""; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("A count or a short fact beside the title, in quieter text.")]
    public string Note { get => note; set { note = value ?? ""; Invalidate(); } }

    protected int HeaderHeight => S(48);

    // The area under the header, 16 px in from the frame.
    protected Rectangle Body => Rectangle.FromLTRB(S(16), HeaderHeight + S(12), Width - S(16), Height - S(16));

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var r = new RectangleF(0, 0, Width, Height);
      Painter.Fill(g, P.Bg, r, S(M.RadiusPanel));
      Painter.Outline(g, P.Line, r, S(M.RadiusPanel));
      var font = KitTheme.Font(16, true);
      var tw = Painter.Measure(title, font).Width;
      Painter.Text(g, title, font, new Rectangle(S(16), 0, Width - S(32), HeaderHeight), P.Ink);
      if (note.Length > 0)
        Painter.Text(g, note, KitTheme.Font(13), new Rectangle(S(16) + tw + S(8), 0, Width - tw - S(40), HeaderHeight), P.Ink2);
      using (var pen = new Pen(P.Line)) g.DrawLine(pen, 0, HeaderHeight - 1, Width, HeaderHeight - 1);
      PaintBody(g);
    }

    protected virtual void PaintBody(Graphics g) { }
  }

  // Pesos received and issued per week, as two lines or as paired bars, in
  // one card: the switch sits in the header, the kit's charts below it.
  [ToolboxItem(true)]
  [Description("Pesos received and issued per week, as lines or bars, in a card.")]
  public class FlowsCard : DashboardCard {
    readonly Segmented kind = new() { Options = new[] { "Lines", "Bars" }, SelectedIndex = 0, ControlSize = ControlSize.Small, AccessibleName = "Chart type" };
    readonly LineChart lines = new();
    readonly BarChart bars = new() { Visible = false };
    string[] labels = Array.Empty<string>();
    float[] received = Array.Empty<float>(), issued = Array.Empty<float>();

    public FlowsCard() {
      Controls.Add(kind);
      Controls.Add(lines);
      Controls.Add(bars);
      kind.SelectedIndexChanged += (_, _) => { lines.Visible = kind.SelectedIndex == 0; bars.Visible = kind.SelectedIndex == 1; };
      foreach (var chart in new KitChart[] { lines, bars }) {
        chart.Format = v => $"₱{v:N2}";
        chart.AxisFormat = v => "₱" + KitChart.Compact(v);
      }
      Size = new Size(560, 400);
    }

    [Category("Data")]
    [Description("The weeks, one label per value.")]
    public string[] Labels { get => labels; set { labels = value ?? Array.Empty<string>(); Fill(); } }
    bool ShouldSerializeLabels() => labels.Length > 0;
    void ResetLabels() => Labels = Array.Empty<string>();

    [Category("Data")]
    [Description("Pesos received, one value per week.")]
    public float[] Received { get => received; set { received = value ?? Array.Empty<float>(); Fill(); } }
    bool ShouldSerializeReceived() => received.Length > 0;
    void ResetReceived() => Received = Array.Empty<float>();

    [Category("Data")]
    [Description("Pesos issued, one value per week.")]
    public float[] Issued { get => issued; set { issued = value ?? Array.Empty<float>(); Fill(); } }
    bool ShouldSerializeIssued() => issued.Length > 0;
    void ResetIssued() => Issued = Array.Empty<float>();

    [Category("Appearance"), DefaultValue(false)]
    [Description("Shows the weeks as paired bars rather than lines.")]
    public bool ShowBars { get => kind.SelectedIndex == 1; set => kind.SelectedIndex = value ? 1 : 0; }

    void Fill() {
      foreach (var chart in new KitChart[] { lines, bars }) {
        chart.Labels = labels;
        chart.Series.Clear();
        chart.Series.Add(new ChartSeries("Received", received));
        chart.Series.Add(new ChartSeries("Issued", issued));
      }
    }

    protected override void OnLayout(LayoutEventArgs levent) {
      base.OnLayout(levent);
      var w = kind.GetPreferredSize(Size.Empty).Width;
      kind.SetBounds(Width - S(16) - w, (HeaderHeight - kind.Height) / 2, w, kind.Height);
      lines.Bounds = bars.Bounds = Body;
    }
  }

  // One line of the open work list: what is waiting, its icon, how many.
  public sealed class WorkItem : KitItem {
    string text = "Work", icon = "", count = "";

    public WorkItem() { }

    public WorkItem(string text, string icon, string count) {
      this.text = text;
      this.icon = icon;
      this.count = count;
    }

    [DefaultValue("Work")] public string Text { get => text; set { text = value ?? ""; Notify(); } }

    [DefaultValue(""), IconName, TypeConverter(typeof(IconNameConverter))]
    public string Icon { get => icon; set { icon = value ?? ""; Notify(); } }

    [DefaultValue("")] public string Count { get => count; set { count = value ?? ""; Notify(); } }

    public override string ToString() => text;
  }

  // What is waiting on the yard, one row each (scan2.jpg's list): an icon,
  // the work, the count at the right, a line between rows.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Items))]
  [Description("A card listing what is waiting, each row an icon, the work and its count.")]
  public class OpenWorkCard : DashboardCard {
    readonly ItemList<WorkItem> items;

    public OpenWorkCard() {
      items = new ItemList<WorkItem>(Invalidate);
      Size = new Size(340, 400);
    }

    [Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Description("The rows in order, each the work, an icon and its count.")]
    public ItemList<WorkItem> Items => items;

    protected override void PaintBody(Graphics g) {
      var rowH = S(KitTheme.Metrics.Row);
      var y = HeaderHeight;
      var font = KitTheme.Font(14);
      var strong = KitTheme.Font(14, true);
      using var pen = new Pen(P.Line);
      for (var i = 0; i < items.Count; i++) {
        var item = items[i];
        var x = S(16);
        if (item.Icon.Length > 0) {
          Icons.Draw(g, item.Icon, new RectangleF(x, y + (rowH - S(18)) / 2f, S(18), S(18)), P.Ink2);
          x += S(18) + S(12);
        }
        var cw = Painter.Measure(item.Count, strong).Width + 1;
        Painter.Text(g, item.Text, font, new Rectangle(x, y, Width - x - cw - S(28), rowH), P.Ink);
        Painter.Text(g, item.Count, strong, new Rectangle(Width - S(16) - cw, y, cw, rowH), P.Ink, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        y += rowH;
        if (i < items.Count - 1) g.DrawLine(pen, S(16), y - 1, Width - S(16), y - 1);
      }
    }
  }
}
