# -----------------------------
# Stage 1: Runtime base (Linux)
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# -----------------------------
# Stage 2: Build
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Limpar cache NuGet
RUN dotnet nuget locals all --clear

# Copiar todos os csproj primeiro (para cache de restore)
COPY ["OrderManager.Application/OrderManager.Application.csproj", "OrderManager.Application/"]
COPY ["OrderManager.Domain/OrderManager.Domain.csproj", "OrderManager.Domain/"]
COPY ["OrderManager.Infrastructure/OrderManager.Infrastructure.csproj", "OrderManager.Infrastructure/"]
COPY ["OrderManager.Web.Api/OrderManager.Web.Api.csproj", "OrderManager.Web.Api/"]

# Restaurar dependencias
RUN dotnet restore "OrderManager.Web.Api/OrderManager.Web.Api.csproj"

# Copiar todo o código
COPY . .

# Build do projeto web
WORKDIR "/src/OrderManager.Web.Api"
RUN dotnet build "OrderManager.Web.Api.csproj" -c Release -o /app/build

# -----------------------------
# Stage 3: Publish
# -----------------------------
FROM build AS publish
RUN dotnet publish "OrderManager.Web.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -----------------------------
# Stage 4: Final container
# -----------------------------
FROM base AS final
WORKDIR /app

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Entrypoint
ENTRYPOINT ["dotnet", "OrderManager.Web.Api.dll"]
