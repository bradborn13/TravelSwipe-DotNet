FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["EventHub.Worker/EventHub.Worker.csproj", "EventHub.Worker/"]
COPY ["EventHub.Application/EventHub.Application.csproj", "EventHub.Application/"]
COPY ["TravelSwipe.Shared/TravelSwipe.Shared.csproj", "TravelSwipe.Shared/"]

RUN dotnet restore "EventHub.Worker/EventHub.Worker.csproj"

COPY . .

RUN dotnet build "EventHub.Worker/EventHub.Worker.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "EventHub.Worker/EventHub.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - CHANGE THIS
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "EventHub.Worker.dll"]