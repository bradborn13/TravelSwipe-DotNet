FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["EventHub.Worker/EventHub.Worker.csproj", "EventHub.Worker/"]
COPY ["EventHub.Application/EventHub.Application.csproj", "EventHub.Application/"]
COPY ["TravelSwipe.Shared/TravelSwipe.Shared.csproj", "TravelSwipe.Shared/"]

# Restore dependencies
RUN dotnet restore "EventHub.Worker/EventHub.Worker.csproj"

# Copy source code
COPY . .

# Build
RUN dotnet build "EventHub.Worker/EventHub.Worker.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "EventHub.Worker/EventHub.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "EventHub.Worker.dll"]