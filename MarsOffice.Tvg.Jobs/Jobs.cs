using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MarsOffice.Microfunction;
using MarsOffice.Tvg.Jobs.Abstractions;
using MarsOffice.Tvg.Jobs.Entities;
using MarsOffice.Tvg.Videos.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MarsOffice.Tvg.Jobs
{
    public class Jobs
    {
        private readonly IMapper _mapper;
        private readonly IValidator<Job> _jobValidator;

        public Jobs(IMapper mapper, IValidator<Job> jobValidator)
        {
            _mapper = mapper;
            _jobValidator = jobValidator;
        }

        [FunctionName("GetJobs")]
        public async Task<IActionResult> GetJobs(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/jobs/getJobs")] HttpRequest req,
            [Table("Jobs", Connection = "localsaconnectionstring")] CloudTable jobsTable,
            ILogger log
            )
        {
            try
            {
                var principal = MarsOfficePrincipal.Parse(req);
                var userId = principal.FindFirst("id").Value;

                var query = new TableQuery<JobEntity>()
                    .Where(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId)
                    );

                var result = new List<Job>();

                TableContinuationToken tct = null;
                var hasData = true;
                while (hasData)
                {
                    var queryResult = await jobsTable.ExecuteQuerySegmentedAsync(query, tct);
                    result.AddRange(
                        _mapper.Map<IEnumerable<Job>>(queryResult)
                    );
                    tct = queryResult.ContinuationToken;
                    if (tct == null)
                    {
                        hasData = false;
                    }
                }
                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }

        [FunctionName("GetJob")]
        public async Task<IActionResult> GetJob(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/jobs/getJob/{id}")] HttpRequest req,
                    [Table("Jobs", Connection = "localsaconnectionstring")] CloudTable jobsTable,
                    ILogger log
                    )
        {
            try
            {
                var principal = MarsOfficePrincipal.Parse(req);
                var userId = principal.FindFirst("id").Value;
                var id = req.RouteValues["id"].ToString();

                var query = new TableQuery<JobEntity>()
                    .Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)
                        )
                    )
                    .Take(1);

                var queryResult = await jobsTable.ExecuteQuerySegmentedAsync(query, null);
                if (!queryResult.Results.Any())
                {
                    return new OkObjectResult(null);
                }
                return new OkObjectResult(
                    _mapper.Map<Job>(queryResult.Results.First())
                );
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }

        [FunctionName("GetJobInternal")]
        public async Task<IActionResult> GetJobInternal(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/jobs/getJobInternal/{userId}/{id}")] HttpRequest req,
            [Table("Jobs", Connection = "localsaconnectionstring")] CloudTable jobsTable,
            ILogger log,
            ClaimsPrincipal principal
            )
        {
            try
            {
                var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Development";
                if (env != "Development" && principal.FindFirstValue("roles") != "Application")
                {
                    return new StatusCodeResult(401);
                }
                var userId = req.RouteValues["userId"].ToString();
                var id = req.RouteValues["id"].ToString();

                var query = new TableQuery<JobEntity>()
                    .Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)
                        )
                    )
                    .Take(1);

                var queryResult = await jobsTable.ExecuteQuerySegmentedAsync(query, null);
                if (!queryResult.Results.Any())
                {
                    return new OkObjectResult(null);
                }
                return new OkObjectResult(
                    _mapper.Map<Job>(queryResult.Results.First())
                );
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }


        [FunctionName("CreateJob")]
        public async Task<IActionResult> CreateJob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/jobs/createJob")] HttpRequest req,
            [Table("Jobs", Connection = "localsaconnectionstring")] CloudTable jobsTable,
            ILogger log
            )
        {
            try
            {
                var principal = MarsOfficePrincipal.Parse(req);
                var userId = principal.FindFirst("id").Value;
                var userEmail = principal.FindFirst("email").Value;
                var json = string.Empty;
                using (var streamReader = new StreamReader(req.Body))
                {
                    json = await streamReader.ReadToEndAsync();
                }
                var payload = JsonConvert.DeserializeObject<Job>(json, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                payload.UserId = userId;
                payload.Id = Guid.NewGuid().ToString();
                payload.UserEmail = userEmail;
                await _jobValidator.ValidateAndThrowAsync(payload);

                var entity = _mapper.Map<JobEntity>(payload);
                entity.PartitionKey = entity.UserId;
                entity.RowKey = entity.Id;
                entity.ETag = "*";

                var operation = TableOperation.InsertOrReplace(entity);
                await jobsTable.ExecuteAsync(operation);

                return new OkObjectResult(payload);
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }

        [FunctionName("UpdateJob")]
        public async Task<IActionResult> UpdateJob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/jobs/updateJob/{id}")] HttpRequest req,
            [Table("Jobs", Connection = "localsaconnectionstring")] CloudTable jobsTable,
            ILogger log
            )
        {
            try
            {
                var principal = MarsOfficePrincipal.Parse(req);
                var userId = principal.FindFirst("id").Value;
                var userEmail = principal.FindFirst("email").Value;
                var id = req.RouteValues["id"].ToString();

                var json = string.Empty;
                using (var streamReader = new StreamReader(req.Body))
                {
                    json = await streamReader.ReadToEndAsync();
                }
                var payload = JsonConvert.DeserializeObject<Job>(json, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                payload.UserId = userId;
                payload.Id = id;
                payload.UserEmail = userEmail;
                await _jobValidator.ValidateAndThrowAsync(payload);

                var entity = _mapper.Map<JobEntity>(payload);
                entity.PartitionKey = entity.UserId;
                entity.RowKey = entity.Id;
                entity.ETag = "*";

                var operation = TableOperation.Replace(entity);
                await jobsTable.ExecuteAsync(operation);

                return new OkObjectResult(payload);
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }

        [FunctionName("DeleteJob")]
        public async Task<IActionResult> DeleteJob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/jobs/deleteJob/{id}")] HttpRequest req,
            [Table("Jobs", Connection = "localsaconnectionstring")] CloudTable jobsTable,
            ILogger log
            )
        {
            try
            {
                var principal = MarsOfficePrincipal.Parse(req);
                var userId = principal.FindFirst("id").Value;
                var id = req.RouteValues["id"].ToString();

                var operation = TableOperation.Delete(new JobEntity
                {
                    PartitionKey = userId,
                    RowKey = id,
                    ETag = "*"
                });

                try
                {
                    await jobsTable.ExecuteAsync(operation);
                }
                catch (Exception)
                {
                    // ignored
                }

                return new OkResult();
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }

        [FunctionName("StartJob")]
        public async Task<IActionResult> StartJob(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/jobs/startJob/{id}")] HttpRequest req,
                    [Table("Jobs", Connection = "localsaconnectionstring")] CloudTable jobsTable,
                     [Queue("generate-video", Connection = "localsaconnectionstring")] IAsyncCollector<GenerateVideo> generateVideoQueue,
                    ILogger log
                    )
        {
            try
            {
                var principal = MarsOfficePrincipal.Parse(req);
                var userId = principal.FindFirst("id").Value;
                var id = req.RouteValues["id"].ToString();

                var query = new TableQuery<JobEntity>()
                    .Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)
                        )
                    )
                    .Take(1);

                var queryResult = await jobsTable.ExecuteQuerySegmentedAsync(query, null);
                if (!queryResult.Results.Any())
                {
                    return new BadRequestObjectResult(new Dictionary<string, List<ErrorDto>> {
                        {"", new List<ErrorDto> {
                            new ErrorDto {
                                Message = "Job not found"
                            }
                        }}
                    });
                }

                var dto = _mapper.Map<Job>(queryResult.Results.First());
                await generateVideoQueue.AddAsync(new GenerateVideo
                {
                    Job = dto,
                    RequestDate = DateTimeOffset.UtcNow
                });
                await generateVideoQueue.FlushAsync();

                return new OkObjectResult(
                    new JobStarted()
                );
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }
    }
}