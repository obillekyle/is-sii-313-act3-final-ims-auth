using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace Kit {
  // The kit's config on a form: drop one from the Toolbox and set its mode,
  // accent and density in the Properties window. Every kit control, in the
  // designer and at run time, repaints to match. With StyleForm on, the
  // form's own background, text and title bar follow the mode too.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Mode))]
  [Description("Sets the kit's theme for every kit control: light, dark or the system's; an accent; a density.")]
  public sealed class ThemeProvider : Component, ISupportInitialize {
    ThemeMode mode = ThemeMode.System;
    Accent accent = Accent.Blue;
    Density density = Density.Comfortable;
    string fontFamily = "Segoe UI";
    bool styleForm = true;
    bool initializing;
    ContainerControl? container;

    public ThemeProvider() => KitTheme.Changed += OnThemeChanged;

    public ThemeProvider(IContainer container) : this() => container.Add(this);

    [Category("Theme"), DefaultValue(ThemeMode.System)]
    [Description("Light, dark, or whatever Windows is set to. System follows Windows when it switches.")]
    public ThemeMode Mode { get => mode; set { mode = value; Apply(); } }

    [Category("Theme"), DefaultValue(Accent.Blue)]
    [Description("The color of the current selection, focus rings and links. The primary button stays black on white, or white on black.")]
    public Accent Accent { get => accent; set { accent = value; Apply(); } }

    [Category("Theme"), DefaultValue(Density.Comfortable)]
    [Description("Comfortable: controls 28, 32 and 40 px tall, rows 40. Compact: 24, 28 and 36, rows 32.")]
    public Density Density { get => density; set { density = value; Apply(); } }

    [Category("Theme"), DefaultValue("Segoe UI")]
    [Description("The typeface every kit control writes in. DM Sans works when it is installed.")]
    public string FontFamily { get => fontFamily; set { fontFamily = value; Apply(); } }

    [Category("Behavior"), DefaultValue(true)]
    [Description("Sets the form's background, text color and title bar to the mode.")]
    public bool StyleForm { get => styleForm; set { styleForm = value; Apply(); } }

    // The form it styles. Set by the designer when the component is dropped
    // on a form, as ErrorProvider does.
    [Category("Behavior"), DefaultValue(null)]
    [Description("The form whose background and title bar follow the theme.")]
    public ContainerControl? ContainerControl {
      get => container;
      set { container = value; Apply(); }
    }

    public override ISite? Site {
      get => base.Site;
      set {
        base.Site = value;
        if (value?.GetService(typeof(IDesignerHost)) is IDesignerHost host && host.RootComponent is ContainerControl root)
          container = root;
      }
    }

    void ISupportInitialize.BeginInit() => initializing = true;

    void ISupportInitialize.EndInit() {
      initializing = false;
      Apply();
    }

    void Apply() {
      if (initializing) return;
      KitTheme.SetFont(fontFamily);
      KitTheme.Set(mode, accent, density);
      StyleContainer();
    }

    void StyleContainer() {
      if (!styleForm || container == null) return;
      var p = KitTheme.Palette;
      container.BackColor = p.Bg;
      container.ForeColor = p.Ink;
      if (container is Form form) {
        if (form.IsHandleCreated) DarkTitleBar(form, p.Dark);
        else form.HandleCreated += (_, _) => DarkTitleBar(form, KitTheme.IsDark);
      }
    }

    void OnThemeChanged(object? sender, EventArgs e) => StyleContainer();

    protected override void Dispose(bool disposing) {
      if (disposing) KitTheme.Changed -= OnThemeChanged;
      base.Dispose(disposing);
    }

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    static void DarkTitleBar(Form form, bool dark) {
      var on = dark ? 1 : 0;
      _ = DwmSetWindowAttribute(form.Handle, 20, ref on, sizeof(int)); // DWMWA_USE_IMMERSIVE_DARK_MODE
    }
  }
}
