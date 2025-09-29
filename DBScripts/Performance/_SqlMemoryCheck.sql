/*
http://udayarumilli.com/script-to-monitor-sql-server-memory-usage/

Script: Captures Buffer Pool Usage
Analysis:
BPool_Commit_Tgt_MB > BPool_Committed_MB: SQL Server Memory Manager tries to obtain additional memory
BPool_Commit_Tgt_MB < BPool_Committed_MB: SQL Server Memory Manager tries to shrink the amount of memory committed
If the value of BPool_Visible_MB is too low: We might receive out of memory errors or memory dump will be created.
*/

-- SQL server 2012 / 2014 / 2016
SELECT
      (committed_kb)/1024.0 as BPool_Committed_MB,
      (committed_target_kb)/1024.0 as BPool_Commit_Tgt_MB,
      (visible_target_kb)/1024.0 as BPool_Visible_MB
FROM  sys.dm_os_sys_info;

/*
Script: Captures System Memory Usage
Analysis:
available_physical_memory_mb: Should be some positive sign based on total physical memory
available_page_file_mb: Should be some positive sign based on your total page file
Percentage_Used: 100% for a long time indicates a memory pressure
system_memory_state_desc: should be Available physical memory is high / steady
*/


select
      total_physical_memory_kb/1024 AS total_physical_memory_mb,
      available_physical_memory_kb/1024 AS available_physical_memory_mb,
      total_page_file_kb/1024 AS total_page_file_mb,
      available_page_file_kb/1024 AS available_page_file_mb,
      100 - (100 * CAST(available_physical_memory_kb AS DECIMAL(18,3))/CAST(total_physical_memory_kb AS DECIMAL(18,3))) 
      AS 'Percentage_Used',
      system_memory_state_desc
from  sys.dm_os_sys_memory;

/*
Script: SQL Server Process Memory Usage
Analysis:
physical_memory_in_use: We can’t figure out the exact amount of physical memory using by sqlservr.exe using task manager but this column showcase the actual amount of physical memory using by SQL Server.
locked_page_allocations: If this is > 0 means Locked Pages is enabled for SQL Server which is one of the best practice
available_commit_limit: This indciates the available amount of memory that can be committed by the process sqlservr.exe
page_fault_count: Pages fetching from the page file on the hard disk instead of from physical memory. Consistently high number of hard faults per second represents Memory pressure.
*/
select
      physical_memory_in_use_kb/1024.0 AS 'physical_memory_in_use (MB)',
      locked_page_allocations_kb/1024.0 AS 'locked_page_allocations (MB)',
      virtual_address_space_committed_kb/1024.0 AS 'virtual_address_space_committed (MB)',
      available_commit_limit_kb/1024.0 AS 'available_commit_limit (MB)',
      page_fault_count as 'page_fault_count',
	  process_physical_memory_low,
	  process_virtual_memory_low
from  sys.dm_os_process_memory;



/*
Script: Top 25 Costliest Stored Procedures by Logical Reads
Analysis:
This helps you find the most expensive cached stored procedures from a memory perspective
You should look at this if you see signs of memory pressure
More number of logical reads means you need to check execution plan to find the bottleneck


SELECT  TOP(25)
        p.name AS [SP Name],
        qs.total_logical_reads AS [TotalLogicalReads],
        qs.total_logical_reads/qs.execution_count AS [AvgLogicalReads],
        qs.execution_count AS 'execution_count',
        qs.total_elapsed_time AS 'total_elapsed_time',
        qs.total_elapsed_time/qs.execution_count AS 'avg_elapsed_time',
        qs.cached_time AS 'cached_time'
FROM    sys.procedures AS p
        INNER JOIN sys.dm_exec_procedure_stats AS qs 
                   ON p.[object_id] = qs.[object_id]
WHERE
        qs.database_id = DB_ID()
ORDER BY
        qs.total_logical_reads DESC;
*/

/*
http://searchsqlserver.techtarget.com/tip/SQL-Server-out-of-memory-Troubleshoot-and-avoid-SQL-memory-problems
*/

USE MaxPainAPI
GO

SELECT 
	sys.tables.name TableName,
	sum(a.page_id)*8 AS MemorySpaceKB,
	SUM(sys.allocation_units.data_pages)*8 AS StorageSpaceKB,
	CASE 
		WHEN SUM(sys.allocation_units.data_pages) <> 0 
		THEN SUM(a.page_id)/CAST(SUM(sys.allocation_units.data_pages) AS NUMERIC(18,2))
	END AS 'Percentage Of Object In Memory'
FROM (
	SELECT 
		database_id, 
		allocation_unit_id, 
		COUNT(page_id) page_id 
	FROM sys.dm_os_buffer_descriptors 
	GROUP BY database_id, allocation_unit_id
) a
JOIN sys.allocation_units 
	ON a.allocation_unit_id = sys.allocation_units.allocation_unit_id
JOIN sys.partitions 
	ON (
		sys.allocation_units.type IN (1,3)
		AND sys.allocation_units.container_id = sys.partitions.hobt_id
	)
	OR (sys.allocation_units.type = 2 AND sys.allocation_units.container_id = sys.partitions.partition_id)
JOIN sys.tables 
	ON sys.partitions.object_id = sys.tables.object_id
	AND sys.tables.is_ms_shipped = 0
WHERE a.database_id = DB_ID()
GROUP BY sys.tables.name