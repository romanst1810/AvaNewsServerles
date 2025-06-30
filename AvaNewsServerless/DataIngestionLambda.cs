using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Net.Http.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AvaNewsServerless;

public class Function
{
    private static readonly HttpClient client = new();
    private readonly AmazonDynamoDBClient _dbClient = new();

    public async Task FunctionHandler(ILambdaContext context)
    {
        string apiKey = await GetPolygonApiKey();
        var newsResponse = await client.GetFromJsonAsync<List<NewsItem>>($"https://api.polygon.io/v2/reference/news?apiKey={apiKey}");

        var contextDb = new DynamoDBContext(_dbClient);

        foreach (var item in newsResponse)
        {
            await contextDb.SaveAsync(item);
        }
    }

    private async Task<string> GetPolygonApiKey()
    {
        var client = new AmazonSecretsManagerClient();
        var secretValue = await client.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = "PolygonApiKey"
        });
        return secretValue.SecretString;
    }

    public class NewsItem
    {
        [DynamoDBHashKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Instrument { get; set; } = "";
        public DateTime PublishedUtc { get; set; }
        public string Url { get; set; } = "";
    }
}


