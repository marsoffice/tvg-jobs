using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MarsOffice.Tvg.Jobs.Abstractions;
using MarsOffice.Tvg.Jobs.Entities;
using MarsOffice.Tvg.Videos.Abstractions;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace MarsOffice.Tvg.Jobs
{
    public class JobStarter
    {
        private readonly IMapper _mapper;

        public JobStarter(IMapper mapper)
        {
            _mapper = mapper;
        }

        [FunctionName("JobStarter")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
        [Table("Jobs", Connection = "localsaconnectionstring")] CloudTable jobsTable,
        [Queue("generate-video", Connection = "localsaconnectionstring")] IAsyncCollector<GenerateVideo> generateVideoQueue,
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
                    var nextOccurence = cronSchedule.GetNextOccurrence(normalizedNow.UtcDateTime.AddSeconds(-1));
                    if (nextOccurence == normalizedNow.UtcDateTime)
                    {
                        await generateVideoQueue.AddAsync(
                            new GenerateVideo {
                                RequestDate = normalizedNow.UtcDateTime,
                                Job = _mapper.Map<Job>(job)
                            }
                        );
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
