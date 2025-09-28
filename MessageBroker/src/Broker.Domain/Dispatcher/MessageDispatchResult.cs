namespace Broker.Domain.Entites.Dispatcher
{
    public class MessageDispatchResult
    {
        public bool Delivered { get; set; }
        public string? Reason { get; set; }
    }
}
