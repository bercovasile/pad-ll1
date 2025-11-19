
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["University.Api/University.Api.csproj", "University.Api/"]
COPY ["University.Persistance/University.Persistance.csproj", "University.Persistance/"]
COPY ["University.Domain/University.Domain.csproj", "University.Domain/"]
RUN dotnet restore "./University.Api/University.Api.csproj"
COPY . .
WORKDIR "/src/University.Api"
RUN dotnet build "./University.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./University.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "University.Api.dll"]