namespace Obille_Try.Pages {
  // Stands in for the screens the sketches do not draw.
  public partial class PlaceholderPage : UI.Page {
    public PlaceholderPage() {
      InitializeComponent();
      emptyState.ActionClick += (_, _) => DashboardRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? DashboardRequested;

    public void ShowFor(string title, string icon) {
      pageHeader.Title = title;
      emptyState.Icon = icon;
      emptyState.Title = $"{title} is not in this prototype";
    }
  }
}
