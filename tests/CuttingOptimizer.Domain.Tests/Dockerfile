FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tests/CuttingOptimizer.Domain.Tests/CuttingOptimizer.Domain.Tests.csproj", "tests/CuttingOptimizer.Domain.Tests/"]
COPY ["src/CuttingOptimizer.Domain/CuttingOptimizer.Domain.csproj", "src/CuttingOptimizer.Domain/"]
RUN dotnet restore "tests/CuttingOptimizer.Domain.Tests/CuttingOptimizer.Domain.Tests.csproj"
COPY . .
WORKDIR "/src/tests/CuttingOptimizer.Domain.Tests"
RUN dotnet build "./CuttingOptimizer.Domain.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CuttingOptimizer.Domain.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CuttingOptimizer.Domain.Tests.dll"]
