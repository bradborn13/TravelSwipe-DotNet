FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
 
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
 
COPY ["Activities.Api/Activities.Api.csproj", "Activities.Api/"]
COPY ["Activities.Application/Activities.Application.csproj", "Activities.Application/"]
COPY ["Activities.Core/Activities.Core.csproj", "Activities.Core/"]
COPY ["Activities.Infrastructure/Activities.Infrastructure.csproj", "Activities.Infrastructure/"]
COPY ["TravelSwipe.Contracts/TravelSwipe.Contracts.csproj", "TravelSwipe.Contracts/"]

RUN dotnet restore "Activities.Api/Activities.Api.csproj"
 
COPY . .
 
WORKDIR "/src/Activities.Api"
RUN dotnet build "Activities.Api.csproj" -c Debug -o /app/build
 
FROM build AS publish
RUN dotnet publish "Activities.Api.csproj" -c Debug -o /app/publish /p:UseAppHost=false
 
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Activities.Api.dll"]
 