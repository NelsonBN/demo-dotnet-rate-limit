using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddRateLimiter(rateLimiter =>
{
    rateLimiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;


    rateLimiter.AddFixedWindowLimiter("fixed-window", options =>
    {
        options.Window = TimeSpan.FromSeconds(10);
        options.PermitLimit = 3;
        // options.QueueLimit = 10;
        // options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });


    rateLimiter.AddSlidingWindowLimiter("sliding-window", options =>
    {
        options.Window = TimeSpan.FromSeconds(15);
        options.SegmentsPerWindow = 3; // 5s + 5s + 5s
        options.PermitLimit = 3;
    });


    rateLimiter.AddTokenBucketLimiter("token-bucket", options =>
    {
        options.TokenLimit = 5;
        options.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        options.TokensPerPeriod = 1;
    });


    rateLimiter.AddConcurrencyLimiter("concurrency", options => {
        options.PermitLimit = 5;
    });


    rateLimiter.AddPolicy("limited-by-ip", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(10),
                PermitLimit = 5
            }));


    rateLimiter.AddPolicy("limited-by-vu", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers["X-VU-ID"].FirstOrDefault() ?? "",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(10),
                PermitLimit = 3
            }));

    // rateLimiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    //     RateLimitPartition.GetFixedWindowLimiter(
    //         partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "",
    //         factory: _ => new FixedWindowRateLimiterOptions
    //         {
    //             Window = TimeSpan.FromSeconds(10),
    //             PermitLimit = 3
    //         }));

    // rateLimiter.OnRejected = async (context, cancellationToken) =>
    // {
    //     var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    //     logger.LogWarning("Rate limit exceeded");
    //     context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
    //     await context.HttpContext.Response.WriteAsJsonAsync(new { Message = "Rate limit exceeded" });
    // };
});


var app = builder.Build();

app.UseRateLimiter();


app.MapGet("/fixed-window", (ILogger<Program> logger) =>
{
    var id = Guid.NewGuid();
    var now = DateTime.UtcNow;

    logger.LogInformation("[FIXED WINDOW][{now}][{Id}]", now.ToString("HH:mm:ss.ffffff"), id);

    return Results.Ok(new { Id = id, Now = now, Type = "fixed-window" });
}).RequireRateLimiting("fixed-window");

app.MapGet("/sliding-window", (ILogger<Program> logger) =>
{
    var id = Guid.NewGuid();
    var now = DateTime.UtcNow;

    logger.LogInformation("[SLIDING WINDOW][{now}][{Id}]", now.ToString("HH:mm:ss.ffffff"), id);

    return Results.Ok(new { Id = id, Now = now, Type = "sliding-window" });
}).RequireRateLimiting("sliding-window");

app.MapGet("/token-bucket", (ILogger<Program> logger) =>
{
    var id = Guid.NewGuid();
    var now = DateTime.UtcNow;

    logger.LogInformation("[TOKEN BUCKET][{now}][{Id}]", now.ToString("HH:mm:ss.ffffff"), id);

    return Results.Ok(new { Id = id, Now = now, Type = "token-bucket" });
}).RequireRateLimiting("token-bucket");

app.MapGet("/concurrency", async (ILogger<Program> logger) =>
{
    var id = Guid.NewGuid();
    var now = DateTime.UtcNow;
    logger.LogInformation("[CONCURRENCY][{now}][{Id}] starting...", now.ToString("HH:mm:ss.ffffff"), id);

    await Task.Delay(500);

    logger.LogInformation("[CONCURRENCY][{now}][{Id}] ended", now.ToString("HH:mm:ss.ffffff"), id);

    return Results.Ok(new { Id = id, Now = now, Type = "concurrency" });
}).RequireRateLimiting("concurrency");

app.MapGet("/limited-by-ip", (ILogger<Program> logger) =>
{
    var id = Guid.NewGuid();
    var now = DateTime.UtcNow;

    logger.LogInformation("[LIMITED BY IP][{now}][{Id}]", now.ToString("HH:mm:ss.ffffff"), id);

    return Results.Ok(new { Id = id, Now = now, Type = "limited-by-ip" });
}).RequireRateLimiting("limited-by-ip");

app.MapGet("/limited-by-vu", (ILogger<Program> logger) =>
{
    var id = Guid.NewGuid();
    var now = DateTime.UtcNow;

    logger.LogInformation("[LIMITED BY VU][{now}][{Id}]", now.ToString("HH:mm:ss.ffffff"), id);

    return Results.Ok(new { Id = id, Now = now, Type = "limited-by-vu" });
}).RequireRateLimiting("limited-by-vu");

app.MapGet("/demo", (ILogger<Program> logger) =>
{
    var id = Guid.NewGuid();
    var now = DateTime.UtcNow;

    logger.LogInformation("[DEMO][{now}][{Id}]", now.ToString("HH:mm:ss.ffffff"), id);

    return Results.Ok(new { Id = id, Now = now, Type = "demo" });
});

app.Run();
