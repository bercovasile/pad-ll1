using Broker.Application;
using Broker.Infrastructure.Jobs;
using Broker.Persistence;
using Broker.Presentation.Services.gRPC.Handlers;
using Broker.Presentation.Socket;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MongoDB.Driver;
using Quartz;
using Quartz.AspNetCore;


// Build the app
var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;


//builder.Services.AddMemoryCache();
//builder.Services.AddMongoBroker(configuration);
//builder.Services.AddPostgreSQLBroker(configuration);

//builder.Services.AddTopicProviders();
builder.Services.AddApplicationServices();

//builder.Services.UseWebSocketReceiverBroker();
//builder.Services.UseSocketReceiverBroker();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(37001, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddGrpc();

builder.Services.AddSingleton<GrpcReceiverMessageHandler>();

builder.Services.AddBrokerServices(configuration);
var app = builder.Build();

// Enable WebSockets
app.UseWebSockets();

app.MapGrpcService<GrpcReceiverServerService>();
app.MapGrpcService<GrpcConsumerServerService>();


// Map the endpoint
app.MapReceiverSocketBroker("/messages/publisher/{topic}");

//app.MapSubscriberSocketBroker("/messages/subscriber/{topic}");

app.MapSocketBrokerManagement("/topics/{topic}");

app.Run();