using System.Data.OleDb;

namespace Obille_Try.Data {
  public sealed record Account(int Id, string Username, string FullName, string Role);

  public enum SignInStatus { SignedIn, WrongCredentials, Inactive }

  // Sign in and registration against the Users table. Every value goes in
  // as a parameter: OLE DB's parameters are positional, one ? each, added
  // in the order they appear, and the provider passes them apart from the
  // SQL text, so nothing typed into a form can change the query.
  public static class Accounts {
    public const int MinPasswordLength = 8;

    public static Account? Current { get; set; }

    // Hashed once, for a username nobody has: checking a password against
    // it takes as long as against a real account's hash.
    static readonly Lazy<string> standIn = new(() => PasswordHelper.Hash(Guid.NewGuid().ToString()));

    // A wrong username and a wrong password get the same answer in the same
    // time, so the form tells nobody which usernames exist.
    public static (SignInStatus Status, Account? Account) SignIn(string username, string password) {
      using var con = Database.GetConnection();
      con.Open();
      using var cmd = new OleDbCommand(
          "SELECT UserID, Username, PasswordHash, FullName, UserRole, IsActive FROM Users WHERE Username = ?", con);
      cmd.Parameters.AddWithValue("@Username", username.Trim());
      using var reader = cmd.ExecuteReader();
      if (!reader.Read()) {
        PasswordHelper.Verify(password, standIn.Value);
        return (SignInStatus.WrongCredentials, null);
      }

      var storedHash = Convert.ToString(reader["PasswordHash"]) ?? "";
      if (!PasswordHelper.Verify(password, storedHash)) return (SignInStatus.WrongCredentials, null);
      if (!Convert.ToBoolean(reader["IsActive"])) return (SignInStatus.Inactive, null);

      var account = new Account(
          Convert.ToInt32(reader["UserID"]),
          Convert.ToString(reader["Username"]) ?? "",
          Convert.ToString(reader["FullName"]) ?? "",
          Convert.ToString(reader["UserRole"]) ?? "");
      return (SignInStatus.SignedIn, account);
    }

    // Access compares text without case, so "Juan" and "juan" are one name
    // here, as the unique index on Username has them.
    public static bool UsernameTaken(string username) {
      using var con = Database.GetConnection();
      con.Open();
      using var cmd = new OleDbCommand("SELECT COUNT(*) FROM Users WHERE Username = ?", con);
      cmd.Parameters.AddWithValue("@Username", username.Trim());
      return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    // Saves a new active account with the User role. The form has checked
    // the fields; the name is checked again here, since another window
    // could have taken it since.
    public static bool Register(string fullName, string username, string password) {
      if (password.Length < MinPasswordLength)
        throw new ArgumentException($"A password needs at least {MinPasswordLength} characters.", nameof(password));
      if (UsernameTaken(username)) return false;

      using var con = Database.GetConnection();
      con.Open();
      using var cmd = new OleDbCommand(
          "INSERT INTO Users (Username, PasswordHash, FullName, UserRole, IsActive, CreatedAt) VALUES (?, ?, ?, ?, ?, ?)", con);
      cmd.Parameters.AddWithValue("@Username", username.Trim());
      cmd.Parameters.AddWithValue("@PasswordHash", PasswordHelper.Hash(password));
      cmd.Parameters.AddWithValue("@FullName", fullName.Trim());
      cmd.Parameters.AddWithValue("@UserRole", "User");
      cmd.Parameters.AddWithValue("@IsActive", true);
      // A DateTime parameter goes in as DBTimeStamp, which Access refuses
      // with fractional seconds; Date is Access's own date type.
      cmd.Parameters.Add("@CreatedAt", OleDbType.Date).Value = DateTime.Now;
      return cmd.ExecuteNonQuery() == 1;
    }
  }
}
