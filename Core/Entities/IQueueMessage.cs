namespace Core.Entities
{
    public interface IQueueMessage<T>
        where T : class
    {
        string TraceId { get; }
        T Entity { get; }
    }
}