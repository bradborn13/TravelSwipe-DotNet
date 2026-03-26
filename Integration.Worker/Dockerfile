FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Integration.Worker/Integration.Worker.csproj", "Integration.Worker/"]
COPY ["Integration.Application/Integration.Application.csproj", "Integration.Application/"]
COPY ["Integration.Core/Integration.Core.csproj", "Integration.Core/"]
COPY ["TravelSwipe.Contracts/TravelSwipe.Contracts.csproj", "TravelSwipe.Contracts/"]

RUN dotnet restore "Integration.Worker/Integration.Worker.csproj"

COPY . .

WORKDIR "/src/Integration.Worker"
RUN dotnet build "Integration.Worker.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "Integration.Worker.csproj" -c Debug -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Integration.Worker.dll"]