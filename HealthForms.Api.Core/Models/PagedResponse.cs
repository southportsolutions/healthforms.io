
namespace HealthForms.Api.Core.Models;

public class PagedResponse<T> where T : class
{
    public T Data { get; set; } = default!;
    public string? Next {get; set; }
    
    public int Total { get; set; }
    public int Size { get; set; }
    public int TotalItems { get; set; }
}