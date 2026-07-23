namespace WMS.Web.Services;

public interface IWmsApiClient
{
    Task<T?> GetAsync<T>(string endpoint, string? token = null);
    Task<T?> PostAsync<T>(string endpoint, object data, string? token = null);
    Task<T?> PutAsync<T>(string endpoint, object data, string? token = null);
    Task<T?> PatchAsync<T>(string endpoint, object? data, string? token = null);
    Task<bool> DeleteAsync(string endpoint, string? token = null);
}
