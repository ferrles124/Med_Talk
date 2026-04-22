using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MedTalk
{
    internal class LlmGroq : Llm
    {
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static readonly TimeSpan _minDelay = TimeSpan.FromSeconds(2);

        public LlmGroq(string apiKey, string modelName, string url)
        {
            _apiKey = apiKey;
            _modelName = string.IsNullOrEmpty(modelName) ? "llama-3.3-70b-versatile" : modelName;
            _url = "https://api.groq.com/openai/v1/chat/completions";
        }

        public override async Task<string> GenerateDialogue(string prompt)
        {
            var timeSinceLastRequest = DateTime.Now - _lastRequestTime;
            if (timeSinceLastRequest < _minDelay)
            {
                var waitTime = _minDelay - timeSinceLastRequest;
                if (waitTime.TotalMilliseconds > 100)
                {
                    await Task.Delay(waitTime);
                }
            }
            _lastRequestTime = DateTime.Now;

            Log.Info($"Groq: Using model {_modelName}");

            var inputString = JsonConvert.SerializeObject(new
            {
                model = _modelName,
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 150,
                temperature = 0.7
            });

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
                var request = new HttpRequestMessage(HttpMethod.Post, _url);
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                request.Content = new StringContent(inputString, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 429)
                    {
                        Log.Error($"Groq: Rate limit aşıldı!");
                        return "...";
                    }
                    Log.Error($"Groq API Error: {response.StatusCode}");
                    return "...";
                }

                var responseJson = JObject.Parse(responseString);
                var text = responseJson["choices"]?[0]?["message"]?["content"]?.ToString();
                return string.IsNullOrEmpty(text) ? "..." : text;
            }
            catch (Exception ex)
            {
                Log.Error($"Groq error: {ex.Message}");
                return "...";
            }
        }
    }
}
