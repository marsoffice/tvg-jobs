using System.Collections.Generic;

namespace MarsOffice.Tvg.Jobs.Abstractions
{
    public class JobsList
    {
        public string NextRowKey { get; set; }
        public IEnumerable<Job> Items { get; set; }
    }
}