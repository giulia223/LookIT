//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using System.Text.RegularExpressions;
//using System.Net;

//namespace LookIT.Services
//{
//    public class MesajeService : IMesajeAnalizaService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly string _apiKey;

//        public MesajeService(IConfiguration configuration)
//        {
//            _httpClient = new HttpClient();
//            _apiKey = configuration["OpenAI:ApiKey"]; // Asigură-te că cheia e în appsettings.json
//            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");

//            if (!string.IsNullOrEmpty(_apiKey))
//            {
//                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
//            }
//        }

//        // pentru a scoate tag-urile HTML din editorul de text
//        private string StripHtml(string input)
//        {
//            if (string.IsNullOrEmpty(input)) return string.Empty;
//            var noTags = Regex.Replace(input, "<.*?>", " ");
//            return WebUtility.HtmlDecode(noTags).Trim();
//        }


//        public async Task<SentimentResult> AnalyzeSentimentAsync(string text)
//        {
//            try
//            {
//                string cleanText = StripHtml(text);

//                // Verificare API Key
//                if (string.IsNullOrEmpty(_apiKey))
//                {
//                    System.Diagnostics.Debug.WriteLine("Eroare: API Key lipseste!");
//                    return new SentimentResult { Label = "neutral", Success = false };
//                }

//                var systemPrompt = @"You are a sentiment analysis assistant.
//        Classify the text into exactly one of these labels: 'positive', 'neutral', 'negative'.
//        Respond ONLY with a JSON object: {""label"": ""positive|neutral|negative"", ""confidence"": 0.0-1.0}";

//                var requestBody = new
//                {
//                    model = "gpt-4o-mini",
//                    messages = new[]
//                    {
//                new { role = "system", content = systemPrompt },
//                new { role = "user", content = cleanText }
//            },
//                    temperature = 0.0,
//                    max_tokens = 50
//                };

//                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
//                var response = await _httpClient.PostAsync("chat/completions", content);

//                // --- AICI PRINDEM EROAREA DE LA OPENAI ---
//                if (!response.IsSuccessStatusCode)
//                {
//                    return new SentimentResult { Label = "neutral", Success = false }; 
//                }

//                var jsonString = await response.Content.ReadAsStringAsync();
               
                
//                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//                var aiResponse = JsonSerializer.Deserialize<OpenAiResponse>(jsonString, options);
//                var messageContent = aiResponse?.Choices?.FirstOrDefault()?.Message?.Content;

//                if (!string.IsNullOrEmpty(messageContent))
//                {
//                    messageContent = messageContent.Replace("```json", "").Replace("```", "").Trim();
//                }

//                var sentimentData = JsonSerializer.Deserialize<SentimentData>(messageContent ?? "{}", options);

//                return new SentimentResult
//                {
//                    Label = sentimentData?.Label?.ToLower() ?? "neutral",
                  
//                    Success = true
//                };
//            }
//            catch (Exception ex)
//            {
//                // Orice eroare apare, returnam false ca sa nu blocam aplicatia
//                return new SentimentResult { Label = "neutral", Success = false };
//            }
//        }

//        // Clase interne pentru maparea JSON-ului de la OpenAI
//        private class OpenAiResponse { public List<Choice> Choices { get; set; } }
//        private class Choice { public Message Message { get; set; } }
//        private class Message { public string Content { get; set; } }
//        private class SentimentData { public string Label { get; set; } public double Confidence { get; set; } }
//    }
//}