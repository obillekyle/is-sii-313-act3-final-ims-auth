namespace Kit.Gallery {
  // The playground: the theme on top, the components on the left, the one
  // chosen live in the middle with the C# that sets it up under it, and its
  // properties in a PropertyGrid on the right. Every edit in the grid shows
  // at once on the stage and in the code.
  internal sealed class GalleryForm : Form {
    readonly ThemeProvider theme;
    readonly Toaster toaster;
    readonly SideNav nav = new() { Dock = DockStyle.Top };
    // The list is taller than the window: it scrolls in here.
    readonly Panel navScroll = new() { Dock = DockStyle.Fill, AutoScroll = true };
    readonly TextInput search = new() { Type = InputType.Search, Placeholder = "Find a component", Dock = DockStyle.Top };
    readonly Panel stage = new() { Dock = DockStyle.Fill };
    readonly TextBox code = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.None, BorderStyle = BorderStyle.None, Dock = DockStyle.Fill, WordWrap = true };
    readonly PropertyGrid grid = new() { Dock = DockStyle.Fill, PropertySort = PropertySort.Categorized, ToolbarVisible = false, HelpVisible = true };
    readonly Switch kitOnly = new() { Text = "Only the kit's properties", Checked = true, Dock = DockStyle.Top };
    readonly Caption heading = new() { Dock = DockStyle.Top, Height = 44, Px = 20, Strong = true };
    readonly Caption summary = new() { Dock = DockStyle.Top, Height = 44, Px = 14, Quiet = true };
    readonly Segmented modeChoice = new() { Options = new[] { "System", "Light", "Dark" }, SelectedIndex = 0, Dock = DockStyle.Right };
    List<Sample> shown = Samples.All.ToList();
    Sample? current;
    Sample.Built? built;

    public GalleryForm() {
      Text = "Kit gallery";
      ClientSize = new Size(1360, 820);
      StartPosition = FormStartPosition.CenterScreen;
      // Laid out below in 96 DPI units and scaled once at the end.
      AutoScaleMode = AutoScaleMode.None;
      Font = KitTheme.Font(14);

      KitEditors.Install();
      theme = new ThemeProvider { ContainerControl = this };
      toaster = new Toaster { ContainerControl = this };
      Samples.Toaster = toaster;

      // Top: the theme, set with the kit's own controls, on the page
      // color so the Segmented's track shows against it.
      var top = new Panel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(16, 12, 16, 12) };
      top.Paint += (_, e) => { using var pen = new Pen(KitTheme.Palette.Line); e.Graphics.DrawLine(pen, 0, top.Height - 1, top.Width, top.Height - 1); };
      var title = new Label { Text = "Kit gallery", Dock = DockStyle.Left, Width = 180, TextAlign = ContentAlignment.MiddleLeft, Font = KitTheme.Font(16, true) };
      var mode = modeChoice;
      var accent = new Select { Options = Enum.GetNames<Accent>(), SelectedIndex = 0, Dock = DockStyle.Right, Width = 140 };
      var density = new Segmented { Options = new[] { "Comfortable", "Compact" }, SelectedIndex = 0, Dock = DockStyle.Right };
      mode.SelectedIndexChanged += (_, _) => theme.Mode = (ThemeMode)mode.SelectedIndex;
      accent.SelectedIndexChanged += (_, _) => theme.Accent = (Accent)accent.SelectedIndex;
      density.SelectedIndexChanged += (_, _) => theme.Density = (Density)density.SelectedIndex;
      top.Controls.Add(title);
      top.Controls.Add(Spacer());
      top.Controls.Add(mode);
      top.Controls.Add(Spacer());
      top.Controls.Add(accent);
      top.Controls.Add(Spacer());
      top.Controls.Add(density);

      // Left: the components.
      // 12 px at the bottom, so the list ends on the code box's line.
      var left = new Panel { Dock = DockStyle.Left, Width = 248, Padding = new Padding(8, 8, 8, 12) };
      navScroll.Controls.Add(nav);
      left.Controls.Add(navScroll);
      left.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8 });
      left.Controls.Add(search);
      search.TextChanged += (_, _) => Fill();
      nav.SelectedIndexChanged += (_, _) => {
        if (nav.SelectedIndex < 0 || nav.SelectedIndex >= shown.Count) return;
        Open(shown[nav.SelectedIndex]);
        // Keep the chosen row in view, as the arrow keys move it.
        var row = nav.RowBounds(nav.SelectedIndex);
        var top = -navScroll.AutoScrollPosition.Y;
        if (row.Top < top) navScroll.AutoScrollPosition = new Point(0, row.Top);
        else if (row.Bottom > top + navScroll.ClientSize.Height) navScroll.AutoScrollPosition = new Point(0, row.Bottom - navScroll.ClientSize.Height);
      };

      // Right: the properties.
      var right = new Panel { Dock = DockStyle.Right, Width = 360, Padding = new Padding(12, 12, 12, 12) };
      right.Controls.Add(grid);
      right.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8 });
      right.Controls.Add(kitOnly);
      kitOnly.CheckedChanged += (_, _) => Inspect();
      grid.PropertyValueChanged += (_, _) => { Arrange(); WriteCode(); };

      // Middle: the stage and the code.
      // 12 px at the bottom, as the properties column has, so the code
      // box and the property description end on one line.
      var middle = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 12, 24, 12) };
      var codePanel = new Panel { Dock = DockStyle.Bottom, Height = 190, Padding = new Padding(12, 10, 12, 10) };
      var copy = new KitButton { Text = "Copy", Icon = "copy", Variant = ButtonVariant.Ghost, ControlSize = ControlSize.Small, AutoSize = true, Dock = DockStyle.Right };
      copy.Click += (_, _) => { if (code.TextLength > 0) { Clipboard.SetText(code.Text); toaster.Show("Code copied"); } };
      var codeHead = new Panel { Dock = DockStyle.Top, Height = 32 };
      codeHead.Controls.Add(new Label { Text = "C#", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = KitTheme.Font(13, true) });
      codeHead.Controls.Add(copy);
      codePanel.Controls.Add(code);
      codePanel.Controls.Add(codeHead);
      middle.Controls.Add(stage);
      middle.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 12 });
      middle.Controls.Add(codePanel);
      middle.Controls.Add(summary);
      middle.Controls.Add(heading);
      stage.Resize += (_, _) => Arrange();

      Controls.Add(middle);
      Controls.Add(right);
      Controls.Add(left);
      Controls.Add(top);

      // Every size above is in 96 DPI pixels; this scales the window, its
      // panels and their padding to the screen in one step. The kit's
      // controls size themselves to the screen already.
      if (DeviceDpi != 96) Scale(new SizeF(DeviceDpi / 96f, DeviceDpi / 96f));
      MinimumSize = new Size(LogicalToDeviceUnits(1100), LogicalToDeviceUnits(640));

      KitTheme.Changed += (_, _) => Recolor(top, left, right, codePanel, codeHead);
      Recolor(top, left, right, codePanel, codeHead);
      code.SizeChanged += (_, _) => FitCodeScroll();
      Fill();
    }

    // For --shots, the window keeps its full size even on a screen smaller
    // than it, so every capture is the same size. Windows otherwise caps a
    // window at the screen's size.
    protected override void WndProc(ref Message m) {
      base.WndProc(ref m);
      const int WM_GETMINMAXINFO = 0x0024;
      if (Quiet && m.Msg == WM_GETMINMAXINFO) {
        var info = System.Runtime.InteropServices.Marshal.PtrToStructure<MinMaxInfo>(m.LParam);
        info.MaxTrackX = info.MaxTrackY = 8000;
        info.MaxSizeX = info.MaxSizeY = 8000;
        System.Runtime.InteropServices.Marshal.StructureToPtr(info, m.LParam, false);
      }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct MinMaxInfo {
      public int ReservedX, ReservedY, MaxSizeX, MaxSizeY, MaxPositionX, MaxPositionY, MinTrackX, MinTrackY, MaxTrackX, MaxTrackY;
    }

    static Panel Spacer() => new() { Dock = DockStyle.Right, Width = 12 };

    // For --shots: shown without taking focus from whoever is at the machine.
    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool Quiet { get; set; }

    protected override bool ShowWithoutActivation => Quiet;

    public void SetMode(ThemeMode mode) => modeChoice.SelectedIndex = (int)mode;

    public void OpenAt(int index) {
      search.Text = "";
      nav.SelectedIndex = index;
      if (current != Samples.All[index]) Open(Samples.All[index]);
    }

    void Fill() {
      var q = search.Text.Trim();
      shown = Samples.All.Where(s => q.Length == 0 || s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || s.Group.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
      nav.Items.Clear();
      nav.Items.AddRange(shown.Select(s => new NavItem(s.Name, s.Icon, null, s.Name)));
      nav.Height = nav.GetPreferredSize(Size.Empty).Height;
      var index = current == null ? 0 : shown.IndexOf(current);
      nav.SelectedIndex = -1;
      nav.SelectedIndex = Math.Max(0, index);
      if (shown.Count > 0 && (current == null || !shown.Contains(current))) Open(shown[0]);
    }

    void Open(Sample sample) {
      current = sample;
      if (built != null) {
        stage.Controls.Remove(built.Stage);
        built.Stage.Dispose();
      }
      built = sample.Make();
      stage.Controls.Add(built.Stage);
      heading.Text = sample.Name;
      summary.Text = (built.Inspect.GetType().GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true).FirstOrDefault() as System.ComponentModel.DescriptionAttribute)?.Description ?? "";
      Arrange();
      Inspect();
    }

    void Inspect() {
      if (built == null) return;
      grid.SelectedObject = new PropertyView(built.Inspect, kitOnly.Checked);
      WriteCode();
    }

    void WriteCode() {
      if (built == null || current == null) return;
      var name = char.ToLowerInvariant(built.Inspect.GetType().Name[0]) + built.Inspect.GetType().Name[1..] + "1";
      code.Text = CodeWriter.For(built.Inspect, name).Replace("\n", Environment.NewLine);
      FitCodeScroll();
    }

    // A scrollbar only when the code is longer than its box.
    void FitCodeScroll() {
      var lines = Math.Max(1, code.GetLineFromCharIndex(code.TextLength) + 1);
      var fits = lines * code.Font.Height <= code.ClientSize.Height;
      var want = fits ? ScrollBars.None : ScrollBars.Vertical;
      if (code.ScrollBars != want) { code.ScrollBars = want; Painter.ThemeScrollbars(code); }
    }

    // The component sits at the left of the stage, on the same edge as
    // the heading and the code box, at its sample width.
    void Arrange() {
      if (built == null) return;
      var c = built.Stage;
      var width = Math.Min(c.LogicalToDeviceUnits(built.Width), stage.ClientSize.Width);
      if (c.Width != width && built.Width > 0 && c is not (Avatar or KitIcon)) c.Width = Math.Max(width, c.Width < width ? c.Width : width);
      c.Location = new Point(0, stage.LogicalToDeviceUnits(16));
    }

    void Recolor(Control top, params Control[] areas) {
      var p = KitTheme.Palette;
      foreach (var a in areas) a.BackColor = p.BgSubtle;
      top.BackColor = p.Bg;
      top.Invalidate();
      stage.BackColor = p.Bg;
      heading.Invalidate();
      summary.Invalidate();
      code.BackColor = p.BgSubtle;
      code.ForeColor = p.Ink;
      code.Font = KitTheme.Font(13, mono: true);
      grid.BackColor = p.BgSubtle;
      grid.ViewBackColor = p.Bg;
      grid.ViewForeColor = p.Ink;
      grid.LineColor = p.BgSubtle;
      grid.CategoryForeColor = p.Ink2;
      grid.CategorySplitterColor = p.Line;
      grid.HelpBackColor = p.BgSubtle;
      grid.HelpForeColor = p.Ink2;
      grid.HelpBorderColor = p.Line;
      grid.ViewBorderColor = p.Line;
      grid.SelectedItemWithFocusBackColor = p.AccentSoft;
      grid.SelectedItemWithFocusForeColor = p.Ink;
      foreach (Control a in areas.Append(top))
        foreach (Control child in a.Controls)
          if (child is Label l) l.ForeColor = p.Ink;
      Painter.ThemeScrollbars(code);
      Painter.ThemeScrollbars(grid);
      Painter.ThemeScrollbars(navScroll);
    }
  }

  // A line of text drawn with no inset, so its letters start on the same
  // edge as the boxes beside it; it wraps when it is longer than its width.
  internal sealed class Caption : Control {
    public Caption() {
      SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
          | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
      BackColor = Color.Transparent;
    }

    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public float Px { get; set; } = 14;

    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool Strong { get; set; }

    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool Quiet { get; set; }

    // As tall as its wrapped text, plus a gap under it; Height in the
    // declaration is only the starting value.
    void Fit() {
      if (Width <= 0) return;
      var font = KitTheme.Font(Px, Strong);
      var h = Math.Max(font.Height, Painter.MeasureHeight(Text, font, Width)) + LogicalToDeviceUnits(Strong ? 8 : 16);
      if (Height != h) Height = h;
    }

    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Fit(); Invalidate(); }

    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); Fit(); }

    protected override void OnPaint(PaintEventArgs e) {
      var p = KitTheme.Palette;
      Painter.Wrapped(e.Graphics, Text, KitTheme.Font(Px, Strong), ClientRectangle, Quiet ? p.Ink2 : p.Ink,
          TextFormatFlags.Left | TextFormatFlags.Top);
    }
  }
}
