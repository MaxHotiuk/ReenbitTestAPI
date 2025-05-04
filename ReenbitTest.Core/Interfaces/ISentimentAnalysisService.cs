namespace ReenbitTest.Core.Interfaces
{
    public interface ISentimentAnalysisService
    {
        Task<(string Score, string Label)> AnalyzeSentimentAsync(string text);
    }
}