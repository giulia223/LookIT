using LookIT.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LookIT.Services
{
    // clasa pentru rezultatul moderarii
    public class ModerationResult
    {
        public bool IsFlagged { get; set; }
        public string? Reason { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    //interfata serviciului penteu dependency injection
    public interface IModerationService
    {
        //metoda pentru comentarii
        Task<ModerationResult> CheckContentAsync(string text);

        //metoda pentru postari
        Task<ModerationResult> CheckPostAsync(string content);
    }

    //implementarea serviciului folosind OpenAi API
    public class ModerationService : IModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<ModerationService> _logger;

        public ModerationService(IConfiguration configuration, ILogger<ModerationService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey not configured");
            _logger = logger;

            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // metoda noua care verifica continutul unei potsari
        public async Task<ModerationResult> CheckPostAsync( string content)
        {

            string combinedText = $"[CONTENT START] {content} [CONTENT END]";

            //refolosim logica de baza de la CheckContentAsync
            return await CheckContentAsync(combinedText);
        }

        public async Task<ModerationResult> CheckContentAsync(string text)
        {
            try
            {
                // construim prompt-ul pentru analiza continutului
                var systemPrompt = @"You are a strict, zero-tolerance content moderation assistant. 
                                     Your goal is to maintain a completely safe, family-friendly environment.
        
                                     Analyze the input text (in any language, especially Romanian and English) and flag it if it contains ANY of the following:
                                     1. Profanity: Strong or mild swearing, masked words (e.g., 'f*ck', 'b@d'), slang, or vulgarity.
                                     2. Hate Speech & Discrimination: Racism, sexism, homophobia, religious intolerance, or mockery of disabilities.
                                     3. Toxicity & Insults: Bullying, harassment, aggressive behavior, personal attacks, or calling someone names (e.g., 'stupid', 'idiot').
                                     4. Sexual Content: Explicit descriptions, sexual innuendos, solicitation, or creeping.
                                     5. Violence: Threats, encouragement of self-harm, or graphic descriptions of violence.

                                     INSTRUCTIONS:
                                     - Be extremely strict. If you are unsure, FLAG IT.
                                     - Detect toxic intent even without specific keywords.
                                    - Respond ONLY with a JSON object.

                                    Format:
                                    {""isFlagged"" : true/false, ""category"": ""Reason (e.g. Profanity, Harassment)""}";

                var userPrompt = $"Analyze the content of this text: \"{text}\"";

                //construim request ul pentru OpenAi API
                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.1,
                    max_tokens = 50
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                _logger.LogInformation("Sending moderation request to OpenAI API");

                //trimitem request ul catre OpenAi API
                var response = await _httpClient.PostAsync("chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new ModerationResult
                    {
                        Success = false,
                        ErrorMessage = $"API Error: {response.StatusCode}"
                    };
                }

                // parsam raspunsul fFolosim clasele interne definite jos)
                var openAiResponse = JsonSerializer.Deserialize<ModerationOpenAiResponse>(responseContent);
                var assistantMessage = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content;

                if (string.IsNullOrEmpty(assistantMessage))
                {
                    return new ModerationResult 
                    { 
                        Success = false, 
                        ErrorMessage = "Empty response from API" 
                    };
                }

                _logger.LogInformation("OpenAI response: {Response}", assistantMessage);

                var moderationData = JsonSerializer.Deserialize<ModerationResponse>(assistantMessage);

                if (moderationData is null)
                {
                    return new ModerationResult 
                    { 
                        Success = false, 
                        ErrorMessage = "Failed to parse API response" 
                    };
                }

                return new ModerationResult
                {
                    Success = true,
                    IsFlagged = moderationData.IsFlagged,
                    Reason = moderationData.Category
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing content.");
                return new ModerationResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }

    public class ModerationResponse
    {
        [JsonPropertyName("isFlagged")]
        public bool IsFlagged { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }
    }

    public class ModerationOpenAiResponse
    {
        [JsonPropertyName("choices")]
        public List<ModerationChoice>? Choices { get; set; }
    }

    public class ModerationChoice
    {
        [JsonPropertyName("message")]
        public ModerationMessage? Message { get; set; }
    }

    public class ModerationMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}