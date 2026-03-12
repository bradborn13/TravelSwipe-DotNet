Copy

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
 
COPY ["Services/Countries/Countries.Api/Countries.Api.csproj", "Services/Countries/Countries.Api/"]
COPY ["Services/Countries/Countries.Application/Countries.Application.csproj", "Services/Countries/Countries.Application/"]
COPY ["Services/Countries/Countries.Core/Countries.Core.csproj", "Services/Countries/Countries.Core/"]
COPY ["Services/Countries/Countries.Infrastructure/Countries.Infrastructure.csproj", "Services/Countries/Countries.Infrastructure/"]
 
RUN dotnet restore "Services/Countries/Countries.Api/Countries.Api.csproj"
 
COPY . .
 
WORKDIR "/src/Services/Countries/Countries.Api"
RUN dotnet build "Countries.Api.csproj" -c Release -o /app/build
 
FROM build AS publish
RUN dotnet publish "Countries.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
 
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Countries.Api.dll"]