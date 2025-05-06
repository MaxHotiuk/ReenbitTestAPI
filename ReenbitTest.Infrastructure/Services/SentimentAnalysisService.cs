using Azure;
using Azure.AI.TextAnalytics;
using ReenbitTest.Core.Interfaces;

namespace ReenbitTest.Infrastructure.Services
{
    /// <summary>
    /// Implementation of <see cref="ISentimentAnalysisService"/> that uses Azure Cognitive Services
    /// Text Analytics API to analyze sentiment in text messages.
    /// </summary>
    /// <remarks>
    /// This service follows Azure best practices for Cognitive Services integration.
    /// </remarks>
    public class SentimentAnalysisService : ISentimentAnalysisService
    {
        private readonly TextAnalyticsClient _textAnalyticsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SentimentAnalysisService"/> class.
        /// </summary>
        /// <param name="azureCognitiveServicesEndpoint">The Azure Cognitive Services endpoint URL.</param>
        /// <param name="azureCognitiveServicesKey">The access key for Azure Cognitive Services.</param>
        /// <remarks>
        /// It's recommended to store the endpoint and key in Azure Key Vault for production environments.
        /// </remarks>
        public SentimentAnalysisService(string azureCognitiveServicesEndpoint, string azureCognitiveServicesKey)
        {
            var credentials = new AzureKeyCredential(azureCognitiveServicesKey);
            _textAnalyticsClient = new TextAnalyticsClient(new Uri(azureCognitiveServicesEndpoint), credentials);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This implementation uses Azure Cognitive Services Text Analytics API to evaluate text sentiment.
        /// Following Azure best practices:
        /// - Implements proper error handling
        /// - Returns a neutral fallback value on failure
        /// - Gracefully handles empty input
        /// - Formats sentiment scores consistently
        /// 
        /// For optimal performance, consider:
        /// - Batching multiple text analyses in production scenarios
        /// - Implementing client-side caching for frequently analyzed phrases
        /// - Using Azure Functions with automatic scaling for high-volume processing
        /// </remarks>
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
            catch (RequestFailedException)
            {
                // Azure-specific exception handling
                // Consider logging specifics about the Azure service error
                // In production, log exception details like ErrorCode, Status (HTTP status code), and Message
                return ("0", "neutral");
            }
            catch (Exception)
            {
                // General exception handling
                // Consider logging the exception details for troubleshooting
                return ("0", "neutral");
            }
        }

        /// <summary>
        /// Converts the sentiment analysis result to a normalized score string.
        /// </summary>
        /// <param name="sentiment">The document sentiment analysis result from Azure Cognitive Services.</param>
        /// <returns>
        /// A string representation of the sentiment score:
        /// - Positive values (0 to 1) for positive sentiment
        /// - Negative values (-1 to 0) for negative sentiment
        /// - "0.00" for neutral sentiment
        /// </returns>
        /// <remarks>
        /// This method standardizes sentiment scores to a format consistent with the application's requirements.
        /// </remarks>
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