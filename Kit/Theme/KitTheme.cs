using Microsoft.Win32;

namespace Kit {
  public enum ThemeMode { System, Light, Dark }

  public enum Accent { Blue, Violet, Green, Orange, Ink }

  public enum Density { Comfortable, Compact }

  public enum Tone { Neutral, Info, Success, Warning, Danger, Accent }

  public enum ControlSize { Small, Medium, Large }

  // The design tokens, one set per mode. Every
  // control paints from these and none of them knows which theme is on.
  public sealed class Palette {
    public bool Dark { get; init; }
    public Color Bg { get; init; }
    public Color BgSubtle { get; init; }
    public Color BgHover { get; init; }
    public Color BgPress { get; init; }
    public Color Line { get; init; }
    public Color LineStrong { get; init; }
    public Color Ink { get; init; }
    public Color Ink2 { get; init; }
    public Color Ink3 { get; init; }
    public Color InkDisabled { get; init; }
    public Color Inverse { get; init; }
    public Color InverseInk { get; init; }
    public Color Accent { get; init; }
    public Color AccentInk { get; init; }
    public Color AccentSoft { get; init; }
    public Color Selection { get; init; }
    public Color Ok { get; init; }
    public Color OkSoft { get; init; }
    public Color Warn { get; init; }
    public Color WarnSoft { get; init; }
    public Color Bad { get; init; }
    public Color BadSoft { get; init; }
    public Color[] Series { get; init; } = Array.Empty<Color>();
    public Color SeriesMuted { get; init; }
    public Color[] Sequential { get; init; } = Array.Empty<Color>();
    public Color Grid { get; init; }
    public Color Axis { get; init; }
    public Color Scrim { get; init; }

    // A tone's text color and its soft fill.
    public (Color Ink, Color Soft) ToneColors(Tone tone) => tone switch {
      Tone.Info => (AccentInk, AccentSoft),
      Tone.Success => (Ok, OkSoft),
      Tone.Warning => (Warn, WarnSoft),
      Tone.Danger => (Bad, BadSoft),
      Tone.Accent => (AccentInk, AccentSoft),
      _ => (Ink2, BgHover),
    };

    static Color Hex(string h) => ColorTranslator.FromHtml(h);

    // color-mix(in srgb, a p%, b)
    public static Color Mix(Color a, double p, Color b) => Color.FromArgb(
        (int)Math.Round(a.R * p + b.R * (1 - p)),
        (int)Math.Round(a.G * p + b.G * (1 - p)),
        (int)Math.Round(a.B * p + b.B * (1 - p)));

    // Each accent carries its fill and a text value chosen for 4.5:1 on the
    // page, per mode.
    static readonly Dictionary<Kit.Accent, (string Fill, string Ink, string DarkFill, string DarkInk)> accents = new() {
      [Kit.Accent.Blue] = ("#0d99ff", "#0768b8", "#0d99ff", "#6cbdff"),
      [Kit.Accent.Violet] = ("#9747ff", "#6a1fd1", "#9747ff", "#c39bff"),
      [Kit.Accent.Green] = ("#14ae5c", "#0b6b38", "#14ae5c", "#5fd394"),
      [Kit.Accent.Orange] = ("#f24e1e", "#b8360d", "#f24e1e", "#ff8a66"),
      [Kit.Accent.Ink] = ("#0f0f0f", "#0f0f0f", "#f2f2f2", "#f2f2f2"),
    };

    public static Palette Create(bool dark, Kit.Accent accent) {
      var a = accents[accent];
      var fill = Hex(dark ? a.DarkFill : a.Fill);
      var ink = Hex(dark ? a.DarkInk : a.Ink);
      if (!dark) {
        var bg = Hex("#ffffff");
        return new Palette {
          Dark = false,
          Bg = bg,
          BgSubtle = Hex("#f5f5f5"),
          BgHover = Hex("#ebebeb"),
          BgPress = Hex("#e0e0e0"),
          Line = Hex("#e6e6e6"),
          LineStrong = Hex("#8a8a8a"),
          Ink = Hex("#0f0f0f"),
          Ink2 = Hex("#5c5c5c"),
          Ink3 = Hex("#6e6e6e"),
          InkDisabled = Hex("#a3a3a3"),
          Inverse = Hex("#0f0f0f"),
          InverseInk = Hex("#ffffff"),
          Accent = fill,
          AccentInk = ink,
          AccentSoft = Mix(fill, 0.12, bg),
          Selection = Color.FromArgb(56, fill),
          Ok = Hex("#0b7a3e"),
          OkSoft = Hex("#e3f6ea"),
          Warn = Hex("#8a5a00"),
          WarnSoft = Hex("#fff4d1"),
          Bad = Hex("#c4320a"),
          BadSoft = Hex("#fde8e2"),
          Series = new[] { "#2a78d6", "#eb6834", "#1baf7a", "#eda100", "#e87ba4", "#008300", "#4a3aa7", "#e34948" }.Select(Hex).ToArray(),
          SeriesMuted = Hex("#c4c4c4"),
          Sequential = new[] { "#e3eefc", "#b7d3f6", "#6da7ec", "#2a78d6", "#184f95" }.Select(Hex).ToArray(),
          Grid = Hex("#ececec"),
          Axis = Hex("#c4c4c4"),
          Scrim = Color.FromArgb(82, 0, 0, 0),
        };
      }
      var dbg = Hex("#1e1e1e");
      return new Palette {
        Dark = true,
        Bg = dbg,
        BgSubtle = Hex("#2c2c2c"),
        BgHover = Hex("#383838"),
        BgPress = Hex("#444444"),
        Line = Hex("#3a3a3a"),
        LineStrong = Hex("#7a7a7a"),
        Ink = Hex("#f2f2f2"),
        Ink2 = Hex("#b8b8b8"),
        Ink3 = Hex("#999999"),
        InkDisabled = Hex("#666666"),
        Inverse = Hex("#f2f2f2"),
        InverseInk = Hex("#111111"),
        Accent = fill,
        AccentInk = ink,
        AccentSoft = Mix(fill, 0.12, dbg),
        Selection = Color.FromArgb(56, fill),
        Ok = Hex("#5fd394"),
        OkSoft = Hex("#173a26"),
        Warn = Hex("#f5c451"),
        WarnSoft = Hex("#3d3014"),
        Bad = Hex("#ff8a66"),
        BadSoft = Hex("#45201a"),
        Series = new[] { "#3987e5", "#d95926", "#199e70", "#c98500", "#d55181", "#008300", "#9085e9", "#e66767" }.Select(Hex).ToArray(),
        SeriesMuted = Hex("#555555"),
        Sequential = new[] { "#1a2f4a", "#1c5cab", "#3987e5", "#86b6ef", "#cde2fb" }.Select(Hex).ToArray(),
        Grid = Hex("#2e2e2e"),
        Axis = Hex("#4a4a4a"),
        Scrim = Color.FromArgb(140, 0, 0, 0),
      };
    }
  }

  // Sizes in logical pixels: control heights and the row
  // height by density, radii by role, the 4 px space unit, the type scale.
  public sealed class Metrics {
    public int HeightSmall { get; init; }
    public int HeightMedium { get; init; }
    public int HeightLarge { get; init; }
    public int Row { get; init; }
    public int RadiusTag => 4;
    public int RadiusControl => 6;
    public int RadiusPanel => 8;
    public int RadiusDialog => 12;
    public int Space(int steps) => steps * 4;

    public int Height(ControlSize size) => size switch {
      ControlSize.Small => HeightSmall,
      ControlSize.Large => HeightLarge,
      _ => HeightMedium,
    };

    public static Metrics Create(Density density) => density == Density.Compact
        ? new Metrics { HeightSmall = 24, HeightMedium = 28, HeightLarge = 36, Row = 32 }
        : new Metrics { HeightSmall = 28, HeightMedium = 32, HeightLarge = 40, Row = 40 };
  }

  // The theme in force for every kit control in the process: a mode, an
  // accent, a density. A ThemeProvider on a form sets it; code can
  // call Set directly. Controls repaint on Changed.
  public static class KitTheme {
    static readonly Dictionary<(float, bool, bool), Font> fonts = new();
    static bool systemHooked;

    public static ThemeMode Mode { get; private set; } = ThemeMode.System;
    public static Accent Accent { get; private set; } = Accent.Blue;
    public static Density Density { get; private set; } = Density.Comfortable;
    public static string FontFamily { get; private set; } = "Segoe UI";

    public static Palette Palette { get; private set; } = Palette.Create(SystemIsDark(), Accent.Blue);
    public static Metrics Metrics { get; private set; } = Metrics.Create(Density.Comfortable);

    public static event EventHandler? Changed;

    public static bool IsDark => Palette.Dark;

    public static void Set(ThemeMode mode, Accent accent, Density density) {
      if (mode == Mode && accent == Accent && density == Density) return;
      Mode = mode;
      Accent = accent;
      Density = density;
      Apply();
    }

    public static void SetFont(string family) {
      if (string.IsNullOrWhiteSpace(family) || family == FontFamily) return;
      FontFamily = family;
      lock (fonts) fonts.Clear();
      Changed?.Invoke(null, EventArgs.Empty);
    }

    static void Apply() {
      Palette = Palette.Create(Mode == ThemeMode.Dark || (Mode == ThemeMode.System && SystemIsDark()), Accent);
      Metrics = Metrics.Create(Density);
      if (Mode == ThemeMode.System && !systemHooked) {
        // Follows Windows when it switches between light and dark.
        SystemEvents.UserPreferenceChanged += (_, e) => {
          if (e.Category == UserPreferenceCategory.General && Mode == ThemeMode.System) Apply();
        };
        systemHooked = true;
      }
      Changed?.Invoke(null, EventArgs.Empty);
    }

    public static bool SystemIsDark() {
      try {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
      } catch (System.Security.SecurityException) {
        return false;
      }
    }

    // The type scale is in px (12, 13, 14, 16, 20, 24); fonts are made in
    // points so they follow the screen's scaling like any other font.
    public static Font Font(float px, bool strong = false, bool mono = false) {
      lock (fonts) {
        if (fonts.TryGetValue((px, strong, mono), out var f)) return f;
        var family = mono ? "Cascadia Mono" : strong ? FontFamily + " Semibold" : FontFamily;
        if (!System.Drawing.FontFamily.Families.Any(x => x.Name == family))
          family = mono ? "Consolas" : FontFamily;
        f = new Font(family, px * 0.75f, strong && !family.EndsWith("Semibold") ? FontStyle.Bold : FontStyle.Regular);
        fonts[(px, strong, mono)] = f;
        return f;
      }
    }
  }
}
