SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- 1) Типы зон (настраиваемый список), idempotent по имени.
IF NOT EXISTS(SELECT 1 FROM dbo.Booking_ZoneType WHERE Name=N'1 этаж') INSERT dbo.Booking_ZoneType(Name,SortOrder,Active) VALUES(N'1 этаж',1,1);
IF NOT EXISTS(SELECT 1 FROM dbo.Booking_ZoneType WHERE Name=N'2 этаж') INSERT dbo.Booking_ZoneType(Name,SortOrder,Active) VALUES(N'2 этаж',2,1);
IF NOT EXISTS(SELECT 1 FROM dbo.Booking_ZoneType WHERE Name=N'Комнаты') INSERT dbo.Booking_ZoneType(Name,SortOrder,Active) VALUES(N'Комнаты',3,1);

DECLARE @tFloor1 int=(SELECT Id FROM dbo.Booking_ZoneType WHERE Name=N'1 этаж');
DECLARE @tFloor2 int=(SELECT Id FROM dbo.Booking_ZoneType WHERE Name=N'2 этаж');
DECLARE @tRooms  int=(SELECT Id FROM dbo.Booking_ZoneType WHERE Name=N'Комнаты');

-- 2) Столики: клонируем строку зоны-образца (IdZone=3), меняем имя, назначаем тип.
DECLARE @isIdentity int = COLUMNPROPERTY(OBJECT_ID('dbo.Zones'),'IdZone','IsIdentity');
DECLARE @zcols nvarchar(max);
SELECT @zcols = STUFF((
    SELECT ',' + QUOTENAME(c.name) FROM sys.columns c
    WHERE c.object_id=OBJECT_ID('dbo.Zones') AND c.is_computed=0 AND c.name<>'IdZone'
      AND TYPE_NAME(c.system_type_id)<>'timestamp'
    ORDER BY c.column_id FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,1,'');

DECLARE @tpl int = 3;

DECLARE @targets TABLE (nm nvarchar(100), typeId int, ord int);
INSERT @targets VALUES
 (N'Стол 1-1',@tFloor1,1),(N'Стол 1-2',@tFloor1,2),(N'Стол 1-3',@tFloor1,3),
 (N'Стол 1-4',@tFloor1,4),(N'Стол 1-5',@tFloor1,5),(N'Стол 1-6',@tFloor1,6),
 (N'Стол 2-1',@tFloor2,1),(N'Стол 2-2',@tFloor2,2),(N'Стол 2-3',@tFloor2,3),
 (N'Стол 2-4',@tFloor2,4),(N'Стол 2-5',@tFloor2,5),(N'Стол 2-6',@tFloor2,6);

DECLARE @nm nvarchar(100), @typeId int, @ord int, @newZone int, @sql nvarchar(max);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT nm,typeId,ord FROM @targets;
OPEN cur; FETCH NEXT FROM cur INTO @nm,@typeId,@ord;
WHILE @@FETCH_STATUS=0
BEGIN
    IF NOT EXISTS(SELECT 1 FROM dbo.Zones WHERE NameZone=@nm)
    BEGIN
        SET @newZone = NULL;
        IF @isIdentity = 1
        BEGIN
            SET @sql = N'INSERT INTO dbo.Zones ('+@zcols+N') SELECT '+@zcols+N' FROM dbo.Zones WHERE IdZone=@tpl; SET @newZone=CAST(SCOPE_IDENTITY() AS int);';
            EXEC sp_executesql @sql, N'@tpl int, @newZone int OUTPUT', @tpl=@tpl, @newZone=@newZone OUTPUT;
        END
        ELSE
        BEGIN
            SET @newZone = (SELECT MAX(IdZone)+1 FROM dbo.Zones);
            SET @sql = N'INSERT INTO dbo.Zones (IdZone,'+@zcols+N') SELECT @newZone,'+@zcols+N' FROM dbo.Zones WHERE IdZone=@tpl;';
            EXEC sp_executesql @sql, N'@newZone int, @tpl int', @newZone=@newZone, @tpl=@tpl;
        END

        UPDATE dbo.Zones SET NameZone=@nm, ShortNameZone=@nm, Active=1 WHERE IdZone=@newZone;

        IF NOT EXISTS(SELECT 1 FROM dbo.Booking_ZoneAssignment WHERE ZoneId=@newZone)
            INSERT dbo.Booking_ZoneAssignment(ZoneId,ZoneTypeId,SortOrder) VALUES(@newZone,@typeId,@ord);
    END
    FETCH NEXT FROM cur INTO @nm,@typeId,@ord;
END
CLOSE cur; DEALLOCATE cur;

-- 3) Комнаты (IdZone 3..8) -> тип «Комнаты».
INSERT dbo.Booking_ZoneAssignment(ZoneId,ZoneTypeId,SortOrder)
SELECT z.IdZone, @tRooms, z.IdZone
FROM dbo.Zones z
WHERE z.IdZone BETWEEN 3 AND 8 AND z.Active=1
  AND NOT EXISTS(SELECT 1 FROM dbo.Booking_ZoneAssignment a WHERE a.ZoneId=z.IdZone);

PRINT '== назначения зона -> тип ==';
SELECT t.Name AS type, z.IdZone, z.NameZone
FROM dbo.Booking_ZoneAssignment a
JOIN dbo.Booking_ZoneType t ON t.Id=a.ZoneTypeId
JOIN dbo.Zones z ON z.IdZone=a.ZoneId
ORDER BY t.SortOrder, a.SortOrder;
