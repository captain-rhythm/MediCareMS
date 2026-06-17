# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies first (layer cache optimization)
COPY ["MediCareMS.csproj", "./"]
RUN dotnet restore "MediCareMS.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "MediCareMS.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create directory for Data Protection keys (persisted via Render disk or env)
RUN mkdir -p /app/App_Data/DataProtectionKeys

# Copy published output
COPY --from=build /app/publish .

# Render.com uses PORT env variable; ASP.NET Core listens on it
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "MediCareMS.dll"]
