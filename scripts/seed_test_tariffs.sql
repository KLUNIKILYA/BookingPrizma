SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- Списки столбцов для клонирования (без вычисляемых и timestamp; без IdTicket).
DECLARE @tcols nvarchar(max);
SELECT @tcols = STUFF((
    SELECT ',' + QUOTENAME(c.name)
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID('dbo.Ticket')
      AND c.is_computed = 0 AND c.name <> 'IdTicket'
      AND TYPE_NAME(c.system_type_id) <> 'timestamp'
    ORDER BY c.column_id
    FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '');

DECLARE @zcols nvarchar(max);
SELECT @zcols = STUFF((
    SELECT ',' + QUOTENAME(c.name)
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID('dbo.TicketZone')
      AND c.is_computed = 0 AND c.name <> 'IdTicket'
      AND TYPE_NAME(c.system_type_id) <> 'timestamp'
    ORDER BY c.column_id
    FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '');

DECLARE @targets TABLE (mins int, price numeric(18,2), lbl nvarchar(40));
INSERT INTO @targets VALUES
    (60, 30, N'1 час'),
    (120, 40, N'2 часа'),
    (180, 60, N'3 часа'),
    (720, 200, N'целый день');

DECLARE @zone int = 3;
WHILE @zone <= 8
BEGIN
    DECLARE @roomName nvarchar(100) = (SELECT NameZone FROM dbo.Zones WHERE IdZone = @zone);
    -- шаблон: любой существующий тариф-бронь этой комнаты (берём его строку Ticket+TicketZone для клонирования)
    DECLARE @tpl int = (SELECT MIN(tz.IdTicket)
                        FROM dbo.TicketZone tz JOIN dbo.Ticket t ON t.IdTicket = tz.IdTicket
                        WHERE tz.IdZone = @zone AND tz.Reservation = 1 AND t.Active = 1);

    IF @tpl IS NOT NULL
    BEGIN
        DECLARE @mins int, @price numeric(18,2), @lbl nvarchar(40);
        DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT mins, price, lbl FROM @targets;
        OPEN cur; FETCH NEXT FROM cur INTO @mins, @price, @lbl;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @name nvarchar(256) = N'ТЕСТ ' + @roomName + N' — ' + @lbl;
            IF NOT EXISTS (SELECT 1 FROM dbo.Ticket WHERE NameTicket = @name)
            BEGIN
                DECLARE @sql nvarchar(max), @newId int = NULL;
                SET @sql = N'INSERT INTO dbo.Ticket (' + @tcols + N') SELECT ' + @tcols
                         + N' FROM dbo.Ticket WHERE IdTicket=@tpl; SET @newId = CAST(SCOPE_IDENTITY() AS int);';
                EXEC sp_executesql @sql, N'@tpl int, @newId int OUTPUT', @tpl = @tpl, @newId = @newId OUTPUT;

                UPDATE dbo.Ticket SET NameTicket = @name, TotalPrice = @price, OnePrice = 0, Active = 1
                WHERE IdTicket = @newId;

                SET @sql = N'INSERT INTO dbo.TicketZone (IdTicket,' + @zcols + N') SELECT @newId,' + @zcols
                         + N' FROM dbo.TicketZone WHERE IdTicket=@tpl AND IdZone=@zone;';
                EXEC sp_executesql @sql, N'@newId int, @tpl int, @zone int', @newId = @newId, @tpl = @tpl, @zone = @zone;

                UPDATE dbo.TicketZone SET Reservation = 1, ReservationTime = @mins, FreeTime = @mins
                WHERE IdTicket = @newId AND IdZone = @zone;
            END
            FETCH NEXT FROM cur INTO @mins, @price, @lbl;
        END
        CLOSE cur; DEALLOCATE cur;
    END
    SET @zone = @zone + 1;
END

PRINT '== созданные тестовые тарифы ==';
SELECT z.NameZone, t.IdTicket, t.NameTicket, tz.ReservationTime AS mins, t.TotalPrice
FROM dbo.Ticket t
JOIN dbo.TicketZone tz ON tz.IdTicket = t.IdTicket
JOIN dbo.Zones z ON z.IdZone = tz.IdZone
WHERE t.NameTicket LIKE N'ТЕСТ %'
ORDER BY tz.IdZone, tz.ReservationTime;
