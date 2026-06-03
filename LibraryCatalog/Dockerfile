# ============ Этап 1: Сборка ============
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем только файлы проекта для восстановления зависимостей (кэширование слоёв)
COPY ["LibraryCatalog/LibraryCatalog.csproj", "LibraryCatalog/"]
RUN dotnet restore "LibraryCatalog/LibraryCatalog.csproj"

# Копируем все исходники
COPY . .
WORKDIR "/src/LibraryCatalog"

# Публикуем приложение в папку /app/publish
RUN dotnet publish "LibraryCatalog.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Запуск
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Открываем порт 8080 (для документации)
EXPOSE 8080

# Устанавливаем переменную окружения, чтобы Kestrel слушал порт 8080
ENV ASPNETCORE_URLS=http://+:8080

# Копируем опубликованное приложение из этапа сборки
COPY --from=build /app/publish .

# Запускаем приложение
ENTRYPOINT ["dotnet", "LibraryCatalog.dll"]