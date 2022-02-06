using AutoMapper;
using MarsOffice.Tvg.Jobs.Abstractions;
using MarsOffice.Tvg.Jobs.Entities;

namespace MarsOffice.Tvg.Jobs.Mappers
{
    public class JobMapper : Profile
    {
        public JobMapper() {
            CreateMap<Job, JobEntity>().PreserveReferences();
            CreateMap<JobEntity, Job>().PreserveReferences();
        }
    }
}