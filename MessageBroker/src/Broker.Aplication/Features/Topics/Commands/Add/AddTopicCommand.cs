using Broker.Context.Response;
using Broker.Context.Topics.Requests;
using MediatR;

namespace Broker.Application.Features.Topics.Commands.Add;

public record AddTopicCommand ( string Topic , TopicRequest TopicRequest) : IRequest<Response>;
