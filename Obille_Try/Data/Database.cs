using System.Data.OleDb;

namespace Obille_Try.Data {
  // The accounts database: an Access file, Data\ims.accdb, copied beside the
  // exe at build. It is read through the Microsoft Access Database Engine's
  // OLE DB provider (ACE), which has to be installed with the same bitness
  // as the app runs at: 64-bit here, where an AnyCPU build runs as x64.
  // The file's one table is Users (UserID, Username, PasswordHash, FullName,
  // UserRole, IsActive, CreatedAt); tools\make-db.ps1 makes an empty one.
  public static class Database {
    // Replaceable so a check can run against a copy of the file.
    public static string FilePath { get; set; } = Path.Combine(AppContext.BaseDirectory, "Data", "ims.accdb");

    // A new, closed connection. The caller opens it inside a using, so it
    // is closed again however the query ends.
    public static OleDbConnection GetConnection() =>
        new($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={FilePath};Persist Security Info=False");
  }
}
