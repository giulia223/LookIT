using Azure;
using Humanizer;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LookIT.Models;

namespace LookIT.Services
{
    public class SentimentMessageResult
    {
        public string Label { get; set; }
        public bool Success { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }

    // Interfata serviciului pentru dependency injection
    public interface IMesajeAnalizaService
    {
        Task<SentimentMessageResult> AnalyzeSentimentAsync(string text);
    }

    // Implementarea serviciului de analiza de sentiment folosind OpenAI API
    public class MesajeAnalizaService : IMesajeAnalizaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<MesajeAnalizaService> _logger;
        public MesajeAnalizaService(IConfiguration configuration, ILogger<MesajeAnalizaService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new
            ArgumentNullException("OpenAI:ApiKey not configured");
            _logger = logger;
            // Configurare HttpClient pentru OpenAI API

            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<SentimentMessageResult> AnalyzeSentimentAsync(string text)
        {
            string cleanText = System.Text.RegularExpressions.Regex.Replace(text ?? "", "<.*?>", string.Empty);
            try
            {
                // Construim prompt-ul pentru analiza de sentiment
                var systemPrompt = @"You are a content moderation assistant. Analyze the given text for 
                harassment, insults, hate speech, or discriminatory language. 
                Respond ONLY with a JSON object in this exact format:
                { ""label"": ""safe"" } or { ""label"": ""unsafe"" }
                Rules:
                - Set label to ""unsafe"" if the text contains harassment, insults, or inappropriate language.
                - Set label to ""safe"" if the text is respectful.
                - Do not include any other text, only the JSON object.";
                var userPrompt = $"Analyze the sentiment of this comment: \"{cleanText}\"";

                // Construim request-ul pentru OpenAI API
                var requestBody = new
                {
                    model = "gpt-4o-mini", // Using gpt-4o-mini as gpt - 5 - nano doesn't exist
                    messages = new[] {
                        new {
                            role = "system", content = systemPrompt
                        },
                        new {
                            role = "user", content = userPrompt
                        }
                    },

                    temperature = 0.1, // Low temperature for consistent results
                    max_tokens = 50
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent,
                Encoding.UTF8, "application/json");
                _logger.LogInformation("Sending sentiment analysis request to OpenAI API");

                // Trimitem request-ul catre OpenAI API
                var response = await _httpClient.PostAsync("chat/completions", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {StatusCode} - { Content}", response.StatusCode, responseContent);
                    return new SentimentMessageResult
                    {
                        Success = false,

                        ErrorMessage = $"API Error:{response.StatusCode}"
                    };
                }

                // Parsam raspunsul de la OpenAI

                var openAiResponse = JsonSerializer.Deserialize<Open_AiResponse>(responseContent);

                var assistantMessage = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content;

                if (string.IsNullOrEmpty(assistantMessage))
                {
                    return new SentimentMessageResult
                    {
                        Success = false,

                        ErrorMessage = "Empty response from API"

                    };
                }

                _logger.LogInformation("OpenAI response: {Response}", assistantMessage);

                // Parsam JSON-ul din raspunsul asistentului
                var sentimentData = JsonSerializer.Deserialize<SentimentResponse>(assistantMessage, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true 
                    }
                );  

                if (sentimentData == null)
                {
                    return new SentimentMessageResult
                    {
                        Success = false,
                        ErrorMessage = "Failed to parse sentiment response"
                    };
                }

                // Validam si normalizam label-ul
                var label = sentimentData.Label?.ToLower() switch
                {
                    "safe" => "safe",
                    "unsafe" => "unsafe",
                    _ => "unsafe"
                };

                return new SentimentMessageResult
                {
                    Label = label,
                    Success = true

                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing sentiment");
                return new SentimentMessageResult
                {
                    Success = false,

                    ErrorMessage = ex.Message

                };
            }
        }
    }

    // //Clase pentru deserializarea raspunsului OpenAI
    public class Open_AiResponse
    {
        [JsonPropertyName("choices")]
        public List<CHOICE>? Choices { get; set; }
    }
    public class CHOICE
    {
        [JsonPropertyName("message")]
        public Mssg? Message { get; set; }
    }
    public class Mssg
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
    public class SentimentResponse
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }
       
    }
}