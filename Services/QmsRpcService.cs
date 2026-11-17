
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;

namespace QMS.Companion.Api.Services
{
    public class QmsRpcService
    {
        private readonly HttpClient _httpClient;

        public QmsRpcService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetChainIdAsync()
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "eth_chainId",
                @params = Array.Empty<object>(),
                id = 1
            };

            var json = JsonSerializer.Serialize(request);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("", content);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(result);

            return document.RootElement
                .GetProperty("result")
                .GetString()!;
        }



        public async Task<JsonElement> GetLatestBlockAsync()
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "eth_getBlockByNumber",
                @params = new object[] { "latest", false },
                id = 2
            };

            var json = JsonSerializer.Serialize(request);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(result);

            if (document.RootElement.TryGetProperty("error", out var error))
            {
                throw new InvalidOperationException(
                    error.GetProperty("message").GetString()
                );
            }

            return document.RootElement
                .GetProperty("result")
                .Clone();
        }


        public async Task<string> GetBalanceAsync(string address)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "eth_getBalance",
                @params = new object[] { address, "latest" },
                id = 3
            };

            var json = JsonSerializer.Serialize(request);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(result);

            if (document.RootElement.TryGetProperty("error", out var error))
            {
                throw new InvalidOperationException(
                    error.GetProperty("message").GetString()
                );
            }

            return document.RootElement
                .GetProperty("result")
                .GetString()!;
        }


        public async Task<JsonElement> GetTransactionAsync(string hash)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "eth_getTransactionByHash",
                @params = new object[] { hash },
                id = 4
            };

            var json = JsonSerializer.Serialize(request);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(result);

            if (document.RootElement.TryGetProperty("error", out var error))
            {
                throw new InvalidOperationException(
                    error.GetProperty("message").GetString()
                );
            }

            var transaction = document.RootElement.GetProperty("result");

            return transaction.Clone();
        }

        public async Task<JsonElement> GetTransactionReceiptAsync(string hash)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "eth_getTransactionReceipt",
                @params = new object[] { hash },
                id = 1
            };

            var response = await _httpClient.PostAsJsonAsync("", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (json.TryGetProperty("error", out var error))
            {
                throw new HttpRequestException(
                    error.GetProperty("message").GetString()
                );
            }

            return json.GetProperty("result").Clone();
        }



    }
}
