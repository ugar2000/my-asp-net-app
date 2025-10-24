FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NetAppForVika.sln ./
COPY src/Server/NetAppForVika.Server.csproj src/Server/
COPY src/Shared/NetAppForVika.Shared.csproj src/Shared/
RUN dotnet restore NetAppForVika.sln

COPY . .
RUN dotnet publish src/Server/NetAppForVika.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NetAppForVika.Server.dll"]
