using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
builder.Services.AddSingleton<AmazonDynamoDBClient>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();

var app = builder.Build();

app.MapGet("/news", async ([FromServices] IDynamoDBContext db) =>
    await db.ScanAsync<NewsItem>(new List<ScanCondition>()).GetRemainingAsync());

app.MapGet("/news/recent", async ([FromQuery] int days, [FromServices] IDynamoDBContext db) =>
{
var fromDate = DateTime.UtcNow.AddDays(-days);
var conditions = new[] { new ScanCondition("PublishedUtc", ScanOperator.GreaterThanOrEqual, fromDate) };
return await db.ScanAsync<NewsItem>(conditions).GetRemainingAsync();
});

app.MapGet("/news/{instrument}", async ([FromRoute] string instrument, [FromQuery] int limit, [FromServices] IDynamoDBContext db) =>
{
var conditions = new[] { new ScanCondition("Instrument", ScanOperator.Equal, instrument) };
var results = await db.ScanAsync<NewsItem>(conditions).GetRemainingAsync();
return results.Take(limit);
});

app.MapGet("/news/search", async ([FromQuery] string text, [FromServices] IDynamoDBContext db) =>
{
var conditions = new[] { new ScanCondition("Description", ScanOperator.Contains, text) };
return await db.ScanAsync<NewsItem>(conditions).GetRemainingAsync();
});

app.MapPost("/subscribe", async ([FromBody] Subscription subscription, [FromServices] IDynamoDBContext db) =>
{
subscription.Id = Guid.NewGuid().ToString();
await db.SaveAsync(subscription);
return Results.Ok();
});

app.MapGet("/latest-news", async ([FromServices] IDynamoDBContext db) =>
{
var news = await db.ScanAsync<NewsItem>(new List<ScanCondition>()).GetRemainingAsync();
return news
    .OrderByDescending(x => x.PublishedUtc)
    .GroupBy(x => x.Instrument)
    .Select(x => x.First())
    .Take(5);
});

app.Run();

public class NewsItem
{
    [DynamoDBHashKey]
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Instrument { get; set; } = "";
    public DateTime PublishedUtc { get; set; }
    public string Url { get; set; } = "";
}

public class Subscription
{
    [DynamoDBHashKey]
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime SubscribedOn { get; set; } = DateTime.UtcNow;
}
