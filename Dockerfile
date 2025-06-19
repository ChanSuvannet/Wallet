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

# Create data directory and set permissions before switching users
RUN mkdir -p /app/data

# Copy published output from build stage
COPY --from=build /app/publish ./

# Create non-root user and set ownership (if not exists)
RUN groupadd -r app 2>/dev/null || true
RUN useradd -r -g app app 2>/dev/null || true
RUN chown -R app:app /app

# Switch to non-root user
USER app

# Expose port 80
EXPOSE 80

# Set the entrypoint
ENTRYPOINT ["dotnet", "Wallet.dll"]