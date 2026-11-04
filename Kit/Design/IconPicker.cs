using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace Kit {
  // The icon picker: a search box over a grid of every icon in the kit's
  // set. Typing narrows the grid by name; a click, or Enter on the one
  // highlighted, picks. Use it on a form, or let KitEditors.Install() put it
  // behind every icon property in a PropertyGrid.
  [ToolboxItem(true)]
  [DefaultEvent(nameof(IconPicked)), DefaultProperty(nameof(SelectedIcon))]
  [Description("A searchable grid of the kit's icons. Click one to pick it.")]
  public class IconPicker : UserControl {
    readonly TextInput searchField = new() { Type = InputType.Search, Placeholder = "Search icons", Dock = DockStyle.Top };
    readonly Grid grid;

    TextBox search => searchField.TextBox;
    readonly Label status = new() { Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft };
    string selected = "";

    public IconPicker() {
      grid = new Grid(this) { Dock = DockStyle.Fill };
      search.TextChanged += (_, _) => grid.Filter(search.Text);
      search.KeyDown += (_, e) => {
        switch (e.KeyCode) {
          case Keys.Down: grid.MoveBy(Grid.Columns); e.Handled = true; break;
          case Keys.Up: grid.MoveBy(-Grid.Columns); e.Handled = true; break;
          case Keys.Right when search.SelectionStart == search.TextLength: grid.MoveBy(1); e.Handled = true; break;
          case Keys.Left when search.SelectionStart == 0: grid.MoveBy(-1); e.Handled = true; break;
          case Keys.Enter: grid.PickCurrent(); e.Handled = e.SuppressKeyPress = true; break;
          case Keys.Escape: Canceled?.Invoke(this, EventArgs.Empty); e.Handled = e.SuppressKeyPress = true; break;
        }
      };
      Controls.Add(grid);
      Controls.Add(status);
      Controls.Add(new Panel { Dock = DockStyle.Top, Height = 6 });
      Controls.Add(searchField);
      Size = new Size(300, 320);
      KitTheme.Changed += OnThemeChanged;
      OnThemeChanged(null, EventArgs.Empty);
      grid.Filter("");
    }

    [Category("Data"), DefaultValue(""), IconName, TypeConverter(typeof(IconNameConverter))]
    [Description("The icon picked, by name.")]
    public string SelectedIcon {
      get => selected;
      set {
        selected = value ?? "";
        grid.Highlight(selected);
        UpdateStatus(selected);
      }
    }

    public event EventHandler? IconPicked;
    public event EventHandler? Canceled;

    internal void Pick(string name) {
      selected = name;
      UpdateStatus(name);
      IconPicked?.Invoke(this, EventArgs.Empty);
    }

    internal void UpdateStatus(string name) =>
        status.Text = name.Length > 0 ? name : $"{grid.Shown} of {Icons.Names.Count} icons";

    protected override void OnGotFocus(EventArgs e) {
      base.OnGotFocus(e);
      search.Focus();
    }

    void OnThemeChanged(object? sender, EventArgs e) {
      if (IsDisposed) return;
      var p = KitTheme.Palette;
      BackColor = p.Bg;
      status.BackColor = p.BgSubtle;
      status.ForeColor = p.Ink2;
      status.Font = KitTheme.Font(13);
      // The name starts 12 px in, on the search icon's edge.
      status.Padding = new Padding(LogicalToDeviceUnits(12), 0, LogicalToDeviceUnits(12), 0);
      Painter.ThemeScrollbars(grid);
      grid.Invalidate();
    }

    protected override void Dispose(bool disposing) {
      if (disposing) KitTheme.Changed -= OnThemeChanged;
      base.Dispose(disposing);
    }

    sealed class Grid : ScrollableControl {
      public const int Columns = 8;
      readonly IconPicker owner;
      List<string> names = new();
      int current = -1, hover = -1;

      public Grid(IconPicker owner) {
        this.owner = owner;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        AutoScroll = true;
      }

      public int Shown => names.Count;
      int Cell => Math.Max(LogicalToDeviceUnits(32), (ClientSize.Width - LogicalToDeviceUnits(8)) / Columns);

      public void Filter(string query) {
        var q = query.Trim();
        names = Icons.Names.Where(n => q.Length == 0 || n.Contains(q, StringComparison.OrdinalIgnoreCase)).OrderBy(n => n, StringComparer.Ordinal).ToList();
        current = names.Count > 0 ? Math.Max(0, names.IndexOf(owner.selected)) : -1;
        if (q.Length > 0 && names.Count > 0) current = 0;
        AutoScrollMinSize = new Size(0, (names.Count + Columns - 1) / Columns * Cell + LogicalToDeviceUnits(8));
        owner.UpdateStatus(q.Length > 0 || current < 0 ? "" : owner.selected);
        Invalidate();
      }

      public void Highlight(string name) {
        current = names.IndexOf(name);
        Reveal();
        Invalidate();
      }

      public void MoveBy(int by) {
        if (names.Count == 0) return;
        current = Math.Clamp(current < 0 ? 0 : current + by, 0, names.Count - 1);
        owner.UpdateStatus(names[current]);
        Reveal();
        Invalidate();
      }

      public void PickCurrent() {
        if (current >= 0 && current < names.Count) owner.Pick(names[current]);
      }

      Rectangle CellBounds(int i) {
        var pad = LogicalToDeviceUnits(4);
        return new Rectangle(pad + i % Columns * Cell, pad + i / Columns * Cell + AutoScrollPosition.Y, Cell, Cell);
      }

      void Reveal() {
        if (current < 0) return;
        var r = CellBounds(current);
        if (r.Top < 0) AutoScrollPosition = new Point(0, -AutoScrollPosition.Y + r.Top);
        else if (r.Bottom > ClientSize.Height) AutoScrollPosition = new Point(0, -AutoScrollPosition.Y + r.Bottom - ClientSize.Height);
      }

      int HitTest(Point p) {
        for (var i = 0; i < names.Count; i++) if (CellBounds(i).Contains(p)) return i;
        return -1;
      }

      protected override void OnMouseMove(MouseEventArgs e) {
        var h = HitTest(e.Location);
        if (h != hover) {
          hover = h;
          owner.UpdateStatus(h >= 0 ? names[h] : current >= 0 ? names[current] : "");
          Invalidate();
        }
        base.OnMouseMove(e);
      }

      protected override void OnMouseClick(MouseEventArgs e) {
        var i = HitTest(e.Location);
        if (i >= 0) { current = i; owner.Pick(names[i]); Invalidate(); }
        base.OnMouseClick(e);
      }

      // The chosen icon is scrolled into view once the grid has a size;
      // set before that, it sat marked far below the fold.
      protected override void OnSizeChanged(EventArgs e) {
        base.OnSizeChanged(e);
        AutoScrollMinSize = new Size(0, (names.Count + Columns - 1) / Columns * Cell + LogicalToDeviceUnits(8));
        Reveal();
        Invalidate();
      }

      protected override void OnScroll(ScrollEventArgs se) { base.OnScroll(se); Invalidate(); }
      protected override void OnMouseWheel(MouseEventArgs e) { base.OnMouseWheel(e); Invalidate(); }

      protected override void OnPaint(PaintEventArgs e) {
        var g = e.Graphics;
        var p = KitTheme.Palette;
        g.Clear(p.Bg);
        Painter.Smooth(g);
        var icon = LogicalToDeviceUnits(18);
        for (var i = 0; i < names.Count; i++) {
          var r = CellBounds(i);
          if (r.Bottom < 0 || r.Top > ClientSize.Height) continue;
          var inner = Rectangle.Inflate(r, -LogicalToDeviceUnits(2), -LogicalToDeviceUnits(2));
          if (i == current) Painter.Fill(g, p.AccentSoft, inner, LogicalToDeviceUnits(6));
          else if (i == hover) Painter.Fill(g, p.BgHover, inner, LogicalToDeviceUnits(6));
          Icons.Draw(g, names[i], new RectangleF(r.X + (r.Width - icon) / 2f, r.Y + (r.Height - icon) / 2f, icon, icon), i == current ? p.AccentInk : p.Ink);
        }
        if (names.Count == 0)
          Painter.Text(g, "No icon matches that name.", KitTheme.Font(13), new Rectangle(0, 0, Width, LogicalToDeviceUnits(40)), p.Ink2,
              TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
      }
    }
  }

  // The icon picker as a PropertyGrid editor: a dropdown holding an
  // IconPicker, and the icon itself drawn beside its name in the grid.
  public sealed class IconPickerEditor : UITypeEditor {
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context) => UITypeEditorEditStyle.DropDown;

    public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value) {
      if (provider.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService service) return value;
      using var picker = new IconPicker { SelectedIcon = value as string ?? "", Size = new Size(320, 340) };
      var chosen = value;
      picker.IconPicked += (_, _) => { chosen = picker.SelectedIcon; service.CloseDropDown(); };
      picker.Canceled += (_, _) => service.CloseDropDown();
      service.DropDownControl(picker);
      return chosen;
    }

    // No preview in the grid's own row: the grid frames a value's preview
    // in a small box, and an icon in it read as a ticked checkbox.
    public override bool GetPaintValueSupported(ITypeDescriptorContext? context) => false;
  }

  // Puts the visual icon picker behind every icon property of the kit's
  // types in a PropertyGrid at run time. Visual Studio's own Properties
  // window offers the names in a dropdown instead: a picker with its own
  // window would need Visual Studio's client and server designer packages.
  public static class KitEditors {
    static bool installed;

    public static void Install() {
      if (installed) return;
      installed = true;
      foreach (var type in typeof(KitEditors).Assembly.GetExportedTypes()) {
        if (!type.IsClass) continue;
        if (!type.GetProperties().Any(p => p.IsDefined(typeof(IconNameAttribute), true))) continue;
        TypeDescriptor.AddProvider(new Provider(TypeDescriptor.GetProvider(type)), type);
      }
    }

    sealed class Provider : TypeDescriptionProvider {
      public Provider(TypeDescriptionProvider parent) : base(parent) { }

      public override ICustomTypeDescriptor? GetTypeDescriptor(Type objectType, object? instance) =>
          new Descriptor(base.GetTypeDescriptor(objectType, instance));
    }

    sealed class Descriptor : CustomTypeDescriptor {
      public Descriptor(ICustomTypeDescriptor? parent) : base(parent) { }

      public override PropertyDescriptorCollection GetProperties() => Wrap(base.GetProperties());

      public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => Wrap(base.GetProperties(attributes));

      static PropertyDescriptorCollection Wrap(PropertyDescriptorCollection props) => new(
          props.Cast<PropertyDescriptor>()
              .Select(p => p.Attributes[typeof(IconNameAttribute)] != null ? new IconProperty(p) : p)
              .ToArray());
    }

    sealed class IconProperty : PropertyDescriptor {
      readonly PropertyDescriptor inner;

      public IconProperty(PropertyDescriptor inner) : base(inner) => this.inner = inner;

      public override object? GetEditor(Type editorBaseType) =>
          editorBaseType == typeof(UITypeEditor) ? new IconPickerEditor() : inner.GetEditor(editorBaseType);

      public override Type ComponentType => inner.ComponentType;
      public override bool IsReadOnly => inner.IsReadOnly;
      public override Type PropertyType => inner.PropertyType;
      public override bool CanResetValue(object component) => inner.CanResetValue(component);
      public override object? GetValue(object? component) => inner.GetValue(component);
      public override void ResetValue(object component) => inner.ResetValue(component);
      public override void SetValue(object? component, object? value) => inner.SetValue(component, value);
      public override bool ShouldSerializeValue(object component) => inner.ShouldSerializeValue(component);
      public override TypeConverter Converter => inner.Converter;
    }
  }
}
