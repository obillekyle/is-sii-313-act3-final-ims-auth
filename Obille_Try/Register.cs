using System.ComponentModel;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using Kit;
using Obille_Try.Data;

namespace Obille_Try {
  public enum RegisterOutcome { Quit, Back, Registered }

  // Makes a new account in the Users table: an active one with the User
  // role. Each field says what is wrong under it; the username is checked
  // against the table only once the rest is right. Login opens this window
  // in its own place and reads Outcome when it closes.
  public partial class Register : Form {
    static readonly Regex UsernameChars = new("^[A-Za-z0-9._-]+$");

    public Register() {
      InitializeComponent();
      // As wide as its words, so the button sits right after them.
      signInPrompt.Width = signInPrompt.GetPreferredSize(Size.Empty).Width;
      Icon = (Icon?)new ComponentResourceManager(typeof(Login)).GetObject("$this.Icon");
    }

    // Quit until Back or a new account closes the window, so its window
    // button ends the app, as Dash's does.
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RegisterOutcome Outcome { get; private set; } = RegisterOutcome.Quit;

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string RegisteredUsername { get; private set; } = "";

    protected override void OnShown(EventArgs e) {
      base.OnShown(e);
      txtFullName.Focus();
    }

    private void btnRegister_Click(object sender, EventArgs e) {
      problem.Visible = false;
      var fullName = txtFullName.Text.Trim();
      var username = txtUsername.Text.Trim();
      var password = txtPassword.Text;

      fullNameField.Error =
          fullName.Length == 0 ? "Enter your full name." :
          fullName.Length > 100 ? "Use 100 characters or fewer." : "";
      usernameField.Error =
          username.Length == 0 ? "Choose a username." :
          username.Length > 50 ? "Use 50 characters or fewer." :
          !UsernameChars.IsMatch(username) ? "Use letters, numbers, dots, dashes or underscores, with no spaces." : "";
      passwordField.Error = password.Length < Accounts.MinPasswordLength
          ? $"Use at least {Accounts.MinPasswordLength} characters." : "";
      confirmField.Error =
          txtConfirmPassword.Text.Length == 0 ? "Type the password again." :
          txtConfirmPassword.Text != password ? "This does not match the password above." : "";
      if (FocusFirstError()) return;

      bool saved;
      Cursor = Cursors.WaitCursor;
      try {
        saved = Accounts.Register(fullName, username, password);
      } catch (Exception ex) when (ex is OleDbException or InvalidOperationException) {
        problem.Title = "The accounts database did not open";
        problem.Text = ex.Message;
        problem.Visible = true;
        return;
      } finally {
        Cursor = Cursors.Default;
      }

      if (!saved) {
        usernameField.Error = "That username is taken. Choose another.";
        txtUsername.Focus();
        return;
      }
      RegisteredUsername = username;
      Outcome = RegisterOutcome.Registered;
      Close();
    }

    bool FocusFirstError() {
      foreach (var (field, input) in new (Field, TextInput)[] {
          (fullNameField, txtFullName), (usernameField, txtUsername),
          (passwordField, txtPassword), (confirmField, txtConfirmPassword) }) {
        if (field.Error.Length == 0) continue;
        input.Focus();
        return true;
      }
      return false;
    }

    private void btnBack_Click(object sender, EventArgs e) {
      Outcome = RegisterOutcome.Back;
      Close();
    }

    private void txtFullName_TextChanged(object sender, EventArgs e) => fullNameField.Error = "";

    private void txtUsername_TextChanged(object sender, EventArgs e) => usernameField.Error = "";

    private void txtPassword_TextChanged(object sender, EventArgs e) => passwordField.Error = "";

    private void txtConfirmPassword_TextChanged(object sender, EventArgs e) => confirmField.Error = "";
  }
}
