using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;

namespace LookIT.Services
{
    
    public class SentimentAnalysisService : ISentimentAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public SentimentAnalysisService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["OpenAI:ApiKey"];
            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");

            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            }
        }

        private string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var noTags = Regex.Replace(input, "<.*?>", " ");
            return WebUtility.HtmlDecode(noTags).Trim();
        }

        public async Task<SentimentResult> AnalyzeSentimentAsync(string text)
        {
            try
            {
                string cleanText = StripHtml(text);

                if (string.IsNullOrWhiteSpace(cleanText) || string.IsNullOrEmpty(_apiKey))
                {
                    return new SentimentResult { Label = "neutral", Success = false };
                }

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "system", content = "Classify text as: 'positive', 'neutral', or 'negative'. JSON format: {\"label\": \"...\", \"confidence\": 0.9}" },
                        new { role = "user", content = cleanText }
                    },
                    temperature = 0.0,
                    max_tokens = 50
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    return new SentimentResult { Label = "neutral", Success = false };
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var aiResponse = JsonSerializer.Deserialize<OpenAiResponse>(jsonString, options);
                var messageContent = aiResponse?.Choices?.FirstOrDefault()?.Message?.Content;

                if (!string.IsNullOrEmpty(messageContent))
                {
                    messageContent = messageContent.Replace("```json", "").Replace("```", "").Trim();
                }

                var sentimentData = JsonSerializer.Deserialize<SentimentData>(messageContent ?? "{}", options);

                return new SentimentResult
                {
                    Label = sentimentData?.Label?.ToLower() ?? "neutral",
                    Confidence = sentimentData?.Confidence ?? 0.0,
                    Success = true
                };
            }
            catch
            {
                return new SentimentResult { Label = "neutral", Success = false };
            }
        }

        // Clase interne strict pentru OpenAI (nu le folosim in afara)
        private class OpenAiResponse { public List<Choice> Choices { get; set; } }
        private class Choice { public Message Message { get; set; } }
        private class Message { public string Content { get; set; } }
        private class SentimentData { public string Label { get; set; } public double Confidence { get; set; } }
    }
}