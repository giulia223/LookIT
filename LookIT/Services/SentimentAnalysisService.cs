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
    // --- MODELELE (Copiate si adaptate din Curs) ---

    // Rezultatul final pe care il folosim in Controller
    public class SentimentResult
    {
        public string Label { get; set; } = "neutral"; // positive, neutral, negative
        public double Confidence { get; set; } = 0.0;    // 0.0 - 1.0
        public bool Success { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }

    // Clase pentru deserializarea raspunsului OpenAI (interne)
    public class OpenAiResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class SentimentResponse
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }

    // --- INTERFATA ---
    public interface ISentimentAnalysisService
    {
        Task<SentimentResult> AnalyzeSentimentAsync(string text);
    }

    // --- IMPLEMENTAREA SERVICIULUI ---
    public class SentimentAnalysisService : ISentimentAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<SentimentAnalysisService> _logger;

        public SentimentAnalysisService(IConfiguration configuration, ILogger<SentimentAnalysisService> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;

            // Verificam cheia exact ca in curs
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey not configured");

            // Configurare HttpClient
            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Functie ajutatoare pentru curatarea HTML-ului (pastrata de la noi, vitala pentru editorul tau)
        private string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var noTags = Regex.Replace(input, "<.*?>", " ");
            return WebUtility.HtmlDecode(noTags).Trim();
        }

        public async Task<SentimentResult> AnalyzeSentimentAsync(string text)
        {
            try
            {// 1. DEBUG: Scriem in Output ca a inceput
                System.Diagnostics.Debug.WriteLine($"[AI START] Analizam textul: {text}");
                // Curatam textul inainte de a-l trimite
                string cleanText = StripHtml(text);

                if (string.IsNullOrWhiteSpace(cleanText))
                {
                    return new SentimentResult { Success = false, ErrorMessage = "Empty text" };
                }

                // Construim prompt-ul exact ca in curs
                var systemPrompt = @"You are a sentiment analysis assistant. Analyze the sentiment of the given text and respond ONLY with a JSON object in this exact format:
                {""label"": ""positive|neutral|negative"", ""confidence"": 0.0-1.0}
                Rules:
                - label must be exactly one of: positive, neutral, negative
                - confidence must be a number between 0.0 and 1.0
                - Do not include any other text, only the JSON object";

                var userPrompt = $"Analyze the sentiment of this comment: \"{cleanText}\"";

                // Request Body
                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.1, // Temperatura mica pentru rezultate consistente
                    max_tokens = 50
                };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 2. Trimitem cererea
                var response = await _httpClient.PostAsync("chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // 3. DEBUG: Scriem ce a raspuns OpenAI in consola
                System.Diagnostics.Debug.WriteLine($"[AI RESPONSE] Cod: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[AI BODY] {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    // --- TRUC DE DEBUG ---
                    // Returnam Success = TRUE dar punem EROAREA in Label ca sa o vezi pe site!
                    return new SentimentResult
                    {
                        Success = true,
                        Label = $"ERR: {response.StatusCode}", // Aici va scrie eroarea pe site (ex: ERR: Unauthorized)
                        Confidence = 0.0
                    };
                }

                var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseContent);
                var assistantMessage = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content;

                // Curatare JSON
                if (!string.IsNullOrEmpty(assistantMessage))
                {
                    assistantMessage = assistantMessage.Replace("```json", "").Replace("```", "").Trim();
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var sentimentData = JsonSerializer.Deserialize<SentimentResponse>(assistantMessage ?? "{}", options);

                return new SentimentResult
                {
                    Label = sentimentData?.Label?.ToLower() ?? "neutral",
                    Confidence = sentimentData?.Confidence ?? 0.0,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AI EXCEPTION] {ex.Message}");
                // Afisam exceptia pe site
                return new SentimentResult
                {
                    Success = true,
                    Label = "EXCEPTIE",
                    Confidence = 0.0,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}