USE Fin
GO

/*

EXECUTE dbo.IndexOptimize
@Databases = 'USER_DATABASES',
@FragmentationLow = NULL,
@FragmentationMedium = 'INDEX_REORGANIZE,INDEX_REBUILD_ONLINE,INDEX_REBUILD_OFFLINE',
@FragmentationHigh = 'INDEX_REBUILD_ONLINE,INDEX_REBUILD_OFFLINE',
@FragmentationLevel1 = 5,
@FragmentationLevel2 = 30,
@UpdateStatistics = 'ALL',
@OnlyModifiedStatistics = 'Y'

*/

SELECT
    name,
    size,
    size * 8/1024.0 'Size (MB)',
    size * 8/1024.0/1024.0 'Size (GB)',
    max_size
FROM sys.master_files
WHERE DB_NAME(database_id) = 'Fin';

SELECT 
    t.NAME AS TableName,
    s.Name AS SchemaName,
    p.rows,
    SUM(a.total_pages) * 8 AS TotalSpaceKB, 
    CAST(ROUND(((SUM(a.total_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS TotalSpaceMB,
    CAST(ROUND((((SUM(a.total_pages) * 8) / 1024.00) / 1024.00), 2) AS NUMERIC(36, 2)) AS TotalSpaceGB,
    SUM(a.used_pages) * 8 AS UsedSpaceKB, 
    CAST(ROUND(((SUM(a.used_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS UsedSpaceMB, 
    (SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB,
    CAST(ROUND(((SUM(a.total_pages) - SUM(a.used_pages)) * 8) / 1024.00, 2) AS NUMERIC(36, 2)) AS UnusedSpaceMB
FROM 
    sys.tables t
INNER JOIN      
    sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN 
    sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN 
    sys.allocation_units a ON p.partition_id = a.container_id
LEFT OUTER JOIN 
    sys.schemas s ON t.schema_id = s.schema_id
WHERE 
    t.NAME NOT LIKE 'dt%' 
    AND t.is_ms_shipped = 0
    AND i.OBJECT_ID > 255 
GROUP BY 
    t.Name, s.Name, p.Rows
ORDER BY 
    TotalSpaceMB DESC, t.Name

SELECT
    OBJECT_NAME(i.OBJECT_ID) AS TableName,
    i.name AS IndexName,
    i.index_id AS IndexID,
    8 * SUM(a.used_pages) AS 'Indexsize(KB)',
    8 * SUM(a.used_pages)/1000.0 AS 'Indexsize(MB)'
FROM
    sys.indexes AS i
    JOIN sys.partitions AS p ON p.OBJECT_ID = i.OBJECT_ID AND p.index_id = i.index_id
    JOIN sys.allocation_units AS a ON a.container_id = p.partition_id
WHERE
    i.is_primary_key = 0 -- fix for size discrepancy
GROUP BY
    i.OBJECT_ID,
    i.index_id,
    i.name
ORDER BY
    OBJECT_NAME(i.OBJECT_ID),
    i.index_id



select 
    s.Name + N'.' + t.name as [Table]
    ,i.name as [Index] 
    ,i.is_unique as [IsUnique]
    ,ius.user_seeks as [Seeks], ius.user_scans as [Scans]
    ,ius.user_lookups as [Lookups]
    ,ius.user_seeks + ius.user_scans + ius.user_lookups as [Reads]
    ,ius.user_updates as [Updates], ius.last_user_seek as [Last Seek]
    ,ius.last_user_scan as [Last Scan], ius.last_user_lookup as [Last Lookup]
    ,ius.last_user_update as [Last Update]
from 
    sys.tables t with (nolock) join sys.indexes i with (nolock) on
        t.object_id = i.object_id
    join sys.schemas s with (nolock) on 
        t.schema_id = s.schema_id
    left outer join sys.dm_db_index_usage_stats ius on
        ius.database_id = db_id() and
        ius.object_id = i.object_id and 
        ius.index_id = i.index_id
order by
    s.name, t.name, i.index_id
option (recompile)