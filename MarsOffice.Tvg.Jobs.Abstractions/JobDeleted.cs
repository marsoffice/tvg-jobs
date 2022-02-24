namespace MarsOffice.Tvg.Jobs.Abstractions
{
    public class JobDeleted
    {
        public string JobId { get; set; }
        public string UserEmail { get; set; }
        public string UserId { get; set; }
    }
}