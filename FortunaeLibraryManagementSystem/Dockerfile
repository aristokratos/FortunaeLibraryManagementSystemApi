FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FortunaeLibraryManagementSystem/FortunaeLibraryManagementSystem.csproj", "FortunaeLibraryManagementSystem/"]
COPY ["FortunaeLibraryManagementSystem.Domain/FortunaeLibraryManagementSystem.Domain.csproj", "FortunaeLibraryManagementSystem.Domain/"]
COPY ["FortunaeLibraryManagementSystem.Service/FortunaeLibraryManagementSystem.Service.csproj", "FortunaeLibraryManagementSystem.Service/"]
COPY ["FortunaeLibraryManagementSystem.Infrastructure/FortunaeLibraryManagementSystem.Infrastructure.csproj", "FortunaeLibraryManagementSystem.Infrastructure/"]
RUN dotnet restore "./FortunaeLibraryManagementSystem/FortunaeLibraryManagementSystem.csproj"
COPY . .
WORKDIR "/src/FortunaeLibraryManagementSystem"
RUN dotnet build "./FortunaeLibraryManagementSystem.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FortunaeLibraryManagementSystem.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "FortunaeLibraryManagementSystem.dll", "--urls", "http://0.0.0.0:5000"]