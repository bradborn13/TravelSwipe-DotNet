FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
 
COPY ["Countries.Api/Countries.Api.csproj", "Countries.Api/"]
COPY ["Countries.Application/Countries.Application.csproj", "Countries.Application/"]
COPY ["Countries.Core/Countries.Core.csproj", "Countries.Core/"]
COPY ["Countries.Infrastructure/Countries.Infrastructure.csproj", "Countries.Infrastructure/"]
 
RUN dotnet restore "Countries.Api/Countries.Api.csproj"
 
COPY . .
 
WORKDIR "/src/Countries.Api"
RUN dotnet build "Countries.Api.csproj" -c Release -o /app/build
 
FROM build AS publish
RUN dotnet publish "Countries.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
 
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Countries.Api.dll"]
 