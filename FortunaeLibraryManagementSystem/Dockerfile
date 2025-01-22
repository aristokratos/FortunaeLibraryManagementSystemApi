# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FortunaeLibraryManagementSystem/FortunaeLibraryManagementSystem.csproj", "FortunaeLibraryManagementSystem/"]
COPY ["FortunaeLibraryManagementSystem.Domain/FortunaeLibraryManagementSystem.Domain.csproj", "FortunaeLibraryManagementSystem.Domain/"]
COPY ["FortunaeLibraryManagementSystem.Service/FortunaeLibraryManagementSystem.Service.csproj", "FortunaeLibraryManagementSystem.Service/"]
COPY ["FortunaeLibraryManagementSystem.Infrastructure/FortunaeLibraryManagementSystem.Infrastructure.csproj", "FortunaeLibraryManagementSystem.Infrastructure/"]
RUN dotnet restore "FortunaeLibraryManagementSystem/FortunaeLibraryManagementSystem.csproj"
COPY . .
WORKDIR "/src/FortunaeLibraryManagementSystem"
RUN dotnet build "FortunaeLibraryManagementSystem.csproj" -c Release -o /app/build

# Publish image
FROM build AS publish
RUN dotnet publish "FortunaeLibraryManagementSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FortunaeLibraryManagementSystem.dll"]
