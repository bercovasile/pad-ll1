using Broker.Context.Messages;
using Broker.Context.Response;
using MediatR;

namespace Broker.Application.Features.Messages.Commands.Add;

public record AddMessageCommand( string TopicName , MessageRequest MessageRequest ) : IRequest<Response>;