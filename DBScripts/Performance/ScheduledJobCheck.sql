USE fin
GO

/*
TRUNCATE TABLE OptionQuote;
TRUNCATE TABLE OptionSymbol;
TRUNCATE TABLE StockQuote;

TRUNCATE TABLE MessageLog
*/

--SELECT COUNT(*) FROM OptionQuote


select 
 j.name as 'JobName',
 msdb.dbo.agent_datetime(run_date, run_time) as 'RunDateTime',
 ((run_duration/10000*3600 + (run_duration/100)%100*60 + run_duration%100 + 31 ) / 60.0) 
         as 'RunDurationMinutes',
 h.step_name,
 h.[message]
From msdb.dbo.sysjobs j 
INNER JOIN msdb.dbo.sysjobhistory h 
 ON j.job_id = h.job_id 
where j.enabled = 1   --Only Enabled Jobs
--and j.name = 'IndexOptimize - USER_DATABASES' --Uncomment to search for a single job
/*
and msdb.dbo.agent_datetime(run_date, run_time) 
BETWEEN '12/08/2012' and '12/10/2012'  --Uncomment for date range queries
*/
order by JobName, RunDateTime desc


/*
select
(physical_memory_in_use_kb/1024)Memory_usedby_Sqlserver_MB,
(locked_page_allocations_kb/1024 )Locked_pages_used_Sqlserver_MB,
(total_virtual_address_space_kb/1024 )Total_VAS_in_MB,
process_physical_memory_low,
process_virtual_memory_low
from sys.dm_os_process_memory

select * from sys.dm_exec_query_memory_grants where is_next_candidate is not null
*/


