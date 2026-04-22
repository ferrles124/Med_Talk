using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MedTalk
{
    internal class LlmGemini : Llm
    {
        public LlmGemini(string apiKey, string modelName, string url)
        {
            _apiKey = apiKey;
            _modelName = string.IsNullOrEmpty(modelName) ? "gemini-1.5-flash" : modelName;
            _url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent";
        }

        public override async Task<string> GenerateDialogue(string prompt)
        {
            Log.Info($"Gemini: Using model {_modelName}");
            Log.Info($"Gemini: API Key length {_apiKey?.Length ?? 0}");
            
            var inputString = JsonConvert.SerializeObject(new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { maxOutputTokens = 150 }
            });

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
                var requestUrl = $"{_url}?key={_apiKey}";
                Log.Info($"Gemini: Request URL {requestUrl}");
                
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Content = new StringContent(inputString, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();
                
                Log.Info($"Gemini: Status code {(int)response.StatusCode} {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"Gemini API Error: {response.StatusCode} - {responseString}");
                    return "...";
                }
                
                var responseJson = JObject.Parse(responseString);
                var text = responseJson["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                Log.Info($"Gemini: Response text length {text?.Length ?? 0}");
                return string.IsNullOrEmpty(text) ? "..." : text;
            }
            catch (Exception ex)
            {
                Log.Error($"Gemini error: {ex.Message}");
                return "...";
            }
        }
    }
}
