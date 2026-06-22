-- Идемпотентный сид демо-данных для системы бронирования (локальная PPS_3_1).
-- Заполняет ТОЛЬКО собственные таблицы Booking_*; легаси (SingleService/CashboxVisitor) не трогаем.

SET NOCOUNT ON;

-- 1) Настройки услуг (длительность/перерыв) для существующих активных услуг каталога.
MERGE dbo.Booking_ServiceSetting AS t
USING (
    SELECT s.ID AS ServiceId,
           20  AS DurationMinutes,
           10  AS BreakMinutes
    FROM dbo.SingleService s
    WHERE s.Active = 1
) AS src
ON (t.ServiceId = src.ServiceId)
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ServiceId, DurationMinutes, BreakMinutes, ColorOverride, IsBookable)
    VALUES (src.ServiceId, src.DurationMinutes, src.BreakMinutes, NULL, 1);

-- 2) Ресурсы (сотрудники-специалисты) из реальных сотрудников CashboxVisitor.
MERGE dbo.Booking_Resource AS t
USING (
    SELECT v.IdVisitor,
           RTRIM(v.Surname) + ' ' + RTRIM(v.Name) AS DisplayName,
           c.Color,
           c.SortOrder
    FROM dbo.CashboxVisitor v
    JOIN (VALUES
            (3129, '#4F86C6', 1),   -- Мироевская Елена
            (3131, '#6FB07F', 2),   -- Миронова Евгения
            (3130, '#C77DBB', 3),   -- Шеш Ольга
            (8907, '#E0A458', 4)    -- Трухан Дарья
         ) AS c(VisitorId, Color, SortOrder) ON c.VisitorId = v.IdVisitor
    WHERE v.Active = 1
) AS src
ON (t.VisitorId = src.IdVisitor)
WHEN NOT MATCHED BY TARGET THEN
    INSERT (VisitorId, DisplayName, Color, SortOrder, Active)
    VALUES (src.IdVisitor, src.DisplayName, src.Color, src.SortOrder, 1);

SELECT (SELECT COUNT(*) FROM dbo.Booking_ServiceSetting) AS ServiceSettings,
       (SELECT COUNT(*) FROM dbo.Booking_Resource)       AS Resources;
