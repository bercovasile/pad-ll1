using System.Threading.Tasks;
using Grpc.Core;
namespace Broker.Presentation.gRPC;

//public class MessageServiceImpl : MessageService.MessageServiceBase
//{
//	private readonly GrpcMessageReceiver _receiver;

//	public MessageServiceImpl(GrpcMessageReceiver receiver)
//	{
//		_receiver = receiver;
//	}

//	public override async Task<MessageReply> SendMessage(MessageRequest request, ServerCallContext context)
//	{
//		await _receiver.EnqueueAsync(request.Payload, context.CancellationToken);
//		return new MessageReply { Ack = true };
//	}
//}