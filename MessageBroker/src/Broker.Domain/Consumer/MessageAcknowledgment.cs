using System;

namespace Broker.Domain.Entites.Consumer
{
    public enum AckType { Ack, Nak }
    public class MessageAcknowledgment
    {
        public Guid MessageId { get; set; }
        public AckType Type { get; set; }
        public string? Reason { get; set; }
    }
}
