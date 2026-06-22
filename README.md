# BookingSystem — система бронирования

Отдельное .NET 8 решение для записи клиентов к сотрудникам (календарь-планировщик
в духе салонной записи). Работает на **той же базе**, что и LoyaltySystem, но в
**собственных таблицах `Booking_*`**; легаси-таблицы и таблицы аренды (`Renting*`,
`SingleServiceRenting*`) не изменяются.

## Архитектура (без MediatR)

| Проект | Назначение |
|---|---|
| `BookingSystem.Domain` | Сущности (`Booking`, `BookingServiceItem`, `BookingResource`, `BookingServiceSetting`) + enum `BookingLabel` |
| `BookingSystem.Shared` | DTO/Request/Response + интерфейсы сервисов (`IBookingService`, `ICatalogService`, `IResourceService`, `IClientService`) |
| `BookingSystem.Infrastructure` | EF Core `BookingDbContext`, read-only маппинг легаси (`ExcludeFromMigrations`), реализации сервисов, миграции |
| `BookingSystem.WebApi` | Контроллеры → сервисы (DI), Swagger, CORS, `/health` |
| `BookingSystem.WebUI` | Blazor WebAssembly + Syncfusion Scheduler |

Поток: **Controller → Service → BookingDbContext**.

## База данных

- Локальная: `Server=localhost;Database=PPS_3_1;Trusted_Connection=True;TrustServerCertificate=True;`
  (задано в `BookingSystem.WebApi/appsettings.Development.json` и в `BookingDbContextFactory`).
- Переопределить можно через `ConnectionStrings:DefaultConnection` или переменную `BOOKING_CONNECTION` (для `dotnet ef`).

### Собственные таблицы
- `Booking_Resource` — сотрудники-ресурсы (колонки планировщика).
- `Booking_ServiceSetting` — длительность/перерыв/цвет услуги (то, чего нет в `SingleService`).
- `Booking_Booking` — записи.
- `Booking_BookingService` — выбранные услуги записи (снапшоты цены/длительности/перерыва).

### Переиспользуемые легаси-таблицы (только чтение)
- `SingleService`, `SingleServiceGroup` — каталог услуг.
- `CashboxVisitor` — клиенты и сотрудники (`SotrudnikStatus=1`).

## Запуск

```powershell
# 1) Применить миграции (создаст только Booking_* в PPS_3_1)
dotnet ef database update --project BookingSystem.Infrastructure --startup-project BookingSystem.Infrastructure

# 2) (однократно) демо-данные: настройки услуг + ресурсы
sqlcmd -S localhost -E -d PPS_3_1 -i BookingSystem.Infrastructure/Migrations/Seed_Demo.sql

# 3) API
dotnet run --project BookingSystem.WebApi --urls http://localhost:5265
#    Swagger: http://localhost:5265/swagger   Health: http://localhost:5265/health

# 4) UI (в отдельном терминале)
dotnet run --project BookingSystem.WebUI  --urls http://localhost:5006
#    http://localhost:5006
```

Адрес API для UI задаётся в `BookingSystem.WebUI/wwwroot/appsettings.json` → `ApiBaseUrl`.

## API

| Метод | Маршрут | Описание |
|---|---|---|
| GET | `/api/catalog/groups` | Группы услуг |
| GET | `/api/catalog/services?groupId=` | Услуги (groupId не задан → полный список) |
| GET | `/api/resources` | Сотрудники-ресурсы |
| GET | `/api/clients?search=&take=` | Поиск клиентов |
| GET | `/api/bookings?from=&to=&resourceId=&groupId=` | Записи диапазона |
| POST | `/api/bookings` | Создать (время «по» и суммы считает сервер) |
| PUT | `/api/bookings/{id}` | Обновить (вкл. drag/resize) |
| DELETE | `/api/bookings/{id}` | Мягкое удаление |

## Функционал UI

- Планировщик Syncfusion, 4 вида: **День**, **Неделя**, **Сотрудники** (колонки/строки по сотрудникам), **Месяцы** (таймлайн).
- Фильтры «Группа услуг» и «Сотрудник»; легенда цветных меток-статусов; tooltip при наведении.
- Кастомный редактор записи: ФИО, сотрудник, дата/время, метка, примечание, поиск клиента в БД,
  каталог → доступные → выбранные услуги, авто-расчёт времени «по» и суммы; drag/resize сохраняется.

## Syncfusion-лицензия

UI использует **бесплатную Community-лицензию** Syncfusion. Без ключа сверху показывается
информационный баннер «trial version». Чтобы убрать его — получите бесплатный ключ
(syncfusion.com → Claim free Community license) и впишите в
`BookingSystem.WebUI/wwwroot/appsettings.json` → `SyncfusionLicenseKey`.
