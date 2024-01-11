namespace HealthForms.Api.Core.Models.SessionMember;

public class AddSessionMemberBulkResponse

{
    public string Id { get; set; }
    public int? CountAdded { get; set; }
    public int? CountSkipped { get; set; }
    public int? CountTotal { get; set; }
    public bool HasErrors { get; set; }
    public decimal PercentComplete { get; set; }
    public string StatusMessage { get; set; }
    public List<AddSessionMemberDetailBulkResponse> Results { get; set; }
}