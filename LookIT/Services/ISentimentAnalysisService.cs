using System.Threading.Tasks;

namespace LookIT.Services
{
    // Aici definim Clasa de Rezultat și Interfața
    // Astfel sunt vizibile în tot proiectul

    public class SentimentResult
    {
        public string Label { get; set; } = "neutral";
        public double Confidence { get; set; } = 0.0;
        public bool Success { get; set; } = false;
    }

    public interface ISentimentAnalysisService
    {
        Task<SentimentResult> AnalyzeSentimentAsync(string text);
    }
}