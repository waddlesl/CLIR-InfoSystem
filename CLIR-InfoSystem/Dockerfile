FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 1. Install System Dependencies
RUN apt-get update && apt-get install -y \
    libnspr4 \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libgbm1 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CLIR_InfoSystem.csproj", "."]
RUN dotnet restore "./CLIR_InfoSystem.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CLIR_InfoSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CLIR_InfoSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN dotnet CLIR_InfoSystem.dll --help || true 


ENTRYPOINT ["dotnet", "CLIR_InfoSystem.dll"]