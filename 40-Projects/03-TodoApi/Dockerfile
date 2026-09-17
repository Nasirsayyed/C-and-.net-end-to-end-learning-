# Multi-stage build (see module 37-Docker): the SDK image builds/publishes,
# but only the lightweight ASP.NET runtime image is shipped — no compiler,
# no source, no build tools in the final image.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy just the .csproj first so `dotnet restore` is cached as its own Docker
# layer — it only re-runs when dependencies actually change, not on every
# source edit (see module 37-Docker's note on layer caching).
COPY ["src/TodoApi/TodoApi.csproj", "src/TodoApi/"]
RUN dotnet restore "src/TodoApi/TodoApi.csproj"

COPY src/TodoApi/. src/TodoApi/
WORKDIR /src/src/TodoApi
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

# Render (and most free PaaS hosts: Fly.io, Railway, Heroku-style buildpacks)
# assign a random port at runtime via the PORT env var and require the app to
# listen on it — Kestrel doesn't know about PORT by default, so this shell-form
# ENTRYPOINT translates it into ASPNETCORE_URLS at container start. Falls back
# to 8080 for local `docker run` where PORT isn't set.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet TodoApi.dll"]
