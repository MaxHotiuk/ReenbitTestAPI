namespace ReenbitTest.Core.Interfaces
{
    /// <summary>
    /// Defines operations for sentiment analysis of text using Azure Cognitive Services.
    /// Provides functionality to analyze the sentiment of messages in the chat application.
    /// </summary>
    public interface ISentimentAnalysisService
    {
        /// <summary>
        /// Analyzes the sentiment of the provided text using Azure Cognitive Services Text Analytics.
        /// </summary>
        /// <param name="text">The text to analyze for sentiment.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a tuple with:
        /// - Score: A numerical value representing sentiment strength (positive values for positive sentiment, negative values for negative sentiment)
        /// - Label: A string categorization of the sentiment (e.g., "positive", "negative", "neutral")
        /// </returns>
        /// <remarks>
        /// This method uses Azure Cognitive Services Text Analytics API to determine the emotional tone of the text.
        /// For best results, the text should be at least a complete sentence.
        /// The service handles multiple languages automatically based on language detection.
        /// </remarks>
        Task<(string Score, string Label)> AnalyzeSentimentAsync(string text);
    }
}