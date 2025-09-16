FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Transport.API.sln", "."]
COPY ["Transport.API/Transport.API.csproj", "Transport.API/"]
COPY ["Transport.Application/Transport.Application.csproj", "Transport.Application/"]
COPY ["Transport.Core/Transport.Core.csproj", "Transport.Core/"]
COPY ["Transport.Data/Transport.Data.csproj", "Transport.Data/"]

RUN dotnet restore "Transport.API.sln"

COPY . .

WORKDIR "/src/Transport.API"
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://*:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "Transport.API.dll"]