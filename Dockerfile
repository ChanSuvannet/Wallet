# ======================
# Build Stage
# ======================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy and restore project dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ======================
# Runtime Stage
# ======================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create persistent data directory for SQLite
RUN mkdir -p /app/data

# Copy the published output
COPY --from=build /app/publish ./

# Set permissions (if running as non-root)
RUN groupadd -r app || true && useradd -r -g app app || true
RUN chown -R app:app /app
USER app

# Expose HTTP port
EXPOSE 80

# Start the app
ENTRYPOINT ["dotnet", "Wallet.dll"]
