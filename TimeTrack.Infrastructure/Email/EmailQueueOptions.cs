namespace TimeTrack.Infrastructure.Email
{
    public class EmailQueueOptions
    {
        public string? ConnectionString { get; set; }
        public string? QueueName { get; set; }
    }
}
