# University Course Management API

REST Web API для управления студентами, преподавателями, учебными курсами и записями студентов на курсы. Проект выполнен в рамках дисциплины «Разработка веб-сервисов».

## Возможности

- CRUD-операции для `Student`, `Teacher` и `Course`.
- Создание, просмотр, изменение оценки и удаление `Enrollment`.
- Получение студента вместе с курсами, на которые он записан.
- Фильтрация курсов по тексту, преподавателю и диапазону кредитов.
- Валидация входных DTO и единый формат успешных ответов и ошибок.
- Проверка бизнес-правил: уникальные email, существование преподавателя, студента и курса, запрет повторной записи на курс.

## Технологии

- ASP.NET Core 8 Web API;
- Entity Framework Core 8;
- PostgreSQL 16 и Npgsql;
- AutoMapper;
- Swagger / OpenAPI;
- Docker Compose.

## Архитектура

```text
Client
  ↓ HTTP
Controllers
  ↓
UniversityService
  ↓
Repositories
  ↓
Entity Framework Core / ApplicationDbContext
  ↓
PostgreSQL
```

`Controllers` принимают HTTP-запросы и формируют HTTP-ответы. Бизнес-правила находятся в `UniversityService`. Репозитории инкапсулируют запросы к базе данных. Модели базы данных не используются как внешний API-контракт: для этого применяются DTO и AutoMapper.

## Структура проекта

```text
UniversityCourseManagementAPI/
├── database/
│   └── schema-and-seed.sql       # схема PostgreSQL и тестовые данные
├── src/UniversityCourseManagementAPI/
│   ├── Common/                   # ReturnResult и фабрика API-ответов
│   ├── Controllers/              # HTTP endpoints
│   ├── Data/                     # DbContext и начальное заполнение БД
│   ├── DTO/                      # контракты запросов и ответов
│   ├── Mapping/                  # профиль AutoMapper
│   ├── Middleware/               # централизованная обработка исключений
│   ├── Models/                   # EF Core entity-модели
│   ├── Repositories/             # repository interfaces и реализации
│   └── Services/                 # бизнес-логика
├── docker-compose.yml
└── UniversityCourseManagementAPI.sln
```

## Модель данных

- `Student`: студент; имеет много записей `Enrollment`.
- `Teacher`: преподаватель; ведёт много курсов `Course`.
- `Course`: учебный курс; принадлежит одному преподавателю и имеет много записей `Enrollment`.
- `Enrollment`: запись одного студента на один курс.

В базе есть уникальные индексы на `Student.Email`, `Teacher.Email` и комбинацию `Enrollment(StudentId, CourseId)`. Последнее ограничение дополнительно защищает от повторной записи студента на один курс.

## Запуск

### Требования

- .NET SDK 8.0 или новее;
- Docker Desktop либо локально установленный PostgreSQL.

### 1. Запустить PostgreSQL

Из папки проекта выполните:

```bash
docker compose up -d postgres
```

Контейнер создаёт базу `university_course_management` на `localhost:5432`. Учётные данные по умолчанию: `postgres` / `postgres`.

### 2. Настроить строку подключения при необходимости

Строка подключения находится в `src/UniversityCourseManagementAPI/appsettings.json`:

```json
"DefaultConnection": "Host=localhost;Port=5432;Database=university_course_management;Username=postgres;Password=postgres"
```

### 3. Собрать и запустить API

```bash
dotnet restore UniversityCourseManagementAPI.sln
dotnet build UniversityCourseManagementAPI.sln
dotnet run --project src/UniversityCourseManagementAPI
```

При первом запуске приложение создаёт схему базы данных через EF Core и добавляет тестовые данные. Альтернативно схема и начальные данные находятся в `database/schema-and-seed.sql`.

Swagger доступен по адресу, указанному в консоли запуска, обычно `https://localhost:7xxx/swagger` или `http://localhost:5xxx/swagger`.

## Endpoints

### Students

| Метод | URL | Описание |
|---|---|---|
| GET | `/api/students` | Получить всех студентов |
| GET | `/api/students/{id}` | Получить студента по идентификатору |
| GET | `/api/students/{id}/courses` | Получить студента и его записи на курсы |
| POST | `/api/students` | Создать студента |
| PUT | `/api/students/{id}` | Изменить студента |
| DELETE | `/api/students/{id}` | Удалить студента |

Пример создания студента:

```json
{
  "firstName": "Alikhan",
  "lastName": "Mustafayev",
  "email": "alikhan.mustafayev@example.com",
  "birthDate": "2003-04-12"
}
```

### Teachers

| Метод | URL | Описание |
|---|---|---|
| GET | `/api/teachers` | Получить всех преподавателей |
| GET | `/api/teachers/{id}` | Получить преподавателя по идентификатору |
| POST | `/api/teachers` | Создать преподавателя |
| PUT | `/api/teachers/{id}` | Изменить преподавателя |
| DELETE | `/api/teachers/{id}` | Удалить преподавателя |

### Courses

| Метод | URL | Описание |
|---|---|---|
| GET | `/api/courses` | Получить курсы; поддерживает фильтры |
| GET | `/api/courses/{id}` | Получить курс по идентификатору |
| POST | `/api/courses` | Создать курс и закрепить преподавателя |
| PUT | `/api/courses/{id}` | Изменить курс |
| DELETE | `/api/courses/{id}` | Удалить курс |

Параметры фильтрации `GET /api/courses`:

| Параметр | Пример | Описание |
|---|---|---|
| `search` | `web` | Поиск по названию курса |
| `teacherId` | `1` | Курсы преподавателя |
| `minCredits` | `3` | Минимальное число кредитов |
| `maxCredits` | `6` | Максимальное число кредитов |

Пример: `/api/courses?search=web&minCredits=3&maxCredits=6`.

### Enrollments

| Метод | URL | Описание |
|---|---|---|
| GET | `/api/enrollments` | Получить все записи на курсы |
| GET | `/api/enrollments/{id}` | Получить запись по идентификатору |
| POST | `/api/enrollments` | Записать студента на курс |
| PUT | `/api/enrollments/{id}/grade` | Изменить оценку студента |
| DELETE | `/api/enrollments/{id}` | Удалить запись с курса |

Пример записи на курс:

```json
{
  "studentId": 1,
  "courseId": 1
}
```

Пример изменения оценки:

```json
{
  "grade": 92.5
}
```

## Формат ответа

Все основные endpoints возвращают объект `ReturnResult<T>`:

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "result": {},
  "errorCode": null,
  "errorMessage": null,
  "traceId": "..."
}
```

Пример ошибки:

```json
{
  "statusCode": 404,
  "isSuccess": false,
  "result": null,
  "errorCode": "STUDENT_NOT_FOUND",
  "errorMessage": "Student not found",
  "traceId": "..."
}
```

Используемые коды: `200 OK`, `201 Created`, `400 Bad Request`, `404 Not Found`, `409 Conflict`, `500 Internal Server Error`.

## Валидация и бизнес-правила

- Обязательны имя, фамилия и корректный email студента и преподавателя.
- Для курса обязательны название, преподаватель и число кредитов от 1 до 30.
- Оценка должна быть в диапазоне от 0 до 100.
- Email студента и преподавателя уникален в пределах соответствующей сущности.
- Нельзя создать курс для несуществующего преподавателя.
- Нельзя записать на курс несуществующего студента или на несуществующий курс.
- Повторная запись одного студента на один курс возвращает `409 DUPLICATE_ENROLLMENT`.
- Удаление преподавателя, у которого есть курсы, запрещено и возвращает `409 TEACHER_HAS_COURSES`.

Ошибки валидации автоматически возвращаются в формате `ReturnResult` с кодом `400` и `errorCode: VALIDATION_ERROR`.

## Обработка ошибок и логирование

`ExceptionMiddleware` перехватывает непредвиденные исключения, записывает подробности через `ILogger` и возвращает клиенту безопасный ответ `500 INTERNAL_SERVER_ERROR` без stack trace и деталей SQL.

В журнал записываются события создания, изменения и удаления сущностей. Для диагностики в каждом API-ответе присутствует `traceId`.

## Проверка перед сдачей

В Swagger следует проверить:

1. Получение списка студентов и создание нового студента.
2. Получение и изменение созданного студента.
3. Создание преподавателя и курса с его `teacherId`.
4. Запись студента на курс.
5. Повторную запись на этот же курс — ответ `409`.
6. `GET /api/students/{id}/courses`.
7. Фильтрацию `/api/courses`.
8. Некорректный DTO — ответ `400`.
9. Несуществующий идентификатор — ответ `404`.
