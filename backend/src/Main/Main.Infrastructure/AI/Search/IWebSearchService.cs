namespace Main.Infrastructure.AI.Search;

internal interface IWebSearchService
{
    Task<WebSearchResponse> SearchAsync(string query, string topic, CancellationToken cancellationToken = default);
}