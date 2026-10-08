# GLA backend — multi-stage build.
# Both base images are multi-arch (amd64 + arm64), so the image builds natively
# on an ARM64 host; the target platform is set in docker-compose.yml.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so this layer caches until the project file changes.
COPY GLA.csproj ./
RUN dotnet restore GLA.csproj

COPY . .
RUN dotnet publish GLA.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# HTTP-only container port; nginx proxies to http://backend:8080 (see
# frontend/nginx.conf). Settings come from the environment (docker-compose):
# ConnectionStrings:GlaDb, Songs:FilesRoot, Cors:AllowedOrigins.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GLA.dll"]
