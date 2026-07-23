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
        if (!response.IsSuccessStatusCode) return default;

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data, string? token = null)
    {
        SetAuthorizationHeader(token);
        var content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
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
            throw new Exception(json);
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
            throw new Exception(json);
        }
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<bool> DeleteAsync(string endpoint, string? token = null)
    {
        SetAuthorizationHeader(token);
        var response = await _httpClient.DeleteAsync(endpoint);
        return response.IsSuccessStatusCode;
    }
}
