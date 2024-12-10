FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5269

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WindSurfApi.csproj", "./"]
RUN dotnet restore "WindSurfApi.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "WindSurfApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WindSurfApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Assurez-vous que le dossier data existe et copiez le fichier CSV
RUN mkdir -p /app/data
COPY ["data/source.csv", "/app/data/"]

ENV ASPNETCORE_URLS=http://+:5269
ENTRYPOINT ["dotnet", "WindSurfApi.dll"]
