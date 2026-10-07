# Multi-Stage Production Dockerfile for OrderPulse Web API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Step 1: Copy csproj files and restore dependencies (leveraging Docker layer cache)
COPY ["src/OrderPulse.Domain/OrderPulse.Domain.csproj", "src/OrderPulse.Domain/"]
COPY ["src/OrderPulse.Application/OrderPulse.Application.csproj", "src/OrderPulse.Application/"]
COPY ["src/OrderPulse.Infrastructure/OrderPulse.Infrastructure.csproj", "src/OrderPulse.Infrastructure/"]
COPY ["src/OrderPulse.Api/OrderPulse.Api.csproj", "src/OrderPulse.Api/"]

RUN dotnet restore "src/OrderPulse.Api/OrderPulse.Api.csproj"

# Step 2: Copy remaining source code and publish
COPY src/ src/
WORKDIR "/app/src/OrderPulse.Api"
RUN dotnet publish "OrderPulse.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Step 3: Lightweight Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderPulse.Api.dll"]
