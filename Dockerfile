# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore (cache dependencies)
COPY *.csproj ./
RUN dotnet restore

# Copy remaining files and publish
COPY . ./
RUN dotnet publish Wallet.csproj -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install SQLite for debugging (optional)
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Create app user and group first
RUN groupadd -r app && useradd -r -g app app

# Create data directory with proper permissions
RUN mkdir -p /app/data && \
    chown -R app:app /app && \
    chmod -R 755 /app && \
    chmod -R 777 /app/data

# Copy published output from build stage
COPY --from=build /app/publish ./

# Ensure proper ownership after copying
RUN chown -R app:app /app

# Switch to non-root user
USER app

# Expose port 80
EXPOSE 80

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Set the entrypoint
ENTRYPOINT ["dotnet", "Wallet.dll"]