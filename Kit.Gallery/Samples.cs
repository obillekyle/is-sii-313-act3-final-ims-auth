namespace Kit.Gallery {
  // One entry in the gallery: what the component is called, its icon in the
  // list, and how to make one set up with real-looking data. Inspect is
  // what the PropertyGrid shows: the component itself, or for a dialog or
  // a toaster, the object behind the button on the stage.
  internal sealed record Sample(string Name, string Group, string Icon, Func<Sample.Built> Make) {
    internal sealed record Built(Control Stage, object Inspect, int Width = 360);
  }

  internal static class Samples {
    static readonly string[] Categories =
    {
            "Cement and aggregates", "Steel", "Masonry", "Lumber and plywood", "Roofing", "Plumbing", "Electrical",
            "Paint and finishes", "Hardware and fasteners",
        };

    static readonly string[] Suppliers =
    {
            "Agos Pipe and Fittings", "Bagong Silang Roofing Center", "Bulihan Aggregates and Hauling", "Kalasag Electrical Supply",
            "Matibay Cement Distributor", "Primeline Builders Depot", "Pulilan Lumber and Plywood", "Santo Rosario Steel Trading",
        };

    static ContextMenuStrip ExportMenu() {
      var m = new ContextMenuStrip();
      m.Items.Add("Export as CSV");
      m.Items.Add("Export as PDF");
      m.Items.Add(new ToolStripSeparator());
      m.Items.Add("Print");
      return m;
    }

    public static Toaster? Toaster { get; set; }

    // A table's header and rows, so a sample is as tall as its rows.
    static int RowsHeight(KitTable t) => t.ColumnHeadersHeight + t.Rows.GetRowsHeight(DataGridViewElementStates.Visible);

    // The dialog the KitDialog sample opens: the settings from the grid,
    // and a footer with the two answers. The shot harness builds the same.
    public static T FillDialog<T>(T d, KitDialog from) where T : KitDialog {
      d.Title = from.Title;
      d.Description = from.Description;
      d.DialogSize = from.DialogSize;
      d.Dismissable = from.Dismissable;
      var footer = new DialogFooter();
      footer.Controls.Add(new KitButton { Text = "Delete 3", Variant = ButtonVariant.Danger, DialogResult = DialogResult.OK, AutoSize = true });
      footer.Controls.Add(new KitButton { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true });
      d.Controls.Add(footer);
      // A confirmation has no body: the header, then the footer.
      d.ClientSize = new Size(d.ClientSize.Width, d.HeaderHeight + d.LogicalToDeviceUnits(8) + footer.Height);
      return d;
    }

    public static readonly Sample[] All =
    {
            new("KitButton", "Buttons", "mouse-pointer-click", () =>
            {
                var b = new KitButton { Text = "Save changes", Variant = ButtonVariant.Primary, Icon = "check", AutoSize = true };
                return new(b, b);
            }),
            new("MenuButton", "Buttons", "menu", () =>
            {
                var b = new MenuButton { Text = "Export", Icon = "download", DropDown = ExportMenu(), AutoSize = true };
                return new(b, b);
            }),
            new("SplitButton", "Buttons", "split", () =>
            {
                var b = new SplitButton { Text = "Export", Icon = "download", DropDown = ExportMenu() };
                b.Size = b.GetPreferredSize(Size.Empty);
                return new(b, b);
            }),
            new("TextInput", "Inputs", "text-cursor-input", () =>
            {
                var t = new TextInput { Type = InputType.Search, Placeholder = "Search materials", Clearable = true };
                return new(t, t, 320);
            }),
            new("Textarea", "Inputs", "pilcrow", () =>
            {
                var t = new Textarea { Placeholder = "Notes for the site engineer", MaxLength = 200, Threshold = 0.5 };
                return new(t, t, 360);
            }),
            new("NumberInput", "Inputs", "percent", () =>
            {
                var n = new NumberInput { Minimum = 0, Maximum = 100, Step = 5, Value = 25, Unit = "%" };
                return new(n, n, 200);
            }),
            new("Select", "Choices", "chevron-down", () =>
            {
                var s = new Select { Options = Categories, Placeholder = "Choose a category" };
                return new(s, s, 280);
            }),
            new("Combobox", "Choices", "search", () =>
            {
                var c = new Combobox { Options = Suppliers, Placeholder = "Search suppliers", Creatable = true };
                return new(c, c, 320);
            }),
            new("Checkbox", "Choices", "square-check", () =>
            {
                var c = new Checkbox { Text = "Email me when stock runs low", Checked = true };
                c.Width = c.GetPreferredSize(Size.Empty).Width;
                return new(c, c, c.Width);
            }),
            new("Switch", "Choices", "toggle-right", () =>
            {
                var s = new Switch { Text = "Compact rows", Checked = true };
                return new(s, s, 280);
            }),
            new("RadioGroup", "Choices", "circle-dot", () =>
            {
                var r = new RadioGroup { Options = new[] { "Deliver to the site", "Pick up at the yard", "Hold for the next trip" }, SelectedIndex = 0 };
                r.Size = r.GetPreferredSize(Size.Empty);
                return new(r, r, r.Width);
            }),
            new("Segmented", "Choices", "columns-3", () =>
            {
                var s = new Segmented { Options = new[] { "Day", "Week", "Month" }, SelectedIndex = 1 };
                return new(s, s, s.Width);
            }),
            new("Slider", "Choices", "sliders-horizontal", () =>
            {
                var s = new Slider { Value = 40, ShowValue = true, Unit = "%" };
                return new(s, s, 320);
            }),
            new("Field", "Forms", "type", () =>
            {
                var f = new Field { Label = "Supplier", Hint = "The supplier this material is usually bought from.", Error = "Choose a supplier before saving." };
                f.Controls.Add(new Combobox { Options = Suppliers, Placeholder = "Search suppliers" });
                return new(f, f, 320);
            }),
            new("Badge", "Display", "badge", () =>
            {
                var b = new Badge { Text = "Approved", Tone = Tone.Success };
                return new(b, b, b.Width);
            }),
            new("Alert", "Display", "triangle-alert", () =>
            {
                var a = new Alert { Tone = Tone.Warning, Title = "2 deliveries are past due", Text = "The Bulacan site is waiting on steel bars and cement from Oct 24.", Dismissable = true };
                return new(a, a, 420);
            }),
            new("Avatar", "Display", "user", () =>
            {
                var a = new Avatar { PersonName = "Cyrus Obille", Size = new Size(40, 40) };
                return new(a, a, 40);
            }),
            new("KitIcon", "Display", "image", () =>
            {
                var i = new KitIcon { IconName = "house", Size = new Size(32, 32) };
                return new(i, i, 32);
            }),
            new("Kbd", "Display", "keyboard", () =>
            {
                var k = new Kbd { Keys = new[] { "Ctrl", "Shift", "N" } };
                return new(k, k, k.Width);
            }),
            new("Skeleton", "Display", "rows-3", () =>
            {
                var s = new Skeleton { Lines = 3, Size = new Size(320, 52) };
                return new(s, s, 320);
            }),
            new("EmptyState", "Display", "inbox", () =>
            {
                var e = new EmptyState { Title = "No deliveries yet", Text = "Deliveries to sites appear here once a request is issued.", ActionText = "New delivery", Icon = "inbox" };
                e.Height = e.GetPreferredSize(Size.Empty).Height;
                return new(e, e, 420);
            }),
            new("Progress", "Display", "loader-circle", () =>
            {
                var p = new Progress { Label = "Importing materials", Value = 64, Remaining = 40 };
                return new(p, p, 360);
            }),
            new("Meter", "Display", "gauge", () =>
            {
                var m = new Meter { Label = "Cement shed", Value = 21, Maximum = 24, Unit = "pallets" };
                return new(m, m, 360);
            }),
            new("Sparkline", "Display", "activity", () =>
            {
                var s = new Sparkline();
                return new(s, s, 96);
            }),
            new("StatTile", "Display", "chart-line", () =>
            {
                var t = new StatTile
                {
                    Label = "Stock value", Value = "₱5.03M", Change = -2.9, Percent = true, Period = "vs last week",
                    Trend = new[] { 2.35f, 2.34f, 2.51f, 2.53f, 2.71f, 2.76f, 2.87f, 2.94f, 2.68f, 2.85f, 2.7f },
                };
                return new(t, t, 300);
            }),
            new("KitPanel", "Containers", "panel-top", () =>
            {
                var p = new KitPanel { Title = "Low stock", Note = "6 materials", Flush = true, Size = new Size(560, 336) };
                var t = new KitTable { Framed = false, Dock = DockStyle.Fill };
                t.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Material", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                t.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", Width = 80, ValueType = typeof(int) });
                t.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Reorder at", Width = 112, ValueType = typeof(int) });
                p.Controls.Add(t);
                t.HandleCreated += (_, _) =>
                {
                    // The six at or below their reorder point in the IMS's
                    // SampleData, their stock in the danger ink.
                    t.Rows.Add("Concrete curing compound, 18 L", 4, 6);
                    t.Rows.Add("Deformed bar, 20 mm x 6 m, Grade 40", 42, 60);
                    t.Rows.Add("Double-sided foil insulation, 10 mm, 1.2 x 25 m", 3, 6);
                    t.Rows.Add("Water closet, two-piece, white", 2, 2);
                    t.Rows.Add("Panel board, 8 branches", 1, 2);
                    t.Rows.Add("Lacquer thinner, 4 L", 5, 6);
                    foreach (DataGridViewRow r in t.Rows) r.Cells[1].Tag = Tone.Danger;
                    p.Height = p.Padding.Vertical + RowsHeight(t);
                };
                return new(p, p, 560);
            }),
            new("Disclosure", "Containers", "chevrons-right", () =>
            {
                var d = new Disclosure { Title = "Advanced", Note = "2 settings", Size = new Size(380, 120) };
                d.Controls.Add(new Switch { Text = "Round costs to whole pesos", Dock = DockStyle.Top });
                d.Controls.Add(new Switch { Text = "Show archived materials", Dock = DockStyle.Top });
                return new(d, d, 380);
            }),
            new("PageHeader", "Containers", "heading", () =>
            {
                var h = new PageHeader
                {
                    Crumbs = new[] { "Stock", "Materials" }, Title = "Materials", Description = "59 materials in the Malolos yard",
                    Status = "Draft", StatusTone = Tone.Warning, Width = 640,
                };
                h.Controls.Add(new KitButton { Text = "New material", Variant = ButtonVariant.Primary, Icon = "plus", AutoSize = true });
                h.Controls.Add(new KitButton { Text = "Export", Icon = "download", Variant = ButtonVariant.Ghost, IconOnly = true });
                return new(h, h, 640);
            }),
            new("Tabs", "Navigation", "panel-left", () =>
            {
                var t = new Tabs();
                t.Items.AddRange(new[] { new TabItem("Details", null, true), new TabItem("Stock", 6, true), new TabItem("Movements", null, true), new TabItem("Purchase orders", null, false) });
                return new(t, t, 420);
            }),
            new("Breadcrumbs", "Navigation", "chevron-right", () =>
            {
                var b = new Breadcrumbs { Items = new[] { "Stock", "Materials", "Steel", "Angle bar, 2 x 2 in" } };
                return new(b, b, 420);
            }),
            new("SideNav", "Navigation", "panel-left-open", () =>
            {
                var s = new SideNav { Size = new Size(232, 260) };
                s.Items.AddRange(new[]
                {
                    new NavItem("Dashboard", "house", null, "dashboard"), new NavItem("Materials", "layout-grid", null, "materials"),
                    new NavItem("Requests", "clipboard-check", 5, "requests"), new NavItem("Deliveries", "map-pin", 3, "deliveries"),
                    new NavItem("Reports", "chart-column", null, "reports"), new NavItem("Settings", "settings", null, "settings"),
                });
                return new(s, s, 232);
            }),
            new("Pagination", "Navigation", "chevrons-right", () =>
            {
                var p = new Pagination { Total = 213, Page = 3 };
                return new(p, p, 620);
            }),
            new("Stepper", "Navigation", "list-ordered", () =>
            {
                var s = new Stepper();
                s.Steps.AddRange(new[]
                {
                    new StepItem("Request", StepState.Done, "Oct 21"), new StepItem("Approval", StepState.Done, "Oct 22"),
                    new StepItem("Issue", StepState.Current, ""), new StepItem("Delivery", StepState.Todo, ""),
                });
                return new(s, s, 560);
            }),
            new("KitTable", "Data", "table", () =>
            {
                var t = new KitTable { Selectable = true, Size = new Size(620, 260) };
                t.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Material", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                t.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Code", Width = 112 });
                t.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", Width = 100, ValueType = typeof(int), DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                t.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unit cost", Width = 110, ValueType = typeof(decimal), DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N2" } });
                t.HandleCreated += (_, _) =>
                {
                    t.Rows.Add("Aggregate base course, Item 201", "AGG-0004", 1069, 950m);
                    t.Rows.Add("Anchor bolt, 1/2 x 6 in", "HDW-0401", 678, 45m);
                    t.Rows.Add("Angle bar, 2 x 2 in x 3/16 in x 6 m", "STL-0301", 712, 1180m);
                    t.Rows.Add("Ceramic floor tile, 60 x 60 cm", "TIL-0060", 250, 720m);
                    t.Rows.Add("Circuit breaker, 20 A, 1 pole", "ELE-0301", 68, 425m);
                    // The frame's bottom line sits under the last row.
                    t.Height = RowsHeight(t) + t.LogicalToDeviceUnits(1);
                };
                return new(t, t, 620);
            }),
            new("LineChart", "Charts", "chart-line", () =>
            {
                var c = new LineChart
                {
                    Title = "Received and issued, weekly", Note = "Malolos yard, Aug 10 to Oct 19", Size = new Size(560, 280),
                    Labels = new[] { "Aug 10", "Aug 17", "Aug 24", "Aug 31", "Sep 7", "Sep 14", "Sep 21", "Sep 28", "Oct 5", "Oct 12", "Oct 19" },
                };
                c.Series.Add(new ChartSeries("Received", new[] { 184500f, 352800f, 468200f, 512600f, 706400f, 548900f, 402300f, 612700f, 455100f, 688300f, 691300f }));
                c.Series.Add(new ChartSeries("Issued", new[] { 438200f, 366700f, 301500f, 487900f, 532400f, 498100f, 286400f, 541800f, 723600f, 512200f, 842600f }));
                return new(c, c, 560);
            }),
            new("BarChart", "Charts", "chart-column", () =>
            {
                var c = new BarChart
                {
                    Title = "Materials by category", Note = "59 in the Malolos yard", Size = new Size(620, 280),
                    Labels = new[] { "Cement", "Steel", "Masonry", "Lumber", "Roofing", "Plumbing", "Electrical", "Finishes", "Hardware" },
                };
                c.Series.Add(new ChartSeries("Materials", new[] { 7f, 8, 3, 8, 5, 7, 7, 7, 7 }));
                return new(c, c, 620);
            }),
            new("KitDialog", "Overlays", "panel-top", () =>
            {
                var dialog = new KitDialog { Title = "Delete 3 materials?", Description = "Their stock comes off the count. This can't be undone.", DialogSize = DialogSize.Small };
                var open = new KitButton { Text = "Open the dialog", AutoSize = true };
                open.Click += (_, _) =>
                {
                    using var d = FillDialog(new KitDialog(), dialog);
                    if (open.FindForm() is { } owner) d.ShowOver(owner);
                };
                return new(open, dialog, open.Width);
            }),
            new("Toaster", "Overlays", "megaphone", () =>
            {
                var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
                var plain = new KitButton { Text = "Show a toast", AutoSize = true };
                plain.Click += (_, _) => Toaster?.Show("Galvanized steel bar added");
                var undo = new KitButton { Text = "With an undo", AutoSize = true };
                undo.Click += (_, _) => Toaster?.Show("3 materials deleted", Tone.Success, "Undo", () => Toaster?.Show("3 materials restored"));
                row.Controls.Add(plain);
                row.Controls.Add(undo);
                return new(row, (object?)Toaster ?? row, 320);
            }),
            new("IconPicker", "Overlays", "palette", () =>
            {
                var p = new IconPicker { Size = new Size(320, 340), SelectedIcon = "house" };
                return new(p, p, 320);
            }),
        };
  }
}
