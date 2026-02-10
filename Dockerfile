FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file from the subfolder
COPY CLIR-InfoSystem/*.csproj ./ 
RUN dotnet restore

# Copy the rest of the source code
COPY CLIR-InfoSystem/. ./CLIR-InfoSystem/
WORKDIR /app/CLIR-InfoSystem
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/CLIR-InfoSystem/out .

ENTRYPOINT ["dotnet", "CLIR_InfoSystem.dll"]
