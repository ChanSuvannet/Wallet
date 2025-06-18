# Create migration files
dotnet ef migrations add InitialCreate

# Apply migration and create SQLite database
dotnet ef database update
