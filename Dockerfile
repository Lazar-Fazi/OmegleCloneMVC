# 1. BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopiraj solution fajl
COPY *.sln ./

# Kopiraj csproj fajl i restore
COPY OmegleCloneMVC/*.csproj ./OmegleCloneMVC/
RUN dotnet restore

# Kopiraj ceo kod
COPY OmegleCloneMVC/. ./OmegleCloneMVC/
WORKDIR /app/OmegleCloneMVC

# Build
RUN dotnet publish -c Release -o out

# 2. RUNTIME STAGE
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/OmegleCloneMVC/out ./

# Render environment port
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

EXPOSE 10000

ENTRYPOINT ["dotnet", "OmegleCloneMVC.dll"]
