using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LearningApp.API.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    [HttpGet("daily-oracle")]
    public async Task<IActionResult> GetDailyOracle()
    {
        Console.WriteLine("[GetDailyOracle] Endpoint entered.");

        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("[GetDailyOracle] GEMINI_API_KEY env variable is null or empty. Using fairytale fallback response.");
            return Ok(new { oracleText = "Karanlık yollar önünde uzansa da, içindeki kütüphanenin ışığı her engeli aşmana yetecek. Bugün yeni bir sayfa aç ve kendi destanının kahramanı ol! ✨" });
        }

        try
        {
            using var client = new HttpClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
            
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = "Bana bugünün masalsı kehanetini söyle." }
                        }
                    }
                },
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new { text = "Sen masalsı bir dünyadaki kahinsin. Uygulamayı açan öğrenciye bugün daha çok çalışması, dersleri aksatmaması ve bir kahraman olması için 1-2 cümlelik, gizemli, teşvik edici ve sihirli bir günün motivasyon sözünü söyle." }
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            Console.WriteLine("[GetDailyOracle] Sending request to Gemini API...");
            var response = await client.PostAsync(url, new StringContent(jsonContent, Encoding.UTF8, "application/json"));
            
            Console.WriteLine($"[GetDailyOracle] Gemini API responded with status code: {response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                Console.WriteLine("[GetDailyOracle] Oracle text successfully generated.");
                return Ok(new { oracleText = text?.Trim() ?? string.Empty });
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[GetDailyOracle] Gemini API error payload: {errorResponse}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetDailyOracle] Exception caught inside endpoint: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine("[GetDailyOracle] Returning fairytale fallback response due to API call failure or exception.");
        return Ok(new { oracleText = "Karanlık yollar önünde uzansa da, içindeki kütüphanenin ışığı her engeli aşmana yetecek. Bugün yeni bir sayfa aç ve kendi destanının kahramanı ol! ✨" });
    }
}
