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

RUN apt-get update && \
  apt-get install -y sqlite3 curl && \
  rm -rf /var/lib/apt/lists/*

RUN groupadd -r app || true && useradd -r -g app app || true
RUN mkdir -p /app/data && \
  chown -R app:app /app && \
  chmod -R 755 /app && \
  chmod -R 777 /app/data

COPY --from=build /app/publish ./
RUN chown -R app:app /app

USER app

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1
ENTRYPOINT ["dotnet", "Wallet.dll"]
