USE MaxPainAPI
GO

/*
 * Smart Index Defrag
 * by Neil Laslett
 *
 * Version 1: Initial release
 *
 * Version 2: Schema-aware
 * Specify a single schema when calling to restrict to that schema (e.g. 'dbo').
 * Execute without specifying a schema to check user tables in all schemas.
 *
 * Version 3: SQL Server version aware
 * Heap tables cannot be rebuilt before SQL Server 2008.
 *
 * Version 4: Added ability to skip sparse tables which will never adequately defragment.
 *
 * See: http://msdn.microsoft.com/en-us/library/ms189858.aspx
 */
 BEGIN
    DECLARE @schema     sysname = '',
            @skipSparse bit = 0

    SET NOCOUNT ON

    DECLARE @table      sysname,
            @index_id   int,
            @object_id  int,
            @name       sysname,
            @avg        decimal(5,2),
            @msg        varchar(max),
            @ver_str    sysname,
            @version    tinyint,
            @dot        int

    SELECT  @ver_str = CONVERT(varchar(50), SERVERPROPERTY('ProductVersion')),
            @dot = CHARINDEX('.', @ver_str),
            @version = LEFT(@ver_str, @dot - 1)

    DECLARE @IndexTable TABLE
    (
        [Schema]    sysname NOT NULL,
        [Table]     sysname NOT NULL,
        index_id    int     NOT NULL,
        name        sysname NULL,
        avg_fragmentation_in_percent decimal(5,2) NOT NULL,
        result      varchar(50) NULL
    )

    DECLARE cTables CURSOR FOR
        SELECT  s.name AS s_name, o.name AS t_name, o.object_id
        FROM    sys.objects o INNER JOIN
                sys.schemas s ON o.schema_id = s.schema_id
        WHERE   o.[type] = 'U' AND
                (s.name = @schema OR @schema = '')

    OPEN cTables
    FETCH NEXT FROM cTables INTO @schema, @table, @object_id
    WHILE @@FETCH_STATUS = 0
    BEGIN
        INSERT INTO @IndexTable
        SELECT  @schema, @table, a.index_id, b.name, a.avg_fragmentation_in_percent, NULL
        FROM    sys.dm_db_index_physical_stats (DB_ID(), @object_id, NULL, NULL, NULL) AS a
        INNER JOIN sys.indexes AS b ON a.object_id = b.object_id AND a.index_id = b.index_id
        WHERE   avg_fragmentation_in_percent > 5
                AND (a.index_id > 0 OR @version > 9)
                AND (a.avg_fragment_size_in_pages > 8 OR @skipSparse = 0)
        
        FETCH NEXT FROM cTables INTO @schema, @table, @object_id
    END
    CLOSE cTables
    DEALLOCATE cTables

    DECLARE cIndexes CURSOR FOR
        SELECT [Schema], [Table], index_id, name, avg_fragmentation_in_percent FROM @IndexTable

    OPEN cIndexes
    FETCH NEXT FROM cIndexes INTO @schema, @table, @index_id, @name, @avg
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @index_id = 0
        BEGIN
            SET @msg = 'Rebuilding heap table ' + @schema + '.' + @table + '...'
            RAISERROR(@msg, 0, 1) WITH NOWAIT
            EXEC('ALTER TABLE [' + @schema + '].[' + @table + '] REBUILD')
            UPDATE @IndexTable
            SET result = 'REBUILD HEAP'
            WHERE [Schema] = @schema AND [Table] = @table AND index_id = @index_id
        END
        ELSE IF @avg > 30
        BEGIN
            SET @msg = 'Rebuilding index ' + @schema + '.' + @table + '.' + @name + '...'
            RAISERROR(@msg, 0, 1) WITH NOWAIT
            EXEC('ALTER INDEX [' + @name + '] ON [' + @schema + '].[' + @table + '] REBUILD')
            UPDATE @IndexTable
            SET result = 'REBUILD INDEX'
            WHERE [Schema] = @schema AND [Table] = @table AND index_id = @index_id
        END
        ELSE
        BEGIN
            SET @msg = 'Reorganizing index ' + @schema + '.' + @table + '.' + @name + '...'
            RAISERROR(@msg, 0, 1) WITH NOWAIT
            EXEC('ALTER INDEX [' + @name + '] ON [' + @schema + '].[' + @table + '] REORGANIZE')
            UPDATE @IndexTable
            SET result = 'REORGANIZE INDEX'
            WHERE [Schema] = @schema AND [Table] = @table AND index_id = @index_id
        END
        
        FETCH NEXT FROM cIndexes INTO @schema, @table, @index_id, @name, @avg
    END
    CLOSE cIndexes
    DEALLOCATE cIndexes

    PRINT 'Done!'
    SELECT * FROM @IndexTable
END