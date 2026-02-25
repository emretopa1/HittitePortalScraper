# BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore

# PUBLISH
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# FINAL
FROM mcr.microsoft.com/playwright/dotnet:v1.42.0-jammy AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "HittitePortalScraper.dll"]