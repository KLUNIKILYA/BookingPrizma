-- Пересид ресурсов: комнаты и столики вместо мастеров. Чистит демо-брони и ресурсы,
-- затем заводит комнаты/столики. Booking_ServiceSetting (услуги) не трогаем.
SET NOCOUNT ON;

DELETE FROM dbo.Booking_BookingService;
DELETE FROM dbo.Booking_Booking;
DELETE FROM dbo.Booking_Resource;
DBCC CHECKIDENT ('dbo.Booking_Resource', RESEED, 0);
DBCC CHECKIDENT ('dbo.Booking_Booking', RESEED, 0);
DBCC CHECKIDENT ('dbo.Booking_BookingService', RESEED, 0);

-- Kind: 0 = Комната (Room), 1 = Столик (Table)
INSERT INTO dbo.Booking_Resource (Kind, VisitorId, DisplayName, Color, SortOrder, Active) VALUES
 (0, NULL, N'Комната 1',   '#4F86C6', 1, 1),
 (0, NULL, N'Комната 2',   '#6A59F0', 2, 1),
 (0, NULL, N'VIP-комната', '#C77DBB', 3, 1),
 (1, NULL, N'Столик 1',    '#6FB07F', 4, 1),
 (1, NULL, N'Столик 2',    '#E0A458', 5, 1),
 (1, NULL, N'Столик 3',    '#5FB0C8', 6, 1),
 (1, NULL, N'Столик 4',    '#E08D8D', 7, 1);

SELECT Id, Kind, DisplayName, Color, SortOrder FROM dbo.Booking_Resource ORDER BY SortOrder;
