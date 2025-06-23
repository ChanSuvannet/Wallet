# ============================================
# Build Stage
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy remaining files and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ============================================
# Runtime Stage
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# ✅ Install sqlite3 CLI tool for DB inspection (optional but useful)
RUN apt-get update && \
  apt-get install -y sqlite3 curl && \
  rm -rf /var/lib/apt/lists/*

# ✅ Create a non-root app user
RUN groupadd -r app || true && useradd -r -g app app || true

# ✅ Create data folder for SQLite DB and apply permissions
RUN mkdir -p /app/data && \
  chown -R app:app /app && \
  chmod -R 755 /app && \
  chmod -R 777 /app/data

# ✅ Copy published build output
COPY --from=build /app/publish ./

# ✅ Fix file ownership (optional but safe)
RUN chown -R app:app /app

# ✅ Switch to non-root user
USER app

# ✅ Expose HTTP port
EXPOSE 80

# ✅ Add healthcheck endpoint for Docker Compose
HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

# ✅ Start the app
ENTRYPOINT ["dotnet", "Wallet.dll"]
