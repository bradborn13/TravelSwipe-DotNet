FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
 
COPY ["Services/Activities/Activities.Api/Activities.Api.csproj", "Services/Activities/Activities.Api/"]
COPY ["Services/Activities/Activities.Application/Activities.Application.csproj", "Services/Activities/Activities.Application/"]
COPY ["Services/Activities/Activities.Core/Activities.Core.csproj", "Services/Activities/Activities.Core/"]
COPY ["Services/Activities/Activities.Infrastructure/Activities.Infrastructure.csproj", "Services/Activities/Activities.Infrastructure/"]
 
RUN dotnet restore "Services/Activities/Activities.Api/Activities.Api.csproj"
 
COPY . .
 
WORKDIR "/src/Services/Activities/Activities.Api"
RUN dotnet build "Activities.Api.csproj" -c Release -o /app/build
 
FROM build AS publish
RUN dotnet publish "Activities.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
 
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Activities.Api.dll"]