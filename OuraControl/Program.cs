using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using OuraRing;

var webHookUrl = new Uri("http://24.56.240.38/api/WebHook");

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var oura_client_id = configuration["oura_client_id"];
var oura_client_secret = configuration["oura_client_secret"];

if (string.IsNullOrEmpty(oura_client_id) || string.IsNullOrEmpty(oura_client_secret))
{
    var userSecretsPath = PathHelper.GetSecretsPathFromSecretsId(oura_client_id);
    Console.WriteLine($"Set oura_client_id and oura_client_secret in {userSecretsPath} file.");
    return;
}

var client = new OuraRingClient(oura_client_id, oura_client_secret);

var subscriptions = await client.ListSubscribttions();

#pragma warning disable CS8321 // Local function is declared but never used
void SubscribeAll()
{
    var excluded = new[]
    {
        new { DataType = DataType.daily_cycle_phases, EventType = EventType.create },
        new { DataType = DataType.daily_cycle_phases, EventType = EventType.update },
        new { DataType = DataType.daily_cycle_phases, EventType = EventType.delete },
    };

    var dataTypes = Enum.GetValues(typeof(DataType)).Cast<DataType>().ToArray();
    var eventTypes = Enum.GetValues(typeof(EventType)).Cast<EventType>().ToArray();
    var allCallbacks = dataTypes
        .SelectMany(DataType => eventTypes.Select(EventType => new { DataType, EventType }))
        .Except(subscriptions.Select(s => new { s.DataType, s.EventType }))
        .Except(excluded)
        .ToList();

    allCallbacks.ForEach(cb => client.Subscribe(webHookUrl, cb.DataType, cb.EventType).Wait());
}

async Task UnsubscribeAll()
{
    foreach (var sub in subscriptions.Where(s => s.Id != null))
    {
        await client.Unsubscribe(sub.Id!);
    }
}
#pragma warning restore CS8321 // Local function is declared but never used

//await client.Unsubscribe(subscriptions.Single(s => s.DataType == DataType.tag && s.EventType == EventType.update).Id!);

await client.Subscribe(webHookUrl, DataType.tag, EventType.update);

foreach (var sub in subscriptions.Where(s => s.Id != null).OrderBy(s => s.DataType.ToString()).ThenBy(s => s.EventType.ToString()))
{
    Console.WriteLine($"{sub.Id}: {sub.CallbackUrl} {sub.DataType}/{sub.EventType} -> {sub.ExpirationTime}");
}

Console.WriteLine("Press any key to continue");
Console.ReadKey();
