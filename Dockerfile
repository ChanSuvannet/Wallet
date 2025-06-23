# ======================
# Build Stage
# ======================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all files and publish
COPY . ./
RUN dotnet publish Wallet.csproj -c Release -o /app/publish

# ======================
# Runtime Stage
# ======================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# (Optional) Install sqlite3 CLI for debugging inside container
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# ✅ Add group and user only if they don't exist
RUN getent group app || groupadd -r app
RUN id -u app || useradd -r -g app app

# Create data directory and fix permissions
RUN mkdir -p /app/data && \
    chown -R app:app /app && \
    chmod -R 755 /app && \
    chmod -R 777 /app/data

# Copy build output
COPY --from=build /app/publish ./

# Ensure ownership
RUN chown -R app:app /app

# Switch to non-root user
USER app

# Expose port
EXPOSE 80

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

# Start the app
ENTRYPOINT ["dotnet", "Wallet.dll"]
