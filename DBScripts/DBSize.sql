SELECT
    DB_NAME(mf.database_id) AS DatabaseName,
    SUM(mf.size * 8 / 1024) AS DatabaseSizeMB,
    (SUM(mf.size * 8 / 1024))/1000.0 AS DatabaseSizeGB
FROM
    sys.master_files mf
INNER JOIN
    sys.databases d ON mf.database_id = d.database_id
WHERE
    mf.type = 0 -- Data files
GROUP BY
    mf.database_id
ORDER BY
    DatabaseName;