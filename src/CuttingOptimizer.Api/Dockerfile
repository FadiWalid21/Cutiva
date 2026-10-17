FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CuttingOptimizer.Api/CuttingOptimizer.Api.csproj", "src/CuttingOptimizer.Api/"]
COPY ["src/CuttingOptimizer.Application/CuttingOptimizer.Application.csproj", "src/CuttingOptimizer.Application/"]
COPY ["src/CuttingOptimizer.Domain/CuttingOptimizer.Domain.csproj", "src/CuttingOptimizer.Domain/"]
COPY ["src/CuttingOptimizer.Infrastructure/CuttingOptimizer.Infrastructure.csproj", "src/CuttingOptimizer.Infrastructure/"]
RUN dotnet restore "src/CuttingOptimizer.Api/CuttingOptimizer.Api.csproj"
COPY . .
WORKDIR "/src/src/CuttingOptimizer.Api"
RUN dotnet build "./CuttingOptimizer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CuttingOptimizer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CuttingOptimizer.Api.dll"]
