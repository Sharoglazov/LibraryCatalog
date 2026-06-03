 LIBRARY CATALOG API
  RESTful Web API для библиотечного каталога
  .NET 8 Minimal API + Dapper + SQLite + Docker
=====================================================

ОПИСАНИЕ

Полный CRUD книг с фильтрацией, валидацией, Swagger-документацией
и сохранением данных между перезапусками контейнера через volume.

ТРЕБОВАНИЯ

- .NET 8 SDK (для локального запуска)
- Docker (для контейнеризации)


  ЗАПУСК БЕЗ DOCKER

1. Клонируйте репозиторий:
   git clone https://github.com/your-username/LibraryCatalog.git
   cd LibraryCatalog

2. Перейдите в папку основного проекта:
   cd LibraryCatalog

3. Запустите:
   dotnet run

4. Откройте Swagger:
   http://localhost:<порт>/swagger   (порт указан в консоли)


  ЗАПУСК В DOCKER


СБОРКА ОБРАЗА (из корня репозитория, где лежит Dockerfile):
   docker build -t library-api .

ЗАПУСК КОНТЕЙНЕРА:
   docker run -d -p 5000:8080 -v ./data:/app/data --name library-api-container library-api

Параметры:
   -d                     – фоновый режим
   -p 5000:8080           – порт хоста 5000 -> порт контейнера 8080
   -v ./data:/app/data    – volume для сохранения books.db
   --name library-api-container – имя контейнера

После запуска API доступно: http://localhost:5000/swagger

ПРОВЕРКА СОХРАННОСТИ ДАННЫХ:
   1. Создайте книгу через Swagger (POST /api/books).
   2. Остановите и удалите контейнер:
        docker stop library-api-container
        docker rm library-api-container
   3. Запустите заново той же командой docker run ...
   4. Выполните GET /api/books – книга на месте.


  ПРИМЕРЫ ЗАПРОСОВ (curl)


- Получить все книги (фильтр):
  curl -X GET "http://localhost:5000/api/books?genre=Роман&isAvailable=true"

- Получить книгу по ID:
  curl -X GET "http://localhost:5000/api/books/1"

- Добавить книгу:
  curl -X POST "http://localhost:5000/api/books" ^
    -H "Content-Type: application/json" ^
    -d "{\"title\":\"Война и мир\",\"author\":\"Лев Толстой\",\"publishedYear\":1869,\"genre\":\"Роман\",\"isAvailable\":true}"

- Полностью обновить:
  curl -X PUT "http://localhost:5000/api/books/1" ^
    -H "Content-Type: application/json" ^
    -d "{\"title\":\"Новое издание\",\"author\":\"Л. Толстой\",\"publishedYear\":1873,\"genre\":\"Классика\",\"isAvailable\":false}"

- Удалить:
  curl -X DELETE "http://localhost:5000/api/books/1"

- Изменить доступность (PATCH):
  curl -X PATCH "http://localhost:5000/api/books/1/availability" ^
    -H "Content-Type: application/json" ^
    -d "{\"isAvailable\":false}"


  ТЕСТЫ


Запуск всех тестов (из корня решения):
   dotnet test

Или из папки Tests:
   cd Tests
   dotnet test


  СТРУКТУРА ПРОЕКТА


LibraryCatalog/               – основной проект Minimal API
  Models/                     – Book, ValidationErrorResponse
  Repositories/               – IBookRepository, BookRepository (Dapper)
  Endpoints/                  – маршруты
  Program.cs                  – точка входа
  appsettings.json            – строка подключения

Tests/                        – тесты (xUnit)
  UnitTests/
  IntegrationTests/

Dockerfile                    – многоэтапная сборка
README.txt (или .md)          – документация
.gitignore


  ПРИМЕЧАНИЯ


- База данных books.db создаётся автоматически в папке data/.
- В Docker папка data монтируется с хоста.
- Внутренний порт контейнера 8080, наружу пробрасывается 5000.
- Валидация: Title (обязателен, ≤200), Author (обязателен),
  PublishedYear (1450 – текущий год), Genre необязателен.
