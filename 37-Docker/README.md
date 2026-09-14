# 37 — Docker

## 🎯 Learning Objectives
- Explain images vs containers and write a correct multi-stage .NET Dockerfile.
- Use Docker Compose to run a full local stack (API + SQL Server + Redis).

## 🤔 What is it?
Docker packages an application with everything it needs to run (runtime, dependencies, config) into a portable **image**, run as an isolated **container** — solving "works on my machine" by making the runtime environment itself part of the shipped artifact.

## 🧠 Core Concept

```mermaid
flowchart TD
    App[Application Code] --> Image[Docker Image\n(built once, immutable)]
    Image --> Container1[Container instance 1]
    Image --> Container2[Container instance 2]
    Container1 --> HostOS[Host OS Kernel — shared, not virtualized]
    Container2 --> HostOS
```
- **Image** — a read-only, layered template (built from a `Dockerfile`).
- **Container** — a running instance of an image, with its own writable layer and isolated process/network namespace, but sharing the host OS kernel (much lighter-weight than a full VM).

### Multi-stage .NET Dockerfile

```dockerfile
# Stage 1: build (needs the full SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Orders.Api/Orders.Api.csproj", "Orders.Api/"]
RUN dotnet restore "Orders.Api/Orders.Api.csproj"
COPY . .
WORKDIR /src/Orders.Api
RUN dotnet publish -c Release -o /app/publish

# Stage 2: runtime (only needs the runtime — much smaller final image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
```
Copying the `.csproj` and restoring **before** copying the rest of the source lets Docker cache the (usually slow) `restore` layer, only re-running it when dependencies actually change — a significant build-time optimization via Docker's layer caching.

### Docker Compose — local multi-container stack

```yaml
version: "3.9"
services:
  api:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ConnectionStrings__Default=Server=db;Database=Orders;User=sa;Password=${SA_PASSWORD}
    depends_on:
      - db
      - redis
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SA_PASSWORD}
    volumes:
      - sql-data:/var/opt/mssql
  redis:
    image: redis:7
volumes:
  sql-data:
```
- **Volumes** persist data (like the SQL Server data files) beyond a container's lifecycle — without one, recreating the `db` container would wipe all data.
- **Networks** (implicit default network here) let services reach each other by service name (`db`, `redis`) as hostnames.
- **Environment variables** configure each container without baking secrets into the image itself.

## 🏢 Real-World Example
A CI pipeline builds the API's Docker image once, runs the full integration test suite against a `docker-compose`-managed SQL Server + Redis stack (identical to production topology, just smaller), then — if tests pass — pushes the exact same image to a registry for deployment, guaranteeing what was tested is bit-for-bit what ships.

## ⚠️ Common Mistakes
- Using the SDK image (not just the runtime) for the final production stage — larger image, larger attack surface, no benefit since production never compiles anything.
- Baking secrets (connection strings, API keys) directly into the image instead of injecting them via environment variables/secrets at runtime.
- Not using `.dockerignore` (e.g. excluding `bin/`, `obj/`, `.git/`), bloating build context and image layers.
- Running containers as root unnecessarily — a security hardening step (`USER app` in modern .NET base images) that's easy to skip.

## ✅ Best Practices
- Always use multi-stage builds: SDK for building, runtime-only image for the final stage.
- Order Dockerfile instructions to maximize layer cache hits (dependencies before source code).
- Use Docker Compose (or an equivalent) to mirror production topology locally for realistic integration testing.
- Tag images with a specific version/commit SHA, never rely solely on `latest` for deployments.

## ⚡ Performance Considerations
- Smaller runtime-only images start faster and pull faster in CI/CD and orchestrated environments (Kubernetes) — directly affects deployment speed and autoscaling responsiveness.
- Layer caching (ordering `COPY`/`RUN` instructions well) can cut CI build times dramatically for iterative changes.

## 🔄 Related Concepts
- [01 — C# Fundamentals](../01-CSharp-Fundamentals) (SDK vs Runtime)
- [34 — Testing](../34-Testing) (Testcontainers)
- [38 — CI/CD](../38-CI-CD)

## 🎤 Interview Questions

**Junior:** "What's the difference between a Docker image and a container?"
*Expected:* An image is an immutable, layered template; a container is a running (or stopped) instance of that image with its own writable layer and isolated namespace.

**Mid-level:** "Why use a multi-stage Dockerfile for a .NET app?"
*Expected:* Separates the build environment (needing the full SDK) from the runtime environment (only needing the ASP.NET Core/`.NET` runtime), producing a much smaller, more secure final image that never carries build tools into production.

**Senior:** "How would you optimize Docker build times and image size for a large .NET solution with many projects?"
*Expected:* Should mention: copying only `.csproj` files and restoring before copying full source to maximize layer cache reuse, using `.dockerignore` aggressively, choosing the smallest viable base image (e.g. Alpine variants where compatible), multi-stage builds, and potentially a shared base image/layer for common dependencies across multiple services' Dockerfiles.

## 🧪 Practice Exercises

**Easy**
1. Write a multi-stage Dockerfile for a simple Web API and build it.
2. Run the resulting image and hit its endpoint from the host.
3. Compare the final image size against a naive single-stage SDK-based image.

**Medium**
1. Set up a `docker-compose.yml` running the API alongside SQL Server, with a volume for data persistence.
2. Add a `.dockerignore` and measure the build context size difference.
3. Inject a connection string via environment variables instead of baking it into the image.

**Hard**
1. Optimize a multi-stage Dockerfile for a solution with 3+ projects to maximize layer cache reuse across incremental builds.
2. Set up a Testcontainers-based integration test suite running against a Compose-defined stack in CI.

**Real-world scenario:** A team's Docker image is 1.2GB and takes 8 minutes to build every CI run, even for a one-line code change. Diagnose likely causes and propose fixes.

## 📌 Key Takeaways
- Images are immutable templates; containers are running instances sharing the host kernel (lighter than VMs).
- Always multi-stage build: SDK to build, runtime-only image to ship.
- Docker Compose lets you mirror production topology locally for realistic testing.
