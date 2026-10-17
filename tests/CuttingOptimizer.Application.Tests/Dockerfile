FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tests/CuttingOptimizer.Application.Tests/CuttingOptimizer.Application.Tests.csproj", "tests/CuttingOptimizer.Application.Tests/"]
COPY ["src/CuttingOptimizer.Application/CuttingOptimizer.Application.csproj", "src/CuttingOptimizer.Application/"]
COPY ["src/CuttingOptimizer.Domain/CuttingOptimizer.Domain.csproj", "src/CuttingOptimizer.Domain/"]
RUN dotnet restore "tests/CuttingOptimizer.Application.Tests/CuttingOptimizer.Application.Tests.csproj"
COPY . .
WORKDIR "/src/tests/CuttingOptimizer.Application.Tests"
RUN dotnet build "./CuttingOptimizer.Application.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CuttingOptimizer.Application.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CuttingOptimizer.Application.Tests.dll"]
