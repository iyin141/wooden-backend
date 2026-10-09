# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# restore (layer-cached)
COPY WoodLounge.slnx ./
COPY src/WoodLounge.Domain/WoodLounge.Domain.csproj src/WoodLounge.Domain/
COPY src/WoodLounge.Application/WoodLounge.Application.csproj src/WoodLounge.Application/
COPY src/WoodLounge.Infrastructure/WoodLounge.Infrastructure.csproj src/WoodLounge.Infrastructure/
COPY src/WoodLounge.Api/WoodLounge.Api.csproj src/WoodLounge.Api/
RUN dotnet restore src/WoodLounge.Api/WoodLounge.Api.csproj

# publish
COPY src/ src/
RUN dotnet publish src/WoodLounge.Api/WoodLounge.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "WoodLounge.Api.dll"]
