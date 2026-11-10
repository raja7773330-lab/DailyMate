
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RoutinelyApi.Services;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GenerateResponseAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is missing.");
        }

        var requestBody = new
        {
            model = "gemini-3.5-flash-lite",
            input = prompt
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://generativelanguage.googleapis.com/v1beta/interactions");

        request.Headers.Add("x-goog-api-key", apiKey);

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Gemini API error ({(int)response.StatusCode}): {responseContent}");
        }

        using var document = JsonDocument.Parse(responseContent);

        var root = document.RootElement;

        if (!root.TryGetProperty("steps", out var steps))
        {
            throw new InvalidOperationException(
                "Gemini returned no response steps.");
        }

        var result = new StringBuilder();

        foreach (var step in steps.EnumerateArray())
        {
            if (step.TryGetProperty("type", out var type) &&
                type.GetString() == "model_output" &&
                step.TryGetProperty("content", out var content))
            {
                foreach (var item in content.EnumerateArray())
                {
                    if (item.TryGetProperty("text", out var text))
                    {
                        result.Append(text.GetString());
                    }
                }
            }
        }

        if (result.Length == 0)
        {
            throw new InvalidOperationException(
                "Gemini returned an empty response.");
        }

        return result.ToString();
    }
}
