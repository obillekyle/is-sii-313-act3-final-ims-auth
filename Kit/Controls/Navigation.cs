using System.ComponentModel;

namespace Kit {
  // Tabs switch between views of one record: an underline under
  // the current tab, a count in a small badge after a label.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Items)), DefaultEvent(nameof(SelectedIndexChanged))]
  [Description("Tabs between views of one record, the current one underlined.")]
  public class Tabs : KitControl {
    readonly ItemList<TabItem> tabs;
    int selected, hover = -1;

    public Tabs() {
      tabs = new ItemList<TabItem>(Invalidate);
      SetStyle(ControlStyles.Selectable, true);
      TabStop = true;
      Size = new Size(360, 40);
    }

    [Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Description("The tabs in order, each a label, an optional count and whether it can be chosen.")]
    public ItemList<TabItem> Items => tabs;

    [Category("Data"), DefaultValue(0)]
    public int SelectedIndex {
      get => selected;
      set {
        if (value == selected || value < 0 || value >= Math.Max(1, tabs.Count)) return;
        selected = value;
        Invalidate();
        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    public event EventHandler? SelectedIndexChanged;

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) =>
        base.SetBoundsCore(x, y, width, S(M.Row), specified);

    Font LabelFont => KitTheme.Font(14, true);

    int TabWidth(int i) {
      var t = tabs[i];
      var w = Painter.Measure(t.Text, LabelFont).Width + S(24);
      if (t.Count is { } c) w += S(6) + Painter.Measure(c.ToString(), KitTheme.Font(12, true)).Width + S(12);
      return w;
    }

    Rectangle TabBounds(int i) {
      var x = 0;
      for (var k = 0; k < i; k++) x += TabWidth(k);
      return new Rectangle(x, 0, TabWidth(i), Height);
    }

    int HitTest(Point p) {
      for (var i = 0; i < tabs.Count; i++) if (TabBounds(i).Contains(p)) return i;
      return -1;
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      var h = HitTest(e.Location);
      if (h != hover) {
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
      if (i >= 0 && tabs[i].Enabled) SelectedIndex = i;
      Focus();
      base.OnMouseClick(e);
    }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e) {
      if (tabs.Count > 0 && e.KeyCode is Keys.Left or Keys.Right) {
        var dir = e.KeyCode == Keys.Right ? 1 : -1;
        for (int i = selected + dir, n = 0; n < tabs.Count; i += dir, n++) {
          var k = (i + tabs.Count) % tabs.Count;
          if (tabs[k].Enabled) { SelectedIndex = k; break; }
        }
        e.Handled = true;
      }
      base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      using (var line = new Pen(P.Line)) g.DrawLine(line, 0, Height - 1, Width, Height - 1);
      for (var i = 0; i < tabs.Count; i++) {
        var t = tabs[i];
        var r = TabBounds(i);
        var on = i == selected;
        var ink = !t.Enabled ? P.InkDisabled : on ? P.Ink : Fade.Mix(i + 1, P.Ink2, P.Ink);
        var tw = Painter.Measure(t.Text, LabelFont).Width;
        Painter.Text(g, t.Text, LabelFont, new Rectangle(r.X + S(12), 0, tw + 1, Height), ink);
        if (t.Count is { } c) {
          var cf = KitTheme.Font(12, true);
          var bw = Painter.Measure(c.ToString(), cf).Width + S(12);
          var badge = new RectangleF(r.X + S(12) + tw + S(6), (Height - S(18)) / 2f, bw, S(18));
          Painter.Fill(g, on ? P.AccentSoft : P.BgHover, badge, badge.Height / 2);
          Painter.Text(g, c.ToString(), cf, Rectangle.Round(badge), on ? P.AccentInk : P.Ink2, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        if (on) Painter.Fill(g, P.Inverse, new RectangleF(r.X + S(8), Height - S(2), r.Width - S(16), S(2)), 0);
        if (on && ShowFocus) Painter.FocusRing(g, new RectangleF(r.X + S(2), S(4), r.Width - S(4), Height - S(8)), S(M.RadiusControl), S(2));
      }
    }
  }

  // Where this page sits: the trail with chevrons
  // between, the last entry the current page and no link. A trail longer
  // than Max keeps the first and the last ones and folds the middle into "…".
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Items)), DefaultEvent(nameof(ItemClicked))]
  [Description("The trail to this page, the last entry the current page.")]
  public class Breadcrumbs : KitControl {
    string[] items = { "Home", "Section", "This page" };
    int max = 4, hover = -1;

    public Breadcrumbs() => Size = new Size(360, 24);

    [Category("Data")]
    [Description("The trail from the top level down to this page.")]
    public string[] Items { get => items; set { items = value ?? Array.Empty<string>(); Invalidate(); } }
    bool ShouldSerializeItems() => !items.SequenceEqual(new[] { "Home", "Section", "This page" });
    void ResetItems() => Items = new[] { "Home", "Section", "This page" };

    [Category("Behavior"), DefaultValue(4)]
    [Description("The most entries the trail shows, the folded ones counted as one.")]
    public int Max { get => max; set { max = Math.Max(2, value); Invalidate(); } }

    // The index in Items of the entry clicked; the current page is not a link.
    public event EventHandler<int>? ItemClicked;

    List<(int Index, string Text, Rectangle Bounds)> Parts() {
      var shown = new List<(int, string)>();
      if (items.Length <= max) shown.AddRange(items.Select((t, i) => (i, t)));
      else {
        shown.Add((0, items[0]));
        shown.Add((-1, "…"));
        for (var i = items.Length - (max - 2); i < items.Length; i++) shown.Add((i, items[i]));
      }
      var font = KitTheme.Font(13);
      var x = 0;
      var result = new List<(int, string, Rectangle)>();
      foreach (var (i, t) in shown) {
        var w = Painter.Measure(t, font).Width + 1;
        result.Add((i, t, new Rectangle(x, 0, w, Height)));
        x += w + S(20);
      }
      return result;
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      var hit = Parts().FirstOrDefault(r => r.Bounds.Contains(e.Location) && r.Index >= 0 && r.Index < items.Length - 1);
      var h = hit.Text == null ? -1 : hit.Index;
      if (h != hover) { hover = h; Cursor = h >= 0 ? Cursors.Hand : Cursors.Default; Invalidate(); }
      base.OnMouseMove(e);
    }

    protected override void OnMouseClick(MouseEventArgs e) {
      if (hover >= 0) ItemClicked?.Invoke(this, hover);
      base.OnMouseClick(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      var font = KitTheme.Font(13);
      var parts = Parts();
      for (var k = 0; k < parts.Count; k++) {
        var (i, t, r) = parts[k];
        var last = k == parts.Count - 1;
        var f = i == hover ? new Font(font, FontStyle.Underline) : font;
        Painter.Text(g, t, f, r, last ? P.Ink : P.Ink2);
        if (!ReferenceEquals(f, font)) f.Dispose();
        if (!last) Icons.Draw(g, "chevron-right", new RectangleF(r.Right + S(4), (Height - S(12)) / 2f, S(12), S(12)), P.Ink3);
      }
    }
  }

  // The app's left navigation: each link an icon and a label,
  // a count at the right end, the current page a tinted row. Collapsed, it
  // shows only the icons, each with its name as a tooltip.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Items)), DefaultEvent(nameof(SelectedIndexChanged))]
  [Description("The app's left navigation: icons and labels, the current page a tinted row.")]
  public class SideNav : KitControl {
    readonly ItemList<NavItem> items;
    int selected, hover = -1;
    bool collapsed;
    readonly ToolTip tip = new();

    public SideNav() {
      items = new ItemList<NavItem>(Invalidate);
      SetStyle(ControlStyles.Selectable, true);
      TabStop = true;
      Size = new Size(232, 320);
    }

    [Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Description("The links in order, each a label, an icon, an optional count and a key.")]
    public ItemList<NavItem> Items => items;

    [Category("Data"), DefaultValue(0)]
    [Description("The current page's position in Items.")]
    public int SelectedIndex {
      get => selected;
      set {
        if (value == selected || value < -1 || value >= Math.Max(1, items.Count)) return;
        selected = value;
        Invalidate();
        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? SelectedKey {
      get => selected >= 0 && selected < items.Count ? items[selected].Key : null;
      set => SelectedIndex = items.ToList().FindIndex(i => i.Key == value);
    }

    [Category("Appearance"), DefaultValue(false)]
    [Description("Shows only the icons, each with its name as a tooltip.")]
    public bool Collapsed { get => collapsed; set { collapsed = value; Invalidate(); } }

    public event EventHandler? SelectedIndexChanged;

    int RowH => S(36);
    // Where row i sits, for a scrolling parent to bring it into view.
    public Rectangle RowBounds(int i) => new(S(8), S(8) + i * (RowH + S(2)), Width - S(16), RowH);

    // As tall as its rows: a list longer than its space goes in a
    // scrolling panel at this height.
    public override Size GetPreferredSize(Size proposedSize) =>
        new(Width, S(16) + Math.Max(0, items.Count * (RowH + S(2)) - S(2)));

    int HitTest(Point p) {
      for (var i = 0; i < items.Count; i++) if (RowBounds(i).Contains(p)) return i;
      return -1;
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      var h = HitTest(e.Location);
      if (h != hover) {
        // A row's hover fill eases in and out (key: its index + 1).
        if (hover >= 0) Fade.Set(hover + 1, false);
        if (h >= 0) Fade.Set(h + 1, true);
        hover = h;
        if (collapsed && h >= 0) tip.Show(items[h].Text, this, Width + S(4), RowBounds(h).Y + S(6));
        else tip.Hide(this);
        Invalidate();
      }
      base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hover = -1; tip.Hide(this); base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e) {
      var i = HitTest(e.Location);
      if (i >= 0) SelectedIndex = i;
      Focus();
      base.OnMouseClick(e);
    }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Up or Keys.Down || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e) {
      if (items.Count > 0 && e.KeyCode == Keys.Down) { SelectedIndex = Math.Min(items.Count - 1, selected + 1); e.Handled = true; } else if (items.Count > 0 && e.KeyCode == Keys.Up) { SelectedIndex = Math.Max(0, selected - 1); e.Handled = true; }
      base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      for (var i = 0; i < items.Count; i++) {
        var it = items[i];
        var r = RowBounds(i);
        var on = i == selected;
        // The current page: a pressed neutral fill and the label at
        // weight 600, never color alone.
        if (on) Painter.Fill(g, P.BgPress, r, S(M.RadiusControl));
        else if (Fade[i + 1] > 0) Painter.Fill(g, Fade.Mix(i + 1, Color.Empty, P.BgHover), r, S(M.RadiusControl));
        if (on && ShowFocus) Painter.FocusRing(g, r, S(M.RadiusControl), S(2));
        var ink = P.Ink;
        var iconBox = new RectangleF(collapsed ? r.X + (r.Width - S(18)) / 2f : r.X + S(10), r.Y + (r.Height - S(18)) / 2f, S(18), S(18));
        if (it.Icon.Length > 0) Icons.Draw(g, it.Icon, iconBox, on ? P.Ink : P.Ink2);
        if (collapsed) continue;
        var tx = r.X + S(40);
        var countW = 0;
        if (it.Count is { } c) {
          var cf = KitTheme.Font(12, true);
          countW = Painter.Measure(c.ToString(), cf).Width + S(12);
          var badge = new RectangleF(r.Right - S(8) - countW, r.Y + (r.Height - S(18)) / 2f, countW, S(18));
          Painter.Fill(g, on ? P.Bg : P.BgHover, badge, badge.Height / 2);
          Painter.Text(g, c.ToString(), cf, Rectangle.Round(badge), P.Ink2, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        Painter.Text(g, it.Text, KitTheme.Font(14, on), new Rectangle(tx, r.Y, r.Right - tx - countW - S(12), r.Height), ink);
      }
    }

    protected override void Dispose(bool disposing) {
      if (disposing) tip.Dispose();
      base.Dispose(disposing);
    }
  }

  // Record tables page: the range shown and the total,
  // the page numbers between previous and next arrows, and a Rows menu for
  // the page size.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Total)), DefaultEvent(nameof(PageChanged))]
  [Description("The range shown, numbered pages between arrows, and the page size.")]
  public class Pagination : KitControl {
    int total = 59, page = 1, pageSize = 25, hover = -2;
    int[] sizes = { 25, 50, 100 };
    List<(int Page, Rectangle Bounds)> hits = new();

    public Pagination() => Size = new Size(560, 36);

    [Category("Data"), DefaultValue(59)]
    [Description("The number of records across every page.")]
    public int Total { get => total; set { total = Math.Max(0, value); page = Math.Min(page, Pages); Invalidate(); } }

    [Category("Data"), DefaultValue(1)]
    [Description("The page shown, from 1.")]
    public int Page {
      get => page;
      set {
        var v = Math.Clamp(value, 1, Pages);
        if (v == page) return;
        page = v;
        Invalidate();
        PageChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    [Category("Data"), DefaultValue(25)]
    public int PageSize {
      get => pageSize;
      set {
        pageSize = Math.Max(1, value);
        page = Math.Min(page, Pages);
        Invalidate();
        PageSizeChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    [Category("Data")]
    [Description("The page sizes the Rows menu offers.")]
    public int[] Sizes { get => sizes; set { sizes = value ?? Array.Empty<int>(); Invalidate(); } }
    bool ShouldSerializeSizes() => !sizes.SequenceEqual(new[] { 25, 50, 100 });
    void ResetSizes() => Sizes = new[] { 25, 50, 100 };

    public event EventHandler? PageChanged;
    public event EventHandler? PageSizeChanged;

    int Pages => Math.Max(1, (total + pageSize - 1) / pageSize);

    // Page numbers with gaps: the first, the last, and two either side of
    // the current one.
    List<int> Numbers() {
      var list = new List<int>();
      for (var p = 1; p <= Pages; p++) {
        if (p == 1 || p == Pages || Math.Abs(p - page) <= 1) list.Add(p);
        else if (list.Count > 0 && list[^1] != 0) list.Add(0);
      }
      return list;
    }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) =>
        base.SetBoundsCore(x, y, width, S(M.HeightMedium + 4), specified);

    protected override void OnMouseMove(MouseEventArgs e) {
      var h = hits.FirstOrDefault(t => t.Bounds.Contains(e.Location));
      var v = h.Bounds.IsEmpty ? -2 : h.Page;
      if (v != hover) {
        // Each page button's hover eases (key: its id + 1000, since
        // ids run below zero for the arrows and the rows menu).
        if (hover != -2) Fade.Set(hover + 1000, false);
        if (v != -2) Fade.Set(v + 1000, true);
        hover = v;
        Invalidate();
      }
      base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hover = -2; base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e) {
      var h = hits.FirstOrDefault(t => t.Bounds.Contains(e.Location));
      if (h.Bounds.IsEmpty) return;
      if (h.Page == -1) OpenSizes(h.Bounds);
      else if (h.Page == -3) Page = page - 1;
      else if (h.Page == -4) Page = page + 1;
      else if (h.Page > 0) Page = h.Page;
      base.OnMouseClick(e);
    }

    void OpenSizes(Rectangle under) {
      var menu = new ContextMenuStrip { ShowCheckMargin = true, ShowImageMargin = false };
      foreach (var s in sizes) {
        var item = new ToolStripMenuItem($"{s} rows") { Checked = s == pageSize };
        item.Click += (_, _) => PageSize = s;
        menu.Items.Add(item);
      }
      menu.Closed += (_, _) => BeginInvoke(menu.Dispose);
      KitMenu.Style(menu);
      menu.Show(this, new Point(under.X, under.Bottom + S(4)));
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      hits = new List<(int, Rectangle)>();
      var font = KitTheme.Font(13);
      var first = total == 0 ? 0 : (page - 1) * pageSize + 1;
      var last = Math.Min(page * pageSize, total);
      var range = total == 0 ? "No records" : $"{first:N0} to {last:N0} of {total:N0}";
      Painter.Text(g, range, font, new Rectangle(0, 0, Width / 2, Height), P.Ink2);

      var h = S(M.HeightMedium);
      var y = (Height - h) / 2;
      var x = Width;
      void Square(int id, string? text, string? icon, bool enabled, bool current) {
        var w = text == null ? h : Math.Max(h, Painter.Measure(text, font).Width + S(12));
        x -= w;
        var r = new Rectangle(x, y, w, h);
        // The current page in the inverse colors.
        if (current) Painter.Fill(g, P.Inverse, r, S(M.RadiusControl));
        else if (enabled && Fade[id + 1000] > 0) Painter.Fill(g, Fade.Mix(id + 1000, Color.Empty, P.BgHover), r, S(M.RadiusControl));
        var ink = !enabled ? P.InkDisabled : current ? P.InverseInk : P.Ink;
        if (icon != null) Icons.Draw(g, icon, new RectangleF(r.X + (w - S(16)) / 2f, r.Y + (h - S(16)) / 2f, S(16), S(16)), ink);
        else Painter.Text(g, text, font, r, ink, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        if (enabled && id != 0) hits.Add((id, r));
        x -= S(4);
      }

      Square(-4, null, "chevron-right", page < Pages, false);
      var nums = Numbers();
      for (var i = nums.Count - 1; i >= 0; i--) {
        if (nums[i] == 0) Square(0, "…", null, false, false);
        else Square(nums[i], nums[i].ToString(), null, true, nums[i] == page);
      }
      Square(-3, null, "chevron-left", page > 1, false);
      if (sizes.Length > 0) {
        x -= S(12);
        var label = $"{pageSize} rows";
        var w = Painter.Measure(label, font).Width + S(36);
        x -= w;
        var r = new Rectangle(x, y, w, h);
        Painter.Fill(g, Fade.Mix(-1 + 1000, P.Bg, P.BgHover), r, S(M.RadiusControl));
        Painter.Outline(g, P.LineStrong, r, S(M.RadiusControl));
        Painter.Text(g, label, font, new Rectangle(x + S(10), y, w, h), P.Ink);
        Icons.Draw(g, "chevron-down", new RectangleF(r.Right - S(22), y + (h - S(14)) / 2f, S(14), S(14)), P.Ink2);
        hits.Add((-1, r));
      }
    }
  }

  // Progress through a fixed sequence: each step a column
  // under a 2 px bar in its state's color, a 32 px mark (a check when done,
  // its number otherwise), and beside it the step's name over its detail,
  // or its state in words, so it reads without color.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Steps))]
  [Description("Named, numbered steps on one line, finished ones checked.")]
  public class Stepper : KitControl {
    readonly ItemList<StepItem> steps;

    public Stepper() {
      steps = new ItemList<StepItem>(Invalidate);
      Size = new Size(560, 72);
    }

    [Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Description("The steps in order, each a name, a state and an optional detail.")]
    public ItemList<StepItem> Steps => steps;

    static string Word(StepState s) => s switch {
      StepState.Done => "Done",
      StepState.Current => "Current step",
      StepState.Error => "Needs attention",
      _ => "Not started",
    };

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      Painter.Smooth(g);
      if (steps.Count == 0) return;
      var gap = S(8);
      var col = (Width - gap * (steps.Count - 1)) / (float)steps.Count;
      var d = S(32);
      for (var i = 0; i < steps.Count; i++) {
        var s = steps[i];
        var x = i * (col + gap);
        var bar = s.State switch { StepState.Done => P.Ink, StepState.Current => P.Accent, StepState.Error => P.Bad, _ => P.Line };
        using (var b = new SolidBrush(bar)) g.FillRectangle(b, x, 0, col, S(2));

        var mark = new RectangleF(x + S(1), S(2) + S(10), d, d);
        var fill = P.Bg;
        switch (s.State) {
          case StepState.Done:
            fill = P.Inverse;
            using (var b = new SolidBrush(fill)) g.FillEllipse(b, mark);
            Icons.Draw(g, "check", RectangleF.Inflate(mark, -S(9), -S(9)), P.InverseInk);
            break;
          case StepState.Current:
            using (var ring = new Pen(P.AccentSoft, S(3))) g.DrawEllipse(ring, RectangleF.Inflate(mark, S(1.5f), S(1.5f)));
            using (var b = new SolidBrush(fill)) g.FillEllipse(b, mark);
            using (var pen = new Pen(P.Accent)) g.DrawEllipse(pen, RectangleF.Inflate(mark, -0.5f, -0.5f));
            Painter.TextOn(g, (i + 1).ToString(), KitTheme.Font(12, true), Rectangle.Round(mark), P.AccentInk, fill);
            break;
          case StepState.Error:
            using (var b = new SolidBrush(fill)) g.FillEllipse(b, mark);
            using (var pen = new Pen(P.Bad)) g.DrawEllipse(pen, RectangleF.Inflate(mark, -0.5f, -0.5f));
            Icons.Draw(g, "circle-alert", RectangleF.Inflate(mark, -S(9), -S(9)), P.Bad);
            break;
          default:
            using (var b = new SolidBrush(fill)) g.FillEllipse(b, mark);
            using (var pen = new Pen(P.LineStrong)) g.DrawEllipse(pen, RectangleF.Inflate(mark, -0.5f, -0.5f));
            Painter.TextOn(g, (i + 1).ToString(), KitTheme.Font(12, true), Rectangle.Round(mark), P.Ink2, fill);
            break;
        }

        // The name on the mark's top edge, the detail on its bottom one.
        var tx = (int)(mark.Right + S(10));
        var tw = (int)(x + col) - tx;
        var nameFont = KitTheme.Font(14);
        var detailFont = KitTheme.Font(13);
        Painter.Text(g, s.Text, nameFont, new Rectangle(tx, (int)mark.Y - S(2), tw, nameFont.Height), s.State == StepState.Todo ? P.Ink2 : P.Ink,
            TextFormatFlags.Left | TextFormatFlags.Top);
        Painter.Text(g, s.Detail.Length > 0 ? s.Detail : Word(s.State), detailFont,
            new Rectangle(tx, (int)mark.Bottom - detailFont.Height + S(2), tw, detailFont.Height), P.Ink2, TextFormatFlags.Left | TextFormatFlags.Top);
      }
    }
  }
}
