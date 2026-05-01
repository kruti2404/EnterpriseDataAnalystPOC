using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using EnterpriseDataAnalyst.Application.DTOs;
using EnterpriseDataAnalyst.Application.Interfaces;

namespace EnterpriseDataAnalyst.Infrastructure.Services;

public class WebSearchAgent : IWebSearchAgent
{
    private readonly HttpClient _httpClient;
    private readonly string? _googleApiKey;
    private readonly string? _googleCx;

    public WebSearchAgent(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "EnterpriseDataAnalyst/1.0");
        _googleApiKey = configuration["WebSearch:GoogleApiKey"];
        _googleCx = configuration["WebSearch:GoogleCx"];
    }

    public async Task<List<WebSearchResult>> SearchAsync(string query)
    {
        // Use Google Custom Search if configured, otherwise fall back to DuckDuckGo
        if (!string.IsNullOrWhiteSpace(_googleApiKey) && !string.IsNullOrWhiteSpace(_googleCx))
        {
            return await SearchGoogleAsync(query);
        }

        return await SearchDuckDuckGoAsync(query);
    }

    private async Task<List<WebSearchResult>> SearchDuckDuckGoAsync(string query)
    {
        try
        {
            var encoded = Uri.EscapeDataString(query);
            var url = $"https://api.duckduckgo.com/?q={encoded}&format=json&no_html=1&skip_disambig=1";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<WebSearchResult>();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var results = new List<WebSearchResult>();

            // Abstract (main summary)
            if (root.TryGetProperty("Abstract", out var abs) && abs.GetString() is { Length: > 0 } abstractText)
            {
                results.Add(new WebSearchResult
                {
                    Title = root.TryGetProperty("AbstractSource", out var src) ? src.GetString() ?? "DuckDuckGo" : "DuckDuckGo",
                    Snippet = abstractText,
                    Url = root.TryGetProperty("AbstractURL", out var absUrl) ? absUrl.GetString() ?? "" : ""
                });
            }

            // Answer (instant answer)
            if (root.TryGetProperty("Answer", out var answer) && answer.GetString() is { Length: > 0 } answerText)
            {
                results.Add(new WebSearchResult
                {
                    Title = "Instant Answer",
                    Snippet = answerText,
                    Url = ""
                });
            }

            // Related topics (up to 5)
            if (root.TryGetProperty("RelatedTopics", out var topics) && topics.ValueKind == JsonValueKind.Array)
            {
                var count = 0;
                foreach (var topic in topics.EnumerateArray())
                {
                    if (count >= 5) break;
                    if (topic.TryGetProperty("Text", out var text) && text.GetString() is { Length: > 0 } topicText)
                    {
                        results.Add(new WebSearchResult
                        {
                            Title = topicText.Length > 80 ? topicText[..80] + "…" : topicText,
                            Snippet = topicText,
                            Url = topic.TryGetProperty("FirstURL", out var u) ? u.GetString() ?? "" : ""
                        });
                        count++;
                    }
                }
            }

            return results;
        }
        catch
        {
            return new List<WebSearchResult>();
        }
    }

    private async Task<List<WebSearchResult>> SearchGoogleAsync(string query)
    {
        try
        {
            var encoded = Uri.EscapeDataString(query);
            var url = $"https://www.googleapis.com/customsearch/v1?key={_googleApiKey}&cx={_googleCx}&q={encoded}&num=5";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<WebSearchResult>();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var results = new List<WebSearchResult>();
            if (doc.RootElement.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    results.Add(new WebSearchResult
                    {
                        Title = item.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                        Snippet = item.TryGetProperty("snippet", out var s) ? s.GetString() ?? "" : "",
                        Url = item.TryGetProperty("link", out var l) ? l.GetString() ?? "" : ""
                    });
                }
            }

            return results;
        }
        catch
        {
            return new List<WebSearchResult>();
        }
    }
}
