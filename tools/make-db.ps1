# Makes the IMS accounts database: an Access file with the Users table the
# sign-in and register forms read and write. Run from 64-bit PowerShell
# (the ACE 16.0 provider here is 64-bit). Refuses to overwrite a file.
param([Parameter(Mandatory)][string]$Out)

if (Test-Path $Out) { Write-Host "exists, left alone: $Out"; exit 1 }
$conn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$Out"

# ADOX.Catalog.Create makes the empty .accdb file.
$cat = New-Object -ComObject ADOX.Catalog
$cat.Create($conn) | Out-Null
$cat.ActiveConnection.Close()

$db = New-Object System.Data.OleDb.OleDbConnection $conn
$db.Open()
$ddl = @(
  'CREATE TABLE Users (UserID COUNTER CONSTRAINT PK_Users PRIMARY KEY, Username TEXT(50) NOT NULL, PasswordHash TEXT(255) NOT NULL, FullName TEXT(100) NOT NULL, UserRole TEXT(20) NOT NULL, IsActive YESNO NOT NULL, CreatedAt DATETIME NOT NULL)',
  'CREATE UNIQUE INDEX UX_Users_Username ON Users (Username)'
)
foreach ($sql in $ddl) {
  $cmd = $db.CreateCommand()
  $cmd.CommandText = $sql
  $cmd.ExecuteNonQuery() | Out-Null
}
$cmd = $db.CreateCommand()
$cmd.CommandText = 'SELECT * FROM Users'
$reader = $cmd.ExecuteReader([System.Data.CommandBehavior]::SchemaOnly)
$schema = $reader.GetSchemaTable()
$reader.Close()
$db.Close()
$schema | ForEach-Object { Write-Host ("{0} {1} size {2}" -f $_.ColumnName, $_.DataType.Name, $_.ColumnSize) }
Write-Host "made $Out"
