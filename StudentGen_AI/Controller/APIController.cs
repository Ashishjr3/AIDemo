using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class ApiController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _openAiApiKey;
    private readonly string _openAiApiUrl;
    private readonly string _giminiApiKey;

    public ApiController(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _openAiApiKey = configuration["OpenAI:ChatGPTApiKey"];
        _openAiApiUrl = configuration["OpenAI:ChatGPTApiUrl"];
        _giminiApiKey = configuration["GiminiAI:GiminiApiKey"];
       
    }

    [HttpGet("ChatGPT")]
    public async Task<IActionResult> GetChatResponse([FromQuery] string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return BadRequest("Prompt cannot be empty.");
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_openAiApiUrl, content);

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

        var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        var chatResponse = responseJson
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return Ok(chatResponse);
    }

    [HttpGet("Gemini")]
    /// <summary>
    /// Gets a response from the Gemini API based on the provided prompt.
    /// </summary>
    /// <param name="prompt">The prompt to send to the Gemini API.</param>
    /// <returns>A response from the Gemini API.</returns>
    public async Task<IActionResult> GetGeminiResponse([FromQuery] string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return BadRequest("Prompt cannot be empty.");

        string geminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_giminiApiKey}";
       Console.WriteLine($"Gemini API URL: {_giminiApiKey}");
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(geminiApiUrl, content);

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

        var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        var geminiResponse = responseJson
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return Ok(geminiResponse);
    }
}