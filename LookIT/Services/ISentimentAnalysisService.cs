//namespace LookIT.Services
//{
//    public class SentimentResult
//    {
//        public string Label { get; set; } = "neutral";
//        public double Confidence { get; set; } = 0.0;
//        public bool Success { get; set; } = false;
//        public string ErrorMessage { get; set; } // <--- NOU
//    }

//    public class MesajeService : IMesajeAnalizaService
//    {
//        // ... (constructorul ramane la fel) ...

//        public async Task<SentimentResult> AnalyzeSentimentAsync(string text)
//        {
//            try
//            {
//                // Daca nu e cheie
//                if (string.IsNullOrEmpty(_apiKey))
//                    return new SentimentResult { Success = false, ErrorMessage = "LIPSA API KEY" };

//                // ... (partea de request body ramane la fel) ...

//                // AICI E SCHIMBAREA IMPORTANTA
//                var response = await _httpClient.PostAsync("chat/completions", content);

//                if (!response.IsSuccessStatusCode)
//                {
//                    // Citim eroarea de la OpenAI
//                    var errorDetails = await response.Content.ReadAsStringAsync();
//                    return new SentimentResult { Success = false, ErrorMessage = $"API ERROR: {response.StatusCode} - {errorDetails}" };
//                }

//                // ... (restul codului de parsare ramane la fel) ...

//                return new SentimentResult
//                {
//                    Label = sentimentData?.Label?.ToLower() ?? "neutral",
//                    Confidence = sentimentData?.Confidence ?? 0.0,
//                    Success = true
//                };
//            }
//            catch (Exception ex)
//            {
//                return new SentimentResult { Success = false, ErrorMessage = $"EXCEPTION: {ex.Message}" };
//            }
//        }
//    }
//}