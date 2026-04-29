using System.Text;
using System.Text.Json;

namespace ResourceSpider.Server.Services;

public interface IWebhookService
{
    Task<bool> PushAsync(string url, string secret, object payload);
    Task<List<WebhookDelivery>> GetRecentDeliveriesAsync(string taskId);
}

public class WebhookService : IWebhookService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookService> _logger;
    private readonly List<WebhookDelivery> _recentDeliveries = new();
    private readonly object _deliveryLock = new();
    private const int MaxRecentDeliveries = 100;

    public WebhookService(
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> PushAsync(string url, string secret, object payload)
    {
        var delivery = new WebhookDelivery
        {
            DeliveryId = Guid.NewGuid().ToString("N"),
            Url = url,
            Payload = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var client = _httpClientFactory.CreateClient("Webhook");
            client.Timeout = TimeSpan.FromSeconds(30);

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(secret))
            {
                var signature = ComputeHmacSha256(secret, json);
                content.Headers.Add("X-Webhook-Signature", signature);
            }

            content.Headers.Add("X-Webhook-Delivery-Id", delivery.DeliveryId);
            content.Headers.Add("X-Webhook-Timestamp", delivery.CreatedAt.ToString("O"));

            var response = await client.PostAsync(url, content);
            delivery.StatusCode = (int)response.StatusCode;
            delivery.Response = await response.Content.ReadAsStringAsync();
            delivery.Success = response.IsSuccessStatusCode;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Webhook 推送失败: {Url}, 状态码: {StatusCode}", url, response.StatusCode);
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            delivery.Success = false;
            delivery.Error = ex.Message;
            _logger.LogError(ex, "Webhook 推送异常: {Url}", url);
            return false;
        }
        finally
        {
            lock (_deliveryLock)
            {
                _recentDeliveries.Insert(0, delivery);
                if (_recentDeliveries.Count > MaxRecentDeliveries)
                {
                    _recentDeliveries.RemoveAt(_recentDeliveries.Count - 1);
                }
            }
        }
    }

    public Task<List<WebhookDelivery>> GetRecentDeliveriesAsync(string taskId)
    {
        lock (_deliveryLock)
        {
            var deliveries = _recentDeliveries
                .Where(d => d.Payload.Contains(taskId))
                .Take(20)
                .ToList();
            return Task.FromResult(deliveries);
        }
    }

    private static string ComputeHmacSha256(string key, string data)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return $"sha256={Convert.ToHexStringLower(hash)}";
    }
}

public class WebhookDelivery
{
    public string DeliveryId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public string? Response { get; set; }
    public string? Error { get; set; }
    public bool Success { get; set; }
    public DateTime CreatedAt { get; set; }
}
