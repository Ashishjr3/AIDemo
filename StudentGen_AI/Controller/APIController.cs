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

    public ApiController(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _openAiApiKey = configuration["OpenAI:ChatGPTApiKey"];
        _openAiApiUrl = configuration["OpenAI:ChatGPTApiUrl"];
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");
    }

    [HttpGet("ChatGPT")]
    public async Task<IActionResult> GetChatResponse([FromQuery] string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return BadRequest("Prompt cannot be empty.");
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

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
}