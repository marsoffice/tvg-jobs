using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;

namespace MarsOffice.Tvg.Jobs.Entities
{
    public class JobEntity : TableEntity
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        
    }
}