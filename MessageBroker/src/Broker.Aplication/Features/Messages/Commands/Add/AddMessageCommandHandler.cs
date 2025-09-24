

using Broker.Application.Abstractions;
using Broker.Context.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Broker.Application.Features.Messages.Commands.Add;

internal class AddMessageCommandHandler(
		IBaseTopicProvider baseTopicProvide,
		ITopicProviderFactory topicProviderFactory,
		ILogger<AddMessageCommandHandler> logger
	) : IRequestHandler<AddMessageCommand, Response>
{
	public async Task<Response> Handle(AddMessageCommand request, CancellationToken cancellationToken)
	{
		try
		{

			var topic = await baseTopicProvide.GetTopicAsync(request.TopicName, cancellationToken);
			if (topic == null)
				return Response.Fail($"Topic '{request.TopicName}' don't exists.");

			var topicProvider = topicProviderFactory.Create(topic.Behavior);

			if (topicProvider == null)
				return Response.Fail($"Topic behavior '{topic.Behavior}' is not supported.");

			var id = await topicProvider.AddMessageAsync( request.TopicName , request.MessageRequest, cancellationToken);

			if (id == Guid.Empty)
				return Response.Fail("Failed to create topic.");



			return Response.Ok("");

		}
		catch (Exception ex)
		{
			return Response.Fail(ex.Message);

		}
	}
}
