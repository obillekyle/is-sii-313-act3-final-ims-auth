using System.Security.Cryptography;

namespace Obille_Try.Data {
  // Passwords are stored as salted PBKDF2 hashes, never as the password:
  // "PBKDF2$<iterations>$<salt>$<hash>", salt and hash in Base64. A hash
  // cannot be turned back into the password; Verify hashes what was typed
  // with the stored salt and count and compares the two hashes.
  public static class PasswordHelper {
    // OWASP's 2023 figure for PBKDF2 with SHA-256 is 600,000; 100,000 took
    // 76 to 119 ms on an i5-6200U. The count is stored in each hash, so
    // raising it later leaves older accounts working.
    const int Iterations = 100_000;
    const int SaltBytes = 16;
    const int HashBytes = 32;

    public static string Hash(string password) {
      var salt = RandomNumberGenerator.GetBytes(SaltBytes);
      var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashBytes);
      return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string stored) {
      var parts = stored.Split('$');
      if (parts.Length != 4 || parts[0] != "PBKDF2" || !int.TryParse(parts[1], out var iterations) || iterations < 1)
        return false;
      byte[] salt, expected;
      try {
        salt = Convert.FromBase64String(parts[2]);
        expected = Convert.FromBase64String(parts[3]);
      } catch (FormatException) {
        return false;
      }
      var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
      // Takes the same time wherever the first difference is, so the time
      // a check takes says nothing about how close a guess was.
      return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
  }
}
