# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/playwright/dotnet:v1.48.0-jammy AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# The rest of the file (Build/Publish steps) can stay as Visual Studio generated it
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CLIR-InfoSystem/CLIR_InfoSystem.csproj", "CLIR-InfoSystem/"]
RUN dotnet restore "./CLIR-InfoSystem/CLIR_InfoSystem.csproj"
COPY . .
WORKDIR "/src/CLIR-InfoSystem"
RUN dotnet build "./CLIR_InfoSystem.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CLIR_InfoSystem.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CLIR_InfoSystem.dll"]