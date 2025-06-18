`README.md`
---

# 💰 ElsSaleWallet

A simple wallet management system built with ASP.NET Core 9 MVC and SQLite. Designed for easy local and production deployment using Docker.

---

## 🧑‍💻 Getting Started

### 📦 Prerequisites

- [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)

---

## 🔧 Local Development

```bash
# Run 
dotnet run

# Run in Watch Mode
dotnet watch run

# migration 
dotnet ef migrations add InitialCreate

# update database 
dotnet ef database update
````
---

## 🐳 Docker Build & Run (Production)

### Build the image and run the container

```bash
docker-compose up --build -d
```

---

## 💾 Database

SQLite is used for simplicity. Your DB file (`wallet.db`) is automatically created and persisted via Docker volume mapping.

> To reset or inspect it, open it using any SQLite browser like **DB Browser for SQLite**.

---

## 🔍 Swagger API Docs (Development Only)

If you run in development mode:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

---

## 📂 Environment Configuration

Change the connection string or environment settings in `appsettings.json` or `appsettings.Production.json`.

---

## ✅ Production Tips

* Use HTTPS with Nginx reverse proxy or Azure App Service.
* Protect `wallet.db` using volume-only access.
* Move sensitive configs to environment variables or secrets manager.

---

## 📄 License

This project is open-source and free to use under the MIT License.
