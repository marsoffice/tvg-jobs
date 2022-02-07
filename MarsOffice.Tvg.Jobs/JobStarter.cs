using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarsOffice.Tvg.Jobs.Entities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace MarsOffice.Tvg.Jobs
{
    public class JobStarter
    {
        [FunctionName("JobStarter")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
        [Table("Jobs", Connection = "localsaconnectionstring")] CloudTable jobsTable,
        [Queue("generate-video", Connection = "localsaconnectionstring")] IAsyncCollector<dynamic> generateVideoQueue,
        ILogger log)
        {
            var now = DateTimeOffset.UtcNow;
            var normalizedNow = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0, TimeSpan.Zero);

            var allJobsQuery = new TableQuery<JobEntity>()
                .Where(
                    TableQuery.GenerateFilterCondition("Disabled", QueryComparisons.NotEqual, "true")
                );

            TableContinuationToken tct = null;
            var hasData = true;
            var allJobs = new List<JobEntity>();
            while (hasData)
            {
                var batchResponse = await jobsTable.ExecuteQuerySegmentedAsync(allJobsQuery, tct);
                allJobs.AddRange(batchResponse);
                tct = batchResponse.ContinuationToken;
                if (tct == null)
                {
                    hasData = false;
                }
            }

            var added = false;
            foreach (var job in allJobs)
            {
                try
                {
                    var cronSchedule = CrontabSchedule.Parse(job.Cron);
                    var nextOccurence = cronSchedule.GetNextOccurrence(normalizedNow.UtcDateTime);
                    if (nextOccurence == normalizedNow.UtcDateTime)
                    {
                        // await generateVideoQueue.AddAsync("test");
                        added = true;
                    }
                }
                catch (Exception e)
                {
                    log.LogError(e, "Exception when starting job");
                }
            }
            if (added)
            {
                await generateVideoQueue.FlushAsync();
            }
        }
    }
}
