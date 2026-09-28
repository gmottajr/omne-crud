using System.Text.Json;
using FastEndpoints;
using OmneCrudDemo.Server.Events;

namespace OmneCrudDemo.Server.Endpoints.Products;

public sealed class ProductEventsEndpoint(ProductEventStream eventStream) : EndpointWithoutRequest
{
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(15);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public override void Configure()
    {
        Get("/events/products");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        HttpContext.Response.ContentType = "text/event-stream";
        HttpContext.Response.Headers.CacheControl = "no-cache, no-transform";
        HttpContext.Response.Headers.Connection = "keep-alive";
        HttpContext.Response.Headers["X-Accel-Buffering"] = "no";

        using var subscription = eventStream.Subscribe();

        await HttpContext.Response.WriteAsync("retry: 5000\n\n", ct);
        await HttpContext.Response.Body.FlushAsync(ct);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var eventAvailable = subscription.Reader.WaitToReadAsync(ct).AsTask();
                var heartbeatDue = Task.Delay(HeartbeatInterval, ct);
                var completed = await Task.WhenAny(eventAvailable, heartbeatDue);

                if (completed == eventAvailable && await eventAvailable)
                {
                    while (subscription.Reader.TryRead(out var notification))
                    {
                        var json = JsonSerializer.Serialize(notification, JsonOptions);
                        await HttpContext.Response.WriteAsync(
                            $"id: {notification.Id}\nevent: product\ndata: {json}\n\n",
                            ct);
                    }
                }
                else
                {
                    await HttpContext.Response.WriteAsync(": keep-alive\n\n", ct);
                }

                await HttpContext.Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // A disconnected SSE client is a normal end to the request.
        }
    }
}
