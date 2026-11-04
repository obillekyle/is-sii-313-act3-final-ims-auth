using Kit;

namespace Obille_Try.Dialogs {
  // Asks before Sign out closes the main window. A header and the two
  // buttons, so the dialog is as tall as those two.
  public partial class SignOutDialog : KitDialog {
    public SignOutDialog() {
      InitializeComponent();
      ClientSize = new Size(ClientSize.Width, HeaderHeight + footer.Height);
    }
  }
}
