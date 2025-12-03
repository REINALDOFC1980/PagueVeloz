# ==============================
# 1) STAGE: BUILD
# ==============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copiar apenas os arquivos .csproj (para cache do restore)
COPY PagueVeloz.Api/PagueVeloz.Api.csproj PagueVeloz.Api/
COPY PagueVeloz.Application/PagueVeloz.Application.csproj PagueVeloz.Application/
COPY PagueVeloz.Domain/PagueVeloz.Domain.csproj PagueVeloz.Domain/
COPY PagueVeloz.Infrastructure/PagueVeloz.Infrastructure.csproj PagueVeloz.Infrastructure/
COPY PagueVeloz.Shared/PagueVeloz.Shared.csproj PagueVeloz.Shared/

# Restaurar dependências
RUN dotnet restore PagueVeloz.Api/PagueVeloz.Api.csproj

# Copiar todo o restante da solução
COPY . .

WORKDIR "/src/PagueVeloz.Api"

# Build da API
RUN dotnet build "PagueVeloz.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ==============================
# 2) STAGE: PUBLISH
# ==============================
FROM build AS publish

RUN dotnet publish "PagueVeloz.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ==============================
# 3) STAGE: RUNTIME
# ==============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

# Copiar resultado da publicação
COPY --from=publish /app/publish .

# Expor portas da API
EXPOSE 8080 9090

# ENTRYPOINT para iniciar a API
ENTRYPOINT ["dotnet", "PagueVeloz.Api.dll"]
