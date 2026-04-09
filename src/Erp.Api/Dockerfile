# ── Build stage ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (layer cache for restore)
COPY Dynamix.sln Directory.Build.props ./
COPY src/Erp.Platform/Erp.Platform.csproj              src/Erp.Platform/
COPY src/Erp.Shared/Erp.Shared.csproj                  src/Erp.Shared/
COPY src/Erp.Infrastructure/Erp.Infrastructure.csproj   src/Erp.Infrastructure/
COPY src/Erp.Modules.Sales/Erp.Modules.Sales.csproj    src/Erp.Modules.Sales/
COPY src/Erp.Modules.Finance/Erp.Modules.Finance.csproj src/Erp.Modules.Finance/
COPY src/Erp.Api/Erp.Api.csproj                        src/Erp.Api/
COPY src/Erp.Web/Erp.Web.csproj                        src/Erp.Web/
COPY tests/Erp.UnitTests/Erp.UnitTests.csproj          tests/Erp.UnitTests/

RUN dotnet restore Dynamix.sln

# Copy everything and publish the API
COPY . .
RUN dotnet publish src/Erp.Api/Erp.Api.csproj -c Release -o /app/publish --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "Erp.Api.dll"]
