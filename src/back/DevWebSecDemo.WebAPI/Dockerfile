# Consultez https://aka.ms/customizecontainer pour savoir comment personnaliser votre conteneur de débogage et comment Visual Studio utilise ce Dockerfile pour générer vos images afin d’accélérer le débogage.

# Cet index est utilisé lors de l'exécution à partir de VS en mode rapide (par défaut pour la configuration de débogage)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Cette phase est utilisée pour générer le projet de service
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DevWebSecDemo.WebAPI/DevWebSecDemo.WebAPI.csproj", "DevWebSecDemo.WebAPI/"]
COPY ["DevWebSecDemo.Business/DevWebSecDemo.Business.csproj", "DevWebSecDemo.Business/"]
COPY ["DevWebSecDemo.Providers/DevWebSecDemo.Providers.csproj", "DevWebSecDemo.Providers/"]
COPY ["DevWebSecDemo.Entities/DevWebSecDemo.Entities.csproj", "DevWebSecDemo.Entities/"]
RUN dotnet restore "./DevWebSecDemo.WebAPI/DevWebSecDemo.WebAPI.csproj"
COPY . .
WORKDIR "/src/DevWebSecDemo.WebAPI"
RUN dotnet build "./DevWebSecDemo.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Cette étape permet de publier le projet de service à copier dans la phase finale
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DevWebSecDemo.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Cette phase est utilisée en production ou lors de l’exécution à partir de VS en mode normal (par défaut quand la configuration de débogage n’est pas utilisée)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "DevWebSecDemo.WebAPI.dll"]