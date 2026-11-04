using System.Data.OleDb;
using Kit;
using Obille_Try.Data;

namespace Obille_Try {
  // The first window: sign in against the Users table, or go to Register.
  // Login owns the message loop (Program.Main runs it), so it is hidden
  // while Dash or Register is open, never closed; closing either of those
  // by its window button ends the app, while Sign out brings this window
  // back, and Register's Sign in button brings it back where Register was.
  public partial class Login : Form {
    public Login() {
      InitializeComponent();
      // As wide as its words, so the button sits right after them.
      registerPrompt.Width = registerPrompt.GetPreferredSize(Size.Empty).Width;
    }

    protected override void OnShown(EventArgs e) {
      base.OnShown(e);
      txtUsername.Focus();
    }

    private void btnSignIn_Click(object sender, EventArgs e) {
      HideProblem();
      var username = txtUsername.Text.Trim();
      var password = txtPassword.Text;
      usernameField.Error = username.Length == 0 ? "Enter your username." : "";
      passwordField.Error = password.Length == 0 ? "Enter your password." : "";
      if (username.Length == 0) { txtUsername.Focus(); return; }
      if (password.Length == 0) { txtPassword.Focus(); return; }

      SignInStatus status;
      Account? account;
      Cursor = Cursors.WaitCursor;
      try {
        (status, account) = Accounts.SignIn(username, password);
      } catch (Exception ex) when (ex is OleDbException or InvalidOperationException) {
        // InvalidOperationException is the provider missing: the Access
        // database engine is not installed, or not at the app's bitness.
        ShowProblem(Tone.Danger, "The accounts database did not open", ex.Message);
        return;
      } finally {
        Cursor = Cursors.Default;
      }

      switch (status) {
        case SignInStatus.WrongCredentials:
          ShowProblem(Tone.Danger, "Wrong username or password", "Check both and try again. Passwords are case-sensitive.");
          txtPassword.Text = "";
          txtPassword.Focus();
          break;
        case SignInStatus.Inactive:
          ShowProblem(Tone.Warning, "This account is turned off", "Ask a system admin to turn it back on.");
          break;
        case SignInStatus.SignedIn:
          Accounts.Current = account;
          OpenDash();
          break;
      }
    }

    void OpenDash() {
      // Centered on the screen this window is on. Dash's own CenterScreen
      // takes the primary screen, so on a second monitor it opened there.
      var area = Screen.FromControl(this).WorkingArea;
      var dash = new Dash { StartPosition = FormStartPosition.Manual };
      dash.Location = new Point(
          area.Left + Math.Max(0, (area.Width - dash.Width) / 2),
          area.Top + Math.Max(0, (area.Height - dash.Height) / 2));
      dash.FormClosed += (_, _) => {
        if (dash.SignedOut) {
          Accounts.Current = null;
          txtPassword.Text = "";
          HideProblem();
          Show();
          Activate();
          txtPassword.Focus();
        } else {
          Close();
        }
      };
      Hide();
      dash.Show();
    }

    private void btnRegister_Click(object sender, EventArgs e) {
      var maximized = WindowState == FormWindowState.Maximized;
      var register = new Register {
        StartPosition = FormStartPosition.Manual,
        Bounds = maximized ? RestoreBounds : Bounds,
        WindowState = maximized ? FormWindowState.Maximized : FormWindowState.Normal,
      };
      register.FormClosed += (_, _) => {
        if (register.Outcome == RegisterOutcome.Quit) {
          Close();
          return;
        }
        HideProblem();
        usernameField.Error = "";
        passwordField.Error = "";
        txtPassword.Text = "";
        ComeBack(register);
        if (register.Outcome == RegisterOutcome.Registered) ShowCreated(register.RegisteredUsername);
        else if (txtUsername.Text.Length > 0) txtPassword.Focus();
        else txtUsername.Focus();
      };
      Hide();
      register.Show();
    }

    // Shows this window again where Register was, at its size (the two
    // share a layout), and maximized if it was.
    void ComeBack(Register from) {
      var maximized = from.WindowState == FormWindowState.Maximized;
      Bounds = maximized ? from.RestoreBounds : from.Bounds;
      WindowState = maximized ? FormWindowState.Maximized : FormWindowState.Normal;
      Show();
      Activate();
    }

    // After a new account: its username filled in and the caret in the
    // password, so the next keys typed are the password.
    void ShowCreated(string username) {
      txtUsername.Text = username;
      ShowProblem(Tone.Success, "Account created", "Sign in with your new username and password.");
      txtPassword.Focus();
    }

    void ShowProblem(Tone tone, string title, string body) {
      problem.Tone = tone;
      problem.Title = title;
      problem.Text = body;
      problem.Visible = true;
    }

    void HideProblem() => problem.Visible = false;

    private void txtUsername_TextChanged(object sender, EventArgs e) => usernameField.Error = "";

    private void txtPassword_TextChanged(object sender, EventArgs e) => passwordField.Error = "";
  }
}
