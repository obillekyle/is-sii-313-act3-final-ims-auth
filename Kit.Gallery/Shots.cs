using System.Runtime.InteropServices;

namespace Kit.Gallery {
  // Kit.Gallery.exe --shots <folder>: each component on a window of its
  // own, sized to it with 32 px around, in the light mode and then the dark
  // one, plus the gallery itself in each mode. Every window is shown off
  // screen without taking focus, and saved as it was really painted
  // (PrintWindow), so a repaint fault shows in the file as on the screen.
  // A window of its own per component keeps the shots full size on any
  // screen: Windows caps a window at roughly its screen's size, and the
  // gallery is larger than a small or portrait display.
  internal static class Shots {
    [DllImport("user32.dll")]
    static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

    sealed class Quiet : Form {
      protected override bool ShowWithoutActivation => true;
    }

    sealed class QuietDialog : KitDialog {
      protected override bool ShowWithoutActivation => true;
    }

    public static void Run(string dir) {
      Directory.CreateDirectory(dir);
      var log = new List<string>();
      using var gallery = new GalleryForm { Quiet = true, StartPosition = FormStartPosition.Manual, ShowInTaskbar = false };
      gallery.Location = new Point(-8000, 0);
      gallery.Show();
      // Form.SetBoundsCore caps a window at SystemInformation's
      // MaxWindowTrackSize, about the screen's size; SetWindowPos with
      // SWP_NOSENDCHANGING sets the full 1360 x 820 client past it.
      var full = new Size(1360 + gallery.Width - gallery.ClientSize.Width, 820 + gallery.Height - gallery.ClientSize.Height);
      // FRAMECHANGED redraws the frame at the new width; without it the
      // 1 px top border Windows draws over the client kept the capped
      // width's color up to x 935.
      SetWindowPos(gallery.Handle, IntPtr.Zero, -8000, 0, full.Width, full.Height, 0x0004 | 0x0010 | 0x0400 | 0x0020); // NOZORDER, NOACTIVATE, NOSENDCHANGING, FRAMECHANGED
      foreach (var mode in new[] { ThemeMode.Light, ThemeMode.Dark }) {
        gallery.SetMode(mode);
        gallery.OpenAt(0);
        Pump(300);
        var name = $"00-gallery-{mode.ToString().ToLowerInvariant()}.png";
        // The client area only: the title bar is Windows', and it kept
        // the capped width's paint past x 945 after the resize.
        Save(gallery, Path.Combine(dir, name), clientOnly: true, skipTop: 1);
        log.Add($"{name}: {gallery.ClientSize.Width}x{gallery.ClientSize.Height - 1}");

        for (var i = 0; i < Samples.All.Length; i++) {
          var sample = Samples.All[i];
          var built = sample.Make();
          // A frame of some kind: a borderless window off screen came
          // back from PrintWindow black where its controls were.
          using var host = new Quiet {
            FormBorderStyle = FormBorderStyle.FixedToolWindow,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            BackColor = KitTheme.Palette.Bg,
            Location = new Point(-8000, 0),
          };
          var c = built.Stage;
          if (!c.AutoSize && c is not (Avatar or KitIcon or Badge or Kbd or Checkbox or Segmented or RadioGroup or SplitButton)) c.Width = built.Width;
          c.Location = new Point(32, 32);
          host.Controls.Add(c);
          host.ClientSize = new Size(c.Width + 64, c.Height + 64);
          host.Show();
          host.Location = new Point(-8000, 0);
          Pump(200);
          host.ClientSize = new Size(c.Right + 32, c.Bottom + 32);
          host.Refresh();
          Pump(600);
          var file = $"{i + 1:D2}-{sample.Name}-{mode.ToString().ToLowerInvariant()}.png";
          Save(host, Path.Combine(dir, file), clientOnly: true);
          log.Add($"{file}: {host.Width}x{host.Height}");

          // The same component hovered, through a real repaint: the
          // state where a transparent button once drew over stale
          // pixels.
          if (mode == ThemeMode.Light) {
            typeof(Control).GetMethod("OnMouseEnter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(c, new object[] { EventArgs.Empty });
            c.Invalidate(true);
            c.Update();
            Pump(150);
            var hovered = $"{i + 1:D2}-{sample.Name}-hover.png";
            Save(host, Path.Combine(dir, hovered), clientOnly: true);
            log.Add($"{hovered}: {host.Width}x{host.Height}");
          }

          // What the overlay samples open, which the button on the
          // stage does not show by itself.
          var suffix = mode.ToString().ToLowerInvariant();
          if (built.Inspect is KitDialog settings) {
            using var d = Samples.FillDialog(new QuietDialog { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-8000, 0) }, settings);
            d.Show();
            d.Location = new Point(-8000, 0);
            Pump(400);
            var open = $"{i + 1:D2}-{sample.Name}-open-{suffix}.png";
            // Borderless, so DrawToBitmap: PrintWindow returns a
            // borderless window off screen as black.
            using (var bmp = new Bitmap(d.Width, d.Height)) {
              d.DrawToBitmap(bmp, new Rectangle(Point.Empty, d.Size));
              bmp.Save(Path.Combine(dir, open));
            }
            log.Add($"{open}: {d.Width}x{d.Height}");
            d.Close();
          }
          if (sample.Name == "Toaster") {
            using var stand = new Quiet {
              FormBorderStyle = FormBorderStyle.FixedToolWindow,
              ShowInTaskbar = false,
              StartPosition = FormStartPosition.Manual,
              BackColor = KitTheme.Palette.BgSubtle,
              Location = new Point(-8000, 0),
            };
            stand.ClientSize = new Size(stand.LogicalToDeviceUnits(520), stand.LogicalToDeviceUnits(200));
            var toasts = new Toaster { ContainerControl = stand };
            stand.Show();
            stand.Location = new Point(-8000, 0);
            toasts.Show("Galvanized steel bar added");
            toasts.Show("3 materials deleted", Tone.Success, "Undo", () => { });
            stand.Refresh();
            Pump(400);
            var open = $"{i + 1:D2}-{sample.Name}-open-{suffix}.png";
            Save(stand, Path.Combine(dir, open), clientOnly: true);
            log.Add($"{open}: {stand.Width}x{stand.Height}");
          }
        }
      }
      File.WriteAllLines(Path.Combine(dir, "shots.txt"), log);
    }

    static void Pump(int ms) {
      var until = DateTime.Now.AddMilliseconds(ms);
      while (DateTime.Now < until) {
        Application.DoEvents();
        Thread.Sleep(5);
      }
    }

    // skipTop drops rows from the top of the capture: Windows 11 draws the
    // window's 1 px top border over the client, and on the gallery, sized
    // past the screen, DWM composes that row in two colors split at the
    // capped width (x 935 here) whatever the frame is told to redraw.
    static void Save(Form form, string file, bool clientOnly = false, int skipTop = 0) {
      var size = clientOnly ? form.ClientSize : form.Size;
      using var bmp = new Bitmap(size.Width, size.Height);
      using (var g = Graphics.FromImage(bmp)) {
        var hdc = g.GetHdc();
        PrintWindow(form.Handle, hdc, clientOnly ? 3u : 2u); // PW_RENDERFULLCONTENT, with PW_CLIENTONLY
        g.ReleaseHdc(hdc);
      }
      if (skipTop == 0) { bmp.Save(file); return; }
      using var cut = bmp.Clone(new Rectangle(0, skipTop, size.Width, size.Height - skipTop), bmp.PixelFormat);
      cut.Save(file);
    }
  }
}
