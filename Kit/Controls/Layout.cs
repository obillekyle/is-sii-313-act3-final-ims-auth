using System.ComponentModel;
using System.Windows.Forms.Layout;

namespace Kit {
  // A control whose look reaches past its body: a card's shadow. CSS lets a
  // box-shadow spill outside the box; a WinForms control cannot paint outside
  // its bounds, so it keeps the room inside them and says how much here. Flex
  // and Grid lay the body out on the cell and grow the control by its bleed
  // around it, so a tile's card lines up with the panels beside it.
  public interface IKitBleed {
    Padding Bleed { get; }
  }

  public enum FlexDirection { Row, Column }
  public enum FlexAlign { Stretch, Start, Center, End }
  public enum FlexJustify { Start, Center, End, Between }

  // Spacing steps: the kit's gaps count 4 px each.
  static class Space {
    public static int Px(Control c, int steps) => c.LogicalToDeviceUnits(Math.Max(0, steps) * 4);

    // The bleed a container must leave inside its own edges for its
    // children's, and which it then reports to its own parent.
    // The children that take part in layout. Visible reads false for every
    // child of a hidden container (a page not in view), so it is trusted
    // only while the container itself shows; until then all count.
    // In the order they were added, as FlowLayoutPanel lays them out and
    // as the designer writes their Controls.Add lines.
    public static List<Control> Shown(Control container) =>
        container.Controls.Cast<Control>().Where(c => c.Visible || !container.Visible).ToList();

    public static Padding MaxBleed(Control container) {
      int l = 0, t = 0, r = 0, b = 0;
      foreach (var c in Shown(container)) {
        if (c is not IKitBleed k) continue;
        var p = k.Bleed;
        l = Math.Max(l, p.Left); t = Math.Max(t, p.Top); r = Math.Max(r, p.Right); b = Math.Max(b, p.Bottom);
      }
      return new Padding(l, t, r, b);
    }

    public static Rectangle Grow(Rectangle body, Control c) =>
        c is IKitBleed k
            ? Rectangle.FromLTRB(body.Left - k.Bleed.Left, body.Top - k.Bleed.Top, body.Right + k.Bleed.Right, body.Bottom + k.Bleed.Bottom)
            : body;

    public static Size Body(Control c) =>
        c is IKitBleed k ? new Size(c.Width - k.Bleed.Horizontal, c.Height - k.Bleed.Vertical) : c.Size;
  }

  // A row or a column of controls with the theme's gap between them:
  // aligned across, spread along, and a child set to Grow takes
  // the room left over. A column's children take its width and keep their
  // own height; a row's take its height and keep their own width.
  [ToolboxItem(true)]
  [ProvideProperty("Grow", typeof(Control))]
  [Description("A row or a column of controls with the kit's gap between them.")]
  public class Flex : Panel, IExtenderProvider, IKitBleed {
    FlexDirection direction = FlexDirection.Row;
    FlexAlign align = FlexAlign.Stretch;
    FlexJustify justify = FlexJustify.Start;
    int gap = 2;
    readonly HashSet<Control> growing = new();
    readonly FlexLayout engine;

    public Flex() {
      engine = new FlexLayout(this);
      Size = new Size(320, 120);
    }

    [Category("Layout"), DefaultValue(FlexDirection.Row)]
    [Description("Side by side in a row, or one under another in a column.")]
    public FlexDirection Direction { get => direction; set { direction = value; PerformLayout(); } }

    [Category("Layout"), DefaultValue(2)]
    [Description("The space between the controls in 4 px steps: 2 is 8 px, 4 is 16 px.")]
    public int Gap { get => gap; set { gap = Math.Max(0, value); PerformLayout(); } }

    [Category("Layout"), DefaultValue(FlexAlign.Stretch)]
    [Description("Places the controls across the direction. Stretch gives a column's controls its width and a row's its height.")]
    public FlexAlign Align { get => align; set { align = value; PerformLayout(); } }

    [Category("Layout"), DefaultValue(FlexJustify.Start)]
    [Description("Spreads the controls along the direction when none grows. Between shares the room left between them.")]
    public FlexJustify Justify { get => justify; set { justify = value; PerformLayout(); } }

    bool IExtenderProvider.CanExtend(object extendee) => extendee is Control c && c.Parent == this;

    [Category("Layout"), DefaultValue(false), DisplayName("Grow")]
    [Description("Takes the room the other controls leave along the Flex, shared with any other that grows.")]
    public bool GetGrow(Control control) => growing.Contains(control);

    public void SetGrow(Control control, bool value) {
      if (value) growing.Add(control); else growing.Remove(control);
      PerformLayout();
    }

    public Padding Bleed => Space.MaxBleed(this);

    public override LayoutEngine LayoutEngine => engine;

    protected override void OnControlRemoved(ControlEventArgs e) {
      if (e.Control != null) growing.Remove(e.Control);
      base.OnControlRemoved(e);
    }

    sealed class FlexLayout : LayoutEngine {
      readonly Flex flex;
      public FlexLayout(Flex flex) => this.flex = flex;

      public override bool Layout(object container, LayoutEventArgs args) {
        var f = flex;
        var bleed = f.Bleed;
        var area = f.DisplayRectangle;
        area = Rectangle.FromLTRB(area.Left + f.Padding.Left + bleed.Left, area.Top + f.Padding.Top + bleed.Top,
            area.Right - f.Padding.Right - bleed.Right, area.Bottom - f.Padding.Bottom - bleed.Bottom);
        var row = f.direction == FlexDirection.Row;
        var items = Space.Shown(f);
        if (items.Count == 0) return false;
        var gap = Space.Px(f, f.gap);

        int Main(Control c) => row ? Space.Body(c).Width : Space.Body(c).Height;
        var fixedMain = items.Where(c => !f.growing.Contains(c)).Sum(Main) + gap * (items.Count - 1);
        var room = (row ? area.Width : area.Height) - fixedMain;
        var growers = items.Count(c => f.growing.Contains(c));
        var share = growers > 0 ? Math.Max(0, room) / growers : 0;

        var pos = row ? area.Left : area.Top;
        var between = gap;
        if (growers == 0 && room > 0) {
          if (f.justify == FlexJustify.Center) pos += room / 2;
          else if (f.justify == FlexJustify.End) pos += room;
          else if (f.justify == FlexJustify.Between && items.Count > 1) between += room / (items.Count - 1);
        }

        foreach (var c in items) {
          var main = f.growing.Contains(c) ? share : Main(c);
          var crossRoom = row ? area.Height : area.Width;
          var own = row ? Space.Body(c).Height : Space.Body(c).Width;
          var cross = f.align == FlexAlign.Stretch ? crossRoom : Math.Min(own, crossRoom);
          var offset = f.align switch {
            FlexAlign.Center => (crossRoom - cross) / 2,
            FlexAlign.End => crossRoom - cross,
            _ => 0,
          };
          var body = row
              ? new Rectangle(pos, area.Top + offset, main, cross)
              : new Rectangle(area.Left + offset, pos, cross, main);
          var bounds = Space.Grow(body, c);
          if (c.Bounds != bounds) c.Bounds = bounds;
          pos += main + between;
        }
        return false;
      }
    }
  }

  // A grid of equal columns, counted or as many as fit at a least width,
  // with the theme's gap. A control spans columns with the Span
  // the grid lends it. Each row is as tall as its tallest
  // control, and the grid as tall as its rows, unless FillHeight shares the
  // grid's own height between the rows.
  [ToolboxItem(true)]
  [ProvideProperty("Span", typeof(Control))]
  [Description("A grid of equal columns with the kit's gap; a control spans columns with Span.")]
  public class Grid : Panel, IExtenderProvider, IKitBleed {
    int columns = 2, min, gap = 4, rowGap = -1;
    bool fillHeight;
    readonly Dictionary<Control, int> spans = new();
    readonly GridLayout engine;

    public Grid() {
      engine = new GridLayout(this);
      Size = new Size(480, 160);
    }

    [Category("Layout"), DefaultValue(2)]
    [Description("The number of equal columns, 1 to 12. Set aside when MinColumnWidth is set.")]
    public int Columns { get => columns; set { columns = Math.Clamp(value, 1, 12); PerformLayout(); } }

    [Category("Layout"), DefaultValue(0)]
    [Description("The least width of a column in px. Set, the grid makes as many columns as fit in place of Columns.")]
    public int MinColumnWidth { get => min; set { min = Math.Max(0, value); PerformLayout(); } }

    [Category("Layout"), DefaultValue(4)]
    [Description("The space between columns, and between rows unless RowGap is set, in 4 px steps: 4 is 16 px.")]
    public int Gap { get => gap; set { gap = Math.Max(0, value); PerformLayout(); } }

    [Category("Layout"), DefaultValue(-1)]
    [Description("The space between rows in 4 px steps, where it differs from Gap. -1 uses Gap.")]
    public int RowGap { get => rowGap; set { rowGap = Math.Max(-1, value); PerformLayout(); } }

    [Category("Layout"), DefaultValue(false)]
    [Description("Shares the grid's height between its rows, for panels that fill the rest of a page. Off, the grid is as tall as its rows.")]
    public bool FillHeight { get => fillHeight; set { fillHeight = value; PerformLayout(); } }

    bool IExtenderProvider.CanExtend(object extendee) => extendee is Control c && c.Parent == this;

    [Category("Layout"), DefaultValue(1), DisplayName("Span")]
    [Description("The number of the grid's columns this control takes.")]
    public int GetSpan(Control control) => spans.TryGetValue(control, out var s) ? s : 1;

    public void SetSpan(Control control, int value) {
      if (value <= 1) spans.Remove(control); else spans[control] = Math.Min(value, 12);
      PerformLayout();
    }

    public Padding Bleed => Space.MaxBleed(this);

    public override LayoutEngine LayoutEngine => engine;

    protected override void OnControlRemoved(ControlEventArgs e) {
      if (e.Control != null) spans.Remove(e.Control);
      base.OnControlRemoved(e);
    }

    sealed class GridLayout : LayoutEngine {
      readonly Grid grid;
      public GridLayout(Grid grid) => this.grid = grid;

      public override bool Layout(object container, LayoutEventArgs args) {
        var g = grid;
        var bleed = g.Bleed;
        var area = g.DisplayRectangle;
        area = Rectangle.FromLTRB(area.Left + g.Padding.Left + bleed.Left, area.Top + g.Padding.Top + bleed.Top,
            area.Right - g.Padding.Right - bleed.Right, area.Bottom - g.Padding.Bottom - bleed.Bottom);
        var items = Space.Shown(g);
        if (items.Count == 0) return false;
        var colGap = Space.Px(g, g.gap);
        var rowGap = g.rowGap < 0 ? colGap : Space.Px(g, g.rowGap);
        var minPx = g.LogicalToDeviceUnits(g.min);
        var cols = g.min > 0 ? Math.Max(1, (area.Width + colGap) / (minPx + colGap)) : g.columns;
        var colW = Math.Max(1f, (area.Width - colGap * (cols - 1)) / (float)cols);

        // Place in order, a row at a time, wrapping when a span does
        // not fit what is left of the row.
        var rows = new List<List<(Control C, int Col, int Span)>> { new() };
        var col = 0;
        foreach (var c in items) {
          var span = Math.Min(g.GetSpan(c), cols);
          if (col + span > cols) { rows.Add(new()); col = 0; }
          rows[^1].Add((c, col, span));
          col += span;
        }

        var heights = rows.Select(r => r.Max(i => Space.Body(i.C).Height)).ToList();
        if (g.fillHeight && rows.Count > 0) {
          var each = Math.Max(0, (area.Height - rowGap * (rows.Count - 1)) / rows.Count);
          for (var i = 0; i < heights.Count; i++) heights[i] = each;
        }

        var y = area.Top;
        for (var r = 0; r < rows.Count; r++) {
          foreach (var (c, start, span) in rows[r]) {
            var x = area.Left + (int)Math.Round(start * (colW + colGap));
            var right = area.Left + (int)Math.Round((start + span) * (colW + colGap) - colGap);
            var body = new Rectangle(x, y, right - x, heights[r]);
            var bounds = Space.Grow(body, c);
            if (c.Bounds != bounds) c.Bounds = bounds;
          }
          y += heights[r] + rowGap;
        }

        // As tall as its rows, unless it shares its own height out.
        if (!g.fillHeight && g.Dock is not (DockStyle.Fill or DockStyle.Left or DockStyle.Right)) {
          var want = y - rowGap - area.Top + g.Padding.Vertical + bleed.Vertical;
          if (g.Height != want) g.Height = want;
        }
        return false;
      }
    }
  }
}
