using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WMS.Web.Services;

public class WmsApiClient : IWmsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public WmsApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private void SetAuthorizationHeader(string? token)
    {
        var jwt = token ?? _httpContextAccessor.HttpContext?.User?.FindFirst("JWToken")?.Value;
        if (!string.IsNullOrEmpty(jwt))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint, string? token = null)
    {
        SetAuthorizationHeader(token);
        var response = await _httpClient.GetAsync(endpoint);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"API GET '{endpoint}' failed ({response.StatusCode}): {json}");
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize API response from '{endpoint}': {ex.Message}. Content: {json}", ex);
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data, string? token = null)
    {
        SetAuthorizationHeader(token);
        var content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(ExtractErrorMessage(json, response));
        }
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data, string? token = null)
    {
        SetAuthorizationHeader(token);
        var content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(endpoint, content);

        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(ExtractErrorMessage(json, response));
        }
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<T?> PatchAsync<T>(string endpoint, object? data, string? token = null)
    {
        SetAuthorizationHeader(token);
        HttpContent? content = data != null 
            ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
            : null;
            
        var response = await _httpClient.PatchAsync(endpoint, content);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(ExtractErrorMessage(json, response));
        }
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<bool> DeleteAsync(string endpoint, string? token = null)
    {
        SetAuthorizationHeader(token);
        var response = await _httpClient.DeleteAsync(endpoint);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(ExtractErrorMessage(json, response));
        }
        return true;
    }

    private string ExtractErrorMessage(string json, HttpResponseMessage response)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return $"Yêu cầu thất bại với mã lỗi HTTP {(int)response.StatusCode}.";
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
            {
                var msg = msgProp.GetString();
                if (!string.IsNullOrWhiteSpace(msg)) return msg;
            }

            if (root.TryGetProperty("Message", out var msgProp2) && msgProp2.ValueKind == JsonValueKind.String)
            {
                var msg = msgProp2.GetString();
                if (!string.IsNullOrWhiteSpace(msg)) return msg;
            }

            if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
            {
                var firstProp = errorsProp.EnumerateObject().FirstOrDefault();
                if (firstProp.Value.ValueKind == JsonValueKind.Array)
                {
                    var firstMsg = firstProp.Value.EnumerateArray().FirstOrDefault().GetString();
                    if (!string.IsNullOrWhiteSpace(firstMsg)) return firstMsg;
                }
            }
        }
        catch
        {
        }

        return json.Length < 200 ? json : $"Thao tác thất bại (Mã lỗi {(int)response.StatusCode}).";
    }
}
