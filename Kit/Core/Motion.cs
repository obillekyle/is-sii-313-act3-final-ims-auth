using System.Runtime.InteropServices;

namespace Kit {
  // The kit's motion: Fast 100 ms for hovers, Base 160 ms for things that come
  // and go (a dialog, a toast), Slow 300 ms for things that change size, all
  // on Ease, cubic-bezier(0.2, 0, 0, 1). Off when Windows' animation effects
  // are off (Settings, Accessibility, Visual effects): every duration is then
  // 0 and a change lands at once.
  public static class Motion {
    public const int Fast = 100;
    public const int Base = 160;
    public const int Slow = 300;

    [DllImport("user32.dll")]
    static extern bool SystemParametersInfo(uint action, uint param, out bool value, uint winIni);

    // SPI_GETCLIENTAREAANIMATION: the switch behind "Animation effects".
    public static bool Enabled {
      get {
        if (Override is { } o) return o;
        return !SystemParametersInfo(0x1042, 0, out var on, 0) || on;
      }
    }

    // For a test or a screenshot that needs motion on or off whatever
    // Windows says; null follows Windows.
    public static bool? Override { get; set; }

    // cubic-bezier(0.2, 0, 0, 1): x(s) is solved for s by
    // Newton's method from the time, then y(s) is the progress.
    public static float Ease(float t) {
      if (t <= 0) return 0;
      if (t >= 1) return 1;
      const float x1 = 0.2f, y1 = 0f, x2 = 0f, y2 = 1f;
      static float Bez(float s, float a, float b) => 3 * a * s * (1 - s) * (1 - s) + 3 * b * s * s * (1 - s) + s * s * s;
      static float Slope(float s, float a, float b) => 3 * a * (1 - s) * (1 - s) + 6 * (b - a) * s * (1 - s) + 3 * (1 - b) * s * s;
      var u = t;
      for (var i = 0; i < 8; i++) {
        var dx = Bez(u, x1, x2) - t;
        if (Math.Abs(dx) < 1e-5f) break;
        var d = Slope(u, x1, x2);
        if (Math.Abs(d) < 1e-6f) break;
        u = Math.Clamp(u - dx / d, 0, 1);
      }
      return Bez(u, y1, y2);
    }

    public static Color Lerp(Color a, Color b, float t) {
      // An empty color is the other one at no alpha, so a ghost
      // button's fill fades in from nothing rather than from black.
      if (a.IsEmpty) a = Color.FromArgb(0, b);
      if (b.IsEmpty) b = Color.FromArgb(0, a);
      t = Math.Clamp(t, 0, 1);
      int L(int x, int y) => (int)Math.Round(x + (y - x) * t);
      return Color.FromArgb(L(a.A, b.A), L(a.R, b.R), L(a.G, b.G), L(a.B, b.B));
    }

    // One timer for every running animation, ticking about 60 times a
    // second while any runs and stopped when none does.
    static readonly System.Windows.Forms.Timer timer = new() { Interval = 15 };
    static readonly List<Func<bool>> running = new();

    static Motion() => timer.Tick += (_, _) => Step();

    static void Step() {
      for (var i = running.Count - 1; i >= 0; i--) {
        bool more;
        try { more = running[i](); } catch (ObjectDisposedException) { more = false; }
        if (!more) running.RemoveAt(i);
      }
      if (running.Count == 0) timer.Stop();
    }

    // Runs step with the eased progress from 0 to 1 over ms, the last call
    // at exactly 1, then done. With motion off, one call at 1.
    public static void Run(int ms, Action<float> step, Action? done = null) {
      if (!Enabled || ms <= 0) {
        step(1);
        done?.Invoke();
        return;
      }
      // The clock starts at the first frame, not here: a dialog asks for
      // its fade and then spends about 380 ms on its first paint, and a
      // clock started at the ask found the fade over at its first frame.
      var start = -1L;
      step(0);
      running.Add(() => {
        var now = Environment.TickCount64;
        if (start < 0) start = now;
        var t = Math.Min(1f, (now - start) / (float)ms);
        step(Ease(t));
        if (t < 1) return true;
        done?.Invoke();
        return false;
      });
      timer.Start();
    }

    internal static void Keep(Func<bool> tick) {
      running.Add(tick);
      timer.Start();
    }
  }

  // Hover fades for one control: each key (0 for the whole control, or a
  // row, an option, a page) eases toward on or off over Motion.Fast, and the
  // control repaints while any moves. Paint mixes with Mix(key, off, on).
  public sealed class HoverFade {
    readonly Control owner;
    readonly int ms;

    // Each key moves from where it was when last set toward its target,
    // eased over the whole duration from that moment, as a CSS transition
    // restarts from the current value. Easing a running value in both
    // directions instead made a fade out start slow and end in a drop.
    readonly Dictionary<int, (float From, float To, long Start)> keys = new();
    bool ticking;

    public HoverFade(Control owner, int ms = Motion.Fast) {
      this.owner = owner;
      this.ms = ms;
    }

    public void Set(int key, bool on) {
      var to = on ? 1f : 0f;
      var now = Environment.TickCount64;
      if (keys.TryGetValue(key, out var k) && k.To == to) return;
      if (!keys.ContainsKey(key) && !on) return;
      var from = keys.ContainsKey(key) ? Value(k, now) : 0f;
      keys[key] = Motion.Enabled ? (from, to, now) : (to, to, now);
      owner.Invalidate();
      if (ticking || !Motion.Enabled) return;
      ticking = true;
      Motion.Keep(Tick);
    }

    // Every key off, as when the pointer leaves the control.
    public void Clear() {
      foreach (var key in keys.Keys.ToList()) Set(key, false);
    }

    float Value((float From, float To, long Start) k, long now) {
      var t = Math.Min(1f, (now - k.Start) / (float)ms);
      return k.From + (k.To - k.From) * Motion.Ease(t);
    }

    bool Tick() {
      if (owner.IsDisposed) { ticking = false; return false; }
      var now = Environment.TickCount64;
      var moving = keys.Values.Any(k => now - k.Start < ms && k.From != k.To);
      owner.Invalidate();
      ticking = moving;
      return moving;
    }

    // A key's progress, 0 off and 1 on.
    public float this[int key] => keys.TryGetValue(key, out var k) ? Value(k, Environment.TickCount64) : 0f;

    public Color Mix(int key, Color off, Color on) => Motion.Lerp(off, on, this[key]);
  }
}
