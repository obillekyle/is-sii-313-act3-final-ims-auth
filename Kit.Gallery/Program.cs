namespace Kit.Gallery {
  internal static class Program {
    [STAThread]
    static void Main(string[] args) {
      if (args.Length == 2 && args[0] == "--shots") {
        // Rendered at 100% whatever the screen's scaling, so shots
        // taken on different displays compare pixel for pixel.
        Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Shots.Run(args[1]);
        return;
      }
      ApplicationConfiguration.Initialize();
      Application.Run(new GalleryForm());
    }
  }
}
