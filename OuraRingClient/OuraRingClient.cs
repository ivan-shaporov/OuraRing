using System.Net;
using System.Net.Http.Json;

namespace OuraRing
{

    public class OuraRingClient
    {
        private readonly HttpClient client = new();

        private readonly Uri OuraSubscriptionApiBaseUrl = new("https://api.ouraring.com/v2/webhook/subscription");

        /// <summary>
        /// Constructor for querying data.
        /// Currently only supports heart rate.
        /// </summary>
        /// <param name="personalToken"></param>
        public OuraRingClient(string personalToken) =>
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {personalToken}");

        /// <summary>
        /// Constructor for managing web hooks subscriptions.
        /// </summary>
        public OuraRingClient(string client_id, string client_secret)
        {
            client.DefaultRequestHeaders.Add("x-client-Id", client_id);
            client.DefaultRequestHeaders.Add("x-client-Secret", client_secret);
        }

        public async Task<HeartRate?> GetHeartRateAsync(DateTimeOffset start)
        {
            var url = new Uri($"https://api.ouraring.com/v2/usercollection/heartrate?start_datetime={start:yyyy-MM-ddTHH:mm:ss}");

            try
            {
                var root = await client.GetFromJsonAsync<ApiResultRoot>(url).ConfigureAwait(false);

                var result = root?.Data.OrderBy(r => r.Timestamp).LastOrDefault();

                return result;
            }
            catch (HttpRequestException)
            {
                throw new OuraClientException();
            }
        }

        public async Task<Callback?> Subscribe(Callback cb)
        {
            cb.VerificationToken = $"{cb.DataType}_{cb.EventType}";

            var response = await client.PostAsJsonAsync(OuraSubscriptionApiBaseUrl, cb);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    return null;
                }
                else
                {
                    throw new OuraClientException();
                }
            }

            var result = await response.Content.ReadFromJsonAsync<Callback>();
            return result;
        }

        public async Task<Callback?> Subscribe(Uri webHookUrl, DataType dataType, EventType eventType)
        {
            var cb = new Callback
            {
                CallbackUrl = webHookUrl,
                VerificationToken = Guid.NewGuid().ToString(),
                DataType = dataType,
                EventType = eventType
            };

            return await Subscribe(cb);
        }

        public async Task<Callback[]> ListSubscribttions()
        {
            try
            {
                var result = await client.GetFromJsonAsync<Callback[]>(OuraSubscriptionApiBaseUrl) ?? [];

                return result;
            }
            catch (HttpRequestException)
            {
                throw new OuraClientException();
            }
        }

        public async Task Unsubscribe(string id)
        {
            try
            {
                await client.DeleteAsync(new Uri(OuraSubscriptionApiBaseUrl, $"subscription/{id}"));
            }
            catch (HttpRequestException)
            {
                throw new OuraClientException();
            }
        }
    }
}
