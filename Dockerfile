# FILE: Dockerfile
# 1️⃣ Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Çözümü ve projeleri kopyala
COPY ["OBEYSOFT.sln", "./"]
COPY ["Obeysoft.Domain/Obeysoft.Domain.csproj", "Obeysoft.Domain/"]
COPY ["Obeysoft.Application/Obeysoft.Application.csproj", "Obeysoft.Application/"]
COPY ["Obeysoft.Infrastructure/Obeysoft.Infrastructure.csproj", "Obeysoft.Infrastructure/"]
COPY ["Obeysoft.Api/Obeysoft.Api.csproj", "Obeysoft.Api/"]

# Restore
RUN dotnet restore "./Obeysoft.Api/Obeysoft.Api.csproj"

# Tüm kaynak kodlarını kopyala
COPY . .

# Build + Publish (Release)
RUN dotnet publish "./Obeysoft.Api/Obeysoft.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2️⃣ Run aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Ortam değişkeni (Production)
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

# Health check - Render PORT kullanır
HEALTHCHECK CMD curl --fail http://localhost:$PORT/health || exit 1

# API başlat
ENTRYPOINT ["dotnet", "Obeysoft.Api.dll"]
