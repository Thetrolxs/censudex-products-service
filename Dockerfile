# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# ===== Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Exponer puertos:
#  - 50051 -> gRPC (HTTP/2) interno/externo
#  - 8080  -> HTTP/REST (si tu app usa 8080)
EXPOSE 50051
EXPOSE 8080

ENV ASPNETCORE_URLS="http://+:8080;http://+:50051"

ENTRYPOINT ["dotnet", "censudex-products-service.dll"]
