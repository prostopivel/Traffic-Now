# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Traffic.API.csproj", "."]
RUN dotnet restore "Traffic.API.csproj"
COPY . .
RUN dotnet publish -c Release -o /app

# Финальный этап
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

# Важно: слушаем на порте, который предоставит Render
ENV ASPNETCORE_URLS=http://*:$PORT
EXPOSE $PORT

# Точка входа
ENTRYPOINT ["dotnet", "Traffic.API.dll"]