# University Course Management API

Учебный проект по дисциплине **CSE5032 «Разработка веб-сервисов»**: REST Web API для управления студентами, преподавателями, курсами и записями на курсы.

| Часть | Модули | Что реализовано |
|---|---|---|
| Задание 1 | 1–4 | REST API, CRUD, Repository, DTO, AutoMapper, валидация, единый формат ответов, обработка ошибок, PostgreSQL |
| Задание 2 | 5–8 | CORS и MVC-клиент, ASP.NET Core Identity, JWT, роли и политики, проверка владения ресурсом, версии API v1/v2, EF Core Migrations |

Задание 2 является продолжением задания 1: функциональность первой части сохранена и продолжает работать.

## Содержание

1. [Технологии](#технологии)
2. [Структура репозитория](#структура-репозитория)
3. [Быстрый старт](#быстрый-старт)
4. [Конфигурация и секреты](#конфигурация-и-секреты)
5. [База данных и миграции](#база-данных-и-миграции)
6. [Тестовые пользователи и данные](#тестовые-пользователи-и-данные)
7. [Архитектура](#архитектура)
8. [Прохождение запроса через аутентификацию и авторизацию](#прохождение-запроса-через-аутентификацию-и-авторизацию)
9. [Аутентификация: Identity + JWT](#аутентификация-identity--jwt)
10. [Авторизация: роли, политики, владение ресурсом](#авторизация-роли-политики-владение-ресурсом)
11. [Версии API](#версии-api)
12. [MVC-клиент и CORS](#mvc-клиент-и-cors)
13. [Endpoints](#endpoints)
14. [Формат ответов и коды ошибок](#формат-ответов-и-коды-ошибок)
15. [Комплексные сценарии безопасности](#комплексные-сценарии-безопасности)
16. [Проверка через Swagger и Postman](#проверка-через-swagger-и-postman)
17. [Вопросы к защите](#вопросы-к-защите)

## Технологии

- ASP.NET Core 8 Web API, ASP.NET Core MVC (клиент);
- Entity Framework Core 8 + Npgsql, PostgreSQL 16;
- ASP.NET Core Identity, JWT Bearer Authentication;
- Asp.Versioning (версионирование API);
- AutoMapper, Swagger / OpenAPI (Swashbuckle);
- Docker Compose.

## Структура репозитория

```text
.
├── README.md                                  # этот файл
└── UniversityCourseManagementAPI/
    ├── UniversityCourseManagementAPI.sln       # решение: API + клиент
    ├── docker-compose.yml                      # PostgreSQL 16
    ├── dotnet-tools.json                       # локальный инструмент dotnet-ef 8.0.11
    ├── .env.example                            # шаблон переменных окружения (без реальных секретов)
    ├── database/
    │   ├── schema-and-seed.sql                 # схема и тестовые данные части 1
    │   ├── 002-security-identity.sql           # таблицы Identity для БД, созданной в части 1
    │   └── migrations-idempotent.sql           # SQL-скрипт EF Core Migrations
    ├── postman/
    │   └── UniversityCourseManagementAPI.postman_collection.json
    └── src/
        ├── UniversityCourseManagementAPI/      # Web API
        │   ├── Common/                         # ReturnResult<T>, фабрика ответов ApiResponses
        │   ├── Controllers/                    # Students, Teachers, Courses, Enrollments, Auth, CourseVersions (v1/v2)
        │   ├── Data/                           # ApplicationDbContext, Migrations, DatabaseMigrator, DbSeeder
        │   ├── DTO/                            # контракты запросов и ответов (в т. ч. Auth, CourseV1/V2)
        │   ├── Mapping/                        # профиль AutoMapper
        │   ├── Middleware/                     # ExceptionMiddleware
        │   ├── Models/                         # Student, Teacher, Course, Enrollment, ApplicationUser
        │   ├── Repositories/                   # интерфейсы и реализации репозиториев
        │   └── Services/                       # UniversityService, AuthService, Roles, Policies, JwtOptions
        └── UniversityCourseManagementClient/   # MVC-клиент (без доступа к БД)
            ├── Controllers/  Models/  Services/CourseApiClient.cs  Views/
```

## Быстрый старт

Требования: .NET SDK 8.0+, Docker Desktop (или локальный PostgreSQL 16).

```bash
cd UniversityCourseManagementAPI

# 1. PostgreSQL (для чистой БД: docker compose down -v перед этим)
docker compose up -d postgres

# 2. Секреты — только через переменные окружения, не в appsettings.json
export Jwt__Key='replace-this-with-a-unique-local-secret-at-least-32-characters-long'
export Seed__AdminPassword='Admin123!'
export Seed__TestUserPassword='User123!'

# 3. API: https://localhost:7001/swagger
dotnet run --project src/UniversityCourseManagementAPI

# 4. MVC-клиент (в другом терминале): https://localhost:7101
dotnet run --project src/UniversityCourseManagementClient
```

При старте API автоматически применяет миграции, создаёт роли и тестовых пользователей. На пустой БД также создаются тестовые студенты, преподаватели, курсы и записи.

## Конфигурация и секреты

| Ключ | Где задаётся | Назначение |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `appsettings.json` | строка подключения к PostgreSQL |
| `Jwt:Issuer`, `Jwt:Audience` | `appsettings.json` | издатель и аудитория токена |
| `Jwt:AccessTokenMinutes` | `appsettings.json` (30) | срок действия access token |
| `Jwt__Key` | **переменная окружения / user-secrets** | ключ подписи JWT, не короче 32 символов |
| `Seed__AdminPassword` | **переменная окружения** | пароль пользователя `admin` |
| `Seed__TestUserPassword` | **переменная окружения** | пароль пользователей `teacher1`, `student1` |
| `Api:BaseUrl` | `appsettings.json` клиента | адрес API для MVC-клиента |

- Ключ подписи и пароли отсутствуют в исходном коде и Git; шаблон — `UniversityCourseManagementAPI/.env.example`, файл `.env` исключён через `.gitignore`.
- Без `Jwt__Key` длиной не менее 32 символов API не запускается — это защищает от запуска с пустым или слабым ключом.
- Вместо `export` можно использовать `dotnet user-secrets set "Jwt:Key" "..." --project src/UniversityCourseManagementAPI`.

## База данных и миграции

Схема управляется **EF Core Migrations** (`src/UniversityCourseManagementAPI/Data/Migrations`). Миграция `InitialCreate` содержит доменные таблицы (`Students`, `Teachers`, `Courses`, `Enrollments`) и таблицы ASP.NET Core Identity (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` и др.). При запуске API вызывается `Database.MigrateAsync()`.

| Ситуация | Что происходит |
|---|---|
| Новая пустая БД | миграции применяются автоматически при старте |
| БД из части 1 (создана `EnsureCreated`, без истории миграций) | `DatabaseMigrator` находит существующие таблицы, записывает `InitialCreate` как применённую и применяет только новые миграции. Если таблиц Identity ещё нет — один раз выполнить `database/002-security-identity.sql` |
| Развёртывание без запуска приложения | выполнить `database/migrations-idempotent.sql` |

Работа с миграциями (из папки `UniversityCourseManagementAPI`):

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project src/UniversityCourseManagementAPI --output-dir Data/Migrations
dotnet ef database update --project src/UniversityCourseManagementAPI
dotnet ef migrations script --idempotent --project src/UniversityCourseManagementAPI -o database/migrations-idempotent.sql
```

**Модель данных.** `Teacher` 1 — * `Course`; `Student` * — * `Course` через `Enrollment` (с оценкой `Grade`). `ApplicationUser` (Identity) связан с `Student` через `StudentId` или с `Teacher` через `TeacherId`. Уникальные индексы: `Student.Email`, `Teacher.Email`, `Enrollment(StudentId, CourseId)`.

## Тестовые пользователи и данные

`DbSeeder` при запуске создаёт роли `Admin`, `Teacher`, `Student` через `RoleManager` и учётные записи через `UserManager` (пароли хешируются Identity):

| Логин | Пароль | Роль | Связанная сущность |
|---|---|---|---|
| `admin` | `Seed__AdminPassword` | Admin | — |
| `teacher1` | `Seed__TestUserPassword` | Teacher | `Teacher` #1 Amina Sadykova (ведёт «Web Services») |
| `student1` | `Seed__TestUserPassword` | Student | `Student` #1 Alikhan Mustafayev |

Данные для сценариев безопасности (создаются на пустой БД):

| Enrollment | Студент | Курс (преподаватель) | Используется в сценарии |
|---|---|---|---|
| 1 | student1 | Web Services (teacher1) | Teacher меняет оценку на своём курсе → 200 |
| 2 | student1 | Databases (Daniyar Akhmetov) | Teacher меняет оценку на чужом курсе → 403 |
| 3 | Aruzhan Bekova (#2) | Web Services (teacher1) | Student запрашивает чужую оценку → 403 |

## Архитектура

```text
Client (Swagger / Postman / MVC-клиент)
  ↓ HTTP
Middleware: Exception → HTTPS → CORS → Authentication → Authorization
  ↓
Controllers (HTTP, атрибуты [Authorize]/[AllowAnonymous], проверка владения ресурсом)
  ↓
Services (UniversityService — бизнес-правила; AuthService — Identity и JWT)
  ↓
Repositories
  ↓
EF Core / ApplicationDbContext (IdentityDbContext<ApplicationUser>)
  ↓
PostgreSQL
```

Сущности БД не используются как внешний контракт — наружу отдаются только DTO (маппинг через AutoMapper). Бизнес-логика не дублируется между версиями API: v1 и v2 используют общий `ICourseRepository` и различаются только DTO.

## Прохождение запроса через аутентификацию и авторизацию

```text
Клиент
  │  GET/PUT ...  Authorization: Bearer <JWT>
  ▼
ExceptionMiddleware ─── необработанное исключение → 500 ReturnResult (без stack trace)
  ▼
CORS (политика ClientApplications: только https://localhost:7101 и http://localhost:5101)
  ▼
UseAuthentication ─── JwtBearer проверяет подпись, Issuer, Audience, срок действия
  │                    ├─ токена нет → запрос анонимный
  │                    ├─ токен просрочен/изменён/некорректен → не аутентифицирован
  │                    └─ ок → HttpContext.User = ClaimsPrincipal (id, name, roles)
  ▼
UseAuthorization ─── endpoint с [AllowAnonymous] → пропуск
  │                   [Authorize(Policy = ...)]:
  │                     ├─ пользователь не аутентифицирован → 401 Unauthorized
  │                     └─ нет нужной роли (политика не выполнена) → 403 Forbidden
  ▼
Controller ─── загрузка ресурса и проверка владения:
  │             Teacher: ApplicationUser.TeacherId == Course.TeacherId ?
  │             Student: ApplicationUser.StudentId == Enrollment.StudentId ?
  │             нет → 403 Forbidden
  ▼
Service → Repository → EF Core → PostgreSQL
  ▼
200 OK + ReturnResult<DTO> (без конфиденциальных полей)
```

## Аутентификация: Identity + JWT

| Метод | URL | Доступ | Описание |
|---|---|---|---|
| POST | `/api/auth/register` | аноним | регистрация; роль всегда `Student`, привязка к существующему `StudentId` |
| POST | `/api/auth/login` | аноним | проверка пароля через `SignInManager.CheckPasswordSignInAsync`, выдача JWT |
| GET | `/api/auth/me` | аутентифицированный | текущий пользователь, роли, `StudentId` / `TeacherId` |
| PUT | `/api/auth/users/{userId}/role` | Admin | назначение роли и привязка к Teacher/Student |

**ASP.NET Core Identity**

- Подключён через `AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()`.
- Используются `UserManager` (создание, роли, поиск), `SignInManager` (проверка пароля с учётом блокировки), `RoleManager` (создание ролей).
- Пароль: не менее 8 символов, цифра, заглавная и строчная буква, спецсимвол.
- Имя пользователя и email уникальны: повторная регистрация → `409 USERNAME_CONFLICT` / `EMAIL_CONFLICT`.
- Блокировка: после 5 неудачных попыток аккаунт блокируется на 15 минут → `401 ACCOUNT_LOCKED`, вход невозможен даже с верным паролем.
- Пароли хранятся только как хеши Identity (PBKDF2); собственный алгоритм хеширования не используется.
- Регистрация не даёт административных прав; роль меняет только Admin — самостоятельное повышение привилегий невозможно.

**JWT**

- Claims: `sub` и `nameidentifier` — ID пользователя, `name` и `unique_name` — имя пользователя, `role` — каждая роль.
- Подпись HMAC-SHA256 ключом `Jwt__Key`; проверяются подпись, `Issuer`, `Audience`, срок действия; `ClockSkew = 0`.
- Срок действия — `Jwt:AccessTokenMinutes` (30 минут).
- Передача: заголовок `Authorization: Bearer <token>`.
- Просроченный, изменённый или некорректный токен → `401 UNAUTHORIZED`.
- JWT назначен схемой по умолчанию (authenticate/challenge/forbid). Без этого `AddIdentity` подставил бы cookie-схему и `[Authorize]` отвечал бы редиректом `302 /Account/Login` вместо `401`.

**Защита данных**

- Наружу отдаются только DTO: в `CurrentUserDto` нет `PasswordHash`, `SecurityStamp` и других служебных полей.
- Анонимный `GET /api/teachers` возвращает `TeacherPublicDto` без email; полный `TeacherDto` получает только Admin. Данные студентов доступны только Admin (и студенту — только свои курсы и оценки).
- В логи пишутся только ID пользователей; пароли, токены и ключи не логируются.

## Авторизация: роли, политики, владение ресурсом

| Роль | Полномочия |
|---|---|
| Admin | управление пользователями и ролями, полный доступ к университетским данным |
| Teacher | просмотр курсов, изменение оценок только по собственным курсам |
| Student | просмотр курсов, собственных записей и оценок |

Политики объявлены в `Program.cs` (`AddAuthorization`) и применяются атрибутом `[Authorize(Policy = ...)]`:

| Политика | Требование | Применение |
|---|---|---|
| `AdminOnly` | роль Admin | CRUD студентов, преподавателей, курсов и записей; `GET /api/students`; назначение ролей |
| `CanGrade` | Admin или Teacher | `PUT /api/enrollments/{id}/grade` |
| `CanViewEnrollment` | Admin или Student | `GET /api/enrollments/{id}`, `GET /api/students/{id}/courses` |
| `StudentOnly` | Student | `GET /api/enrollments/my` |

Публичные (`[AllowAnonymous]`): `GET /api/courses`, `GET /api/courses/{id}`, `GET /api/v1/courses…`, `GET /api/v2/courses…`, `GET /api/teachers…`, `POST /api/auth/login`, `POST /api/auth/register`.

**Проверка владения ресурсом.** Политика пропускает Teacher к изменению оценок, но не по любому курсу. Поэтому контроллер загружает запись и сравнивает `TeacherId` учётной записи с `Course.TeacherId`; для студента — `StudentId` учётной записи с `Enrollment.StudentId`. Несовпадение → `403`. Все проверки выполняются на сервере; клиент ничего не решает за API.

## Версии API

Версионирование — пакет `Asp.Versioning`, версия в сегменте URL; в ответе есть заголовок `api-supported-versions: 1.0, 2.0`.

| Версия | Маршруты | DTO |
|---|---|---|
| v1 | `GET /api/v1/courses`, `GET /api/v1/courses/{id}` | `CourseV1Dto`: id, name, description, credits, teacherId, createdAt |
| v2 | `GET /api/v2/courses`, `GET /api/v2/courses/{id}` | `CourseV2Dto`: поля v1 + объект `teacher` (имя, фамилия, кафедра) + `enrollmentCount` |

- Обе версии доступны одновременно; контракт v1 не изменился, поэтому клиенты v1 продолжают работать.
- Для v2 используется отдельный DTO; общая логика доступа к данным — `ICourseRepository`.
- В Swagger UI два документа: **University API v1** и **University API v2**.
- Маршруты первой части (`/api/courses` и др.) сохранены без изменений.

Пример ответа v2:

```json
{
  "id": 1,
  "name": "Web Services",
  "description": "REST APIs and ASP.NET Core",
  "credits": 5,
  "createdAt": "2026-10-22T06:33:53Z",
  "teacher": { "id": 1, "firstName": "Amina", "lastName": "Sadykova", "department": "Software Engineering" },
  "enrollmentCount": 2
}
```

## MVC-клиент и CORS

`UniversityCourseManagementAPI/src/UniversityCourseManagementClient` — отдельное ASP.NET Core MVC-приложение, которое **не обращается к БД** и получает данные только по HTTP.

- HTTP-запросы — typed client `CourseApiClient`, зарегистрированный через `AddHttpClient` (`IHttpClientFactory`), таймаут 10 секунд.
- Адрес API — `Api:BaseUrl` в `appsettings.json` клиента, в коде не указан.
- Список курсов берётся из **API v1**, страница курса — из **API v2** (преподаватель, кафедра, число студентов).
- Обработка ошибок: API не запущен → «The University API is unavailable»; таймаут → «timed out»; `404` → «Course not found»; другие неуспешные коды → «API returned <код>».

CORS: именованная политика `ClientApplications` разрешает ровно два адреса — `https://localhost:7101` и `http://localhost:5101` (адреса клиента). Запросы с других origin не получают заголовок `Access-Control-Allow-Origin`.

## Endpoints

### Auth

| Метод | URL | Доступ |
|---|---|---|
| POST | `/api/auth/register` | аноним |
| POST | `/api/auth/login` | аноним |
| GET | `/api/auth/me` | аутентифицированный |
| PUT | `/api/auth/users/{userId}/role` | Admin |

### Students

| Метод | URL | Доступ |
|---|---|---|
| GET | `/api/students` | Admin |
| GET | `/api/students/{id}` | Admin |
| GET | `/api/students/{id}/courses` | Admin; Student — только свой id |
| POST | `/api/students` | Admin |
| PUT | `/api/students/{id}` | Admin |
| DELETE | `/api/students/{id}` | Admin |

### Teachers

| Метод | URL | Доступ |
|---|---|---|
| GET | `/api/teachers` | аноним (без email); Admin — полные данные |
| GET | `/api/teachers/{id}` | аноним (без email); Admin — полные данные |
| POST / PUT / DELETE | `/api/teachers`, `/api/teachers/{id}` | Admin |

### Courses

| Метод | URL | Доступ |
|---|---|---|
| GET | `/api/courses?search=&teacherId=&minCredits=&maxCredits=` | аноним |
| GET | `/api/courses/{id}` | аноним |
| POST / PUT / DELETE | `/api/courses`, `/api/courses/{id}` | Admin |
| GET | `/api/v1/courses`, `/api/v1/courses/{id}` | аноним |
| GET | `/api/v2/courses`, `/api/v2/courses/{id}` | аноним |

### Enrollments

| Метод | URL | Доступ |
|---|---|---|
| GET | `/api/enrollments` | Admin |
| GET | `/api/enrollments/my` | Student — свои записи и оценки |
| GET | `/api/enrollments/{id}` | Admin; Student — только своя запись |
| POST | `/api/enrollments` | Admin |
| PUT | `/api/enrollments/{id}/grade` | Admin; Teacher — только по своему курсу |
| DELETE | `/api/enrollments/{id}` | Admin |

Примеры тел запросов:

```json
// POST /api/auth/login
{ "userName": "teacher1", "password": "User123!" }

// POST /api/auth/register
{ "userName": "newstudent", "email": "newstudent@university.local", "password": "Passw0rd!", "studentId": 2 }

// PUT /api/auth/users/{userId}/role
{ "role": "Teacher", "teacherId": 1 }

// PUT /api/enrollments/1/grade
{ "grade": 92.5 }
```

## Формат ответов и коды ошибок

Все endpoints возвращают `ReturnResult<T>`, включая ответы 401/403, сформированные middleware:

```json
{
  "statusCode": 403,
  "isSuccess": false,
  "result": null,
  "errorCode": "COURSE_ACCESS_DENIED",
  "errorMessage": "You can update grades only for your own courses",
  "traceId": "0HNPG0POVC5BS:00000001"
}
```

| Код | Когда | Примеры `errorCode` |
|---|---|---|
| 200 / 201 | успех | — |
| 400 | ошибка валидации DTO / фильтра | `VALIDATION_ERROR`, `INVALID_FILTER`, `INVALID_ROLE` |
| 401 | нет токена, токен неверен/просрочен, неверный пароль, блокировка | `UNAUTHORIZED`, `INVALID_CREDENTIALS`, `ACCOUNT_LOCKED` |
| 403 | пользователь известен, но прав нет | `FORBIDDEN`, `COURSE_ACCESS_DENIED`, `ENROLLMENT_ACCESS_DENIED`, `STUDENT_ACCESS_DENIED` |
| 404 | ресурс не найден | `STUDENT_NOT_FOUND`, `COURSE_NOT_FOUND`, `USER_NOT_FOUND` |
| 409 | конфликт | `DUPLICATE_ENROLLMENT`, `EMAIL_CONFLICT`, `USERNAME_CONFLICT`, `TEACHER_HAS_COURSES` |
| 500 | непредвиденная ошибка | `INTERNAL_SERVER_ERROR` (детали только в логе) |

Бизнес-правила первой части сохранены: уникальные email, проверка существования преподавателя/студента/курса, запрет повторной записи на курс, запрет удаления преподавателя с курсами, оценка 0–100, кредиты 1–30.

## Комплексные сценарии безопасности

Все сценарии проверены на чистой БД с тестовыми данными `DbSeeder`.

| № | Сценарий | Запрос | Ожидаемый результат |
|---|---|---|---|
| 1 | Аноним запрашивает список курсов | `GET /api/courses` | 200 OK |
| 2 | Аноним создаёт курс | `POST /api/courses` | 401 Unauthorized |
| 3 | Неправильный пароль | `POST /api/auth/login` | 401 `INVALID_CREDENTIALS`, JWT не выдаётся |
| 4 | Правильные данные | `POST /api/auth/login` | 200, `accessToken` |
| 5 | Просроченный JWT | `GET /api/auth/me` | 401 Unauthorized |
| 6 | Student удаляет курс | `DELETE /api/courses/1` (student1) | 403 Forbidden* |
| 7 | Teacher меняет оценку на своём курсе | `PUT /api/enrollments/1/grade` (teacher1) | 200 OK |
| 8 | Teacher меняет оценку на чужом курсе | `PUT /api/enrollments/2/grade` (teacher1) | 403 Forbidden |
| 9 | Student запрашивает чужие оценки | `GET /api/enrollments/3`, `GET /api/students/2/courses` (student1) | 403 Forbidden |
| 10 | Admin назначает роль | `PUT /api/auth/users/{id}/role` (admin) | 200 OK |
| 11 | Student назначает себе Admin | `PUT /api/auth/users/{id}/role` (student1) | 403 Forbidden |
| 12 | Клиент обращается к v1 и v2 | `GET /api/v1/courses/1`, `GET /api/v2/courses/1`; MVC-клиент | 200, оба контракта корректны |

\* В таблице задания для сценария 6 указан `401`. Однако студент **аутентифицирован** (токен валиден) и лишь не имеет прав — по определению это `403 Forbidden`. Тот же запрос без токена возвращает `401` (сценарий 2).

**Демонстрация сценария 5 вживую:** запустить API с `export Jwt__AccessTokenMinutes=1`, получить токен, подождать минуту и повторить запрос. Изменённый токен (любой символ подписи) также даёт `401`.

## Проверка через Swagger и Postman

**Swagger** — `https://localhost:7001/swagger`, в правом верхнем углу выбор документа v1/v2.

1. `POST /api/auth/login` с `admin` / `teacher1` / `student1`.
2. Скопировать `result.accessToken`, нажать **Authorize**, вставить токен (без слова `Bearer`).
3. Вызвать защищённый endpoint, например `GET /api/auth/me`. Кнопка **Logout** в окне Authorize — для проверки того же запроса без токена (401).

**Postman** — `UniversityCourseManagementAPI/postman/UniversityCourseManagementAPI.postman_collection.json`.

1. Импортировать коллекцию и задать переменные `adminPassword`, `testUserPassword` (и `baseUrl`, если адрес другой).
2. Выполнить папку **«0. Вход»** — токены всех ролей сохраняются в переменные автоматически.
3. Выполнить папку **«7. Комплексные сценарии безопасности»** — у каждого запроса есть тест ожидаемого кода ответа.

## Вопросы к защите

**Аутентификация и авторизация — в чём разница?**
Аутентификация отвечает на вопрос «кто выполняет запрос?»: JwtBearer проверяет токен и создаёт `ClaimsPrincipal`. Если токена нет или он недействителен, ответ `401 Unauthorized`. Авторизация отвечает на вопрос «что этому пользователю разрешено?»: проверяются политика, роль и владение ресурсом. Если пользователь известен, но прав нет, ответ `403 Forbidden`. Пример: `DELETE /api/courses/1` без токена → 401, с токеном `student1` → 403.

**Как связаны Identity, JWT, Claims, Roles и middleware?**
- **ASP.NET Core Identity** хранит пользователей, хеши паролей и роли в БД (`UserManager`, `SignInManager`, `RoleManager`).
- **Login.** `AuthService` проверяет пароль через Identity, превращает ID, имя и роли пользователя в **Claims** и подписывает их в **JWT**.
- **Authentication Middleware.** На каждом запросе проверяет подпись и срок JWT и восстанавливает из него `ClaimsPrincipal` (`HttpContext.User`).
- **Authorization Middleware.** Сопоставляет claims типа `role` (**Roles**) с требованиями политики endpoint'а.
- **Итог.** Сервер не хранит сессию, вся информация о пользователе находится в подписанном токене.

**Почему проверки роли недостаточно?**
Роль описывает тип пользователя, но не говорит, какие данные ему принадлежат. Роль Teacher позволяет менять оценки, но если проверять только её, любой преподаватель сможет изменить оценку на чужом курсе. Это уязвимость IDOR (Insecure Direct Object Reference): доступ к чужому объекту по его id. Поэтому после проверки роли сервер загружает ресурс и сравнивает его владельца (`Course.TeacherId`, `Enrollment.StudentId`) со связанной сущностью из учётной записи (`ApplicationUser.TeacherId` / `StudentId`).

**Почему важен порядок middleware?**
`UseAuthentication` должен стоять до `UseAuthorization`, потому что авторизации нужен уже заполненный `HttpContext.User`. `UseCors` стоит перед ними, чтобы preflight-запросы (`OPTIONS`) получали CORS-заголовки без аутентификации.
