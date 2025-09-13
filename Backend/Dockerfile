FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Traffic.API.sln", "."]
COPY ["Traffic.API/Traffic.API.csproj", "Traffic.API/"]
COPY ["Traffic.Application/Traffic.Application.csproj", "Traffic.Application/"]
COPY ["Traffic.Core/Traffic.Core.csproj", "Traffic.Core/"]
COPY ["Traffic.Data/Traffic.Data.csproj", "Traffic.Data/"]

RUN dotnet restore "Traffic.API.sln"

COPY . .

WORKDIR "/src/Traffic.API"
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://*:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "Traffic.API.dll"]