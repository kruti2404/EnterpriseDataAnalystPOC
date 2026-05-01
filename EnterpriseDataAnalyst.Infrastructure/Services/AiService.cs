using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EnterpriseDataAnalyst.Application.Interfaces;

namespace EnterpriseDataAnalyst.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<AiService> _logger;

    public AiService(HttpClient httpClient, IConfiguration configuration, ILogger<AiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var baseUrl = configuration["Llm:BaseUrl"] ?? "https://ollama.com";
        if (!baseUrl.EndsWith("/")) baseUrl += "/";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
        _apiKey = configuration["Llm:ApiKey"] ?? string.Empty;
        _model = configuration["Llm:Model"] ?? "gpt-oss:120b";

        if (!string.IsNullOrEmpty(_apiKey))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<T> GenerateJsonAsync<T>(string prompt)
    {
        var requestBody = new
        {
            model = _model,
            system = "You must output valid JSON only. No markdown, no code fences, no explanations — raw JSON only.",
            prompt = prompt,
            format = "json",
            stream = false
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/generate", content);

        var rawResponse = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("LLM API error {Status}: {Body}", (int)response.StatusCode, rawResponse[..Math.Min(500, rawResponse.Length)]);
            throw new InvalidOperationException($"LLM API returned {(int)response.StatusCode}: {rawResponse[..Math.Min(300, rawResponse.Length)]}");
        }

        // Extract the model's text content from the Ollama envelope
        string contentString;
        try
        {
            using var envelope = JsonDocument.Parse(rawResponse);
            contentString = envelope.RootElement.TryGetProperty("response", out var r)
                ? r.GetString() ?? string.Empty
                : string.Empty;
        }
        catch (JsonException)
        {
            // Raw response is not valid Ollama envelope JSON
            _logger.LogError("Could not parse Ollama envelope. Raw: {Raw}", rawResponse[..Math.Min(500, rawResponse.Length)]);
            throw new InvalidOperationException("Unexpected response format from LLM API.");
        }

        if (string.IsNullOrWhiteSpace(contentString))
        {
            _logger.LogError("LLM returned empty content. Envelope: {Raw}", rawResponse[..Math.Min(500, rawResponse.Length)]);
            throw new InvalidOperationException("LLM returned an empty response.");
        }

        // Sanitize: strip markdown fences and extract the first valid JSON object
        contentString = SanitizeJson(contentString);

        _logger.LogDebug("LLM content (sanitized): {Content}", contentString[..Math.Min(300, contentString.Length)]);

        try
        {
            return JsonSerializer.Deserialize<T>(contentString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize LLM response to {Type}. Content: {Content}",
                typeof(T).Name, contentString[..Math.Min(500, contentString.Length)]);
            throw new InvalidOperationException($"LLM response could not be parsed as {typeof(T).Name}: {ex.Message}");
        }
    }

    public async Task<string> GenerateTextAsync(string prompt)
    {
        var requestBody = new
        {
            model = _model,
            prompt = prompt,
            stream = false
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/generate", content);
        response.EnsureSuccessStatusCode();

        var rawResponse = await response.Content.ReadAsStringAsync();
        using var envelope = JsonDocument.Parse(rawResponse);
        return envelope.RootElement.TryGetProperty("response", out var r)
            ? r.GetString() ?? string.Empty
            : string.Empty;
    }

    // Remove markdown fences and extract the first {...} JSON object from the text
    private static string SanitizeJson(string text)
    {
        text = text.Trim();

        // Strip leading ``` or ```json fence
        if (text.StartsWith("```"))
        {
            var newline = text.IndexOf('\n');
            text = newline >= 0 ? text[(newline + 1)..] : text[3..];
        }

        // Strip trailing ```
        if (text.EndsWith("```"))
            text = text[..^3];

        text = text.Trim();

        // Extract the first complete JSON object (handles any stray text before/after {})
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start >= 0 && end > start)
            text = text[start..(end + 1)];

        return text.Trim();
    }
}
