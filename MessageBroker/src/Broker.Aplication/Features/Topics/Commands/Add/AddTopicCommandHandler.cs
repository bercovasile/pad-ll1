using Broker.Application.Abstractions;
using Broker.Context.Response;
using Broker.Domain.Enums;
using MediatR;
using System.Runtime.CompilerServices;

namespace Broker.Application.Features.Topics.Commands.Add;

internal class AddTopicCommandHandler(IBaseTopicProvider baseTopicProvide, ITopicProviderFactory topicProviderFactory) : IRequestHandler<AddTopicCommand, Response>
{
	public async Task<Response> Handle(AddTopicCommand request, CancellationToken cancellationToken)
	{
		try
		{

			var topic = await baseTopicProvide.TopicExistsAsync(request.Topic, cancellationToken);	
			if (topic)
				return Response.Fail($"Topic '{request.Topic}' already exists.");

			var topicProvider = topicProviderFactory.Create((TopicBehavior)request.TopicRequest.Behavior);

			if (topicProvider == null)
				return Response.Fail($"Topic behavior '{request.TopicRequest.Behavior}' is not supported.");

			var id =  await topicProvider.CreateTopicAsync(request.TopicRequest, cancellationToken);

			if (id == Guid.Empty)
				return Response.Fail("Failed to create topic.");



			return Response.Ok($"Topic '{request.Topic}' created successfully.");

		}
		catch (Exception ex)
		{
			return Response.Fail(ex.Message);

		}
	}
}
