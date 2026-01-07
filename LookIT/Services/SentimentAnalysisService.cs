using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LookIT.Services
{
    // --- MODELELE DE DATE ---
    public class SentimentResult
    {
        public string Label { get; set; } = "neutral";
        public double Confidence { get; set; } = 0.0;
        public bool Success { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }

    // --- INTERFATA (RENAMED TO AVOID COLLISIONS) ---
    // This local interface name avoids the duplicate definition error (CS0101).
    // If you intended to implement the project's shared ISentimentAnalysisService, replace this local
    // interface with that shared one (make sure the return type matches).
    public interface ISentimentAnalysisServiceLocal
    {
        Task<SentimentResult> AnalyzeSentimentAsync(string text);
    }

    // --- IMPLEMENTAREA ---
    public class SentimentAnalysisServiceLocalImpl : ISentimentAnalysisServiceLocal
    {
        // Renamed private fields to avoid collisions with other symbols in the project
        private readonly HttpClient _client;
        private readonly string _openAiApiKey;
        private readonly ILogger<SentimentAnalysisServiceLocalImpl> _loggerInstance;

        public SentimentAnalysisServiceLocalImpl(IConfiguration configuration, ILogger<SentimentAnalysisServiceLocalImpl> logger)
        {
            _client = new HttpClient();
            _loggerInstance = logger ?? throw new ArgumentNullException(nameof(logger));
            _openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey lipseste!");

            _client.BaseAddress = new Uri("https://api.openai.com/v1/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);
        }

        private string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return WebUtility.HtmlDecode(Regex.Replace(input, "<.*?>", " ")).Trim();
        }

        public async Task<SentimentResult> AnalyzeSentimentAsync(string text)
        {
            try
            {
                string cleanText = StripHtml(text);
                if (string.IsNullOrWhiteSpace(cleanText)) return new SentimentResult { Success = false };

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "system", content = "Classify text as: 'positive', 'neutral', or 'negative'. JSON format: {\"label\": \"...\", \"confidence\": 0.9}" },
                        new { role = "user", content = cleanText }
                    },
                    temperature = 0.1,
                    max_tokens = 50
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    _loggerInstance.LogError($"Eroare OpenAI: {response.StatusCode}");
                    return new SentimentResult { Success = false, ErrorMessage = response.StatusCode.ToString() };
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                // FOLOSIM CLASELE TALE REDENUMITE (Sentiment...)
                var openAiResponse = JsonSerializer.Deserialize<SentimentOpenAiResponse>(responseContent);
                var messageContent = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content;

//                if (!string.IsNullOrEmpty(messageContent))
//                {
//                    messageContent = messageContent.Replace("```json", "").Replace("```", "").Trim();
//                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Use the renamed DTO to avoid collisions with other SentimentResponse types in the project
                var sentimentData = JsonSerializer.Deserialize<ParsedSentimentResponse>(messageContent ?? "{}", options);

                return new SentimentResult
                {
                    Label = sentimentData?.Label?.ToLower() ?? "neutral",
                    Confidence = sentimentData?.Confidence ?? 0.0,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _loggerInstance.LogError(ex, "Eroare critica in serviciul de sentiment.");
                return new SentimentResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }

    // --- CLASE INTERNE REDENUMITE (SPECIFICE TIE) ---
    // Le-am pus prefixul "Sentiment" ca sa nu se bata cu cele ale colegei

    public class SentimentOpenAiResponse
    {
        [JsonPropertyName("choices")]
        public List<SentimentChoice>? Choices { get; set; }
    }

    public class SentimentChoice
    {
        [JsonPropertyName("message")]
        public SentimentMessage? Message { get; set; }
    }

    public class SentimentMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    // Renamed DTO to ParsedSentimentResponse to avoid ambiguity with other SentimentResponse definitions
    public class ParsedSentimentResponse
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}