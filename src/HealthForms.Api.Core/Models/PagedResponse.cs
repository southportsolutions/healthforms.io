
namespace HealthForms.Api.Core.Models;

public class PagedResponse<T> where T : class
{
    public T Data { get; set; } = default!;
    public string? NextUri {get; set; }
    
    public int ResponseCount { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
}