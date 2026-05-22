using DataAggregator.Application.Abstractions;
using DataAggregator.Application.Configurations;
using DataAggregator.Application.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DataAggregator.Infrastructure.Clients
{
    public class EnformionClient : IEnformionClient
    {
        private readonly HttpClient _httpClient;
        private readonly EnformionConfiguration _config;
        private readonly ILogger<EnformionClient> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public EnformionClient(HttpClient httpClient, IOptions<EnformionConfiguration> config, ILogger<EnformionClient> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
        }

        public async Task<EnformionSummaryResult> GetSummaryAsync()
        {
            var response = await _httpClient.GetAsync(_config.SummaryEndpoint);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EnformionSummaryResult>(content, JsonOptions)!;
        }

        public async IAsyncEnumerable<EnformionRecordResult> GetRecordsAsync(
            string contentType,
            int limit,
            string? cursor = null)
        {
            string? currentCursor = cursor;
            int page = 0;

            do
            {
                page++;
                _logger.LogDebug(
                    "Fetching records page {Page} for content_type {ContentType}, cursor: {Cursor}",
                    page, contentType, currentCursor ?? "(none)");

                var url = BuildRecordsUrl(contentType, limit, currentCursor);
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<EnformionRecordsResult>(content, JsonOptions)!;

                _logger.LogDebug(
                    "Page {Page} returned {Count} records, HasMore: {HasMore}, NextCursor: {NextCursor}",
                    page, result.Data.Count, result.Pagination.HasMore, result.Pagination.NextCursor ?? "(none)");

                foreach (var record in result.Data)
                {
                    yield return record;
                }

                if (!result.Pagination.HasMore)
                    yield break;

                var nextCursor = result.Pagination.NextCursor;

                if (string.IsNullOrEmpty(nextCursor))
                {
                    _logger.LogWarning(
                        "API returned HasMore=true but no NextCursor for content_type {ContentType}. Stopping pagination.",
                        contentType);
                    yield break;
                }

                if (nextCursor == currentCursor)
                {
                   _logger.LogWarning(
                       "Pagination cursor did not advance for content_type {ContentType} (cursor: {Cursor}). Stopping pagination.",
                       contentType, currentCursor);
                   yield break;
                }

                currentCursor = nextCursor;

            } while (true);
        }

        private string BuildRecordsUrl(string contentType, int limit, string? cursor)
        {
            var url = $"{_config.RecordsEndpoint}?content_type={Uri.EscapeDataString(contentType)}&limit={limit}";
            if (!string.IsNullOrEmpty(cursor))
                url += $"&cursor={Uri.EscapeDataString(cursor)}";
            return url;
        }
    }
}

