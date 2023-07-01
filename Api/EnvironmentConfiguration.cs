using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class EnvironmentConfiguration
{
    private static DateTime? _systemDate;

    public static string RabbitMqConnectionString;
    public static string TestDynamicConfiguration;
    public static DateTime SystemDate
    {
        get => _systemDate ?? DateTime.Now;
        set => _systemDate = value == DateTime.MinValue ? null : value;
    }

    public static async Task DefineInitConfiguration(IConfiguration configuration)
    {
        var dopplerToken = configuration.GetRequiredSection("Doppler:Token").Get<string>();
        var dopplerUrl = configuration.GetRequiredSection("Doppler:Url").Get<string>();

        var client = new HttpClient();
        var basicAuthHeaderValue = Convert.ToBase64String(Encoding.Default.GetBytes(dopplerToken + ":"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthHeaderValue);

        var streamTask = await client.GetStreamAsync(dopplerUrl);
        var secrets = await JsonSerializer.DeserializeAsync<Payload>(streamTask);

        RabbitMqConnectionString = secrets.RabbitMqConnectionString;
        TestDynamicConfiguration = secrets.TestDynamicConfiguration;
        SystemDate = secrets.SystemDate;

        streamTask.Dispose();
        client.Dispose();
    }

    public struct Payload
    {
        [JsonPropertyName("RABBITMQCONNECTIONSTRING")]
        public string RabbitMqConnectionString { get; init; }
        [JsonPropertyName("TESTDYNAMICCONFIGURATION")]
        public string TestDynamicConfiguration { get; init; }
        [JsonPropertyName("SYSTEMDATE")]
        public DateTime SystemDate { get; init; }
    }
}
