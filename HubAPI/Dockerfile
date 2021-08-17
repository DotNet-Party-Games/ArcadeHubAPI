FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RevArcadeHub/HubAPI/HubAPI.csproj", "RevArcadeHub/HubAPI/"]
RUN dotnet restore "RevArcadeHub\HubAPI\HubAPI.csproj"
COPY . .
WORKDIR "/src/RevArcadeHub/HubAPI"
RUN dotnet build "HubAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HubAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HubAPI.dll"]
