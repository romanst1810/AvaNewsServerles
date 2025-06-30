using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Net.Http.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AvaNewsServerless;

public class Function
{
    private static readonly HttpClient client = new();
    private readonly AmazonDynamoDBClient _dbClient = new();

    public async Task FunctionHandler(NewsItem input, ILambdaContext context)
    {
        var enrichedData = await EnrichNewsData(input);

        var contextDb = new DynamoDBContext(_dbClient);
        await contextDb.SaveAsync(enrichedData);
    }

    private async Task<NewsItem> EnrichNewsData(NewsItem item)
    {
        // Example enrichment: Get instrument ticker details
        string tickerApiUrl = $"https://api.polygon.io/v3/reference/tickers/{item.Instrument}?apiKey=YOUR_API_KEY";
        var tickerDetails = await client.GetFromJsonAsync<TickerDetails>(tickerApiUrl);

        item.Description += $"\nInstrument Name: {tickerDetails?.Name}, Market: {tickerDetails?.Market}";

        return item;
    }

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

    public class TickerDetails
    {
        public string Name { get; set; } = "";
        public string Market { get; set; } = "";
    }
}
