using Broker.Presentation.Socket;
using Broker.Persistence;
using Broker.Application;
using MongoDB.Driver;

// Build the app
var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;


builder.Services.AddMemoryCache();
builder.Services.AddMongoBroker(configuration);
builder.Services.AddPostgreSQLBroker(configuration);

builder.Services.AddTopicProviders();
builder.Services.AddApplicationServices();

builder.Services.UseWebSocketReceiverBroker();

builder.Services.UseWebSocketSubscriberBroker();

builder.Services.UseSocketReceiverBroker();

var app = builder.Build();

// Enable WebSockets
app.UseWebSockets();


// Map the endpoint
app.MapReceiverSocketBroker("/messages/publisher/{topic}");

app.MapSubscriberSocketBroker("/messages/subscriber/{topic}");

app.MapSocketBrokerManagement("/topics/{topic}");

app.Run();