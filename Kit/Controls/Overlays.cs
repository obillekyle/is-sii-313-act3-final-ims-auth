using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace Kit {
  public enum DialogSize { Small, Medium, Large, ExtraLarge }

  // A dialog: its title and description at the top, a close
  // X when it can be dismissed, its buttons in a DialogFooter under a rule,
  // the page behind it under a scrim. Make a dialog by adding a form that
  // inherits KitDialog and placing its controls in the designer, a
  // DialogFooter among them; open it with ShowOver(owner). It opens whole,
  // with no blank frame first: it paints off screen (WS_EX_COMPOSITED),
  // has Windows 11's open animation off, and stays transparent until its
  // first paint is done.
  public class KitDialog : Form {
    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("user32.dll")]
    static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    string title = "Dialog", description = "";
    DialogSize size = DialogSize.Medium;
    bool dismissable = true, overClose;

    public KitDialog() {
      FormBorderStyle = FormBorderStyle.None;
      StartPosition = FormStartPosition.Manual;
      ShowInTaskbar = false;
      KeyPreview = true;
      DoubleBuffered = true;
      ApplySize();
      Arrange();
      KitTheme.Changed += OnThemeChanged;
      OnThemeChanged(null, EventArgs.Empty);
    }

    [Category("Appearance"), DefaultValue("Dialog")]
    [Description("The heading at the top. A confirmation names the action and the thing: Delete 3 materials?")]
    public string Title { get => title; set { title = value ?? ""; Text = title; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("A line under the title.")]
    public string Description { get => description; set { description = value ?? ""; Arrange(); Invalidate(); } }

    [Category("Layout"), DefaultValue(DialogSize.Medium)]
    [Description("The width: 400, 560, 800 or 960 px.")]
    public DialogSize DialogSize { get => size; set { size = value; ApplySize(); } }

    [Category("Behavior"), DefaultValue(true)]
    [Description("Lets Escape and a close button in the header close it.")]
    public bool Dismissable { get => dismissable; set { dismissable = value; Arrange(); Invalidate(); } }

    int S(int px) => LogicalToDeviceUnits(px);

    // The header: 16 px above the title, 20 px before it, the
    // title on a 24 px line, the description 2 px under it on a 20 px
    // line, 8 px below; the body's content keeps the 20 px left edge and
    // 12 px on the right, as the footer's buttons do.
    public int HeaderHeight => S(16) + S(24) + (description.Length > 0 ? S(2) + DescriptionHeight : 0) + S(8);

    // The title block stops 12 px short of the close button; the
    // description wraps inside it.
    int TextRight => dismissable ? CloseBox.Left - S(12) : Width - S(16);
    int DescriptionHeight => Math.Max(S(20), Painter.MeasureHeight(description, KitTheme.Font(14), TextRight - S(20)));

    // The small close button (28 px) centered on the title's line, 16 px
    // from the right edge.
    Rectangle CloseBox => new(Width - S(16) - S(28), S(16) + (S(24) - S(28)) / 2, S(28), S(28));

    int DialogWidth => S(size switch { DialogSize.Small => 400, DialogSize.Large => 800, DialogSize.ExtraLarge => 960, _ => 560 });

    void ApplySize() => ClientSize = new Size(DialogWidth, Math.Max(ClientSize.Height, S(120)));

    // The width is DialogSize's and nothing else's: Visual Studio's new
    // form writes ClientSize = 800 x 450 into InitializeComponent, which
    // runs after this constructor, and a drag in the designer sets it
    // too. The height stays free. The form has no border, so its width
    // is its client width.
    // ClientSize goes through both: SetClientSizeCore keeps the client
    // width it was given apart from the window's.
    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) =>
        base.SetBoundsCore(x, y, (specified & BoundsSpecified.Width) != 0 ? DialogWidth : width, height, specified);

    protected override void SetClientSizeCore(int x, int y) => base.SetClientSizeCore(DialogWidth, y);

    // The footer runs the dialog's full width, so it is laid out here rather
    // than docked inside the padding; the padding's bottom keeps every
    // other control above it.
    DialogFooter? Footer => Controls.OfType<DialogFooter>().FirstOrDefault();

    void Arrange() => Padding = new Padding(S(20), HeaderHeight, S(12), Footer?.Height ?? 0);

    protected override void OnControlAdded(ControlEventArgs e) {
      base.OnControlAdded(e);
      if (e.Control is DialogFooter f) f.SizeChanged += (_, _) => Arrange();
      Arrange();
      Invalidate();
    }

    protected override void OnLayout(LayoutEventArgs levent) {
      base.OnLayout(levent);
      // 1 px in from the sides and the bottom, inside the dialog's frame.
      if (Footer is { } f) f.SetBounds(1, ClientSize.Height - f.Height - 1, ClientSize.Width - 2, f.Height);
    }

    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); Arrange(); Invalidate(); }

    void OnThemeChanged(object? sender, EventArgs e) {
      if (IsDisposed) return;
      BackColor = KitTheme.Palette.Bg;
      ForeColor = KitTheme.Palette.Ink;
      Arrange();
      Invalidate();
    }

    protected override CreateParams CreateParams {
      get {
        var cp = base.CreateParams;
        cp.ClassStyle |= 0x20000; // CS_DROPSHADOW
        cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
        return cp;
      }
    }

    protected override void OnHandleCreated(EventArgs e) {
      base.OnHandleCreated(e);
      var round = 2; // DWMWCP_ROUND
      _ = DwmSetWindowAttribute(Handle, 33, ref round, sizeof(int));
      var off = 1; // no open animation
      _ = DwmSetWindowAttribute(Handle, 3, ref off, sizeof(int));
    }

    bool Designing => DesignMode || Site?.DesignMode == true;

    // Load comes on every ShowDialog, not only the first (Form resets its
    // CalledOnLoad there), and Shown follows it; a dialog kept and opened
    // again starts each open from here.
    protected override void OnLoad(EventArgs e) {
      fadingOut = faded = false;
      if (!Designing) Opacity = 0;
      base.OnLoad(e);
    }

    // Painted whole while still transparent, then faded in over Motion.Base.
    protected override void OnShown(EventArgs e) {
      base.OnShown(e);
      if (Designing) return;
      Refresh();
      // A close started before the fade ends takes over the opacity.
      Motion.Run(Motion.Base, t => { if (!IsDisposed && !fadingOut) Opacity = t; });
    }

    bool fadingOut, faded;

    // Closing fades out the same way: the close waits for the fade, then
    // goes ahead with the result it was asked for.
    protected override void OnFormClosing(FormClosingEventArgs e) {
      base.OnFormClosing(e);
      if (e.Cancel || faded || Designing || !Motion.Enabled || !Visible) return;
      e.Cancel = true;
      if (fadingOut) return;
      fadingOut = true;
      var result = DialogResult;
      var from = Opacity;
      Motion.Run(Motion.Base, t => { if (!IsDisposed) Opacity = from * (1 - t); }, () => {
        if (IsDisposed) return;
        faded = true;
        DialogResult = result;
        if (Modal) return; // setting DialogResult closes a modal dialog
        Close();
      });
    }

    protected override void OnKeyDown(KeyEventArgs e) {
      if (e.KeyCode == Keys.Escape && dismissable) {
        DialogResult = DialogResult.Cancel;
        e.Handled = true;
      }
      base.OnKeyDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      var over = dismissable && CloseBox.Contains(e.Location);
      if (over != overClose) { overClose = over; Invalidate(CloseBox); }
      base.OnMouseMove(e);
    }

    protected override void OnMouseDown(MouseEventArgs e) {
      base.OnMouseDown(e);
      if (e.Button != MouseButtons.Left || e.Y > HeaderHeight || CloseBox.Contains(e.Location)) return;
      ReleaseCapture();
      SendMessage(Handle, 0xA1, 2, IntPtr.Zero); // drag by the header
    }

    protected override void OnMouseClick(MouseEventArgs e) {
      if (dismissable && CloseBox.Contains(e.Location)) DialogResult = DialogResult.Cancel;
      base.OnMouseClick(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      base.OnPaint(e);
      var g = e.Graphics;
      var p = KitTheme.Palette;
      Painter.Smooth(g);
      using (var pen = new Pen(p.Line)) g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
      Painter.Text(g, title, KitTheme.Font(16, true), new Rectangle(S(20), S(16), TextRight - S(20), S(24)), p.Ink);
      if (description.Length > 0)
        Painter.Wrapped(g, description, KitTheme.Font(14), new Rectangle(S(20), S(42), TextRight - S(20), DescriptionHeight), p.Ink2);
      if (dismissable) {
        if (overClose) Painter.Fill(g, p.BgHover, CloseBox, S(4));
        Icons.Draw(g, "x", RectangleF.Inflate(CloseBox, -S(6), -S(6)), p.Ink2);
      }
    }

    // Opens the dialog over owner, with the window behind it under the
    // theme's scrim, and returns how it was closed.
    // The first dialog of a kind a process opens costs most of a second
    // here (measured 630 to 1500 ms against 110 to 150 for the next ones):
    // .NET compiles its code, and the kit's, the first time it runs. Warm
    // opens one off screen, transparent and without taking focus, once
    // the app is idle, and throws it away when it has painted, so the
    // first one a person opens costs what the rest do.
    bool warming;

    protected override bool ShowWithoutActivation => warming || base.ShowWithoutActivation;

    // Off for a test that measures the cold open.
    public static bool WarmUp { get; set; } = true;

    // With keep, the dialog is the one the app will open (a page's kept
    // dialog, read through make), so it is hidden after its paint rather
    // than thrown away. One a person has opened in the meantime is left
    // alone.
    public static void Warm(Func<KitDialog> make, bool keep = false) {
      if (!WarmUp) return;
      void Once(object? sender, EventArgs e) {
        Application.Idle -= Once;
        var d = make();
        if (d.Visible) return;
        d.warming = true;
        d.ShowInTaskbar = false;
        d.StartPosition = FormStartPosition.Manual;
        d.Location = new Point(-32000, -32000);
        d.Shown += Done;
        d.Show();

        void Done(object? s, EventArgs a) {
          d.Shown -= Done;
          d.BeginInvoke(() => {
            if (!keep) { d.Dispose(); return; }
            if (d.warming) d.Hide();
            d.warming = false;
          });
        }
      }
      Application.Idle += Once;
    }

    // A dialog can be kept and opened again: ShowDialog destroys its
    // window on close (Form.cs, .NET 10) but keeps the controls, so a
    // reopen skips building them. Measured on New material, median of 7:
    // a new dialog 120 to 130 ms to its first paint, the same one
    // reopened 47 to 55. The dialog puts its own fields back.
    public DialogResult ShowOver(Form owner) {
      // Asked for while the warm-up still has it open off screen.
      if (warming) { warming = false; Hide(); }

      // The shade fades in with the dialog and out after it.
      var scrim = KitTheme.Palette.Scrim.A / 255.0;
      var shade = new Form {
        FormBorderStyle = FormBorderStyle.None,
        BackColor = Color.Black,
        Opacity = 0,
        ShowInTaskbar = false,
        StartPosition = FormStartPosition.Manual,
        Bounds = owner.Bounds,
      };
      shade.Show(owner);
      Motion.Run(Motion.Base, t => { if (!shade.IsDisposed) shade.Opacity = scrim * t; });
      Location = new Point(owner.Left + (owner.Width - Width) / 2, owner.Top + Math.Max(0, (owner.Height - Height) / 2));
      var result = ShowDialog(shade);
      owner.Activate();
      var from = shade.Opacity;
      Motion.Run(Motion.Base, t => { if (!shade.IsDisposed) shade.Opacity = from * (1 - t); }, () => {
        shade.Close();
        shade.Dispose();
      });
      return result;
    }

    protected override void Dispose(bool disposing) {
      if (disposing) KitTheme.Changed -= OnThemeChanged;
      base.Dispose(disposing);
    }
  }

  // A dialog's button row: across the dialog's full
  // width at its bottom (the dialog places it), a rule along its top, the
  // buttons set right to left 12 px from the right edge with 8 px between
  // them, 12 px above and below. The first button added is the rightmost:
  // add the main action first, then Cancel.
  [ToolboxItem(true)]
  [Description("A dialog's row of buttons, set from the right under a rule.")]
  public class DialogFooter : FlowLayoutPanel {
    public DialogFooter() {
      SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
      FlowDirection = FlowDirection.RightToLeft;
      WrapContents = false;
      Height = LogicalToDeviceUnits(56);
      Padding = new Padding(LogicalToDeviceUnits(20), LogicalToDeviceUnits(12), LogicalToDeviceUnits(12), LogicalToDeviceUnits(12));
      BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e) {
      base.OnPaint(e);
      using var pen = new Pen(KitTheme.Palette.Line);
      e.Graphics.DrawLine(pen, 0, 0, Width, 0);
    }

    protected override void OnControlAdded(ControlEventArgs e) {
      base.OnControlAdded(e);
      if (e.Control == null) return;
      e.Control.Margin = new Padding(LogicalToDeviceUnits(8), 0, 0, 0);
      // The row is the tallest button plus its 12 px above and below.
      Height = Padding.Vertical + Math.Max(LogicalToDeviceUnits(32), e.Control.Height);
    }
  }

  // Where toasts appear: bottom right, stacked, each dark,
  // leaving on its own. Drop one on a form and call Show; an undo or other
  // action goes in the toast with its words.
  [ToolboxItem(true)]
  [Description("Shows toasts at the bottom right of a form: a message, an optional action, gone on their own.")]
  public sealed class Toaster : Component {
    ContainerControl? container;
    readonly List<ToastView> shown = new();

    public Toaster() { }

    public Toaster(IContainer container) : this() => container.Add(this);

    [Category("Behavior"), DefaultValue(null)]
    [Description("The form the toasts appear on. Set by the designer when the component is dropped on a form.")]
    public ContainerControl? ContainerControl { get => container; set => container = value; }

    [Category("Behavior"), DefaultValue(5000)]
    [Description("How long a toast stays, in milliseconds. One with an action stays twice as long.")]
    public int Duration { get; set; } = 5000;

    public override ISite? Site {
      get => base.Site;
      set {
        base.Site = value;
        if (value?.GetService(typeof(IDesignerHost)) is IDesignerHost host && host.RootComponent is ContainerControl root)
          container = root;
      }
    }

    public void Show(string message, Tone tone = Tone.Neutral, string? actionText = null, Action? action = null) {
      if (container == null) return;
      var toast = new ToastView(message, tone, actionText, action, actionText != null ? Duration * 2 : Duration);
      toast.Closing += () => Leave(toast);
      shown.Add(toast);
      container.Controls.Add(toast);
      toast.BringToFront();
      Restack(entering: toast);
    }

    // Every toast moves to its place in the stack over Motion.Base; a new
    // one comes in from 8 px below at no opacity.
    void Restack(ToastView? entering = null) {
      if (container == null) return;
      var s = container.LogicalToDeviceUnits(16);
      var y = container.ClientSize.Height - s;
      for (var i = shown.Count - 1; i >= 0; i--) {
        var t = shown[i];
        y -= t.Height;
        var target = new Point(container.ClientSize.Width - t.Width - s, y);
        t.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        if (t == entering) {
          t.Alpha = 0;
          t.Location = new Point(target.X, target.Y + t.LogicalToDeviceUnits(8));
        }
        Animate(t, target, t == entering ? 1f : t.Alpha);
        y -= container.LogicalToDeviceUnits(8);
      }
    }

    // A dismissed toast leaves 8 px toward its own edge, the right, fading
    // out; the others close up.
    void Leave(ToastView toast) {
      shown.Remove(toast);
      Restack();
      var to = new Point(toast.Left + toast.LogicalToDeviceUnits(8), toast.Top);
      Animate(toast, to, 0f, () => {
        toast.Parent?.Controls.Remove(toast);
        toast.Dispose();
      });
    }

    // Moves a toast from where it is to a place and an opacity. A newer
    // move started on the same toast takes over from this one.
    static void Animate(ToastView t, Point to, float alpha, Action? done = null) {
      var from = t.Location;
      var fromAlpha = t.Alpha;
      var run = ++t.Run;
      Motion.Run(Motion.Base, p => {
        if (t.IsDisposed || t.Run != run) return;
        t.Location = new Point(from.X + (int)Math.Round((to.X - from.X) * p), from.Y + (int)Math.Round((to.Y - from.Y) * p));
        t.Alpha = fromAlpha + (alpha - fromAlpha) * p;
      }, () => {
        if (!t.IsDisposed && t.Run == run) done?.Invoke();
      });
    }

    sealed class ToastView : KitControl {
      readonly string message, actionText;
      readonly Action? action;
      readonly Tone tone;
      readonly System.Windows.Forms.Timer timer;
      bool overAction, overClose, closing;
      float alpha = 1;

      public event Action? Closing;

      // The toast's opacity while it comes and goes; the newest move's
      // number, so an older one stops writing.
      [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public float Alpha { get => alpha; set { alpha = Math.Clamp(value, 0, 1); Invalidate(); } }
      public int Run;

      public ToastView(string message, Tone tone, string? actionText, Action? action, int ms) {
        this.message = message;
        this.tone = tone;
        this.actionText = actionText ?? "";
        this.action = action;
        timer = new System.Windows.Forms.Timer { Interval = Math.Max(1000, ms) };
        timer.Tick += (_, _) => Close();
        // The same parts OnPaint lays out: the 16 px lead and the tone
        // icon's 24, the message, 8 px, the action box, the close box.
        var lead = S(16) + (tone != Tone.Neutral ? S(24) : 0);
        var act = this.actionText.Length > 0 ? Painter.Measure(this.actionText, KitTheme.Font(14, true)).Width + S(16) : 0;
        var w = lead + Painter.Measure(message, KitTheme.Font(14)).Width + S(8) + act + S(36) + S(4);
        Size = new Size(Math.Min(w, S(420)), S(48));
        timer.Start();
      }

      Rectangle ActionBox => new(Width - S(36) - Painter.Measure(actionText, KitTheme.Font(14, true)).Width - S(16), S(8), Painter.Measure(actionText, KitTheme.Font(14, true)).Width + S(16), Height - S(16));
      Rectangle CloseBox => new(Width - S(36), S(10), S(28), S(28));

      // The toaster takes it out, after its leaving fade.
      void Close() {
        if (closing) return;
        closing = true;
        timer.Stop();
        Closing?.Invoke();
      }

      protected override void OnMouseMove(MouseEventArgs e) {
        var a = actionText.Length > 0 && ActionBox.Contains(e.Location);
        var c = CloseBox.Contains(e.Location);
        if (a != overAction || c != overClose) { overAction = a; overClose = c; Invalidate(); }
        Fade.Set(1, a);
        Fade.Set(2, c);
        Cursor = a || c ? Cursors.Hand : Cursors.Default;
        base.OnMouseMove(e);
      }

      protected override void OnMouseClick(MouseEventArgs e) {
        if (closing) return;
        if (actionText.Length > 0 && ActionBox.Contains(e.Location)) { action?.Invoke(); Close(); return; }
        if (CloseBox.Contains(e.Location)) { Close(); return; }
        base.OnMouseClick(e);
      }

      protected override void OnSizeChanged(EventArgs e) {
        base.OnSizeChanged(e);
        using var path = Painter.Round(new RectangleF(0, 0, Width, Height), S(M.RadiusPanel));
        Region = new Region(path);
      }

      // Opaque, it paints straight to the screen. While it fades, it
      // paints into a bitmap and draws that at its opacity: a child
      // control has no opacity of its own, and its transparent
      // background shows the page through.
      protected override void OnPaint(PaintEventArgs e) {
        if (alpha >= 1) { PaintToast(e.Graphics); return; }
        if (alpha <= 0) return;
        using var bmp = new Bitmap(Math.Max(1, Width), Math.Max(1, Height));
        using (var bg = Graphics.FromImage(bmp)) PaintToast(bg);
        using var attributes = new System.Drawing.Imaging.ImageAttributes();
        attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix { Matrix33 = alpha });
        e.Graphics.DrawImage(bmp, ClientRectangle, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
      }

      void PaintToast(Graphics g) {
        g.Clear(P.Inverse);
        Painter.Smooth(g);
        var x = S(16);
        if (tone != Tone.Neutral) {
          var icon = tone switch { Tone.Success => "circle-check", Tone.Warning => "triangle-alert", Tone.Danger => "circle-alert", _ => "info" };
          // The toast is drawn inverse, so its tone color is the
          // other mode's; info takes the accent.
          var color = tone is Tone.Info or Tone.Accent ? P.Accent : Palette.Create(!P.Dark, KitTheme.Accent).ToneColors(tone).Ink;
          Icons.Draw(g, icon, new RectangleF(x, (Height - S(16)) / 2f, S(16), S(16)), color);
          x += S(24);
        }
        var end = actionText.Length > 0 ? ActionBox.X - S(8) : CloseBox.X - S(8);
        Painter.Text(g, message, KitTheme.Font(14), new Rectangle(x, 0, end - x, Height), P.InverseInk);
        if (actionText.Length > 0) {
          if (Fade[1] > 0) Painter.Fill(g, Color.FromArgb((int)(40 * Fade[1]), P.InverseInk), ActionBox, S(M.RadiusControl));
          // Underlined: a word in the
          // message's color would not otherwise read as a button.
          var font = KitTheme.Font(14, true);
          Painter.Text(g, actionText, font, ActionBox, P.InverseInk, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
          var tw = Painter.Measure(actionText, font).Width;
          var ux = ActionBox.X + (ActionBox.Width - tw) / 2;
          var uy = ActionBox.Y + (ActionBox.Height + font.Height) / 2 + S(1);
          var smoothing = g.SmoothingMode;
          g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
          using (var pen = new Pen(P.InverseInk, Math.Max(1, S(1)))) g.DrawLine(pen, ux, uy, ux + tw - 1, uy);
          g.SmoothingMode = smoothing;
        }
        if (Fade[2] > 0) Painter.Fill(g, Color.FromArgb((int)(40 * Fade[2]), P.InverseInk), CloseBox, S(M.RadiusControl));
        Icons.Draw(g, "x", RectangleF.Inflate(CloseBox, -S(7), -S(7)), P.InverseInk);
      }

      protected override void Dispose(bool disposing) {
        if (disposing) timer.Dispose();
        base.Dispose(disposing);
      }
    }
  }
}
