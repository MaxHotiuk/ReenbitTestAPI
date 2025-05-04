using Azure;
using Azure.AI.TextAnalytics;
using ReenbitTest.Core.Interfaces;

namespace ReenbitTest.Infrastructure.Services
{
    public class SentimentAnalysisService : ISentimentAnalysisService
    {
        private readonly TextAnalyticsClient _textAnalyticsClient;

        public SentimentAnalysisService(string azureCognitiveServicesEndpoint, string azureCognitiveServicesKey)
        {
            var credentials = new AzureKeyCredential(azureCognitiveServicesKey);
            _textAnalyticsClient = new TextAnalyticsClient(new Uri(azureCognitiveServicesEndpoint), credentials);
        }

        public async Task<(string Score, string Label)> AnalyzeSentimentAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
                return ("0", "neutral");

            try
            {
                DocumentSentiment documentSentiment = await _textAnalyticsClient.AnalyzeSentimentAsync(text);
                
                string sentimentLabel = documentSentiment.Sentiment.ToString().ToLower();
                string sentimentScore = GetScoreBasedOnSentiment(documentSentiment);
                
                return (sentimentScore, sentimentLabel);
            }
            catch (Exception)
            {
                // Log exception
                return ("0", "neutral");
            }
        }

        private string GetScoreBasedOnSentiment(DocumentSentiment sentiment)
        {
            return sentiment.Sentiment switch
            {
                TextSentiment.Positive => sentiment.ConfidenceScores.Positive.ToString("0.00"),
                TextSentiment.Negative => (-sentiment.ConfidenceScores.Negative).ToString("0.00"),
                _ => "0.00",
            };
        }
    }
}